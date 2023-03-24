using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    public GameObject villagerPrefab;
    private VillagerInfo villager;
    private PathFinder pathFinder;

    public float speed;
    public bool villagerPlaced = false;
    public List<OverlayTile> path = new List<OverlayTile>();
    public List<OverlayTile> tilledTiles = new List<OverlayTile>();

    public bool isMoving;

    private void Start()
    {
        pathFinder = new PathFinder();
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
                    }
                }
                else
                {
                    overlayTile.UntillTile();
                }

            }

            if (Input.GetMouseButtonDown(1) && villagerPlaced)
            {
                overlayTile.TillTile();
                tilledTiles.Add(overlayTile);
            }

            if (Input.GetMouseButtonDown(2) && villagerPlaced)
            {
                Debug.Log("Finding path at " + tilledTiles[0].gridLocation);
                path = pathFinder.FindPath(villager.activeTile, tilledTiles[0]); // initial path
            }

            
        }

        CheckMove();

        // CheckMove();

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
                isMoving = false;
                tilledTiles.RemoveAt(0);
                if (tilledTiles.Count > 0) // get new path
                {
                    path = pathFinder.FindPath(villager.activeTile, tilledTiles[0]);
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
        villager.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder + 5;
        villager.activeTile = tile;
    }
}
