using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Completed
{
    public class Player : MovingObject
    {
        public float restartLevelDelay = 1f; // delay time in seconds to restart level
        public int pointsPerFood = 10;
        public int pointsPerSoda = 20;
        public int pointsPerHouse = 100;
        public int wallDamage = 1; // damage to wall per attack
        public Text foodText;
        public Text paperText;
        public AudioClip moveSound1;
        public AudioClip moveSound2;
        public AudioClip eatSound1;
        public AudioClip eatSound2;
        public AudioClip drinkSound1;
        public AudioClip drinkSound2;
        public AudioClip gameOverSound;

        private Animator playerAnimator;
        private int foodPoints;
        private int paperCount;
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        private Vector2 touchOrigin = -Vector2.one;	// store location of screen touch origin for mobile controls
#endif

        protected override void Start()
        {
            playerAnimator = GetComponent<Animator>();
            foodPoints = GameManager.instance.playerFoodPoints;
            foodText.text = "Candy: " + foodPoints;
            paperCount = GameManager.instance.playerPaperCount;
            paperText.text = "Toilet Paper: " + paperCount;
            base.Start();
        }

        /// <summary>
        /// When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level
        /// </summary>
        void OnDisable()
        {
            GameManager.instance.playerFoodPoints = foodPoints;
            GameManager.instance.playerPaperCount = paperCount;
        }

        /// <summary>
        /// Get input and try to move the player
        /// </summary>
        void Update()
        {
            // do nothing if it's not the player's turn
            if (!GameManager.instance.isPlayerTurn)
            {
                return;
            }

            int horizontalMove = 0;
            int verticalMove = 0;

#if UNITY_STANDALONE || UNITY_WEBPLAYER
            horizontalMove = (int)Input.GetAxisRaw("Horizontal");
            verticalMove = (int)Input.GetAxisRaw("Vertical");

            // if moving horizontally, set vertical movement to zero
            if (horizontalMove != 0)
            {
                verticalMove = 0;
            }

#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
			// check if Input has registered any touches
			if (Input.touchCount > 0)
			{
				// store the first touch detected.
				Touch myTouch = Input.touches[0];
				
                // if first touch has begun (finger touched screen), record the starting position of the touch
				if (myTouch.phase == TouchPhase.Began)
				{
					touchOrigin = myTouch.position;
				}
                // if touch is over (finger lifted), calculate change in touch position (drag)
				else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
				{
					// get end position of touch
					Vector2 touchEnd = myTouch.position;
					
					float touchDx = touchEnd.x - touchOrigin.x;
					float touchDy = touchEnd.y - touchOrigin.y;
					
					// set touchOrigin.x to -1 to avoid repeating this else if statement
					touchOrigin.x = -1;
					
                    // if change in x position is greater than change in y position, move horizontally
					if (Mathf.Abs(touchDx) > Mathf.Abs(touchDy))
                    {
						horizontal = touchDx > 0 ? 1 : -1;
                    }
                    // else move vertically
					else
                    {
						vertical = touchDy > 0 ? 1 : -1;
                    }
				}
			}	
#endif

            if (horizontalMove != 0 || verticalMove != 0)
            {
                AttemptMove<Wall>(horizontalMove, verticalMove);
            }
        }

        /// <summary>
        /// Move the player or have player attack a blocking wall
        /// </summary>
        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            // when the player moves, the food is used up
            foodPoints--;

            base.AttemptMove<T>(xDir, yDir);

            RaycastHit2D hit;

            if (Move(xDir, yDir, out hit))
            {
                SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
            }

            foodText.text = "Candy: " + foodPoints;
            paperText.text = "Toilet Paper: " + paperCount;

            CheckIfGameOver();

            GameManager.instance.isPlayerTurn = false;
        }

        /// <summary>
        /// Have player attack a wall if it is blocking
        /// </summary>
        protected override void OnCannotMove<T>(T component)
        {
            Wall hitWall = component as Wall;
            hitWall.DamageWall(wallDamage);
            playerAnimator.SetTrigger("playerAttack");
        }

        /// <summary>
        /// Logic for player interactions with items
        /// </summary>
        void OnTriggerEnter2D(Collider2D other)
        {
            // if player reaches exit, go to next level
            if (other.tag == "Exit")
            {
                Invoke("Restart", restartLevelDelay);

                // disable the player object since level is over
                enabled = false;
            }
            // if player reaches candy, add food points
            else if (other.tag == "Candy")
            {
                foodPoints += pointsPerFood;
                foodText.text = "+" + pointsPerFood + " Candy: " + foodPoints;

                SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);

                // disable the object the player collided with
                other.gameObject.SetActive(false);
            }
            else if (other.tag == "Toilet Paper")
            {
                paperCount++;
                paperText.text = "+1 Toilet Paper: " + paperCount;

                SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);

                // disable the object the player collided with
                other.gameObject.SetActive(false);
            }
            else if (other.tag == "House")
            {
                if (paperCount > 0)
                {
                    paperCount--;
                    paperText.text = "-1 Toilet Paper: " + paperCount;

                    foodPoints += pointsPerHouse;
                    foodText.text = "+" + pointsPerHouse + " Candy: " + foodPoints;

                    House hitHouse = other.gameObject.GetComponent<House>();
                    hitHouse.DamageHouse();
                    playerAnimator.SetTrigger("playerAttack");

                    other.tag = "Untagged";
                }
            }
        }

        /// <summary>
        /// Reload the scene to start a new level
        /// </summary>
        void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }

        /// <summary>
        /// Reduce player's food points by loss value
        /// </summary>
        public void LoseFood(int loss)
        {
            playerAnimator.SetTrigger("playerHit");

            foodPoints -= loss;
            foodText.text = "-" + loss + " Food: " + foodPoints;

            CheckIfGameOver();
        }

        /// <summary>
        /// End the game if player has no more food
        /// </summary>
        void CheckIfGameOver()
        {
            if (foodPoints <= 0)
            {
                SoundManager.instance.PlaySingle(gameOverSound);

                // stop the background music
                SoundManager.instance.musicSource.Stop();

                GameManager.instance.GameOver();
            }
        }
    }
}

