using UnityEngine;

[System.Serializable]
public class AttackData
{
    [Header("Basic Settings")]
    public string attackName;
    public float cost;
    public float cooldown;
    public float damage;

    [Header("Targeting")]
    public AttackTargetType targetType;

    [Header("Visuals")]
    public GameObject vfxPrefab;
    public float vfxScale = 1f;
    public float vfxDuration = 1f;

    [Header("Audio")]
    public AudioClip castSound;

    // Constructor
    public AttackData(
        string attackName,
        float cost,
        float cooldown,
        float damage,
        AttackTargetType targetType
    )
    {
        this.attackName = attackName;
        this.cost = cost;
        this.cooldown = cooldown;
        this.damage = damage;
        this.targetType = targetType;
    }
}
