package com.whitep.client.network

import android.content.Context
import androidx.test.core.app.ApplicationProvider
import androidx.test.ext.junit.runners.AndroidJUnit4
import androidx.test.platform.app.InstrumentationRegistry
import com.whitep.client.requests.InferenceTransportSubmission
import io.libp2p.core.PeerId
import kotlinx.coroutines.runBlocking
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNotEquals
import org.junit.Assert.assertTrue
import org.junit.Assume.assumeTrue
import org.junit.Test
import org.junit.runner.RunWith
import org.whitep.shared.contracts.v1.InferenceOutcome
import org.whitep.shared.contracts.v1.InferenceRequest
import java.net.Inet4Address
import java.net.InetAddress
import java.nio.charset.StandardCharsets
import java.time.Duration
import java.time.Instant
import java.util.UUID

/**
 * Exercises the production Android libp2p transport against the production Windows .NET libp2p
 * transport over a real, non-loopback LAN. The Windows fixture returns a deterministic protocol
 * response; this proves authenticated transport and response correlation, not model quality.
 */
@RunWith(AndroidJUnit4::class)
class AndroidWindowsLanIntegrationTest {
    @Test
    fun publishesStableAndroidPeerIdForWindowsHostPairing() {
        val context = ApplicationProvider.getApplicationContext<Context>()
        val identityStore = AndroidPeerIdentityStore(context)
        val peerId = identityStore.loadOrCreatePeerId()

        assertTrue(
            "The Android Keystore identity must produce a PeerId.",
            peerId.isNotBlank(),
        )
        assertEquals(
            "The identity must remain stable across loads.",
            peerId,
            identityStore.loadOrCreatePeerId(),
        )

        context.openFileOutput(PEER_ID_FILE, Context.MODE_PRIVATE).use { output ->
            output.write(peerId.toByteArray(StandardCharsets.US_ASCII))
        }
    }

    @Test
    fun connectsToWindowsHostOverNonLoopbackLan() =
        runBlocking<Unit> {
            val arguments = InstrumentationRegistry.getArguments()
            val hostPeerId = arguments.getString("host_peer_id").orEmpty()
            val hostAddressText = arguments.getString("host_ip").orEmpty()
            val hostPort = arguments.getString("host_port")?.toIntOrNull() ?: 0

            assumeTrue(
                "This test is run only by the explicit LAN-acceptance workflow.",
                hostPeerId.isNotBlank(),
            )
            assumeTrue("The LAN host IPv4 address is required.", hostAddressText.isNotBlank())
            assumeTrue("The LAN host TCP port is invalid.", hostPort in 1..65535)

            val hostAddress = InetAddress.getByName(hostAddressText)
            assertTrue("The Windows peer must be addressed over IPv4.", hostAddress is Inet4Address)
            assertTrue("The Windows peer address must be a private LAN address.", hostAddress.isSiteLocalAddress)
            assertFalse("Loopback is not accepted as LAN evidence.", hostAddress.isLoopbackAddress)

            val canonicalHostPeerId = PeerId.fromBase58(hostPeerId).toBase58()
            assertEquals("The supplied Windows PeerId must be canonical.", hostPeerId, canonicalHostPeerId)

            val context = ApplicationProvider.getApplicationContext<Context>()
            val localIdentity = AndroidPeerIdentityStore(context).loadOrCreate()
            val localPeerId = PeerId.fromPubKey(localIdentity.publicKey()).toBase58()
            assertNotEquals(
                "The Android and Windows peers must have independent identities.",
                hostPeerId,
                localPeerId,
            )

            val trustStore = AndroidPeerTrustStore(context)
            var transport: Libp2pInferenceRequestTransport? = null
            try {
                trustStore.trustPeerAfterExplicitConfirmation(
                    hostPeerId,
                    explicitlyConfirmed = true,
                )
                assertTrue(
                    "The expected Windows PeerId must be explicitly trusted for this test.",
                    trustStore.isTrusted(hostPeerId),
                )

                val endpoint =
                    RendezvousPeerEndpoint(
                        peerId = hostPeerId,
                        tcpMultiaddresses =
                            listOf(
                                "/ip4/${hostAddress.hostAddress}/tcp/$hostPort/p2p/$hostPeerId",
                            ),
                        expiresAtUtc = Instant.now().plusSeconds(120),
                    )
                val activeTransport =
                    Libp2pInferenceRequestTransport(
                        localIdentity = localIdentity,
                        resolveEndpoint = { requestedPeerId -> endpoint.takeIf { it.peerId == requestedPeerId } },
                        trustVerifier = trustStore,
                        connectionTimeout = Duration.ofSeconds(20),
                    )
                transport = activeTransport

                val now = System.currentTimeMillis()
                val request =
                    InferenceRequest
                        .newBuilder()
                        .setProtocolVersion("1.0.0")
                        .setRequestId("lan-${UUID.randomUUID()}")
                        .setAttemptId("lan-attempt-${UUID.randomUUID()}")
                        .setRoundId("lan-round-${UUID.randomUUID()}")
                        .setModelId("lan-integration-model")
                        .setPrompt("White-P authenticated Android-to-Windows LAN acceptance probe.")
                        .setDeadlineUnixMs(now + 60_000L)
                        .setMaxOutputTokens(8)
                        .build()

                val submission = activeTransport.submit(hostPeerId, request)
                assertEquals(
                    "The real Windows peer must accept the authenticated request.",
                    InferenceTransportSubmission.QUEUED,
                    submission,
                )

                val response =
                    activeTransport.awaitResponse(
                        requestId = request.requestId,
                        attemptId = request.attemptId,
                        timeout = Duration.ofSeconds(40),
                    )
                assertEquals(request.requestId, response.requestId)
                assertEquals(request.attemptId, response.attemptId)
                assertEquals(request.roundId, response.roundId)
                assertEquals(request.modelId, response.modelId)
                assertEquals(hostPeerId, response.ephemeralHostId)
                assertEquals("dotnet-host-response", response.outputText)
                assertEquals(InferenceOutcome.INFERENCE_OUTCOME_SUCCEEDED, response.outcome)
                assertTrue(
                    "The Android transport must record one validated correlated response.",
                    activeTransport.diagnosticSnapshot().contains("acceptedResponseFrames=1"),
                )
            } finally {
                transport?.close()
                trustStore.revokeTrust(hostPeerId)
            }
        }

    private companion object {
        const val PEER_ID_FILE = "whitep-lan-peer-id.txt"
    }
}
