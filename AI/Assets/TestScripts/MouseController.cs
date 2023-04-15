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
    public Dictionary<Vector2Int, OverlayTile> map;

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
        map = MapManager.Instance.map;
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

    public void Randomize()
    {
        Debug.Log("Randomizing");
        RandomVillager();
        RandomTilledTiles();
        RandomBlockedTiles();
    }
    
    void Update()
    {
        ButtonClick();
        if (villagerPlaced)
        {
            villager.seeds = seeds;
        }
        seedCountScript.seedValue = seeds;

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
                if (Input.GetMouseButtonDown(1) && villagerButtonClicked)
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

                    if (villagerPlaced)
                    {
                        if (!overlayTile.isBlocked)
                        {
                            PositionCharacterOnTile(overlayTile);
                            villagerPlaced = true;
                        }
                    }
                }

                else if (Input.GetMouseButton(0) && villagerButtonClicked)
                {
                    if (villagerPlaced)
                    {
                        if (overlayTile == villager.activeTile)
                        {
                            Debug.Log("Destroy");
                            Destroy(villager.gameObject);
                            villagerPlaced = false;
                        }
                    }
                }
                #endregion

                #region HOE BUTTON PRESS
                if (Input.GetMouseButton(1) && hoeButtonClicked)
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
                }

                else if (Input.GetMouseButton(0) && hoeButtonClicked)
                {
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
                if (Input.GetMouseButton(1) && pickaxeButtonClicked)
                {
                    if (villagerPlaced)
                    {
                        if ((overlayTile.isTilled && villager.activeTile != overlayTile) || 
                            (!overlayTile.isTilled && villager.activeTile != overlayTile))
                        {
                            Debug.Log("Override");
                            // if a tilled tile is selected, replace with obstacle
                            overlayTile.BlockTile();
                            // deduct the villager's seed count accordingly
                            if (tilledTiles.Count > 0) seeds--;
                            // remove tile to the list
                            tilledTiles.Remove(overlayTile);
                            toHarvest.Remove(overlayTile);
                            // overlayTile.isBlocked = true;
                        }                                  
                    }

                    if (!villagerPlaced)
                    {
                        if (overlayTile.isTilled || !overlayTile.isBlocked)
                        {
                            Debug.Log("Override");
                            // if a tilled tile is selected, replace with obstacle
                            overlayTile.BlockTile();
                            // deduct the villager's seed count accordingly
                            if (tilledTiles.Count > 0) seeds--;
                            // remove tile to the list
                            tilledTiles.Remove(overlayTile);
                            toHarvest.Remove(overlayTile);
                        }
                    }
                }

                else if (Input.GetMouseButton(0) && pickaxeButtonClicked)
                {
                    if (villagerPlaced)
                    {
                        if (overlayTile.isBlocked && villager.activeTile != overlayTile) overlayTile.UnblockTile();
                    }
                    if (!villagerPlaced)
                    {
                        if (overlayTile.isBlocked) overlayTile.UnblockTile();
                    }
                }



            #endregion
            }
            else
            {
                // disable cursor sprite once cursor is outside tiles
                gameObject.GetComponent<SpriteRenderer>().enabled = false;
            }   
        }
    }

    private void RandomVillager()
    {
        // Randomize Villager position
        // Destroy previous villager if exists
        if (villagerPlaced) Destroy(villager.gameObject);

        Vector2Int randomVillagerPos = GetRandomMapPosition();
        OverlayTile villagerTile = map[randomVillagerPos];

        villager = Instantiate(villagerPrefab).GetComponent<VillagerInfo>();

        PositionCharacterOnTile(villagerTile);
        villagerPlaced = true;
    }

    private void RandomTilledTiles()
    {
        // Destroy tilled tiles if exists
        foreach (KeyValuePair<Vector2Int, OverlayTile> tile in map)
        {
            OverlayTile tileInfo = tile.Value;
            if (tileInfo.isTilled)
            {
                tileInfo.UntillTile();
                tilledTiles.Remove(tileInfo);
            }
           
        }
        // Randomize Tilled Tiles
        int numTilledTiles = Random.Range(1, map.Count);
        for (int i = 0; i < numTilledTiles; i++)
        {
            Vector2Int randomTilledPos = GetRandomMapPosition();
            OverlayTile tilledTile = map[randomTilledPos];
            if (!tilledTile.isBlocked && !tilledTile.isTilled)
            {
                tilledTile.TillTile();
                tilledTiles.Add(tilledTile);
            }
        }
        seeds = numTilledTiles;
    }

    private void RandomBlockedTiles()
    {
        // Destroy blocked tiles if exists
        foreach (KeyValuePair<Vector2Int, OverlayTile> tile in map)
        {
            OverlayTile tileInfo = tile.Value;
            if (tileInfo.isBlocked)
            {
                tileInfo.UnblockTile();
            }

        }
        // Randomize Tilled Tiles
        int numBlockedTiles = Random.Range(1, tilledTiles.Count);
        for (int i = 0; i < numBlockedTiles; i++)
        {
            Vector2Int randomTilledPos = GetRandomMapPosition();
            OverlayTile blockedTile = map[randomTilledPos];
            if (!blockedTile.isBlocked && blockedTile != villager.activeTile && !blockedTile.isTilled)
            {
                blockedTile.BlockTile();
                tilledTiles.Remove(blockedTile);
            }
        }
    }

    private Vector2Int GetRandomMapPosition()
    {
        // Get a random tile from the dictionary
        KeyValuePair<Vector2Int, OverlayTile> randomTile = map.ElementAt(Random.Range(0, map.Count));
        return randomTile.Key;

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
