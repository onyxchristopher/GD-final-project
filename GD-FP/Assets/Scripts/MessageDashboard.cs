using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MEC;

public class MessageDashboard : MonoBehaviour
{
    private Text textComponent;
    private Sound sound;

    void Start() {
        textComponent = gameObject.GetComponent<Text>();

        EventManager.onEnterEnemyArea += ApproachingEnemyMsg;
        EventManager.onEnemyDefeat += EnemyDefeatMsg;

        EventManager.onEnterCluster += EnterSectorMsg;
        EventManager.onExitCluster += ExitSectorMsg;
        EventManager.onEnterBossArea += ApproachingBossMsg;
        EventManager.onPlayerDeath += PlayerDeathMsg;
        EventManager.onBossDefeat += BossDefeatMsg;
        EventManager.onArtifactPickup += CorePickupMsg;
        EventManager.onArtifactObtain += ArtifactObtainMsg;

        sound = GameObject.FindWithTag("Sound").GetComponent<Sound>();
    }

    public void LoadedMsg() {
        ChangeTextTo("Welcome, Commander. You've arrived just in time."); // unconnected
    }

    public void ThreatMsg() {
        ChangeTextTo("Enemy forces have occupied our planets and captured our cultural artifacts."); // unconnected
    }

    public void MissionMsg() {
        ChangeTextTo("Your mission is to guide your drone through the galaxy and" + 
        " reclaim the artifacts to restore the knowledge in our planet's library."); // unconnected
    }

    public void Launch() {
        ChangeTextTo("Use arrow keys to move. To try out your blade weapon, press 1."); // unconnected
    }

    public void ApproachingEnemyMsg() {
        ChangeTextTo("Look out! Enemies ahead!");
        EventManager.onEnterEnemyArea -= ApproachingEnemyMsg;
    }

    public void EnemyDefeatMsg() {
        ChangeTextTo("You found a fuel canister! Fly over it to pick it up.");
        EventManager.onEnemyDefeat -= EnemyDefeatMsg;
    }

    public void CompassBlueMsg() {
        ChangeTextTo("Follow the blue compass arrow to the next objective."); // unconnected
    }

    public void CompassWhiteMsg() {
        ChangeTextTo("White compass arrows lead to other nearby points of interest."); // unconnected
    }

    public void EnterSectorMsg(int sectorNum) {
        ChangeTextTo($"Entering sector {sectorNum}.");
    }

    public void ExitSectorMsg(int sectorNum) {
        ChangeTextTo($"Leaving sector {sectorNum}.");
    }

    public void ApproachingBossMsg(string bossName) {
        ChangeTextTo("Powerful enemy ahead! Look for its weakness.");
        EventManager.onEnterBossArea -= ApproachingBossMsg;
    }

    public void PlayerDeathMsg() {
        ChangeTextTo("Your drone was defeated. Let's try again from the last checkpoint.");
    }

    public void BossDefeatMsg(string bossName) {
        ChangeTextTo($"You found an artifact! Fly over it to pick it up.");

    }

    public void CorePickupMsg(int id) {
        if (id % 10 == 1) {
            ChangeTextTo("Max fuel has been increased!");
        } else if (id % 10 == 2) {
            ChangeTextTo("Max health has been increased!");
        }
    }

    public void ArtifactObtainMsg(int id) {
        Timing.RunCoroutine(_ArtifactRouter(id));
    }

    private IEnumerator<float> _ArtifactRouter(int firstDigit) {
        yield return Timing.WaitForSeconds(1.8f);
        ChangeTextTo("Knowledge has been added to the library.");
        yield return Timing.WaitForSeconds(4);
        switch (firstDigit) {
            case 1:
                ChangeTextTo("Your blade skill now has a longer range!");
                yield return Timing.WaitForSeconds(4);
                PreCheckpointMsg();
                break;
            case 2:
                ChangeTextTo("New skill unlocked! Press 2 to dash and set a trap.");
                break;
            case 3:
                ChangeTextTo("Your trap skill now has a shorter cooldown!");
                break;
            case 4:
                ChangeTextTo("New skill unlocked! Press 3 to raise a shield and reflect attacks.");
                break;
            case 5:
                ChangeTextTo("Your shield skill now has a shorter cooldown!");
                break;
            case 6:
                ChangeTextTo(""); // TBD
                break;
            case 7:
                ChangeTextTo(""); // TBD
                break;
            case 8:
                ChangeTextTo(""); // TBD
                break;
        }
    }

    public void PreCheckpointMsg() {
        ChangeTextTo("Touching down on the pedestal sets a checkpoint.");
    }

    private void ChangeTextTo(string msg) {
        textComponent.text = msg;
    }
}
