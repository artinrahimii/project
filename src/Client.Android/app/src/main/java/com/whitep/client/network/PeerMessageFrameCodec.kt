package com.whitep.client.network

import java.io.EOFException

/**
 * Encodes and decodes the message framing used by the Windows libp2p peer protocol.
 *
 * The wire format is an unsigned, canonical, 32-bit varint payload length followed by
 * exactly that many bytes. This is framing only: it does not open a socket, discover a
 * peer, authenticate a PeerId, or establish a libp2p stream.
 */
internal object PeerMessageFrameCodec {
    const val DEFAULT_MAXIMUM_MESSAGE_BYTES = 4 * 1024 * 1024
    const val MAXIMUM_VARINT_BYTES = 5

    fun encode(
        message: ByteArray,
        maximumMessageBytes: Int = DEFAULT_MAXIMUM_MESSAGE_BYTES,
    ): ByteArray {
        require(maximumMessageBytes > 0) { "maximumMessageBytes must be positive" }
        require(message.size <= maximumMessageBytes) {
            "The peer message exceeds the configured size limit."
        }

        var remaining = message.size
        var prefixLength = 1
        while (remaining >= 0x80) {
            prefixLength += 1
            remaining = remaining ushr 7
        }

        val frame = ByteArray(prefixLength + message.size)
        remaining = message.size
        var offset = 0
        while (remaining >= 0x80) {
            frame[offset] = ((remaining and 0x7f) or 0x80).toByte()
            remaining = remaining ushr 7
            offset += 1
        }
        frame[offset] = remaining.toByte()
        message.copyInto(frame, offset + 1)
        return frame
    }
}

/**
 * Incremental decoder for streams that may split a varint prefix or message payload
 * across multiple reads. It emits only complete frames and bounds each declared frame
 * before waiting for or copying its payload.
 */
internal class PeerMessageFrameDecoder(
    private val maximumMessageBytes: Int = PeerMessageFrameCodec.DEFAULT_MAXIMUM_MESSAGE_BYTES,
) {
    private data class DecodedLength(
        val value: Int,
        val byteCount: Int,
    )

    private var pending = ByteArray(0)

    init {
        require(maximumMessageBytes > 0) { "maximumMessageBytes must be positive" }
    }

    fun feed(chunk: ByteArray): List<ByteArray> {
        if (chunk.isEmpty()) {
            return emptyList()
        }
        require(pending.size <= Int.MAX_VALUE - chunk.size) { "Input chunk is too large." }

        val input = ByteArray(pending.size + chunk.size)
        pending.copyInto(input)
        chunk.copyInto(input, pending.size)

        val messages = ArrayList<ByteArray>()
        var offset = 0
        while (offset < input.size) {
            val frameStart = offset
            val decodedLength = decodeLength(input, offset)
            if (decodedLength == null) {
                pending = input.copyOfRange(frameStart, input.size)
                return messages
            }

            if (decodedLength.value > maximumMessageBytes) {
                reject("The peer message exceeds the configured size limit.")
            }
            offset += decodedLength.byteCount

            val remainingPayloadBytes = input.size - offset
            if (remainingPayloadBytes < decodedLength.value) {
                pending = input.copyOfRange(frameStart, input.size)
                return messages
            }

            messages.add(input.copyOfRange(offset, offset + decodedLength.value))
            offset += decodedLength.value
        }

        pending = ByteArray(0)
        return messages
    }

    /**
     * Call when the underlying stream closes. A non-empty pending buffer means the
     * stream ended in the middle of a length prefix or payload.
     */
    fun finish() {
        if (pending.isNotEmpty()) {
            throw EOFException("The peer stream ended before a complete message frame arrived.")
        }
    }

    private fun decodeLength(
        source: ByteArray,
        start: Int,
    ): DecodedLength? {
        var value = 0L
        for (index in 0 until PeerMessageFrameCodec.MAXIMUM_VARINT_BYTES) {
            val position = start + index
            if (position >= source.size) {
                return null
            }

            val octet = source[position].toInt() and 0xff
            if (index == PeerMessageFrameCodec.MAXIMUM_VARINT_BYTES - 1 && (octet and 0xf0) != 0) {
                reject("The peer message length varint exceeds 32 bits.")
            }

            value = value or ((octet and 0x7f).toLong() shl (index * 7))
            if ((octet and 0x80) == 0) {
                if (index > 0 && (octet and 0x7f) == 0) {
                    reject("The peer message length varint is not canonical.")
                }
                if (value > Int.MAX_VALUE) {
                    reject("The peer message length exceeds the supported range.")
                }
                return DecodedLength(value.toInt(), index + 1)
            }
        }

        reject("The peer message length varint is longer than five bytes.")
    }

    private fun reject(message: String): Nothing {
        pending = ByteArray(0)
        throw IllegalArgumentException(message)
    }
}
