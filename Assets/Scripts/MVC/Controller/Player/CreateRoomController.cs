using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateRoomController : IMonoBehaviour
{
    [SerializeField] private CreateRoomModel createModel;
    [SerializeField] private CreateRoomView createView;

    public void CreateRoom() {
        createView.CreateRoom();
        createModel.CreateRoom();
    }

    public void JoinRoom() {
        createView.JoinRoom();
        createModel.JoinRoom();
    }

    //! Currently for debug battle.
    public void WatchRoom() {
        if (!GameManager.instance.debugMode) 
            return;

        BattleSettings settings = new BattleSettings(CardZone.Engineering, GameFormat.Rotation, true) {
            evolveStart = 1,
            masterName = Player.Nickname,
            clientName = "電腦",
        };
        BattleDeck myDeck = new BattleDeck(1, 1, 1, (new List<int>() { 100003111, 100003114, 100003211, 100003212 })
            .Concat(Enumerable.Repeat(100001111 , 36)).ToArray());
        BattleDeck opDeck = new BattleDeck(1, 1, 2, Enumerable.Repeat(100002111, 40).ToArray());
        Battle battle = new Battle(myDeck, opDeck, settings);
        SceneLoader.instance.ChangeScene(SceneId.Battle);
    }
}
