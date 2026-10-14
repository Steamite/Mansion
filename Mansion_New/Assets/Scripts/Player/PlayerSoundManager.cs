using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Player
{
    public class PlayerSoundManager : MonoBehaviour
    {
        AudioSource source;

        public static PlayerSoundManager instance;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            instance = this;
        }

        public void PlayAudio(AudioClip clip)
        {
            source.PlayOneShot(clip);
        }
    }
}
