using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// script placed on all the of UI attack buttons
/// </summary>
[RequireComponent(typeof(Button), typeof(Image))]
public class AttackButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private Attack _attack;
    private bool inUse = false;
    private Button button;
    private Image image;
    public SelectionManager sm;
    [SerializeField]
    private TextMeshProUGUI cooldownText;
    public HUDController hudController;

    public Attack attack { get { return _attack; } set
        {
            if (_attack != null)
            {
                throw new InvalidOperationException("Attack of AttackButton can only be set one time");
            }
            _attack = value;
            button = GetComponent<Button>();
            image = GetComponent<Image>();
            button.onClick.AddListener(setSelectionController);
        } 
    }

    private void setSelectionController()
    {
        if (attack.currentCooldown <= 0 && !sm.selectedUnit.controllerBusy)
        {
            FindObjectOfType<AudioManager>().Play("ClickAttack");
            if (!inUse)
            {
                sm.addInterruptingSelectionController(_attack.selectionController, () => 
                {
                    inUse = false;
                    updateCooldownText(); // when the ability is used the cooldown will be changed, so update the text
                });
                inUse = true;
                image.color = new Color(0.94f, 0.8f, 0.35f);
            }
            else
            {
                sm.removeInterruptingSelectionController(_attack.selectionController);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // if the ability is on cooldown set the cooldown display
        updateCooldownText();
    }

    private void updateCooldownText()
    {
        if (attack.currentCooldown != 0)
        {
            cooldownText.SetText(attack.currentCooldown.ToString());
            image.color = new Color(0.5f, 0.5f, 0.5f);
        }
        else
        {
            cooldownText.SetText("");
            image.color = new Color(1, 1, 1);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hudController.notifyAttackHovered(attack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hudController.notifyAttackUnhovered();
    }
}
