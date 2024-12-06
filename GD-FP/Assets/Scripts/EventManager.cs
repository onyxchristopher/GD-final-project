using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {
    public delegate void Notify();
    public static event Notify onPlayerDeath;

    public static void PlayerDeath() {
        onPlayerDeath?.Invoke();
    }
}
