﻿using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {

    #region Private Variables ---------------------------------------------
    // Private variables representing components of the HUD
    private GUIText LabelLives;
    private int InventoryBoxWidth = (int)(Screen.width * .75);
    private int InventoryBoxHeight = 30;
    private int InventoryBoxBottomMargin = 5;

    // If game is paused or not paused
    private bool isPaused;
    #endregion Private Variables ------------------------------------------

    /// <summary>
    /// Method that runs only when HUD starts up
    /// </summary>
    void Start()
    {
        isPaused = false;
    }

    /// <summary>
    /// Method that updates HUD once every frame
    /// Displays game, player information, and player inventory
    /// </summary>
    void OnGUI()
    {
        //GUI.Label(new Rect(10, 10, 250, 20), "Score: ");
        //GUI.Label(new Rect(10, 30, 250, 20), "Lives: " + Context.PlayerLives.ToString());
        //GUI.Label(new Rect(10, 50, 250, 20), "Letters Collected: ");
        //GUI.Label(new Rect(10, 70, 250, 20), "Letters Needed: ");

        GUILayout.Box("Lives: " + Context.PlayerLives.ToString());
        GUILayout.Box("Letters Collected: " + Context.PlayerInventory.TotalCollectedLetters);
        GUILayout.Box("Letters Needed: ");
        GUI.Box(new Rect(Screen.width / 2 - InventoryBoxWidth / 2, Screen.height - InventoryBoxHeight - InventoryBoxBottomMargin, InventoryBoxWidth, InventoryBoxHeight), Context.PlayerInventory.A.Count.ToString());

        
    }

    /// <summary>
    /// Method that runs once every frame
    /// Control for keypresses and inventory
    /// </summary>
    void Update()
    {
        // If [Esc] is pressed, pause the game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                // Unpause
                Time.timeScale = 1;
                isPaused = false;
            }
            else
            {
                // Pause
                Time.timeScale = 0;
                isPaused = true;
            }
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            Context.PlayerInventory.AddCollectedLetter("A");
        }
    }
}
