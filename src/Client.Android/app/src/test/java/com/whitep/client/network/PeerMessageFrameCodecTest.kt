package com.whitep.client.network

import org.junit.Assert.assertArrayEquals
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Assert.assertThrows
import org.junit.Test
import java.io.EOFException

class PeerMessageFrameCodecTest {
    @Test
    fun encodesUnsignedVarintLengthAtSingleAndMultiByteBoundaries() {
        val singleByteFrame = PeerMessageFrameCodec.encode(ByteArray(127))
        val multiByteFrame = PeerMessageFrameCodec.encode(ByteArray(128))

        assertEquals(0x7f, singleByteFrame[0].toInt() and 0xff)
        assertArrayEquals(
            byteArrayOf(0x80.toByte(), 0x01),
            multiByteFrame.copyOfRange(0, 2),
        )
    }

    @Test
    fun decodesFramesSplitAcrossLengthPrefixAndPayloadChunks() {
        val message = ByteArray(128) { index -> index.toByte() }
        val frame = PeerMessageFrameCodec.encode(message)
        val decoder = PeerMessageFrameDecoder()

        assertTrue(decoder.feed(frame.copyOfRange(0, 1)).isEmpty())
        assertTrue(decoder.feed(frame.copyOfRange(1, 5)).isEmpty())
        val decoded = decoder.feed(frame.copyOfRange(5, frame.size))

        assertEquals(1, decoded.size)
        assertArrayEquals(message, decoded.single())
        decoder.finish()
    }

    @Test
    fun decodesMultipleFramesAndAnEmptyMessageFromOneChunk() {
        val first = byteArrayOf(1, 2, 3)
        val second = byteArrayOf(9, 8)
        val combined = PeerMessageFrameCodec.encode(first) +
            PeerMessageFrameCodec.encode(ByteArray(0)) +
            PeerMessageFrameCodec.encode(second)
        val decoder = PeerMessageFrameDecoder()

        val decoded = decoder.feed(combined)

        assertEquals(3, decoded.size)
        assertArrayEquals(first, decoded[0])
        assertArrayEquals(ByteArray(0), decoded[1])
        assertArrayEquals(second, decoded[2])
        decoder.finish()
    }

    @Test
    fun rejectsAnOversizedDeclaredLengthBeforeReceivingItsPayload() {
        val oversizedFrame = PeerMessageFrameCodec.encode(ByteArray(129), maximumMessageBytes = 256)
        val decoder = PeerMessageFrameDecoder(maximumMessageBytes = 128)

        assertThrows(IllegalArgumentException::class.java) {
            decoder.feed(oversizedFrame.copyOfRange(0, 2))
        }
    }

    @Test
    fun rejectsNonCanonicalAndOverlongLengthPrefixes() {
        val decoder = PeerMessageFrameDecoder()

        assertThrows(IllegalArgumentException::class.java) {
            decoder.feed(byteArrayOf(0x80.toByte(), 0x00))
        }
        assertThrows(IllegalArgumentException::class.java) {
            decoder.feed(
                byteArrayOf(
                    0x80.toByte(),
                    0x80.toByte(),
                    0x80.toByte(),
                    0x80.toByte(),
                    0x80.toByte(),
                ),
            )
        }
    }

    @Test
    fun reportsTruncatedPayloadWhenThePeerStreamEnds() {
        val frame = PeerMessageFrameCodec.encode(byteArrayOf(4, 5, 6))
        val decoder = PeerMessageFrameDecoder()

        assertTrue(decoder.feed(frame.copyOfRange(0, frame.size - 1)).isEmpty())
        assertThrows(EOFException::class.java) {
            decoder.finish()
        }
    }

    @Test
    fun rejectsInvalidLimitsAndEncoderOversize() {
        assertThrows(IllegalArgumentException::class.java) {
            PeerMessageFrameDecoder(maximumMessageBytes = 0)
        }
        assertThrows(IllegalArgumentException::class.java) {
            PeerMessageFrameCodec.encode(byteArrayOf(1, 2), maximumMessageBytes = 1)
        }
    }
}
