
using UnityEngine;

public class TileNode
{
    public int G { get; set; }
    public int H { get; set; }
    public int F { get; set; }

    //Parent node is the node that led to this one, used to retrace back and create a final path
    public TileNode Parent { get; set; }

    //The position of the tile
    public Vector3Int Position { get; set; }

    public TileNode(Vector3Int position)
    {
        this.Position = position;
    }
}