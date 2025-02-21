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
    private float oocVolumeCap = 0.20f;
    private float icVolumeCap = 0.20f;

    // whether to override the combat/out of combat fade for player death
    private bool fadeOverride = false;

    // Sound effects
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
    public AudioClip forcefieldZap;
    public AudioClip forcefieldBounce;
    public AudioClip caseSlideDown;
    public AudioClip book;

    // Voice
    public AudioClip missionStart;
    public AudioClip launchTutorial;
    public AudioClip enemyDefeatTutorial;
    public AudioClip approachBossTutorial;
    public AudioClip playerDeath;
    public AudioClip bossDefeatTutorial;
    public AudioClip fuelCorePickup;
    public AudioClip healthCorePickup;
    public AudioClip knowledgeAdded;
    public AudioClip artifact1;
    public AudioClip artifact2;
    public AudioClip artifact3;
    public AudioClip artifact4;
    public AudioClip artifact5;
    public AudioClip artifact6;
    public AudioClip artifact7;
    public AudioClip artifact8;
    public AudioClip checkpointTutorial;
    public AudioClip enterSector1;
    public AudioClip enterSector2;
    public AudioClip enterSector3;
    public AudioClip enterSector4;
    public AudioClip enterSector5;
    public AudioClip enterSector6;
    public AudioClip enterSector7;
    public AudioClip enterSector8;


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
        EventManager.onForcefieldHit += PlayForcefieldZap;
        EventManager.onForcefieldBounce += PlayForcefieldBounce;
        EventManager.onArtifactObtain += PlayCaseSlideDown;

        EventManager.onEnterBossArea += EnterCombat;
        EventManager.onExitBossArea += ExitCombat;
        

        BGMSourceOOC.clip = outOfCombatBGM;
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
        if (fadeOverride) {
            yield break;
        }
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

    public void PlayGetArtifact(int id) {
        PlaySFX(getArtifact, 0.85f);
        if (id % 10 == 0) {
            Timing.RunCoroutine(_PlayArtifactPop());
        }
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
        Timing.RunCoroutine(_DeathFade());
        fadeOverride = true;
    }

    private IEnumerator<float> _DeathFade() {
        float time = 0;
        if (combat) {
            while (time < 1) {
                BGMSourceIC.volume = icVolumeCap - time * icVolumeCap;
                yield return Timing.WaitForOneFrame;
                time += Time.deltaTime;
            }
            BGMSourceIC.volume = 0;
            BGMSourceOOC.Play();
        } else {
            while (time < 1) {
                BGMSourceOOC.volume = oocVolumeCap - time * oocVolumeCap;
                yield return Timing.WaitForOneFrame;
                time += Time.deltaTime;
            }
            BGMSourceOOC.volume = 0;
        }
        yield return Timing.WaitForSeconds(4);
        
        
        time = 0;
        while (time < 1) {
            BGMSourceOOC.volume = time * oocVolumeCap;
            yield return Timing.WaitForOneFrame;
            time += Time.deltaTime;
        }
        BGMSourceOOC.volume = oocVolumeCap;

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

    public void PlayForcefieldZap() {
        PlaySFX(forcefieldZap, 1);
    }

    public void PlayForcefieldBounce() {
        PlaySFX(forcefieldBounce, 1);
    }

    public void PlayCaseSlideDown(int _) {
        PlaySFX(caseSlideDown, 1);
        Timing.RunCoroutine(_PlayBook());
    }

    private IEnumerator<float> _PlayBook() {
        yield return Timing.WaitForSeconds(1.7f);
        PlaySFX(book, 1);
    }

    // Voice functions

    public void PlayMissionStart() {
        PlaySFX(missionStart, 1);
        Timing.RunCoroutine(_BGMVolumeRise());
    }

    private IEnumerator<float> _BGMVolumeRise() {
        BGMSourceOOC.volume = oocVolumeCap / 2;
        yield return Timing.WaitForSeconds(10.6f);
        float time = 0;
        while (time < 1) {
            BGMSourceOOC.volume = oocVolumeCap / 2 + time * oocVolumeCap / 2;
            yield return Timing.WaitForOneFrame;
            time += Time.deltaTime;
        }
        BGMSourceOOC.volume = oocVolumeCap;
    }

    public void PlayLaunchTutorial() {
        PlaySFX(launchTutorial, 1);
    }

    public void PlayEnemyDefeatTutorial() {
        PlaySFX(enemyDefeatTutorial, 1);
    }

    public void PlayApproachBossTutorial() {
        PlaySFX(approachBossTutorial, 1);
    }

    public void PlayPlayerDeath() {
        PlaySFX(playerDeath, 1);
    }

    public void PlayBossDefeatTutorial() {
        PlaySFX(bossDefeatTutorial, 1);
    }

    public void PlayFuelCorePickup() {
        PlaySFX(fuelCorePickup, 1);
    }

    public void PlayHealthCorePickup() {
        PlaySFX(healthCorePickup, 1);
    }

    public void PlayKnowledgeAdded() {
        PlaySFX(knowledgeAdded, 1);
    }

    public void PlayArtifact1() {
        PlaySFX(artifact1, 1);
    }

    public void PlayArtifact2() {
        PlaySFX(artifact2, 1);
    }

    public void PlayArtifact3() {
        PlaySFX(artifact3, 1);
    }

    public void PlayArtifact4() {
        PlaySFX(artifact4, 1);
    }

    public void PlayArtifact5() {
        PlaySFX(artifact5, 1);
    }

    public void PlayArtifact6() {
        PlaySFX(artifact6, 1);
    }

    public void PlayArtifact7() {
        PlaySFX(artifact7, 1);
    }

    public void PlayArtifact8() {
        PlaySFX(artifact8, 1);
    }

    public void PlayCheckpointTutorial() {
        PlaySFX(checkpointTutorial, 1);
    }

    public void PlayEnterSector1() {
        PlaySFX(enterSector1, 1);
    }

    public void PlayEnterSector2() {
        PlaySFX(enterSector2, 1);
    }

    public void PlayEnterSector3() {
        PlaySFX(enterSector3, 1);
    }

    public void PlayEnterSector4() {
        PlaySFX(enterSector4, 1);
    }

    public void PlayEnterSector5() {
        PlaySFX(enterSector5, 1);
    }

    public void PlayEnterSector6() {
        PlaySFX(enterSector6, 1);
    }

    public void PlayEnterSector7() {
        PlaySFX(enterSector7, 1);
    }

    public void PlayEnterSector8() {
        PlaySFX(enterSector8, 1);
    }

    public void PlaySFX(AudioClip clip, float vol = 1) {
        SFXSource.PlayOneShot(clip, vol);
    }
}
