using DarkRift;

public class ServerPlayer : IDarkRiftSerializable
{

    public ushort ID { get; set; }
    public float X { get; set; }
    public float Z { get; set; }
    public float Horizontal { get; set; }
    public float Vertical { get; set; }
    public string Nickname { get; set; }
    public byte Skin { get; set; }

    public ServerPlayer()
    {

    }

    public void Deserialize(DeserializeEvent e)
    {
        ID = e.Reader.ReadUInt16();
        X = e.Reader.ReadSingle();
        Z = e.Reader.ReadSingle();
        Horizontal = e.Reader.ReadSingle();
        Vertical = e.Reader.ReadSingle();
        Nickname = e.Reader.ReadString();
        Skin = e.Reader.ReadByte();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(ID);
        e.Writer.Write(X);
        e.Writer.Write(Z);
        e.Writer.Write(Horizontal);
        e.Writer.Write(Vertical);
        e.Writer.Write(Nickname);
        e.Writer.Write(Skin);

    }
}
