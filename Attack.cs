using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Attack {
    
    // Public Data
    public string animationTrigger;
    public Hitbox[] sweetspots, basespots;
    public int startup, linger, endlag;
    public int power, mpCost;
    public bool isHeal;
    public Vector2 knockback, pushback, movement;
    public Projectile projectile;
    
    // Functions to be called by other scripts
    public int GetLastFrame() { return startup + linger; }
    public bool IsActive(int curFrame) { return curFrame >= startup && curFrame <= GetLastFrame(); }
    public int GetTotalFrames() { return startup + linger + endlag; }
    public bool InEndlag(int curFrame) { return curFrame >= GetLastFrame(); }
}
