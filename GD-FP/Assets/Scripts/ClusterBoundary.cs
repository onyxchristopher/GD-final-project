using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterBoundary : MonoBehaviour
{
    private int id;
    
    public void setId(int newId) {
        id = newId;
    }

    void OnTriggerEnter2D(Collider2D other) {
        EventManager.EnterCluster(id);
    }

    void OnTriggerExit2D(Collider2D other) {
        EventManager.ExitCluster(id);
    }
}
