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

    // Start is called before the first frame update
    void Start()
    {
        /*map = new Dictionary<Vector2Int, OverlayTile>();

        var tileMap = gameObject.GetComponentInChildren<Tilemap>();

        BoundsInt bounds = tileMap.cellBounds;

        // loop through our tiles and instantiate an overlay container
        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                var tileLocation = new Vector3Int(x, y);
                var tileKey = new Vector2Int(x, y);

                if (tileMap.HasTile(tileLocation) && !map.ContainsKey(tileKey))
                {
                    var overlayTile = Instantiate(overlayTilePrefab, overlayContainer.transform);
                    var cellWorldPosition = tileMap.GetCellCenterWorld(tileLocation);

                    //Debug.Log(cellWorldPosition);

                    overlayTile.transform.position = new Vector3(cellWorldPosition.x, cellWorldPosition.y, cellWorldPosition.z + 1);
                    overlayTile.GetComponent<SpriteRenderer>().sortingOrder = tileMap.GetComponent<TilemapRenderer>().sortingOrder + 1;
                    overlayTile.gridLocation = tileLocation;
                    map.Add(tileKey, overlayTile);

                }
            }
        }

        CenterCamera(bounds);*/
        

        // print methods

        /*Debug.Log(map);
        Debug.Log("Printing all elements in dict");

        foreach (KeyValuePair<Vector2Int, OverlayTile> kv in map)
            Debug.Log(kv.Value.ToString());*/

    }

    private void SetUp()
    {
        Debug.Log("Setting Up");
        map = new Dictionary<Vector2Int, OverlayTile>();

        var tileMap = gameObject.GetComponentInChildren<Tilemap>();

        BoundsInt bounds = tileMap.cellBounds;

        // loop through our tiles and instantiate an overlay container
        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                var tileLocation = new Vector3Int(x, y);
                var tileKey = new Vector2Int(x, y);

                if (tileMap.HasTile(tileLocation) && !map.ContainsKey(tileKey))
                {
                    var overlayTile = Instantiate(overlayTilePrefab, overlayContainer.transform);
                    var cellWorldPosition = tileMap.GetCellCenterWorld(tileLocation);

                    //Debug.Log(cellWorldPosition);

                    overlayTile.transform.position = new Vector3(cellWorldPosition.x, cellWorldPosition.y, cellWorldPosition.z + 1);
                    overlayTile.GetComponent<SpriteRenderer>().sortingOrder = tileMap.GetComponent<TilemapRenderer>().sortingOrder + 1;
                    overlayTile.gridLocation = tileLocation;
                    map.Add(tileKey, overlayTile);

                }
            }
        }

        CenterCamera(bounds);

        Debug.Log("Finished Setting Up");
        GameManager.instance.UpdateGameState(GameManager.GameState.MouseControl);
    }

    private void CenterCamera(BoundsInt bounds)
    {
        /*Debug.Log(bounds + " " + bounds.size.x + " " + bounds.size.y);
        Debug.Log("I couldn't make the camera be centered BonkSquirt");*/

        Vector3 center = bounds.center - new Vector3(0.5f, 0.5f, 0);
        _camera.transform.position = new Vector3(center.x, bounds.center.y, -10);
    }

    public List<OverlayTile> GetNeighborTiles(OverlayTile currentOverlayTile)
    {
        // var map = MapManager.Instance.map;

        /*Dictionary<Vector2Int, OverlayTile> tileToSearch = new Dictionary<Vector2Int, OverlayTile>();

        if (searchableTiles.Count > 0)
        {
            foreach (var searchableTile in searchableTiles)
            {
                tileToSearch.Add(searchableTile.grid2DLocation, searchableTile);
            }
        }
        else
        {
            tileToSearch = map;
        }*/

        var tileToSearch = map;

        List<OverlayTile> neighbors = new List<OverlayTile>();

        #region GET NEIGHBORS

        // top
        Vector2Int locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x,
            currentOverlayTile.gridLocation.y + 1
            );

        if (tileToSearch.ContainsKey(locationToCheck))
        {
            neighbors.Add(tileToSearch[locationToCheck]);
        }

        // down
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x,
            currentOverlayTile.gridLocation.y - 1
            );

        if (tileToSearch.ContainsKey(locationToCheck))
        {
            neighbors.Add(tileToSearch[locationToCheck]);
        }

        // left
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x - 1,
            currentOverlayTile.gridLocation.y
            );

        if (tileToSearch.ContainsKey(locationToCheck))
        {
            neighbors.Add(tileToSearch[locationToCheck]);
        }

        // right
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x + 1,
            currentOverlayTile.gridLocation.y
            );

        if (tileToSearch.ContainsKey(locationToCheck))
        {
            neighbors.Add(tileToSearch[locationToCheck]);
        }

        #endregion

        return neighbors;
    }


}
