using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationMethods : MonoBehaviour
{
    [SerializeField] ParticleSystem dustGround;

    public void PlaySound(string s)
    {
        AudioManager.instance.Play(s);
        if (!dustGround.isEmitting)
            dustGround.Play();
    }
}