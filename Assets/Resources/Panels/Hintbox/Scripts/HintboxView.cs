using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintboxView : IMonoBehaviour
{
    [SerializeField] protected int optionNum = 1;
    [SerializeField] protected Image background = null;
    [SerializeField] protected RectTransform rect;
    [SerializeField] protected GameObject[] optionModule;

    [SerializeField] protected Text title, content;
    [SerializeField] protected Outline outline;
    [SerializeField] protected IButton confirmButtonSingle, confirmButton, cancelButton;

    public GameObject GetBackgroundObject() {
        return background?.gameObject;
    }

    public void SetOptionNum(int num) {
        if (!num.IsWithin(0, optionModule.Length)) {
            return;
        }
        foreach (var opt in optionModule) {
            opt.SetActive(false);
        }
        optionNum = num;
        if (num == 0)
            return;

        optionModule[num - 1].SetActive(true);
    }

    public void SetOptionCallback(Action callback, bool isConfirm = true) {
        if (callback == null)
            return;
            
        IButton button = (optionNum == 1) ? confirmButtonSingle : (isConfirm ? confirmButton : cancelButton);
        button.onPointerClickEvent.AddListener(callback.Invoke);
    }

    public void SetBackgroundColor(Color color) {
        if (background == null)
            return;
        background.color = color;
    }

    public void SetOutline(Color color, int thick = 2) {
        outline.effectColor = color;
        outline.effectDistance = new Vector2(thick, -thick);
    }

    public void SetSize(int x, int y) {
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y);
    }

    public void SetTitle(string text = "提示", int fontsize = 20, FontOption font = FontOption.Weibei) {
        SetText("title", text, fontsize, font);
    }

    public void SetContent(string text, int fontsize, FontOption font) {
        SetText("content", text, fontsize, font);
    }

    private void SetText(string which, string text, int fontsize = 20, FontOption font = FontOption.Weibei) {
        Text target = (which == "title") ? title : content;
        if (target == null)
            return;

        target.text = text;
        target.fontSize = fontsize;
        Font f = ResourceManager.instance.GetFont(font);
        if (f == null)
            return;
    
        target.font = f;
    }


}
