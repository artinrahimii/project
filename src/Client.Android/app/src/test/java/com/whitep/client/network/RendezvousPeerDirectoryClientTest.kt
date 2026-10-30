package com.whitep.client.network

import org.json.JSONArray
import org.json.JSONObject
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotNull
import org.junit.Assert.assertNull
import org.junit.Assert.assertThrows
import org.junit.Assert.assertTrue
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config
import java.io.ByteArrayInputStream
import java.io.InputStream
import java.net.HttpURLConnection
import java.net.URI
import java.net.URL
import java.time.Clock
import java.time.Instant
import java.time.ZoneOffset
import java.util.concurrent.atomic.AtomicInteger

@RunWith(RobolectricTestRunner::class)
@Config(sdk = [35])
class RendezvousPeerDirectoryClientTest {
    private val now = Instant.parse("2026-10-30T14:00:00Z")
    private val clock = Clock.fixed(now, ZoneOffset.UTC)
    private val peerId = "QmYwAPJzv5CZsnAzt8auVTLz"

    @Test
    fun resolvesOnlyTheMatchingPeersDirectTcpAddressAndIgnoresQuicForThisClient() {
        val tcp = "/ip4/127.0.0.1/tcp/41000/p2p/" + peerId
        val quic = "/ip4/127.0.0.1/udp/41000/quic-v1/p2p/" + peerId
        val body =
            directoryJson(
                peerId = peerId,
                addresses = listOf(tcp, quic),
                expiresAt = now.plusSeconds(300),
            )
        var requestedUrl: URL? = null
        val client =
            client { url ->
                requestedUrl = url
                StubConnection(url, HttpURLConnection.HTTP_OK, body)
            }

        val endpoint = runBlockingResolve(client, peerId)

        assertNotNull(endpoint)
        assertEquals(peerId, endpoint!!.peerId)
        assertEquals(listOf(tcp), endpoint.tcpMultiaddresses)
        assertEquals(
            "/prefix/v1/peers",
            requestedUrl!!.path,
        )
        assertEquals("capability=inference.v1", requestedUrl!!.query)
    }

    @Test
    fun cachesOneDirectoryFetchForSeveralHostLookupsInTheSameRound() {
        val secondPeer = "QmYwAPJzv5CZsnAzt8auVTLa"
        val body = JSONArray()
        body.put(
            announcement(
                peerId,
                listOf("/ip4/127.0.0.1/tcp/41000/p2p/" + peerId),
                now.plusSeconds(300),
            ),
        )
        body.put(
            announcement(
                secondPeer,
                listOf("/ip4/127.0.0.2/tcp/41001/p2p/" + secondPeer),
                now.plusSeconds(300),
            ),
        )
        val directoryBody = body.toString()
        val calls = AtomicInteger()
        val client =
            client { url ->
                calls.incrementAndGet()
                StubConnection(url, HttpURLConnection.HTTP_OK, directoryBody)
            }

        assertNotNull(runBlockingResolve(client, peerId))
        assertNotNull(runBlockingResolve(client, secondPeer))
        assertEquals(1, calls.get())
    }

    @Test
    fun excludesExpiredPeersFromTheReturnedDirectory() {
        val body =
            directoryJson(
                peerId = peerId,
                addresses = listOf("/ip4/127.0.0.1/tcp/41000/p2p/" + peerId),
                expiresAt = now.minusSeconds(1),
            )
        val client =
            client { url ->
                StubConnection(url, HttpURLConnection.HTTP_OK, body)
            }

        assertNull(runBlockingResolve(client, peerId))
    }

    @Test
    fun rejectsAnAddressThatClaimsAnotherPeerIdentity() {
        val otherPeer = "QmYwAPJzv5CZsnAzt8auVTLa"
        val body =
            directoryJson(
                peerId = peerId,
                addresses = listOf("/ip4/127.0.0.1/tcp/41000/p2p/" + otherPeer),
                expiresAt = now.plusSeconds(300),
            )

        assertThrows(
            IllegalArgumentException::class.java,
        ) {
            RendezvousPeerDirectoryParser.parse(body, now)
        }
    }

    @Test
    fun rejectsPublicHttpAndAllowsLoopbackOnlyHttp() {
        assertThrows(
            IllegalArgumentException::class.java,
        ) {
            RendezvousPeerDirectoryClient(
                serviceBaseUri = URI("http://rendezvous.example"),
                clock = clock,
            )
        }
        val client =
            RendezvousPeerDirectoryClient(
                serviceBaseUri = URI("http://127.0.0.1:8080"),
                clock = clock,
            )
        assertTrue(client.toString().isNotBlank())
    }

    @Test
    fun rejectsAnAnnouncementWithAnInvalidIpAddressOrPort() {
        val badAddress = "/ip4/999.1.1.1/tcp/70000/p2p/" + peerId
        val body =
            directoryJson(
                peerId = peerId,
                addresses = listOf(badAddress),
                expiresAt = now.plusSeconds(300),
            )

        assertThrows(
            IllegalArgumentException::class.java,
        ) {
            RendezvousPeerDirectoryParser.parse(body, now)
        }
    }

    private fun client(
        factory: (URL) -> HttpURLConnection,
    ): RendezvousPeerDirectoryClient {
        return RendezvousPeerDirectoryClient(
            serviceBaseUri = URI("https://rendezvous.example/prefix"),
            connectionFactory = factory,
            clock = clock,
        )
    }

    private fun runBlockingResolve(
        client: RendezvousPeerDirectoryClient,
        id: String,
    ): RendezvousPeerEndpoint? {
        return kotlinx.coroutines.runBlocking {
            client.resolve(id)
        }
    }

    private fun directoryJson(
        peerId: String,
        addresses: List<String>,
        expiresAt: Instant,
    ): String {
        val result = JSONArray()
        result.put(announcement(peerId, addresses, expiresAt))
        return result.toString()
    }

    private fun announcement(
        peerId: String,
        addresses: List<String>,
        expiresAt: Instant,
    ): JSONObject {
        val addressArray = JSONArray()
        addresses.forEach { address -> addressArray.put(address) }
        val capabilities = JSONArray()
        capabilities.put("inference.v1")
        val result = JSONObject()
        result.put("peerId", peerId)
        result.put("addresses", addressArray)
        result.put("capabilities", capabilities)
        result.put("expiresAtUtc", expiresAt.toString())
        return result
    }

    private class StubConnection(
        url: URL,
        private val resultCode: Int,
        private val body: String,
    ) : HttpURLConnection(url) {
        override fun connect() {
            connected = true
        }

        override fun disconnect() {
            connected = false
        }

        override fun usingProxy(): Boolean = false

        override fun getResponseCode(): Int = resultCode

        override fun getInputStream(): InputStream = ByteArrayInputStream(body.toByteArray(Charsets.UTF_8))
    }
}
