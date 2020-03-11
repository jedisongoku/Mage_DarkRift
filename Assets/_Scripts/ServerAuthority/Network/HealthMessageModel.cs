using DarkRift;

public class HealthMessageModel : IDarkRiftSerializable
{
    public ushort NetworkID { get; set; }
    public int Health { get; set; }

    public void Deserialize(DeserializeEvent e)
    {
        NetworkID = e.Reader.ReadUInt16();
        Health = e.Reader.ReadInt32();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(NetworkID);
        e.Writer.Write(Health);
    }
}
