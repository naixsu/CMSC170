using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameControl : MonoBehaviour
{
    public void Retry()
    {
        //Restarts current level
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Loader.Load(Loader.Scene.GameScene);
    }

    public void Quit()
    {
        /*//Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
        //Standalone Game
        Application.Quit();*/
        Loader.Load(Loader.Scene.EndScene);
    }
}
