using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyConfig : ScriptableObject
{
    [Header("Base Enemy Variables")] 
    public int maxHP;
    public float detectionRadius;
    public float moveSpeed;
}
