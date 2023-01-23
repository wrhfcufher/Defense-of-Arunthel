using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


/// <summary>
/// Script put on the text object that apear above units.
/// It handles the movement of the text as well as its transparency.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class FadeTextAnimation : MonoBehaviour
{

    private float t;
    [SerializeField]
    private float totalTime = 1;
    [SerializeField]
    private float moveSpeed = 1;

    private TextMeshProUGUI cur;
    private Color currentColor;

    void Start()
    {
        cur = this.GetComponent<TextMeshProUGUI>();
        currentColor = cur.color;
    }

    // Update is called once per frame
    void Update()
    {
        t+=Time.deltaTime;
        transform.position+= Vector3.up * Time.deltaTime * moveSpeed;
        currentColor.a = Mathf.Max((1 - t/totalTime), 0);
        if(currentColor.a <= 0)
        {
            Destroy(gameObject);
        }
    }

}
