using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ActivateButtom : MonoBehaviour
{
    public static ActivateButtom Instance;

    const string defaultText = "ACTIVATE";
    public bool Pressed { get; private set; }
    TMP_Text text;
    CanvasGroup me;
    void Awake()
    {
        me = GetComponent<CanvasGroup>();
        text = GetComponentInChildren<TMP_Text>();
        if (Instance!=null)
            Destroy(Instance.gameObject);

        me.alpha = 0;
        Instance = this;
    }

    public void Press() => Pressed = true;
    public void Release() => Pressed = false;

    public void Show()
    {
        text.text = defaultText;
        me.alpha = 1;
    }

    public void Hide()
    {
        text.text = defaultText;
        me.alpha = 0;
    }

    public void Show(string newText)
    {
        text.text = newText;
        me.alpha = 1;
    }
}
