using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Randomize : MonoBehaviour
{
    public bool randomize;
    public Dictionary<Vector2Int, OverlayTile> map;
    [SerializeField] private MouseController mouseController;

    private void Start()
    {
        map = MapManager.Instance.map;
        mouseController.seeds = 0;
        StartRandomize();
    }

    private void StartRandomize()
    {
        Debug.Log("Randomizing");
        
        // Randomize Villager position
        Vector2Int randomVillagerPos = GetRandomMapPosition();
        OverlayTile villagerTile = map[randomVillagerPos];

        mouseController.villager = Instantiate(mouseController.villagerPrefab).GetComponent<VillagerInfo>();

        mouseController.PositionCharacterOnTile(villagerTile);
        mouseController.villagerPlaced = true;


        randomize = false;
        GameManager.instance.UpdateGameState(GameManager.GameState.MouseControl);
    }

    private Vector2Int GetRandomMapPosition()
    {
        // Get a random tile from the dictionary
        KeyValuePair<Vector2Int, OverlayTile> randomTile = map.ElementAt(Random.Range(0, map.Count));
        return randomTile.Key;
        
    }
}
