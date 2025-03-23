using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MEC;

public class MessageDashboard : MonoBehaviour
{
    private Text textComponent;
    private Sound sound;
    private int recentSectorNum = 0;

    void Start() {
        textComponent = gameObject.GetComponent<Text>();

        EventManager.onEnemyDefeat += EnemyDefeatMsg;

        EventManager.onEnterCluster += EnterSectorMsg;
        EventManager.onEnterBossArea += ApproachingBossMsg;
        EventManager.onPlayerDeath += PlayerDeathMsg;
        EventManager.onBossDefeat += BossDefeatMsg;
        EventManager.onArtifactPickup += CorePickupMsg;
        EventManager.onArtifactObtain += ArtifactObtainMsg;

        sound = GameObject.FindWithTag("Sound").GetComponent<Sound>();
        Timing.RunCoroutine(_LaunchTimer());
    }

    private IEnumerator<float> _LaunchTimer() {
        sound.PlayMissionStart();
        yield return Timing.WaitForSeconds(12.5f);
        Launch();
    }

    public void Launch() {
        ChangeTextTo("Use arrow keys to move.\n\nYour compass leads to nearby objectives.\n\nPress 1 to use your blade weapon.");
        sound.PlayLaunchTutorial();
    }

    public void EnemyDefeatMsg() {
        ChangeTextTo("You found a fuel canister!\n\nFly over it to pick it up.");
        sound.PlayEnemyDefeatTutorial();
        EventManager.onEnemyDefeat -= EnemyDefeatMsg;
    }

    public void EnterSectorMsg(int sectorNum) {
        if (recentSectorNum != sectorNum) {
            ChangeTextTo($"Entering Sector {sectorNum}.");
            switch (sectorNum) {
                case 1:
                    sound.PlayEnterSector1();
                    break;
                case 2:
                    sound.PlayEnterSector2();
                    break;
                case 3:
                    sound.PlayEnterSector3();
                    break;
                case 4:
                    sound.PlayEnterSector4();
                    break;
                case 5:
                    sound.PlayEnterSector5();
                    break;
                case 6:
                    sound.PlayEnterSector6();
                    break;
                case 7:
                    sound.PlayEnterSector7();
                    break;
                case 8:
                    sound.PlayEnterSector8();
                    break;
            }
            Timing.RunCoroutine(_SectorMsgTimer());
            recentSectorNum = sectorNum;
        }
    }

    // Avoid repetitive sector messages
    private IEnumerator<float> _SectorMsgTimer() {
        yield return Timing.WaitForSeconds(15);
        recentSectorNum = 0;
    }

    public void ApproachingBossMsg(int _) {
        ChangeTextTo("Powerful enemy ahead!\n\nLook for its weakness.");
        sound.PlayApproachBossTutorial();
        EventManager.onEnterBossArea -= ApproachingBossMsg;
    }

    public void PlayerDeathMsg() {
        string droneDefeatText = "Your drone was defeated.\n\nLet's try again from the last checkpoint.";
        ChangeTextTo(droneDefeatText);
        sound.PlayPlayerDeath();
        Timing.RunCoroutine(_PlayerDeathMsgTimer(droneDefeatText));
    }

    private IEnumerator<float> _PlayerDeathMsgTimer(string ddtext) {
        yield return Timing.WaitForSeconds(5);
        if (textComponent.text == ddtext) {
            ChangeTextTo("");
        }
    }

    public void BossDefeatMsg(int _) {
        ChangeTextTo($"You found an artifact!\n\nFly over it to pick it up.");
        sound.PlayBossDefeatTutorial();
    }

    public void CorePickupMsg(int id) {
        if (id % 10 == 0) {
            return;
        }
        if (id == 21 || id == 41 || id == 61 || id == 81) {
            ChangeTextTo("Max fuel has been increased!");
            sound.PlayFuelCorePickup();
        } else {
            ChangeTextTo("Max health has been increased!");
            sound.PlayHealthCorePickup();
        }
    }

    public void ArtifactObtainMsg(int id) {
        Timing.RunCoroutine(_ArtifactRouter(id));
    }

    private IEnumerator<float> _ArtifactRouter(int firstDigit) {
        yield return Timing.WaitForSeconds(1.8f);
        ChangeTextTo("Knowledge has been added to the library.");
        sound.PlayKnowledgeAdded();
        yield return Timing.WaitForSeconds(4);
        switch (firstDigit) {
            case 1:
                ChangeTextTo("Your blade skill now has a longer range!");
                sound.PlayArtifact1();
                yield return Timing.WaitForSeconds(4);
                PreCheckpointMsg();
                break;
            case 2:
                ChangeTextTo("New skill unlocked!\n\nPress 2 to dash and set a trap.");
                sound.PlayArtifact2();
                break;
            case 3:
                ChangeTextTo("Your trap skill now has a shorter cooldown!");
                sound.PlayArtifact3();
                break;
            case 4:
                ChangeTextTo("New skill unlocked!\n\nPress 3 to raise a shield and reflect attacks.");
                sound.PlayArtifact4();
                break;
            case 5:
                ChangeTextTo("Your shield skill now has a shorter cooldown!");
                sound.PlayArtifact5();
                break;
            case 6:
                ChangeTextTo("Your trap skill now has a larger radius!");
                sound.PlayArtifact6();
                break;
            case 7:
                ChangeTextTo("Your shield skill now has a longer duration!");
                sound.PlayArtifact7();
                break;
            case 8:
                ChangeTextTo(""); // TBD
                sound.PlayArtifact8();
                break;
        }
    }

    public void PreCheckpointMsg() {
        ChangeTextTo("Touching down on the pedestal sets a checkpoint and restores your health and fuel.");
        sound.PlayCheckpointTutorial();
    }

    private void ChangeTextTo(string msg) {
        textComponent.text = msg;
    }
}
