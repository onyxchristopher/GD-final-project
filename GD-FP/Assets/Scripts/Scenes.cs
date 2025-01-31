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

    [SerializeField] private GameObject[][] pathModules;

    void Start() {
        EventManager.onBossDefeat += CompleteCluster;
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

        // Get cluster index
        int clusterIndex = id - 1;

        // Assign positions to objects
        Vector3 rootPosition = level1[clusterIndex].getCorePosition();
        root.transform.position = rootPosition;

        if (!level1[clusterIndex].getComplete()) {
            if (id <= 1) {
                root.transform.GetChild(0).gameObject.GetComponent<Enemy>().ReassignSpawn(level1[clusterIndex].getCorePosition());
            }
        } else {
            if (id <= 1) {
                Destroy(root.transform.GetChild(0).gameObject);
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
    public void CompleteCluster(string bossName) {
        level1[GameObject.FindWithTag("Compass").GetComponent<Compass>().currCluster - 1].setComplete(true);
    } 

    /*
    // Generate the path modules starting from cluster pathStartClusterID
    public void GeneratePathModules(int pathStartClusterID) {
        for (int i = 0; i < pathModules[pathStartClusterID].Length; i++) {

        }
    }*/
}
