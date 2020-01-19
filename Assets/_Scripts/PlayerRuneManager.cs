using DarkRift;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRuneManager : MonoBehaviour
{
    //public static PlayerRuneManager Instance;

    private Dictionary<ushort, Rune> runeCatalog = new Dictionary<ushort, Rune>();

    public List<Rune> playerRuneList = new List<Rune>();

    private Player player;

    private PlayerCombatManager playerCombatManager;
    private PlayerHealthManager playerHealthManager;


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
            //runeCatalog.Add(new Rune("Life", "Life", "Get +1 life to continue adventure", 1, 1));
            runeCatalog.Add(11, new Rune(11, "Shield Guard", "ShieldGuard", "A shield circles around you reducing damage taken", 1, 1));
            runeCatalog.Add(12, new Rune(12, "Strong Hearth", "StrongHearth", "You are healed for more HP than before", 1, 1));
            /*
            //Offensive Runes
            runeCatalog.Add(new Rune("Flame Circle", "FlameCircle", "Summon 2 lightning bolts to spin around you", 1, 1));
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
        if(playerRuneList.Count > 4)
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
            writer.Write(playerRuneList[0].RuneID);
            writer.Write(playerRuneList[1].RuneID);
            writer.Write(playerRuneList[2].RuneID);
            Debug.Log("Rune" + playerRuneList[0].RuneID + " " + playerRuneList[0].DisplayName);
            Debug.Log("Rune" + playerRuneList[1].RuneID + " " + playerRuneList[1].DisplayName);
            Debug.Log("Rune" + playerRuneList[2].RuneID + " " + playerRuneList[2].DisplayName);

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

    public void ShowRuneSelection(ushort rune_1, ushort rune_2, ushort rune_3)
    {
        Debug.Log("RuneID " + rune_1 + " RuneName " + runeCatalog[rune_1].DisplayName);
        Debug.Log("RuneID " + rune_2 + " RuneName " + runeCatalog[rune_2].DisplayName);
        Debug.Log("RuneID " + rune_3 + " RuneName " + runeCatalog[rune_3].DisplayName);
        string[] runeNames = { runeCatalog[rune_1].DisplayName, runeCatalog[rune_2].DisplayName, runeCatalog[rune_3].DisplayName };

        HUDManager.Instance.DisplayRunes(runeNames);
    }

    void DamageBoost()
    {
        playerCombatManager.PrimarySkillDamage += Mathf.RoundToInt(PlayerBaseStats.Instance.PrimarySkillDamage * PlayerBaseStats.Instance.PrimarySkillBoostMultiplier);
        Debug.Log("Primary Skill Damage " + playerCombatManager.PrimarySkillDamage);
    }

    void AttackSpeedBoost()
    {
        playerCombatManager.PrimarySkillCooldown -= PlayerBaseStats.Instance.PrimarySkillCooldown * PlayerBaseStats.Instance.PrimarySkillSpeedMultiplier;
        Debug.Log("Primary Skill Cooldown " + playerCombatManager.PrimarySkillCooldown);
    }

    void DashReducedCooldown()
    {
        playerCombatManager.SecondarySkillCooldown -= PlayerBaseStats.Instance.SecondarySkillCooldown * PlayerBaseStats.Instance.SecondarySkillSpeedMultiplier;
        Debug.Log("Secondary Skill Cooldown " + playerCombatManager.SecondarySkillCooldown);
    }

    void Frostbite()
    {
        //playerCombatManager.Frostbite = true;
    }

    void Chill()
    {
        //playerCombatManager.Chill = true;
    }
    void Poison()
    {
        //playerCombatManager.Poison = true;
    }
    void Bouncy()
    {
        //playerCombatManager.Bouncy = true;
    }
    void MultiShot()
    {
        //playerCombatManager.MultiShot = true;
    }
    void Rage()
    {
        //playerHealthManager.Rage = true;
    }
    void FrostNova()
    {
        //playerCombatManager.FrostNova = true;
    }

    void Bloodthirst()
    {
        //playerHealthManager.Bloodthirst = true;
    }
    void HpBoost()
    {
        //playerHealthManager.HpBoost = true;
    }
    void Invincible()
    {

    }
    void Life()
    {

    }
    void ShieldGuard()
    {
        //playerHealthManager.ShieldGuard = true;
    }
    void StrongHearth()
    {
        //playerHealthManager.StrongHeart = true;
        //playerHealthManager.HealthGenerationRate += strongHeartMultiplier;

    }
    void FlameCircle()
    {
        
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
