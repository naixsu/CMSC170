using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    private static MapManager instance;
    public static MapManager Instance { get { return instance; } }

    public OverlayTile overlayTilePrefab;
    public GameObject overlayContainer;

    public Dictionary<Vector2Int, OverlayTile> map;
    [SerializeField] private Camera _camera;
    [SerializeField] private float _padding = 1f;


    #region GAME MANAGER
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        GameManager.OnStateChange += GameManager_OnStateChange;
    }

    private void OnDestroy()
    {
        GameManager.OnStateChange -= GameManager_OnStateChange;
    }

    private void GameManager_OnStateChange(GameManager.GameState state)
    {
        if (state == GameManager.GameState.SetUp)
        {
            SetUp();
        }
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {

    }

    private void SetUp()
    {
        Debug.Log("Setting Up");
        // get a dictionary of all the tiles in the screen
        map = new Dictionary<Vector2Int, OverlayTile>();

        var tileMap = gameObject.GetComponentInChildren<Tilemap>();
        var count = 0;

        // the tilemap's bounds (position , size)
        BoundsInt bounds = tileMap.cellBounds;

        // loop through our tiles and instantiate an overlay container
        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                var tileLocation = new Vector3Int(x, y);
                var tileKey = new Vector2Int(x, y);

                // add to dictionary
                if (tileMap.HasTile(tileLocation) && !map.ContainsKey(tileKey))
                {
                    // instantiate the gameObject
                    var overlayTile = Instantiate(overlayTilePrefab, overlayContainer.transform);
                    // change the name for better tracking purposes
                    overlayTile.name = overlayTile.name + "_" + count++;
                    // get the coordinates for the tile in the scene
                    var cellWorldPosition = tileMap.GetCellCenterWorld(tileLocation);

                    // assign the gameObject's position according to its tileMap position
                    overlayTile.transform.position = new Vector3(cellWorldPosition.x, cellWorldPosition.y, cellWorldPosition.z + 1);
                    // adjust the sprite's sorting order
                    overlayTile.GetComponent<SpriteRenderer>().sortingOrder = tileMap.GetComponent<TilemapRenderer>().sortingOrder + 1;
                    // assign its gridLocation to be used in pathfinding
                    overlayTile.gridLocation = tileLocation;
                    // add to dictionary
                    map.Add(tileKey, overlayTile);
                }
            }
        }

        // try to center camera based on the tilemap's bounds
        // need to make this function perfect
        ResizeCameraToTilemap(tileMap);


        // switch GameState once setup is finished
        Debug.Log("Finished Setting Up");
        GameManager.instance.UpdateGameState(GameManager.GameState.MouseControl);
    }


    private void ResizeCameraToTilemap(Tilemap tilemap)
    {
        BoundsInt bounds = tilemap.cellBounds;
        Vector3Int minTilePosition = bounds.min;
        Vector3Int maxTilePosition = bounds.max;

        // Get the position of the corners of the tilemap in world space
        Vector3 bottomLeftCorner = tilemap.CellToWorld(minTilePosition) + new Vector3(0.5f, 0.5f, 0f);
        Vector3 topRightCorner = tilemap.CellToWorld(maxTilePosition) - new Vector3(0.5f, 0.5f, 0f);

        // Calculate the camera's new position and size
        Vector3 cameraPosition = (bottomLeftCorner + topRightCorner) / 2f;
        float cameraSize = Mathf.Max((topRightCorner.y - bottomLeftCorner.y) / 2f, (topRightCorner.x - bottomLeftCorner.x) / 2f / _camera.aspect) + _padding;

        // Set the camera's position and size
        _camera.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, _camera.transform.position.z);
        _camera.orthographicSize = cameraSize;
    }


    public List<OverlayTile> TryGetNeighborTiles(OverlayTile overlayTile)
    {
        Debug.Log("TryGetNeighborTiles");

        List<OverlayTile> neighborTiles = new List<OverlayTile>();

        // get coordinates of current tile
        Vector3Int currentPos = overlayTile.gridLocation;

        // get neighboring tiles
        for (int x = currentPos.x - 1; x <= currentPos.x + 1; x++)
        {
            for (int y = currentPos.y - 1; y <= currentPos.y + 1; y++)
            {
                if (x == currentPos.x && y == currentPos.y)
                {
                    continue;
                }

                if (map.TryGetValue(new Vector2Int(x, y), out OverlayTile neighborTile))
                {
                    // check if neighbor tile is blocked or not
                    if (!neighborTile.isBlocked)
                    {
                        // check if the diagonal tile is blocked
                        bool blockedHorizontal = map.TryGetValue(new Vector2Int(x, currentPos.y), out OverlayTile horizontalTile) && horizontalTile.isBlocked;
                        bool blockedVertical = map.TryGetValue(new Vector2Int(currentPos.x, y), out OverlayTile verticalTile) && verticalTile.isBlocked;
                        if (!blockedHorizontal || !blockedVertical)
                        {
                            neighborTiles.Add(neighborTile);
                        }
                    }
                }
            }
        }

        return neighborTiles;
    }


    public List<OverlayTile> GetNeighborTiles(OverlayTile currentOverlayTile)
    {
        Debug.Log("GetNeighborTiles");

        // list of neighboring tiles top, down, left, right, topleft, topright, downleft, downright
        List<OverlayTile> neighbors = new List<OverlayTile>();

        #region GET NEIGHBORS

        // top
        Vector2Int locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x,
            currentOverlayTile.gridLocation.y + 1
            );

        if (map.ContainsKey(locationToCheck))
        {
            OverlayTile neighborTile = map[locationToCheck];
            //if (!neighborTile.isBlocked)
                neighbors.Add(neighborTile);
        }

        // top left
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x - 1,
            currentOverlayTile.gridLocation.y + 1
            );

        if (map.ContainsKey(locationToCheck))
        {
            OverlayTile neighborTile = map[locationToCheck];
            //if (!neighborTile.isBlocked)
                neighbors.Add(neighborTile);
        }

        // left
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x - 1,
            currentOverlayTile.gridLocation.y
            );

        if (map.ContainsKey(locationToCheck))
        {
            OverlayTile neighborTile = map[locationToCheck];
            //if (!neighborTile.isBlocked)
                neighbors.Add(neighborTile);
        }

        // left down
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x - 1,
            currentOverlayTile.gridLocation.y - 1
            );

        if (map.ContainsKey(locationToCheck))
        {
            OverlayTile neighborTile = map[locationToCheck];
            //if (!neighborTile.isBlocked)
                neighbors.Add(neighborTile);
        }

        // down
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x,
            currentOverlayTile.gridLocation.y - 1
            );

        if (map.ContainsKey(locationToCheck))
        {
            OverlayTile neighborTile = map[locationToCheck];
            //if (!neighborTile.isBlocked)
                neighbors.Add(neighborTile);
        }

        // down right
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x + 1,
            currentOverlayTile.gridLocation.y - 1
            );

        if (map.ContainsKey(locationToCheck))
        {
            OverlayTile neighborTile = map[locationToCheck];
            //if (!neighborTile.isBlocked)
                neighbors.Add(neighborTile);
        }

        // right
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x + 1,
            currentOverlayTile.gridLocation.y
            );

        if (map.ContainsKey(locationToCheck))
        {
            OverlayTile neighborTile = map[locationToCheck];
            //if (!neighborTile.isBlocked)
                neighbors.Add(neighborTile);
        }

        // right iop
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x + 1,
            currentOverlayTile.gridLocation.y + 1
            );

        if (map.ContainsKey(locationToCheck))
        {
            OverlayTile neighborTile = map[locationToCheck];
            //if (!neighborTile.isBlocked)
                neighbors.Add(neighborTile);
        }

        #endregion

        return neighbors;
    }


}
