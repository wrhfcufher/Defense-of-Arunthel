using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Class used to build attacks this is useful because attacks need unique reference
 * whereas there should only be one attack data for every attack in the game*/
public class AttackData {
    public readonly Sprite imageSprite;
    public readonly string name;
    public readonly string description;
    public readonly int cooldown; // the attacks cooldown
    public readonly int attackRange; // the range of the attack
    public readonly Action<Unit, Unit[]> attackLogic; // the logic that will be run when the attack is used
    public readonly AttackSelectionController selectionController; // the selection code that will be used for this attack

    public AttackData (string imageSpritePath, string name, string description, int cooldown, int attackRange, Action<Unit, Unit[]> attackLogic, AttackSelectionController selectionController)
    {
        this.imageSprite = Resources.Load<Sprite>(imageSpritePath);
        this.name = name;
        this.description = description;
        this.cooldown = cooldown;
        this.attackRange = attackRange;
        this.attackLogic = attackLogic;
        this.selectionController = selectionController;
    }
}
