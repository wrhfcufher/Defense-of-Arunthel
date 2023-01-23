using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// The script on gameobjects that represent units. It holds a reference to the unit it represents and gets events from it to know when important things happen to it.
/// This class is also in charge of animating the unit in the scene.
/// </summary>
public class UnitController : MonoBehaviour
{
    [SerializeField]
    private Image healthBar;
    [SerializeField]
    private Transform unitModel;
    [SerializeField]
    private GameObject msgText;
    [SerializeField]
    public Transform canvas;

    private BoardManager _boardManager;
    private SelectionManager _selectionManager;
    [HideInInspector]
    public HUDController hudController;
    private Unit _unit;
    

    public bool controllerBusy { get; private set; } = false;

    public Unit unit
    {
        get { return _unit; }
        set
        {
            if (_unit != null)
            {
                _unit.removeOnDeathListener(onUnitKilled);
                _unit.removeOnHealthChangeListener(onHealthChanged);
                _unit.removeOnStatusAppliedListener(onStatusApplied);
                _unit.removeOnStatusRemovedListener(onStatusRemoved);
            }
            _unit = value;
            if (_unit != null)
            {
                _unit.addOnDeathListener(onUnitKilled);
                _unit.addOnHealthChangeListener(onHealthChanged);
                _unit.addOnStatusAppliedListener(onStatusApplied);
                _unit.addOnStatusRemovedListener(onStatusRemoved);
                onHealthChanged();
            }
        }
    }
    public float walkingTimeToLerp = 0.25f;
    public float rotationTimeToLerp = 0.15f;

    public BoardManager boardManager
    {
        get { return _boardManager; }
        set
        {
            if (_boardManager == null)
            {
                _boardManager = value;
            }
            else
            {
                throw new System.Exception("Tile Controller was assigned a boardManager while already having one");
            }
        }
    }

    public SelectionManager selectionManager
    {
        get { return _selectionManager; }
        set
        {
            if (_selectionManager == null)
            {
                _selectionManager = value;
            }
            else
            {
                throw new System.Exception("Tile Controller was assigned a selectionManager while already having one");
            }
        }
    }

    // Nice helper that allows you to easily get the tile a unit is on
    public Tile tile { get { return _boardManager.getTile(unit.x, unit.y); } }

    // Start is called before the first frame update
    void Start()
    {
        if (boardManager == null)
        {
            Debug.LogWarning("No boardManager has been assigned to a unit controller. This will cause errors.");
        }

        if (selectionManager == null)
        {
            Debug.LogWarning("No selectionManager has been assigned to a unit controller. This will cause errors.");
        }

        if (unit == null)
        {
            Debug.LogWarning("No unit has been assigned to a unit controller. This will cause errors.");
        }

        if (healthBar == null)
        {
            Debug.LogWarning("No healthbar has been assigned to the healthbar property. This will cause errors");
        }

        if (unitModel == null)
        {
            Debug.LogWarning("No unit model has been assigned to a unit controller. This will cause errors.");
        }
    }

    private void OnMouseDown()
    {
        selectionManager.OnUnitMouseDown(this);
    }
    private void OnMouseUp()
    {
        selectionManager.OnUnitMouseUp(this);
    }

    // When mouse hovers over unit, it shows the units stats.
    private void OnMouseEnter()
    {
        hudController.notifyUnitHovered(_unit);
    }

    private void OnMouseExit()
    {
        hudController.notifyUnitUnhovered();
    }

    public void moveToLocationAlongPath(Vector3[] path)
    {
        controllerBusy = true;
        StartCoroutine(movementCoroutine(path));
    }

    public void faceTile(int tileX, int tileY)
    {
        controllerBusy = true;
        StartCoroutine(faceTargetTileCoroutine(tileX, tileY));
    }

    // used to control the units walking animation
    private IEnumerator movementCoroutine(Vector3[] path)
    {
        yield return null;

        FindObjectOfType<AudioManager>().Play("Movement");

        for (int i = 1; i < path.Length; i++)
        {

            float t = 0f;
            // calculate the rotation needed to face the direction the unit is moving
            Vector3 pathDir = path[i] - path[i - 1];
            pathDir.y = 0;
            Quaternion endRotation = Quaternion.LookRotation(pathDir, unitModel.up);
            Quaternion startRotation = unitModel.rotation;

            // let the rotate coroutine handle rotating on its own, then continue when it is done
            yield return StartCoroutine(rotateCoroutine(startRotation, endRotation));

            // smooth movement using lerp
            while (t <= 1)
            {
                t += Time.deltaTime / walkingTimeToLerp;
                transform.localPosition = Vector3.Lerp(path[i - 1], path[i], t);
                yield return null;
            }
        }

        FindObjectOfType<AudioManager>().Stop("Movement");

        transform.localPosition = path[path.Length - 1];
        controllerBusy = false;
    }

    private IEnumerator faceTargetTileCoroutine(int tileX, int tileY)
    {
        Vector3 lookAt = boardManager.tilePosToWorld(tileX, tileY) - boardManager.tilePosToWorld(unit.x, unit.y);
        lookAt.y = 0;

        // wait till the end of the update cycle to continue
        yield return null;

        // let the rotation coroutine handle rotating then continue executing when it is done
        yield return StartCoroutine(rotateCoroutine(unitModel.rotation, Quaternion.LookRotation(lookAt, unitModel.up)));

        // make the controller no longer busy
        controllerBusy = false;
    }

    // called by other coroutines to handle any rotation they need to do
    private IEnumerator rotateCoroutine(Quaternion startRotation, Quaternion endRotation)
    {
        // don't play the rotation animation if the unit is already facing the right direction
        if (!endRotation.Equals(startRotation))
        {
            float t = 0f;
            // smooth rotation using lerp
            while (t <= 1)
            {
                t += Time.deltaTime / rotationTimeToLerp;
                unitModel.rotation = Quaternion.Lerp(startRotation, endRotation, t);
                yield return null;
            }

            unitModel.rotation = endRotation;
        }
    }

    // called when the unit this controller represents dies
    private void onUnitKilled()
    {
        // Remove the colliders for the gameobject so it cannot be selected in between when it died and when the gameobject is destroyed.
        foreach(Collider c in GetComponentsInChildren<Collider>())
        {
            Destroy(c);
        }
        StartCoroutine(deathRoutine());
    }

    private IEnumerator deathRoutine()
    {
        // wait a little bit to let the animations finish before removing the gameobject
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);

    }

    private void onHealthChanged()
    {
        changeHealthColor();
        int lastHP = _unit.lastKnownHP;
        int HPDiff = lastHP - _unit.health;
        if (HPDiff != 0)
        {
            messageAboveWithDelay(Mathf.Abs(HPDiff)+"", HPDiff < 0 ? Color.green : Color.red, 0.25f);
            StartCoroutine(healthCoroutine(HPDiff, lastHP));
        }

    }

    private void onStatusApplied(AttackModifier status, int duration)
    {
        messageAboveWithDelay($"+{status.ToString()} {duration}", Color.yellow, 0.25f);
    }

    private void onStatusRemoved(AttackModifier status)
    {
        messageAboveWithDelay($"-{status.ToString()}", Color.gray, 0.25f);
    }

    public void messageAboveWithDelay(string message, Color c, float delay)
    {
        StartCoroutine(delayedMessageCoroutine(message, c, delay));
    }

    private IEnumerator delayedMessageCoroutine(string message, Color c, float delay)
    {
        yield return new WaitForSeconds(delay);
        messageAbove(message, c);
    }

    public void messageAbove(string message, Color c)
    {

        GameObject textObject = Instantiate(msgText, canvas);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.SetText(message);
        text.color = c;
    }

    private IEnumerator healthCoroutine(int HPDiff, int lastHP)
    {

        float timePassed = 0;
        while (timePassed < 1)
        {
            timePassed += Time.deltaTime / (1.5f);
            float newHP = lastHP - HPDiff * timePassed;
            healthBar.fillAmount = ((float)newHP) / (float)unit.maxHealth;
            changeHealthColor();

            yield return null;

        }


        healthBar.fillAmount = ((float)(_unit.health) / (float)unit.maxHealth);
        changeHealthColor();
        yield break;

    }

    private void changeHealthColor()
    {

        if (healthBar.fillAmount > .66)
        {
            healthBar.color = new Color32(15, 255, 0, 100);
        }
        else if (healthBar.fillAmount > .33)
        {
            healthBar.color = new Color32(255, 255, 0, 100);
        }
        else
        {
            healthBar.color = new Color32(255, 0, 0, 100);
        }
    }
}
