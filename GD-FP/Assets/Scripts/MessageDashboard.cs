using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageDashboard : MonoBehaviour
{
    private Text textComponent;

    private bool weaponTutorialMsgReached = false;
    private bool fuelPickupMsgReached = false;

    // Start is called before the first frame update
    void Start()
    {
        textComponent = gameObject.GetComponent<Text>();

        EventManager.onEnterEnemyArea += WeaponTutorialMsg;

        //EventManager.onEnterCluster += EnterClusterMsg;
        //EventManager.onExitCluster += ExitClusterMsg;
        EventManager.onEnterBossArea += ApproachingBossMsg;
        EventManager.onBossDefeat += BossDefeatMsg;
        EventManager.onArtifactPickup += ArtifactPickupMsg;
        EventManager.onEnemyDefeat += FuelPickupMsg;
        EventManager.onSetSpawn += CheckpointSetMsg;
    }

    public void EnterClusterMsg(int clusterNum) {
        ChangeTextTo($"Entering cluster {clusterNum}");
    }

    public void ExitClusterMsg(int clusterNum) {
        ChangeTextTo($"Leaving cluster {clusterNum}");
    }

    public void BossDefeatMsg(string bossName) {
        ChangeTextTo($"{bossName} defeated.\nCongratulations! You can pick up the artifact to improve your abilities.");
    }

    public void ArtifactPickupMsg(int id) {
        if (id == 10) {
            ChangeTextTo("Blade length increased by 40%!\nThat's all for the demo. Thanks for playing!");
        }
    }

    public void WeaponTutorialMsg() {
        if (!weaponTutorialMsgReached) {
            ChangeTextTo("Your blade ability can hit enemies.\n\nPress 1 to use it.");
            weaponTutorialMsgReached = true;
        }
        
    }

    public void FuelPickupMsg() {
        if (!fuelPickupMsgReached) {
            ChangeTextTo("Destroyed enemies can drop canisters that replenish some of your fuel.");
            fuelPickupMsgReached = true;
        }
        
    }

    public void ApproachingBossMsg(string bossName) {
        ChangeTextTo("The artifact is guarded by a powerful foe that you must figure out how to defeat.");
    }

    public void CheckpointSetMsg(Vector3 spawn) {
        ChangeTextTo("Checkpoint set");
    }

    private void ChangeTextTo(string msg) {
        textComponent.text = msg;
    }
}
