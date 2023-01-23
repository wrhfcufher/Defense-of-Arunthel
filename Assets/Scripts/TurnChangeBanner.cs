using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Script used on the banners that show up at the start of a turn.
/// </summary>

public class TurnChangeBanner : MonoBehaviour
{
    TextMeshProUGUI tmp;
    public TurnManager turnManager;
    public float fadeTime;
    public bool player;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
        tmp = GetComponent<TextMeshProUGUI>();
        tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, 0);
        turnManager.addStartOfTurnListener(() =>
        {
            if (turnManager.isPlayerTurn == player)
            {
                gameObject.SetActive(true);
                StartCoroutine(DisplayMessage());
            }
        });
    }

    IEnumerator DisplayMessage()
    {
        
        tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, 1);
        while (tmp.color.a > 0.0)
        {
            tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, tmp.color.a - (Time.deltaTime / fadeTime));
            yield return null;
        }
        gameObject.SetActive(false);
    }


}
