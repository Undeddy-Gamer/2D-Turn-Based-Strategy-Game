using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;


public class UnitMovement : MonoBehaviour
{

    public Tilemap tileMap;
    public ATile baseTile;
    

    // current unit position
    public Vector3 currentPos;
    // pos position on click
    public Vector3 mousePos;
    // goal tile position
    public Vector3Int tilePos;

    
    // positions of adjacent tile to the current unit tile 
    private Vector3Int tileLeft;
    private Vector3Int tileUpLeft;
    private Vector3Int tileDownLeft;
    private Vector3Int tileRight;
    private Vector3Int tileUpRight;
    private Vector3Int tileDownRight;

    // remember to change back to private
    public int[][] tileMesh;

    //how fast the unit trabels over the map visually
    public float visualMovementSpeed = .75f;

    //Pathfinding
    //Meta defining play here

    public int teamNum;
    public int x;
    public int y;

    //public int actionPoints;

    /* 
    public Queue<int> movementQueue;
    public Queue<int> combatQueue;

    public List<Node> path = null;

    //Path for moving unit's transform
    public List<Node> pathForMovement = null;
    public bool completedMovement = false;
    */



    //for ray casting on click (only check tile map layer)
    [SerializeField]
    private LayerMask mask;

    // Path Finding Variables
    private Node current;
    private Stack<Vector3Int> path;
    private HashSet<Node> openList;
    private HashSet<Node> closedList;
    private Dictionary<Vector3Int, Node> allNodes = new Dictionary<Vector3Int, Node>();



    private void Start()
    {
        
    }


    void Update()
    {
        if (UnitManager.gameUnits[UnitManager.currUnitTurn].playerControlled & UnitManager.actionPoints > 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                mousePos = Input.mousePosition;
                tilePos = tileMap.WorldToCell(Camera.main.ScreenToWorldPoint(mousePos));

                //Thorws a raycast in the direction of the target
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePos), Vector2.zero, Mathf.Infinity, mask);

                if (hit.collider != null)
                {

                    Debug.Log("HIT");
                    /*
                    *   Calculate pathfinding and apply
                    *
                    *   Insert Code Here
                    *
                    */

                    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector3Int clickPos = tileMap.WorldToCell(mouseWorldPos);

                    // Temporary movment code
                    // jumpt to mouse click
                    StartCoroutine(moveOverSeconds(UnitManager.gameUnits[UnitManager.currUnitTurn].gameObject, tilePos));

                    // get cselected tile 
                    ATile aTile = (ATile)tileMap.GetTile(tilePos);
                    //apply tile movement cost to available action points
                    UnitManager.actionPoints -= aTile.movementSpeed;

                    
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


    // do we need this?
    void GetAdjacentTiles()
    {
        tileRight = tileMap.WorldToCell(new Vector3(this.transform.position.x + 1, this.transform.position.y));
        tileMap.SetTile(tileRight, baseTile);

        tileUpRight = tileMap.WorldToCell(new Vector3(this.transform.position.x + .5f, this.transform.position.y + .75f));
        tileMap.SetTile(tileUpRight, baseTile);        
    }




    #endregion
}
