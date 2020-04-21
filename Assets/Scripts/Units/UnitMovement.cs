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

    [Header("Hex Grid References")]
    public Tilemap tileMap;
    public Tilemap highlightPathMap;
    public Tilemap highlightUnitMap;
    public ATile baseTile;
    public ATile pathTile;
    public ATile playerTile;
    public ATile enemyTile;


    [Header("Other References")]
    public PointerChanger pointerChanger;
    //how fast the unit trabels over the map visually
    public UnitManager unitManager;
    

    [Header("Movement Variables")]
    //for ray casting on click (only check tile map layer)
    public LayerMask RaycastDetectMask;
    // default visual movement speed of units
    public float visualMovementSpeed = .75f;

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


    //Indicator to show if the tile mouse is hovering over has changed
    private bool mouseOverTileChanged = false;
    //last position of tile mouse was hovering over
    private Vector3Int prevMousOverTilePos;



    // Path Finding Variables
    private TileNode current;
    // A Stack is basically a List 'last in first out' so perfect for a set path
    private Stack<Vector3Int> path;
    //checked list
    private HashSet<TileNode> openList;
    private HashSet<TileNode> closedList;
    //we are using HashSets above to avoid duplication of nodes in our lists and help with performance compared to other types of lists
    private Dictionary<Vector3Int, TileNode> allNodes = new Dictionary<Vector3Int, TileNode>();



    private void Start()
    {
       
        UpdateUnitPositions();

        
    }


    void Update()
    {

        if (!Menu.isPaused)
        {
            // If is player's unit turn which has action points
            if (unitManager.gameUnits[unitManager.currUnitTurn].playerControlled && unitManager.actionPoints > 0)
            {
                PlayerTurn();
                //UpdateUnitPositions();
            }
            // else if it's an AI controlled unit's turn
            else if (!unitManager.gameUnits[unitManager.currUnitTurn].playerControlled)
            {
                AITurn();
                //UpdateUnitPositions();
            }
            else
            {
                pointerChanger.SetCursor("Normal");
                highlightPathMap.ClearAllTiles();
                // highlight end turn button
            }            
        }        
    }




    // Updates the highlight/indicator overlay for the units showing th eplayer which units are theirs or enemy
    private void UpdateUnitPositions()
    {
        highlightUnitMap.ClearAllTiles();

        foreach (BaseUnit item in unitManager.gameUnits)
        {
            if (item.playerControlled)
                highlightUnitMap.SetTile(item.currentTilePosition, playerTile);
            else
                highlightUnitMap.SetTile(item.currentTilePosition, enemyTile);
        }
    }


    public bool CheckUnitAtPosition(Vector3Int checkPosition, ref bool isPlayerUnit)
    {        
        bool occupied = false;

       // Debug.Log("Check Unit At Position: " + checkPosition);

        foreach (BaseUnit item in unitManager.gameUnits)
        {
            if (tileMap.WorldToCell(item.transform.position).Equals(checkPosition))
            {
                //Debug.Log("Has Unit player : " + item.playerControlled);
                isPlayerUnit = item.playerControlled;
                occupied = true;
                break;
            }
            //Debug.Log("Unit Position: " + tileMap.WorldToCell(item.transform.position) + " | Check Position: " + checkPosition);
        }
        return occupied;
    }





    private void PlayerTurn()
    {
        cantMoveMore = false;
        
        mousePos = Input.mousePosition;
        tilePos = tileMap.WorldToCell(Camera.main.ScreenToWorldPoint(mousePos));
        ATile mouseOverTile = (ATile)tileMap.GetTile(tilePos);
        //FindNeighbours(origPos);

        if (tilePos != prevMousOverTilePos)
        {
            prevMousOverTilePos = tilePos;
            mouseOverTileChanged = true;
            highlightPathMap.ClearAllTiles();      
        }
        else
            mouseOverTileChanged = false;

        
        if (mouseOverTile != null)
        {
            origPos = tileMap.WorldToCell(unitManager.gameUnits[unitManager.currUnitTurn].transform.position);
            //if mouse button clicked then try and initiate movment
            if (Input.GetMouseButtonDown(0) && !unitIsMoving)
            {
                // Thorw a raycast where player clicked on map help to distinguish whether is was on a tile or UI
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePos), Vector2.zero, Mathf.Infinity);

                if (hit.collider != null)
                {                    
                    current = null;
                    CalculatePath();
                    //Apply movement/attack
                    StartCoroutine(moveViaPath(unitManager.gameUnits[unitManager.currUnitTurn].gameObject));                    
                }
            }
            else if (!unitIsMoving && mouseOverTileChanged)
            {
                // Show path unit will take while hovering
                current = null;
                CalculatePath();
                ShowPathOnMap();
                /* Check if there is a unit on the mouse over tile
                 * if not player controlled then it's an enemy, set the mouse cursor as attack  */
                /*if (CheckUnitAtPosition(tilePos))
                {
                    if (!mouseOverTile.gameObject.GetComponent<BaseUnit>().playerControlled)
                    {
                        //change pointer to Attack                            
                        pointerChanger.SetCursor("MeleeAttack");
                    }
                }*/
            }

        }
        else  // Set cursor to 'Normal' if mouse over tile is null
        {
            highlightPathMap.ClearAllTiles();
            pointerChanger.SetCursor("Normal");
        }
    }


   


    





    #region AIMovement

    private void AITurn()
    {
        pointerChanger.SetCursor("Normal");

        // Do AI Turn
        if (!unitIsMoving)
        {
            // If AI cannot do a ranged attack then

            //Clear any higlighted tiles
            highlightPathMap.ClearAllTiles();

            //Calculate AI movement
            CalculateAIMovement();

            StartCoroutine(moveViaPath(unitManager.gameUnits[unitManager.currUnitTurn].gameObject));

            //unitManager.gameUnits[unitManager.currUnitTurn].currentTilePosition = tileMap.WorldToCell(unitManager.gameUnits[unitManager.currUnitTurn].transform.position);

            //Make sure the unit has finished moving before setting next turn
            if (!unitIsMoving)
            {
                //set unit position

                unitManager.SelectNextUnit();
                cantMoveMore = false;
            }
        }
    }

    private void CalculateAIMovement()
    {        
        //Set position of unit
        origPos = tileMap.WorldToCell(unitManager.gameUnits[unitManager.currUnitTurn].transform.position);
        //Set goal position
        tilePos = tileMap.WorldToCell(unitManager.gameUnits[unitManager.aiController.GetClosestEnemyUnit()].transform.position);
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
            if (unitManager.actionPoints >= ((ATile)tileMap.GetTile(position)).movementCost)
            {
                bool isPlayerUnitOnTile = false;
                bool isUnitOnTile = CheckUnitAtPosition(position, ref isPlayerUnitOnTile);
                

                //If there is no unit on the next tile, 
                if (!isUnitOnTile)
                { // Move the unit to that tile

                    //while unit is not at the next tile, slide the unit to the next tile in path
                    while (objectToMove.transform.position != tileMap.GetCellCenterWorld(position))
                    {
                        // Smoothly move the unit to next point in path
                        objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, tileMap.GetCellCenterWorld(position), visualMovementSpeed * Time.deltaTime);
                        yield return new WaitForEndOfFrame();
                    }

                    //decrease the amount of available action points according to the movement cost of the tile
                    unitManager.actionPoints -= ((ATile)tileMap.GetTile(position)).movementCost;

                    UpdateUnitPositions();
                    yield return new WaitForSeconds(.1f);
                }
                else // there is a unit on tile, check unit and melee attack if an enemy unit, stop moving if player unit
                {
                    Vector3 prevPos = objectToMove.transform.position;

                    //Check if Unit on target tile is friendly unit
                    BaseUnit unitOnTile = unitManager.GetUnitAtPosition(position);
                    
                    //apply a bassic attack movement and do damage to enemy unit

                    //do a quick move halfway toward tile enemy unit is occupying
                    while (objectToMove.transform.position != (tileMap.GetCellCenterWorld(position) + prevPos) / 2)
                    {
                        objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, (tileMap.GetCellCenterWorld(position) + prevPos)/2, (visualMovementSpeed * 3) * Time.deltaTime);
                        yield return new WaitForEndOfFrame();
                    }

                    // apply damage (and effect) to unit
                    unitOnTile.TakeDamage(objectToMove.GetComponent<BaseUnit>().baseAttack);

                    // quickly move back 
                    while (objectToMove.transform.position != prevPos)
                    {
                        objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, prevPos, (visualMovementSpeed * 3) * Time.deltaTime);
                        yield return new WaitForEndOfFrame();
                    }
                    
                    unitManager.actionPoints = 0;
                    
                }

                
                
            }
            else
            {
                //if unit still has an action point available they can attack
                cantMoveMore = !(unitManager.actionPoints > 0);
                                            
                break;
            }

        }
        
        //Check if still unit has action points to attack but can't move more to its destination
        if (!(unitManager.actionPoints > 0))
        {
            cantMoveMore = true;
        }
               
        // set the indicator that the unit is no longer moving
        unitIsMoving = false;
    }


    // Path Finding Functions
    #region PathFinding



    private void Initialise()
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
        // if current tile node is null
        if (current == null)
        {
            // (re)initilise the path variables
            Initialise();
        }

        
        while (openList.Count > 0 && path == null)
        {
            //get any adjacent tiles unit can move to from current tile
            List<TileNode> neighbours = FindNeighbours(current.Position);
                       
            //Check the neighbour tiles of current tile
            ExamineNeighbours(neighbours, current);
            UpdateCurrentTile(ref current);
            path = GeneratePath(current);
        }

        
    }


    void ShowPathOnMap()
    {
        // Iterate through the calculated path and show path on map for player to see
        if (path != null)
        {
            int tempAP = unitManager.actionPoints;
            // iterate through path nodes and set the 'highlight' overlay grid nodes to highlight hex tile
            int range = 0;
            int nextTileMoveCost;
            ATile nextTile;

            foreach (Vector3Int position in path)
            {
                range++;
                //check available action p[oints for unit against the movement cost of the next tile

                nextTile = (ATile)tileMap.GetTile(position);
                if (unitManager.gameUnits[unitManager.currUnitTurn].canFly)
                    nextTileMoveCost = 1;
                else
                    nextTileMoveCost = nextTile.movementCost;

                // ########################################  IF TIME PERMITS
                // move this to a place where it only runs once and not for every tile in the path 
                // will need to change how the neighbour tiles are checked and re-work attack system so unit still moves to neigbour tile if melee
                // set the mouse pointer
                bool playerUnitOnTile = false;
                //Debug.Log("AP: " + tempAP + " | NextTile AP: " + nextTileMoveCost + " | isWalkable: " + nextTile.isWalkable + " | canFly: " + unitManager.gameUnits[unitManager.currUnitTurn].canFly + " | unit At Position " + CheckUnitAtPosition(position, ref playerUnitOnTile));

                if (tempAP < nextTileMoveCost)
                {
                    pointerChanger.SetCursor("Invalid");
                    break;
                }
                else if (!unitManager.gameUnits[unitManager.currUnitTurn].canFly && !nextTile.isWalkable)
                {
                    pointerChanger.SetCursor("Invalid");
                    break;
                }
                else if (CheckUnitAtPosition(position, ref playerUnitOnTile))
                {
                    //Debug.Log("Unit is at mouse position");
                    // check if unit is a player unit
                    if (unitManager.gameUnits[unitManager.currUnitTurn].playerControlled != playerUnitOnTile)
                    { 
                        if (unitManager.gameUnits[unitManager.currUnitTurn].baseAttackRange >= range)
                            pointerChanger.SetCursor("RangedAttack");
                        else
                            pointerChanger.SetCursor("MeleeAttack");
                    }
                    else
                    {
                        pointerChanger.SetCursor("Invalid");
                    }
                    break;
                }
                else
                {                    
                    highlightPathMap.SetTile(position, pathTile);
                    pointerChanger.SetCursor("Move");                    
                    //break;
                } 

                tempAP -= nextTileMoveCost;
            }
        }
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
        Vector3Int tempPosition;
        bool isPlayerUnit = false;
        bool unitOnTile = false;

        //RIGHT TILE: Get the Tile to the right of current tile and add to neighbour  list if a viable tile
        tempPosition = new Vector3Int(tilePosition.x + 1, tilePosition.y, 0);
        tempTile = (ATile)tileMap.GetTile(tempPosition);
        unitOnTile = CheckUnitAtPosition(tempPosition, ref isPlayerUnit);
        //Check if there is a tile there
        if (tempTile != null)
        {
            // check if tile is walkable or the current unit can fly
            if (tempTile.isWalkable || unitManager.gameUnits[unitManager.currUnitTurn].canFly)
            {
                //check if unit is on the tile
                if (unitOnTile)
                {
                    // if unit is not same side then add tile to list (can be attacked)
                    if (unitManager.gameUnits[unitManager.currUnitTurn].playerControlled != isPlayerUnit)
                    {
                        //add tile to our viable neighbour tile list
                        TileNode neighbour = GetNode(tempPosition);
                        neighbours.Add(neighbour);
                    }
                }
                else
                {
                    //add tile to our viable neighbour tile list
                    TileNode neighbour = GetNode(tempPosition);
                    neighbours.Add(neighbour);
                }
            }
        }

        //LEFT TILE: Get the Tile to the left of current tile and add to neighbour  list if a viable tile        
        tempPosition = new Vector3Int(tilePosition.x - 1, tilePosition.y, 0);
        tempTile = (ATile)tileMap.GetTile(tempPosition);
        unitOnTile = CheckUnitAtPosition(tempPosition, ref isPlayerUnit);
        //Check if there is a tile there
        if (tempTile != null)
        {
            // check if tile is walkable or the current unit can fly
            if (tempTile.isWalkable || unitManager.gameUnits[unitManager.currUnitTurn].canFly)
            {
                //check if unit is on the tile
                if (unitOnTile)
                {
                    // if unit is not same side then add tile to list as the tile can be attacked
                    if (unitManager.gameUnits[unitManager.currUnitTurn].playerControlled != isPlayerUnit)
                    {
                        //add tile to our viable neighbour tile list
                        TileNode neighbour = GetNode(tempPosition);
                        neighbours.Add(neighbour);
                    }                    
                }
                else
                {
                    //add tile to our viable neighbour tile list
                    TileNode neighbour = GetNode(tempPosition);
                    neighbours.Add(neighbour);
                }
            }
        }


        //UPRIGHT TILE: Get the Tile up and to the right of current tile and add to neighbour list if a viable tile
        tempPosition = new Vector3Int(tilePosition.x + hexTileOffset, tilePosition.y + 1, 0);
        tempTile = (ATile)tileMap.GetTile(tempPosition);
        unitOnTile = CheckUnitAtPosition(tempPosition, ref isPlayerUnit);
        //Check if there is a tile there
        if (tempTile != null)
        {
           // check if tile is walkable or the current unit can fly 
            if (tempTile.isWalkable || unitManager.gameUnits[unitManager.currUnitTurn].canFly)
            {
                //check if unit is on the tile
                if (unitOnTile)
                {                    
                    // if unit is not same side then add tile to list as the tile can be attacked
                    if (unitManager.gameUnits[unitManager.currUnitTurn].playerControlled != isPlayerUnit)
                    {                 
                        //add tile to our viable neighbour tile list
                        TileNode neighbour = GetNode(tempPosition);
                        neighbours.Add(neighbour);
                    }                    
                }
                else
                {
                    //add tile to our viable neighbour tile list
                    TileNode neighbour = GetNode(tempPosition);
                    neighbours.Add(neighbour);
                }
            }
        }

        //UPLEFT TILE: Get the Tile up and to the left of current tile and add to neighbour list if a viable tile
        tempPosition = new Vector3Int(tilePosition.x - 1 + hexTileOffset, tilePosition.y + 1, 0);
        tempTile = (ATile)tileMap.GetTile(tempPosition);
        unitOnTile = CheckUnitAtPosition(tempPosition, ref isPlayerUnit);
        //Check if there is a tile there
        if (tempTile != null)
        {            
            // check if tile is walkable or the current unit can fly
            if (tempTile.isWalkable || unitManager.gameUnits[unitManager.currUnitTurn].canFly)
            {
                //check if unit is on the tile
                if (unitOnTile)
                {
                    // if unit is not same side then add tile to list as the tile can be attacked
                    if (unitManager.gameUnits[unitManager.currUnitTurn].playerControlled != isPlayerUnit)
                    {
                        //add tile to our viable neighbour tile list
                        TileNode neighbour = GetNode(tempPosition);
                        neighbours.Add(neighbour);
                    }
                }
                else
                {
                    //add tile to our viable neighbour tile list
                    TileNode neighbour = GetNode(tempPosition);
                    neighbours.Add(neighbour);
                }
            }
        }


        //DOWNLEFT TILE: Get the Tile up and to the right of current tile and add to neighbour list if a viable tile
        tempPosition = new Vector3Int(tilePosition.x - 1 + hexTileOffset, tilePosition.y - 1, 0);
        tempTile = (ATile)tileMap.GetTile(tilePosition);
        unitOnTile = CheckUnitAtPosition(tempPosition, ref isPlayerUnit);
        //Check if there is a tile there
        if (tempTile != null)
        {
            // check if tile is walkable or the current unit can fly 
            if (tempTile.isWalkable || unitManager.gameUnits[unitManager.currUnitTurn].canFly)
            {
                //check if unit is on the tile
                if (unitOnTile)
                {
                    // if unit is not same side then add tile to list as the tile can be attacked
                    if (unitManager.gameUnits[unitManager.currUnitTurn].playerControlled != isPlayerUnit)
                    {
                        //add tile to our viable neighbour tile list
                        TileNode neighbour = GetNode(tempPosition);
                        neighbours.Add(neighbour);
                    }
                }
                else
                {
                    //add tile to our viable neighbour tile list
                    TileNode neighbour = GetNode(tempPosition);
                    neighbours.Add(neighbour);
                }
            }
        }

        //DOWNRIGHT TILE: Get the Tile up and to the left of current tile and add to neighbour list if a viable tile
        tempPosition = new Vector3Int(tilePosition.x + hexTileOffset, tilePosition.y - 1, 0);
        tempTile = (ATile)tileMap.GetTile(tempPosition);
        unitOnTile = CheckUnitAtPosition(tempPosition, ref isPlayerUnit);
        //Check if there is a tile there
        if (tempTile != null)
        {
            // check if tile is walkable or the current unit can fly
            if (tempTile.isWalkable || unitManager.gameUnits[unitManager.currUnitTurn].canFly)
            {
                //check if unit is on the tile
                if (unitOnTile)
                {
                    // if unit is not same side then add tile to list as the tile can be attacked
                    if (unitManager.gameUnits[unitManager.currUnitTurn].playerControlled != isPlayerUnit)
                    {
                        //add tile to our viable neighbour tile list
                        TileNode neighbour = GetNode(tempPosition);
                        neighbours.Add(neighbour);
                    }
                }
                else
                {
                    //add tile to our viable neighbour tile list
                    TileNode neighbour = GetNode(tempPosition);
                    neighbours.Add(neighbour);
                }
            }
        }

        //Debug.Log("From Tile: " + tilePosition);
        //DebugNeighbours(neighbours);
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

            //gScore *= 2;

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
            node.G = 5;
            allNodes.Add(position, node);
            return node;
        }
    }
    #endregion
}


