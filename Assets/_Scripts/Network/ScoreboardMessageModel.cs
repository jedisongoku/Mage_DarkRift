using DarkRift;

public class ScoreboardMessageModel : IDarkRiftSerializable
{
    public ushort KillerID { get; set; }
    public ushort Score { get; set; }

    public ushort VictimID { get; set; }

    public void Deserialize(DeserializeEvent e)
    {
        KillerID = e.Reader.ReadUInt16();
        Score = e.Reader.ReadUInt16();
        VictimID = e.Reader.ReadUInt16();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(KillerID);
        e.Writer.Write(Score);
        e.Writer.Write(VictimID);
    }
}