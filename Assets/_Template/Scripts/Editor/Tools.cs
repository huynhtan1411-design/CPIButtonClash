using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UISystems;
using TemplateSystems;

public class Tools : Editor
{
    [MenuItem("Tools/Delete Prefs")]
    public static void DeletePrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("Tools/Cheat/Add Gold")]
    public static void CheatGold()
    {
        TemplateSystems.DataManager.AddCoins(10000);
    }

    [MenuItem("Tools/Simulator/AddSilver")]
    public static void Simulator()
    {
        BuildingManager.Instance.AddResources(100);
    }

    [MenuItem("Tools/Simulator/UnlockAllBuilding")]
    public static void Unlocks()
    {
        TemplateSystems.DataManager.Instance.CheatUnlockAllBuildings();
    }
}
