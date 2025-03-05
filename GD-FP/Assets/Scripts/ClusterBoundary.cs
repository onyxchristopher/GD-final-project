using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterBoundary : MonoBehaviour
{
    // cluster id
    private int id;

    // module to spawn
    [SerializeField] private GameObject[] entryTutorialModules;

    // keep track of the modules
    public static GameObject[] modules;
    
    // player
    private GameObject player;

    // GameController
    private GameController gControl;

    // distance from the camera edge to spawn the tutorial
    private float offsetDist = 5;

    // distance from the player at which the tutorial should not be despawned

    void Start() {
        player = GameObject.FindWithTag("Player");
        gControl = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        ResetAll();
    }

    public void ResetAll() {
        if (modules[id - 1]) {
            Destroy(modules[id - 1]);
        }
        modules[id - 1] = Instantiate(entryTutorialModules[id - 1], Generation.farAway, Quaternion.identity, transform);
        modules[id - 1].SetActive(false);
    }
    
    public void setId(int newId) {
        id = newId;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            EventManager.EnterCluster(id);
            int index = id - 1;

            // determine if the player has other tutorial modules on screen
            bool toSpawn;
            if (index <= 2) {
                toSpawn = true;
            } else {
                toSpawn = false;
            }
            

            if (index <= 2 && !modules[index].GetComponent<TutorialModule>().complete && toSpawn) {
                // the tutorial has not been completed, so it should be spawned
                Vector2 playerLocation = other.gameObject.GetComponent<Rigidbody2D>().position;
                Vector2 rootLocation = (Vector2) transform.position;
                Vector2 diff = rootLocation - playerLocation;
                float diffAngle = Vector2.Angle(Vector2.right, diff) * Mathf.Deg2Rad;

                // calculate the intersection of the camera edge and the diff line
                float aspectAngle = Mathf.Atan(1 / gControl.cam.aspect);

                float distToEdge = 0;

                // calculate the distance to the camera edge along the diff line
                if (diffAngle < aspectAngle || diffAngle > Mathf.PI - aspectAngle) {
                    // intersects the left or right edge, so horizontal distance is used
                    distToEdge = gControl.cam.orthographicSize * gControl.cam.aspect / Mathf.Abs(Mathf.Cos(diffAngle));
                } else {
                    // intersects the top or bottom edge, so vertical distance is used
                    distToEdge = gControl.cam.orthographicSize / Mathf.Abs(Mathf.Sin(diffAngle));
                }

                Vector2 tutSpawnLoc = playerLocation + diff.normalized * (distToEdge + offsetDist);
                if (index == 2) {
                    Scenes scenes = GameObject.FindWithTag("GameController").GetComponent<Scenes>();
                    bool spawn = scenes.TSpawnCheck(tutSpawnLoc, Vector2.one * 15, index);
                    if (!spawn) {
                        return;
                    }
                }
                
                modules[index].SetActive(true);
                modules[index].transform.position = tutSpawnLoc;
                modules[index].GetComponent<TutorialModule>().complete = true;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        EventManager.ExitCluster(id);
        /*if (id <= 2 && !ChildrenInViewport(modules[id - 1])) {
            modules[id - 1].SetActive(false);
        }*/

    }

    /*
    private bool ChildrenInViewport(GameObject module) {
        int activeChildren = 0;
        for (int i = 0; i < module.transform.childCount; i++) {
            Transform child = module.transform.GetChild(i);
            if (child.gameObject.activeSelf) {
                activeChildren++;

                // if any active child from this cluster module is in the viewport, do not spawn new

                Vector3 diffFromPlayer = (child.position - player.transform.position);
                bool withinX = Mathf.Abs(diffFromPlayer.x) <= gControl.cam.orthographicSize * gControl.cam.aspect;
                bool withinY = Mathf.Abs(diffFromPlayer.y) <= gControl.cam.orthographicSize;
                bool withinViewport = withinX && withinY;

                if (withinViewport) {
                    return true;
                }
            }
        }

        // if no enemies left in the module, it is completed
        if (activeChildren == 0) {
            module.GetComponent<TutorialModule>().complete = true;
            module.SetActive(false);
        }

        return false;
    }*/
}
