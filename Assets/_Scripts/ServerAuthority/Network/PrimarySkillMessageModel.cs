using DarkRift;

public class PrimarySkillMessageModel : IDarkRiftSerializable
{

    public ushort NetworkID { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }



    public void Deserialize(DeserializeEvent e)
    {
        NetworkID = e.Reader.ReadUInt16();
        X = e.Reader.ReadSingle();
        Y = e.Reader.ReadSingle();
        Z = e.Reader.ReadSingle();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(NetworkID);
        e.Writer.Write(X);
        e.Writer.Write(Y);
        e.Writer.Write(Z);
    }
}

