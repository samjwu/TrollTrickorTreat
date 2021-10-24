using UnityEngine;

namespace Completed
{
    public class House : MonoBehaviour
    {
        public AudioClip playerAttackSound1;
        public AudioClip playerAttackSound2;
        public Sprite damagedImage;

        private SpriteRenderer spriteRenderer;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Set house sprite renderer to toilet paper house image
        /// </summary>
        public void DamageHouse()
        {
            SoundManager.instance.RandomizeSfx(playerAttackSound1, playerAttackSound2);
            spriteRenderer.sprite = damagedImage;
        }
    }
}
