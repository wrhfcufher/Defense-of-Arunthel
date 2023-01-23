using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// A static class that holds a list of all the attacks in the game. This allows us to easily look up an attack with just an index
/// </summary>

public static class AttackDatabase
{
    private static AttackData[] attackList = {
        /* -----------------------------------------------------------------
        FORMAT
        // <attack name> <attack index>
        new AttackData
            (
                <path to sprite starting in Assets/Resources>
                <attack name>
                <attack description> Little bit of worldbuilding as well as description of effect
                <attack cooldown> Cooldown of 0 means every turn, cooldown of 1 means every other turn etc.
                <attack range> Range of 1 means only next to you
                (Unit user, Unit[] targets) =>
                {
                    <code to execute attack> user will be the casting unit, targets will be an array of all affected units
                }
                <selection controller> Use a selection controller that allows the user to select only the targets this attack can properly affect
            )
        -------------------------------------------------------------------- */
        // dagger strike 0 DO NOT USE OTHER THAN ON ROGUE
        new AttackData
            (
                "AttackSprites/dagger strike",
                "Dagger Strike",
                "You stab forward with your dagger. Does damage equal to 3 times your displacement this turn (minimum of 3 damage).",
                1,
                1,
                (Unit user, Unit[] targets) =>
                {
                    if (targets.Length != 1)
                    {
                        throw new InvalidOperationException($"Tried to use a single target attack on {targets.Length} targets.");
                    }

                    // play attack sound effect
                    GameObject.FindObjectOfType<AudioManager>().Play("DaggerStrike");

                    int damage = HexBoard.distanceBetweenTiles(user.xs, user.ys, user.x, user.y) * 3;

                    targets[0].takeDamage(Math.Max(damage, 3), user);

                    // face towards the target
                    faceTargetTile(user, targets[0].x, targets[0].y)
                    .messageAbove("Dagger Strike", Color.magenta);


                },
                new SingleTargetAttackSelectionController()
            ),
        // Blink 1
        new AttackData
            (
                "AttackSprites/blink",
                "Blink",
                "You quickly move through the shadow. Go to a tile within 4 tiles ignoring movement costs",
                3,
                4,
                (Unit user, Unit[] targets) =>
                {
                    if (targets.Length != 1)
                    {
                        throw new InvalidOperationException($"Tried to use a single target tile attack on {targets.Length} targets.");
                    }

                    GameObject.FindObjectOfType<AudioManager>().Play("Blink");

                    // need to find the board manager to move the unit
                    BoardManager bm = GameObject.FindObjectOfType<BoardManager>();
                    bm.teleportUnit(bm.getUnitControllerForUnit(user), targets[0].x, targets[0].y);
                    displayMessage(user, "Blink", Color.magenta, bm);
                },
                new SingleTargetTileAttackSelectionController()
            ),
        // Arrow 2 - for archer class that never got finished :(
        new AttackData
        (
            "AttackSprites/fist",
            "Arrow",
            "A ranged attack that does more damage if you are on higher ground.",
            1,
            4,
            (Unit user, Unit[] targets) =>
            {
                if (targets.Length != 1)
                {
                        throw new InvalidOperationException($"Tried to use a single target attack on {targets.Length} targets.");
                }

                GameObject.FindObjectOfType<AudioManager>().Play("Arrow");

                // need to find the board manager to check heights
                BoardManager bm = GameObject.FindObjectOfType<BoardManager>();

                int damage = bm.getTile(user.x, user.y).height > bm.getTile(targets[0].x, targets[0].y).height ? 30 : 10;


                targets[0].takeDamage(damage, user);

                // face towards the target
                faceTargetTile(user, targets[0].x, targets[0].y, bm)
                .messageAbove("Arrow", Color.magenta);

            },
            new SingleTargetAttackSelectionController()
        ),
        // Assassinate 3
        new AttackData
        (
            "AttackSprites/Assassinate",
            "Assassinate",
            "A surprise finisher. Instantly kills an enemy if they are at or below 50% health, otherwise does minor damage.",
            4,
            1,
            (Unit user, Unit[] targets) =>
            {
                if (targets.Length != 1)
                {
                        throw new InvalidOperationException($"Tried to use a single target attack on {targets.Length} targets.");
                }

                GameObject.FindObjectOfType<AudioManager>().Play("Assassinate");

                int damage = targets[0].health / (float) targets[0].maxHealth <= 0.5 ? targets[0].maxHealth * 20 : 10;


                targets[0].takeDamage(damage, user);

                // face towards the target
                faceTargetTile(user, targets[0].x, targets[0].y)
                .messageAbove("Assassinate", Color.magenta);
            },
            new SingleTargetAttackSelectionController()
        ),
        // Infuse With Magic 4
        new AttackData
        (
            "AttackSprites/infuse magic",
            "Infuse With Magic",
            "Being filled with dark magic has its upsides. Heal 20 damage to a unit and reduce their movement by 2.",
            2,
            2,
            (Unit user, Unit[] targets) =>
            {
                if (targets.Length != 1)
                {
                        throw new InvalidOperationException($"Tried to use a single target attack on {targets.Length} targets.");
                }

                GameObject.FindObjectOfType<AudioManager>().Play("InfuseWithMagic");

                int movementStart = targets[0].currentMovement;
                targets[0].reduceMovement(2);
                targets[0].heal(10 * (movementStart - targets[0].currentMovement));

                if (user != targets[0])
                {
                    // face towards the target
                    faceTargetTile(user, targets[0].x, targets[0].y)
                    .messageAbove("Infuse With Magic", Color.magenta);
                }
                else
                {
                    displayMessage(user, "Infuse With Magic", Color.magenta);
                }

            },
            new SingleTargetAttackSelectionControllerTargetBoth()
        ),
        // Staff Strike 5
        new AttackData
        (
            "AttackSprites/staff strike",
            "Staff Strike",
            "A swing of a staff meant for witchcraft. Does 10 damage.",
            1,
            1,
            (Unit user, Unit[] targets) =>
            {
                if (targets.Length != 1)
                {
                    throw new InvalidOperationException($"Tried to use a single target attack on {targets.Length} targets.");
                }

                GameObject.FindObjectOfType<AudioManager>().Play("StaffStrike");

                targets[0].takeDamage(10, user);

                // face towards the target
                faceTargetTile(user, targets[0].x, targets[0].y)
                .messageAbove("Staff Strike", Color.magenta);

            },
            new SingleTargetAttackSelectionController()
        ),
        // Siphon Life 6
        new AttackData
        (
            "AttackSprites/siphon life",
            "Siphon Life",
            "Your enemies' pain is your gain. Deal 10 damage and heal the amount dealt.",
            2,
            3,
            (Unit user, Unit[] targets) =>
            {
                if (targets.Length != 1)
                {
                    throw new InvalidOperationException($"Tried to use a single target attack on {targets.Length} targets.");
                }

                GameObject.FindObjectOfType<AudioManager>().Play("SiphonLife");

                int hpStart = targets[0].health;
                targets[0].takeDamage(10, user);
                user.heal(hpStart - targets[0].health);

                // face towards the target
                faceTargetTile(user, targets[0].x, targets[0].y)
                .messageAbove("Siphon Life", Color.magenta);

            },
            new SingleTargetAttackSelectionController()
        ),
        // Cripple 7
        new AttackData
        (
            "AttackSprites/cripple",
            "Cripple",
            "A strike to the knees inhibits your foe. Deals 10 damage and the target can't move next turn.",
            2,
            1,
            (Unit user, Unit[] targets) =>
            {
                if (targets.Length != 1)
                {
                    throw new InvalidOperationException($"Tried to use a single target attack on {targets.Length} targets.");
                }

                GameObject.FindObjectOfType<AudioManager>().Play("Cripple");

                targets[0].takeDamage(10, user);
                targets[0].applyStatusEffect(AttackModifier.cripple, 1);

                // face towards the target
                faceTargetTile(user, targets[0].x, targets[0].y)
                .messageAbove("Cripple", Color.magenta);

            },
            new SingleTargetAttackSelectionController()
        ),
        // Spinning Strike 8
        new AttackData
        (
            "AttackSprites/spinning strike",
            "Spinning Strike",
            "A large sweeping strike that does 10 damage to all adjacent enemies.",
            1,
            0,
            (Unit user, Unit[] targets) =>
            {
                if (targets.Length == 0)
                {
                    throw new InvalidOperationException($"Tried to use a multi target attack on 0 targets.");
                }

                GameObject.FindObjectOfType<AudioManager>().Play("SpinningStrike");

                for (int i = 0; i < targets.Length; i++)
                {
                    targets[i].takeDamage(10, user);
                }

                displayMessage(user, "Spinning Strike", Color.magenta);

            },
            new AOEAttackSelectionController(1)
        ),
        // Reflect 9
        new AttackData
        (
            "AttackSprites/retaliate",
            "Reflect",
            "Sometimes defense is the best offense. Redirect 33% of damage next turn.",
            2,
            0,
            (Unit user, Unit[] targets) =>
            {
                if (targets != null)
                {
                   throw new InvalidOperationException("Tried to use self-augmenting attack on targets");
                }

                GameObject.FindObjectOfType<AudioManager>().Play("Reflect");
                displayMessage(user, "Reflect", Color.magenta);
                user.applyStatusEffect(AttackModifier.reflecting, 2);
            },
            new SelfTargetAttackSelectionController()
        ),
        // Nip 10
        new AttackData
        (
            "AttackSprites/empty",
            "Nip",
            "A quick bite from the mouth of a hungry fiend. Does 5 damage.",
            0,
            1,
            (Unit user, Unit[] targets) =>
            {
                if (targets.Length != 1)
                {
                    throw new InvalidOperationException($"Tried to use a single target attack on {targets.Length} targets.");
                }

                GameObject.FindObjectOfType<AudioManager>().Play("Nip");

                targets[0].takeDamage(5, user);

                // face towards the target
                faceTargetTile(user, targets[0].x, targets[0].y)
                .messageAbove("Nip", Color.magenta);

            },
            new SingleTargetAttackSelectionController()
        ),
        // Spit 11
        new AttackData
        (
            "AttackSprites/empty",
            "Spit",
            "Goblin spit is highly toxic. Poisons for 4 turns.",
            3, 
            2,
            (Unit user, Unit[] targets) =>
            {
                if (targets.Length != 1)
                {
                    throw new InvalidOperationException($"Tried to use a single target attack on {targets.Length} targets.");
                }

                GameObject.FindObjectOfType<AudioManager>().Play("Spit");

                targets[0].applyStatusEffect(AttackModifier.poison, 4);

                // face towards the target
                faceTargetTile(user, targets[0].x, targets[0].y)
                .messageAbove("Spit", Color.magenta);

            },
            new SingleTargetAttackSelectionController()
        ),
        // Club 12
        new AttackData
        (
            "AttackSprites/empty",
            "Club",
            "A dizzying hit from a club, does 3 damage and lowers movement by 1.",
            2,
            1,
            (Unit user, Unit[] targets) =>
            {
                if (targets.Length != 1)
                {
                    throw new InvalidOperationException($"Tried to use a single target attack on {targets.Length} targets.");
                }

                GameObject.FindObjectOfType<AudioManager>().Play("Club");

                targets[0].takeDamage(3, user);
                targets[0].reduceMovement(1);

                // face towards the target
                faceTargetTile(user, targets[0].x, targets[0].y)
                .messageAbove("Club", Color.magenta);

            },
            new SingleTargetAttackSelectionController()
        ),
        // Spear Throw 13
        new AttackData
        (
            "AttackSprites/empty",
            "Spear Throw",
            "",
            2,
            3,
            (Unit user, Unit[] targets) =>
            {
                if (targets.Length != 1)
                {
                    throw new InvalidOperationException($"Tried to use a single target attack on {targets.Length} targets.");
                }

                GameObject.FindObjectOfType<AudioManager>().Play("SpearThrow");

                targets[0].takeDamage(12, user);

                // face towards the target
                faceTargetTile(user, targets[0].x, targets[0].y)
                .messageAbove("Spear Throw", Color.magenta);

            },
            new SingleTargetAttackSelectionController()
        ),
        // Snarl 14
        new AttackData
        (
            "AttackSprites/empty",
            "Snarl",
            "",
            3,
            1,
            (Unit user, Unit[] targets) =>
            {
                if (targets.Length != 0)
                {
                    throw new InvalidOperationException($"Tried to use a 0 target attack on {targets.Length} targets.");
                }

                GameObject.FindObjectOfType<AudioManager>().Play("Snarl");

                BoardManager bm = GameObject.FindObjectOfType<BoardManager>();
                foreach (HexBoard.PathingStep step in bm.getSelectableTiles(user.x, user.y, 1))
                {
                    Unit unit = bm.tileControllers[step.x, step.y].tile.unit;
                    if (unit != null && unit.team == user.team)
                    {
                        unit.heal(5);
                    }
                }

                displayMessage(user, "Snarl", Color.magenta);
            },
            new SelfTargetAttackSelectionController()
        ),
        // Slash 15
        new AttackData
        (
            "AttackSprites/empty",
            "Slash",
            "",
            3,
            1,
            (Unit user, Unit[] targets) =>
            {
                if (targets.Length != 1)
                {
                    throw new InvalidOperationException($"Tried to use a single target attack on {targets.Length} targets.");
                }

                GameObject.FindObjectOfType<AudioManager>().Play("Slash");


                targets[0].takeDamage(8, user);

                // face towards the target
                faceTargetTile(user, targets[0].x, targets[0].y)
                .messageAbove("Slash", Color.magenta);

            },
            new SingleTargetAttackSelectionController()
        ),
        // Stomp 16
        new AttackData
        (
            "AttackSprites/empty",
            "Stomp",
            "",
            2,
            1,
            (Unit user, Unit[] targets) =>
            {
                if (targets.Length != 0)
                {
                    throw new InvalidOperationException($"Tried to use a 0 target attack on {targets.Length} targets.");
                }

                GameObject.FindObjectOfType<AudioManager>().Play("Stomp");

                BoardManager bm = GameObject.FindObjectOfType<BoardManager>();
                foreach (HexBoard.PathingStep step in bm.getSelectableTiles(user.x, user.y, 1))
                {
                    Unit unit = bm.tileControllers[step.x, step.y].tile.unit;
                    if (unit != null && unit.team != user.team)
                    {
                        unit.takeDamage(6, user);
                    }
                }

                displayMessage(user, "Stomp", Color.magenta);
            },
            new SelfTargetAttackSelectionController()
        ),
        // Boulder Throw 17
        new AttackData
        (
            "AttackSprites/empty",
            "Boulder Throw",
            "",
            2,
            2,
            (Unit user, Unit[] targets) =>
            {
                if (targets.Length != 1)
                {
                    throw new InvalidOperationException($"Tried to use a single target attack on {targets.Length} targets.");
                }

                GameObject.FindObjectOfType<AudioManager>().Play("BoulderThrow");

                targets[0].takeDamage(12, user);

                // face towards the target
                faceTargetTile(user, targets[0].x, targets[0].y)
                .messageAbove("Boulder Throw", Color.magenta);
            },
            new SingleTargetAttackSelectionController()
        ),
        // Toxic Bomb 18
        new AttackData
        (
            "AttackSprites/toxic bomb",
            "Toxic Bomb",
            "A thick smoky explosion fills your enemies' lungs. Poisons for 3 turns.",
            3,
            3,
            (Unit user, Unit[] targets) =>
            {
                foreach(Unit target in targets)
                {
                    target.applyStatusEffect(AttackModifier.poison, 3);
                }

                GameObject.FindObjectOfType<AudioManager>().Play("ToxicBomb");

                displayMessage(user, "Toxic Bomb", Color.magenta);
            },
            new AOEAttackSelectionController(2)
        )
    };

    // common functionality that many attacks will want to use to face the unit they targeted
    // return the unit controller in case more stuff needs to be done with it
    private static UnitController faceTargetTile(Unit user, int tileX, int tileY, BoardManager bm = null)
    {

        if (bm == null)
        {
            bm = GameObject.FindObjectOfType<BoardManager>();
        }

        UnitController uc = bm.getUnitControllerForUnit(user);
        uc.faceTile(tileX, tileY);

        return uc;
    }

    // common functionality that many attacks will want to use to display text for a unit
    // return the unit controller in case more stuff needs to be done with it
    private static UnitController displayMessage(Unit unit, string message, Color c, BoardManager bm = null)
    {

        if (bm == null)
        {
            bm = GameObject.FindObjectOfType<BoardManager>();
        }

        UnitController uc = bm.getUnitControllerForUnit(unit);
        uc.messageAbove(message, c);

        return uc;
    }

    public static Attack getAttack(int index)
    {
        return new Attack(attackList[index]);
    }

    public static AttackData getAttackData(int index)
    {
        return attackList[index];
    }
}
