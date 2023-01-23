using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A gameobject that persists between scenes and keeps track of the current level.
/// </summary>
public class LevelManager : MonoBehaviour
{

    [SerializeField]
    private LevelData[] levels;

    [field:SerializeField]
    public int currentLevel { get; private set; } = 0;
    private bool primaryManager = false;

    // Start is called before the first frame update
    void Start()
    {

        // if this object is not the primary manager check to see if it is the only level manager
        if (!primaryManager)
        {
            if (GameObject.FindGameObjectsWithTag("Level Manager").Length != 1)
            {
                // it is not the only level manager so destroy it
                Destroy(gameObject);
                return;
            }
            else
            {
                // it is the only level manager so set primaryManager to true and make it persist
                primaryManager = true;
                gameObject.name = "Primary Level Manager";
                // make it so this object can persist between levels
                SceneManager.sceneLoaded += OnSceneLoaded;
                DontDestroyOnLoad(gameObject);
            }
        }

        // use a coroutine so that the other objects have time to initalize before loading the level
        StartCoroutine(initializeLevelOne());
    }

    // since we can't guarentee the order of start method calls we can use this
    // coroutine to make sure that the level is initialized after every other start method runs
    IEnumerator initializeLevelOne()
    {
        yield return null;
        // tell the level intalizer what level to load
        GameObject.Find("LevelInitalizer").GetComponent<LevelInitializer>().initalize(levels[currentLevel]);
    }

    // what do do every time a new scene is loaded
    void OnSceneLoaded(Scene scene, LoadSceneMode mode){
        Debug.Log("Level: " + currentLevel);
        GameObject levelInitalizer = GameObject.Find("LevelInitalizer");
        if (levelInitalizer != null)
        {
            levelInitalizer.GetComponent<LevelInitializer>().initalize(levels[currentLevel]);
        }
    }

    public void nextLevel(){
        currentLevel = currentLevel + 1;
    }

    public int numLevels(){
        return levels.Length;
    }

    // unsubscribe from sceneLoading events are remove the object
    public void removeLevelManager()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Destroy(gameObject);
    }
}
