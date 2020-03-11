using DarkRift;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRuneManager : MonoBehaviour
{
    //public static PlayerRuneManager Instance;
    public static readonly ushort Frostbite_ID = 4;
    public static readonly ushort Chill_ID = 5;
    public static readonly ushort Multishot_ID = 6;
    public static readonly ushort Rage_ID = 7;
    public static readonly ushort FrostNova_ID = 8;
    public static readonly ushort Bloodthirst_ID = 9;
    public static readonly ushort HpBoost_ID = 10;
    public static readonly ushort ShieldGuard_ID = 11;
    public static readonly ushort StrongHeart_ID = 12;
    public static readonly ushort Life_ID = 13;
    public static readonly ushort FlameCircle_ID = 14;

    private Dictionary<ushort, Rune> runeCatalog = new Dictionary<ushort, Rune>();

    public List<Rune> playerRuneList = new List<Rune>();

    private Player player;

    private PlayerCombatManager playerCombatManager;
    private PlayerHealthManager playerHealthManager;
    private ushort[] runeOptions;


    // Start is called before the first frame update
    void Start()
    {
        //Instance = this;
        

        if(runeCatalog != null)
        {
            //Primary SKill Runes
            runeCatalog.Add(1, new Rune(1, "Damage Boost", "DamageBoost", "Deal Higher Damage", 1,1));
            runeCatalog.Add(2, new Rune(2, "Attack Speed Boost", "AttackSpeedBoost", "Primary Skill Cooldown Reduced", 1, 1));
            runeCatalog.Add(3, new Rune(3, "Dash Reduced Cooldown", "DashReducedCooldown", "Secondary Skill Cooldown Reduced", 1, 1));
            runeCatalog.Add(4, new Rune(4, "Frostbite", "Frostbite", "Attacks cause damage over time burn", 1, 1));
            runeCatalog.Add(5, new Rune(5, "Chill", "Chill", "Attack slow enemies", 1, 1));
            runeCatalog.Add(6, new Rune(6, "Multi Shot", "MultiShot", "Fires an additional attack rapidly", 1, 1));
            runeCatalog.Add(7, new Rune(7, "Rage", "Rage", "Attacks deal more damage at low HP", 1, 1));
            runeCatalog.Add(8, new Rune(8, "Frost Nova", "FrostNova", "Attacks explode on hit, slowing nearby players", 1, 1));
            /*
            runeCatalog.Add(new Rune("Poison", "Poison", "Attacks cause poison spreading nearby players", 1, 1));
            runeCatalog.Add(new Rune("Bouncy", "Bouncy", "Attacks bounce of walls", 1, 1));
            
            
            
            */
            //Survivability Runes
            runeCatalog.Add(9, new Rune(9, "Bloodthirst", "Bloodthirst", "Restores HP when you kill an enemy", 1, 1));
            runeCatalog.Add(10, new Rune(10, "HP Boost", "HpBoost", "Max HP increased", 1, 1));
            //runeCatalog.Add(new Rune("Invincible", "Invincible", "Become invincible once in a while", 1, 1));    
            runeCatalog.Add(11, new Rune(11, "Shield Guard", "ShieldGuard", "A shield circles around you reducing damage taken", 1, 1));
            runeCatalog.Add(12, new Rune(12, "Strong Heart", "StrongHeart", "You are healed for more HP than before", 1, 1));
            runeCatalog.Add(13, new Rune(13, "Life", "Life", "Get +1 life to continue adventure", 1, 1));
            runeCatalog.Add(14, new Rune(14, "Flame Circle", "FlameCircle", "Summon 2 flame bolts to spin around you", 1, 1));
            /*
            //Offensive Runes
            
            runeCatalog.Add(new Rune("Flame Master", "FlameMaster", "Summon a bolt to attack enemies", 1, 1));
            runeCatalog.Add(new Rune("Frozen", "Frozen", "Create an area around player slowing down enemies entering the area", 1, 1));
            runeCatalog.Add(new Rune("Immune", "Immune", "You become poisonous, transferring plague to others who touches you", 1, 1));
            runeCatalog.Add(new Rune("Death Bomb", "DeathBomb", "Enemies when killed explode, dealing damage to small radius", 1, 1));
            runeCatalog.Add(new Rune("Death Nova", "DeathNova", "Enemies when killed fire out projectiles from their body dealing damage to who caught", 1, 1));*/


        }
        player = GetComponent<Player>();
        playerCombatManager = GetComponent<PlayerCombatManager>();
        playerHealthManager = GetComponent<PlayerHealthManager>();
        if(player.IsServer)
        {
            RestartPlayerRunes();
        }
        
    }

    public void RestartPlayerRunes()
    {
        playerRuneList.Clear();
        foreach(Rune r in runeCatalog.Values)
        {
            for(int i = 0; i < r.ChargeAmount; i++)
            {
                playerRuneList.Add(r);
            }
        }
        Debug.Log("Catalog # of Runes: " + runeCatalog.Count);
        Debug.Log("Player # of Runes: " + playerRuneList.Count);
        ShuffleList(playerRuneList);
    }

    public void SelectRune(ushort index)
    {
        Debug.Log("Selected Rune " + playerRuneList[index].FunctionName);
        //playerCombatManager.RuneActivated();
        Invoke(playerRuneList[index].FunctionName, 0);
        ArrangeListAfterSelection(index);
    }

    void ArrangeListAfterSelection(int index)
    {
        if(playerRuneList.Count > 0)
        {
            for (int i = 0; i < 3; i++)
            {
                if (i != index)
                {
                    playerRuneList.Add(playerRuneList[0]);
                }
                playerRuneList.RemoveAt(0);
            }
        }
        
    }


    void ShuffleList(List<Rune> list)
    {
        System.Random _random = new System.Random();
        Rune temp;

        int n = list.Count;
        for(int i = 0; i < n; i++)
        {
            int rand = i + (int)(_random.NextDouble() * (n - i));
            temp = list[rand];
            list[rand] = list[i];
            list[i] = temp;
        }
    }
    //Server-Side Message
    public void SendShowRuneMessage()
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write((ushort)player.ID);
            writer.Write((ushort)playerRuneList.Count);
            if(playerRuneList.Count >= 3)
            {
                for(int i = 0; i < 3; i++)
                {
                    writer.Write(playerRuneList[i].RuneID);
                }
            }
            else
            {
                for (int i = 0; i < playerRuneList.Count; i++)
                {
                    writer.Write(playerRuneList[i].RuneID);
                }
            }
            /*
            writer.Write(playerRuneList[0].RuneID);
            writer.Write(playerRuneList[1].RuneID);
            writer.Write(playerRuneList[2].RuneID);
            Debug.Log("Rune" + playerRuneList[0].RuneID + " " + playerRuneList[0].DisplayName);
            Debug.Log("Rune" + playerRuneList[1].RuneID + " " + playerRuneList[1].DisplayName);
            Debug.Log("Rune" + playerRuneList[2].RuneID + " " + playerRuneList[2].DisplayName);*/

            using (Message message = Message.Create(NetworkTags.ShowRuneTag, writer))
                player.ServerClient.SendMessage(message, SendMode.Reliable);
        }
    }

    //Client-Side Message
    public void SendSelectRuneMessage(ushort index)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(index);

            using (Message message = Message.Create(NetworkTags.SelectRuneTag, writer))
                player.Client.SendMessage(message, SendMode.Reliable);
        }
    }

    public void ShowRuneSelection(ushort[] runeIDs)
    {
        string[] runeNames = new string[runeIDs.Length];
        ushort[] rune_id = new ushort[runeIDs.Length];
        for(int i = 0; i < runeIDs.Length; i++)
        {
            runeNames[i] = runeCatalog[runeIDs[i]].DisplayName;
            rune_id[i] = runeIDs[i];
        }

        runeOptions = rune_id;
        Debug.Log("Number of runes to display " + runeNames.Length);
        HUDManager.Instance.DisplayRunes(runeNames);
        
    }

    void DamageBoost()
    {
        playerCombatManager.PrimarySkillDamage += Mathf.RoundToInt(PlayerBaseStats.Instance.PrimarySkillDamage * PlayerBaseStats.Instance.PrimarySkillBoostMultiplier);
        Debug.Log("Primary Skill Damage " + playerCombatManager.PrimarySkillDamage);
        playerCombatManager.UpdateCooldownMessage();
    }

    void AttackSpeedBoost()
    {
        playerCombatManager.PrimarySkillCooldown -= PlayerBaseStats.Instance.PrimarySkillCooldown * PlayerBaseStats.Instance.PrimarySkillSpeedMultiplier;
        Debug.Log("Primary Skill Cooldown " + playerCombatManager.PrimarySkillCooldown);
        playerCombatManager.UpdateCooldownMessage();
    }

    void DashReducedCooldown()
    {
        playerCombatManager.SecondarySkillCooldown -= PlayerBaseStats.Instance.SecondarySkillCooldown * PlayerBaseStats.Instance.SecondarySkillSpeedMultiplier;
        Debug.Log("Secondary Skill Cooldown " + playerCombatManager.SecondarySkillCooldown);
        playerCombatManager.UpdateCooldownMessage();
    }

    void Frostbite()
    {
        playerCombatManager.Frostbite = true;
    }

    void Chill()
    {
        playerCombatManager.Chill = true;
    }

    void MultiShot()
    {
        playerCombatManager.MultiShot = true;
        playerCombatManager.UpdateMultishotMessage(true, Multishot_ID);
    }
    void FrostNova()
    {
        playerCombatManager.FrostNova = true;
        playerCombatManager.UpdateFrostNovaMessage(true, FrostNova_ID);
    }
    void Rage()
    {
        playerHealthManager.Rage = true;
    }
    void Bloodthirst()
    {
        playerHealthManager.Bloodthirst = true;
    }
    void HpBoost()
    {
        playerHealthManager.HpBoost();
    }

    void ShieldGuard()
    {
        playerHealthManager.ShieldGuard = true;
    }
    void StrongHeart()
    {
        playerHealthManager.StrongHeart();
    }
    void Poison()
    {

    }
    void Bouncy()
    {

    }
    void Invincible()
    {

    }
    void Life()
    {
        player.HasExtraLife = true;
    }
    void FlameCircle()
    {
        player.SendFlameCircleParticleMessage(true, FlameCircle_ID);
    }
    void FlameMaster()
    {
        
    }
    void Frozen()
    {
        
    }
    void Immune()
    {
        
    }
    void DeathBomb()
    {
        
    }
    void DeathNova()
    {
        
    }
}
