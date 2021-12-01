using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideIfNotDesktop : MonoBehaviour
{
    void Awake()
    {
        if (Environment.IsMobile)
            gameObject.SetActive(false);
    }
}
