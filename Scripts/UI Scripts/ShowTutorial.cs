using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowTutorial : MonoBehaviour
{
    private Animator anim;
    
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        throw new NotImplementedException();
        // TODO: Tell animator to show text (bool = true)
    }

    private void OnTriggerExit(Collider other)
    {
        throw new NotImplementedException();
        // TODO: Tell animator to hide text (bool = false)
    }
}