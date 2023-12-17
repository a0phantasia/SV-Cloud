using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;

public class Battle
{
    public BattleManager Hud => BattleManager.instance;
    public BattleSettings settings;
    public BattleState currentState;

    public Battle() {}

    public Battle(BattleDeck masterDeck, BattleDeck clientDeck, BattleSettings settings) {
        Init(masterDeck, clientDeck, settings);
    }

    /// <summary>
    /// Create battle from Photon Network Hashtable for PVP.
    /// </summary>
    /// <param name="roomHash">Room properties</param>
    /// <param name="myHash">Local Player properties</param>
    /// <param name="opHash">Opponent properties</param>
    public Battle(Hashtable roomHash, Hashtable myHash, string myName, Hashtable opHash, string opName) {
        Random.InitState((int)roomHash["seed"]);

        int zfb = (int)roomHash["zfb"];
        int zone = zfb / 100;
        int format = zfb % 100 / 10;
        var settings = new BattleSettings((CardZone)zone, (GameFormat)format) {
            masterName = PhotonNetwork.IsMasterClient ? myName : opName,
            clientName = PhotonNetwork.IsMasterClient ? opName : myName,
        };

        var myDeck = new BattleDeck(zone, format, (int)myHash["craft"], (int[])myHash["deck"]);
        var opDeck = new BattleDeck(zone, format, (int)opHash["craft"], (int[])opHash["deck"]);

        if (PhotonNetwork.IsMasterClient)
            Init(myDeck, opDeck, settings);
        else
            Init(opDeck, myDeck, settings);
    }

    private void Init(BattleDeck masterDeck, BattleDeck clientDeck, BattleSettings settings) {
        this.currentState = new BattleState(masterDeck, clientDeck, settings);
        this.settings = settings;
        Player.currentBattle = this;
    }

    public void PlayerAction(int action, int[] data, bool isMe) {
        if (isMe) {
            Hud.EnemyPlayerAction(action, data);
        }
    }
}
