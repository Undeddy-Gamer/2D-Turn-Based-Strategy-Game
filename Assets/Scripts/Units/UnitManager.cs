using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [Header("Current Unit Variables")]
    public int currUnitTurn;
    public int actionPoints;
    //public BaseUnit currentUnit;
    private int prevAP = 0;
    private float prevHealth = 0;

    [Space(5), Header("UI References")]
    public Text unitHealth;
    public Text unitAP;
    public Text unitRange;
    public Text unitDamage;
    public Image unitIcon;

    [Space(5), Header("Manager References")]
    // List of Units on the map, ordered by which one goes first (initiation)
    public List<BaseUnit> gameUnits;
    public List<Vector3Int> unitPositions;    
    public TurnManager turnManager;
    public AIUnitController aiController;
    public GameObject unitContainer;
    public Tilemap landTilemap;

    

    void Awake()
    {
        //place Units?

        // setup placed units
        InitiliseUnits();

    }

    private void Update()
    {
        //currentUnit = gameUnits[currUnitTurn];

        if (actionPoints != prevAP || gameUnits[currUnitTurn].currentHealth != prevHealth)
        {
            UpdateUnitUI();
        }
    }

    // Algorithim to sort units by initiation
    public int SortByInitiation(BaseUnit u1, BaseUnit u2)
    {
        return u2.baseInitiation.CompareTo(u1.baseInitiation);
    }

    private void InitiliseUnits()
    {

        //Create a list of all units on the battlefield
        gameUnits = new List<BaseUnit>();        
        // Iterate through the units on the map and set them up
        foreach (BaseUnit unit in unitContainer.GetComponentsInChildren<BaseUnit>())
        {
            if (unit != null)
            {
                unit.currentTilePosition = landTilemap.WorldToCell(unit.gameObject.transform.position);
                gameUnits.Add(unit);                
            }            
        }

        // Sort unit turns by their Initiation score (highest First)
        gameUnits.Sort(SortByInitiation);

        // set unit with initial highest initiation to active
        gameUnits[0].active = true;
        currUnitTurn = 0;

        // set current actionpoints for current unit
        actionPoints = gameUnits[currUnitTurn].baseMovementSpeed;

        // update UI
        UpdateUnitUI();
    }


    public void SelectNextUnit()
    {
        //deactivate current Unit
        gameUnits[currUnitTurn].active = false;

        // Set Next unit's Turn
        // if the number of units in the unit list is greater than the current unit's index
        if (currUnitTurn < gameUnits.Count - 1)
            currUnitTurn++; // increase the index for current unit by 1
        else
            currUnitTurn = 0; // restart at the begining of the available units
                
        // Set next unit to active
       gameUnits[currUnitTurn].active = true;
        // Update available action points
        actionPoints = gameUnits[currUnitTurn].baseMovementSpeed;
        
        // Set player turn in turn manager
        if (gameUnits[currUnitTurn].playerControlled)
            TurnManager.PlayerTurn = true;
        else
            TurnManager.PlayerTurn = false;

        UpdateUnitUI();        
    }

    public BaseUnit CurrentActiveUnit()
    {
        return gameUnits[currUnitTurn];
    }

    private void UpdateUnitUI()
    {
        unitIcon.sprite = gameUnits[currUnitTurn].transform.GetComponent<SpriteRenderer>().sprite;
        //unitStats.text = "A/P: " + actionPoints + "\nHealth: " + gameUnits[currUnitTurn].currentHealth + "\nDamage: " + gameUnits[currUnitTurn].baseAttack + "\nRange: " + gameUnits[currUnitTurn].baseAttackRange;
        unitHealth.text = ": " + gameUnits[currUnitTurn].currentHealth;
        unitAP.text = ": " + actionPoints;
        unitDamage.text = ": " + gameUnits[currUnitTurn].baseAttack;
        unitRange.text = ": " + gameUnits[currUnitTurn].baseAttackRange;

        prevAP = actionPoints;
        prevHealth = gameUnits[currUnitTurn].currentHealth;
    }


    public BaseUnit GetUnitAtPosition(Vector3Int checkPosition)
    {
        BaseUnit unit = null;

        foreach (BaseUnit item in gameUnits)
        {
            if (landTilemap.WorldToCell(item.transform.position).Equals(checkPosition))
            {
                unit = item;
                break;
            }
        }

        return unit;
    }

    



}
