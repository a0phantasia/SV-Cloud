using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSettings 
{
    public bool isLocal = false;
    public CardZone zone = CardZone.Engineering;
    public GameFormat format = GameFormat.Rotation;
    public int leaderHp = 20;
    public string masterName;
    public string clientName;

    public BattleSettings(CardZone zoneId, GameFormat formatId, bool local = false, int hp = 20) {
        isLocal = local;
        zone = zoneId;
        format = formatId;
        leaderHp = hp;
    }

    public BattleSettings(BattleSettings rhs) {
        isLocal = rhs.isLocal;
        zone = rhs.zone;
        format = rhs.format;
    }
}
