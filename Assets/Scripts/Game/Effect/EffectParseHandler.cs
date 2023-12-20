using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class EffectParseHandler
{
    public static Dictionary<string, string> SetResult(int[] data) {
        var dict = new Dictionary<string, string>();
        var result = (BattleResultState)data[0] switch {
            BattleResultState.Win => "win",
            BattleResultState.Lose => "lose",
            _ => "none",
        };
        dict.Set("result", result);
        return dict;
    }

    public static Dictionary<string, string> KeepCard(int[] data) {
        var dict = new Dictionary<string, string>();
        var str = string.Empty;
        for (int i = 0; i < data.Length; i++) {
            str += data[i].ToString() + ((i == data.Length - 1) ? string.Empty : "/");
        }
        dict.Set("change", str);
        return dict;
    }
}
