using System;
using TMPro;
using UnityEngine;

public class Counter : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    
    Life playerLife;
    public int deathCount;
    float totalTime;

    public TimeSpan TotalTime => TimeSpan.FromSeconds(totalTime);

    void Update () => totalTime += Time.deltaTime;

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