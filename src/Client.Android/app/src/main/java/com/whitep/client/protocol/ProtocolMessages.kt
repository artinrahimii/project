package com.whitep.client.protocol

import org.whitep.shared.contracts.v1.HostAnnouncement
import org.whitep.shared.contracts.v1.hostAnnouncement

/**
 * Kotlin-side access to the Protobuf Kotlin DSL generated from the canonical .proto source.
 *
 * This helper demonstrates actual build-generated contract use; it is not a network transport.
 */
object ProtocolMessages {
    fun newHostAnnouncement(
        ephemeralHostId: String,
        protocolVersion: String,
        announcedAtUnixMs: Long,
    ): HostAnnouncement {
        require(ephemeralHostId.isNotBlank()) { "ephemeralHostId must not be blank" }
        require(protocolVersion.isNotBlank()) { "protocolVersion must not be blank" }
        require(announcedAtUnixMs >= 0) { "announcedAtUnixMs must not be negative" }

        return hostAnnouncement {
            this.ephemeralHostId = ephemeralHostId
            this.protocolVersion = protocolVersion
            this.announcedAtUnixMs = announcedAtUnixMs
        }
    }

    fun serializeHostAnnouncement(message: HostAnnouncement): ByteArray = message.toByteArray()

    fun parseHostAnnouncement(bytes: ByteArray): HostAnnouncement = HostAnnouncement.parseFrom(bytes)
}
