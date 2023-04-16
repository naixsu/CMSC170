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
        StartCoroutine(RetryLevel());
        
    }

    public void Quit()
    {
        /*//Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
        //Standalone Game
        Application.Quit();*/
        StartCoroutine(QuitLevel());
        
    }

    IEnumerator QuitLevel()
    {
        yield return new WaitForSeconds(0.1f);
        Loader.Load(Loader.Scene.EndScene);
    }
    IEnumerator RetryLevel()
    {
        yield return new WaitForSeconds(0.1f);
        Loader.Load(Loader.Scene.GameScene);
        
    }
}
