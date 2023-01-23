using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to hide the continue button if there is no level manager.
/// </summary>
public class HideIfNoLevelManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(GameObject.Find("Primary Level Manager") != null);
    }
}
