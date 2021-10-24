using UnityEngine;

namespace Completed
{
    public class Wall : MonoBehaviour
    {
        public AudioClip playerAttackSound1;
        public AudioClip playerAttackSound2;
        public Sprite damagedImage;
        public int hp = 2; // wall hitpoints/health points

        private SpriteRenderer spriteRenderer;

        void Awake()
        {
            //Get a component reference to the SpriteRenderer.
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Reduce wall health by loss. Disable wall if its hp is less than 1
        /// </summary>
        public void DamageWall(int loss)
        {
            SoundManager.instance.RandomizeSfx(playerAttackSound1, playerAttackSound2);
            spriteRenderer.sprite = damagedImage;
            hp -= loss;

            if (hp <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
