using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioSource VFX;
    [SerializeField] AudioSource A;
    [SerializeField] AudioSource B;

    [SerializeField] AudioMixerSnapshot snapshotA;
    [SerializeField] AudioMixerSnapshot snapshotB;

    [SerializeField] float transitionDuration;

    public static SoundManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void PlayAudio(AudioClip clip)
    {
        //return;
        VFX.PlayOneShot(clip);
    }

    bool transitionA = true;
    public void SetMusic(AudioClip ambientMusic)
    {
        if (transitionA)
        {
            A.clip = ambientMusic;
            snapshotA.TransitionTo(transitionDuration);
        }
        else
        {
            B.clip = ambientMusic;
            snapshotB.TransitionTo(transitionDuration);
        }
        if (!B.isPlaying)
            B.Play();
        if (!A.isPlaying)
            A.Play();

        transitionA = !transitionA;
    }

    public void EnterRoom(AudioClip enterAudio, AudioClip ambientAudio, bool entered)
    {
        if (entered == false)
            PlayAudio(enterAudio);
        SetMusic(ambientAudio);
    }
}
