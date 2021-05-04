using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GroundedData", menuName = "ScriptableObjects/Enemies/General/Grounded", order = 1)]
public class GroundedEnemyConfig : EnemyConfig
{
    [Header("Grounded Enemy Variables")] 
    public float gravity;
    public float castDistance = 0.5f;
}