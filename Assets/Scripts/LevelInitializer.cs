using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used to initalize the game correctly.
/// This is needed becuase if we made every class
/// initalize using the start method we would run into
/// issues with some start methods being called before others.
/// </summary>
public class LevelInitializer : MonoBehaviour
{

    [field: SerializeField]
    public float HexHeight { get; private set; } = 1;
    [field: SerializeField]
    public float HexWidth { get; private set; } = 1;

    [field: SerializeField]
    public float HexEdgeWidth { get; private set; } = 0.25f;

    [SerializeField]
    private BoardManager boardManager;
    [SerializeField]
    private TurnManager turnManager;
    [SerializeField]
    private SelectionManager selectionManager;
     [SerializeField]
    private WinLossManager winLossManager;
    [SerializeField]
    private EnemyAI enemyAI;
    [SerializeField]
    private HUDController hudController;

    [SerializeField]
    private GameObject[] unitControllerObjects;

    [SerializeField]
    private GameObject tileObject; // gameobject placed at the center of all the hexes

    [SerializeField]
    private Texture[] tileTextures; // array of textures that correspond with the tile texture enum


    // called to initalize the game
    // takes in a levelData which stores the information about the level being loaded
    public void initalize(LevelData levelData)
    {
        // Initalize the board object
        HexBoard board = new HexBoard(levelData);

        // Get the gameobject associated with the boardManager
        GameObject boardObject = boardManager.gameObject;

        TileController[,] tcs = new TileController[levelData.width, levelData.height];

        // place gameobjects to represent each tile
        for (int x = 0; x < levelData.width; x++)
        {
            for (int y = 0; y < levelData.height; y++)
            {

                GameObject newTile = Instantiate(tileObject, boardObject.transform);
                Vector3 tileBase = BoardManager.tilePosToWorld(x, y, HexWidth, HexEdgeWidth, HexHeight, board);
                tileBase.y = 0;
                newTile.transform.localPosition = tileBase;
                newTile.name = $"{x}-{y}";

                // Setup the tile controller
                TileController tileController = newTile.GetComponent<TileController>();
                tileController.boardManager = boardManager;
                tileController.selectionManager = selectionManager;
                tileController.x = x;
                tileController.y = y;

                // set the correct texture
                newTile.GetComponent<Renderer>().material.mainTexture = tileTextures[(int)levelData.tileData[x + y * levelData.width].texture];

                tcs[x, y] = tileController;

            }
        }

        List<UnitController> ucs = new List<UnitController>();
        boardManager.initBoard(board, tcs, HexHeight, HexWidth, HexEdgeWidth);

        turnManager.teams = boardManager.teams;
        boardManager.subscribeEndOfTurn(turnManager);
        selectionManager.subscribeEndOfTurn(turnManager);
        enemyAI.subscribeStartOfTurn(turnManager);

        // place the units stored levelData
        for (int team = 0; team < levelData.teamData.Length; team++)
        {
            for (int i = 0; i < levelData.teamData[team].spawnPoints.Length; i++)
            {
                // set up the unit object using information from the leveldata
                Unit newUnit = new Unit
                    (
                    //levelData.teamData[team].unitData[i].health, 
                    //levelData.teamData[team].unitData[i].movement,
                    team, 
                    UnitClass.getUnitClass(levelData.teamData[team].unitData[i].className), 
                    levelData.teamData[team].spawnPoints[i].x, 
                    levelData.teamData[team].spawnPoints[i].y
                    );
                // add the new unit to the board
                board.addUnit(newUnit);
                // subscribe the unit to end of turn events
                newUnit.subscribeEndOfTurn(turnManager);
                newUnit.subscribeStartOfTurn(turnManager);

                // set up the unit controller
                GameObject unitObject = Instantiate(unitControllerObjects[(int) levelData.teamData[team].unitData[i].className], boardObject.transform);
                // Moves object to tile in world
                unitObject.transform.localPosition = boardManager.tilePosToWorld(newUnit.x, newUnit.y);
                UnitController uc = unitObject.GetComponent<UnitController>();
                uc.boardManager = boardManager;
                uc.selectionManager = selectionManager;
                uc.hudController = hudController;
                uc.unit = newUnit;
                boardManager.addUnitController(uc);
            }
        }

        // Once all the units are added have the selectionManager subscribe to unit list changes
        selectionManager.subscribeOnUnitListChange();
        winLossManager.subscribeOnUnitListChange();

        // start the first turn
        turnManager.startTurn();

        // play music
        FindObjectOfType<AudioManager>().Play("BattleMusic");

        // Once the level is set up this object does not need to exist anymore
        Destroy(this);

    }

    // Start is called before the first frame update
    void Start()
    {

        if (boardManager == null)
        {
            Debug.LogWarning("LevelInitializer was not given a BoardManager to initalize");
        }

        if (selectionManager == null)
        {
            Debug.LogWarning("LevelInitializer was not given a selectionManager to use in initalion");
        }

        if (turnManager == null)
        {
            Debug.LogWarning("LevelInitializer was not given a turnManager to use in initalization");
        }

        if (unitControllerObjects.Length < System.Enum.GetValues(typeof(UnitClass.ClassName)).Length)
        {
            Debug.LogWarning("Not enough unit controllers specified");
        }

        if (tileTextures.Length < System.Enum.GetValues(typeof(TileData.TileTexture)).Length)
        {
            Debug.LogWarning("Not enough tile controllers specified");
        }


    }
}
