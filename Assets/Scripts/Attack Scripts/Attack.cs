using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Class used to keep track each individual attack on a unit (including the cooldown it has).
/// </summary>

public class Attack
{
    public Sprite imageSprite { get; private set; }
    public string name { get; private set; }

    public string description { get; private set; }
    public int cooldown { get; private set; }
    public int currentCooldown { get; set; }
    // will need more properties damage range etc. this will require thought
    public int attackRange = 1;

    // function that will actully do the attack logic
    public Action<Unit, Unit[]> useAttack;

    // selection controller that will be used when the attack is selected
    public AttackSelectionController selectionController {get; private set;}

    // constructure that copies all the attacks properties from an attackData
    public Attack (AttackData data)
    {
        imageSprite = data.imageSprite;
        name = data.name;
        cooldown = data.cooldown;
        attackRange = data.attackRange;
        useAttack = data.attackLogic;
        selectionController = data.selectionController.shallowCopy();
        description = data.description;

        currentCooldown = 0;
        selectionController.setAttack(this);
    }

    public void putOnCooldown()
    {
        currentCooldown = cooldown;
    }

    public void reduceCooldown()
    {
        if  (currentCooldown != 0)
        {
            currentCooldown--;
        }
    }


    // grab some default attacks for debug
    public static Attack[] defaultAttacks()
    {
        return new Attack[] {AttackDatabase.getAttack(0), AttackDatabase.getAttack(1), AttackDatabase.getAttack(2), AttackDatabase.getAttack(3)};
    }
}
