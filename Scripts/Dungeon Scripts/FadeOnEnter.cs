using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOnEnter : MonoBehaviour
{
    public Animator fadeObj;
    private bool fade;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        fade = true;
        StartCoroutine(UpdateAnimator(true));
        //fadeObj.SetBool("fade", true);
        //print("test: " + Time.frameCount);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        fade = false;
        StartCoroutine(UpdateAnimator(false));
        //fadeObj.SetBool("fade", false);
        //print("test2: " + Time.frameCount);
    }
    
    IEnumerator UpdateAnimator(bool change)
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if(change == fade) fadeObj.SetBool("fade", fade);
        }
    }
}
