using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    bool isOn;

    void Awake()
    {
        if (GameManager.instance.IsTheSameCheckPoint(transform.position))
        {
            isOn = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isOn)
            {
                Debug.Log("Checkpointed");
                isOn = true;
                GameManager.instance.UpdateCheckPoint(transform.position);
                AudioManager.instance.Play("checkpoint");
                foreach (Transform child in transform)
                    child.GetComponent<ParticleSystem>().Play();
            }
        }
    }
}
