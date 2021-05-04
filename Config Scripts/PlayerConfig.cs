using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerScriptableObject", order = 1)]
public class PlayerConfig : ScriptableObject {

    public float walkSpeed, dashSpeedF, dashSpeedB;
    public float airSpeed, airAcceleration;
    public float gravity, jumpForce, maxFallSpeed;
    public int jumpHold, jumpReleaseVel, dashHold;
    
    [Header("Base Attacks")] 
    public Attack basicAttack;
    public Attack downAttack;
    public Attack upAttack;
    public Attack upTilt;
    public Attack special;

    [Header("Link Attacks")] 
    public Attack baseLink;
}
