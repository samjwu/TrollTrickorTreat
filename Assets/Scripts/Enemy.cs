using UnityEngine;

namespace Completed
{
    public class Enemy : MovingObject
    {
        public int playerDamage; // damage to player
        public AudioClip attackSound1;
        public AudioClip attackSound2;

        private Animator enemyAnimator;
        private Transform playerLocation;
        private bool skipMove; // skip enemy move if true

        protected override void Start()
        {
            GameManager.instance.AddEnemyToList(this);
            enemyAnimator = GetComponent<Animator>();
            playerLocation = GameObject.FindGameObjectWithTag("Player").transform;
            base.Start();
        }

        /// <summary>
        /// Move the enemy or wait a turn
        /// </summary>
        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            if (skipMove)
            {
                skipMove = false;
                return;
            }

            base.AttemptMove<T>(xDir, yDir);
            skipMove = true;
        }

        /// <summary>
        /// Move enemy towards player location
        /// </summary>
        public void MoveEnemy()
        {
            int xDir = 0;
            int yDir = 0;

            // if player x position is the same as the enemy, move in y direction
            if (Mathf.Abs(playerLocation.position.x - transform.position.x) < float.Epsilon)
            {
                yDir = playerLocation.position.y > transform.position.y ? 1 : -1;
            }
            // else move in x direction
            else
            {
                xDir = playerLocation.position.x > transform.position.x ? 1 : -1;
            }
            
            AttemptMove<Player>(xDir, yDir);
        }

        /// <summary>
        /// Logic for enemy attacking the player
        /// </summary>
        protected override void OnCannotMove<T>(T component)
        {
            Player hitPlayer = component as Player;
            hitPlayer.LoseFood(playerDamage + GameManager.instance.currentLevel - 1);
            enemyAnimator.SetTrigger("enemyAttack");
            SoundManager.instance.RandomizeSfx(attackSound1, attackSound2);
        }
    }
}
