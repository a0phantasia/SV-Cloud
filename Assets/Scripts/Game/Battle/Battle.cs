using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;
using System.Linq;

public class Battle
{
    public BattleManager Hud => BattleManager.instance;
    public BattleSettings Settings => CurrentState?.settings;
    public BattleResult Result => CurrentState?.result;
    public BattleState CurrentState;

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
        this.CurrentState = new BattleState(masterDeck, clientDeck, settings);
        Player.currentBattle = this;
    }

    /// <summary>
    /// Player do actions, and will send RPC to notify other players IF "isMe = true". <br/>
    /// Same action should call this with isMe set to either true or false. <br/><br/>
    /// Eg. Player A draw 1 card. <br/>
    ///     "Draw" on side A deals with effect (Draw, isMe = true), <br/>
    ///     and A will then send RPC to B with (Draw, isMe = false), <br/>
    ///     "Draw" on side B deals with effect (Draw, isMe = false), <br/>
    /// </summary>
    public void PlayerAction(int[] data, bool isMe) {
        if (isMe) {
            short[] enemyData = data.Select(x => (short)x).ToArray();
            var ability = (EffectAbility)data[0];
            
            if (ability.IsTargetSelectableAbility()) {
                for (int i = 2; i < data.Length; i++)
                    enemyData[i] = (short)((1 - (data[i] / 100)) * 100 + data[i] % 100);
            }
            Hud.EnemyPlayerAction(enemyData);
        }

        var leader = (isMe ? CurrentState.myUnit : CurrentState.opUnit).leader.leaderCard;
        Effect effect = new Effect(data);
        effect.source = leader;
        effect.invokeUnit = isMe ? CurrentState.myUnit : CurrentState.opUnit;
        EnqueueEffect(effect);

        if (effectQueue.Count == 1)
            ProcessQueue();
    }

    public void ProcessQueue() {
        while (effectQueue.Count > 0) {
            if (CurrentState.result.masterState != BattleResultState.None)
                break;

            var e = effectQueue.Peek();
            e.Apply(CurrentState);
            effectQueue.Dequeue();
        }
        Hud.ProcessQueue();
    }

    public void EnqueueEffect(Effect effect) {
        effectQueue.Enqueue(effect);
    }

    public void EnqueueEffect(List<Effect> effects) {
       for (int i = 0; i < effects.Count; i++) {
            EnqueueEffect(effects[i]);
        }
    }
}
