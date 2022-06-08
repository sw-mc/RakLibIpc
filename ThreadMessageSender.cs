using System.Text;
using SkyWing.Binary;
using SkyWing.RakLib.Protocol;
using SkyWing.RakLib.Server;

namespace SkyWing.RakLib.Ipc;

public sealed class RakLibToUserThreadMessageSender : ServerEventListener {

    private readonly InterThreadChannelWriter channel;

    public RakLibToUserThreadMessageSender(InterThreadChannelWriter channel) {
        this.channel = channel;
    }
    
    public void OnClientConnect(int sessionId, string address, int port, long clientId) {
        var stream = new BinaryStream();
        stream.WriteByte(RakLibToUserThreadMessageProtocol.PACKET_OPEN_SESSION);
        stream.WriteInt(sessionId);
        stream.WriteByte((byte) address.Length);
        stream.WriteBytes(Encoding.UTF8.GetBytes(address));
        stream.WriteShort((short) port);
        stream.WriteLong(clientId);
        channel.Write(stream.GetBuffer());
    }

    public void OnClientDisconnect(int sessionId, string reason) {
        var stream = new BinaryStream();
        stream.WriteByte(RakLibToUserThreadMessageProtocol.PACKET_CLOSE_SESSION);
        stream.WriteInt(sessionId);
        stream.WriteByte((byte) reason.Length);
        stream.WriteBytes(Encoding.UTF8.GetBytes(reason));
        channel.Write(stream.GetBuffer());
    }

    public void OnPacketReceive(int sessionId, byte[] packet) {
        var stream = new BinaryStream();
        stream.WriteByte(RakLibToUserThreadMessageProtocol.PACKET_ENCAPSULATED);
        stream.WriteInt(sessionId);
        stream.WriteBytes(packet);
        channel.Write(stream.GetBuffer());
    }

    public void OnRawPacketReceive(string address, int port, byte[] payload) {
        var stream = new BinaryStream();
        stream.WriteByte(RakLibToUserThreadMessageProtocol.PACKET_RAW);
        stream.WriteByte((byte) address.Length);
        stream.WriteBytes(Encoding.UTF8.GetBytes(address));
        stream.WriteShort((short) port);
        stream.WriteBytes(payload);
        channel.Write(stream.GetBuffer());
    }

    public void OnPacketAck(int sessionId, int identifierAck) {
        var stream = new BinaryStream();
        stream.WriteByte(RakLibToUserThreadMessageProtocol.PACKET_ACK_NOTIFICATION);
        stream.WriteInt(sessionId);
        stream.WriteInt(identifierAck);
        channel.Write(stream.GetBuffer());
    }

    public void OnBandwidthStatsUpdate(long bytesSentDiff, long bytesReceivedDiff) {
        var stream = new BinaryStream();
        stream.WriteByte(RakLibToUserThreadMessageProtocol.PACKET_REPORT_BANDWIDTH_STATS);
        stream.WriteLong(bytesSentDiff);
        stream.WriteLong(bytesReceivedDiff);
        channel.Write(stream.GetBuffer());
    }

    public void OnPingMeasure(int sessionId, long pingMs) {
        var stream = new BinaryStream();
        stream.WriteByte(RakLibToUserThreadMessageProtocol.PACKET_RAW);
        stream.WriteInt(sessionId);
        stream.WriteLong(pingMs);
        channel.Write(stream.GetBuffer());
    }
    
}

public sealed class UserToRakLibThreadMessageSender : ServerInterface {

    private readonly InterThreadChannelWriter channel;

    public UserToRakLibThreadMessageSender(InterThreadChannelWriter channel) {
        this.channel = channel;
    }

    public void SendEncapsulated(int sessionId, EncapsulatedPacket packet, bool immediate = false) {
        var flags =
            (immediate ? UserToRakLibThreadMessageProtocol.ENCAPSULATED_FLAG_IMMEDIATE : 0) |
        (packet.IdentifierAck != null ? UserToRakLibThreadMessageProtocol.ENCAPSULATED_FLAG_NEED_ACK : 0);

        var stream = new BinaryStream();
        stream.WriteByte(UserToRakLibThreadMessageProtocol.PACKET_ENCAPSULATED);
        stream.WriteInt(sessionId);
        stream.WriteByte((byte) flags);
        stream.WriteByte((byte) packet.Reliability);
        if (packet.IdentifierAck != null) stream.WriteInt((int) packet.IdentifierAck);
        if(PacketReliability.IsSequencedOrOrdered(packet.Reliability)) stream.WriteByte((byte) packet.OrderChannel);
        stream.WriteBytes(packet.Buffer);
        channel.Write(stream.GetBuffer());
    }

    public void SendRaw(string address, int port, byte[] payload) {
        var stream = new BinaryStream();
        stream.WriteByte(UserToRakLibThreadMessageProtocol.PACKET_RAW);
        stream.WriteByte((byte) address.Length);
        stream.WriteBytes(Encoding.UTF8.GetBytes(address));
        stream.WriteShort((short) port);
        stream.WriteBytes(payload);
        channel.Write(stream.GetBuffer());
    }

    public void CloseSession(int sessionId) {
        var stream = new BinaryStream();
        stream.WriteByte(UserToRakLibThreadMessageProtocol.PACKET_CLOSE_SESSION);
        stream.WriteInt(sessionId);
        channel.Write(stream.GetBuffer());
    }

    public void SetName(string name) {
        var stream = new BinaryStream();
        stream.WriteByte(UserToRakLibThreadMessageProtocol.PACKET_SET_NAME);
        stream.WriteBytes(Encoding.UTF8.GetBytes(name));
        channel.Write(stream.GetBuffer());
    }

    public void SetPortCheck(bool portCheck) {
        channel.Write(new[] {
            portCheck
                ? UserToRakLibThreadMessageProtocol.PACKET_ENABLE_PORT_CHECK
                : UserToRakLibThreadMessageProtocol.PACKET_DISABLE_PORT_CHECK
        });
    }

    public void SetPacketsPerTickLimit(long limit) {
        var stream = new BinaryStream();
        stream.WriteByte(UserToRakLibThreadMessageProtocol.PACKET_SET_PACKETS_PER_TICK_LIMIT);
        stream.WriteLong(limit);
        channel.Write(stream.GetBuffer());
    }

    public void BlockAddress(string address, int timeout = 300) {
        var stream = new BinaryStream();
        stream.WriteByte(UserToRakLibThreadMessageProtocol.PACKET_BLOCK_ADDRESS);
        stream.WriteByte((byte) address.Length);
        stream.WriteBytes(Encoding.UTF8.GetBytes(address));
        stream.WriteInt(timeout);
        channel.Write(stream.GetBuffer());
    }

    public void UnblockAddress(string address) {
        var stream = new BinaryStream();
        stream.WriteByte(UserToRakLibThreadMessageProtocol.PACKET_UNBLOCK_ADDRESS);
        stream.WriteByte((byte) address.Length);
        stream.WriteBytes(Encoding.UTF8.GetBytes(address));
        channel.Write(stream.GetBuffer());
    }

    public void AddRawPacketFilter(string regex) {
        var stream = new BinaryStream();
        stream.WriteByte(UserToRakLibThreadMessageProtocol.PACKET_UNBLOCK_ADDRESS);
        stream.WriteBytes(Encoding.UTF8.GetBytes(regex));
        channel.Write(stream.GetBuffer());
    }
    
}