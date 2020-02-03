using DarkRift;

public class ParticleEffectMessageModel : IDarkRiftSerializable
{
    public ushort NetworkID { get; set; }
    public ushort ParticleID { get; set; }
    public bool Frostbite { get; set; }
    public bool StrongHeart { get; set; }
    public bool ShieldGuard { get; set; }
    public bool Healing { get; set; }
    public bool DashTrail { get; set; }
    public bool Chill { get; set; }
    public bool Rage { get; set; }
    public bool Bloodthirst { get; set; }
    public bool HpBoost { get; set; }
    public bool Multishot { get; set; }
    public bool FrostNova { get; set; }
    public bool Poison { get; set; }
    public bool DashLock { get; set; }
    public bool WintersChill { get; set; }
    public bool DistortAim { get; set; }


    public void Deserialize(DeserializeEvent e)
    {
        NetworkID = e.Reader.ReadUInt16();
        ParticleID = e.Reader.ReadUInt16();
        Frostbite = e.Reader.ReadBoolean();
        StrongHeart = e.Reader.ReadBoolean();
        ShieldGuard = e.Reader.ReadBoolean();
        Healing = e.Reader.ReadBoolean();
        DashTrail = e.Reader.ReadBoolean();
        Chill = e.Reader.ReadBoolean();
        Rage = e.Reader.ReadBoolean();
        Bloodthirst = e.Reader.ReadBoolean();
        HpBoost = e.Reader.ReadBoolean();
        Multishot = e.Reader.ReadBoolean();
        FrostNova = e.Reader.ReadBoolean();
        Poison = e.Reader.ReadBoolean();
        DashLock = e.Reader.ReadBoolean();
        WintersChill = e.Reader.ReadBoolean();
        DistortAim = e.Reader.ReadBoolean();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(NetworkID);
        e.Writer.Write(ParticleID);
        e.Writer.Write(Frostbite);
        e.Writer.Write(StrongHeart);
        e.Writer.Write(ShieldGuard);
        e.Writer.Write(Healing);
        e.Writer.Write(DashTrail);
        e.Writer.Write(Chill);
        e.Writer.Write(Rage);
        e.Writer.Write(Bloodthirst);
        e.Writer.Write(HpBoost);
        e.Writer.Write(Multishot);
        e.Writer.Write(FrostNova);
        e.Writer.Write(Poison);
        e.Writer.Write(DashLock);
        e.Writer.Write(WintersChill);
        e.Writer.Write(DistortAim);
    }
}

