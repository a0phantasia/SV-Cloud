using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hintbox : Panel
{
    [SerializeField] protected HintboxView hintboxView;

    public static Hintbox OpenHintbox() {
        return Hintbox.OpenHintbox<Hintbox>();
    }

    public static T OpenHintbox<T>() where T : Hintbox {
        T panel = Panel.OpenPanel<T>();
        return panel;        
    }

    public static Hintbox OpenHintbox(string content) {
        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent(content, 18, FontOption.Weibei);
        hintbox.SetOptionNum(1);
        return hintbox;
    }


    public void SetHintboxActive(bool active) {
        GameObject topLayer = (background == null) ? gameObject : background.gameObject;
        topLayer.SetActive(active);
    }

    public virtual void SetOptionNum(int num) {
        hintboxView.SetOptionNum(num);
    }

    public virtual void SetSize(int x, int y) {
        hintboxView.SetSize(x, y);
    }

    public virtual void SetTitle(string text = "提示", int fontsize = 22, FontOption font = FontOption.Weibei) {
        hintboxView.SetTitle(text, fontsize, font);
    }

    public virtual void SetContent(string text, int fontsize = 18, FontOption font = FontOption.Weibei) {
        hintboxView.SetContent(text, fontsize, font);
    }

    public virtual void SetOptionCallback(Action callback, bool isConfirm = true) {
        hintboxView.SetOptionCallback(callback, isConfirm);
    }

    public virtual void SetOutline(Color color) {
        hintboxView.SetOutline(color);
    }

}
