using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A class that defines a units class (as in warlock, orc, troll, etc).
/// Allows us to easily create units with certain presets.
/// </summary>
public class UnitClass
{
    public ClassName className;
    public Sprite unitSprite;
    private int[] attackIds;
    private int[] unlockableAttackIds;

    public int health { get; private set; }
    public int movement { get; private set; }

    public enum ClassName
    {
        // ALLIED
        Warlock,
        Rogue,
        Juggernaught,

        // ENEMY
        Goblin,
        Orc,
        Troll,

        // DUMMY
        Dummy,     
    }

    private UnitClass(ClassName className, string unitSpritePath, int[] attackIds, int[] unlockableAttackIds, int health, int movement)
    {
        this.className = className;
        this.unitSprite = Resources.Load<Sprite>(unitSpritePath);
        this.attackIds = attackIds;
        this.unlockableAttackIds = unlockableAttackIds;
        this.health = health;
        this.movement = movement;
    }

    public Attack[] getStartingAttacks()
    {
        Attack[] ret = new Attack[attackIds.Length];
        for (int i = 0; i < attackIds.Length; i++)
        {
            ret[i] = AttackDatabase.getAttack(attackIds[i]);
        }
        return ret;
    }

    // static for getting instances of unit classes
    private static Dictionary<ClassName, UnitClass> unitClasses = new Dictionary<ClassName, UnitClass>
    {
        // ALLIED
        {
            ClassName.Warlock,
            new UnitClass(ClassName.Warlock, "UnitSprites/Warlock profile", new int[] {4, 5, 6}, new int[] {}, 50, 3)
        },
        {
            ClassName.Juggernaught,
            new UnitClass(ClassName.Juggernaught, "UnitSprites/Juggernaught profile", new int[] {7, 8, 9}, new int[] {}, 75, 3)
        },
        {
            ClassName.Rogue,
            new UnitClass(ClassName.Rogue, "UnitSprites/Rogue profile", new int[] {0, 1, 3, 18}, new int[] { }, 50, 5)
        },

        // ENEMY
        {
            ClassName.Goblin,
            new UnitClass(ClassName.Goblin, "UnitSprites/Goblin profile", new int[] {10, 11, 12}, new int[] {}, 25, 5)
        },
        {
            ClassName.Orc,
            new UnitClass(ClassName.Orc, "UnitSprites/Orc profile", new int[] {13, 14, 15}, new int[] { }, 40, 4)
        },
        {
            ClassName.Troll,
            new UnitClass(ClassName.Troll, "UnitSprites/Troll profile", new int[] {16, 17}, new int[] { }, 90, 3)
        },

        // DUMMY
        {
            ClassName.Dummy,
            new UnitClass(ClassName.Dummy, "UnitSprites/Warlock profile", new int[] { }, new int[] { }, 0, 0)
        },
    };
    public static UnitClass getUnitClass(ClassName className)
    {
        return unitClasses[className];
    }
}
