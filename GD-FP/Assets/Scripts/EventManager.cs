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

    public delegate void NotifyArtifactPickup(int id);
    public static event NotifyArtifactPickup onArtifactPickup;
    public static void ArtifactPickup(int id) {
        if (onArtifactPickup != null) {
            onArtifactPickup(id);
        }
    }

    public delegate void NotifyNewUniverse();
    public static event NotifyNewUniverse onNewUniverse;
    public static void NewUniverse() {
        onNewUniverse?.Invoke();
    }
}
