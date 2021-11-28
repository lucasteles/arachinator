using UnityEngine;

public enum State
{
    Stop,
    Searching,
    Seeking,
    Shooting,
    Desingage
}

[CreateAssetMenu(fileName = "Enemy Configuration", menuName = "ScriptableObjects/EnemyConfiguration", order = 1)]
public class EnemyConfiguration : ScriptableObject
{
    public State initialState = State.Searching;
    public float speed = 5;
    public float maxLife = 10;
    public float view;
    public float distanceToView = 5;
    public float distanceAroundToSee = 3;
    public float searchStep = 2;
    public float minShootDistance = 4;
    public int maxShoots = 3;
    public float shootCooldownTime = 12;
    public bool shouldShoot = true;
    public GameObject drop;
    public float dropPercentFrom0to1;

    public void InstantiateDrop(Vector3 position, Quaternion rotation)
    {
        if (drop != null && Random.value <= dropPercentFrom0to1)
            Instantiate(drop, new Vector3(position.x, 1f,position.z), rotation);
    }

}

