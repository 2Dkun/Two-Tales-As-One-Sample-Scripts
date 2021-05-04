using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cam : MonoBehaviour
{
     public Transform target;
     public Vector3 shift;
     public float smoothTime = 0.3F;
     private Vector3 velocity = Vector3.zero;

     void Update()
     { 
         if (target)
         {
            Vector3 newPos = Vector2.Lerp (transform.position, target.position + shift, 0.1f);
            newPos.z = -10;
            transform.localPosition = newPos;
         }
     }
}
