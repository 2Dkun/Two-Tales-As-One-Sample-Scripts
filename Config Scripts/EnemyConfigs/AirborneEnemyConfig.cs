using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AirborneEnemyConfig : EnemyConfig
{
    [Header("Airborne Enemy Variables")] 
    public float waypointDistance = 0.25f;
}
