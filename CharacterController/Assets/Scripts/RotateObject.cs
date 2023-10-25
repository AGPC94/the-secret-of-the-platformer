using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] Vector3 direction;
    [SerializeField] float speed;

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(direction * speed * Time.deltaTime);
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ThirdPersonController player = other.GetComponent<ThirdPersonController>();

            if (player.IsGrounded || player.IsInLedge)
            {
                player.transform.SetParent(transform);
                player.transform.eulerAngles = new Vector3(0, other.transform.eulerAngles.y, 0);
            }
            else
            {
                player.transform.SetParent(null);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null);
        }
    }
}
