using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string GameScene = "Game";
    [SerializeField] string song = "menu";

    void Start()
    {
        AudioManager.instance.PlayMusic(song);
    }

    public void Play()
    {
        Debug.Log("MainMenu Play)");
        SceneLoader.instance.LoadScene(GameScene);
        AudioManager.instance.Play("button");
    }

    public void Quit()
    {
        Debug.Log("MainMenu Quit()");
        Application.Quit();
    }

}
