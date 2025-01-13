using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class PlayerAbilities : MonoBehaviour
{
    // Attack prefabs
    [SerializeField] private GameObject blade;
    [SerializeField] private GameObject trap;
    [SerializeField] private GameObject shield;

    // Attack settings
    private float bladeLength = 5;
    private int bladeDamage = 5;
    private int trapDamage = 5;
    private float shieldDuration = 1f;


    private float trapCooldown = 2f;
    private bool trapOnCd = false;

    private float shieldCooldown = 5f;
    private bool shieldOnCd = false;

    void Start() {
        
    }

    private void OnAction1() {
        if (!GameObject.FindWithTag("Blade")) {
            GameObject bladeInstance = Instantiate(blade, transform.position + Vector3.up * bladeLength / 2, Quaternion.identity, transform);
            bladeInstance.GetComponent<Blade>().SetDamage(bladeDamage);
        } 
    }

    private void OnAction2() {
        if (!trapOnCd) {
            // move + trap
            GameObject trapInstance = Instantiate(trap, transform.position, Quaternion.identity);
            trapInstance.GetComponent<Trap>().SetDamage(trapDamage);
            trapOnCd = true;
            Timing.RunCoroutine(_TrapCooldown());
        }
    }

    private void OnAction3() {
        if (!shieldOnCd) {
            // shield
            GameObject shieldInstance = Instantiate(shield, transform.position, Quaternion.identity, transform);
            shieldInstance.GetComponent<Shield>().SetDuration(shieldDuration);
            shieldOnCd = true;
            Timing.RunCoroutine(_ShieldCooldown());
        }
    }

    private IEnumerator<float> _TrapCooldown() {
        yield return Timing.WaitForSeconds(trapCooldown);
        trapOnCd = false;
    }

    private IEnumerator<float> _ShieldCooldown() {
        yield return Timing.WaitForSeconds(shieldCooldown);
        shieldOnCd = false;
    }

}
