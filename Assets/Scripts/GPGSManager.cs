using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System;


public class GPGSManager : MonoBehaviour
{
    #region singleton
    public static GPGSManager instance;
    public void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        instance = this;
    }
    #endregion

    #region config

    private bool configured = false;
    private PlayGamesClientConfiguration pgcc;

    public void Init()
    {
        configured = ConfigureGPGS();
    }

    

    private bool ConfigureGPGS()
    {
#if UNITY_ANDROID
        try {
            pgcc = new PlayGamesClientConfiguration.Builder().EnableSavedGames().Build();
        }
        catch (Exception e) {
            Debug.LogError(e);
            return false;
        }
        return true;
#endif
    }
    #endregion

    #region login

    public void AutomaticLogin()
    {
#if UNITY_ANDROID
        SignIn(SignInInteractivity.CanPromptOnce); // enables silent sign in for next time, doesn't prompt account selection
#endif
    }

    public void SignIn(SignInInteractivity inter)
    {
#if UNITY_ANDROID
        AuthManager.instance.frontBG.enabled = true;
        //Time.timeScale = 0f;
        PlayGamesPlatform.InitializeInstance(pgcc);
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate(inter, (status) => {

            if (status == SignInStatus.Success) {
                PlayerData.playerUID = PlayGamesPlatform.Instance.GetUserId(); 
                PlayerData.guest = false;
                PlayerPrefs.SetInt("signedInBefore", 1);
                PlayerPrefs.SetInt("wantsGuest", 0);
                AuthManager.instance.doneLoading = false;
                LoadData();
            }
            else {
                Debug.LogError(status);
                AuthManager.instance.frontBG.enabled = false;
                AuthManager.instance.errorText.text = "Failed to sign in. Continue as guest or try again.";
            }
        });
#endif
    }

    #endregion

    #region LoadingSaveGame
    public void LoadData()
    {

#if UNITY_ANDROID
        if (PlayGamesPlatform.Instance.localUser.authenticated) {
            PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution("SaveGameName", DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseLongestPlaytime, OpenLoadGame);
        }
#endif
    }

    public void OpenLoadGame(SavedGameRequestStatus status, ISavedGameMetadata metadata)
    {
#if UNITY_ANDROID
        if (status == SavedGameRequestStatus.Success) {
            PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(metadata, GameLoaded);
        }
        else {
            Debug.LogError("Failed to open save game" + status);
            AuthManager.instance.frontBG.enabled = false;
            AuthManager.instance.errorText.text = "Failed to open saved game. Continue as guest or try again.";
        }
#endif
    }

    public void GameLoaded(SavedGameRequestStatus status, byte[] bytes)
    {
        if (status == SavedGameRequestStatus.Success) {
            Debug.Log("Game loaded successfully");
            SavingData sd;
            if (bytes != null && bytes.Length > 1)
                sd = DataParsing.returnSDObject(bytes);
            else sd = AuthManager.instance.SetupFirstLogin();

            DataParsing.LoadDataFromSDO(sd);
            AuthManager.instance.doneLoading = true;

            if (PlayerPrefs.HasKey("ManuallyAuth"))
                PlayerPrefs.DeleteKey("ManuallyAuth");

            MenuManager.instance.ShowLoginScreen();
        }
        else {
            Debug.LogError(status);
            AuthManager.instance.frontBG.enabled = false;
            AuthManager.instance.errorText.text = "Failed to load saved data. Continue as guest or try again.";
        }
    }

    #endregion

    #region SavingGame
    public void SaveData()
    {
        
        if (!AuthManager.instance.doneLoading)
            return;

#if UNITY_ANDROID
        if (PlayGamesPlatform.Instance.localUser.authenticated) {
            PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution("SaveGameName", DataSource.ReadCacheOrNetwork,ConflictResolutionStrategy.UseLongestPlaytime,OpenSaveGame);
        }
#endif
    }

    public void OpenSaveGame(SavedGameRequestStatus status, ISavedGameMetadata metadata)
    {
#if UNITY_ANDROID
        if(status == SavedGameRequestStatus.Success) {
            byte[] byteArray = DataParsing.returnSavingBytes();

            SavedGameMetadataUpdate updateForMetadata = new SavedGameMetadataUpdate.Builder().WithUpdatedDescription(DateTime.Now.ToString()).Build();
            PlayGamesPlatform.Instance.SavedGame.CommitUpdate(metadata, updateForMetadata, byteArray, GameSaved);

        }
        else {
            Debug.LogError("saving game failed : " + status);
            AuthManager.instance.frontBG.enabled = false;
            AuthManager.instance.errorText.text = "Failed to open saved game. Continue as guest or try again.";
        }
#endif
    }

    

    public void GameSaved(SavedGameRequestStatus status, ISavedGameMetadata meta)
    {
        if (status == SavedGameRequestStatus.Success) { Debug.Log("Game Saved"); }
        else Debug.Log("failed to save");
    }

    #endregion

    #region DeleteAccount
    public void DeleteAccount()
    {
#if UNITY_ANDROID
        PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution("SaveGameName", DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseLongestPlaytime, OnDeleteSavedGame);
#endif
        }
    public void OnDeleteSavedGame(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
#if UNITY_ANDROID
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        if (status == SavedGameRequestStatus.Success) {
            // delete the game.
            savedGameClient.Delete(game);
            AuthManager.instance.errorText.text = "Delete successfull.";
            Debug.Log("Account deleted successfully");
        }
        else {
            // handle error
            Debug.LogError("Account deletion failed : " + status);
            AuthManager.instance.errorText.text = "Failed to delete account";
        }
#endif
    }
    #endregion
}
