using System;
using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;

namespace Completed
{
    /// <summary>
    /// Sets up board tiles for the level (borders, floors, walls, items, enemies)
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        /// <summary>
        /// Range defines the lower and upper limit for RNG
        /// </summary>
        [Serializable]
        public class Range
        {
            public int minimum;
            public int maximum;

            public Range(int min, int max)
            {
                minimum = min;
                maximum = max;
            }
        }

        public int boardRows = 8;
        public int boardColumns = 8;
        public Range wallRange = new Range(5, 9);
        public Range foodRange = new Range(1, 5);
        public GameObject exit;
        public GameObject[] floorTiles;
        public GameObject[] wallTiles;
        public GameObject[] borderTiles;
        public GameObject[] foodTiles;
        public GameObject[] enemyTiles;

        private Transform board;
        private List<Vector3> randomPositions = new List<Vector3>(); // coordinates for special tiles (potential walls/items/enemies)

        /// <summary>
        /// Generate border and floor tiles for the board
        /// </summary>
        void InitializeBoard()
        {
            board = new GameObject("Board").transform;

            // borders corners located at -1 and maxRow/Col
            for (int x = -1; x < boardColumns + 1; x++)
            {
                for (int y = -1; y < boardRows + 1; y++)
                {
                    GameObject tile;
                    if (x == -1 || x == boardColumns || y == -1 || y == boardRows)
                    {
                        tile = borderTiles[Random.Range(0, borderTiles.Length)];
                    }
                    else
                    {
                        tile = floorTiles[Random.Range(0, floorTiles.Length)];
                    }

                    GameObject tileGameObject = Instantiate(tile, new Vector3(x, y, 0), Quaternion.identity);
                    tileGameObject.transform.SetParent(board);
                }
            }
        }

        /// <summary>
        /// Prepare list of positions for generation of random walls/items/enemies
        /// </summary>
        void InitializeRandomPositions()
        {
            randomPositions.Clear();

            // loop from [1, maxRow/Col-2] to prevent generating impassable boards
            for (int row = 1; row < boardRows - 1; row++)
            {
                for (int col = 1; col < boardColumns - 1; col++)
                {
                    randomPositions.Add(new Vector3(row, col, 0));
                }
            }
        }

        /// <summary>
        /// Generate a random board position from the random positions list
        /// </summary>
        Vector3 GenerateRandomPosition()
        {
            int randomIndex = Random.Range(0, randomPositions.Count);
            Vector3 randomPosition = randomPositions[randomIndex];
            // remove randomPosition from the list so that it isn't reused
            randomPositions.RemoveAt(randomIndex);
            return randomPosition;
        }

        /// <summary>
        /// Place randomly generated special objects (potential walls/items/enemies)
        /// </summary>
        /// <param name="tiles">Array of tiles to be placed</param>
        /// <param name="minimum">Min number of objects to be placed</param>
        /// <param name="maximum">Max number of objects to be placed</param>
        void GenerateRandomTiles(GameObject[] tiles, int minimum, int maximum)
        {
            int objectCount = Random.Range(minimum, maximum + 1);

            for (int i = 0; i < objectCount; i++)
            {
                Vector3 randomPosition = GenerateRandomPosition();
                GameObject randomTile = tiles[Random.Range(0, tiles.Length)];
                Instantiate(randomTile, randomPosition, Quaternion.identity);
            }
        }

        /// <summary>
        /// Sets up the entire board (borders, floors, walls, items, enemies)
        /// </summary>
        /// <param name="level"></param>
        public void SetupScene(int level)
        {
            InitializeBoard();
            InitializeRandomPositions();
            GenerateRandomTiles(wallTiles, wallRange.minimum, wallRange.maximum);
            GenerateRandomTiles(foodTiles, foodRange.minimum, foodRange.maximum);

            // number of enemies scales logarithmically to level
            int enemyCount = (int)Mathf.Log(level, 2f);
            GenerateRandomTiles(enemyTiles, enemyCount, enemyCount);

            // create exit
            Instantiate(exit, new Vector3(boardColumns - 1, boardRows - 1, 0f), Quaternion.identity);
        }
    }
}
