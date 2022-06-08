namespace SkyWing.RakLib.Ipc;

public interface InterThreadChannelReader {
    
    public byte[]? Read();
    
}

public interface InterThreadChannelWriter {
    
    public void Write(byte[] str);
    
}