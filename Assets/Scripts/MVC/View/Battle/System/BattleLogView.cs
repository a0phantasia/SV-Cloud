using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleLogView : BattleBaseView
{
    public static string[] title => new string[] { "戰場紀錄", "已使用的卡片", "被破壞的從者", "對戰資訊" };

    [SerializeField] private OptionSelectModel selectModel;
    [SerializeField] private Text titleText;
    [SerializeField] private BattleLogMainView mainView;
    [SerializeField] private PageView pageView;


    public void SetActive(bool active) {
        gameObject.SetActive(active);
    }

    public void SetState(BattleState state) {
        mainView?.Log(state);
    }

    private void OnLogSetPage() {
        titleText?.SetText(title[selectModel.Page]);
        pageView?.SetPage(selectModel.Page, selectModel.LastPage);
    }

    public void OnLogPrevPage() {
        selectModel.PrevPage();
        OnLogSetPage();
    }

    public void OnLogNextPage() {
        selectModel.NextPage();
        OnLogSetPage();
    }
}
