using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] CanvasGroup pauseWindow;
    [SerializeField] float delayTime = .5f;
    public bool IsPaused;
    public static PauseMenu instance;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        ToggleCursor();
    }

    void ToggleCursor()
    {
        ThirdPersonController player = FindObjectOfType<ThirdPersonController>();

        if (player != null)
        {
            if (IsPaused)
                ShowCursor();
            else
                HideCursor();
        }
        else
            ShowCursor();
    }

    void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    //Call this method in the character controller with an Input a if it can move
    public void TogglePause()
    {
        if (IsPaused)
            Resume();
        else
            Pause();
    }

    public void Resume()
    {
        AudioManager.instance.Play("button");
        StartCoroutine(DelayResume());

        IEnumerator DelayResume()
        {
            print("PauseMenu Resume");
            //HideCursor();
            pauseWindow.interactable = false;
            pauseWindow.DOFade(0, delayTime).SetUpdate(true);
            yield return new WaitForSecondsRealtime(delayTime);
            Time.timeScale = 1;
            IsPaused = false;
        }
    }


    public void Pause()
    {
        AudioManager.instance.Play("button");
        print("PauseMenu Pause");
        //ShowCursor();
        pauseWindow.interactable = true;
        pauseWindow.DOFade(1, delayTime).SetUpdate(true);
        Time.timeScale = 0;
        IsPaused = true;
    }

    public void Quit()
    {
        AudioManager.instance.Play("button");
        print("PauseMenu Quit");
        pauseWindow.interactable = false;
        pauseWindow.alpha = 0;
        Time.timeScale = 1;
        IsPaused = false;
        SceneLoader.instance.LoadScene("MainMenu");
    }
}