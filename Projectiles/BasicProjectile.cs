using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicProjectile : Projectile {
    public bool targetIsPlayer;
    private bool hitPlayer;

    // Update is called once per frame
    new void Update() {
        base.Update();

        if(!targetIsPlayer){
            // For each enemy, see if they swung and remove the hitbox if they did
            GameObject[] foes = dungeonData.GetComponent<DungeonManager>().enemies;
            for(int i = 0; i < foes.Length; i++){
                // Check if enemy exists
                if(foes[i] != null) {     
                    HitBox foeHurt = new HitBox();  
                    foes[i].SendMessage("GetHurtBox", foeHurt);
					bool isHit = IsHitTarget(hitBox, gameObject, foeHurt, foes[i]);
                    if(isHit){
                        foes[i].SendMessage("Attacked", basePow);
                    }
                }
            }
        }
        else if(!hitPlayer){
            GameObject player = dungeonData.GetComponent<DungeonManager>().player;
            HitBox hurt = player.GetComponent<Player>().hurtBox;

            bool isHit = IsHitTarget(hitBox, gameObject, hurt, player);
			if(isHit){
				player.SendMessage("Attacked", basePow);
				hitPlayer = true;
			}
        }


    }
}
