using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerData
{

    private static int intVar = 0;

    public static int IntVar
    {
        get { return intVar; }
        set { 
            intVar = value;
            AuthManager.instance.SaveData();
        }
    }
    public static bool guest = false;


    public static string playerUID;
}
