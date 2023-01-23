using System.Collections;
using System.Collections.Generic;
using TMPro;
#if (UNITY_EDITOR) // this only works in the editor
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script used in the level maker scene allowing us to easily edit levels
/// </summary>

public class LevelMaker : MonoBehaviour
{

    private int width;
    private int height;

    private int selectedX = -1;
    private int selectedY = -1;
    private LevelMakerUnit selectedUnit;

    public LevelData levelData;




    public GameObject LevelMakerTile;
    public GameObject LevelMakerUnit;
    [field: SerializeField]
    public float HexHeight { get; private set; } = 1;
    [field: SerializeField]
    public float HexWidth { get; private set; } = 1;

    [field: SerializeField]
    public float HexEdgeWidth { get; private set; } = 0.25f;

    public TMP_InputField widthInput;
    public TMP_InputField heightInput;

    public TMP_InputField tileHeightInput;
    public TMP_InputField moveCostInput;
    public TMP_Dropdown tileTextureDropdown;

    public GameObject tileInputGroup;

    public TMP_InputField unitHealthInput;
    public TMP_InputField unitMovementInput;
    public TMP_InputField unitTeamInput;
    public TMP_Dropdown unitClassDropdown;

    public GameObject unitInputGroup;

    private List<GameObject> tilesObjects = new List<GameObject>();
    private List<GameObject> unitObjects = new List<GameObject>();

    public Texture[] tileTextures;

    private bool validTileSelected { get { return selectedX >= 0 && selectedX < width && selectedY >= 0 && selectedY < height; } }

    // Start is called before the first frame update
    void Start()
    {
        List<string> classNames = new List<string>();
        foreach (string name in System.Enum.GetNames(typeof(UnitClass.ClassName)))
        {
            classNames.Add(name);
        }
        unitClassDropdown.AddOptions(classNames);

        List<string> tileTextures = new List<string>();
        foreach (string name in System.Enum.GetNames(typeof(TileData.TileTexture)))
        {
            tileTextures.Add(name);
        }
        tileTextureDropdown.AddOptions(tileTextures);

        // if the levelData does not have any tile data in it, initalize it to a 1 by 1 tile
        if (levelData.width == 0 || levelData.tileData.Length == 0)
        {
            levelData.tileData = new TileData[] { new TileData(1, 1) };
            levelData.width = 1;
        }

        // set this classes with and height using the level data
        width = levelData.width;
        height = levelData.tileData.Length / levelData.width;
        
        // set the width and height input correctly
        widthInput.text = width.ToString();
        heightInput.text = height.ToString();

        // add listeners to the input fields.
        widthInput.onEndEdit.AddListener(widthChanged);
        heightInput.onEndEdit.AddListener(heightChanged);
        tileHeightInput.onEndEdit.AddListener(tileHeightChange);
        moveCostInput.onEndEdit.AddListener(tileCostChange);
        unitHealthInput.onEndEdit.AddListener(unitHealthChange);
        unitMovementInput.onEndEdit.AddListener(unitMovementChange);
        unitTeamInput.onEndEdit.AddListener(unitTeamChange);

        unitClassDropdown.onValueChanged.AddListener(unitClassChange);
        tileTextureDropdown.onValueChanged.AddListener(tileTextureChange);

        // place all the tiles
        placeTiles();

        // place all the units
        placeUnits();
        
    }

    private void tileTextureChange(int num)
    {
        if (!validTileSelected)
        {
            return;
        }

        TileData.TileTexture newTexture = System.Enum.Parse<TileData.TileTexture>(tileTextureDropdown.options[num].text);
        levelData.tileData[selectedX + selectedY * width].texture = newTexture;

        tilesObjects[selectedX + selectedY * width].GetComponent<Renderer>().material.mainTexture = tileTextures[(int) newTexture];

    }

    private void unitClassChange(int num)
    {
        if (selectedUnit == null)
        {
            return;
        }

        UnitClass.ClassName newClass = System.Enum.Parse<UnitClass.ClassName>(unitClassDropdown.options[num].text);

        if (!levelData.teamData[selectedUnit.team].unitData[selectedUnit.index].className.Equals(newClass))
        {
            levelData.teamData[selectedUnit.team].unitData[selectedUnit.index].className = newClass;
        }
    }

    // legacy: health is no longer per unit, but keeping this in case we revert it
    private void unitHealthChange(string arg)
    {
        if (selectedUnit == null)
        {
            return;
        }

        int newValue;
        if (int.TryParse(arg, out newValue) && newValue != levelData.teamData[selectedUnit.team].unitData[selectedUnit.index].health)
        {
            levelData.teamData[selectedUnit.team].unitData[selectedUnit.index].health = newValue;
        }
    }

    // legacy: movement is no longer per unit, but keeping this in case we revert it
    private void unitMovementChange(string arg)
    {
        if (selectedUnit == null)
        {
            return;
        }

        int newValue;
        if (int.TryParse(arg, out newValue) && newValue != levelData.teamData[selectedUnit.team].unitData[selectedUnit.index].movement)
        {
            levelData.teamData[selectedUnit.team].unitData[selectedUnit.index].movement = newValue;
        }
    }

    private void unitTeamChange(string arg)
    {
        if (selectedUnit == null)
        {
            return;
        }

        int newValue;
        if (int.TryParse(arg, out newValue) && newValue != selectedUnit.team && newValue >= 0)
        {
            // get the units information
            LevelData.UnitData unitData = levelData.teamData[selectedUnit.team].unitData[selectedUnit.index];
            LevelData.SpawnPoint spawnPoint = levelData.teamData[selectedUnit.team].spawnPoints[selectedUnit.index];

            // remove unit from its origional team data
            removeUnitAtIndex(selectedUnit.team, selectedUnit.index);


            // check if the teamData array needs to be expanded
            if (newValue > levelData.teamData.Length - 1)
            {
                // create the new teamData array and copy the old values into it
                LevelData.TeamData[] newTeamData = new LevelData.TeamData[newValue + 1];
                System.Array.Copy(levelData.teamData, newTeamData, levelData.teamData.Length);

                // add empty team data to all the needed teams
                for (int i = levelData.teamData.Length; i < newValue + 1; i++)
                {
                    LevelData.TeamData tmpTeamData = new LevelData.TeamData();
                    tmpTeamData.spawnPoints = new LevelData.SpawnPoint[] { };
                    tmpTeamData.unitData = new LevelData.UnitData[] { };

                    newTeamData[i] = tmpTeamData;
                }

                // re-assign the team data
                levelData.teamData = newTeamData;
            }

            // add the unit to the new teams arrays
            // initalize new unit arrays with one more size
            LevelData.UnitData[] newUnitData = new LevelData.UnitData[levelData.teamData[newValue].unitData.Length + 1];
            LevelData.SpawnPoint[] newSpawnPoints = new LevelData.SpawnPoint[levelData.teamData[newValue].spawnPoints.Length + 1];

            // copy the old elements into the arrays
            System.Array.Copy(levelData.teamData[newValue].unitData, newUnitData, levelData.teamData[newValue].unitData.Length);
            System.Array.Copy(levelData.teamData[newValue].spawnPoints, newSpawnPoints, levelData.teamData[newValue].unitData.Length);

            // add units data to the end of the array
            newUnitData[newUnitData.Length - 1] = unitData;
            newSpawnPoints[newUnitData.Length - 1] = spawnPoint;

            // reassign the arrays
            levelData.teamData[newValue].unitData = newUnitData;
            levelData.teamData[newValue].spawnPoints = newSpawnPoints;

            // update the team and index of the selected unit
            selectedUnit.team = newValue;
            selectedUnit.index = newSpawnPoints.Length - 1;

        }
    }

    private void removeUnitAtIndex(int team, int index)
    {
        // remove unit from its origional team data
        // initalize arrays with 1 less size
        LevelData.UnitData[] newUnitData = new LevelData.UnitData[levelData.teamData[team].unitData.Length - 1];
        LevelData.SpawnPoint[] newSpawnPoints = new LevelData.SpawnPoint[levelData.teamData[team].spawnPoints.Length - 1];

        // copy over all the other unit data into the arrays
        System.Array.Copy(levelData.teamData[team].unitData, newUnitData, index);
        System.Array.Copy(levelData.teamData[team].unitData, index + 1, newUnitData, index, levelData.teamData[team].unitData.Length - index - 1);
        System.Array.Copy(levelData.teamData[team].spawnPoints, newSpawnPoints, index);
        System.Array.Copy(levelData.teamData[team].spawnPoints, index + 1, newSpawnPoints, index, levelData.teamData[team].spawnPoints.Length - index - 1);

        // re-assign the arrays
        levelData.teamData[team].unitData = newUnitData;
        levelData.teamData[team].spawnPoints = newSpawnPoints;
    }

    private void tileCostChange(string arg)
    {
        if (!validTileSelected)
        {
            return;
        }

        int newValue;
        if (int.TryParse(arg, out newValue) && newValue != width)
        {
            levelData.tileData[selectedX + selectedY * width].moveCost = newValue;
        }
    }

    private void tileHeightChange(string arg)
    {
        if(!validTileSelected)
        {
            return;
        }

        int newValue;
        if (int.TryParse(arg, out newValue) && newValue != levelData.tileData[selectedX + selectedY * width].height)
        {
            levelData.tileData[selectedX + selectedY * width].height = newValue;
            tilesObjects[selectedX + selectedY * width].GetComponent<LevelMakerTile>().resizeMesh(newValue);
        }
    }

    private void widthChanged(string arg)
    {
        int newValue;
        if(int.TryParse(arg, out newValue) && newValue != width)
        {
            width = newValue;
            TileData[] newTileData = new TileData[width * height];

            for (int i = 0; i < newTileData.Length; i++)
            {
                int x = i % width;
                int y = i / width;

                if (x >= levelData.width || levelData.tileData[x + y * levelData.width] == null)
                {
                    newTileData[i] = new TileData(1, 1);
                }
                else
                {
                    newTileData[i] = levelData.tileData[x + y * levelData.width];
                }
            }

            levelData.tileData = newTileData;
            levelData.width = width;
            removeTiles();
            placeTiles();
        }
    }

    private void heightChanged(string arg)
    {
        int newValue;
        if (int.TryParse(arg, out newValue) && newValue != height)
        {
            TileData[] newTileData = new TileData[width * newValue];

            for (int i = 0; i < newTileData.Length; i++)
            {
                int x = i / width;
                int y = i / width;

                if (y >= height || levelData.tileData[x + y * levelData.width] == null)
                {
                    newTileData[i] = new TileData(1, 1);
                }
                else
                {
                    newTileData[i] = levelData.tileData[x + y * levelData.width];
                }
            }

            levelData.tileData = newTileData;
            height = newValue;
            removeTiles();
            placeTiles();
        }
    }

    private void removeTiles()
    {
        foreach (GameObject tile in tilesObjects)
        {
            Destroy(tile);
        }

        tilesObjects.Clear();
    }

    private void removeUnits()
    {
        foreach (GameObject unit in unitObjects)
        {
            Destroy(unit);
        }

        unitObjects.Clear();
    }

    private void removeAllPlacedObjects()
    {
        removeUnits();
        removeTiles();
    }

    private void placeTiles()
    {
        for (int i = 0; i < levelData.tileData.Length; i++)
        {
            int x = i % width;
            int y = i / width;
            GameObject newTile = Instantiate(LevelMakerTile, transform);
            newTile.transform.localPosition = new Vector3(x * (HexWidth - HexEdgeWidth) + HexWidth / 2, 0, y * HexHeight + HexHeight / 2 - HexHeight / 2 * (x % 2));
            newTile.name = $"{x}-{y}";
            LevelMakerTile lvt = newTile.GetComponent<LevelMakerTile>();
            lvt.x = x;
            lvt.y = y;
            lvt.levelMaker = this;
            lvt.resizeMesh(levelData.tileData[i].height);
            newTile.GetComponent<Renderer>().material.mainTexture = tileTextures[(int) levelData.tileData[i].texture];
            tilesObjects.Add(newTile);

        }
    }

    private void placeUnits()
    {
        for (int team = 0; team < levelData.teamData.Length; team++)
        {
            for (int i = 0; i < levelData.teamData[team].spawnPoints.Length; i++)
            {
                placeUnit(team, i);
            }
        }
    }

    private void placeUnit(int team, int index)
    {
        GameObject newUnit = Instantiate(LevelMakerUnit, transform);
        LevelData.SpawnPoint spawnPoint = levelData.teamData[team].spawnPoints[index];
        float spawnHeight = 1 + levelData.tileData[spawnPoint.x + spawnPoint.y * width].height / 10f;
        newUnit.transform.localPosition = new Vector3(spawnPoint.x * (HexWidth - HexEdgeWidth) + HexWidth / 2, spawnHeight, spawnPoint.y * HexHeight + HexHeight / 2 - HexHeight / 2 * (spawnPoint.x % 2));


        LevelMakerUnit lvu = newUnit.GetComponent<LevelMakerUnit>();
        lvu.team = team;
        lvu.index = index;
        lvu.levelMaker = this;

        unitObjects.Add(newUnit);
    }

    public void onTileSelected(int x, int y)
    {

        if (x == selectedX && y == selectedY)
        {
            return;
        }

        // deselect things if needed
        if (selectedUnit != null)
        {
            selectedUnit.setSelectionIndicatorState(false);
            selectedUnit = null;
        }
        if (validTileSelected)
        {
            tilesObjects[selectedX + selectedY * width].GetComponent<LevelMakerTile>().setSelectionIndicatorState(false);
        }

        // set the selected variables to the tile selected
        selectedX = x;
        selectedY = y;

        // turn on the right input fields
        tileInputGroup.SetActive(true);
        unitInputGroup.SetActive(false);

        // set the input to the values of the tile
        tileHeightInput.text = levelData.tileData[x + y * levelData.width].height.ToString();
        moveCostInput.text = levelData.tileData[x + y * levelData.width].moveCost.ToString();

        // set the correct tile dropdown
        int index = -1;
        for (int i = 0; i < tileTextureDropdown.options.Count; i++)
        {
            if (levelData.tileData[selectedX + selectedY * width].texture.Equals(System.Enum.Parse<TileData.TileTexture>(tileTextureDropdown.options[i].text)))
            {
                index = i;
                break;
            }
        }

        if (index == -1)
        {
            levelData.tileData[selectedX + selectedY * width].texture = System.Enum.Parse<TileData.TileTexture>(tileTextureDropdown.options[0].text);
            index = 0;
        }
        tileTextureDropdown.value = index;

    }

    public void onUnitSelected(LevelMakerUnit unit)
    {

        if (selectedUnit == unit)
        {
            return;
        }

        // deselect things if needed
        if (selectedUnit != null)
        {
            selectedUnit.setSelectionIndicatorState(false);
        }
        if (validTileSelected)
        {
            tilesObjects[selectedX + selectedY * width].GetComponent<LevelMakerTile>().setSelectionIndicatorState(false);
            selectedX = -1;
            selectedY = -1;
        }

        // set the selected variables
        selectedUnit = unit;

        // turn on the unit input fields
        tileInputGroup.SetActive(false);
        unitInputGroup.SetActive(true);

        // set the input to the values of the unit
        unitHealthInput.text = levelData.teamData[selectedUnit.team].unitData[selectedUnit.index].health.ToString();
        unitMovementInput.text = levelData.teamData[selectedUnit.team].unitData[selectedUnit.index].movement.ToString();
        unitTeamInput.text = selectedUnit.team.ToString();

        int index = -1;
        for (int i = 0; i < unitClassDropdown.options.Count; i++)
        {
            if (levelData.teamData[selectedUnit.team].unitData[selectedUnit.index].className.Equals(System.Enum.Parse<UnitClass.ClassName>(unitClassDropdown.options[i].text)))
            {
                index = i;
                break;
            }
        }

        if (index == -1)
        {
            levelData.teamData[selectedUnit.team].unitData[selectedUnit.index].className = System.Enum.Parse<UnitClass.ClassName>(unitClassDropdown.options[0].text);
            index = 0;
        }
        unitClassDropdown.value = index;

    }

    // Update is called once per frame
    void Update()
    {

        // place new unit keybind
        if (Input.GetKeyDown(KeyCode.N) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            if (validTileSelected)
            {
                // if there is no team data create a team data that holds the new unit
                if (levelData.teamData.Length == 0)
                {
                    LevelData.TeamData newTeamData = new LevelData.TeamData();
                    newTeamData.spawnPoints = new LevelData.SpawnPoint[] { new LevelData.SpawnPoint(selectedX, selectedY) };
                    newTeamData.unitData = new LevelData.UnitData[] { new LevelData.UnitData(100, 5, System.Enum.Parse<UnitClass.ClassName>(unitClassDropdown.options[0].text)) };
                    levelData.teamData = new LevelData.TeamData[] { newTeamData };

                    placeUnit(0, 0);
                }
                else
                {
                    // make sure there isn't already a unit on this tile
                    bool tileUsed = false;
                    foreach (LevelData.TeamData teamData in levelData.teamData)
                    {
                        foreach(LevelData.SpawnPoint spawnPoint in teamData.spawnPoints)
                        {
                            if (spawnPoint.x == selectedX && spawnPoint.y == selectedY)
                            {
                                tileUsed = true;
                                break;
                            }
                        }
                        if (tileUsed)
                        {
                            break;
                        }    
                    }

                    if (!tileUsed)
                    {
                        LevelData.UnitData[] newUnitData = new LevelData.UnitData[levelData.teamData[0].unitData.Length + 1];
                        LevelData.SpawnPoint[] newSpawnPoint = new LevelData.SpawnPoint[levelData.teamData[0].spawnPoints.Length + 1];
                        System.Array.Copy(levelData.teamData[0].unitData, newUnitData, levelData.teamData[0].unitData.Length);
                        System.Array.Copy(levelData.teamData[0].spawnPoints, newSpawnPoint, levelData.teamData[0].spawnPoints.Length);
                        newUnitData[newUnitData.Length - 1] = new LevelData.UnitData(100, 5, System.Enum.Parse<UnitClass.ClassName>(unitClassDropdown.options[0].text));
                        newSpawnPoint[newSpawnPoint.Length - 1] = new LevelData.SpawnPoint(selectedX, selectedY);

                        levelData.teamData[0].unitData = newUnitData;
                        levelData.teamData[0].spawnPoints = newSpawnPoint;
                        placeUnit(0, newSpawnPoint.Length - 1);
                    }
                }
            }
        }

        // delete unit keybind
        if (Input.GetKeyDown(KeyCode.Delete) && selectedUnit != null)
        {
            removeUnitAtIndex(selectedUnit.team, selectedUnit.index);
            Destroy(selectedUnit.gameObject);
        }
        
        #if (UNITY_EDITOR) // this only works in the editor
        // save level keybind
        if (Input.GetKeyDown(KeyCode.S) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            packLevelData();
            EditorUtility.SetDirty(levelData);
            AssetDatabase.SaveAssets();
            Debug.Log("Saved Level");
        }
        #endif


    }

    // helper method that removes empty teams from the level data
    private void packLevelData()
    {
        List<LevelData.TeamData> newTeamData = new List<LevelData.TeamData>();
        for (int team = 0; team < levelData.teamData.Length; team++)
        {
            if (levelData.teamData[team].spawnPoints.Length != 0)
            {
                newTeamData.Add(levelData.teamData[team]);
            }
        }
        levelData.teamData = newTeamData.ToArray();

        removeUnits();
        placeUnits();
        selectedUnit = null;
        unitInputGroup.SetActive(false);
    }
}
