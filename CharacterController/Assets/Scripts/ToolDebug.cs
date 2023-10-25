using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolDebug : MonoBehaviour
{
    public static ToolDebug instance;

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

    // Update is called once per frame
    void Update()
    {
        GoToCheckPoint();
        SelectLevel();
    }

    void SelectLevel()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            GameManager.instance.ResetCheckPoint();
            SceneLoader.instance.LoadScene("Level1");
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            GameManager.instance.ResetCheckPoint();
            SceneLoader.instance.LoadScene("Level2");
        }
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            GameManager.instance.ResetCheckPoint();
            SceneLoader.instance.LoadScene("Level3");
        }
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            GameManager.instance.ResetCheckPoint();
            SceneLoader.instance.LoadScene("Level4");
        }
    }

    void GoToCheckPoint()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            ThirdPersonController player = FindObjectOfType<ThirdPersonController>();
            CheckPoint cp = FindObjectOfType<CheckPoint>();

            if (cp == null || player == null)
                return;

            player.GetComponent<CharacterController>().enabled = false;
            player.transform.position = cp.transform.position;
            player.GetComponent<CharacterController>().enabled = true;
        }
    }
}
