﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightShield : Projectile {
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    new void Update() {
        base.Update();

        // For each enemy, see if they swung and remove the hitbox if they did
        GameObject[] foes = dungeonData.GetComponent<DungeonManager>().enemies;
        for(int i = 0; i < foes.Length; i++){
            // Check if enemy exists
			if(foes[i] != null) {
				HitBox foeHit = new HitBox(); 
				foes[i].SendMessage("GetCurAtk", foeHit);
				if(!foeHit.IsEqual(new HitBox())){
					if(base.IsHitTarget(hitBox, gameObject, foeHit, foes[i])){
						foes[i].SendMessage("RmvCurAtk");
						Debug.Log("yoterson");
					}
				}
			}
        }
    }


}
