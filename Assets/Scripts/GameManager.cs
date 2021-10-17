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
        private GameObject levelImage; // image to hide levels as they are being set up, background for levelText
        private BoardManager boardManager;
        private int currentLevel = 1;
        private List<Enemy> enemies;
        private bool areEnemiesMoving;
        private bool isBoardInSetupMode = true; // if true, prevent player from moving

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
            boardManager = GetComponent<BoardManager>();
            InitGame();
        }

        /// <summary>
        /// Run the OnSceneLoaded callback function after a scene is loaded
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static public void CallbackInitialization()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <summary>
        /// Increase level counter and set up the new level
        /// </summary>
        static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            instance.currentLevel++;
            instance.InitGame();
        }

        /// <summary>
        /// Set up game for each new level
        /// </summary>
        void InitGame()
        {
            // use isBoardInSetupMode to prevent player from moving in loading mode
            isBoardInSetupMode = true;

            levelImage = GameObject.Find("LevelImage");
            levelText = GameObject.Find("LevelText").GetComponent<Text>();
            levelText.text = "Block " + currentLevel;
            levelImage.SetActive(true);

            Invoke("HideLevelImage", levelStartDelay);

            enemies.Clear();

            boardManager.SetupScene(currentLevel);
        }

        /// <summary>
        /// Remove the level hiding image from view
        /// </summary>
        void HideLevelImage()
        {
            levelImage.SetActive(false);

            // allow player to move again
            isBoardInSetupMode = false;
        }

        /// <summary>
        /// Logic to handle the enemy movement
        /// </summary>
        void Update()
        {
            // do not move enemies on player's turn, if enemies are already moving, or if in setup mode
            if (isPlayerTurn || areEnemiesMoving || isBoardInSetupMode)
            {
                return;
            }

            StartCoroutine(MoveEnemies());
        }

        /// <summary>
        /// Add an enemy to the list of tracked enemies
        /// </summary>
        /// <param name="script"></param>
        public void AddEnemyToList(Enemy script)
        {
            enemies.Add(script);
        }

        /// <summary>
        /// End the game when player has no food
        /// </summary>
        public void GameOver()
        {
            levelText.text = "After travelling " + currentLevel + " blocks, you starved.";

            levelImage.SetActive(true);

            //Disable this GameManager.
            enabled = false;
        }

        /// <summary>
        /// Coroutine for moving enemies logic
        /// </summary>
        IEnumerator MoveEnemies()
        {
            // player cannot move while enemies move
            areEnemiesMoving = true;

            // wait for turnDelay seconds
            yield return new WaitForSeconds(turnDelay);

            // even if there no enemies, perform a wait to simulate delay when there are enemies
            if (enemies.Count == 0)
            {
                yield return new WaitForSeconds(turnDelay);
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].MoveEnemy();
                // wait for enemy's moveTime before moving the next enemy
                yield return new WaitForSeconds(enemies[i].moveTime);
            }

            // after enemies finish moving, allow player to move
            isPlayerTurn = true;

            areEnemiesMoving = false;
        }
    }
}

