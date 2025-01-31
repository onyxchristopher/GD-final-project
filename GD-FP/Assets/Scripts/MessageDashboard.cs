using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageDashboard : MonoBehaviour
{
    private Text textComponent;

    // Start is called before the first frame update
    void Start()
    {
        textComponent = gameObject.GetComponent<Text>();

        EventManager.onEnterEnemyArea += WeaponTutorialMsg;

        //EventManager.onEnterCluster += EnterClusterMsg;
        //EventManager.onExitCluster += ExitClusterMsg;
        EventManager.onEnterBossArea += ApproachingBossMsg;
        EventManager.onBossDefeat += BossDefeatedMsg;
        EventManager.onArtifactPickup += ArtifactPickupMsg;
        
    }

    public void EnterClusterMsg(int clusterNum) {
        ChangeTextTo($"Entering cluster {clusterNum}");
    }

    public void ExitClusterMsg(int clusterNum) {
        ChangeTextTo($"Leaving cluster {clusterNum}");
    }

    public void BossDefeatedMsg(string bossName) {
        ChangeTextTo($"{bossName} defeated.\nCongratulations! You can pick up the artifact to improve your abilities.");
    }

    public void ArtifactPickupMsg(int id) {
        if (id == 10) {
            ChangeTextTo("Blade length increased by 40%!\nThat's all for the demo. Thanks for playing!");
        }
    }

    public void WeaponTutorialMsg() {
        ChangeTextTo("Your blade ability can hit enemies.\n\nPress 1 to use it.");
    }

    public void ApproachingBossMsg(string bossName) {
        ChangeTextTo("The artifact is guarded by a powerful foe that you must figure out how to defeat.");
    }

    private void ChangeTextTo(string msg) {
        textComponent.text = msg;
    }
}
