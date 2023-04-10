using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NewPathFinder
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    public List<OverlayTile> FindPath(OverlayTile start, OverlayTile end)
    {
        Debug.Log("newPathfinder");
        // list of OverlayTiles
        List<OverlayTile> openList = new List<OverlayTile>();
        List<OverlayTile> closedList = new List<OverlayTile>();

        Dictionary<Vector2Int, OverlayTile> map = MapManager.Instance.map;

        // https://en.wikipedia.org/wiki/A*_search_algorithm for more reference
        // add start tile to openList
        openList.Add(start);

        // initialize each tile
        foreach (OverlayTile tile in map.Values)
        {
            tile._G = int.MaxValue;
            tile.CalculateFCost();
            tile.previous = null;
        }

        // starting data
        start._G = 0;
        start._H = GetDiagonalDistance(start, end);
        start.CalculateFCost();

        while (openList.Count > 0) 
        {
            // OverlayTile currentOverlayTile = GetLowestFCostNode(openList);

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

/*            if (currentOverlayTile == end)
            {
                // reach final node
                return GetFinishedList(start, end);
            }

            openList.Remove(currentOverlayTile);
            closedList.Add(currentOverlayTile);*/

            // neighboring tiles
            var neighborTiles = MapManager.Instance.GetNeighborTiles(currentOverlayTile);

            foreach (var neighborTile in neighborTiles)
            {
                if (neighborTile.isBlocked || closedList.Contains(neighborTile))
                {
                    continue;
                }

                int tentativeGCost = currentOverlayTile._G + GetDiagonalDistance(currentOverlayTile, neighborTile);
                if (tentativeGCost < neighborTile._G)
                {
                    neighborTile.previous = currentOverlayTile;
                    neighborTile._G = tentativeGCost;
                    neighborTile._H = GetDiagonalDistance(neighborTile, end);
                    neighborTile.CalculateFCost();
                    
                    if (!openList.Contains(neighborTile))
                    {
                        openList.Add(neighborTile);
                    }
                }
            }
        }

        return null;
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



    private int GetDiagonalDistance(OverlayTile start, OverlayTile neighbor)
    {
        int dx = Mathf.Abs(start.gridLocation.x - neighbor.gridLocation.x);
        int dy = Mathf.Abs(start.gridLocation.y - neighbor.gridLocation.y);
        int diagonalSteps = Mathf.Min(dx, dy);
        int straightSteps = Mathf.Abs(dx - dy);
        return diagonalSteps * MOVE_DIAGONAL_COST + straightSteps * MOVE_STRAIGHT_COST;
    }

    private OverlayTile GetLowestFCostNode(List<OverlayTile> pathNodeList)
    {
        OverlayTile lowestFCostNode = pathNodeList[0];

        for (int i = 1; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i]._F < lowestFCostNode._F)
                lowestFCostNode = pathNodeList[i];
        }

        return lowestFCostNode;
    }
}
