using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public ThirdPersonController player;
    public Vector3 lastCheckPointPosition;
    public static GameManager instance;

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

        Application.targetFrameRate = 60;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded");

        player = FindObjectOfType<ThirdPersonController>();

        if (player == null)
            return;

        SpawnCheckPoint();
    }

    #region CheckPoint

    public bool IsTheSameCheckPoint(Vector3 position)
    {
        return lastCheckPointPosition.Equals(position);
    }

    public void SpawnCheckPoint()
    {
        if (lastCheckPointPosition == Vector3.zero)
            return;

        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = lastCheckPointPosition;
        player.GetComponent<CharacterController>().enabled = true;
    }

    public void ResetCheckPoint()
    {
        UpdateCheckPoint(Vector3.zero);
    }

    public void UpdateCheckPoint(Vector3 checkPointPosition)
    {
        lastCheckPointPosition = checkPointPosition;
    }

    #endregion

}
