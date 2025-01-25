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

        EventManager.onEnterCluster += EnterClusterMsg;
        EventManager.onExitCluster += ExitClusterMsg;
    }

    public void EnterClusterMsg(int clusterNum) {
        ChangeTextTo($"Entering cluster {clusterNum}");
    }

    public void ExitClusterMsg(int clusterNum) {
        ChangeTextTo($"Leaving cluster {clusterNum}");
    }

    private void ChangeTextTo(string msg) {
        textComponent.text = msg;
    }
}
