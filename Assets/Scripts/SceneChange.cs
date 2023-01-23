using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script that is used to change change between different levels and the main menu
/// </summary>

public class SceneChange : MonoBehaviour
{
    public void LoadNextLevel()
    {

        LevelManager levelManager = GameObject.Find("Primary Level Manager").GetComponent<LevelManager>();

        if (levelManager.currentLevel+1 >= levelManager.numLevels()){
            Debug.Log("YOU BEAT ALL THE LEVELS");
            SceneManager.LoadScene(0);
        }
        else{
            levelManager.nextLevel();
            Debug.Log("currentLevel: " + levelManager.currentLevel + "numLevels: " + levelManager.numLevels());
            SceneManager.LoadScene(1);
            
        }
    }

    public void ToMainMenu(){
        SceneManager.LoadScene(0);
    }

    // called when going to main menu after winning
    // increases the level to the next one before going to the main menu
    public void ToMainMenuWin()
    {
        LevelManager levelManager = GameObject.Find("Primary Level Manager").GetComponent<LevelManager>();
        levelManager.nextLevel();
        ToMainMenu();
    }

    public void ReloadLevel(){
        SceneManager.LoadScene(1);
    }
}
