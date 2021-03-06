﻿using UnityEngine;
using System.Collections;
using System.Linq;
using GGProductions.LetterStorm.Data;
using GGProductions.LetterStorm.Utilities;
using GGProductions.LetterStorm.Data.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ManageLessons : MonoBehaviour
{
    #region Private Variables -------------------------------------------------
    private static PlayerData playerData = null;
    private static List<string> lessonNames = null;
    private static List<string> lessonWordTexts = null;
    private static int selectedLessonBtnIdx = 0;
    private static int selectedWordBtnIdx = 0;
    /// <summary>
    /// Vector used to store the scrolled position of the Scrollable View 
    /// within the AllLessons Area
    /// </summary>
    private static Vector2 allLessonsScrollPosition;
    /// <summary>
    /// Vector used to store the scrolled position of the Scrollable View 
    /// for the selected lesson's words within the CurrentLessons Area
    /// </summary>
    private static Vector2 lessonWordsScrollPosition;
    #endregion Private Variables ----------------------------------------------
    
    void OnGUI()
    {
        // If the player's Lessons and WordSets have not been loaded from 
        // persistent storage, do so
        if (playerData == null)
        {
            playerData = GameStateUtilities.Load();
            playerData.Curriculum.CreateSampleLesson();
        }


        CreateGUI();


        //// Make a background box
        //GUI.Box(new Rect(10, 10, 100, 90), "Loader Menu");

        //// Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
        //if (GUI.Button(new Rect(20, 40, 80, 20), "Level 1"))
        //{
        //    Application.LoadLevel(1);
        //}

        //// Make the second button.
        //if (GUI.Button(new Rect(20, 70, 80, 20), "Level 2"))
        //{
        //    Application.LoadLevel(2);
        //}
    }

    private void CreateGUI()
    {
        // Calculate the location where the top left of the GUI should 
        // start if it is to be centered on screen
        int guiAreaLeft = (Screen.width / 2) - (700 / 2);
        int guiAreaTop = (Screen.height / 2) - (600 / 2);

        // If any of the calculations fall below zero (because the screen is too small),
        // default to zero
        if (guiAreaLeft < 0)
            guiAreaLeft = 0;
        if (guiAreaTop < 0)
            guiAreaTop = 0;

        GUILayout.BeginArea(new Rect(guiAreaLeft, guiAreaTop, 700, 600));

        CreateAllLessonsArea();

        GUILayout.EndArea();
    }

    #region All Lessons Area Methods ------------------------------------------
    /// <summary>
    /// Create all the controls used to display all existing lessons and 
    /// create new lessons
    /// </summary>
    private void CreateAllLessonsArea()
    {
        // If the List of lesson names has not yet been built, 
        // populate the lessons name cache
        BuildLessonNamesCache(false);

        // Wrap everything in the designated GUI Area to group controls together
        GUILayout.BeginArea(new Rect(0, 0, 200, 600));
        // Ensure the controls are laid out vertically
        GUILayout.BeginVertical();

        // Note the button that was previously selected
        int oldSelectedLessonIdx = selectedLessonBtnIdx;

        // Create the button used to create a new lesson, and do so if it has been clicked
        CreateNewLessonBtn(ref selectedLessonBtnIdx, ref oldSelectedLessonIdx);

        // Create a scrollable area incase the number of lessons exceed the space available
        allLessonsScrollPosition = GUILayout.BeginScrollView(allLessonsScrollPosition);
        
        // Build a vertical, one-column grid of buttons corresponding to the 
        // lesson names, and note which one the player selected
        selectedLessonBtnIdx = GUILayout.SelectionGrid(selectedLessonBtnIdx, lessonNames.ToArray(), 1);

        GUILayout.EndScrollView();

        // End the cotroller wrappers
        GUILayout.EndVertical();
        GUILayout.EndArea();

        // If the currently-selected button is different from that selected last frame...
        if (selectedLessonBtnIdx != oldSelectedLessonIdx)
            // Populate the Words area from a new word set corresponding to the selected lesson
            CreateCurrentLessonArea(ref selectedLessonBtnIdx, true);
        // Else, repopulate the Words area from the word set stored in cache
        else
            CreateCurrentLessonArea(ref selectedLessonBtnIdx, false);
    }

    /// <summary>
    /// Create the button used to create a new lesson, and create a new lesson
    /// if it has been clicked.
    /// </summary>
    /// <param name="selectedLessonIdx">Reference to the index of the currently selected lesson</param>
    /// <param name="oldSelectedLessonIdx">Reference to the index of the last selected lesson</param>
    private void CreateNewLessonBtn(ref int selectedLessonIdx, ref int oldSelectedLessonIdx)
    {
        // Create the button used to create a new lesson.  If it was clicked...
        if (GUILayout.Button("New Lesson"))
        {   // If a lesson with the same name does not already exist...
            if (lessonNames.Contains("Lesson") == false)
            {
                // Add a new lesson with a default title to the beginning of the curriculum (so it shows up at the top of the lessons list)
                playerData.Curriculum.Lessons.Insert(0, new Lesson("Lesson"));
                // Add a default word to the new lesson
                playerData.Curriculum.Lessons[0].Words.Add(new Word("word", "hint"));
                // Rebuild the lesson cache, so a button is created for the new lesson
                BuildLessonNamesCache(true);
                // Ensure that the new button is selected so the lesson editor area is populated with the new lesson
                selectedLessonIdx = 0;
                oldSelectedLessonIdx++;     // Increment the old selected index, since a new lesson has been added before it
            }
        }
    }
    #endregion All Lessons Area Methods ---------------------------------------

    #region Current Lesson Area Methods ---------------------------------------
    /// <summary>
    /// Create all controls used to display and modify the selected lesson
    /// </summary>
    /// <param name="lessonIdx">Reference to the index of the selected lesson</param>
    /// <param name="refreshWordList">Refresh the lesson words cache</param>
    private void CreateCurrentLessonArea(ref int lessonIdx, bool refreshWordList)
    {
        // If a List of the lesson words has not yet been built, or if a new 
        // lesson has been selected and the word list should be refreshed, 
        // populate the word cache
        BuildLessonWordsCache(lessonIdx, refreshWordList);

        // Wrap everything in the designated GUI Area to group controls together
        GUILayout.BeginArea(new Rect(250, 0, 200, 600));
        // Ensure the controls are laid out vertically
        GUILayout.BeginVertical();

        // Create the text field used to update the current lesson's name, 
        // and do so if the user changes its contents
        CreateLessonNameTextField(lessonIdx);

        // Create the button used to create a new word, and do so if it has been clicked
        CreateNewWordBtn(lessonIdx, ref selectedWordBtnIdx);

        // Create a scrollable area incase the number of words exceeds the space available
        lessonWordsScrollPosition = GUILayout.BeginScrollView(lessonWordsScrollPosition);

        // Build a vertical, one-column grid of buttons corresponding to the 
        // lesson words, and note which one the player selected
        selectedWordBtnIdx = GUILayout.SelectionGrid(selectedWordBtnIdx, lessonWordTexts.ToArray(), 1);

        GUILayout.EndScrollView();

        // Create the button used to delete the selected lesson, and do so if it has been clicked
        CreateDeleteLessonBtn(ref lessonIdx);

        // End the cotroller wrappers
        GUILayout.EndVertical();
        GUILayout.EndArea();

        CreateWordEditorArea(lessonIdx, ref selectedWordBtnIdx);
    }

    /// <summary>
    /// Create the text field used to update the current lesson's name, and 
    /// update the lesson's name if the user changes the text field's contents
    /// </summary>
    /// <param name="lessonIdx">The index of the current lesson</param>
    private void CreateLessonNameTextField(int lessonIdx)
    {
        // Build the text field used to update the current lesson's name, and retrieve any text the user entered
        string lessonName = GUILayout.TextField(lessonNames[lessonIdx], 50);
        // If text was entered by the user, and if it only contains characters, numbers, or spaces, accept/store it
        if (lessonNames[lessonIdx].Equals(lessonName, System.StringComparison.InvariantCultureIgnoreCase) == false &&
            Regex.IsMatch(lessonName, "([^A-Za-z0-9 ]+)") == false)
        {
            playerData.Curriculum.Lessons[lessonIdx].Name = lessonName;
            BuildLessonNamesCache(true); // Refresh the lesson names cache so the updated
                                         // lesson title will appear in the lessons list
        }
    }

    /// <summary>
    /// Create the button used to create a new word, and create a new word
    /// if it has been clicked.
    /// </summary>
    /// <param name="lessonIdx">The index of the current lesson</param>
    /// <param name="selectedWordIdx">Reference to the index of the selected word</param>
    private void CreateNewWordBtn(int lessonIdx, ref int selectedWordIdx)
    {
        // If the button used to create a new word was clicked...
        if (GUILayout.Button("New Word"))
        {   // If the lesson does not already contain a new word...
            if (lessonWordTexts.Contains("word") == false)
            {
                // Add a default word and hint to the beginning of the lesson (so it shows up at the top of the word list)
                playerData.Curriculum.Lessons[lessonIdx].Words.Insert(0, new Word("word", "hint"));
                // Rebuild the word cache, so the a button is created for the new word
                BuildLessonWordsCache(lessonIdx, true);
                // Ensure that the new button is selected so the word editor area is populated with the new word
                selectedWordIdx = 0;
            }
        }
    }

    /// <summary>
    /// Create the button used to delete the selected lesson, 
    /// and delete the lesson if the user has clicked it
    /// </summary>
    /// <param name="lessonIdx">Reference to the index of the selected lesson</param>
    private void CreateDeleteLessonBtn(ref int lessonIdx)
    {
        // Create the button used to delete the selected lesson.  If it was clicked...
        if (GUILayout.Button("Delete Lesson"))
        {   // If there is at least one lesson left...
            if (lessonNames.Count > 1)
            {   // Remove the lesson from the curriculum
                playerData.Curriculum.Lessons.RemoveAt(lessonIdx);
                // Update the lesson name cache so the associated button is removed
                BuildLessonNamesCache(true);

                // If the lesson index is now out of bounds,
                // default it to the location of the last existing lesson
                if (lessonNames.Count <= lessonIdx)
                    lessonIdx = lessonNames.Count - 1;
            }
        }
    }
    #endregion Current Lesson Area Methods ------------------------------------

    #region Word Editor Area Methods ------------------------------------------
    /// <summary>
    /// Create all controls used to display and modify the selected word
    /// </summary>
    /// <param name="lessonIdx">The index of the selected lesson</param>
    /// <param name="wordIdx">Reference to the index of the selected word</param>
    private void CreateWordEditorArea(int lessonIdx, ref int wordIdx)
    {
        // Wrap everything in the designated GUI Area to group controls together
        GUILayout.BeginArea(new Rect(500, 0, 200, 600));
        // Ensure the controls are laid out vertically
        GUILayout.BeginVertical();

        // Create the text fields to edit the word text and hint, retrieving anything the user entered
        Word wordToEdit = playerData.Curriculum.Lessons[lessonIdx].Words[wordIdx];
        CreateWordTextTextField(lessonIdx, wordToEdit);
        CreateWordHintTextField(wordToEdit);

        // Create the button used to delete the selected word, and do so if it was clicked
        CreateDeleteWordBtn(lessonIdx, ref wordIdx);

        // End the cotroller wrappers
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    /// <summary>
    /// Create the text field used to edit a word's text, 
    /// and update its text if the user changes it
    /// </summary>
    /// <param name="lessonIdx">The index of the selected lesson</param>
    /// <param name="wordToEdit">The Word being edited</param>
    private void CreateWordTextTextField(int lessonIdx, Word wordToEdit)
    {
        // Create the text field used to edit the word's text, and retrieve its current contents
        string wordText = GUILayout.TextField(wordToEdit.Text, 25);

        // If text was entered by the user, and if it only contains characters, accept/store it
        if (wordToEdit.Text.Equals(wordText, System.StringComparison.InvariantCultureIgnoreCase) == false &&
            Regex.IsMatch(wordText, "([^A-Za-z]+)") == false)
        {
            wordToEdit.Text = wordText.ToLowerInvariant();
            BuildLessonWordsCache(lessonIdx, true); // Refresh the word cache so the updated
            // word will appear in the words list
        }
    }

    /// <summary>
    /// Create the text field used to edit a word's hint,
    /// and update its hint if the user changes it
    /// </summary>
    /// <param name="wordToEdit">The Word being edited</param>
    private void CreateWordHintTextField(Word wordToEdit)
    {
        // Create the text field used to edit the word's hint, and store any changes to the hint
        wordToEdit.Hint = GUILayout.TextField(wordToEdit.Hint, 200);
    }

    /// <summary>
    /// Create the button used to delete the selected word, 
    /// and delete the word if the user has clicked it
    /// </summary>
    /// <param name="lessonIdx">The index of the selected lesson</param>
    /// <param name="wordIdx">Reference to the index of the selected word</param>
    private void CreateDeleteWordBtn(int lessonIdx, ref int wordIdx)
    {
        // If the button used to delete the selected word was clicked...
        if (GUILayout.Button("Delete Word"))
        {   // If there is at least one word left in the lesson...
            if (lessonWordTexts.Count > 1)
            {   // Remove the word from the lesson
                playerData.Curriculum.Lessons[lessonIdx].Words.RemoveAt(wordIdx);
                // Update the lesson's word cache so the associated button is removed
                BuildLessonWordsCache(lessonIdx, true);

                // If the word index is now out of bounds,
                // default it to the location of the last existing word
                if (lessonWordTexts.Count <= wordIdx)
                    wordIdx = lessonWordTexts.Count - 1;
            }
        }
    }
    #endregion Word Editor Area Methods ---------------------------------------

    #region Helper Methods ----------------------------------------------------
    /// <summary>
    /// Cache the names of all the available lessons. 
    /// This is done to minimize execution time between frames, 
    /// as the GUI is built from cache.
    /// </summary>
    /// <param name="refresh">Refresh the lesson names cache</param>
    private void BuildLessonNamesCache(bool refresh)
    {
        // If the List of lesson names has not yet been built...
        if (lessonNames == null || refresh == true)
        {   // Create a query to retrieve all lesson names from all existing lessons
            var queryLessonNames = from Lesson l in playerData.Curriculum.Lessons
                                   select l.Name;
            // Convert the query results to a List
            lessonNames = queryLessonNames.ToList();
        }
    }
    
    /// <summary>
    /// Cache the text of the words for the currently selected lesson. 
    /// This is done to minimize execution time between frames, 
    /// as the GUI is built from cache.
    /// </summary>
    /// <param name="lessonIdx">The id of the lesson whose words should be cached</param>
    /// <param name="refresh">Refresh the word cache</param>
    private void BuildLessonWordsCache(int lessonIdx, bool refresh)
    {
        // If a List of the lesson words has not yet been built, 
        // or if the word list should be refreshed...
        if (lessonWordTexts == null || refresh == true)
        {   // Create a query to retrieve the text of all words in the selected lesson
            var queryWordTexts = from Word w in playerData.Curriculum.Lessons[lessonIdx].Words
                                 select w.Text;
            // Convert the query results to a List
            lessonWordTexts = queryWordTexts.ToList();
        }
    }

    #endregion Helper Methods -------------------------------------------------
}
