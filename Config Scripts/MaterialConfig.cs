using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MaterialData", menuName = "ScriptableObjects/MaterialScriptableObject", order = 1)]
public class MaterialConfig : ScriptableObject {
    public Material hitlagShader;
    public Material spiritShader;
    public Material blinkShader;
}