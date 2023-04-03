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
    [SerializeField] private Transform _camera;


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
        CenterCamera(bounds);


        // switch GameState once setup is finished
        Debug.Log("Finished Setting Up");
        GameManager.instance.UpdateGameState(GameManager.GameState.MouseControl);
    }

    private void CenterCamera(BoundsInt bounds)
    {
        Vector3 center = bounds.center - new Vector3(0.5f, 0.5f, 0);
        _camera.transform.position = new Vector3(center.x, bounds.center.y, -10);
    }

    public List<OverlayTile> GetNeighborTiles(OverlayTile currentOverlayTile)
    {
        // pathfinding algorithm

        // list of neighboring tiles top, down, left, right
        List<OverlayTile> neighbors = new List<OverlayTile>();

        #region GET NEIGHBORS

        // top
        Vector2Int locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x,
            currentOverlayTile.gridLocation.y + 1
            );

        if (map.ContainsKey(locationToCheck))
        {
            neighbors.Add(map[locationToCheck]);
        }

        // left
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x - 1,
            currentOverlayTile.gridLocation.y
            );

        if (map.ContainsKey(locationToCheck))
        {
            neighbors.Add(map[locationToCheck]);
        }

        // down
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x,
            currentOverlayTile.gridLocation.y - 1
            );

        if (map.ContainsKey(locationToCheck))
        {
            neighbors.Add(map[locationToCheck]);
        }

        // right
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x + 1,
            currentOverlayTile.gridLocation.y
            );

        if (map.ContainsKey(locationToCheck))
        {
            neighbors.Add(map[locationToCheck]);
        }

        #endregion

        return neighbors;
    }


}
