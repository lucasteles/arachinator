using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeathCount : MonoBehaviour
{
    [SerializeField] TMP_Text text;

    int deathCount;
    Life playerLife;

    void Awake()
    {
        playerLife = FindObjectOfType<Player>().GetComponent<Life>();
        playerLife.onDeath += onPlayerDeath;
    }

    void OnDestroy() =>
        playerLife.onDeath -= onPlayerDeath;

    void onPlayerDeath(Life obj)
    {
        deathCount++;
        UpdateText();
    }

    void UpdateText() =>
        text.text = deathCount.ToString();

    void Start() => UpdateText();




}
