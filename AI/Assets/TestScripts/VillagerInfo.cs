using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagerInfo : MonoBehaviour
{
    public int seeds;
    public int crops;
    public OverlayTile activeTile;
    // Start is called before the first frame update
    void Start()
    {
        seeds = 0;
        crops = 0;
        // seeds = MapManager.Instance.map.Count;
    }
}
