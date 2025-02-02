using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour
{
    [SerializeField] private AudioSource BGMSource;
    [SerializeField] private AudioSource SFXSource;

    public AudioClip getArtifact;
    public AudioClip blade;
    public AudioClip playerHit;
    public AudioClip enemyHit;
    public AudioClip fuelPickup;

    void Start() {
        EventManager.onArtifactPickup += PlayGetArtifact;
        EventManager.onBladeUse += PlayBlade;
        EventManager.onPlayerHit += PlayPlayerHit;
        EventManager.onEnemyHit += PlayEnemyHit;
        EventManager.onFuelPickup += PlayFuelPickup;
    }

    public void PlayGetArtifact(int _) {
        PlaySFX(getArtifact, 0.6f);
    }

    public void PlayBlade() {
        PlaySFX(blade, 0.25f);
    }

    public void PlayPlayerHit() {
        PlaySFX(playerHit, 1);
    }

    public void PlayEnemyHit() {
        PlaySFX(enemyHit, 0.5f);
    }

    public void PlayFuelPickup() {
        PlaySFX(fuelPickup, 1);
    }

    public void PlaySFX(AudioClip clip, float vol = 1) {
        SFXSource.PlayOneShot(clip, vol);
    }
}
