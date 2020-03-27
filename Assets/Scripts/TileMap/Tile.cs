using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Tile", menuName = "Tiles/ATile")]

[System.Serializable]
public class ATile : Tile
{
    [SerializeField]
    public int movementCost = 1;
    [SerializeField]
    public bool isWalkable = true;
    [SerializeField]
    public int damagePerTurn = 0;
}

