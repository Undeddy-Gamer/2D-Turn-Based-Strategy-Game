using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridLines : MonoBehaviour
{
    // Add grid lines tile to all used tiles in the 'main map' tilemap
    
    // The tilemap that holds the tiles for the map
    public Tilemap landTiles;
    // The tilemap that we will use to display the gridlines on
    public Tilemap gridTiles;
    // the tile that we will use for the gridlines
    public ATile gridLinesTile;

    void Start()
    {
        // Iterate through all tiles in the Land Tiles tilemap
        foreach (var pos in landTiles.cellBounds.allPositionsWithin)
        {

            Vector3Int currentTile = new Vector3Int(pos.x, pos.y, pos.z);
            //Debug.Log(currentTile);
            if (landTiles.HasTile(currentTile))
            {
                // Add gridlines tile to grid lines tilemap
                gridTiles.SetTile(currentTile, gridLinesTile);
            }
        }
    }


}
