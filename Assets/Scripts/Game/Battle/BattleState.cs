using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BattleState
{
    public BattleState lastTurnState = null;
    public BattleSettings settings;

    public bool isMasterTurn;
    public BattleUnit masterUnit, clientUnit;
    public BattleUnit myUnit => settings.isLocal ? masterUnit : (PhotonNetwork.IsMasterClient ? masterUnit : clientUnit);
    public BattleUnit opUnit => settings.isLocal ? clientUnit : (PhotonNetwork.IsMasterClient ? clientUnit : masterUnit);

    public BattleState(BattleDeck masterDeck, BattleDeck clientDeck, BattleSettings settings) {
        this.settings = settings;
        isMasterTurn = Random.Range(0, 2) == 0;
        masterUnit = new BattleUnit(0, settings.masterName , masterDeck, isMasterTurn);
        clientUnit = new BattleUnit(1, settings.clientName , clientDeck, !isMasterTurn);
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

}
