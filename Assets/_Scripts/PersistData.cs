using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PersistData : MonoBehaviour
{
    private string GameFilePath;
    private string GameFileName;
    private bool gamePaused;

    public delegate void LoadDataComplete();

    public static event LoadDataComplete OnloadDataComplete;

    public static PersistData instance;

    public GameDataModel GameData;


    // Start is called before the first frame update
    void Awake()
    {
        GameFilePath = Application.persistentDataPath + "/userData";
        GameFileName = "/mageTest_02.dat";

        // Singelton - There can be only one...
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    void OnEnable()
    {
        instance.Load();
    }
    void OnDisable()
    {
        instance.Save();
    }

    public void Load()
    {
        GameData = new GameDataModel();

        if (File.Exists(GameFilePath + "/" + GameFileName))
        {
            var bf = new BinaryFormatter();
            var file = File.Open(GameFilePath + "/" + GameFileName, FileMode.Open);
            var gameData = (GameDataModel)bf.Deserialize(file);
            file.Close();

            if (string.IsNullOrEmpty(gameData.PlayerName))
            {
                CreateDefaultGameData();
            }
            else
            {
                GameData = gameData;
            }
        }
        else
        {
            CreateDefaultGameData();
        }


        if (OnloadDataComplete != null)
        {
            OnloadDataComplete();
        }
    }

    public void Save()
    {
        //CollectDataToSave();

        if (!Directory.Exists(GameFilePath))
        {
            Directory.CreateDirectory(GameFilePath);
        }

        var bf = new BinaryFormatter();
        var file = File.Create(GameFilePath + "/" + GameFileName);

        bf.Serialize(file, this.GameData);
        file.Close();
    }


    private void CreateDefaultGameData()
    {
        GameData = new GameDataModel
        {
            PlayerName = "Mage" + UnityEngine.Random.Range(10000, 100000),
            CoinBalance = 0,
            GemBalance = 0,
            EnergyBalance = 50,
            GameMode = "Deathmatch"
        };
    }
}

        
