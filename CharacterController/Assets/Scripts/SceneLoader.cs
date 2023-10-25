using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;
    [SerializeField] float transitionTime;
    CanvasGroup loadingScreen;

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

        loadingScreen = GetComponent<CanvasGroup>();
    }

    public void LoadScene(string scene)
    {
        StartCoroutine(LoadAsynchronously(scene));
    }

    public void RestartScene()
    {
        string scene = SceneManager.GetActiveScene().name;
        StartCoroutine(LoadAsynchronously(scene));
    }

    public void NextScene()
    {
        int scene = SceneManager.GetActiveScene().buildIndex + 1;
        StartCoroutine(LoadAsynchronously(scene));
    }

    IEnumerator LoadAsynchronously(string scene)
    {
        //In animation
        yield return StartCoroutine(TweenIn());

        //Loading Screen
        AsyncOperation operation = SceneManager.LoadSceneAsync(scene);
        yield return new WaitUntil(() => operation.isDone);

        //Out Animation
        yield return StartCoroutine(TweenOut());
    }
    IEnumerator LoadAsynchronously(int scene)
    {
        //In animation
        yield return StartCoroutine(TweenIn());

        //Loading Screen
        AsyncOperation operation = SceneManager.LoadSceneAsync(scene);
        yield return new WaitUntil(() => operation.isDone);

        //Out Animation
        yield return StartCoroutine(TweenOut());
    }

    IEnumerator TweenIn()
    {
        Time.timeScale = 0;
        Tween tween = loadingScreen.DOFade(1, transitionTime).SetUpdate(true);
        yield return tween.WaitForCompletion();
    }

    IEnumerator TweenOut()
    {
        Tween tween = loadingScreen.DOFade(0, transitionTime).SetUpdate(true);
        yield return tween.WaitForCompletion();
        Time.timeScale = 1;
    }
}
