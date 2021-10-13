using UnityEngine;

namespace Completed
{
    /// <summary>
    /// Create the GameManager and SoundManager instances
    /// </summary>
    public class Loader : MonoBehaviour
    {
        public GameObject gameManager;
        public GameObject soundManager;

        void Awake()
        {
            if (GameManager.instance == null)
            {
                Instantiate(gameManager);
            }

            if (SoundManager.instance == null)
            {
                Instantiate(soundManager);
            }
        }
    }
}