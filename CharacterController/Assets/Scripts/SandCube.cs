using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandCube : MonoBehaviour
{
    [SerializeField] float speed;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            StartCoroutine(BreackUp());
    }

    IEnumerator BreackUp()
    {
        while (transform.localScale.y > 0.1f)
        {
            Vector3 scale = transform.localScale;
            scale.y -= speed * Time.deltaTime;
            transform.localScale = scale;
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
