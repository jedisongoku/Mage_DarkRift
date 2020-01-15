using DarkRift;

public class MovementMessageModel : IDarkRiftSerializable
{
    public ushort NetworkID { get; set; }
    public float Horizontal { get; set; }
    public float Vertical { get; set; }
    public float X { get; set; }
    public float Z { get; set; }

    public void Deserialize(DeserializeEvent e)
    {
        NetworkID = e.Reader.ReadUInt16();
        Horizontal = e.Reader.ReadSingle();
        Vertical = e.Reader.ReadSingle();
        X = e.Reader.ReadSingle();
        Z = e.Reader.ReadSingle();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(NetworkID);
        e.Writer.Write(Horizontal);
        e.Writer.Write(Vertical);
        e.Writer.Write(X);
        e.Writer.Write(Z);
    }
}