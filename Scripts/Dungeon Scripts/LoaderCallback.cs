using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used to update the loading screen once
public class LoaderCallback : MonoBehaviour {

    private bool isLoaded;

    // Update is called once per frame
    private void Update() {
        if (!isLoaded) {
            isLoaded = true;
            SceneLoader.LoaderCallback();
        }        
    }
}