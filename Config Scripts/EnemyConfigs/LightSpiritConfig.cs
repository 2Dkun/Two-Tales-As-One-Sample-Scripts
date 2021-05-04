using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LightSpiritData", menuName = "ScriptableObjects/Enemies/LightSpirit", order = 1)]
public class LightSpiritConfig : AirborneEnemyConfig
{
    [Header("Light Spirit Variables")] 
    public int reactionTime;
    public float flipTolerance, loseDetect;
}
