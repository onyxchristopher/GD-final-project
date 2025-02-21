using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    [SerializeField] private bool instantCountdown;
    [SerializeField] private float timeToDestroy;
    void Start() {
        if (instantCountdown) {
            Destroy(gameObject, timeToDestroy);
        }
    }

    public void DestroyTrigger() {
        Destroy(gameObject, timeToDestroy);
    }

    
}
