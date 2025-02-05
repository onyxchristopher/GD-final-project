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
    private float fadeTimeDelay = 0f;
    private float oocVolumeCap = 0.25f;
    private float icVolumeCap = 0.25f;
    

    public AudioClip getArtifact;
    public AudioClip pop;
    public AudioClip blade;
    public AudioClip playerHit;
    public AudioClip enemyHit;
    public AudioClip pickup;
    public AudioClip death;
    public AudioClip respawn;
    public AudioClip respawnEnd;
    public AudioClip explosion;

    void Start() {
        EventManager.onArtifactPickup += PlayGetArtifact;
        EventManager.onBladeUse += PlayBlade;
        EventManager.onPlayerHit += PlayPlayerHit;
        EventManager.onEnemyHit += PlayEnemyHit;
        EventManager.onPickup += PlayPickup;
        EventManager.onPlayerDeath += PlayDeath;
        EventManager.onPlayerDeath += PlayRespawn;
        EventManager.onPlayerRespawn += PlayRespawnEnd;
        EventManager.onBossDefeat += PlayExplosion;

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
            BGMSourceOOC.volume = oocVolumeCap;
            BGMSourceIC.volume = 0;
            while (time < fadeTime / 2) {
                // fade in combat music twice as fast, but only up to cap
                if (BGMSourceIC.volume < icVolumeCap) {
                    BGMSourceIC.volume += Time.deltaTime * icVolumeCap * 2 / fadeTime;
                }
                if (time > 0.25f) { // float specifying a beat in the music
                    // zero out of combat music
                    BGMSourceOOC.volume = 0;
                } else {
                    // fade out noncombat music
                    BGMSourceOOC.volume -= Time.deltaTime * oocVolumeCap / fadeTime;
                }
                yield return Timing.WaitForOneFrame;
                time += Time.deltaTime;
            }
            BGMSourceIC.volume = icVolumeCap;
            BGMSourceOOC.volume = 0;
            BGMSourceOOC.Pause();
        } else {
            BGMSourceOOC.Play();
            BGMSourceOOC.volume = 0;
            BGMSourceIC.volume = icVolumeCap;
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
            }
            BGMSourceOOC.volume = oocVolumeCap;
            BGMSourceIC.volume = 0;
            BGMSourceIC.Stop();
        }
    }

    public void PlayGetArtifact(int _) {
        PlaySFX(getArtifact, 0.85f);
        Timing.RunCoroutine(_PlayArtifactPop());
    }

    private IEnumerator<float> _PlayArtifactPop() {
        yield return Timing.WaitForSeconds(2.25f);
        PlaySFX(pop, 0.25f);
    }

    public void PlayBlade() {
        PlaySFX(blade, 0.25f);
    }

    public void PlayPlayerHit() {
        PlaySFX(playerHit, 0.6f);
    }

    public void PlayEnemyHit() {
        PlaySFX(enemyHit, 0.6f);
    }

    public void PlayPickup() {
        PlaySFX(pickup, 1);
    }

    public void PlayDeath() {
        PlaySFX(death, 1);
    }

    public void PlayRespawn() {
        PlaySFX(respawn, 0.8f);
    }

    public void PlayRespawnEnd() {
        //PlaySFX(respawnEnd, 1);
    }

    public void PlayExplosion(string _) {
        PlaySFX(explosion, 1);
    }

    public void PlaySFX(AudioClip clip, float vol = 1) {
        SFXSource.PlayOneShot(clip, vol);
    }
}
