using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class MouseController : MonoBehaviour
{
    public GameObject villagerPrefab;
    public VillagerInfo villager;

    public int seeds;
    public bool mouseControl;
    public bool villagerPlaced = false;
    public bool isMoving;

    public bool villagerButtonClicked = false;
    [SerializeField] private GameObject _villagerButton;
    [SerializeField] private Sprite defaultVillager;
    [SerializeField] private Sprite villagerSelected;

    public bool hoeButtonClicked = false;
    [SerializeField] private GameObject _hoeButton;
    [SerializeField] private Sprite defaultHoe;
    [SerializeField] private Sprite hoeSelected;

    public bool pickaxeButtonClicked = false;
    [SerializeField] private GameObject _pickaxeButton;
    [SerializeField] private Sprite defaultPickaxe;
    [SerializeField] private Sprite pickaxeSelected;

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
        seeds = 0;
    }

    // Update is called once per frame
    public void villagerButton()
    {
        villagerButtonClicked = !villagerButtonClicked;
        hoeButtonClicked = false;
        pickaxeButtonClicked = false;
    }
    public void pickaxeButton()
    {
        pickaxeButtonClicked = !pickaxeButtonClicked;
        villagerButtonClicked = false;
        hoeButtonClicked = false;
    }
    public void hoeButton()
    {
        hoeButtonClicked = !hoeButtonClicked;
        villagerButtonClicked = false;
        pickaxeButtonClicked = false;
    }
    public void spawnSeedsButton()
    {
        if (villagerPlaced && !isMoving)
            {
                // updates GameState to PlantSeeds if there are one or more tilled tiles in the screen
                if (tilledTiles.Count > 0)
                    GameManager.instance.UpdateGameState(GameManager.GameState.PlantSeeds);
                else
                    Debug.Log("No tilled tiles to plant on");
            }
        villagerButtonClicked = false;
        pickaxeButtonClicked = false;
        hoeButtonClicked = false;
    }
    void Update()
    {
        ButtonClick();

        if (villager != null)
        {
            villager.seeds = seeds;
            seedCountScript.seedValue = villager.seeds;
        }

        if (mouseControl)
        {
            // gets a tile's collider on the game window that a raycast hit
            var focusedTileHit = GetFocusedOnTile();
            // if there is a tile from the raycast hit
            if (focusedTileHit.HasValue)
            {
                gameObject.GetComponent<SpriteRenderer>().enabled = true;
                // get the gameObject the raycast has hit
                OverlayTile overlayTile = focusedTileHit.Value.collider.gameObject.GetComponent<OverlayTile>();
                // set this gameObject's position according to the overlayTile's position
                // this also changes the Cursor sprite's position, which makes it look like
                // the cursor is selecting other tiles
                transform.position = overlayTile.transform.position;
                // adjust this sorting order
                gameObject.GetComponent<SpriteRenderer>().sortingOrder = overlayTile.GetComponent<SpriteRenderer>().sortingOrder + 1;

                #region VILLAGER BUTTON PRESS
                if (Input.GetMouseButtonDown(0) && villagerButtonClicked)
                {
                    if (!villagerPlaced)
                    {
                        if (villager == null && !overlayTile.isBlocked)
                        {
                            // if the villager is not placed and there is no villager prefab in the scene,
                            // instantiate the villager
                            villager = Instantiate(villagerPrefab).GetComponent<VillagerInfo>();
                            // basically positions the villager's transform.position according to the overlayTile's transform.position
                            // set the villager's active tile to the overlayTile detected
                            PositionCharacterOnTile(overlayTile);
                            villagerPlaced = true;
                            //villagerButtonClicked = false;
                        }
                    }
                    else
                    if (villagerPlaced && !overlayTile.isBlocked)
                    {
                        PositionCharacterOnTile(overlayTile);
                        villagerPlaced = true;
                    }

                }
                #endregion

                #region HOE BUTTON PRESS
                if (Input.GetMouseButtonDown(0) && hoeButtonClicked)
                {
                    if(!overlayTile.isTilled && !overlayTile.isBlocked)
                    {
                        // click to till tiles
                        overlayTile.TillTile();
                        // add the villager's seed count accordingly
                        if (tilledTiles.Count >= 0)
                        {
                            //villager.seeds++;
                            seeds++;
                            Debug.Log("seeds " + seeds);
                        }
                        // add tilled tiles to the list
                        tilledTiles.Add(overlayTile);
                        toHarvest.Add(overlayTile);
                    }
                    else
                    if (overlayTile.isTilled && !overlayTile.isBlocked)
                    {
                        // can click to untill a tile (if tilled)
                        overlayTile.UntillTile();
                        // deduct the villager's seed count accordingly
                        if (tilledTiles.Count > 0)
                            //villager.seeds--;
                            seeds--;
                        // remove tile to the list
                        tilledTiles.Remove(overlayTile);
                        toHarvest.Remove(overlayTile);
                    }
                    
                }
                #endregion

                #region PICKAXE BUTTON PRESS
                if (Input.GetMouseButtonDown(0) && pickaxeButtonClicked)
                {

                    if(overlayTile.isTilled && villager.activeTile != overlayTile)
                    {
                        Debug.Log("Override");
                        // if a tilled tile is selected, untill that tile and replace with obstacle
                        overlayTile.UntillTile();
                        overlayTile.BlockTile();
                        // deduct the villager's seed count accordingly
                        if (tilledTiles.Count > 0)
                            //villager.seeds--;
                            seeds--;
                        // remove tile to the list
                        tilledTiles.Remove(overlayTile);
                        toHarvest.Remove(overlayTile);
                        // overlayTile.isBlocked = true;
                    }
                    else if(overlayTile.isBlocked && villager.activeTile != overlayTile)
                    {
                        // place obstacle on tile
                        Debug.Log("here");
                        overlayTile.UnblockTile();
                        // overlayTile.isBlocked = false;
                    }
                    else 
                    if (!villagerPlaced)
                    {

                        Debug.Log("here here");
                        overlayTile.BlockTile();
                        // overlayTile.isBlocked = true;
                    }
                    else if (villagerPlaced && villager.activeTile != overlayTile)
                    {
                        overlayTile.BlockTile();
                    }
                }
            #endregion
            }
            else
            {
                gameObject.GetComponent<SpriteRenderer>().enabled = false;
            }
            
        }

    }

    private void ButtonClick()
    {
        if (hoeButtonClicked)
        {
            _hoeButton.GetComponent<Image>().sprite = hoeSelected;
            _villagerButton.GetComponent<Image>().sprite = defaultVillager;
            _pickaxeButton.GetComponent<Image>().sprite = defaultPickaxe;
        }

        if (pickaxeButtonClicked)
        {
            _hoeButton.GetComponent<Image>().sprite = defaultHoe;
            _villagerButton.GetComponent<Image>().sprite = defaultVillager;
            _pickaxeButton.GetComponent<Image>().sprite = pickaxeSelected;
        }

        if (villagerButtonClicked)
        {
            _hoeButton.GetComponent<Image>().sprite = defaultHoe;
            _villagerButton.GetComponent<Image>().sprite = villagerSelected;
            _pickaxeButton.GetComponent<Image>().sprite = defaultPickaxe;
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
