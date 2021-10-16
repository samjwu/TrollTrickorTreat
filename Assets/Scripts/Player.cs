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
        public int wallDamage = 1; // damage to wall per attack
        public Text foodText;
        public AudioClip moveSound1;
        public AudioClip moveSound2;
        public AudioClip eatSound1;
        public AudioClip eatSound2;
        public AudioClip drinkSound1;
        public AudioClip drinkSound2;
        public AudioClip gameOverSound;

        private Animator playerAnimator;
        private int foodPoints;
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        private Vector2 touchOrigin = -Vector2.one;	// store location of screen touch origin for mobile controls
#endif

        protected override void Start()
        {
            playerAnimator = GetComponent<Animator>();
            foodPoints = GameManager.instance.playerFoodPoints;
            foodText.text = "Food: " + foodPoints;
            base.Start();
        }

        /// <summary>
        /// When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level
        /// </summary>
        private void OnDisable()
        {
            GameManager.instance.playerFoodPoints = foodPoints;
        }

        private void Update()
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

            //Check if we have a non-zero value for horizontal or vertical
            if (horizontalMove != 0 || verticalMove != 0)
            {
                //Call AttemptMove passing in the generic parameter Wall, since that is what Player may interact with if they encounter one (by attacking it)
                //Pass in horizontal and vertical as parameters to specify the direction to move Player in.
                AttemptMove<Wall>(horizontalMove, verticalMove);
            }
        }

        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            // when the player moves, the food is used up
            foodPoints--;
            foodText.text = "Food: " + foodPoints;

            base.AttemptMove<T>(xDir, yDir);

            RaycastHit2D hit;
            if (Move(xDir, yDir, out hit))
            {
                SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
            }

            CheckIfGameOver();

            GameManager.instance.isPlayerTurn = false;
        }


        //OnCantMove overrides the abstract function OnCantMove in MovingObject.
        //It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
        protected override void OnCannotMove<T>(T component)
        {
            //Set hitWall to equal the component passed in as a parameter.
            Wall hitWall = component as Wall;

            //Call the DamageWall function of the Wall we are hitting.
            hitWall.DamageWall(wallDamage);

            //Set the attack trigger of the player's animation controller in order to play the player's attack animation.
            playerAnimator.SetTrigger("playerChop");
        }


        //OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
        private void OnTriggerEnter2D(Collider2D other)
        {
            //Check if the tag of the trigger collided with is Exit.
            if (other.tag == "Exit")
            {
                //Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
                Invoke("Restart", restartLevelDelay);

                //Disable the player object since level is over.
                enabled = false;
            }

            //Check if the tag of the trigger collided with is Food.
            else if (other.tag == "Food")
            {
                //Add pointsPerFood to the players current food total.
                foodPoints += pointsPerFood;

                //Update foodText to represent current total and notify player that they gained points
                foodText.text = "+" + pointsPerFood + " Food: " + foodPoints;

                //Call the RandomizeSfx function of SoundManager and pass in two eating sounds to choose between to play the eating sound effect.
                SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);

                //Disable the food object the player collided with.
                other.gameObject.SetActive(false);
            }

            //Check if the tag of the trigger collided with is Soda.
            else if (other.tag == "Soda")
            {
                //Add pointsPerSoda to players food points total
                foodPoints += pointsPerSoda;

                //Update foodText to represent current total and notify player that they gained points
                foodText.text = "+" + pointsPerSoda + " Food: " + foodPoints;

                //Call the RandomizeSfx function of SoundManager and pass in two drinking sounds to choose between to play the drinking sound effect.
                SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);

                //Disable the soda object the player collided with.
                other.gameObject.SetActive(false);
            }
        }


        //Restart reloads the scene when called.
        private void Restart()
        {
            //Load the last scene loaded, in this case Main, the only scene in the game. And we load it in "Single" mode so it replace the existing one
            //and not load all the scene object in the current scene.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }


        //LoseFood is called when an enemy attacks the player.
        //It takes a parameter loss which specifies how many points to lose.
        public void LoseFood(int loss)
        {
            //Set the trigger for the player animator to transition to the playerHit animation.
            playerAnimator.SetTrigger("playerHit");

            //Subtract lost food points from the players total.
            foodPoints -= loss;

            //Update the food display with the new total.
            foodText.text = "-" + loss + " Food: " + foodPoints;

            //Check to see if game has ended.
            CheckIfGameOver();
        }


        //CheckIfGameOver checks if the player is out of food points and if so, ends the game.
        private void CheckIfGameOver()
        {
            //Check if food point total is less than or equal to zero.
            if (foodPoints <= 0)
            {
                //Call the PlaySingle function of SoundManager and pass it the gameOverSound as the audio clip to play.
                SoundManager.instance.PlaySingle(gameOverSound);

                //Stop the background music.
                SoundManager.instance.musicSource.Stop();

                //Call the GameOver function of GameManager.
                GameManager.instance.GameOver();
            }
        }
    }
}

