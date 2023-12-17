using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputHintbox : Hintbox
{
    [SerializeField] protected InputHintboxView inputHintboxView;

    public override void SetOptionNum(int num)
    {
        base.SetOptionNum(num);
        inputHintboxView.SetOptionNum(num);
    }

    public virtual void SetOptionCallback(Action<string> callback, bool isConfirm = true) {
        inputHintboxView.SetOptionCallback(callback, isConfirm);
    }

    public virtual void SetNote(string text, int fontsize = 16, FontOption font = FontOption.Weibei) {
        inputHintboxView.SetNote(text, fontsize, font);
    }

    public virtual void SetInputField(int charLimit, InputField.ContentType contentType = InputField.ContentType.Standard) {
        inputHintboxView.SetInputField(charLimit, contentType);
    }
}
