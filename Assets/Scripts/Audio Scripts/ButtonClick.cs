using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Audio feature that allows for different sounds to be played or stopped on a button click
public class ButtonClick : MonoBehaviour
{
    public void playClick()
    {
        FindObjectOfType<AudioManager>().Play("Click2");
    }

    public void stopVictoryMusic()
    {
        FindObjectOfType<AudioManager>().Stop("VictoryMusic");
    }

    public void stopLossMusic()
    {
        FindObjectOfType<AudioManager>().Stop("LossMusic");
    }
}
