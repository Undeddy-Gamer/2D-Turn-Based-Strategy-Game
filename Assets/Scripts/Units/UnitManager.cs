using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    
    public static int currUnitTurn;
    public static int actionPoints;

    // List of Units on the map, ordered by which one goes first (initiation)  
    public static List<BaseUnit> gameUnits;

    public GameObject gameWorld;

    void Awake()
    {

        //Create a list of all units on the battlefield
        gameUnits = new List<BaseUnit>();
        foreach (BaseUnit unit in gameWorld.GetComponentsInChildren<BaseUnit>())
        {
            if (unit != null)
            { 
                gameUnits.Add(unit);
            }
        }
        
        // Sort unit turns by their Initiation score (highest First)
        gameUnits.Sort(SortByInitiation);
        
        // set unit with initial highest initiation to active
        gameUnits[0].active = true;
        currUnitTurn = 0;

        actionPoints = gameUnits[currUnitTurn].baseMovementSpeed;
       
    }

    // Algorithim to sort units by initiation
    static int SortByInitiation(BaseUnit u1, BaseUnit u2)
    {
        return u2.baseInitiation.CompareTo(u1.baseInitiation);
    }

    public static void SelectNextUnit()
    {
        //deactivate current Unit
        gameUnits[currUnitTurn].active = false;

        // Set Next unit's Turn
        // if the number of units in the unit list is greater than the current unit's index
        if (gameUnits.Count > currUnitTurn + 1)
            currUnitTurn++; // increase the index for current by 1
        else
            currUnitTurn = 0; // restart at the begining of the available units

        // set next unit to active
        gameUnits[currUnitTurn].active = true;
        //update available action points
        actionPoints = gameUnits[currUnitTurn].baseMovementSpeed;
        
        // set player turn in turn manager???
        if (gameUnits[currUnitTurn].playerControlled)
            TurnManager.PlayerTurn = true;
        else
            TurnManager.PlayerTurn = false;
    }



 }
