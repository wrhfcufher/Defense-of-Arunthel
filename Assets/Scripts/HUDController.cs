using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using UnityEngine.EventSystems;

public class HUDController : MonoBehaviour
{
    public GameObject buttonPrefab;
    public SelectionManager selectionManager;
    private List<GameObject> buttons = new List<GameObject>();
    public float paddingBetweenButtons;
    public float buttonElevation;
    private float buttonWidth = 50f;
    public Image unitPicture;
    public TextMeshProUGUI health;
    public TextMeshProUGUI movementPoints;
    public TextMeshProUGUI unitName;
    private Unit uiUnit;
    public GameObject toolTipObject;
    private TextMeshProUGUI toolTipText;
    public int lineLength;

    private void Start()
    {
        toolTipText = toolTipObject.GetComponentInChildren<TextMeshProUGUI>();
        toolTipObject.SetActive(false);
    }

    private void Update()
    {
        if (uiUnit == null)
        {
            // Unit Unselected
            if (health.gameObject.activeSelf)
            {
                unitPicture.gameObject.SetActive(false);
                health.gameObject.SetActive(false);
                movementPoints.gameObject.SetActive(false);
                unitName.gameObject.SetActive(false);
            }
        }
        else
        {
            // Unit Selected
            if (!health.gameObject.activeSelf)
            {
                unitPicture.gameObject.SetActive(true);
                health.gameObject.SetActive(true);
                movementPoints.gameObject.SetActive(true);
                unitName.gameObject.SetActive(true);
            }
            // Update UI to show unit information
            unitPicture.sprite = uiUnit.unitClass.unitSprite;
            health.SetText($"Health: {uiUnit.health} / {uiUnit.maxHealth}");
            movementPoints.SetText($"Movement: {uiUnit.currentMovement} / {uiUnit.maxMovement}");
            unitName.SetText(System.Enum.GetName(typeof(UnitClass.ClassName), uiUnit.unitClass.className));
        }

        if (toolTipObject.gameObject.activeSelf)
        {
            toolTipObject.transform.position = Input.mousePosition;
        }
    }
    // Will create button gameobjects equal to the number of attacks the character has
    public void CreateAttackButtons(Attack[] attacks)
    {
        int numButtons = attacks.Length;
        float buttonPosition = numButtons % 2 == 0 ?
            // even case
            (buttonWidth / 2f) - (numButtons / 2f) * (paddingBetweenButtons + buttonWidth) :
            // odd case
            0f - ((numButtons - 1) / 2) * (paddingBetweenButtons + buttonWidth);

        foreach (Attack attack in attacks)
        {
            GameObject button = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, transform);
            RectTransform m_transform = button.GetComponent<RectTransform>();
            m_transform.anchoredPosition = new Vector2(buttonPosition, buttonElevation);
            buttons.Add(button);
            Image m_Image = button.GetComponent<Image>();
            m_Image.sprite = attack.imageSprite;
            AttackButton attackButton = button.GetComponent<AttackButton>();
            attackButton.sm = selectionManager;
            attackButton.attack = attack;
            attackButton.hudController = this;
            buttonPosition += paddingBetweenButtons + buttonWidth;
            button.transform.SetAsFirstSibling();
        }
    }

    public void DestroyAttackButtons()
    {
        foreach (GameObject button in buttons)
        {
            Destroy(button);
        }
        buttons = new List<GameObject>();
    }

    public void notifyUnitHovered(Unit unit)
    {
        uiUnit = unit;

        StringBuilder statusText = new StringBuilder("");
        foreach (KeyValuePair<AttackModifier,int> status in uiUnit.status)
        {
            statusText.Append($"{status.Key}: {status.Value} ");
        }

        toolTipText.text = $"{uiUnit.unitClass.className}\n" +
            $"Health: {uiUnit.health} / {uiUnit.maxHealth}\n" +
            $"Movement: {uiUnit.currentMovement} / {uiUnit.maxMovement}\n" +
            $"Status: {statusText.ToString()}";
        toolTipObject.SetActive(true);
        toolTipObject.GetComponent<HorizontalLayoutGroup>().enabled = false;
        toolTipObject.GetComponent<HorizontalLayoutGroup>().enabled = true;
    }

    public void notifyUnitUnhovered()
    {
        toolTipObject.SetActive(false);
        uiUnit = selectionManager.selectedUnit == null ? null : selectionManager.selectedUnit.unit;
    }

    public void notifyAttackHovered(Attack attack)
    {
        toolTipText.text = formatDescription(attack.description);
        toolTipObject.SetActive(true);
    }

    public void notifyAttackUnhovered()
    {
        toolTipObject.SetActive(false);
    }

    public void notifyTileHovered(TileController tc)
    {
        if (EventSystem.current.IsPointerOverGameObject()) { return; } // if hovering over a button or something do not update the text
        toolTipText.text = $"Movement Cost: {tc.tile.moveCost}";
        toolTipObject.SetActive(true);
    }

    public void notifyTileUnhovered()
    {
        if (EventSystem.current.IsPointerOverGameObject()) { return; } // if hovering over a button or something do not update the text
        toolTipObject.SetActive(false);
    }

    // parse description into lines of reasonable length
    private string formatDescription(string str)
    {
        int lettersSinceNewLine = 0;
        string[] words = str.Split(" ");

        StringBuilder newString = new StringBuilder();

        foreach (string word in words)
        {
            newString.Append(word);
            lettersSinceNewLine += word.Length + 1;
            if (lettersSinceNewLine > lineLength)
            {
                newString.Append("\n");
                lettersSinceNewLine = 0;
            }
            else
            {
                newString.Append(" ");
            }
        }

        return newString.ToString();
    }


}
