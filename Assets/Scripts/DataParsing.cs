using System;

using UnityEngine;


[System.Serializable]
public class SavingData
{
    public int IntVar = 0;
    public string UID;
}

public static class DataParsing 
{
    public static byte[] returnSavingBytes()
    {
        SavingData sd = new SavingData();
        sd.IntVar = PlayerData.IntVar;
        sd.UID = PlayerData.playerUID;


        string updatedBinaryData = JsonUtility.ToJson(sd);
        byte[] byteArray = System.Text.ASCIIEncoding.ASCII.GetBytes(updatedBinaryData);
        return byteArray;
    }

    public static string returnSavingString()
    {
        SavingData sd = new SavingData();
        sd.IntVar = PlayerData.IntVar;
        sd.UID = PlayerData.playerUID;

        string updatedBinaryData = JsonUtility.ToJson(sd);
        return updatedBinaryData;
    }

    public static void loadDataFromString(string data)
    {
        SavingData sd = JsonUtility.FromJson<SavingData>(data);

        PlayerData.IntVar = sd.IntVar;
        PlayerData.playerUID = sd.UID;
    }

    public static void LoadDataFromSDO(SavingData sd)
    {
        PlayerData.IntVar = sd.IntVar;
        PlayerData.playerUID = sd.UID;
    }

    public static SavingData returnSDObject(string data)
    {
        return JsonUtility.FromJson<SavingData>(data);
    }

    public static SavingData returnSDObject(byte[] bytes)
    {
        string data = System.Text.ASCIIEncoding.ASCII.GetString(bytes);
        return JsonUtility.FromJson<SavingData>(data);
    }
}
