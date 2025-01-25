using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scenes : MonoBehaviour
{
    // Current scene ID to operate upon
    private int id;

    // References to clusters
    private Cluster[] level1;
    private Cluster[][] level2;

    public void InitializeScenes(int num, Cluster[] lvl1, Cluster[][] lvl2) {
        // Unload any loaded scene
        for (int i = 1; i <= num; i++) {
            if (SceneManager.GetSceneByBuildIndex(i).isLoaded) {
                SceneManager.UnloadSceneAsync(i);
            }
        }
        // Set cluster arrays for easy access when loading
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

        // Get cluster index to avoid confusion
        int clusterIndex = id - 1;

        // Assign positions to objects
        Vector3 rootPosition = level1[clusterIndex].getCorePosition();
        root.transform.position = rootPosition;
    }

    public void QueueUnload(int newId) {
        id = newId;
        StartCoroutine("_Unload");
    }

    private IEnumerator _Unload() {
        SceneManager.UnloadSceneAsync(id);
        yield return null;
    }
}
