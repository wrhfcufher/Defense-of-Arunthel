using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script is on the end turn button
/// </summary>
[RequireComponent(typeof(Button))]
public class EndTurnButton : MonoBehaviour
{

    [SerializeField]
    private TurnManager turnManager;
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(turnManager.endTurn);
        turnManager.addStartOfTurnListener(hideShowButton);
    }

    // method to disable/enable the button depending on if its the players turn
    void hideShowButton()
    {
        button.interactable = turnManager.isPlayerTurn;
    }
}
