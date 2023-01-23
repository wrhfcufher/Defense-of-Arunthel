using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A sub class of selection controller that is specificly used for attacks. Has a few more inhearited methods.
/// </summary>
public abstract class AttackSelectionController : SelectionController
{
    protected Attack attack;

    public void setAttack(Attack attack)
    {
        this.attack = attack;
    }

    // each attack needs a unique controller, so make it easy to copy yourself.
    public abstract AttackSelectionController shallowCopy();

}
