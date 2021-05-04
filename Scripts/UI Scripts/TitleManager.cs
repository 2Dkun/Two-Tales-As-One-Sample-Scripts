using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : MonoBehaviour
{

    public void NewGame()
    {
        SceneLoader.Load(SceneLoader.Scene.Rooftops);
    }

    public void QuitGame()
    {
        DataManager.SaveOptions();
        Application.Quit();
    }
}
