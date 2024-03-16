using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapGeneration : MonoBehaviour
{
    // Map generation data
    public GameObject[] gridPrefabs;
    private Tile[,] grid;

    /* Change these through the Game Manager before starting */
    public int rows;
    public int cols;
    public float tileWidth = 50f;
    public float tileHeight = 50f;
    
    // Random Generation
    public int mapSeed;
    [HideInInspector]
    public enum GenerationMethod {MapOfTheDay, MapSeed, RandomTimeGeneration}
    public GenerationMethod currentMapGenerationMethod;

    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    // Random Tile Picker
    public GameObject RandomTilePrefab()
    {
        return gridPrefabs[UnityEngine.Random.Range(0, gridPrefabs.Length)];
    }

    public void GenerateMap()
    {
        SetMapGenerationMethod();

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
                /* Still through error for using the parent transform */ // TODO: Fix Me!!!!
                tempTileObject.transform.parent = transform;

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

    public int DateToInt(DateTime dateData)
    {
        // Add up all the values that represent the time for the player in their given time zone.
        return dateData.Year + dateData.Month + dateData.Day + dateData.Hour + dateData.Minute + dateData.Second + dateData.Millisecond;
    }

    protected void SetMapGenerationMethod()
    {
         switch (currentMapGenerationMethod)
        {
            case GenerationMethod.MapOfTheDay:
                // Use todays current date to seed the Random function
                UnityEngine.Random.InitState(DateToInt(DateTime.Now.Date));
                break;
            case GenerationMethod.MapSeed:
                // Set the mapSeed value for random generation.
                UnityEngine.Random.InitState(mapSeed);
                break;
            case GenerationMethod.RandomTimeGeneration:
                // Use the current time as an integer to seed the Random function
                UnityEngine.Random.InitState(DateToInt(DateTime.Now));
                break;
        }
    }
}
