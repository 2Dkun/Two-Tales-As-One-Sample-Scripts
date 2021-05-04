using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConstantData", menuName = "ScriptableObjects/ConstantScriptableObject", order = 1)]
public class ConstantConfig : ScriptableObject
{
    [Header("Variables for system")] 
    public int hurtTime;
    public int hurtIframes;
    public Vector2 hurtKnockback, hurtDeceleration;
    public float critMultiplier;
    public float jumpChanceTime;
    public float enemyActionRange;
    public Vector2 centerPlayerShift;
    
    [Header("Variables for spirit movement")] 
    public float spiritSmoothTime;
    public float spiritXPos, spiritYPos;
    public float spiritRecallTime;
    
    [Header("Variables for player movement")] 
    public float tapTime;
    public float dashTime;
    public float dashCooldown;
    public float swapForce;
}
