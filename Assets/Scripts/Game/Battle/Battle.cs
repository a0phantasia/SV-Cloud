using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;

public class Battle
{
    public BattleManager Hud => BattleManager.instance;
    public BattleSettings settings => currentState?.settings;
    public BattleResult result => currentState?.result;
    public BattleState currentState;

    public Queue<Effect> effectQueue = new Queue<Effect>();

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
        Player.currentBattle = this;
    }

    public void PlayerAction(int[] data, bool isMe) {
        if (isMe) {
            Hud.EnemyPlayerAction(data);
        }

        var leader = (isMe ? currentState.myUnit : currentState.opUnit).leader.leaderCard;
        Effect effect = new Effect(data);
        effect.source = leader;
        effect.invokeUnit = isMe ? currentState.myUnit : currentState.opUnit;
        Enqueue(effect);

        if (effectQueue.Count == 1)
            ProcessQueue();
    }

    public void ProcessQueue() {
        while (effectQueue.Count > 0) {
            if (currentState.result.masterState != BattleResultState.None)
                break;

            var e = effectQueue.Peek();
            e.Apply(currentState);
            effectQueue.Dequeue();
        }
        Hud.ProcessQueue();
    }

    public void Enqueue(Effect effect) {
        effectQueue.Enqueue(effect);
    }
}
