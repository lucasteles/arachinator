using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enviroment : MonoBehaviour
{
    [SerializeField] GameObject[] enableOnMobile;

    public static bool IsMobile =>
         Application.platform == RuntimePlatform.Android ||
         Application.platform == RuntimePlatform.IPhonePlayer;

    void Awake()
    {
        if (IsMobile)
        {
            foreach (var item in enableOnMobile)
                item.SetActive(true);
        }
        else
        {
            foreach (var item in enableOnMobile)
                item.SetActive(false);
        }
    }
}
