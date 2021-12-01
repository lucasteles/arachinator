using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SwitchText : MonoBehaviour
{
    void Awake()
    {
        var text = GetComponentInChildren<Text>();

        if (Environment.IsMobile)
            text.text = "ACTIVATE to open the gate";
        else
            text.text = "Press [E] to open the gate";
    }

    public void Show()
    {
        GetComponent<Canvas>().enabled = true;
        if (Environment.IsMobile)
            ActivateButtom.Instance.Show();
    }
    
    public void Hide()
    {
        GetComponent<Canvas>().enabled = false;
        if (Environment.IsMobile)
            ActivateButtom.Instance.Hide();
    }
}
