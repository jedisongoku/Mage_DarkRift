using DarkRift;

public class PoisonShopMessageModel : IDarkRiftSerializable
{
    public ushort NetworkID { get; set; }
    public ushort Time { get; set; }

    public ushort PoisonID { get; set; }
    public bool IsTimerOn { get; set; }
    public bool IsPoisonAreaEnabled { get; set; }
    public bool IsPoisonActivated { get; set; }




    public void Deserialize(DeserializeEvent e)
    {
        NetworkID = e.Reader.ReadUInt16();
        Time = e.Reader.ReadUInt16();
        PoisonID = e.Reader.ReadUInt16();
        IsTimerOn = e.Reader.ReadBoolean();
        IsPoisonAreaEnabled = e.Reader.ReadBoolean();
        IsPoisonActivated = e.Reader.ReadBoolean();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(NetworkID);
        e.Writer.Write(Time);
        e.Writer.Write(PoisonID);
        e.Writer.Write(IsTimerOn);
        e.Writer.Write(IsPoisonAreaEnabled);
        e.Writer.Write(IsPoisonActivated);
    }
}

