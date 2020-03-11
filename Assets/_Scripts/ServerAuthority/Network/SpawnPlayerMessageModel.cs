using DarkRift;

public class SpawnPlayerMessageModel : IDarkRiftSerializable
{

    public ushort NetworkID { get; set; }
    public float X { get; set; }
    public float Z { get; set; }
    public string Nickname { get; set; }
    public byte Skin { get; set; }


    public void Deserialize(DeserializeEvent e)
    {
        NetworkID = e.Reader.ReadUInt16();
        X = e.Reader.ReadSingle();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(NetworkID);
    }
}
