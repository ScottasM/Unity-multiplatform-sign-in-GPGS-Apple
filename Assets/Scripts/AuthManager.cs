using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{
    public static AuthManager instance;

    [SerializeField] private GPGSManager gpgsManager;
    [SerializeField] private AppleManager appleManager;

    [SerializeField] public Image frontBG; // transparent background that should appear when sign in is in process blocking buttons.
    [SerializeField] public Text errorText;

    [HideInInspector] public bool doneLoading = false;

    public void Awake()
    {
        instance = this; 
    }

    public void Start()
    {
#if UNITY_ANDROID
        Destroy(appleManager);
        gpgsManager.Init();
#elif UNITY_IOS
        Destroy(gpgsManager);
        appleManager.Init();
#endif
    }

    public void ContinueAsGuest()
    {
        PlayerPrefs.SetInt("wantsGuest", 1);
        GuestManager.instance.LoadGuest();
    }


    /*
     * Should be called on start or upon showing login screen
     * checks whether player has selected guest/platform logins and if it should login automatically
     * if not, we wait for ManuallyAuthenticate();
     */
    public void StartLogin() 
    {
        if (PlayerPrefs.HasKey("wantsGuest")) {

            if (PlayerPrefs.GetInt("wantsGuest") == 1) {
                GuestManager.instance.LoadGuest();
                return;
            }
        }

        if (PlayerPrefs.HasKey("signedInBefore") && !PlayerPrefs.HasKey("ManuallyAuth")) {
#if UNITY_ANDROID
            GPGSManager.instance.AutomaticLogin();
#elif UNITY_IOS
            AppleManager.instance.LoginGameKit();
#endif
        }
    }

    /*
     * Used for authenticating with buttons;
     */
    public void ManuallyAuthenticate()
    {
#if UNITY_ANDROID
        if(PlayGamesPlatform.Instance.IsAuthenticated())
            PlayGamesPlatform.Instance.SignOut();
        GPGSManager.instance.SignIn(SignInInteractivity.CanPromptAlways); // CanPromptAlways makes the account selection UI to appear
#elif UNITY_IOS
        AppleManager.instance.LoginGameKit();
#endif
    }

    public void LogOut()
    {
        PlayerPrefs.SetInt("ManuallyAuthenticate", 1);
        PlayerPrefs.DeleteKey("signedInBefore");
        PlayerPrefs.DeleteKey("wantsGuest");

        frontBG.enabled = false;
        doneLoading = false;
    }

    public void DeleteAccount()
    {
        PlayerData.IntVar = 0; // reset the player values
        PlayerPrefs.SetInt("ManuallyAuth", 1); // Upon login screen wait for manual authentication (UI buttons)

        if (PlayerData.guest) {
            PlayerPrefs.DeleteAll();
        }
        else {
            
#if UNITY_ANDROID
            GPGSManager.instance.DeleteAccount();
#elif UNITY_IOS
            AppleManager.instance.DeleteAccount();
#endif
        }
        MenuManager.instance.ShowLoginScreen();
    }


    public void SaveData()
    {
        if(PlayerData.guest)
            GuestManager.instance.SaveData();
        else {
#if UNITY_ANDROID
            GPGSManager.instance.SaveData();
#elif UNITY_IOS
            AppleManager.instance.SaveData();
#endif
        }
    }

    public SavingData SetupFirstLogin() // returns a new SavingData object with default values
    {
        SavingData sd = new SavingData();
        sd.IntVar = 0;
        return sd;
    }
}
