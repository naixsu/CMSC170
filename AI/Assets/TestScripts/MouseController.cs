using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MouseController : MonoBehaviour
{
    public GameObject villagerPrefab;
    public VillagerInfo villager;

    public bool mouseControl;
    public bool villagerPlaced = false;
    public bool isMoving;

    public List<OverlayTile> tilledTiles = new List<OverlayTile>();
    public List<OverlayTile> toHarvest = new List<OverlayTile>();


    #region GAME MANAGER

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
        if (state == GameManager.GameState.MouseControl)
        {
            mouseControl = true;
        }
    }

    #endregion

    private void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (mouseControl)
        {
            // gets a tile's collider on the game window that a raycast hit
            var focusedTileHit = GetFocusedOnTile();
            // if there is a tile from the raycast hit
            if (focusedTileHit.HasValue)
            {
                // get the gameObject the raycast has hit
                OverlayTile overlayTile = focusedTileHit.Value.collider.gameObject.GetComponent<OverlayTile>();
                // set this gameObject's position according to the overlayTile's position
                // this also changes the Cursor sprite's position, which makes it look like
                // the cursor is selecting other tiles
                transform.position = overlayTile.transform.position;
                // adjust this sorting order
                gameObject.GetComponent<SpriteRenderer>().sortingOrder = overlayTile.GetComponent<SpriteRenderer>().sortingOrder + 1;

                #region LEFT CLICK ACTIONS
                if (Input.GetMouseButtonDown(0))
                {
                    if (!villagerPlaced)
                    {
                        if (villager == null)
                        {
                            // if the villager is not placed and there is no villager prefab in the scene,
                            // instantiate the villager
                            villager = Instantiate(villagerPrefab).GetComponent<VillagerInfo>();
                            // basically positions the villager's transform.position according to the overlayTile's transform.position
                            // set the villager's active tile to the overlayTile detected
                            PositionCharacterOnTile(overlayTile);
                            villagerPlaced = true;
                        }
                    }
                    else
                    {
                        // if a villager is in the screen, can left click to untill a tile (if tilled)
                        overlayTile.UntillTile();
                        // deduct the villager's seed count accordingly
                        if (tilledTiles.Count > 0)
                            villager.seeds--;
                        // remove tile to the list
                        tilledTiles.Remove(overlayTile);
                    }

                }
                #endregion

                #region RIGHT CLICK ACTIONS
                if (Input.GetMouseButtonDown(1) && villagerPlaced)
                {
                    // if a villager is in the scene, can right click to till tiles
                    overlayTile.TillTile();
                    // add the villager's seed count accordingly
                    if (tilledTiles.Count >= 0)
                    {
                        villager.seeds++;
                        villager.crops++;
                        Debug.Log("seeds " + villager.seeds);
                    }
                        
                    // add tilled tiles to the list
                    tilledTiles.Add(overlayTile);
                    toHarvest.Add(overlayTile);
                }
                #endregion
            }

            #region MIDDLE MOUSE CLICK ACTIONS
            if (Input.GetMouseButtonDown(2) && villagerPlaced && !isMoving)
            {
                // updates GameState to PlantSeeds if there are one or more tilled tiles in the screen
                if (tilledTiles.Count > 0)
                    GameManager.instance.UpdateGameState(GameManager.GameState.PlantSeeds);
                else
                    Debug.Log("No tilled tiles to plant on");
            }
            #endregion
        }
    }

    // simple raycast function that gets everything the line touches from top to bottom
    public RaycastHit2D? GetFocusedOnTile()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);

        if (hits.Length > 0)
        {
            // returns the topmost component/gameObject/whatever the raycast hits
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }
        return null;
    }

    // comments about this above
    public void PositionCharacterOnTile(OverlayTile tile)
    {
        villager.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z);
        villager.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder + 10;
        villager.activeTile = tile;
    }
}
