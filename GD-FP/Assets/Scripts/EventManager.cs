using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {
    public delegate void NotifyPlayerDamage(int damage);
    public static event NotifyPlayerDamage onPlayerDamage;
    public static void PlayerDamage(int damage) {
        if (onPlayerDamage != null) {
            onPlayerDamage(damage);
        }
    }

    public delegate void NotifyPlayerDeath();
    public static event NotifyPlayerDeath onPlayerDeath;
    public static void PlayerDeath() {
        onPlayerDeath?.Invoke();
    }
}
