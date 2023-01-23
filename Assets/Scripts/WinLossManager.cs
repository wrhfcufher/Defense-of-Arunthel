using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script used to detect when the player wins or loses.
/// </summary>

public class WinLossManager : MonoBehaviour
{
    [SerializeField]
    private BoardManager boardManager;
    [SerializeField]
    private GameObject winMenu;
    [SerializeField]
    private GameObject finishedMenu;
    [SerializeField]
    private GameObject loseMenu;
    public void subscribeOnUnitListChange(){
        boardManager.addOnUnitListChangeListener(onUnitListChange);
    }

    // Checks if a team has no units left, ends level
    private void onUnitListChange(){
        List<Unit> playerUnits = boardManager.getUnits(0);
        List<Unit> enemyUnits = boardManager.getUnits(1);

        // Pop-Up You Lose UI
        if(playerUnits.Count <= 0){
            loseMenu.gameObject.SetActive(true);
            FindObjectOfType<AudioManager>().Stop("BattleMusic");
            FindObjectOfType<AudioManager>().Play("LossMusic");
        }
        // Pop-Up You Win UI 
        else if(enemyUnits.Count <= 0){
            LevelManager levelManager = GameObject.Find("Primary Level Manager").GetComponent<LevelManager>();
            // check if the player finished the last level
            if (levelManager.currentLevel + 1 == levelManager.numLevels())
            {
                finishedMenu.SetActive(true);
            }
            else
            {
                winMenu.gameObject.SetActive(true);
            }
            FindObjectOfType<AudioManager>().Stop("BattleMusic");
            FindObjectOfType<AudioManager>().Play("VictoryMusic");
        }
    }
}
