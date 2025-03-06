using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
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

    public delegate void NotifyPlayerRespawn();
    public static event NotifyPlayerRespawn onPlayerRespawn;
    public static void PlayerRespawn() {
        onPlayerRespawn?.Invoke();
    }

    public delegate void NotifyArtifactPickup(int id);
    public static event NotifyArtifactPickup onArtifactPickup;
    public static void ArtifactPickup(int id) {
        if (onArtifactPickup != null) {
            onArtifactPickup(id);
        }
    }

    public delegate void NotifyArtifactObtain(int id);
    public static event NotifyArtifactObtain onArtifactObtain;
    public static void ArtifactObtain(int id) {
        if (onArtifactObtain != null) {
            onArtifactObtain(id);
        }
    }

    public delegate void NotifyMinorObjectiveComplete(int sectorId, int objectiveId);
    public static event NotifyMinorObjectiveComplete onMinorObjectiveComplete;
    public static void MinorObjectiveComplete(int sectorId, int objectiveId) {
        if (onMinorObjectiveComplete != null) {
            onMinorObjectiveComplete(sectorId, objectiveId);
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

    public delegate void NotifyEnterBossArea(int sectorId);
    public static event NotifyEnterBossArea onEnterBossArea;
    public static void EnterBossArea(int sectorId) {
        if (onEnterBossArea != null) {
            onEnterBossArea(sectorId);
        }
    }

    public delegate void NotifyExitBossArea();
    public static event NotifyExitBossArea onExitBossArea;
    public static void ExitBossArea() {
        onExitBossArea?.Invoke();
    }

    public delegate void NotifyBossDefeat(int sectorId);
    public static event NotifyBossDefeat onBossDefeat;
    public static void BossDefeat(int sectorId) {
        if (onBossDefeat != null) {
            onBossDefeat(sectorId);
        }
    }

    public delegate void NotifyEnemyDefeat();
    public static event NotifyEnemyDefeat onEnemyDefeat;
    public static void EnemyDefeat() {
        onEnemyDefeat?.Invoke();
    }

    public delegate void NotifyBladeUse();
    public static event NotifyBladeUse onBladeUse;
    public static void BladeUse() {
        onBladeUse?.Invoke();
    }

    public delegate void NotifyTrapUse();
    public static event NotifyTrapUse onTrapUse;
    public static void TrapUse() {
        onTrapUse?.Invoke();
    }

    public delegate void NotifyPlayerHit();
    public static event NotifyPlayerHit onPlayerHit;
    public static void PlayerHit() {
        onPlayerHit?.Invoke();
    }

    public delegate void NotifyEnemyHit();
    public static event NotifyEnemyHit onEnemyHit;
    public static void EnemyHit() {
        onEnemyHit?.Invoke();
    }

    public delegate void NotifyPickup();
    public static event NotifyPickup onPickup;
    public static void Pickup() {
        onPickup?.Invoke();
    }

    public delegate void NotifySetSpawn(Vector3 spawn);
    public static event NotifySetSpawn onSetSpawn;
    public static void SetSpawn(Vector3 spawn) {
        if (onSetSpawn != null) {
            onSetSpawn(spawn);
        }
    }

    public delegate void NotifyForcefieldHit();
    public static event NotifyForcefieldHit onForcefieldHit;
    public static void ForcefieldHit() {
        onForcefieldHit?.Invoke();
    }

    public delegate void NotifyForcefieldBounce();
    public static event NotifyForcefieldBounce onForcefieldBounce;
    public static void ForcefieldBounce() {
        onForcefieldBounce?.Invoke();
    }

    public delegate void NotifyLaserCharge();
    public static event NotifyLaserCharge onLaserCharge;
    public static void LaserCharge() {
        onLaserCharge?.Invoke();
    }

    public delegate void NotifyLaserZap();
    public static event NotifyLaserZap onLaserZap;
    public static void LaserZap() {
        onLaserZap?.Invoke();
    }
}
