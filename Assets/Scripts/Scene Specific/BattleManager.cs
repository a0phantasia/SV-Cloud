using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class BattleManager : Manager<BattleManager>
{
    public Battle Battle => Player.currentBattle;

    [SerializeField] private BattleSystemView systemView;
    [SerializeField] private BattleUnitView myView, opView;
    [SerializeField] private PhotonView masterPhotonView, clientPhotonView;

    public PhotonView myPhotonView => PhotonNetwork.IsMasterClient ? masterPhotonView : clientPhotonView;

    protected override void Awake()
    {
        base.Awake();
        if (Battle.settings.isLocal)
            return;
            
        NetworkManager.instance.onDisconnectEvent += OnLocalPlayerDisconnect;
        NetworkManager.instance.onOtherPlayerLeftRoomEvent += OnOtherPlayerDisconnect;

        myPhotonView.RequestOwnership();
    }

    public void OnConfirmBattleResult() {
        var scene = Battle.settings.isLocal ? SceneId.Main : SceneId.Room;
        SceneLoader.instance.ChangeScene(scene);
    }

    protected void OpenDisconnectHintbox(string message) {
        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent(message);
        hintbox.SetOptionNum(1);
        hintbox.SetOptionCallback(OnConfirmBattleResult);
    }

    public void OnLocalPlayerDisconnect(DisconnectCause disconnectCause, string failedMessage) {
        OpenDisconnectHintbox("我方連線已中斷");
        NetworkManager.instance.onDisconnectEvent -= OnLocalPlayerDisconnect;
    }

    public void OnOtherPlayerDisconnect(Photon.Realtime.Player player) {
        OpenDisconnectHintbox("對方連線已中斷");
        NetworkManager.instance.onOtherPlayerLeftRoomEvent -= OnOtherPlayerDisconnect;
    }

    public void EnemyPlayerAction(int action, int[] data) {
        if (Battle.settings.isLocal)
            return;

        var photonView = masterPhotonView.IsMine ? masterPhotonView : clientPhotonView;
        photonView.RPC("RPCPlayerAction", RpcTarget.Others, (object)action, (object)data);
    }

    [PunRPC]
    private void RPCPlayerAction(int action, int[] data) {
        Battle.PlayerAction(action, data, false);
    }


    public void SetState() {
        myView.SetUnit(Battle?.currentState?.myUnit);
        opView.SetUnit(Battle?.currentState?.opUnit);
    }
}
