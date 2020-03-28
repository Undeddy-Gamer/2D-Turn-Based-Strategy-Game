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

    [Header("Hex Grid Variables")]
    public Tilemap tileMap;
    public Tilemap highlightPathMap;
    public Tilemap highlightUnitMap;
    public ATile baseTile;
    public ATile pathTile;
    public ATile playerTile;
    public ATile enemyTile;
    //for ray casting on click (only check tile map layer)
    [SerializeField]
    private LayerMask RaycastDetectMask;


    [Header("Movement Variables")]
    //change below back to private after debugging complete
    // original current unit position
    public Vector3Int origPos;
    // position on mouse click (move to or attack if tile is occupied by enemy)
    public Vector3 mousePos;
    // goal tile position
    public Vector3Int tilePos;
    // indicates wether a unit is currently moving
    public bool unitIsMoving = false;
    // tells us if the unit is stopped because the next tile costs more AP than it has available
    public bool cantMoveMore = false;


    [Header("Misc Variables")]
    public PointerChanger pointerChanger;    
    //how fast the unit trabels over the map visually
    public float visualMovementSpeed = .75f;

    

   

    // Path Finding Variables
    //bool first = true;

    private TileNode current;
    // Node holds all required information we need about a tile to help calculate the fastest available path
    private TileNode currentNode; 
    // A Stack is basically a List 'last in first out' so perfect for a set path
    private Stack<Vector3Int> path;
    
    //checked list
    private HashSet<TileNode> openList;
    private HashSet<TileNode> closedList;
    //we are using HashSets above to avoid duplication of nodes in our lists and help with performance compared to other types of lists

    private Dictionary<Vector3Int, TileNode> allNodes = new Dictionary<Vector3Int, TileNode>();

    

    //private int tempAP;
    //Indicator to show if the tile mouse is hovering over has changed
    private bool mouseOverTileChanged = false;
    private Vector3Int prevMousOverTilePos;
    
    private void Start()
    {
        //HighLightUnits();
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
                HighLightUnits();

                if (tileMap.GetTile(tilePos) != null)
                {
                    if (tilePos != prevMousOverTilePos)
                    {
                        prevMousOverTilePos = tilePos;
                        mouseOverTileChanged = true;
                        highlightPathMap.ClearAllTiles();                        
                    }
                    else
                        mouseOverTileChanged = false;

                    if (Input.GetMouseButtonDown(0) && !unitIsMoving)
                    {
                        //Thorws a raycast where player clicked on map checking only the tilemap (hitDetectMask)
                        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePos), Vector2.zero, Mathf.Infinity, RaycastDetectMask);

                        if (hit.collider != null)
                        {
                            current = null;
                            CalculatePath();
                            //Apply movement
                            StartCoroutine(moveViaPath(UnitManager.gameUnits[UnitManager.currUnitTurn].gameObject));
                            HighLightUnits();
                        }
                    }
                    else if (!unitIsMoving && mouseOverTileChanged)
                    {
                        // Show path unit will take while hovering

                        current = null;

                        CalculatePath();
                    }
                }
                else
                    pointerChanger.SetCursor("Normal");
                
            }
            // else if it's an AI controlled unit's turn
            else if (!UnitManager.gameUnits[UnitManager.currUnitTurn].playerControlled) 
            {
                // Do AI Turn
                if (!unitIsMoving)
                {
                    // If AI cannot attack then
                    /*
                    

                    */


                    //Clear any higlighted tiles
                    highlightPathMap.ClearAllTiles();
                    
                    //Calculate AI movement
                    CalculateAIMovement();

                    
                    StartCoroutine(moveViaPath(UnitManager.gameUnits[UnitManager.currUnitTurn].gameObject));
                    HighLightUnits();
                    //Make sure the unit has finished moving before setting next turn
                    if (!unitIsMoving)
                    {
                        UnitManager.SelectNextUnit();
                        cantMoveMore = false;
                    }
                }
                

                //else
                //AI Unit Attack
            }
            else
            {
                highlightPathMap.ClearAllTiles();
                
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


    private void HighLightUnits()
    {
        highlightUnitMap.ClearAllTiles();
        foreach (BaseUnit item in UnitManager.gameUnits)
        {
            Vector3Int tempPos = highlightUnitMap.WorldToCell(item.transform.position);

            if (item.playerControlled)
                highlightUnitMap.SetTile(tempPos, playerTile);            
            else
                highlightUnitMap.SetTile(tempPos, enemyTile);

        }
    }


    #region AIMovement

    private void CalculateAIMovement()
    {        
        //Set position of unit
        origPos = tileMap.WorldToCell(UnitManager.gameUnits[UnitManager.currUnitTurn].transform.position);
        //Set goal position
        tilePos = tileMap.WorldToCell(UnitManager.gameUnits[AIUnitController.GetClosestEnemyUnitPosition()].transform.position);
        //reset path finding 
        current = null;
        //calculate path
        CalculatePath();
    }
    
    #endregion



    public IEnumerator moveViaPath(GameObject objectToMove)
    {
        unitIsMoving = true;

        // Move unit one tile at a time toward its destination via caculated path
        foreach (Vector3Int position in path)
        {       
            //check each movement against available action points
            if (UnitManager.actionPoints >= ((ATile)tileMap.GetTile(position)).movementCost)
            {
                //while unit is not at next tile, slide the unit to the next tile in path
                while (objectToMove.transform.position != tileMap.GetCellCenterWorld(position))
                {
                    // Smoothly move the unit to next point in path
                    objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, tileMap.GetCellCenterWorld(position), visualMovementSpeed * Time.deltaTime);
                    yield return new WaitForEndOfFrame();
                }

                //decrease the amount of available action points according to the movement cost of the tile
                UnitManager.actionPoints -= ((ATile)tileMap.GetTile(position)).movementCost;
                yield return new WaitForSeconds(.1f);
            }
            else
            {
                //if unit still has an action point available they can attack
                if (!(UnitManager.actionPoints > 0))
                {
                    cantMoveMore = true;
                }
                // set the indicator that the unit is no longer moving
                unitIsMoving = false;
                
                break;
            }
        }
        //Check if still unit has action points to attack but can't move more to its destination
        if (!(UnitManager.actionPoints > 0))
        {
            cantMoveMore = true;
        }

        //if (UnitManager.actionPoints <= 0 || !UnitManager.gameUnits[UnitManager.currUnitTurn].playerControlled)
        //{
        //    cantMoveMore = true;
        //}

        // set the indicator that the unit is no longer moving
        unitIsMoving = false;
    }

        // Path Finding Code

    #region PathFinding
        

    private void Initialize()
    {
        // get  the original start position node
        current = GetNode(origPos);

        //Create a new 'open list' for nodes to check later if required
        openList = new HashSet<TileNode>();

        //Creates a 'closed list' for nodes that we have checked
        closedList = new HashSet<TileNode>();
        
        //Adds the current node to the open list
        openList.Add(current);

        path = null;
        //first = false;
    }


    public void CalculatePath()
    {

        if (current == null)
        {
            Initialize();
        }

        while (openList.Count > 0 && path == null)
        {
            List<TileNode> neighbours = FindNeighbours(current.Position);

            //Debug.Log("neighbours: " + neighbours.Count);

            ExamineNeighbours(neighbours, current);
            UpdateCurrentTile(ref current);
            path = GeneratePath(current);

            //DebugNeighbours(neighbours);
        }


        // show path on map for player to see
        if (path != null)
        {
            int tempAP = UnitManager.actionPoints;
            // iterate through path nodes and set the 'highlight' overlay grid nodes to highlight hex tile
            foreach (Vector3Int position in path)
            {
                //check available action p[oints for unit against the movement cost of the next tile
                int nextTileMoveCost = ((ATile)tileMap.GetTile(position)).movementCost;
                //highlight path so player can see it on the map
                if (tempAP < nextTileMoveCost)
                {
                    pointerChanger.SetCursor("Invalid");
                    break;
                }
                else 
                {
                    highlightPathMap.SetTile(position, pathTile);
                    pointerChanger.SetCursor("Move");
                }

                tempAP -= nextTileMoveCost;
            }
        }
        //else return null;
    }



    void DebugNeighbours(List<TileNode> neighbours)
    {
        foreach (TileNode item in neighbours)
        {
            Debug.Log("Tile: " + item.Position + " | G:" + item.G + " H:" + item.H + " F:" + item.F);
        }
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

            //Hex tile offset fix for cacluating the numbers for next tile to go to
            if (current.Position.y % 2 == 0)
            {
                if (!(neighbour.Position.y % 2 == 0))
                { 
                    gScore -= 1;
                }
            }
            else
            {
                if (neighbour.Position.y % 2 == 0)
                {
                    gScore -= 1;
                }
            }
            

            // Take an Educated guess which will be the fastest way to goal using tile to goal 'as the crow flies' distance and tile move value
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


    //Calculates teh values we will use to (as efficiently as possible) guess the next nodes to check
    private void CalcValues(TileNode parent, TileNode neighbour, int cost)
    {
        //Sets the parent node
        neighbour.Parent = parent;
        //Calculates this nodes g cost, The parents g cost + what it costs to move to this tile
        neighbour.G = parent.G + cost;
        //H is calucalted (the distance from this node to the goal * 10)
        neighbour.H = ((Math.Abs((neighbour.Position.x - tilePos.x)) + Math.Abs((neighbour.Position.y - tilePos.y))) * 2);
        //F is both numbers above together
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
            node.G = 1;
            allNodes.Add(position, node);
            return node;
        }
    }
    #endregion
}


