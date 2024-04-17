using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.Authentication;
#if UNITY_IOS
using Apple.GameKit;
using Apple.GameKit.Players;
#endif

using System;
using UnityEngine.Networking;

public class AppleManager : MonoBehaviour
{
    public static AppleManager instance;
    private bool signingIn = false;

    public void Awake()
    {
        instance = this; 
    }
    

    #region GameKitLogin

#if UNITY_IOS
    private GKIdentityVerificationResponse fetchItemsResponse;
#endif

    private string Signature;
    private string TeamPlayerID;
    private string Salt;
    private string PublicKeyUrl;
    private string Timestamp;

    public async Task LoginGameKit()
    {
#if UNITY_IOS
        if (signingIn) return;
        signingIn = true;
        if (PlayerPrefs.HasKey("ManuallyAuth")) {
            try {
                AuthenticationService.Instance.ClearSessionToken();
                Debug.Log("Session token cleared");
            }
            catch (Exception e) {
                Debug.LogError("Session token failed to clear : " + e.Message);
            }
            
        }
        AuthManager.instance.doneLoading = false;
        AuthManager.instance.frontBG.enabled = true;
        if (AuthenticationService.Instance.IsSignedIn) {
            Debug.Log("player is logged in");
            if (AuthenticationService.Instance.IsAuthorized) {
                Debug.Log("Player is authorized");
                ReadDataAsync(); // start loading player
            }
            else {
                SignInCachedUserAsync();
            }
            Debug.Log("Session token exists : " + AuthenticationService.Instance.SessionTokenExists);

        }
        else {
            Debug.Log("not signed in, accepting manual auth");
            try {
                var player = await GKLocalPlayer.Authenticate();
                Debug.Log($"GameKit Authentication: player {player}");

                var localPlayer = GKLocalPlayer.Local;
                Debug.Log($"Local Player: {localPlayer.DisplayName}");

                fetchItemsResponse = await GKLocalPlayer.Local.FetchItems();

                Signature = Convert.ToBase64String(fetchItemsResponse.GetSignature());
                TeamPlayerID = localPlayer.TeamPlayerId;

                Salt = Convert.ToBase64String(fetchItemsResponse.GetSalt());
                PublicKeyUrl = fetchItemsResponse.PublicKeyUrl;
                Timestamp = fetchItemsResponse.Timestamp.ToString();

                Debug.Log($"GameKit Authentication: signature => {Signature}");
                Debug.Log($"GameKit Authentication: publickeyurl => {PublicKeyUrl}");
                Debug.Log($"GameKit Authentication: salt => {Salt}");
                Debug.Log($"GameKit Authentication: Timestamp => {Timestamp}");
                SignInWithAppleGameCenterAsync(Signature, TeamPlayerID, PublicKeyUrl, Salt, fetchItemsResponse.Timestamp);
            }
            catch (Exception ex) {

                AuthManager.instance.frontBG.enabled = false;
                Debug.LogError("Login with apple failed : " + ex.Message);
                AuthManager.instance.errorText.text = "Login failed";
                signingIn = false;
            }
        }
#endif
    }
    #endregion

    #region Unity auth
    async Task SignInWithAppleGameCenterAsync(string signature, string teamPlayerId, string publicKeyURL, string salt, ulong timestamp)
    {
        try {
            await AuthenticationService.Instance.SignInWithAppleGameCenterAsync(signature, teamPlayerId, publicKeyURL, salt, timestamp);
            Debug.Log("Sign in is successfull");
            PlayerPrefs.DeleteKey("ManuallyAuth");
            ReadDataAsync();
        }
        catch(Exception ex) {
            Debug.LogError("Unity login : " + ex.Message);
            AuthManager.instance.errorText.text = "Failed to authenticate with unity servers";
            AuthManager.instance.frontBG.enabled = false;
            signingIn = false;
        }
    }

    async Task SignInCachedUserAsync()
    {
        // Check if a cached player already exists by checking if the session token exists
        if (!AuthenticationService.Instance.SessionTokenExists) {
            signingIn = false;
            return;
        }

        try {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
            ReadDataAsync();
        }
        catch (Exception ex) {
            Debug.LogError("Cached login failed : " + ex.Message);
            AuthManager.instance.frontBG.enabled = false;
            signingIn = false;
        }
    }
    #endregion

    private long lastSaveTime;
    public async Task SaveData()
    {
        if (AuthManager.instance.doneLoading == false)
            return;

        try
        {
            var playerData = new Dictionary<string, object>{
              {"alldata", DataParsing.returnSavingString()},
            };
            await CloudSaveService.Instance.Data.Player.SaveAsync(playerData);
            Debug.Log($"Saved data {string.Join(',', playerData)}");
        }
        catch(Exception ex)
        {
            Debug.LogError("error saving data " + ex.Message);
            AuthManager.instance.frontBG.enabled = false;
        }

    }

    public async Task ReadDataAsync()
    {
        try
        {
            var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> {
              "alldata"
            });


            if (playerData.TryGetValue("alldata", out var firstKey))
            {
                Debug.Log($"firstKeyName value: {firstKey.Value.GetAs<string>()}");

                DataParsing.loadDataFromString(firstKey.Value.GetAs<string>());
                Debug.Log("data loaded");
            }
            else
            {
                Debug.Log("First time loading, creating save");
                SavingData sd = AuthManager.instance.SetupFirstLogin();
                DataParsing.LoadDataFromSDO(sd);
            }

            AuthManager.instance.doneLoading = true;
            if (PlayerPrefs.HasKey("ManuallyAuth"))
                PlayerPrefs.DeleteKey("ManuallyAuth");
            PlayerPrefs.SetInt("signedInBefore", 1);

            PlayerData.guest = false;
            PlayerData.playerUID = AuthenticationService.Instance.PlayerId;

            MenuManager.instance.ShowMenuScreen();
            signingIn = false;
        }
        catch(Exception ex)
        {
            Debug.LogError("failed to load data: " + ex.Message);
            AuthManager.instance.frontBG.enabled = false;
            signingIn = false;
        }

    }

    public void DeleteAccount()
    {
        try {
            AuthenticationService.Instance.DeleteAccountAsync();
            Debug.Log("Delete succeeded");
            signingIn = false;
            PlayerData.guest = true;
            AuthManager.instance.doneLoading = false;
        }
        catch(Exception ex) {
            Debug.LogError($"Error deleting account: {ex.Message}");
            signingIn = false;
        }
    }
}




