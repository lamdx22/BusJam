using Hiker.GUI;
using SkyJam;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DateTime = System.DateTime;

public partial class GameManager : MonoBehaviour
{
    public PopupLoadLevel popupLoadLevel;
    public PopupThang popupWin;
    public PopupThua popupLose;

    public string urlStore;

    public BoardController[] levels;
    public int[] levelNums;

    public const string Booster_Meteor = "BO_ThienThach";
    public const string Booster_DongHo = "BO_DongHo";
    public const string Booster_MeteorTime = "BO_ThienThachTime";
    public const string Booster_HourglassTime = "BO_DongHoTime";
    public const string PowerUp_Magnet = "PO_Hut";
    public const string PowerUp_Hammer = "PO_Bua";
    public const string PowerUp_Saw = "PO_Cua";
    public const string PowerUp_Freeze = "PO_Bang";

    public static GameManager instance = null;

    public PlayerInfo PInfo = new PlayerInfo();

    static readonly Dictionary<string, int> unlockDic = new Dictionary<string, int>()
    {
        { "PO_Bua", 1 }
    };

    static readonly Dictionary<string, int> inventory = new Dictionary<string, int>();

    static readonly int[] GoldLevels = new int[]
    {
        12
    };
    static readonly int[] DoKhoLevels = new int[]
    {
        0,0,0,0,0,0,0,0,0,0,0,0,0,1,0
    };
    public static long GetGoldRewardFromLevel(int lvlIdx)
    {
        if (lvlIdx < 0) return 0;

        if (GoldLevels.Length > lvlIdx)
        {
            return GoldLevels[lvlIdx];
        }
        else if (GoldLevels.Length > 0)
        {
            return GoldLevels[GoldLevels.Length - 1];
        }
        return 0;
    }
    public static int GetDoKhoLevel(int lvlIdx)
    {
        if (lvlIdx < 0) return 0;

        if (DoKhoLevels.Length > lvlIdx)
        {
            return DoKhoLevels[lvlIdx];
        }

        int lvlNum = lvlIdx + 1;
        int doKho = (lvlNum + 1) % 5;
        switch (doKho)
        {
            case 0: return 1; // Hard
            //case 1: return 0; // Normal
            default:
                return 0; // Normal
        }

        return 0;
    }

    private void Awake()
    {
        instance = this;
        if (SoundManager.instance)
        {
            SoundManager.instance.MusicEnable = false;
            SoundManager.instance.SaveSoundAndMusicSetting();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        var curLvl = PInfo.Level;

        popupLoadLevel.LoadLevel(levels[curLvl], levelNums[curLvl], false);
    }

    public bool LoadNextLevel()
    {
        var curLvl = PInfo.Level;
        curLvl++;
        if (curLvl >= levels.Length)
        {
            popupWin.Show(0);
            return false;
        }
        else
        {
            PInfo.Level = curLvl;
            popupLoadLevel.LoadLevel(levels[curLvl], levelNums[curLvl], true);
            return true;
        }
    }

    public void OnLose()
    {
        var curLvl = PInfo.Level;

        popupLose.Show();
    }


    public void GoToStore()
    {
        if (string.IsNullOrEmpty(urlStore) == false)
        {
            Application.OpenURL(urlStore);
        }
    }

    public bool IsUnlockFeature(string featureName)
    {
        if (unlockDic.ContainsKey(featureName)) {
            return unlockDic[featureName] <= (PInfo.Level + 1);
        }
        return false;
    }
    public int GetLevelUnlock(string feature)
    {
        if (unlockDic.ContainsKey(feature))
        {
            return unlockDic[feature];
        }

        return int.MaxValue >> 1;
    }

    public int GetGamerCounter(string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }
    public int GetCurrentMaterial(string key)
    {
        if (inventory.ContainsKey(key))
        {
            return inventory[key];
        }

        return 0;
    }
    public bool OnUsePowerUp(string key, int quantity)
    {
        if (inventory.ContainsKey(key) && inventory[key] >= quantity)
        {
            inventory[key] -= quantity;

            return true;
        }
        return false;
    }

    public static string GetTimeRemainStringNoDate(DateTime now, DateTime expireTime)
    {
        if (now < expireTime)
        {
            var ts = expireTime - now;
            if (ts.Days * 24 + ts.Hours <= 0)
            {
                return string.Empty;
                //return string.Format(Localization.Get("{0:00}:{1:00}"), ts.Minutes, ts.Seconds);
            }
            return string.Empty;
            //return string.Format(Localization.Get("{0:00}:{1:00}:{2:00}"), ts.Days * 24 + ts.Hours, ts.Minutes, ts.Seconds);
        }
        else
        {
            return string.Empty;
        }
    }
}
