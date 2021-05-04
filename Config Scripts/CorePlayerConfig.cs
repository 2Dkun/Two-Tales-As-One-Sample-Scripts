using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CorePlayerData", menuName = "ScriptableObjects/PlayerScriptableObject", order = 1)]
public class CorePlayerConfig : ScriptableObject {

    public float maxHP, maxMP, maxSP;
    
}
