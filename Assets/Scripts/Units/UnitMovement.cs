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
    public Tilemap highlightMap;
    public ATile baseTile;
    public ATile pathTile;

    //UnitManager unitManager 

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

    //public int teamNum;
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

    public bool unitIsMoving = false;
    // tells us if the unit is stopped because the next tile costs more AP than it has available
    public bool cantMoveMore = false;

    private void Start()
    {
        
    }


    void Update()
    {

        if (!Menu.isPaused)
        {            
            if (UnitManager.gameUnits[UnitManager.currUnitTurn].playerControlled && UnitManager.actionPoints > 0)
            {
                cantMoveMore = false;
                
                origPos = tileMap.WorldToCell(UnitManager.gameUnits[UnitManager.currUnitTurn].transform.position);
                mousePos = Input.mousePosition;
                tilePos = tileMap.WorldToCell(Camera.main.ScreenToWorldPoint(mousePos));

                if (Input.GetMouseButtonDown(0) && !unitIsMoving)
                {
                    
                    //Thorws a raycast in the direction of the target
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePos), Vector2.zero, Mathf.Infinity, mask);

                    if (hit.collider != null)
                    {
                        current = null;
                        CalculatePath(false);

                        //Apply movement
                        StartCoroutine(moveViaPath(UnitManager.gameUnits[UnitManager.currUnitTurn].gameObject));                        
                        
                    }
                }
                else if (!unitIsMoving)
                {
                    // Show path unit will take while hovering
                    current = null;
                    highlightMap.ClearAllTiles();
                    CalculatePath(false);
                }
            }
            // else if it's an AI controlled unit's turn
            else if (!UnitManager.gameUnits[UnitManager.currUnitTurn].playerControlled) 
            {
                // Do AI Turn

                

                // If AI cannot attack then
                Debug.Log("AI Unit ID : " + UnitManager.currUnitTurn);
                Debug.Log("AI Unit Name : " + UnitManager.gameUnits[UnitManager.currUnitTurn].name);
                Debug.Log("Action Points: " + UnitManager.actionPoints);
                Debug.Log("Closests Enemy Position ID : " + AIUnitController.GetClosestEnemyUnitPosition());
                Debug.Log("Unit Count: " + UnitManager.gameUnits.Count);
               
                //Clear any higlighted tiles
                highlightMap.ClearAllTiles();
                //Calculate AI movement
                CalculateAIMovement();                    
                StartCoroutine(moveViaPath(UnitManager.gameUnits[UnitManager.currUnitTurn].gameObject));

                
                //UnitManager.SelectNextUnit();
                //cantMoveMore = false;


                //else
                //AI Unit Attack
            }                
            //// else if is AI controlled unit's turn and has no more action points
            //else if (!UnitManager.gameUnits[UnitManager.currUnitTurn].playerControlled && cantMoveMore) 
            //{
            //    //Initiate next unit turn
            //    cantMoveMore = false;
            //    UnitManager.SelectNextUnit();
            //}
            // else wait for player to click end turn button
        }


    }


    #region AIMovement

    private void CalculateAIMovement()
    {        
        if (!cantMoveMore)
        {
            //
            origPos = tileMap.WorldToCell(UnitManager.gameUnits[UnitManager.currUnitTurn].transform.position);
            tilePos = tileMap.WorldToCell(UnitManager.gameUnits[AIUnitController.GetClosestEnemyUnitPosition()].transform.position);
            current = null;
            CalculatePath(false);
        }
        else
            cantMoveMore = false;
            UnitManager.SelectNextUnit();
    }

    

    #endregion



    public IEnumerator moveViaPath(GameObject objectToMove)
    {
        unitIsMoving = true;

        foreach (Vector3Int position in path)
        {       
            if (UnitManager.actionPoints >= ((ATile)tileMap.GetTile(position)).movementCost)
            {
                while (objectToMove.transform.position != tileMap.GetCellCenterWorld(position))
                {
                    objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, tileMap.GetCellCenterWorld(position), visualMovementSpeed * Time.deltaTime);
                    yield return new WaitForEndOfFrame();
                }
                UnitManager.actionPoints -= ((ATile)tileMap.GetTile(position)).movementCost;
                yield return new WaitForSeconds(.1f);
            }
            else
            { 
                cantMoveMore = true;
                unitIsMoving = false;
                break;
            }
        }

        if (UnitManager.actionPoints <= 0 || !UnitManager.gameUnits[UnitManager.currUnitTurn].playerControlled)
        {
            cantMoveMore = true;
        }

        unitIsMoving = false;
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


    public void CalculatePath(bool step)
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
            int tempAP = UnitManager.actionPoints;
            
            foreach (Vector3Int position in path)
            {
                int nextTileMoveCost = ((ATile)tileMap.GetTile(position)).movementCost;
                tempAP -= nextTileMoveCost;
                //Debug.Log("AP Available: " + tempAP);
                
                highlightMap.SetTile(position, pathTile);
                
                
                if (tempAP < nextTileMoveCost)
                {                    
                    break;
                }
            }
        }
        //else return null;
    }




    
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

            int gScore = ((ATile)tileMap.GetTile(neighbour.Position)).movementCost;

            // Take an Educated guess which will be the fastest way to goal
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



    private Stack<Vector3Int> GeneratePath(TileNode current)
    {
        if (current.Position == tilePos) //If the current node is the goal tile position, then we found a path ... WOOHOO!
        {
            //Creates a stack to contain the final path
            Stack<Vector3Int> finalPath = new Stack<Vector3Int>();

            //Add the nodes to a final path list
            while (current.Position != origPos )
            {       
                    //add the current Node to the path stack
                    finalPath.Push(current.Position);
                    //set the current node to the parent node, whis will retrace the steps back to original position and create the final path
                    current = current.Parent;  
            }

            //Returns the complete path
            return finalPath;
        }

        return null;
    }


    //Calculates teh values we will use to (as efficiently as possible) guess and check the next nodes to check
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
            node.G = 5;
            allNodes.Add(position, node);
            return node;
        }
    }
    #endregion
}


