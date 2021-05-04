using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WalkerSpiritData", menuName = "ScriptableObjects/Enemies/WalkerSpirit", order = 1)]
public class WalkerSpiritConfig : GroundedEnemyConfig
{
    [Header("Walker Spirit Variables")] 
    public Vector2 detectionShift;
    public float chargeSpeed, chargeCastLength;
    public int startup, endlag;
}
