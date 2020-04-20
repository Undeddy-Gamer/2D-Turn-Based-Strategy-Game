using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitCombat : MonoBehaviour    
{
    public UnitMovement unitMovement;

    //game object refers to both the attacker and the defendor


    //deal damage to defendor based on attacker base damage + modifers
    //apply status efffects to the defender
    //destroy defender if health less then 1
    // Update is called once per frame
    void Update()
    {
        
    }

    public void MeleeAttack(ref GameObject AttactingUnit, ref GameObject RecievingUnit)
    {
        // do combat animation

        // calculate damages applied

        // apply damage
    }


}
