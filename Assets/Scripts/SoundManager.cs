using UnityEngine;

namespace Completed
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager instance = null;

        public AudioSource sfxSource;
        public AudioSource musicSource;
        public float lowestPitch = 0.95f; // lowest pitch for sound effect
        public float highestPitch = 1.05f; // highest pitch for sound effect

        void Awake()
        {
            // enforce singleton pattern (only one instance)
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }

            // do not destroy when reloading scene
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Play a single sound clip
        /// </summary>
        /// <param name="clip"></param>
        public void PlaySingle(AudioClip clip)
        {
            sfxSource.clip = clip;
            sfxSource.Play();
        }

        /// <summary>
        /// Choose a random sound clip and a random pitch for the clip to play at
        /// </summary>
        /// <param name="clips"></param>
        public void RandomizeSfx(params AudioClip[] clips)
        {
            int randomIndex = Random.Range(0, clips.Length);
            float randomPitch = Random.Range(lowestPitch, highestPitch);
            sfxSource.pitch = randomPitch;
            sfxSource.clip = clips[randomIndex];
            sfxSource.Play();
        }
    }
}
