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

    public delegate void NotifyEnterCluster(int clusterNum);
    public static event NotifyEnterCluster onEnterCluster;
    public static void EnterCluster(int clusterNum) {
        if (onEnterCluster != null) {
            onEnterCluster(clusterNum);
        }
    }

    public delegate void NotifyExitCluster(int clusterNum);
    public static event NotifyExitCluster onExitCluster;
    public static void ExitCluster(int clusterNum) {
        if (onExitCluster != null) {
            onExitCluster(clusterNum);
        }
    }

    public delegate void NotifyEnterBossArea(string bossName);
    public static event NotifyEnterBossArea onEnterBossArea;
    public static void EnterBossArea(string bossName) {
        if (onEnterBossArea != null) {
            onEnterBossArea(bossName);
        }
    }

    public delegate void NotifyExitBossArea();
    public static event NotifyExitBossArea onExitBossArea;
    public static void ExitBossArea() {
        onExitBossArea?.Invoke();
    }

    public delegate void NotifyBossDefeat(string bossName);
    public static event NotifyBossDefeat onBossDefeat;
    public static void BossDefeat(string bossName) {
        if (onBossDefeat != null) {
            onBossDefeat(bossName);
        }
    }

    public delegate void NotifyEnterEnemyArea();
    public static event NotifyEnterEnemyArea onEnterEnemyArea;
    public static void EnterEnemyArea() {
        onEnterEnemyArea?.Invoke();
    }
}
