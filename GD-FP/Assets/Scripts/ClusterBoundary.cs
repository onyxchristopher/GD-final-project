using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterBoundary : MonoBehaviour
{
    // cluster id
    private int id;

    // module to spawn
    [SerializeField] private GameObject[] entryTutorialModules;
    
    // player
    private GameObject player;

    // GameController
    private GameController gControl;

    // distance from the camera edge to spawn the tutorial
    private float offsetDist = 5;

    // distance from the player at which the tutorial should not be despawned

    // tutorial completion status
    private bool[] completedTutorials;

    void Start() {
        player = GameObject.FindWithTag("Player");
        gControl = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        ResetCompletionStatus();
    }

    public void ResetCompletionStatus() {
        completedTutorials = new bool[entryTutorialModules.Length];
    }
    
    public void setId(int newId) {
        id = newId;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            EventManager.EnterCluster(id);
            int index = id - 1;

            // determine if the player has other tutorial modules on screen
            bool toSpawn = ModuleUpdate();

            if (!completedTutorials[index] && toSpawn) {
                // the tutorial has not been completed, so it should be spawned
                Vector2 playerLocation = other.gameObject.GetComponent<Rigidbody2D>().position;
                Vector2 rootLocation = (Vector2) transform.position;
                Vector2 diff = rootLocation - playerLocation;
                float diffAngle = Vector2.Angle(Vector2.right, diff) * Mathf.Deg2Rad;

                // calculate the intersection of the camera edge and the diff line
                float aspectAngle = Mathf.Atan(1 / gControl.cam.aspect);

                float distToEdge = 0;

                // calculate the distance to the camera edge along the diff line
                if (diffAngle < aspectAngle || diffAngle > 2 * Mathf.PI - aspectAngle) {
                    // intersects the left or right edge, so horizontal distance is used
                    distToEdge = gControl.cam.orthographicSize * gControl.cam.aspect / Mathf.Cos(diffAngle);
                } else {
                    // intersects the top or bottom edge, so vertical distance is used
                    distToEdge = gControl.cam.orthographicSize / Mathf.Sin(diffAngle);
                }

                Vector2 tutorialSpawnLocation = playerLocation + diff.normalized * (distToEdge + offsetDist);
                Instantiate(entryTutorialModules[index], tutorialSpawnLocation, Quaternion.identity, transform);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        EventManager.ExitCluster(id);
    }

    // Destroys modules not within the viewport and returns 
    private bool ModuleUpdate() {
        // Destroy any active tutorial module that is not within the player's viewport
        GameObject[] activeTutorialModules = GameObject.FindGameObjectsWithTag("TutorialModule");

        // iterate over all active tutorial modules
        for (int i = 0; i < activeTutorialModules.Length; i++) {
            // if no enemies left in the module, it is completed
            int activeChildren = 0;
            // iterate over each child in that module
            for (int j = 0; j < activeTutorialModules[i].transform.childCount; j++) {
                Transform child = activeTutorialModules[i].transform.GetChild(j);
                if (child.gameObject.activeSelf) {
                    activeChildren++;

                    Vector3 diffFromPlayer = (child.position - player.transform.position);
                    bool withinX = Mathf.Abs(diffFromPlayer.x) <= gControl.cam.orthographicSize * gControl.cam.aspect;
                    bool withinY = Mathf.Abs(diffFromPlayer.y) <= gControl.cam.orthographicSize;
                    bool withinViewport = withinX && withinY;
                    
                    if (withinViewport) {
                        return false;
                    } else {
                        Destroy(activeTutorialModules[i]);
                    }
                }
            }
            if (activeChildren == 0) {
                completedTutorials[activeTutorialModules[i].GetComponent<TutorialModule>().index] = true;
                Destroy(activeTutorialModules[i].gameObject);
            }
        }

        return true;
    }
}
