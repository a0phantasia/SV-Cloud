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

    public void WatchRoom() {
        BattleSettings settings = new BattleSettings(CardZone.Engineering, GameFormat.Rotation, true) {
            masterName = Player.Nickname,
            clientName = "電腦",
        };
        BattleDeck myDeck = new BattleDeck(1, 1, 0, (new List<int>() { 100000113, 100000114, 100000115 })
            .Concat(Enumerable.Repeat(100000112 , 37)).ToArray());
        BattleDeck opDeck = new BattleDeck(1, 1, 0, Enumerable.Repeat(100000111, 40).ToArray());
        Battle battle = new Battle(myDeck, opDeck, settings);
        SceneLoader.instance.ChangeScene(SceneId.Battle);
    }
}
