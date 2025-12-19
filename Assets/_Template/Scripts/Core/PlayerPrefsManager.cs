using UnityEngine;

namespace UISystems
{
    public enum TYPELEVEL
    {
        LEVEL,
        LEVELEnergy,
        LEVELHp,
        LEVELDmg,
        LEVELIncome,
        First
    }
    public static class PlayerPrefsManager
    {
        static string COINSKEY = "COINS";
        static string ITEMUNLOCKEDKEY = "ITEMUNLOCKED";
        static string SOUNDKEY = "SOUNDS";
        static string MUSICKEY = "MUSICS";
        static string VIBRATION = "VIBRATION";
        static string LEVELKEY = "LEVEL";
        static string TUTORIALKEY = "TUTORIALKEY";
        static string LEVELEnergyKEY = "LEVELEnergy";
        static string LEVELHpKEY = "LEVELHp";
        static string LEVELDmgKEY = "LEVELDmg";
        static string LEVELIncomeKEY = "LEVELIncome";



        public static int GetCoins()
        { return PlayerPrefs.GetInt(COINSKEY, -1); }

        public static void SaveCoins(int coinsAmount)
        { PlayerPrefs.SetInt(COINSKEY, coinsAmount); }

        public static int GetTutorial(int id)
        { return PlayerPrefs.GetInt(TUTORIALKEY+ "_"+ id, 0); }
        public static void SaveTutorial(int id)
        { PlayerPrefs.SetInt(TUTORIALKEY + "_" + id, 1); }



        public static int GetItemUnlockedState(int itemIndex)
        { return PlayerPrefs.GetInt(ITEMUNLOCKEDKEY + itemIndex); }

        public static void SetItemUnlockedState(int itemIndex, int state)
        { PlayerPrefs.SetInt(ITEMUNLOCKEDKEY + itemIndex, state); }




        public static int GetSoundState()
        { return PlayerPrefs.GetInt(SOUNDKEY, 1); }

        public static void SetSoundState(int state)
        { PlayerPrefs.SetInt(SOUNDKEY, state); }


        public static int GetMusicState()
        { return PlayerPrefs.GetInt(MUSICKEY, 1); }

        public static void SetMusicState(int state)
        { PlayerPrefs.SetInt(MUSICKEY, state); }


        public static int GetVibrationState()
        { return PlayerPrefs.GetInt(VIBRATION, 1); }

        public static void SetVibrationState(int state)
        { PlayerPrefs.SetInt(VIBRATION, state); }

        public static int GetLevel()
        { return PlayerPrefs.GetInt(LEVELKEY); }

        public static void SaveLevel(int level)
        { PlayerPrefs.SetInt(LEVELKEY, level); }

        public static int GetLevel(TYPELEVEL tYPELEVEL)
        {
            return PlayerPrefs.GetInt(tYPELEVEL.ToString());
        }

        public static void SaveLevel(TYPELEVEL tYPELEVEL, int level)
        { PlayerPrefs.SetInt(tYPELEVEL.ToString(), level); }
    }
}
