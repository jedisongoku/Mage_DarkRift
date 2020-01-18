using DarkRift;

public class MovementMessageModel : IDarkRiftSerializable
{
    public ushort NetworkID { get; set; }
    public float Horizontal { get; set; }
    public float Vertical { get; set; }
    public float Pos_X { get; set; }
    public float Pos_Z { get; set; }
    public float Move_X { get; set; }
    public float Move_Z { get; set; }

    public void Deserialize(DeserializeEvent e)
    {
        NetworkID = e.Reader.ReadUInt16();
        Horizontal = e.Reader.ReadSingle();
        Vertical = e.Reader.ReadSingle();
        Pos_X = e.Reader.ReadSingle();
        Pos_Z = e.Reader.ReadSingle();
        Move_X = e.Reader.ReadSingle();
        Move_Z = e.Reader.ReadSingle();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(NetworkID);
        e.Writer.Write(Horizontal);
        e.Writer.Write(Vertical);
        e.Writer.Write(Pos_X);
        e.Writer.Write(Pos_Z);
        e.Writer.Write(Move_X);
        e.Writer.Write(Move_Z);
    }
}