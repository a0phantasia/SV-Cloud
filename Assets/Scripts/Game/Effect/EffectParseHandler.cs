using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class EffectParseHandler
{
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
