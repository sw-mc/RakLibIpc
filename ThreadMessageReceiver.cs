using System.Text;
using SkyWing.Binary;
using SkyWing.RakLib.Protocol;
using SkyWing.RakLib.Server;

namespace SkyWing.RakLib.Ipc;

public sealed class RakLibToUserThreadMessageReceiver {

    private readonly InterThreadChannelReader channel;

    public RakLibToUserThreadMessageReceiver(InterThreadChannelReader channel) {
        this.channel = channel;
    }

    public bool Handle(ServerEventListener listener) {
        var buffer = channel.Read();
        if (buffer == null) return false;

        var stream = new BinaryStream(buffer);
        var id = stream.ReadByte();
        int sessionId;
        byte length;
        string address;
        short port;
        byte[] payload;
        switch (id) {
            case RakLibToUserThreadMessageProtocol.PACKET_ENCAPSULATED:
                sessionId = stream.ReadInt();
                payload = stream.GetRemainingBytes();
                listener.OnPacketReceive(sessionId, payload);
                break;
            case RakLibToUserThreadMessageProtocol.PACKET_RAW:
                length = stream.ReadByte();
                address = Encoding.UTF8.GetString(stream.ReadBytes(length));
                port = stream.ReadShort();
                payload = stream.GetRemainingBytes();
                listener.OnRawPacketReceive(address, port, payload);
                break;
            case RakLibToUserThreadMessageProtocol.PACKET_REPORT_BANDWIDTH_STATS:
                var sentBytes = stream.ReadLong();
                var receiveBytes = stream.ReadLong();
                listener.OnBandwidthStatsUpdate(sentBytes, receiveBytes);
                break;
            case RakLibToUserThreadMessageProtocol.PACKET_OPEN_SESSION:
                sessionId = stream.ReadInt();
                length = stream.ReadByte();
                address = Encoding.UTF8.GetString(stream.ReadBytes(length));
                port = stream.ReadShort();
                var clientId = stream.ReadLong();
                listener.OnClientConnect(sessionId, address, port, clientId);
                break;
            case RakLibToUserThreadMessageProtocol.PACKET_CLOSE_SESSION:
                sessionId = stream.ReadInt();
                length = stream.ReadByte();
                var reason = Encoding.UTF8.GetString(stream.ReadBytes(length));
                listener.OnClientDisconnect(sessionId, reason);
                break;
            case RakLibToUserThreadMessageProtocol.PACKET_ACK_NOTIFICATION:
                sessionId = stream.ReadInt();
                var identifierAck = stream.ReadInt();
                listener.OnPacketAck(sessionId, identifierAck);
                break;
            case RakLibToUserThreadMessageProtocol.PACKET_REPORT_PING:
                sessionId = stream.ReadInt();
                var pingMs = stream.ReadLong();
                listener.OnPingMeasure(sessionId, pingMs);
                break;
            default:
                return false;
        }
        return true;
    }

}

public sealed class UserToRakLibThreadMessageReceiver : ServerEventSource {
    
    private readonly InterThreadChannelReader channel;

    public UserToRakLibThreadMessageReceiver(InterThreadChannelReader channel) {
        this.channel = channel;
    }

    public bool Process(ServerInterface server) {
        var buffer = channel.Read();
        if (buffer == null) return false;

        var stream = new BinaryStream(buffer);
        var id = stream.ReadByte();
        int sessionId;
        byte length;
        string address;
        switch (id) {
            case UserToRakLibThreadMessageProtocol.PACKET_ENCAPSULATED:
                sessionId = stream.ReadInt();
                var flags = stream.ReadByte();
                var immediate = (flags & UserToRakLibThreadMessageProtocol.ENCAPSULATED_FLAG_IMMEDIATE) != 0;
                var needAck = (flags & UserToRakLibThreadMessageProtocol.ENCAPSULATED_FLAG_NEED_ACK) != 0;

                var encapsulated = new EncapsulatedPacket {
                    Reliability = stream.ReadByte()
                };

                if (needAck) encapsulated.IdentifierAck = stream.ReadInt();

                if (PacketReliability.IsSequencedOrOrdered(encapsulated.Reliability))
                    encapsulated.OrderChannel = stream.ReadByte();

                encapsulated.Buffer = stream.GetRemainingBytes();
                server.SendEncapsulated(sessionId, encapsulated, immediate);
                break;
            case UserToRakLibThreadMessageProtocol.PACKET_RAW:
                length = stream.ReadByte();
                address = Encoding.UTF8.GetString(stream.ReadBytes(length));
                var port = stream.ReadShort();
                var payload = stream.GetRemainingBytes();
                server.SendRaw(address, port, payload);
                break;
            case UserToRakLibThreadMessageProtocol.PACKET_CLOSE_SESSION:
                sessionId = stream.ReadInt();
                server.CloseSession(sessionId);
                break;
            case UserToRakLibThreadMessageProtocol.PACKET_SET_NAME:
                server.SetName(Encoding.UTF8.GetString(stream.GetRemainingBytes()));
                break;
            case UserToRakLibThreadMessageProtocol.PACKET_ENABLE_PORT_CHECK:
                server.SetPortCheck(true);
                break;
            case UserToRakLibThreadMessageProtocol.PACKET_DISABLE_PORT_CHECK:
                server.SetPortCheck(false);
                break;
            case UserToRakLibThreadMessageProtocol.PACKET_SET_PACKETS_PER_TICK_LIMIT:
                server.SetPacketsPerTickLimit(stream.ReadLong());
                break;
            case UserToRakLibThreadMessageProtocol.PACKET_BLOCK_ADDRESS:
                length = stream.ReadByte();
                address = Encoding.UTF8.GetString(stream.ReadBytes(length));
                var timeout = stream.ReadInt();
                server.BlockAddress(address, timeout);
                break;
            case UserToRakLibThreadMessageProtocol.PACKET_UNBLOCK_ADDRESS:
                length = stream.ReadByte();
                address = Encoding.UTF8.GetString(stream.ReadBytes(length));
                server.UnblockAddress(address);
                break;
            case UserToRakLibThreadMessageProtocol.PACKET_RAW_FILTER:
                server.AddRawPacketFilter(Encoding.UTF8.GetString(stream.GetRemainingBytes()));
                break;
            default:
                return false;
        }
        return false;
    }
}