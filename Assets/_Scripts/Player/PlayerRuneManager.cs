using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRuneManager : MonoBehaviour
{
    public static PlayerRuneManager Instance;

    private static List<Rune> runeCatalog = new List<Rune>();

    public static List<Rune> playerRuneList = new List<Rune>();

    private PlayerCombatManager playerCombatManager;
    private PlayerHealthManager playerHealthManager;


    void Start()
    {
        Instance = this;

    }
    public void Initialize()
    {
        

        if(runeCatalog != null)
        {
            //Primary SKill Runes
            runeCatalog.Add(new Rune("Damage Boost", "DamageBoost", "Deal Higher Damage", 1,1));
            runeCatalog.Add(new Rune("Faster Recharge", "FasterRecharge", "Primary Skill Recharges Faster", 1, 1));
            runeCatalog.Add(new Rune("Dash Reduced Cooldown", "DashReducedCooldown", "Secondary Skill Cooldown Reduced", 1, 1));
            runeCatalog.Add(new Rune("Frostbite", "Frostbite", "Attacks cause damage over time burn", 1, 1));
            runeCatalog.Add(new Rune("Chill", "Chill", "Attack slow enemies", 1, 1));
            //runeCatalog.Add(new Rune("Multi Shot", "MultiShot", "Fires an additional attack rapidly", 1, 1));
            runeCatalog.Add(new Rune("Rage", "Rage", "Attacks deal more damage at low HP", 1, 1));
            //runeCatalog.Add(new Rune("Frost Nova", "FrostNova", "Attacks explode on hit, slowing nearby players", 1, 1));
            /*
            runeCatalog.Add(new Rune("Poison", "Poison", "Attacks cause poison spreading nearby players", 1, 1));
            runeCatalog.Add(new Rune("Bouncy", "Bouncy", "Attacks bounce of walls", 1, 1));
            
            
            
            */
            //Survivability Runes
            runeCatalog.Add(new Rune("Bloodthirst", "Bloodthirst", "Restores HP when you kill an enemy", 1, 1));
            runeCatalog.Add(new Rune("HP Boost", "HpBoost", "Max HP increased", 1, 1));
            //runeCatalog.Add(new Rune("Invincible", "Invincible", "Become invincible once in a while", 1, 1));
            //runeCatalog.Add(new Rune("Life", "Life", "Get +1 life to continue adventure", 1, 1));
            runeCatalog.Add(new Rune("Shield Guard", "ShieldGuard", "A shield circles around you reducing damage taken", 1, 1));
            //runeCatalog.Add(new Rune("Strong Hearth", "StrongHearth", "You are healed for more HP than before", 1, 1));
            /*
            //Offensive Runes
            runeCatalog.Add(new Rune("Flame Circle", "FlameCircle", "Summon 2 lightning bolts to spin around you", 1, 1));
            runeCatalog.Add(new Rune("Flame Master", "FlameMaster", "Summon a bolt to attack enemies", 1, 1));
            runeCatalog.Add(new Rune("Frozen", "Frozen", "Create an area around player slowing down enemies entering the area", 1, 1));
            runeCatalog.Add(new Rune("Immune", "Immune", "You become poisonous, transferring plague to others who touches you", 1, 1));
            runeCatalog.Add(new Rune("Death Bomb", "DeathBomb", "Enemies when killed explode, dealing damage to small radius", 1, 1));
            runeCatalog.Add(new Rune("Death Nova", "DeathNova", "Enemies when killed fire out projectiles from their body dealing damage to who caught", 1, 1));*/


        }
        Debug.Log("TEST" + GameManager.Instance);
        if(GameManager.Instance.GetCurrentPlayer != null)
        {
            Debug.Log("Current Player Setup");
            playerCombatManager = GameManager.Instance.GetCurrentPlayer.GetComponent<PlayerCombatManager>();
            playerHealthManager = GameManager.Instance.GetCurrentPlayer.GetComponent<PlayerHealthManager>();
        }       
        RestartPlayerRunes();
    }

    public void RestartPlayerRunes()
    {
        playerRuneList.Clear();
        foreach(Rune r in runeCatalog)
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

    public void SelectRune(int index)
    {
        //playerCombatManager.RuneActivated();
        Invoke(playerRuneList[index].FunctionName, 0);
        ArrangeListAfterSelection(index);
    }

    void ArrangeListAfterSelection(int index)
    {
        int runeCount = playerRuneList.Count >= 3 ? 3 : playerRuneList.Count;
        if(playerRuneList.Count > 0)
        {
            for (int i = 0; i < runeCount; i++)
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

    void DamageBoost()
    {
        playerCombatManager.PrimarySkillDamage = 0;
    }

    void FasterRecharge()
    {
        playerCombatManager.PrimarySkillRecharge = PlayFabDataStore.playerBaseStats.PrimarySkillRecharge * PlayFabDataStore.playerBaseStats.FasterRechargeMultiplier;
    }

    void DashReducedCooldown()
    {
        playerCombatManager.SecondarySkillCooldown = PlayFabDataStore.playerBaseStats.SecondarySkillCooldown * PlayFabDataStore.playerBaseStats.DashSpeedMultiplier;
    }

    void Frostbite()
    {
        playerCombatManager.Frostbite = true;
    }

    void Chill()
    {
        playerCombatManager.Chill = true;
    }
    void Rage()
    {
        playerHealthManager.Rage = true;
    }
    void FrostNova()
    {
        playerCombatManager.FrostNova = true;
    }

    void Bloodthirst()
    {
        playerHealthManager.Bloodthirst = true;
    }
    void HpBoost()
    {
        playerHealthManager.HpBoost = true;
    }

    void ShieldGuard()
    {
        playerHealthManager.ShieldGuard = true;
    }
    void StrongHearth()
    {
        playerHealthManager.StrongHeart = true;

    }
}
