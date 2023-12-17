using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : Manager<TitleManager>
{
    [SerializeField] private InputHintbox inputHintbox;

    private void Start() {
        AudioSystem.instance.PlayMusic("Title");
    }

    public void GameStart() {
        if (GameManager.versionData == null) {
            RequestManager.OnRequestFail("正在獲取版本資料，請稍後再進入遊戲");
            return;
        }

        if (GameManager.versionData.IsEmpty()) {
            RequestManager.OnRequestFail("獲取版本資料失敗，請重新啟動遊戲");
            return;
        }

        if (Player.gameData.IsEmpty()) {
            OpenNicknamePanel();
            return;
        }

        SceneLoader.instance.ChangeScene(SceneId.Main);
    }

    private void OpenNicknamePanel() {
        void SetNickname(string name) {
            Player.gameData.SetNickname(name);
            GameStart();
        }

        inputHintbox.SetHintboxActive(true);
        inputHintbox.SetOptionCallback(SetNickname);
    }
}
