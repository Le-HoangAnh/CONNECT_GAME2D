using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Connect.Core
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                return;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        [SerializeField] private AudioSource _effectSource;

        public void PlaySound(AudioClip clip)
        {
            _effectSource.PlayOneShot(clip);
        }
    }
}
