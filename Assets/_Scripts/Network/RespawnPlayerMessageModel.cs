using DarkRift;

public class RespawnPlayerMessageModel : IDarkRiftSerializable
{
    public ushort NetworkID { get; set; }
    public int SpawnLocation { get; set; }

    public void Deserialize(DeserializeEvent e)
    {
        NetworkID = e.Reader.ReadUInt16();
        SpawnLocation = e.Reader.ReadInt32();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(NetworkID);
        e.Writer.Write(SpawnLocation);
    }
}
