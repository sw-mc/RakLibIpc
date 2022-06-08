namespace SkyWing.RakLib.Ipc; 

public static class RakLibToUserThreadMessageProtocol{

    /*
     * Internal Packet:
     * byte (packet ID)
     * byte[] (payload)
     */

    /*
     * ENCAPSULATED payload:
     * int32 (internal session ID)
     * byte[] (user packet payload)
     */
    public const byte  PACKET_ENCAPSULATED = 0x01;

    /*
     * OPEN_SESSION payload:
     * int32 (internal session ID)
     * byte (address length)
     * byte[] (address)
     * short (port)
     * long (clientID)
     */
    public const byte PACKET_OPEN_SESSION = 0x02;

    /*
     * CLOSE_SESSION payload:
     * int32 (internal session ID)
     * string (reason)
     */
    public const byte PACKET_CLOSE_SESSION = 0x03;

    /*
     * ACK_NOTIFICATION payload:
     * int32 (internal session ID)
     * int32 (identifierACK)
     */
    public const byte PACKET_ACK_NOTIFICATION = 0x04;

    /*
     * REPORT_BANDWIDTH_STATS payload:
     * int64 (sent bytes diff)
     * int64 (received bytes diff)
     */
    public const byte PACKET_REPORT_BANDWIDTH_STATS = 0x05;

    /*
     * RAW payload:
     * byte (address length)
     * byte[] (address from/to)
     * short (port)
     * byte[] (payload)
     */
    public const byte PACKET_RAW = 0x06;

    /*
     * REPORT_PING payload:
     * int32 (internal session ID)
     * int32 (measured latency in MS)
     */
    public const byte PACKET_REPORT_PING = 0x07;

}

public static class UserToRakLibThreadMessageProtocol{

    /*
     * Internal Packet:
     * byte (packet ID)
     * byte[] (payload)
     */

    /*
     * ENCAPSULATED payload:
     * int32 (internal session ID)
     * byte (flags, last 3 bits, priority)
     * byte (reliability)
     * int32 (ack identifier)
     * byte? (order channel, only when sequenced or ordered reliability)
     * byte[] (user packet payload)
     */
    public const byte PACKET_ENCAPSULATED = 0x01;

    public const byte ENCAPSULATED_FLAG_NEED_ACK = 1 << 0;
    public const byte ENCAPSULATED_FLAG_IMMEDIATE = 1 << 1;

    /*
     * CLOSE_SESSION payload:
     * int32 (internal session ID)
     */
    public const byte PACKET_CLOSE_SESSION = 0x02;

    /*
     * RAW payload:
     * byte (address length)
     * byte[] (address from/to)
     * short (port)
     * byte[] (payload)
     */
    public const byte PACKET_RAW = 0x04;

    /*
     * BLOCK_ADDRESS payload:
     * byte (address length)
     * byte[] (address)
     * int (timeout)
     */
    public const byte PACKET_BLOCK_ADDRESS = 0x05;

    /*
     * UNBLOCK_ADDRESS payload:
     * byte (address length)
     * byte[] (address)
     */
    public const byte PACKET_UNBLOCK_ADDRESS = 0x06;

    /*
     * RAW_FILTER payload:
     * byte[] (pattern)
     */
    public const byte PACKET_RAW_FILTER = 0x07;

    /*
     * SET_NAME payload:
     * byte[] (name)
     */
    public const byte PACKET_SET_NAME = 0x08;

    /* No payload */
    public const byte PACKET_ENABLE_PORT_CHECK = 0x09;

    /* No payload */
    public const byte PACKET_DISABLE_PORT_CHECK = 0x10;

    /*
     * PACKETS_PER_TICK_LIMIT payload:
     * int64 (limit)
     */
    public const byte PACKET_SET_PACKETS_PER_TICK_LIMIT = 0x11;
}

