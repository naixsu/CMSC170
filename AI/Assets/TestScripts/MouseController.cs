using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    public GameObject villagerPrefab;
    private VillagerInfo villager;
    private PathFinder pathFinder;
    private RangeFinder rangeFinder;
    private Coroutine coroutine;

    public float speed;
    public int range;
    public int waitTime;
    public bool villagerPlaced = false;
    public bool isMoving;
    public bool tileFound;
    public bool plantingState;
    public List<OverlayTile> path = new List<OverlayTile>();
    public List<OverlayTile> tilledTiles = new List<OverlayTile>();
    public List<OverlayTile> inRangeTiles = new List<OverlayTile>();



    private void Start()
    {
        pathFinder = new PathFinder();
        rangeFinder = new RangeFinder();
        range = 1;
        
    }

    // Update is called once per frame
    void Update()
    {
        var focusedTileHit = GetFocusedOnTile();

        if (focusedTileHit.HasValue)
        {
            OverlayTile overlayTile = focusedTileHit.Value.collider.gameObject.GetComponent<OverlayTile>();

            transform.position = overlayTile.transform.position;
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = overlayTile.GetComponent<SpriteRenderer>().sortingOrder + 1;

            // show overlay tile
            /*if (Input.GetMouseButtonDown(0))
            {
                overlayTile.GetComponent<OverlayTile>().ShowTile();
            }*/

            if (Input.GetMouseButtonDown(0))
            {
                if (!villagerPlaced) 
                {
                    if (villager == null)
                    {
                        villager = Instantiate(villagerPrefab).GetComponent<VillagerInfo>();
                        PositionCharacterOnTile(overlayTile);
                        villagerPlaced = true;

                        // GetInRangeTiles();
                    }
                }
                else
                {
                    overlayTile.UntillTile();
                    villager.seeds--;
                }

            }

            if (Input.GetMouseButtonDown(1) && villagerPlaced)
            {
                overlayTile.TillTile();
                villager.seeds++;
                tilledTiles.Add(overlayTile);
            }

            /*if (Input.GetMouseButtonDown(2) && villagerPlaced && tilledTiles.Count > 0 && !isMoving)
            {
                Debug.Log("Finding path at " + tilledTiles[0].gridLocation);
                path = pathFinder.FindPath(villager.activeTile, tilledTiles[0]); // initial path
                //path = pathFinder.FindPath(villager.activeTile, overlayTile, inRangeTiles);
            }*/

            



        }

        if (Input.GetMouseButtonDown(2) && villagerPlaced && tilledTiles.Count > 0 && !isMoving)
        {
            plantingState = true;
            GetInRangeTiles();
            //path = pathFinder.FindPath(villager.activeTile, tilledTiles[0]); // initial path
        }



        CheckMove();
        CheckPlant();


        if (Input.GetKeyDown(KeyCode.E))
        {
            range++;
            GetInRangeTiles();
        }
            

        // CheckMove();

    }

    private void CheckPlant()
    {
        if (villagerPlaced && villager.seeds == 0 && plantingState)
        {
            plantingState = false;
            Debug.Log("All seeds have been planted");
            EditorApplication.isPaused = true;
        }
    }

    private IEnumerator AddRange()
    {
        yield return new WaitForSeconds(waitTime);

        if (tilledTiles.Count > 0 && !isMoving)
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
            //tile.ShowTile(0.5f);
            tile.HighlightTile();
            if (tile.isTilled && !tile.hasSeed)
            {
                tileFound = true;

                //Debug.Log("Found tilled tile at " + tile.gridLocation);
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

        /*foreach (var tile in inRangeTiles)
        {
            tile.HideHighlightTile();
        }*/
        HideHighlightRange();

        inRangeTiles = rangeFinder.GetTilesInRange(villager.activeTile, range);

        /*foreach (var tile in inRangeTiles)
        {
            //tile.ShowTile(0.5f);
            tile.HighlightTile();
            if (tile.isTilled && !tile.hasSeed)
            {
                tileFound = true;

                //Debug.Log("Found tilled tile at " + tile.gridLocation);
                path = pathFinder.FindPath(villager.activeTile, tile);
                break;
            }
        }*/
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

                // GetInRangeTiles();  

                // pop one tilled tile from the list
                tilledTiles.RemoveAt(0);
                if (tilledTiles.Count > 0) // get new path
                {
                    Debug.Log("there are still " + tilledTiles.Count + " more tilled tiles");
                    GetInRangeTiles();
                    //path = pathFinder.FindPath(villager.activeTile, tilledTiles[0]);
                    //path = pathFinder.FindPath(villager.activeTile, overlayTile, inRangeTiles);
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
            PositionCharacterOnTile(path[0]);
            path.RemoveAt(0); 
        }
    }

    public RaycastHit2D? GetFocusedOnTile()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);

        if (hits.Length > 0)
        {
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }
        return null;
    }

    private void PositionCharacterOnTile(OverlayTile tile)
    {
        villager.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z);
        villager.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder + 10;
        villager.activeTile = tile;
    }
}
