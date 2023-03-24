using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RangeFinder
{
    public List<OverlayTile> GetTilesInRange(OverlayTile startingTile, int range)
    {
        var inRangeTiles = new List<OverlayTile>();
        int stepCount = 0;

        // add villager's active tile to range list
        //inRangeTiles.Add(startingTile);

        var tileForPreviousStep = new List<OverlayTile>();
        tileForPreviousStep.Add(startingTile);


        while (stepCount < range)
        {
            var neighborTiles = new List<OverlayTile>();

            foreach ( var neighborTile in tileForPreviousStep)
            {
                neighborTiles.AddRange(MapManager.Instance.GetNeighborTiles(neighborTile));
            }

            inRangeTiles.AddRange(neighborTiles);
            tileForPreviousStep = neighborTiles.Distinct().ToList();
            stepCount++;
        }

        return inRangeTiles.Distinct().ToList();
    }
}
