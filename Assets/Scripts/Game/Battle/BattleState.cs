using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BattleState
{
    public BattleSettings settings;
    public BattleResult result;
    public Effect currentEffect;

    public BattleUnit masterUnit, clientUnit;
    protected bool isMasterTurn = false;
    public bool IsMasterTurn { 
        get => isMasterTurn;
        set => SetMasterTurn(value);
    }

    public BattleUnit currentUnit => isMasterTurn ? masterUnit : clientUnit;
    public BattleUnit myUnit => settings.isLocal ? masterUnit : (PhotonNetwork.IsMasterClient ? masterUnit : clientUnit);
    public BattleUnit opUnit => settings.isLocal ? clientUnit : (PhotonNetwork.IsMasterClient ? clientUnit : masterUnit);

    public BattleState(BattleDeck masterDeck, BattleDeck clientDeck, BattleSettings settings) {
        this.settings = settings;
        this.result = new BattleResult();

        //! Debug mode always go first
        isMasterTurn = GameManager.instance.debugMode ? true : (Random.Range(0, 2) == 0);

        masterUnit = new BattleUnit(0, settings.masterName , masterDeck, isMasterTurn);
        clientUnit = new BattleUnit(1, settings.clientName , clientDeck, !isMasterTurn);
    }

    public BattleState(BattleState rhs) {
        isMasterTurn = rhs.isMasterTurn;
        settings = new BattleSettings(rhs.settings);
        result = new BattleResult(rhs.result);
        currentEffect = new Effect(rhs.currentEffect);
        masterUnit = new BattleUnit(rhs.masterUnit);
        clientUnit = new BattleUnit(rhs.clientUnit);
    }

    public virtual BattleUnit GetUnitById(int id) {
        if (masterUnit.id == id)
            return masterUnit;
        if (clientUnit.id == id)
            return clientUnit;
        return null;
    }
    public virtual BattleUnit GetRhsUnitById(int lhsId) {
        return GetUnitById(1 - lhsId);
    }

    public virtual void SetMasterTurn(bool newTurn) {
        isMasterTurn = newTurn;
        masterUnit.isMyTurn = isMasterTurn;
        clientUnit.isMyTurn = !isMasterTurn;
    }

    public virtual BattleUnit GetInvokeUnit(BattleCard card) {
        if (myUnit.hand.cards.Contains(card) || myUnit.field.cards.Contains(card) ||
            myUnit.deck.cards.Contains(card) || myUnit.grave.usedCards.Contains(card.CurrentCard) ||
            (myUnit.leader.leaderCard == card) || (myUnit.territory == card))
                return myUnit;

        if (opUnit.hand.cards.Contains(card) || opUnit.field.cards.Contains(card) ||
            opUnit.deck.cards.Contains(card) || opUnit.grave.usedCards.Contains(card.CurrentCard) ||
            (opUnit.leader.leaderCard == card) || (opUnit.territory == card))
                return opUnit;

        return null;
    }

}
