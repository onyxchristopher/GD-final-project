using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
    public enum gameState {
        running,
        paused
    }

    void Start() {
        Screen.SetResolution(1080, 1080, true);
    }

    public void Pause() {

    }
}
