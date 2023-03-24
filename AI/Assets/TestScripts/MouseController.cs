using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    public GameObject villagerPrefab;
    public VillagerInfo villager;

    public bool mouseControl;
    public bool villagerPlaced = false;
    public bool isMoving;

    public List<OverlayTile> tilledTiles = new List<OverlayTile>();



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

    private void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (mouseControl)
        {
            var focusedTileHit = GetFocusedOnTile();

            if (focusedTileHit.HasValue)
            {
                OverlayTile overlayTile = focusedTileHit.Value.collider.gameObject.GetComponent<OverlayTile>();

                transform.position = overlayTile.transform.position;
                gameObject.GetComponent<SpriteRenderer>().sortingOrder = overlayTile.GetComponent<SpriteRenderer>().sortingOrder + 1;

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
                        villager.seeds--;
                    }

                }

                if (Input.GetMouseButtonDown(1) && villagerPlaced)
                {
                    overlayTile.TillTile();
                    villager.seeds++;
                    tilledTiles.Add(overlayTile);
                }
            }

            if (Input.GetMouseButtonDown(2) && villagerPlaced && tilledTiles.Count > 0 && !isMoving)
            {
                GameManager.instance.UpdateGameState(GameManager.GameState.PlantSeeds);
            }
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

    public void PositionCharacterOnTile(OverlayTile tile)
    {
        villager.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z);
        villager.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder + 10;
        villager.activeTile = tile;
    }
}
