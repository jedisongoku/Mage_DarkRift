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
            runeCatalog.Clear();
            //Primary SKill Runes
            
            runeCatalog.Add(new Rune("DAMAGE BOOST", "DamageBoost", "Increases damage done by 20%", 1,1));
            runeCatalog.Add(new Rune("FASTER RECHARGE", "FasterRecharge", "Mana charges 30% faster", 1, 1));
            runeCatalog.Add(new Rune("QUICK DASH", "DashReducedCooldown", "Reduces Dash cooldown by 40%", 1, 1));
            runeCatalog.Add(new Rune("FROSTBITE", "Frostbite", "Applies 10% additional damage over 8 seconds", 1, 1));
            runeCatalog.Add(new Rune("CHILL", "Chill", "Attacks slow enemies by 15% for 3 seconds", 1, 1));   
            runeCatalog.Add(new Rune("BLOODTHIRST", "Bloodthirst", "Restores 30 health when you kill an enemy", 1, 1));
            runeCatalog.Add(new Rune("HEALTH BOOST", "HpBoost", "Increases max health by 50%", 1, 1));
            runeCatalog.Add(new Rune("SECOND CHANCE", "SecondChance", "Respawn once without losing your abilities", 1, 1));
            runeCatalog.Add(new Rune("SHIELD GUARD", "ShieldGuard", "Reduces damage taken by 35%", 1, 1));
            runeCatalog.Add(new Rune("STRONG HEART", "StrongHeart", "Increases healing over time by 75%", 1, 1));
            runeCatalog.Add(new Rune("SMART", "Smart", "Earn 50% more XP", 1, 1));
            runeCatalog.Add(new Rune("RAGE", "Rage", "Deal 15% more damage at low health", 1, 1));

            /*
            runeCatalog.Add(new Rune("Multi Shot", "MultiShot", "Fires an additional attack rapidly", 1, 1));
            runeCatalog.Add(new Rune("Invincible", "Invincible", "Become invincible once in a while", 1, 1));
            runeCatalog.Add(new Rune("Frost Nova", "FrostNova", "Attacks explode on hit, slowing nearby players", 1, 1));
            runeCatalog.Add(new Rune("Poison", "Poison", "Attacks cause poison spreading nearby players", 1, 1));
            runeCatalog.Add(new Rune("Bouncy", "Bouncy", "Attacks bounce of walls", 1, 1));        
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
        HUDManager.Instance.ShowRuneInfo(playerRuneList[index].DisplayName, playerRuneList[index].Description);
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
    void StrongHeart()
    {
        playerHealthManager.StrongHeart = true;

    }
    void Smart()
    {
        GameManager.Instance.GetCurrentPlayer.GetComponent<PlayerLevelManager>().SmartMultiplier = PlayFabDataStore.playerBaseStats.SmartMultiplier;

    }

    void SecondChance()
    {
        playerCombatManager.SecondChance = true;
    }
}
