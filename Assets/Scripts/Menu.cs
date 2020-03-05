﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    //set Pause menu as a panel, set inactive
    public GameObject Pause_menu;

    //set Settings menu as a panel, set inactive
    public GameObject Settings_menu;
    //create variable isPaused
    public bool isPaused = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void TogglePause()
    {
        if (isPaused == true)
        {
           
            Pause_menu.SetActive(false);
            isPaused = false;
            return;
        }
        else
        {
            
            Pause_menu.SetActive(true);
            isPaused = true;
            return;
        }
    }
    public void ToggleSetting(bool on)
    {
        if (on == true)
        {
            Settings_menu.SetActive(false);
           
            return;

        }
        else
        {
            Settings_menu.SetActive(true);
            
            return;
        }
        
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }
    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1, LoadSceneMode.Single);
    }
    public void Prevlevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1, LoadSceneMode.Single);
    }
    public void ChangeLevel(int index)
    {
        SceneManager.LoadScene(index, LoadSceneMode.Single);
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    //create a function that toggles pause on and off
    //make timescale 0 when paused and 1 when unpaused
    //set pause menu active when paused and inactive when unpaused
    //add a button that sets settings active and sets pause inactive
    //Create a way to change scenes and sub functions to change screens in the game
    //create a function to change the game

    //in Settings
    //add a button and function to set Settings menu inactive and pause menu active
    //add a slider function to change the volume
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();

        }
    }
}
