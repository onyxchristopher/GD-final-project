using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttacks : MonoBehaviour
{
    // Attack prefabs
    [SerializeField] private GameObject blade;
    [SerializeField] private GameObject trap;

    // Attack settings
    private float bladeLength = 5;

    void Start() {
        
    }

    private void OnAction1() {
        if (!GameObject.FindWithTag("Blade")) {
            Instantiate(blade, transform.position + Vector3.up * bladeLength / 2, Quaternion.identity, transform);
        }
        
    }
}
