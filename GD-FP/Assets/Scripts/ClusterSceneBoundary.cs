using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterSceneBoundary : MonoBehaviour
{
    private int id;

    private Scenes scenes;

    void Start() {
        scenes = GameObject.FindWithTag("GameController").GetComponent<Scenes>();
    }
    
    public void setId(int newId) {
        id = newId;
    }

    void OnTriggerEnter2D(Collider2D other) {
        scenes.Load(id);
    }

    void OnTriggerExit2D(Collider2D other) {
        scenes.Unload(id);
    }
}
