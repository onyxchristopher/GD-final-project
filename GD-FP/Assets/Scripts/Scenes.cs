using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scenes : MonoBehaviour
{
    // Current scene ID to operate upon
    private int id;

    // References to clusters
    private Cluster level0;
    private Cluster[] level1;
    private Cluster[][] level2;

    void Start() {
        EventManager.onBossDefeat += CompleteSector;
        EventManager.onMinorObjectiveComplete += CompleteMinorObjective;
    }

    public void InitializeScenes(int num, Cluster lvl0, Cluster[] lvl1, Cluster[][] lvl2) {
        // Unload any loaded scene
        for (int i = 1; i <= num; i++) {
            if (SceneManager.GetSceneByBuildIndex(i).isLoaded) {
                SceneManager.UnloadSceneAsync(i);
            }
        }
        // Set cluster arrays for easy access when loading
        level0 = lvl0;
        level1 = lvl1;
        level2 = lvl2;
    }

    public void QueueLoad(int newId) {
        id = newId;
        StartCoroutine("_Load");
    }

    private IEnumerator _Load() {
        
        // Load scene addititively
        AsyncOperation op = SceneManager.LoadSceneAsync(id, LoadSceneMode.Additive);

        yield return op;

        // Get loaded scene
        Scene loadedScene = SceneManager.GetSceneByBuildIndex(id);

        // Get scene's root object
        GameObject root = loadedScene.GetRootGameObjects()[0];

        // Get sector index
        int sectorIndex = id - 1;

        // Assign positions to objects
        Vector3 rootPosition = level1[sectorIndex].getCorePosition();
        root.transform.position = rootPosition;

        if (id <= 2) {
            root.transform.GetChild(0).GetComponent<Enemy>().ReassignSpawn(level1[sectorIndex].getCorePosition());
            if (id <= 2) {
                root.transform.GetChild(1).position = level2[sectorIndex][0].getCorePosition();
                root.transform.GetChild(2).position = level2[sectorIndex][1].getCorePosition();
            }
        }

        if (level1[sectorIndex].getComplete()) {
            if (id <= 2) {
                root.transform.GetChild(0).gameObject.SetActive(false);
            }
        }
        for (int i = 0; i < level2[sectorIndex].Length; i++) {
            if (level2[sectorIndex][i].getComplete()) {
                if (id <= 2) {
                    root.transform.GetChild(i + 1).gameObject.SetActive(false);
                }
            }
        }
    }

    public void QueueUnload(int newId) {
        id = newId;
        StartCoroutine("_Unload");
    }

    private IEnumerator _Unload() {
        SceneManager.UnloadSceneAsync(id);
        yield return null;
    }

    // set the cluster as complete, avoiding boss respawn
    public void CompleteSector(string bossName) {
        level1[GameObject.FindWithTag("Compass").GetComponent<Compass>().currCluster - 1].setComplete(true);
    }

    // set the minor objective as complete, avoiding respawn
    public void CompleteMinorObjective(int sectorId, int objectiveId) {
        level2[sectorId - 1][objectiveId - 1].setComplete(true);
    }
}
