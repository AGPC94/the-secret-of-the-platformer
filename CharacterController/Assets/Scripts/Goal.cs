using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.ResetCheckPoint();
            AudioManager.instance.Play("win");
            SceneLoader.instance.NextScene();
        }
    }
}
