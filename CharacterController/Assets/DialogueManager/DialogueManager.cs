using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DialogueManager : MonoBehaviour
{
    [SerializeField] Text txtName;
    [SerializeField] Text txtDialogue;
    [SerializeField] Animator animator;

    Queue<string> sentences;

    public bool isOpen;
    public static DialogueManager instance;

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

    void Start()
    {
        sentences = new Queue<string>();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        Debug.Log("Starting dialogue with " + dialogue.name);

        if (isOpen)
            return;

        sentences.Clear();

        isOpen = true;

        animator.SetBool("isOpen", isOpen);

        txtName.text = dialogue.name;

        foreach (string sentence in dialogue.sentences)
            sentences.Enqueue(sentence);

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentences(sentence));
    }

    IEnumerator TypeSentences(string sentence)
    {
        txtDialogue.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            txtDialogue.text += letter;
            yield return null;
        }
    }

    void EndDialogue()
    {
        isOpen = false;
        animator.SetBool("isOpen", isOpen);
        Debug.Log("End conversation");
    }

}
