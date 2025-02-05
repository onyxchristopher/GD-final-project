using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
The fire chaser has two states: IDLE and ATTACK.
It starts in IDLE, and when its trigger is entered by the player, moves to ATTACK.
In ATTACK, it moves towards the player at a constant rate.
It also fires at the player every [delay] seconds.
It moves back to IDLE, returning to its spawnpoint, when its trigger is exited by the player.
*/

public class FireChaserEnemy : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
