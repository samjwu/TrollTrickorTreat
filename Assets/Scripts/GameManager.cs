using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Completed
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance = null; // to act as singleton

        public float levelStartDelay = 2f; // time to wait before starting level in seconds
        public float turnDelay = 0.1f; // delay between each Player turn in seconds
        public int playerFoodPoints = 100;
        [HideInInspector]
        public bool isPlayerTurn = true;

        private Text levelText;
        private GameObject blockerImage; // block out level as levels are being set up, background for levelText
        private BoardManager boardScript;
        private int currentLevel = 1;
        private List<Enemy> enemies;
        private bool areEnemiesMoving;
        private bool isBoardInSetupMode = true;

        void Awake()
        {
            // enforce singleton pattern (allow only one GameManager instance)
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }

            // do not destroy GameManager when reloading scene
            DontDestroyOnLoad(gameObject);
            enemies = new List<Enemy>();
            boardScript = GetComponent<BoardManager>();
            InitGame();
        }

        //this is called only once, and the paramter tell it to be called only after the scene was loaded
        //(otherwise, our Scene Load callback would be called the very first load, and we don't want that)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static public void CallbackInitialization()
        {
            //register the callback to be called everytime the scene is loaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        //This is called each time a scene is loaded.
        static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            instance.currentLevel++;
            instance.InitGame();
        }


        //Initializes the game for each level.
        void InitGame()
        {
            //While doingSetup is true the player can't move, prevent player from moving while title card is up.
            isBoardInSetupMode = true;

            //Get a reference to our image LevelImage by finding it by name.
            blockerImage = GameObject.Find("LevelImage");

            //Get a reference to our text LevelText's text component by finding it by name and calling GetComponent.
            levelText = GameObject.Find("LevelText").GetComponent<Text>();

            //Set the text of levelText to the string "Day" and append the current level number.
            levelText.text = "Day " + currentLevel;

            //Set levelImage to active blocking player's view of the game board during setup.
            blockerImage.SetActive(true);

            //Call the HideLevelImage function with a delay in seconds of levelStartDelay.
            Invoke("HideLevelImage", levelStartDelay);

            //Clear any Enemy objects in our List to prepare for next level.
            enemies.Clear();

            //Call the SetupScene function of the BoardManager script, pass it current level number.
            boardScript.SetupScene(currentLevel);

        }


        //Hides black image used between levels
        void HideLevelImage()
        {
            //Disable the levelImage gameObject.
            blockerImage.SetActive(false);

            //Set doingSetup to false allowing player to move again.
            isBoardInSetupMode = false;
        }

        //Update is called every frame.
        void Update()
        {
            //Check that playersTurn or enemiesMoving or doingSetup are not currently true.
            if (isPlayerTurn || areEnemiesMoving || isBoardInSetupMode)

                //If any of these are true, return and do not start MoveEnemies.
                return;

            //Start moving enemies.
            StartCoroutine(MoveEnemies());
        }

        //Call this to add the passed in Enemy to the List of Enemy objects.
        public void AddEnemyToList(Enemy script)
        {
            //Add Enemy to List enemies.
            enemies.Add(script);
        }


        //GameOver is called when the player reaches 0 food points
        public void GameOver()
        {
            //Set levelText to display number of levels passed and game over message
            levelText.text = "After " + currentLevel + " days, you starved.";

            //Enable black background image gameObject.
            blockerImage.SetActive(true);

            //Disable this GameManager.
            enabled = false;
        }

        //Coroutine to move enemies in sequence.
        IEnumerator MoveEnemies()
        {
            //While enemiesMoving is true player is unable to move.
            areEnemiesMoving = true;

            //Wait for turnDelay seconds, defaults to .1 (100 ms).
            yield return new WaitForSeconds(turnDelay);

            //If there are no enemies spawned (IE in first level):
            if (enemies.Count == 0)
            {
                //Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
                yield return new WaitForSeconds(turnDelay);
            }

            //Loop through List of Enemy objects.
            for (int i = 0; i < enemies.Count; i++)
            {
                //Call the MoveEnemy function of Enemy at index i in the enemies List.
                enemies[i].MoveEnemy();

                //Wait for Enemy's moveTime before moving next Enemy, 
                yield return new WaitForSeconds(enemies[i].moveTime);
            }
            //Once Enemies are done moving, set playersTurn to true so player can move.
            isPlayerTurn = true;

            //Enemies are done moving, set enemiesMoving to false.
            areEnemiesMoving = false;
        }
    }
}

