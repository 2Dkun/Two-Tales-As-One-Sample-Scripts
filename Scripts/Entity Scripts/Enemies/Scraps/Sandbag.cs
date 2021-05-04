using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sandbag : BaseEnemy {
    
    protected new void Start() {
        base.Start();
        curHP = 100;
    }

    public new void UpdateEntity(){
        base.UpdateEntity();
    }
    
}
