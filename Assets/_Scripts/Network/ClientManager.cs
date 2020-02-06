using DarkRift.Client.Unity;
using DarkRift.Client;
using DarkRift;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class ClientManager : MonoBehaviour
{
    public static ClientManager Instance;
    public UnityClient client;

    Dictionary<ushort, Player> networkPlayers = new Dictionary<ushort, Player>();
    public GameObject localPlayer;

    void Awake()
    {
        
        Instance = this;
        client = GetComponent<UnityClient>();
        client.MessageReceived += MessageReceived;
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        //SceneManager.LoadScene("PoisonShop", LoadSceneMode.Additive);
    }

    public void Add(ushort id, Player player)
    {
        networkPlayers.Add(id, player);
    }

    private void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            if (message.Tag == NetworkTags.MovePlayerTag)
                Movement(sender, e);
            else if (message.Tag == NetworkTags.PrimarySkillTag)
                PrimarySkill(sender, e);
            else if (message.Tag == NetworkTags.SecondarySkillTag)
                SecondarySkill(sender, e);
            else if (message.Tag == NetworkTags.HealthPlayerTag)
                Health(sender, e);
            else if (message.Tag == NetworkTags.RespawnPlayerTag)
                Respawn(sender, e);
            else if (message.Tag == NetworkTags.ScoreboardTag)
                Scoreboard(sender, e);
            else if (message.Tag == NetworkTags.ShowRuneTag)
                ShowRune(sender, e);
            else if (message.Tag == NetworkTags.ParticleEffectTag)
                ParticleEffect(sender, e);
            else if (message.Tag == NetworkTags.UpdateCooldownTag)
                UpdateCooldown(sender, e);
            else if (message.Tag == NetworkTags.IncreaseHealthTag)
                IncreaseHealth(sender, e);
            else if (message.Tag == NetworkTags.PoisonShopTag)
                PoisonShop(sender, e);
        }
    }

    private void PoisonShop(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                PoisonShopMessageModel newMessage = reader.ReadSerializable<PoisonShopMessageModel>();

                if (newMessage.IsTimerOn) PoisonShopManager.Instance.InitializePoisonTimer();
                PoisonShopManager.Instance.PoisonTimer = newMessage.Time;
                if (newMessage.IsPoisonAreaEnabled) PoisonShopManager.Instance.EnablePoisonArea(newMessage.PoisonID);
                else PoisonShopManager.Instance.DisablePoisonArea();

            }
        }
    }

    private void IncreaseHealth(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                ushort id = reader.ReadUInt16();
                ushort playerMaxHealth = reader.ReadUInt16();


                if (networkPlayers.ContainsKey(id))
                    networkPlayers[id].GetComponent<PlayerHealthManager>().IncreaseMaxHP(playerMaxHealth);
            }
        }
    }

    private void UpdateCooldown(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                ushort id = reader.ReadUInt16();
                float primaryCooldown = reader.ReadSingle();
                float secondaryCooldown = reader.ReadSingle();


                if (networkPlayers.ContainsKey(id))
                    networkPlayers[id].GetComponent<PlayerCombatManager>().UpdateSkillCooldowns(primaryCooldown, secondaryCooldown);
            }
        }
    }

    private void ParticleEffect(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                ParticleEffectMessageModel newMessage = reader.ReadSerializable<ParticleEffectMessageModel>();
                ushort id = newMessage.NetworkID;
                ushort particleID = newMessage.ParticleID;


                if (networkPlayers.ContainsKey(id))
                {
                    if (particleID == PlayerRuneManager.Frostbite_ID) networkPlayers[id].GetComponent<PlayerParticleManager>().Frostbite(newMessage.Frostbite);
                    if (particleID == PlayerRuneManager.Chill_ID) networkPlayers[id].GetComponent<PlayerParticleManager>().Chill(newMessage.Chill);
                    if (particleID == PlayerRuneManager.Bloodthirst_ID) networkPlayers[id].GetComponent<PlayerParticleManager>().Bloodthirst();
                    if (particleID == PlayerRuneManager.HpBoost_ID) networkPlayers[id].GetComponent<PlayerParticleManager>().HpBoost();
                    if (particleID == PlayerRuneManager.Rage_ID) networkPlayers[id].GetComponent<PlayerParticleManager>().Rage(newMessage.Rage);
                    if (particleID == PlayerRuneManager.ShieldGuard_ID) networkPlayers[id].GetComponent<PlayerParticleManager>().ShieldGuard(true);
                    if (particleID == PlayerRuneManager.StrongHeart_ID) networkPlayers[id].GetComponent<PlayerParticleManager>().StrongHeart(true);
                    if (particleID == PlayerRuneManager.Multishot_ID) networkPlayers[id].GetComponent<PlayerCombatManager>().MultiShot = newMessage.Multishot;
                    if (particleID == PlayerRuneManager.FrostNova_ID) networkPlayers[id].GetComponent<PlayerCombatManager>().FrostNova = newMessage.FrostNova;
                    if (particleID == PlayerRuneManager.FlameCircle_ID) networkPlayers[id].GetComponent<PlayerParticleManager>().FlameCircle(newMessage.FlameCircle);
                    if (particleID == PoisonShopManager.Poison_ID) networkPlayers[id].GetComponent<PlayerParticleManager>().Poison(newMessage.Poison);
                    if (particleID == PoisonShopManager.WintersChill_ID) networkPlayers[id].GetComponent<PlayerParticleManager>().WintersChill(newMessage.WintersChill);
                    if (particleID == PoisonShopManager.DashLock_ID) networkPlayers[id].GetComponent<PlayerParticleManager>().DashLock(newMessage.DashLock);
                    if (particleID == PoisonShopManager.DistortAim_ID) networkPlayers[id].GetComponent<PlayerParticleManager>().DistortAim(newMessage.DistortAim);

                }
            }
        }
    }

    private void ShowRune(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                ushort id = reader.ReadUInt16();
                ushort listCount = reader.ReadUInt16();
                Debug.Log("Rune List count in ShowRune " + listCount);
                ushort[] runeIDs;
                if (listCount > 3)
                {
                    runeIDs = new ushort[3];
                    for (int i = 0; i < 3; i++)
                    {
                        runeIDs[i] = reader.ReadUInt16();
                    }
                }
                else
                {
                    runeIDs = new ushort[listCount];
                    for (int i = 0; i < listCount; i++)
                    {
                        runeIDs[i] = reader.ReadUInt16();
                    }
                }
                /*
                ushort runeID_1 = reader.ReadUInt16();
                ushort runeID_2 = reader.ReadUInt16();
                ushort runeID_3 = reader.ReadUInt16();*/


                if (networkPlayers.ContainsKey(id))
                    networkPlayers[id].GetComponent<PlayerRuneManager>().ShowRuneSelection(runeIDs);
            }
        }
    }

    void Scoreboard(object sender, MessageReceivedEventArgs e)
    {
        //Debug.Log("Scoreboard message " + e.GetMessage());
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                ScoreboardMessageModel newMessage = reader.ReadSerializable<ScoreboardMessageModel>();
                ushort killerId = newMessage.KillerID;
                Debug.Log("score returned in the message " + newMessage.Score);
                ushort score = (ushort)newMessage.Score;
                ushort victimId = newMessage.VictimID;

                ScoreManager.Instance.UpdateScoreboard(killerId, score, victimId);
            }
        }
    }

    private void Respawn(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                RespawnPlayerMessageModel newMessage = reader.ReadSerializable<RespawnPlayerMessageModel>();
                ushort id = newMessage.NetworkID;
                ushort spawnLocation = (ushort)newMessage.SpawnLocation;


                if (networkPlayers.ContainsKey(id))
                    networkPlayers[id].GetComponent<Player>().RespawnPlayer(spawnLocation);
            }
        }
    }

    void Health(object sender, MessageReceivedEventArgs e)
    {
        //Debug.Log("Health Message " + e.GetMessage());
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                HealthMessageModel newMessage = reader.ReadSerializable<HealthMessageModel>();
                ushort id = newMessage.NetworkID;
                //Debug.Log("Health returned in the message " + newMessage.Health);
                ushort health = (ushort)newMessage.Health;

                //ushort id = reader.ReadUInt16();
                //ushort health = reader.ReadUInt16();
                

                if (networkPlayers.ContainsKey(id))
                    networkPlayers[id].GetComponent<PlayerHealthManager>().HealthMessageReceived(health);
            }
        }
    }

    void Movement(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                MovementMessageModel newMessage = reader.ReadSerializable<MovementMessageModel>();
                ushort id = newMessage.NetworkID;
                float horizontal = newMessage.Horizontal;
                float vertical = newMessage.Vertical;
                float pos_x = newMessage.Pos_X;
                float pos_z = newMessage.Pos_Z;
                float move_x = newMessage.Move_X;
                float move_z = newMessage.Move_Z;
                float speed = newMessage.WalkSpeed;


                if (networkPlayers.ContainsKey(id))
                    networkPlayers[id].GetComponent<PlayerMovementManager>().SetMovement(new Vector3(pos_x,0,pos_z), new Vector3(move_x,0,move_z), horizontal, vertical, speed);
            }
        }
    }

    void SecondarySkill(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                ushort id = reader.ReadUInt16();
                bool secondary = reader.ReadBoolean();


                if (networkPlayers.ContainsKey(id))
                    networkPlayers[id].GetComponent<PlayerCombatManager>().SecondarySkillMessageReceived();
            }
        }
    }

    void PrimarySkill(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                ushort id = reader.ReadUInt16();
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                float distort = reader.ReadSingle();
                bool multishot = reader.ReadBoolean();
                Debug.Log("Multishot " + multishot);


                if (networkPlayers.ContainsKey(id))
                    networkPlayers[id].GetComponent<PlayerCombatManager>().PrimarySkillMessageReceived(x, y, z, distort, multishot);
            }
        }
    }

    public void DestroyPlayer(ushort id)
    {
        Player o = networkPlayers[id];

        Destroy(o.gameObject);

        networkPlayers.Remove(id);
        ScoreManager.Instance.RemovePlayer(id, false);
    }

    public int TotalPlayer
    {
        get
        {
            return networkPlayers.Count;
        }
    }

    
}
