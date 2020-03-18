using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
// Linq allows query based operations (like SQL)
using System.Linq;

public class UnitMovement : MonoBehaviour
{

    public Tilemap tileMap;
    public ATile baseTile;
    public ATile pathTile;

    //public ATile dlTile;
    //public ATile drTile;
    //public ATile ulTile;
    //public ATile urTile;
    //public ATile rTile;
    //public ATile lTile;

    //change back to private after debugging complete
    // original unit position
    public Vector3Int origPos;
    // pos position on click
    public Vector3 mousePos;
    // goal tile position
    public Vector3Int tilePos;

    
    //how fast the unit trabels over the map visually
    public float visualMovementSpeed = .75f;

    //Pathfinding
    //Meta defining play here

    public int teamNum;
    //public int x;
    //public int y;
    
    //for ray casting on click (only check tile map layer)
    [SerializeField]
    private LayerMask mask;

    // Path Finding Variables
    bool first = true;
    private TileNode current;
    // Node holds all required information we need about a tile to help calculate the fastest available path
    private TileNode currentNode; 
    // A Stack is a list 'last in first out'
    private Stack<Vector3Int> path;
    
    //checked list
    private HashSet<TileNode> openList;
    private HashSet<TileNode> closedList;
    //we are using HashSets above to avoid duplication of nodes in our lists and help with performance compared to other types of lists

    private Dictionary<Vector3Int, TileNode> allNodes = new Dictionary<Vector3Int, TileNode>();



    private void Start()
    {
        
    }


    void Update()
    {

        if (!Menu.isPaused)
        { 
            if (UnitManager.gameUnits[UnitManager.currUnitTurn].playerControlled && UnitManager.actionPoints > 0)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    origPos = tileMap.WorldToCell(UnitManager.gameUnits[UnitManager.currUnitTurn].transform.position );
                    mousePos = Input.mousePosition;
                    tilePos = tileMap.WorldToCell(Camera.main.ScreenToWorldPoint(mousePos));

                    //Thorws a raycast in the direction of the target
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePos), Vector2.zero, Mathf.Infinity, mask);

                    if (hit.collider != null)
                    {


                        /*
                        *   Calculate pathfinding and apply
                        *
                        *   Insert Code Here
                        *
                        */

                        Stack<Vector3Int> finaPath = CalculatePath(false);


                        // Debugging

                        // Temporary movment code
                        // jumpt to mouse click
                        StartCoroutine(moveOverSeconds(UnitManager.gameUnits[UnitManager.currUnitTurn].gameObject, tilePos));

                        // get cselected tile 
                        //ATile aTile = (ATile)tileMap.GetTile(tilePos);
                        //apply tile movement cost to available action points
                        //UnitManager.actionPoints -= aTile.movementCost;

                        //GetAdjacentTiles(tilePos);

                        // End Debugging
                    }
                }
            }
            // else it's an AI controlled unit's turn and if it still has action points
            else if (UnitManager.actionPoints > 0) //AI's turn
                AIUnitController.CalculateAITurn();
            // else if is AI controlled unit's turn and has no more action points
            else if (!UnitManager.gameUnits[UnitManager.currUnitTurn].playerControlled & UnitManager.actionPoints <= 0) 
            {
                //Initiate next unit turn
                UnitManager.SelectNextUnit();
            }
        }
        // else wait for player to click end turn button

    }


    // Function to move unit (gameobject) to tile over time
    public IEnumerator moveOverSeconds(GameObject objectToMove, Vector3Int endTile)
    {
        while (objectToMove.transform.position != tileMap.GetCellCenterWorld(endTile))
        {
            objectToMove.transform.position = Vector3.Lerp(objectToMove.transform.position, tileMap.GetCellCenterWorld(endTile), visualMovementSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

    }


    // Path Finding Code

    #region PathFinding


    private void Initialize()
    {
        // get  the original start position node
        current = GetNode(origPos);

        //Create an 'open list' for nodes to check later if required
        openList = new HashSet<TileNode>();

        //Creates a 'closed list' for nodes that we have checked
        closedList = new HashSet<TileNode>();

        //changedTiles = new HashSet<Vector3Int>();

        //Adds the current node to the open list
        openList.Add(current);

        path = null;
        first = false;
    }


    public Stack<Vector3Int> CalculatePath(bool step)
    {
        if (current == null)
        {
            Initialize();
        }

        while (openList.Count > 0 && path == null)
        {
            List<TileNode> neighbours = FindNeighbours(current.Position);

            ExamineNeighbours(neighbours, current);

            UpdateCurrentTile(ref current);

            path = GeneratePath(current);

            if (step)
            {
                break;
            }
        }


        // show path on map (debugging)
        if (path != null)
        {
            foreach (Vector3Int position in path)
            {
                //if (position != tilePos)
                //{
                tileMap.SetTile(position, baseTile);
                //}
                int nextTileMoveCost = ((ATile)tileMap.GetTile(current.Position)).movementCost;

                if (UnitManager.actionPoints <= nextTileMoveCost)
                {
                    UnitManager.actionPoints -= ((ATile)tileMap.GetTile(current.Position)).movementCost;
                    if (!UnitManager.gameUnits[UnitManager.currUnitTurn].playerControlled)
                    {
                        UnitManager.SelectNextUnit();
                    } // otherwise wait for player to hit End Turn button;
                }
                //Apply unit movement

            }
            tileMap.SetTile(origPos, baseTile);
            return path;
            //tileMap.SetTile(tilePos, baseTile);

        }
        else return null;


    }


    // get neigbour tiles debug test
    //void GetAdjacentTiles(Vector3Int tilePosition)
    //{
    //    List<TileNode> neighbours = FindNeighbours(tilePosition);

    //    foreach (TileNode n in neighbours)
    //    {
    //        //Debug check
    //       tileMap.SetTile(n.Position, baseTile);
    //    }
    //}


    
    private List<TileNode> FindNeighbours(Vector3Int tilePosition)
    {
        List<TileNode> neighbours = new List<TileNode>();
        //'Lazy' add neigbours tiles to list, this will not work universally and is dependant on the tiles being the pre-determined size


        // an offset used to correct the position of the bottom and top tiles when the selected position is an odd number, required to do this due to being a Hexagon grid
        int hexTileOffset = 0;
        if (!(tilePosition.y % 2 == 0))
        {
            hexTileOffset = 1;
        }

        ATile tempTile;
        

        //RIGHT Get the Tile to the right of current tile and add to neighbour  list if a viable tile
        tempTile = (ATile)tileMap.GetTile(new Vector3Int(tilePosition.x + 1, tilePosition.y, 0));
        //Check if there is a tile there
        if (tempTile != null)
        {
            // check if tile is walkable or the current unit can fly
            if (tempTile.isWalkable || UnitManager.gameUnits[UnitManager.currUnitTurn].canFly)
            {
                //add tile to our viable neighbour tile list
                TileNode neighbour = GetNode(new Vector3Int(tilePosition.x + 1, tilePosition.y, 0));
                neighbours.Add(neighbour);
            }
        }

        //LEFT Get the Tile to the left of current tile and add to neighbour  list if a viable tile
        tempTile = (ATile)tileMap.GetTile(new Vector3Int(tilePosition.x - 1, tilePosition.y, 0));
        //Check if there is a tile there
        if (tempTile != null)
        {
            // check if tile is walkable or the current unit can fly
            if (tempTile.isWalkable || UnitManager.gameUnits[UnitManager.currUnitTurn].canFly)
            {
                //add tile to our viable neighbour tile list
                TileNode neighbour = GetNode(new Vector3Int(tilePosition.x - 1, tilePosition.y, 0));
                neighbours.Add(neighbour);
            }
        }


        //UPRIGHT Get the Tile up and to the right of current tile and add to neighbour list if a viable tile
        tempTile = (ATile)tileMap.GetTile(new Vector3Int(tilePosition.x + hexTileOffset, tilePosition.y + 1, 0));
        //Check if there is a tile there
        if (tempTile != null)
        {
           // check if tile is walkable or the current unit can fly 
            if (tempTile.isWalkable || UnitManager.gameUnits[UnitManager.currUnitTurn].canFly)
            {
                //add tile to our viable neighbour tile list
                TileNode neighbour = GetNode(new Vector3Int(tilePosition.x + hexTileOffset, tilePosition.y + 1, 0));
                neighbours.Add(neighbour);
            }
        }

        //UPLEFT Get the Tile up and to the left of current tile and add to neighbour list if a viable tile
        tempTile = (ATile)tileMap.GetTile(new Vector3Int(tilePosition.x - 1 + hexTileOffset, tilePosition.y + 1, 0));
        //Check if there is a tile there
        if (tempTile != null)
        {
            // check if tile is walkable or the current unit can fly
            if (tempTile.isWalkable || UnitManager.gameUnits[UnitManager.currUnitTurn].canFly)
            {
                //add tile to our viable neighbour tile list
                TileNode neighbour = GetNode(new Vector3Int(tilePosition.x - 1 + hexTileOffset, tilePosition.y + 1, 0));
                neighbours.Add(neighbour);
            }
        }

        

        //DOWNLEFT Get the Tile up and to the right of current tile and add to neighbour list if a viable tile
        tempTile = (ATile)tileMap.GetTile(new Vector3Int(tilePosition.x - 1 + hexTileOffset, tilePosition.y - 1, 0));
        //Check if there is a tile there
        if (tempTile != null)
        {
            // check if tile is walkable or the current unit can fly 
            if (tempTile.isWalkable || UnitManager.gameUnits[UnitManager.currUnitTurn].canFly)
            {
                //add tile to our viable neighbour tile list
                TileNode neighbour = GetNode(new Vector3Int(tilePosition.x - 1 + hexTileOffset, tilePosition.y - 1, 0));
                neighbours.Add(neighbour);
            }
        }

        //DOWNRIGHT Get the Tile up and to the left of current tile and add to neighbour list if a viable tile
        tempTile = (ATile)tileMap.GetTile(new Vector3Int(tilePosition.x + hexTileOffset, tilePosition.y - 1, 0));
        //Check if there is a tile there
        if (tempTile != null)
        {
            // check if tile is walkable or the current unit can fly
            if (tempTile.isWalkable || UnitManager.gameUnits[UnitManager.currUnitTurn].canFly)
            {
                //add tile to our viable neighbour tile list
                TileNode neighbour = GetNode(new Vector3Int(tilePosition.x + hexTileOffset, tilePosition.y - 1, 0));
                neighbours.Add(neighbour);
            }
        }        

        return neighbours;
    }

    private void ExamineNeighbours(List<TileNode> neighbours, TileNode current)
    {
        for (int i = 0; i < neighbours.Count; i++)
        {
            TileNode neighbour = neighbours[i];

            //if (!ConnectedDiagonally(current, neighbour))
            //{
            //    continue;
            //}

            current.G = 10;

            int gScore = ((ATile)tileMap.GetTile(neighbour.Position)).movementCost;

            if (openList.Contains(neighbour))
            {
                if (current.G + gScore < neighbour.G)
                {
                    CalcValues(current, neighbour, gScore);
                }
            }
            else if (!closedList.Contains(neighbour))
            {
                CalcValues(current, neighbour, gScore);

                if (!openList.Contains(neighbour)) //An extra check for openlist containing the neigbour
                {
                    openList.Add(neighbour); //Then we need to add the node to the openlist
                }
            }
        }
    }

    private void UpdateCurrentTile(ref TileNode current)
    {
        //The current node is removed fromt he open list
        openList.Remove(current);

        //The current node is added to the closed list
        closedList.Add(current);

        if (openList.Count > 0) //If the openlist has nodes on it, then we need to sort them by it's F value
        {
            current = openList.OrderBy(x => x.F).First();//Orders the list by the f value, to make it easier to pick the node with the lowest F val
        }
    }


    //Don't need this it will be the same for all neighbours
    //private int DetermineGScore(Vector3Int neighbour, Vector3Int current)
    //{
    //    int gScore = 0;

    //    int x = current.x - neighbour.x;
    //    int y = current.y - neighbour.y;

    //    if (Mathf.Abs(x - y) % 2 == 1)
    //    {
    //        gScore = 10; //The gscore for a vertical or horizontal node is 10
    //    }
    //    else
    //    {
    //        gScore = 14;
    //    }

    //    return gScore;
    //}


    private Stack<Vector3Int> GeneratePath(TileNode current)
    {
        if (current.Position == tilePos) //If the current node is the goal tile position, then we found a path ... WOOHOO!
        {
            //Creates a stack to contain the final path
            Stack<Vector3Int> finalPath = new Stack<Vector3Int>();

            //Add the nodes to the final path
            while (current.Position != origPos )
            {
                
                    //Adds the current node to the final path
                    finalPath.Push(current.Position);
                    //Find the parent of the node, this is actually retracing the whole path back to start
                    //By doing so, we will end up with a complete path.
                    current = current.Parent;                                
                    
            }

            //Returns the complete path
            return finalPath;
        }

        return null;
    }


    //Calculates teh values we will use to (as efficiently as possible) guess the next nodes to check
    private void CalcValues(TileNode parent, TileNode neighbour, int cost)
    {
        //Sets the parent node
        neighbour.Parent = parent;

        //Calculates this nodes g cost, The parents g cost + what it costs to move tot his node
        neighbour.G = parent.G + cost;

        //H is calucalted, it's the distance from this node to the goal * 10
        neighbour.H = ((Math.Abs((neighbour.Position.x - tilePos.x)) + Math.Abs((neighbour.Position.y - tilePos.y))) * 10);

        //F is calcualted 
        neighbour.F = neighbour.G + neighbour.H;
    }


    private TileNode GetNode(Vector3Int position)
    {
        if (allNodes.ContainsKey(position))
        {
            return allNodes[position];
        }
        else
        {
            TileNode node = new TileNode(position);
            allNodes.Add(position, node);
            return node;
        }
    }
    #endregion
}

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
