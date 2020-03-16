using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIUnitController : MonoBehaviour
{
    BaseUnit unit;

    public float moveTimer = .5f;
    public TurnManager turn;

    void Start()
    {
        //unit = units[currU]
    }

    // Update is called once per frame
    void Update()
    {
        if (unit.active)
        {
            if (!unit.playerControlled)
            {
                CalculateAITurn();
            }

        }
    }

    public static void CalculateAITurn()
    {
        //Testing
        Debug.Log("AI Unit Turn: " + UnitManager.gameUnits[UnitManager.currUnitTurn].name);
        UnitManager.gameUnits[UnitManager.currUnitTurn].active = false;
        UnitManager.actionPoints++;

        if (UnitManager.actionPoints >= UnitManager.gameUnits[UnitManager.currUnitTurn].baseMovementSpeed)        
        {
            UnitManager.SelectNextUnit();
        }


        // Do AI Turn
    }
}
