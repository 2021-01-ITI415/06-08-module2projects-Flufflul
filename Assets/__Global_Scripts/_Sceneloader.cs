using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class _Sceneloader : MonoBehaviour
{
    public void LoadScene_Prospector() { SceneManager.LoadScene("Prospector_Scene_0"); }

    public void LoadScene_Golf() { SceneManager.LoadScene("Golf_Scene_0"); }
}