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
    public Image button;

    public UnitManager unitManager;

    // Start is called before the first frame update
    void Start()
    {
        if (UnitManager.gameUnits[UnitManager.currUnitTurn].playerControlled)
            PlayerTurn = true;
        else
            PlayerTurn = false;
    }
    
    public void NextTurn()
    {
        Debug.Log("END TURN CLICKED");        

        turnNumber += 1;
        TurnEnds += 1;

        //UnitManager manages player turn bool
        unitManager.SelectNextUnit();
        
    }


    void Update()
    {


        PlayerTurn = UnitManager.gameUnits[UnitManager.currUnitTurn].playerControlled;

        if (PlayerTurn)
        {
            whosTurnIsitAnyway = "Player";
            button.color = Color.blue;
        }
        else
        {
            whosTurnIsitAnyway = "Enemy";
            button.color = Color.red;
        }

        turnText.text = whosTurnIsitAnyway + "'s Turn: " + turnNumber;


        // Eddie asks What is this for below?        
        //if (TurnEnds >= 0)
        //{
        //    TurnEnds -= Time.deltaTime;
        //}
    }
}
