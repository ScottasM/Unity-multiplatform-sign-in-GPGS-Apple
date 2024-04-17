using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using System;

public class GuestManager : MonoBehaviour
{
    public static GuestManager instance;

    public void Awake()
    {
        instance = this; 
    }

    public void LoadGuest()
    {
        AuthManager.instance.doneLoading = false;
        PlayerData.guest = true;
        if (PlayerPrefs.HasKey("saveString")) {
            string data = PlayerPrefs.GetString("saveString");
            DataParsing.loadDataFromString(data);
        }
        else {
            DataParsing.LoadDataFromSDO(AuthManager.instance.SetupFirstLogin());
            PlayerData.playerUID = Guid.NewGuid().ToString();
        }
        AuthManager.instance.doneLoading = true;

        MenuManager.instance.ShowMenuScreen();
    }

    public void SaveData()
    {
        if (!AuthManager.instance.doneLoading)
            return;
        string data = DataParsing.returnSavingString();
        PlayerPrefs.SetString("saveString", data);
        return;
        
    }
}
