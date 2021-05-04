using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LinkUser { Shield, Sword, Shadow }

public class Link : MonoBehaviour {
    
     /*
      * Spirit Link Notes:
      * - hold data of attack 
      * - must be activated on command(public function)
      * - let playermanager know when the attack is done
      * - have a special animator controller with default state as nothing
      *     - branching animations that return to default
      * - 
      */

     private enum LinkState { Completed, Set, Activated }
     private LinkState curState;
     
     public GameObject icon;
     // Object that holds the sprite of each character/user
     [Header("Please set Shield, Sword, then Shadow")]
     public GameObject[] linkers;
     
     private int order;
     private Attack attack;
     private LinkUser user;
     private Animator[] _animator;
     private readonly Timer timer = new Timer();

     private void Awake() {
         icon.SetActive(false);
         _animator = new Animator[linkers.Length];
         for (int i = 0; i < linkers.Length; i++) {
             _animator[i] = linkers[i].GetComponent<Animator>();
             linkers[i].SetActive(false);
         }
     }

     // Defines the user and attack of the link
     public void SetAttack(Attack a, LinkUser l) {
         user = l;
         attack = a;
         timer.ResetTimer();
         curState = LinkState.Set;
         //icon.SetActive(true);
     }

     // Begins the attack of the link
     public void ActivateLink() {
         if (curState != LinkState.Set) return;
         
         //icon.SetActive(false);
         curState = LinkState.Activated;
         linkers[(int)user].SetActive(true);
         _animator[(int)user].SetTrigger(attack.animationTrigger);
     }

     // Allows for the attack to be done if initiated
     public void UpdateLink(BaseEnemy[] targets) {
         if (curState != LinkState.Activated) return;
         // See if the attack has ended
         if (timer.WaitForXFrames(attack.GetTotalFrames())) {
             curState = LinkState.Completed;
             linkers[(int)user].SetActive(false);
             return;
         }

         // Check if the attack connected during its active frames
         if (!attack.IsActive(timer.CurFrame())) return;
         Vector2 kb = attack.knockback;
         kb.x *= Mathf.Sign(transform.localScale.x);
         for (int i = 0; i < targets.Length; i++) {
             var tarHurt = targets[i].GetHurtBox();
             var isHit = BaseEntity.IsHitTarget(attack.basespots, gameObject, tarHurt, targets[i].gameObject);
             // Tell the target that it has been attacked
             if (isHit) { targets[i].LinkAttacked(attack.power, kb, false); }
         }
         
     }

     public bool IsCompleted() { return curState == LinkState.Completed; }
}
