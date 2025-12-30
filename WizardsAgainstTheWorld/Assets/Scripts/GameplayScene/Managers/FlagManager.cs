using System.Collections.Generic;
using UnityEngine;

public enum GameFlag
{
    None,
    TookALoan,
    FerrymanAdvertized,
    UsePickaxeAdvertized,
    BuyKeyAdvertized,
}

public interface IFlagManager
{
    void SetFlag(GameFlag gameFlag, bool value = true);
    bool GetFlag(GameFlag gameFlag);
}

public class FlagManager : MonoBehaviour, IFlagManager
{
    private Dictionary<GameFlag, bool> _flags = new();
    
    public void SetFlag(GameFlag gameFlag, bool value = true)
    {
        if (_flags.ContainsKey(gameFlag))
        {
            _flags[gameFlag] = value;
        }
        else
        {
            _flags.Add(gameFlag, value);
        }
        
        GameLogger.Log($"Flag {gameFlag} set to {value}");
    }
    
    public bool GetFlag(GameFlag gameFlag)
    {
        return _flags.ContainsKey(gameFlag) && _flags[gameFlag];
    }
}
