using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public int turnNumber = 1;
    public string whosTurnIsitAnyway;
    public Text turnText;

    public static float TurnEnds = 1;
    public static bool PlayerTurn;
    public Button nextTurnButton;

    public UnitManager unitManager;

        
    public void NextTurn()
    {
        Debug.Log("END TURN CLICKED");        

        turnNumber += 1;
        TurnEnds += 1;

        //unitManager manages player turn bool
        unitManager.SelectNextUnit();        
    }


    void Update()
    {
        PlayerTurn = unitManager.gameUnits[unitManager.currUnitTurn].playerControlled;

        if (PlayerTurn)
        {
            whosTurnIsitAnyway = "Player";
            nextTurnButton.image.color = Color.blue;
        }
        else
        {
            whosTurnIsitAnyway = "Enemy";
            nextTurnButton.image.color = Color.red;
        }

        turnText.text = whosTurnIsitAnyway + "'s Turn: " + turnNumber;

    }
}
