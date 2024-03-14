using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGeneration : MonoBehaviour
{
    public GameObject[] gridPrefabs;
    public int rows;
    public int cols;
    public float tileWidth = 50f;
    public float tileHeight = 50f;
    private Tile[,] grid;

    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    // Random Tile Picker
    public GameObject RandomTilePrefab()
    {
        return gridPrefabs[Random.Range(0, gridPrefabs.Length)];
    }

    public void GenerateMap()
    {
        // Initialize and Clear the grid - Column "X", Row "Y"
        grid = new Tile[rows, cols];

        // Grid Row
        for (int currentRow = 0; currentRow < rows; currentRow++)
        {
            // Grid Column in the Row
            for (int currentCol = 0;currentCol < cols; currentCol++)
            {
                // Calculate the location
                float xPosition = tileWidth * currentCol;
                float zPosition = tileHeight * currentRow;
                Vector3 newPosition = new Vector3(xPosition, 0.0f, zPosition);

                // Create a new grid at the calculated position
                GameObject tempTileObject = Instantiate(RandomTilePrefab(), newPosition, Quaternion.identity);

                // Set the parent of the tempRoomObject
                tempTileObject.transform.parent = this.transform;

                // Give the tile a structured name
                tempTileObject.name = "Tile_" + currentCol + "," + currentRow;

                // Retrieve the Tile Object
                Tile tempTile = tempTileObject.GetComponent<Tile>();

                // Set the Tile to the grid array
                grid[currentCol,currentRow] = tempTile;


                /* Open and Close Doors */
                if (currentRow == 0)
                {
                    // Set Bottom Row Doors
                    tempTile.doorNorth.SetActive(false);
                }
                else if (currentRow == rows - 1) 
                {
                    // Set Top Row Doors
                    tempTile.doorSouth.SetActive(false);
                }
                else
                {
                    // Set Doors of Rows in between
                    tempTile.doorSouth.SetActive(false);
                    tempTile.doorNorth.SetActive(false);
                }

                if (currentCol == 0)
                {
                    tempTile.doorEast.SetActive(false);
                }
                else if (currentCol == cols - 1)
                {
                    tempTile.doorWest.SetActive(false);
                }
                else
                {
                    tempTile.doorEast.SetActive(false);
                    tempTile.doorWest.SetActive(false);
                }
            }
        }
    }
}
