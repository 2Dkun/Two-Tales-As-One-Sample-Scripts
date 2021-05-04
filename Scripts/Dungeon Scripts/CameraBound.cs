using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraBound : MonoBehaviour
{
    private CameraManager cam;
    public Boundary[] bounds;

    private void Start()
    { if (!cam) cam = FindObjectOfType<CameraManager>(); }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        // Add new camera bounds
        cam.PushBounds(bounds);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        // Remove from camera bounds
        cam.PopBounds(bounds);
    }
}

[System.Serializable]
public class Boundary {
    public float boundValue;
    public bool isXBound, isMaxBound;
}
