using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Holds the functionality for the main menu buttons
/// </summary>
public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject InstructionMenu;
    void Start()
    {
        FindObjectOfType<AudioManager>().Play("MenuMusic");
    }

    public void playGame(){
        FindObjectOfType<AudioManager>().Stop("MenuMusic");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void newGame()
    {
        GameObject levelManager = GameObject.Find("Primary Level Manager");
        if (levelManager != null)
        {
            levelManager.GetComponent<LevelManager>().removeLevelManager();
        }

        playGame();
    }

    public void quitGame(){
        Application.Quit();
    }
    public void instructionMenu()
    {
        gameObject.SetActive(false);
        InstructionMenu.SetActive(true);
    }
    public void backButton()
    {
        gameObject.SetActive(true);
        InstructionMenu.SetActive(false);

    }
}
