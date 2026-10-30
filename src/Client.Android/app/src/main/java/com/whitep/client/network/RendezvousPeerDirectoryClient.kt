package com.whitep.client.network

import org.json.JSONArray
import org.json.JSONException
import java.io.ByteArrayOutputStream
import java.io.IOException
import java.io.InputStream
import java.net.HttpURLConnection
import java.net.Inet6Address
import java.net.URI
import java.net.URL
import java.nio.charset.StandardCharsets
import java.time.Clock
import java.time.Duration
import java.time.Instant
import java.time.OffsetDateTime
import java.util.Locale
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock
import kotlinx.coroutines.withContext

/**
 * A metadata-only peer directory entry returned by the Windows rendezvous service.
 *
 * [peerId] is the canonical libp2p identity advertised by the Windows host. Only direct TCP
 * multiaddresses are exposed here because the Android transport integration currently targets
 * the shared TCP + Noise path. QUIC addresses remain valid directory metadata but are not
 * selected by this client.
 */
data class RendezvousPeerEndpoint(
    val peerId: String,
    val tcpMultiaddresses: List<String>,
    val expiresAtUtc: Instant,
)

/**
 * Reads only the rendezvous control-plane directory. Inference prompts, responses and request
 * history are neither sent to nor downloaded from this service.
 *
 * The service must use HTTPS, except for loopback-only tests. A small in-memory cache prevents
 * three host submissions in one request round from issuing three identical directory queries.
 */
class RendezvousPeerDirectoryClient(
    serviceBaseUri: URI,
    private val connectionFactory: (URL) -> HttpURLConnection = { url ->
        url.openConnection() as? HttpURLConnection
            ?: throw IOException("The rendezvous URL did not create an HTTP connection.")
    },
    private val clock: Clock = Clock.systemUTC(),
    private val cacheDuration: Duration = DEFAULT_CACHE_DURATION,
) {
    private val requestUrl: URL
    private val cacheMutex = Mutex()

    @Volatile
    private var cachedPeers: Map<String, RendezvousPeerEndpoint> = emptyMap()

    @Volatile
    private var cacheExpiresAt: Instant = Instant.EPOCH

    init {
        require(cacheDuration > Duration.ZERO && cacheDuration <= MAXIMUM_CACHE_DURATION) {
            "The directory cache duration must be positive and no longer than one minute."
        }
        require(serviceBaseUri.isAbsolute && !serviceBaseUri.isOpaque) {
            "An absolute rendezvous service URI is required."
        }
        require(
            serviceBaseUri.userInfo == null &&
                serviceBaseUri.query == null &&
                serviceBaseUri.fragment == null,
        ) {
            "The rendezvous URI must not contain credentials, query parameters or a fragment."
        }
        val scheme = serviceBaseUri.scheme.lowercase(Locale.ROOT)
        val host =
            serviceBaseUri.host?.lowercase(Locale.ROOT)
                ?: throw IllegalArgumentException("The rendezvous URI must include a host.")
        require(
            scheme == "https" ||
                (scheme == "http" && isLoopbackHost(host))
        ) {
            "Rendezvous requires HTTPS except for loopback-only tests."
        }

        val normalizedPath =
            serviceBaseUri.normalize().path.orEmpty().trimEnd('/')
        requestUrl =
            URI(
                scheme,
                null,
                host,
                serviceBaseUri.port,
                "$normalizedPath/v1/peers",
                "capability=inference.v1",
                null,
            ).toURL()
    }

    /**
     * Resolves one ephemeral host ID to its unexpired direct TCP address set.
     *
     * The Windows host currently uses its canonical libp2p PeerId as EphemeralHostId, so the
     * directory's peerId must match the requested value exactly. No identifier is synthesized.
     */
    suspend fun resolve(ephemeralHostId: String): RendezvousPeerEndpoint? {
        require(PeerAnnouncementDirectoryParser.isValidPeerId(ephemeralHostId)) {
            "A normalized libp2p PeerId is required."
        }

        return cacheMutex.withLock {
            val now = clock.instant()
            if (!now.isBefore(cacheExpiresAt)) {
                val freshPeers =
                    withContext(Dispatchers.IO) {
                        fetchDirectory(now)
                    }
                cachedPeers = freshPeers
                val earliestPeerExpiry = freshPeers.values.minOfOrNull { it.expiresAtUtc }
                val requestedCacheExpiry = now.plus(cacheDuration)
                cacheExpiresAt =
                    if (earliestPeerExpiry == null ||
                        requestedCacheExpiry.isBefore(earliestPeerExpiry)
                    ) {
                        requestedCacheExpiry
                    } else {
                        earliestPeerExpiry
                    }
            }

            cachedPeers[ephemeralHostId]?.takeIf { endpoint ->
                endpoint.tcpMultiaddresses.isNotEmpty() &&
                    endpoint.expiresAtUtc.isAfter(clock.instant())
            }
        }
    }

    private fun fetchDirectory(now: Instant): Map<String, RendezvousPeerEndpoint> {
        val connection = connectionFactory(requestUrl)
        try {
            connection.requestMethod = "GET"
            connection.connectTimeout = REQUEST_TIMEOUT_MILLIS
            connection.readTimeout = REQUEST_TIMEOUT_MILLIS
            connection.instanceFollowRedirects = false
            connection.useCaches = false
            connection.setRequestProperty("Accept", "application/json")
            connection.setRequestProperty("Cache-Control", "no-store")

            val status = connection.responseCode
            if (status != HttpURLConnection.HTTP_OK) {
                throw IOException("Rendezvous directory request failed with HTTP status $status.")
            }

            val body =
                connection.inputStream.use { input ->
                    readBounded(input, MAXIMUM_DIRECTORY_BYTES)
                }
            return RendezvousPeerDirectoryParser.parse(body, now)
        } finally {
            connection.disconnect()
        }
    }

    private fun readBounded(
        input: InputStream,
        maximumBytes: Int,
    ): String {
        val output = ByteArrayOutputStream()
        val buffer = ByteArray(DEFAULT_READ_BUFFER_BYTES)
        while (true) {
            val count = input.read(buffer)
            if (count < 0) {
                break
            }
            if (output.size() > maximumBytes - count) {
                throw IOException("The rendezvous directory response exceeds the size limit.")
            }
            output.write(buffer, 0, count)
        }
        return output.toString(StandardCharsets.UTF_8.name())
    }

    private fun isLoopbackHost(host: String): Boolean =
        host == "localhost" || host == "127.0.0.1" || host == "::1" || host == "[::1]"

    private companion object {
        const val REQUEST_TIMEOUT_MILLIS = 10_000
        const val DEFAULT_READ_BUFFER_BYTES = 1_024
        const val MAXIMUM_DIRECTORY_BYTES = 16 * 1024
        val DEFAULT_CACHE_DURATION: Duration = Duration.ofSeconds(5)
        val MAXIMUM_CACHE_DURATION: Duration = Duration.ofMinutes(1)
    }
}

/**
 * Strict parser for the metadata-only JSON format produced by PeerAnnouncementServiceHost.
 */
internal object RendezvousPeerDirectoryParser {
    private const val MAXIMUM_PEERS = 256
    private const val MAXIMUM_ADDRESSES = 8
    private const val MAXIMUM_CAPABILITIES = 32
    private const val MAXIMUM_ANNOUNCEMENT_LIFETIME_HOURS = 24L
    private const val REQUIRED_CAPABILITY = "inference.v1"

    private val peerIdPattern = Regex("^[1-9A-HJ-NP-Za-km-z]{20,128}$")
    private val tcpAddressPattern =
        Regex(
            "^/(ip4|ip6)/([^/]+)/tcp/([0-9]{1,5})/p2p/([1-9A-HJ-NP-Za-km-z]{20,128})$",
        )
    private val quicAddressPattern =
        Regex(
            "^/(ip4|ip6)/([^/]+)/udp/([0-9]{1,5})/quic-v1/p2p/([1-9A-HJ-NP-Za-km-z]{20,128})$",
        )

    fun isValidPeerId(peerId: String): Boolean = peerIdPattern.matches(peerId)

    fun parse(
        json: String,
        now: Instant,
    ): Map<String, RendezvousPeerEndpoint> {
        require(json.toByteArray(StandardCharsets.UTF_8).size <= 16 * 1024) {
            "The rendezvous directory response exceeds the size limit."
        }

        val announcements =
            try {
                JSONArray(json)
            } catch (exception: JSONException) {
                throw IllegalArgumentException("The rendezvous directory response is not a JSON array.", exception)
            }
        require(announcements.length() <= MAXIMUM_PEERS) {
            "The rendezvous directory contains too many peers."
        }

        val result = LinkedHashMap<String, RendezvousPeerEndpoint>()
        val seenPeerIds = HashSet<String>()
        for (index in 0 until announcements.length()) {
            val announcement = announcements.getJSONObject(index)
            val peerId = announcement.getString("peerId")
            require(isValidPeerId(peerId)) {
                "The rendezvous directory contains an invalid PeerId."
            }
            require(seenPeerIds.add(peerId)) {
                "The rendezvous directory contains a duplicate PeerId."
            }

            val expiresAtUtc =
                try {
                    OffsetDateTime.parse(announcement.getString("expiresAtUtc")).toInstant()
                } catch (exception: Exception) {
                    throw IllegalArgumentException("The peer announcement expiry is invalid.", exception)
                }
            require(
                !expiresAtUtc.isAfter(now.plus(Duration.ofHours(MAXIMUM_ANNOUNCEMENT_LIFETIME_HOURS))),
            ) {
                "The peer announcement expiry exceeds the allowed lifetime."
            }
            if (!expiresAtUtc.isAfter(now)) {
                continue
            }

            val capabilities =
                readUniqueStrings(
                    announcement.getJSONArray("capabilities"),
                    MAXIMUM_CAPABILITIES,
                    "capabilities",
                )
            if (REQUIRED_CAPABILITY !in capabilities) {
                continue
            }

            val announcedAddresses =
                readUniqueStrings(
                    announcement.getJSONArray("addresses"),
                    MAXIMUM_ADDRESSES,
                    "addresses",
                )
            val tcpAddresses = ArrayList<String>()
            for (address in announcedAddresses) {
                val tcpMatch = tcpAddressPattern.matchEntire(address)
                val quicMatch = quicAddressPattern.matchEntire(address)
                val match =
                    tcpMatch ?: quicMatch
                        ?: throw IllegalArgumentException("The peer announcement contains a non-direct address.")
                val groups = match.groupValues
                val family = groups[1]
                val host = groups[2]
                val port = groups[3].toIntOrNull()
                val addressPeerId = groups[4]

                require(addressPeerId == peerId) {
                    "The multiaddress PeerId does not match the announced PeerId."
                }
                require(port != null && port in 1..65_535) {
                    "The peer announcement contains an invalid transport port."
                }
                require(isValidIpLiteral(family, host)) {
                    "The peer announcement contains an invalid IP address."
                }

                if (tcpMatch != null) {
                    tcpAddresses.add(address)
                }
            }

            result[peerId] =
                RendezvousPeerEndpoint(
                    peerId = peerId,
                    tcpMultiaddresses = tcpAddresses.toList(),
                    expiresAtUtc = expiresAtUtc,
                )
        }
        return result
    }

    private fun readUniqueStrings(
        values: JSONArray,
        maximumCount: Int,
        fieldName: String,
    ): List<String> {
        require(
            values.length() in 1..maximumCount,
        ) {
            "The peer announcement has an invalid $fieldName count."
        }
        val result = ArrayList<String>(values.length())
        val seen = HashSet<String>()
        for (index in 0 until values.length()) {
            val value = values.getString(index)
            require(
                value.isNotBlank() && seen.add(value),
            ) {
                "The peer announcement contains an empty or duplicate $fieldName value."
            }
            result.add(value)
        }
        return result
    }

    private fun isValidIpLiteral(
        family: String,
        host: String,
    ): Boolean =
        when (family) {
            "ip4" -> {
                val octets = host.split('.')
                octets.size == 4 &&
                    octets.all { octet ->
                        octet.isNotEmpty() &&
                            octet.length <= 3 &&
                            octet.all(Char::isDigit) &&
                            octet.toIntOrNull()?.let { it in 0..255 } == true
                    }
            }
            "ip6" -> {
                if (!host.contains(':') || !Regex("^[0-9A-Fa-f:]+$").matches(host)) {
                    false
                } else {
                    try {
                        val address = java.net.InetAddress.getByName(host)
                        address is Inet6Address && address.scopeId == 0
                    } catch (_: Exception) {
                        false
                    }
                }
            }
            else -> false
        }
}
