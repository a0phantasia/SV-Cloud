using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputHintboxView : IMonoBehaviour
{
    [SerializeField] protected int optionNum;
    [SerializeField] protected IInputField inputField;
    [SerializeField] protected Text note;
    [SerializeField] protected IButton confirmButtonSingle, confirmButton, cancelButton;

    public string InputString => inputField.InputString;

    public void SetOptionNum(int num) {
        if (!num.IsWithin(0, 2))
            return; 
        optionNum = num;
    }

    public void SetOptionCallback(Action<string> callback, bool isConfirm = true) {
        if (callback == null)
            return;

        IButton button = (optionNum == 1) ? confirmButtonSingle : (isConfirm ? confirmButton : cancelButton);
        Action confirmCallback = () => { 
            callback?.Invoke(inputField.InputString);
        };
        button.onPointerClickEvent.AddListener(confirmCallback.Invoke);
    }

    public void SetNote(string text, int fontsize, FontOption font) {
        note.text = text;
        note.fontSize = fontsize;
        Font f = ResourceManager.instance.GetFont(font);
        if (f == null)
            return;

        note.font = f;
    }    

    public void SetInputField(int charLimit, InputField.ContentType contentType) {
        inputField.inputField.characterLimit = charLimit;
        inputField.inputField.contentType = contentType;
    }
    
}
