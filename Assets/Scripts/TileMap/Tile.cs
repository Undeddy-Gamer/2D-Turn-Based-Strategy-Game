using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Tile", menuName = "Tiles/ATile")]
public class ATile : Tile
{
    [SerializeField]
    public int movementCost = 1;
    [SerializeField]
    public bool isWalkable = true;
}

