using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{
    //[SerializeField] float timeToBreak = 0.001f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //if (other.GetComponent<ThirdPersonController>().velocity.y > 0)
                //Invoke("Break", timeToBreak);
        }
    }

    void Break()
    {
        gameObject.SetActive(false);
    }
}
