 using UnityEngine;
using System.Collections;

public class Win : MonoBehaviour
{
    #region Fields

    public Texture backgroundTexture;

    private int buttonWidth = 200;
    private int buttonHeight = 50;
    #endregion

    #region Properties

    #endregion

    #region Functions
    void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), backgroundTexture);

        if(GUI.Button(new Rect(130, Screen.height / 2 + 25, 75, 20), "Level 1"))
        {
            Player.Score = 0;
            Player.Lives = 3;
            Player.Missed = 0;
            Application.LoadLevel(2);
        }
        
        if(GUI.Button(new Rect(130, Screen.height / 2 + 50, 75, 20), "Next Level"))
        {
            Player.Score = 0;
            Player.Lives = 3;
            Player.Missed = 0;
            Application.LoadLevel(1);
        }
        
        if(GUI.Button(new Rect(130, Screen.height / 2 + 75, 75, 20), "Main Menu"))
        {
            Player.Score = 0;
            Player.Lives = 3;
            Player.Missed = 0;
			Application.LoadLevel(1);
        }
    }

    #endregion
}
