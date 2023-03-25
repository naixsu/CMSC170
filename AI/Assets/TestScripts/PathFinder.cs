using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinder
{
    public List<OverlayTile> FindPath(OverlayTile start, OverlayTile end)
    {
        // list of OverlayTiles
        List<OverlayTile> openList = new List<OverlayTile>();
        List<OverlayTile> closedList = new List<OverlayTile>();

        // https://en.wikipedia.org/wiki/A*_search_algorithm for more reference
        // add start tile to openList
        openList.Add(start);

        while (openList.Count > 0)
        {
            // gets the first tile with the lowest F value
            OverlayTile currentOverlayTile = openList.OrderBy(x => x._F).First();

            // remove currentOverlayTile from open list as path to be considered
            openList.Remove(currentOverlayTile);
            // add to closed list
            closedList.Add(currentOverlayTile);

            // retrace path once goal is found
            if (currentOverlayTile == end)
            {
                return GetFinishedList(start, end);
            }

            // neighboring tiles
            var neighborTiles = MapManager.Instance.GetNeighborTiles(currentOverlayTile);

            foreach (var neighbor in neighborTiles)
            {
                // check valid neighbors
                // don't allow player to move if a neighbor is invalid
                if (neighbor.isBlocked || closedList.Contains(neighbor))
                {
                    continue;
                }

                // calculate G value
                neighbor._G = GetManhattanDistance(start, neighbor);
                // calculate H value
                neighbor._H = GetManhattanDistance(end, neighbor);
                // neighbor's previous tile will be the currentOverlayTile
                // aka where we're standing on is the previous tiles for those four (4) neighbors
                neighbor.previous = currentOverlayTile;

                // if a neighbor is not in an open list,
                // aka a list where we consider as a valid path, then
                // add neighbor to open list
                if (!openList.Contains(neighbor))
                {
                    openList.Add(neighbor);
                }
            }
        }

        return new List<OverlayTile>();
    }

    private List<OverlayTile> GetFinishedList(OverlayTile start, OverlayTile end)
    {
        // funtion basically backtracks/retraces the list of tiles when the shortest
        // path is found
        List<OverlayTile> finishedList = new List<OverlayTile>();

        // set local var currentTile to end
        // aka from end to start
        OverlayTile currentTile = end;

        // loop through the end tile's previous tiles
        // and their previous tiles
        // and their previous tiles
        // and so on
        while (currentTile != start)
        {
            finishedList.Add(currentTile);
            currentTile = currentTile.previous;
        }

        // once path is reached (end -> prev -> ... -> prev -> start)
        // reverse that so the player can follow that path
        finishedList.Reverse();

        return finishedList;
    }

    private int GetManhattanDistance(OverlayTile start, OverlayTile neighbor)
    {
        // https://en.wikipedia.org/wiki/Taxicab_geometry for more reference
        return Mathf.Abs(start.gridLocation.x - neighbor.gridLocation.x) + Mathf.Abs(start.gridLocation.y - neighbor.gridLocation.y);
    }
}
