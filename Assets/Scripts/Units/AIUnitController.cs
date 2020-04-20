using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIUnitController : MonoBehaviour
{
       
    public UnitManager unitManager;

    //bool canAttack = false;

    
    public void CalculateAITurn()
    {
        
        
    }

    public int GetClosestEnemyUnit()
    {
        int unitToAttack = -1;

        for (int i = 0; i < unitManager.gameUnits.Count - 1; i++)
        {
            if (unitManager.gameUnits[i].playerControlled)
            {
                unitToAttack = i;
                //break;
            }
        }

        return unitToAttack;
    }
}
