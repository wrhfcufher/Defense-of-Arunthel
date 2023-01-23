using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// C# class that represents a unit on the board. Uses events to signal when important things occur.
/// </summary>
public class Unit
{

    private TurnManager turnManager;

    private int _currentMovement;

    public int health
    {
        get
        {
            return _health;
        }
        private set
        {
            _health = Mathf.Max(Mathf.Min(value, maxHealth), 0);
            if (healthChanged != null)
            {
                healthChanged();
            }
        }
    }

    private int _health;
    public int maxHealth { get; private set; }
    public int maxMovement { get; private set; }
    public int currentMovement
    {
        get
        {
            return _currentMovement;
        }
        set
        {
            _currentMovement = Math.Clamp(value, 0, maxMovement);
        }
    }

    public UnitClass unitClass;

    public Attack[] attacks { get; private set; }

    public bool isDead { get { return health <= 0; } }

    private event System.Action onKilled;

    private event System.Action healthChanged;

    private event System.Action<AttackModifier, int> statusApplied;
    private event System.Action<AttackModifier> statusRemoved;

    // holds the previous hp. Used to do health animations
    public int lastKnownHP { get; private set; }

    // rogue specific properties
    public int xs, ys;

    public Dictionary<AttackModifier, int> status = new Dictionary<AttackModifier, int>();

    // (-1, -1) means the unit is not on a tile
    public int x;
    public int y;
    public int team;

    public Unit(int team, UnitClass unitClass, int x = -1, int y = -1)
    {
        this.health = lastKnownHP = maxHealth = unitClass.health;
        this.maxMovement = unitClass.movement;
        currentMovement = maxMovement;
        this.team = team;
        this.x = x;
        this.y = y;
        this.unitClass = unitClass;
        this.attacks = unitClass.getStartingAttacks();
    }

    public void subscribeEndOfTurn(TurnManager tm)
    {
        turnManager = tm;
        tm.addEndOfTurnListener(onTurnEnd);
    }

    public void subscribeStartOfTurn(TurnManager tm)
    {
        turnManager = tm;
        tm.addStartOfTurnListener(onTurnStart);
    }

    public void addOnDeathListener(System.Action listener)
    {
        onKilled += listener;
    }

    public void removeOnDeathListener(System.Action listener)
    {
        onKilled -= listener;
    }
    public void addOnHealthChangeListener(System.Action listener)
    {
        healthChanged += listener;
    }

    public void removeOnHealthChangeListener(System.Action listener)
    {
        healthChanged -= listener;
    }

    public void addOnStatusAppliedListener(System.Action<AttackModifier, int> listener)
    {
        statusApplied += listener;
    }

    public void removeOnStatusAppliedListener(System.Action<AttackModifier, int> listener)
    {
        statusApplied -= listener;
    }

    public void addOnStatusRemovedListener(System.Action<AttackModifier> listener)
    {
        statusRemoved += listener;
    }

    public void removeOnStatusRemovedListener(System.Action<AttackModifier> listener)
    {
        statusRemoved -= listener;
    }

    public bool takeDamage(int amount, Unit source)
    {
        if (amount < 0)
        {
            throw new InvalidOperationException("Damaged for negative damage");
        }
        lastKnownHP = health;

        // check for reflecting of damage
        if (status.ContainsKey(AttackModifier.reflecting) && source != null)
        {
            int reflectionAmount = (int)(.34f * amount);
            source.takeDamage(reflectionAmount, null);
            health -= (amount - reflectionAmount);
        }
        else
        {
            health -= amount;
        }

        if (isDead)
        {
            // remove the listeners so they do not get called anymore
            turnManager.removeEndOfTurnListener(onTurnEnd);
            turnManager.removeStartOfTurnListener(onTurnStart);
            if (onKilled != null)
            {
                onKilled();
            }
        }

        return isDead;
    }

    public void heal(int amount)
    {
        if (amount < 0)
        {
            throw new InvalidOperationException("Healed for negative health");
        }
        lastKnownHP = health;
        // heal removes poison
        if (status.ContainsKey(AttackModifier.poison))
        {
            status.Remove(AttackModifier.poison);
            if (statusRemoved != null)
            {
                statusRemoved(AttackModifier.poison);
            }
        }

        health += amount;
    }

    // this function exists in case we want something to happen when movement is reduced by attack like animation or text
    public void reduceMovement(int amount)
    {
        this.currentMovement -= amount;
    }

    public void reduceCooldowns()
    {
        foreach (Attack attack in attacks)
        {
            attack.reduceCooldown();
        }
    }

    // Will be called at the start of each turn
    private void onTurnStart()
    {
        if (turnManager.isMyTurn(team))
        {
            // stuff that happens at the start of your turn
            if (unitClass.className == UnitClass.ClassName.Rogue)
            {
                xs = x;
                ys = y;
            }
        }
    }

    // Method that will be called when the turn ends through an event
    private void onTurnEnd()
    {
        if (turnManager.isMyTurn(team))
        {
            // stuff that happens as your turn ends

            currentMovement = maxMovement;

            // decrease duration of all status effects 
            foreach (AttackModifier attackModifier in Enum.GetValues(typeof(AttackModifier)))
            {
                if (status.ContainsKey(attackModifier))
                {
                    if (attackModifier == AttackModifier.poison)
                    {
                        // 3 damage per turn poison could be changed
                        this.takeDamage(3, null);
                    }
                    status[attackModifier] -= 1;
                    if (status[attackModifier] == 0)
                    {
                        status.Remove(attackModifier);
                        if (statusRemoved != null)
                        {
                            statusRemoved(attackModifier);
                        }
                    }
                }
            }
        }
        else
        {
            // stuff that happens as the enemy turn ends

            if (status.ContainsKey(AttackModifier.cripple))
            {
                currentMovement = 0;
            }
        }
    }

    public void applyStatusEffect(AttackModifier attackModifier, int duration)
    {
        if (!status.ContainsKey(attackModifier))
        {
            status.Add(attackModifier, duration);
        }
        status[attackModifier] = Math.Max(status[attackModifier], duration);
        if (statusApplied != null)
        {
            statusApplied(attackModifier, duration);
        }
    }
}
