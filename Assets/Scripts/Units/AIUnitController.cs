using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIUnitController : MonoBehaviour
{
       
    public TurnManager turn;

    bool canAttack = false;
    
    public static void CalculateAITurn()
    {
        
        
    }

    public static int GetClosestEnemyUnitPosition()
    {
        int unitToAttack = -1;

        for (int i = 0; i < UnitManager.gameUnits.Count - 1; i++)
        {
            if (UnitManager.gameUnits[i].playerControlled)
            {
                unitToAttack = i;
                //break;
            }
        }

        return unitToAttack;
    }
}
