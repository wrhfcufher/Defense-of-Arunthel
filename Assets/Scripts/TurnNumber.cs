using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Script on the turn banner on top to let the player know whos turn it is.
/// </summary>

public class TurnNumber : MonoBehaviour
{
    TextMeshProUGUI tmp;
    public TurnManager turnManager;

    // Start is called before the first frame update
    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        turnManager.addStartOfTurnListener(updateText);
        updateText();
    }

    void updateText()
    {
        string text = "";
        text += turnManager.isPlayerTurn ? "Player Turn: " : "Enemy Turn: ";
        text += turnManager.turnCount / 2 + 1;
        tmp.SetText(text);
    }
}
