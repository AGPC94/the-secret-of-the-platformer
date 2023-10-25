using UnityEngine;

public class InstakillZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("InstakillZone");
            AudioManager.instance.Play("lose");
            SceneLoader.instance.RestartScene();
        }
    }
}