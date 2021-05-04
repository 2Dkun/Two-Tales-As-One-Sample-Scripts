using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof (Text))]
public class FPSDisplay : MonoBehaviour
{
     
    private Text m_Text;
    private float deltaTime;

    private void Start()
    {
        m_Text = GetComponent<Text>();
    }


    private void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        m_Text.text = "FPS: " +  1.0f / deltaTime;
    }
}