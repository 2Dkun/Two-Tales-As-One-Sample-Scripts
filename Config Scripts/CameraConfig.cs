using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraData", menuName = "ScriptableObjects/CameraScriptableObject", order = 1)]
public class CameraConfig : ScriptableObject
{
    public Vector3 playerBias;
    public float slowLerp, fastLerp;
    public float riseLerp, fallLerp;
}