using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEnemy : MonoBehaviour
{
    [SerializeField] entryTutorialModule;
    private GameObject player;
    void Start() {
        player = GameObject.FindWithTag("Player");
        EventManager.onEnterCluster += SpawnModule;
    }

    public void SpawnModule(int clusterNum) {

    }
}
