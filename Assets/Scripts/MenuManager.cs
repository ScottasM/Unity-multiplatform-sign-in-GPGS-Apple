using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{

    public static MenuManager instance;

    [SerializeField] private GameObject LoginScreen;
    [SerializeField] private GameObject MenuScreen;
    [SerializeField] private Text VarText;
    [SerializeField] private Text UIDText;

    public void Awake()
    {
        instance = this;
    }

    public void ShowLoginScreen()
    {
        LoginScreen.SetActive(true);
        MenuScreen.SetActive(false);

        AuthManager.instance.StartLogin();
    }

    public void ShowMenuScreen()
    {
        LoginScreen.SetActive(false);
        MenuScreen.SetActive(true);
        UpdateVariable();
        UpdatePlayerID();
    }

    public void PressedAddVariable()
    {
        PlayerData.IntVar++;
        /*
         * You can call AuthManager.instance.SaveData() or just use getters/setters as i've done in PlayerData, to automatically save once var has been updated
         */


        UpdateVariable();
    }

    public void PressedLogOut()
    {
        ShowLoginScreen();
        AuthManager.instance.LogOut();
    }

    public void PressedDeleteAccount()
    {
        ShowLoginScreen();
        AuthManager.instance.DeleteAccount();
    }

    public void UpdateVariable()
    {
        VarText.text = PlayerData.IntVar.ToString();
    }

    public void UpdatePlayerID()
    {
        UIDText.text = PlayerData.playerUID;
    }
}
