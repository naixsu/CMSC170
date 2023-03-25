using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    public MouseController mouseController;
    private VillagerInfo villager;
    private PathFinder pathFinder;
    private RangeFinder rangeFinder;
    private Coroutine coroutine;

    
    public float waitTime;
    public int range;
    public int speed;
    public bool plantingState;
    public bool harvestingState;
    public bool isMoving;
    public bool tileFound;

    public List<OverlayTile> path = new List<OverlayTile>();
    public List<OverlayTile> tilledTiles = new List<OverlayTile>();
    public List<OverlayTile> inRangeTiles = new List<OverlayTile>();
    private void Awake()
    {
        GameManager.OnStateChange += GameManager_OnStateChange;
    }

    private void OnDestroy()
    {
        GameManager.OnStateChange -= GameManager_OnStateChange;
    }

    private void GameManager_OnStateChange(GameManager.GameState state)
    {
        if (state == GameManager.GameState.PlantSeeds)
        {
            plantingState = true;
            mouseController.mouseControl = false;

            /*villager = mouseController.villager;
            range = 1;
            pathFinder = new PathFinder();
            rangeFinder = new RangeFinder();
            tilledTiles = mouseController.tilledTiles;

            GetInRangeTiles();*/

        }

        if (state == GameManager.GameState.HarvestSeeds)
        {
            harvestingState = true;
        }
    }

    private void Start()
    {
        villager = mouseController.villager;
        range = 1;
        pathFinder = new PathFinder();
        rangeFinder = new RangeFinder();
        tilledTiles = mouseController.tilledTiles;

        GetInRangeTiles();
    }

    private void Update()
    {
        if (plantingState)
        {
            CheckMove();

            CheckPlant();
        }

        if (harvestingState)
        {
            EditorApplication.isPaused = true;
        }
    }

    private void CheckPlant()
    {
        if (mouseController.villagerPlaced && villager.seeds == 0 && plantingState)
        {
            plantingState = false;
            Debug.Log("All seeds have been planted");
            GameManager.instance.UpdateGameState(GameManager.GameState.HarvestSeeds);
        }
    }

    private IEnumerator AddRange()
    {
        yield return new WaitForSeconds(waitTime);

        if (mouseController.tilledTiles.Count > 0 && !isMoving)
        {
            range++;
            GetInRangeTiles();
        }
    }

    private void RemoveRange()
    {
        inRangeTiles = new List<OverlayTile>();
    }

    private void RangeDetection()
    {
        foreach (var tile in inRangeTiles)
        {
            tile.HighlightTile();
            if (tile.isTilled && !tile.hasSeed)
            {
                tileFound = true;
                path = pathFinder.FindPath(villager.activeTile, tile);
                break;
            }
        }
    }

    private void HideHighlightRange()
    {
        foreach (var tile in inRangeTiles)
        {
            tile.HideHighlightTile();
        }
    }

    private void GetInRangeTiles()
    {
        tileFound = false;

        HideHighlightRange();

        inRangeTiles = rangeFinder.GetTilesInRange(villager.activeTile, range);

        RangeDetection();

        if (tileFound)
        {
            Debug.Log("Stop coroutine");
            StopCoroutine(coroutine);
        }
        else if (!tileFound && !isMoving)
        {
            coroutine = StartCoroutine(AddRange());
        }
    }

    private void CheckMove()
    {
        if (path.Count > 0)
        {
            MoveAlongPath();
            isMoving = true;
        }

        if (tilledTiles.Count > 0 && isMoving)
        {
            if (path.Count == 0)
            {
                // plant seed
                villager.activeTile.PlantSeed();
                villager.seeds--;
                isMoving = false;

                // reset range
                range = 1;

                // hide highlight range and remove past range
                HideHighlightRange();
                RemoveRange();

                // pop one tilled tile from the list
                tilledTiles.RemoveAt(0);
                if (tilledTiles.Count > 0) // get new path
                {
                    Debug.Log("there are still " + tilledTiles.Count + " more tilled tiles");
                    GetInRangeTiles();
                }
            }
        }
    }

    private void MoveAlongPath()
    {
        var step = speed * Time.deltaTime;

        villager.transform.position = Vector2.MoveTowards(villager.transform.position, path[0].transform.position, step);

        if (Vector2.Distance(villager.transform.position, path[0].transform.position) < 0.001f)
        {
            mouseController.PositionCharacterOnTile(path[0]);
            path.RemoveAt(0);
        }
    }
}
