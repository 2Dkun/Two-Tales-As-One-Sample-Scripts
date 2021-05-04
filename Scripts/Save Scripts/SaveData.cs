using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData {

    // Video settings
    //private bool particlesOn;

    // Location variables
    public string sceneName;
    public float xPos, yPos;
    public bool isShield = true;
    
    // Checkpoints/Flags
    public HashSet<string> flags;

    public SaveData()
    {
        flags = new HashSet<string>();
    }

    public void ClearData()
    {
        flags.Clear();
    }
}