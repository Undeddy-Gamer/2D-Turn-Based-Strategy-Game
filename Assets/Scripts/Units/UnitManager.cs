﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    
    public static int currUnitTurn;
    public static int actionPoints;

    // List of Units on the map, ordered by which one goes first     
    public static List<BaseUnit> gameUnits;

    public GameObject gameWorld;

    void Awake()
    {
        gameUnits = new List<BaseUnit>();
        foreach (BaseUnit unit in gameWorld.GetComponentsInChildren<BaseUnit>())
        {
            if (unit != null)
            { 
                gameUnits.Add(unit);
                //units[i] = unit;
                //Debug.Log(unit.gameObject.name + " : " + unit.baseAttack);
            }

        }
        gameUnits[0].active = true;
        currUnitTurn = 0;

        actionPoints =gameUnits[currUnitTurn].baseMovementSpeed;

        Debug.Log("Number of Units: " + gameUnits.Count);
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
    }



 }
