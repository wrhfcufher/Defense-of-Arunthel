using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Script that keeps track of the current turn.
/// </summary>

public class TurnManager : MonoBehaviour
{
    public uint turnCount { get; private set; }
    public bool isPlayerTurn { get; private set; } = false;

    private int _teams = -1;
    public int teams { get { return _teams; } set { if (_teams == -1) _teams = value; else throw new InvalidOperationException("teams set twice"); } }

    private event Action endOfTurn;
    private event Action startOfTurn;
    // Start is called before the first frame update
    void Start()
    {
        turnCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void endTurn()
    {
        if (endOfTurn != null)
        {
            FindObjectOfType<AudioManager>().Play("EndTurn");
            endOfTurn();
        }

        turnCount++;
        startTurn();
    }

    public void addEndOfTurnListener(Action lis)
    {
        endOfTurn += lis;
    }

    public void removeEndOfTurnListener(Action lis)
    {
        endOfTurn -= lis;
    }

    public void startTurn()
    {
        isPlayerTurn = !isPlayerTurn;
        if (startOfTurn != null)
        {
            startOfTurn();
        }
    }

    public void addStartOfTurnListener(Action lis)
    {
        startOfTurn += lis;
    }

    public void removeStartOfTurnListener(Action lis)
    {
        startOfTurn -= lis;
    }

    public bool isMyTurn(int team)
    {
        return turnCount % teams == team;
    }

}
