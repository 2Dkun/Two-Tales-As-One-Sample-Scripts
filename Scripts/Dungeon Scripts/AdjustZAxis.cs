using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustZAxis : MonoBehaviour
{
    public float zEnter, zExit, changeSpeed;
    public GameObject background;
    private float newZ;
    private bool isSet;

    private void Update()
    {
        if (isSet)
        {
            var curPos = background.transform.position;
            var newPos = curPos; newPos.z = newZ;
            
            if (Mathf.Abs(curPos.z - newPos.z) < 0.01f)
            {
                isSet = false;
                background.transform.position = newPos;
            }
            else
            {
                //background.transform.position = Vector3.Lerp(curPos, newPos, changeSpeed * Time.deltaTime);

                background.transform.position = Vector3.MoveTowards(curPos, newPos,changeSpeed * Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        newZ = -zEnter;
        isSet = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        newZ = -zExit;
        isSet = true;
    }
}
