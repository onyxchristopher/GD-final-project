using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class Sound : MonoBehaviour
{
    [SerializeField] private AudioSource BGMSourceOOC;
    [SerializeField] private AudioSource BGMSourceIC;
    [SerializeField] private AudioSource SFXSource;

    public AudioClip outOfCombatBGM;
    public AudioClip inCombatBGM;

    private bool combat = false;
    private float fadeTime = 1.5f;
    private float fadeTimeDelay = 0.5f;
    private float oocVolumeCap = 0.35f;
    private float icVolumeCap = 0.35f;
    

    public AudioClip getArtifact;
    public AudioClip blade;
    public AudioClip playerHit;
    public AudioClip enemyHit;
    public AudioClip fuelPickup;
    public AudioClip pop;
    public AudioClip respawn;
    public AudioClip respawnEnd;

    void Start() {
        EventManager.onArtifactPickup += PlayGetArtifact;
        EventManager.onBladeUse += PlayBlade;
        EventManager.onPlayerHit += PlayPlayerHit;
        EventManager.onEnemyHit += PlayEnemyHit;
        EventManager.onFuelPickup += PlayFuelPickup;
        EventManager.onPlayerDeath += PlayRespawn;
        EventManager.onPlayerRespawn += PlayRespawnEnd;
        EventManager.onEnterBossArea += EnterCombat;
        EventManager.onExitBossArea += ExitCombat;

        BGMSourceOOC.clip = outOfCombatBGM;
        BGMSourceOOC.volume = oocVolumeCap;
        BGMSourceIC.clip = inCombatBGM;
        BGMSourceIC.volume = 0;

        BGMSourceOOC.Play();
    }

    public void EnterCombat(string _) {
        combat = true;
        Timing.RunCoroutine(_Fade());
    }

    public void ExitCombat() {
        combat = false;
        Timing.RunCoroutine(_Fade());
    }

    public void PlayBGM() {

    }

    public void PauseBGM() {

    }

    public void StopBGM() {

    }

    private IEnumerator<float> _Fade() {
        float time = 0;
        if (combat) {
            BGMSourceIC.Play();
            while (time < fadeTime + fadeTimeDelay) {
                if (time < fadeTimeDelay) {
                    // fade out noncombat music
                    BGMSourceOOC.volume -= Time.deltaTime * oocVolumeCap / fadeTime;
                } else if (time > fadeTime) {
                    // fade in combat music
                    BGMSourceIC.volume += Time.deltaTime * icVolumeCap / fadeTime;
                } else {
                    // fade in combat music, fade out noncombat music
                    BGMSourceOOC.volume -= Time.deltaTime * oocVolumeCap / fadeTime;
                    BGMSourceIC.volume += Time.deltaTime * icVolumeCap / fadeTime;
                }
                yield return Timing.WaitForOneFrame;
                time += Time.deltaTime;
                Debug.Log($"ooc: {BGMSourceOOC.volume}, ic: {BGMSourceIC.volume}");
            }
            if (BGMSourceIC.volume > icVolumeCap) {
                BGMSourceIC.volume = icVolumeCap;
            }
            if (BGMSourceOOC.volume < 0) {
                BGMSourceOOC.volume = 0;
            }
            BGMSourceOOC.Pause();
        } else {
            BGMSourceOOC.Play();
            while (time < fadeTime + fadeTimeDelay) {
                if (time < fadeTimeDelay) {
                    // fade out combat music
                    BGMSourceIC.volume -= Time.deltaTime * oocVolumeCap / fadeTime;
                } else if (time > fadeTime) {
                    // fade in noncombat music
                    BGMSourceOOC.volume += Time.deltaTime * icVolumeCap / fadeTime;
                } else {
                    // fade out combat music, fade in noncombat music
                    BGMSourceIC.volume -= Time.deltaTime * oocVolumeCap / fadeTime;
                    BGMSourceOOC.volume += Time.deltaTime * icVolumeCap / fadeTime;
                }
                yield return Timing.WaitForOneFrame;
                time += Time.deltaTime;
                Debug.Log($"ooc: {BGMSourceOOC.volume}, ic: {BGMSourceIC.volume}");
            }
            if (BGMSourceOOC.volume > oocVolumeCap) {
                BGMSourceOOC.volume = oocVolumeCap;
            }
            if (BGMSourceIC.volume < 0) {
                BGMSourceIC.volume = 0;
            }
            BGMSourceIC.Stop();
        }
    }

    public void PlayGetArtifact(int _) {
        PlaySFX(respawn, 0.85f);
        //Timing.RunCoroutine(_PlayArtifactPop());
    }

    private IEnumerator<float> _PlayArtifactPop() {
        yield return Timing.WaitForSeconds(2.25f);
        PlaySFX(pop, 0.15f);
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

    public void PlayRespawn() {
        PlaySFX(respawn, 0.8f);
    }

    public void PlayRespawnEnd() {
        //PlaySFX(respawnEnd, 1);
    }

    public void PlaySFX(AudioClip clip, float vol = 1) {
        SFXSource.PlayOneShot(clip, vol);
    }
}
