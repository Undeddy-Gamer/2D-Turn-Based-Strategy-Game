using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Saving : MonoBehaviour
{
    public Scores[] saveSlots = new Scores[9]; 
    public void Awake()
    {
        //Load Data saved
        LoadData();
    }

    public void LoadData()
    {
        //Load the data from the XML File
        SaveData data = XMLSaving.ReadData();
        //If data isnt equal to null
        if (data != null)
        {
            
            for (int i = 0; i < saveSlots.Length; i++)
            {
                
                saveSlots[i].player = data.playerName[i];
                
                saveSlots[i].level = data.level[i];
                saveSlots[i].turn = data.turn[i];
            }
        }
        else //else if data is null
        {
            
            for (int i = 0; i < saveSlots.Length; i++)
            {
                //Set the player to blank
                saveSlots[i].player = "Blank";
                
                saveSlots[i].level = 0;
                saveSlots[i].turn = 0;
            }
        }
        //Sort the score data
        Sort();
    }

    //When a new score is added
    public void NewScore(string name, int number, int completion,int savePlace)
    {
        
        saveSlots[savePlace].level = number;
        saveSlots[savePlace].turn = completion;
        //set the player to equal name
        saveSlots[savePlace].player = name;
        //Sort
        Sort();
    }

    public void Sort()
    {
        //Sort the strut in a desending order
        Array.Sort(saveSlots, (x, y) => x.level.CompareTo(y.level));
        //Save the data
        Save();
    }

    
    public void Save()
    {
        //New Highscores class under the refernce data
        SaveData data = new SaveData();
        //New temp string name
        string[] name = new string[9];
        //New temp string number
        int[] number = new int[9];
        //For every strut in the high array
        for (int i = 0; i < saveSlots.Length; i++)
        {
            
            data.playerName[i] = saveSlots[i].player;
            
            data.level[i] = saveSlots[i].level;
            data.turn[i] = saveSlots[i].turn;
        }
        //Write the Variable data to an XML file
        XMLSaving.WriteData(data);
    }
}

[System.Serializable]
public struct Scores
{
    public string player; //String for player names
    public int level;
    public int turn;
}
