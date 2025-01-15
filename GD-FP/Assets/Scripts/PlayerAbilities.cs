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

    // Starter settings

    [SerializeField] private float startingBladeLength;
    [SerializeField] private int startingBladeDamage;

    [SerializeField] private float startingTrapCooldown;
    [SerializeField] private int startingTrapDamage;

    [SerializeField] private float startingShieldCooldown;
    [SerializeField] private float startingShieldDuration;

    // Current settings
    private float bladeLength;
    private int bladeDamage;

    private float trapCooldown;
    private int trapDamage;

    private float shieldCooldown;
    private float shieldDuration;

    // Availability booleans
    private bool trapUnlocked = false;
    private bool trapOnCd = false;

    private bool shieldUnlocked = false;
    private bool shieldOnCd = false;

    // Script refs
    private PlayerMovement pMove;

    void Start() {
        EventManager.onNewUniverse += InitializeAbilities;

        pMove = gameObject.GetComponent<PlayerMovement>();
    }

    public void InitializeAbilities() {
        bladeLength = startingBladeLength;
        bladeDamage = startingBladeDamage;
        trapCooldown = startingTrapCooldown;
        trapDamage = startingTrapDamage;
        shieldCooldown = startingShieldCooldown;
        shieldDuration = startingShieldDuration;

        trapUnlocked = false;
        shieldUnlocked = false;
    }

    private void OnAction1() {
        if (!GameObject.FindWithTag("Blade")) {
            GameObject bladeInstance = Instantiate(blade, transform.position + Vector3.up * bladeLength / 2, Quaternion.identity, transform);
            bladeInstance.GetComponent<Blade>().SetDamage(bladeDamage);
        } 
    }

    private void OnAction2() {
        if (trapUnlocked && !trapOnCd) {
            // move + trap
            pMove.QueueDash();
            GameObject trapInstance = Instantiate(trap, transform.position, Quaternion.identity);
            trapInstance.GetComponent<Trap>().SetDamage(trapDamage);
            trapOnCd = true;
            Timing.RunCoroutine(_TrapCooldown());
        }
    }

    private void OnAction3() {
        if (shieldUnlocked && !shieldOnCd) {
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

    public void UnlockTrap() {
        trapUnlocked = true;
    }

    public void UnlockShield() {
        shieldUnlocked = true;
    }

    public void SetBladeLength(float length) {
        bladeLength = length;
    }

    public void SetBladeDamage(int damage) {
        bladeDamage = damage;
    }

    public void SetTrapCooldown(float cd) {
        trapCooldown = cd;
    }

    public void SetTrapDamage(int damage) {
        trapDamage = damage;
    }

    public void SetShieldCooldown(float cd) {
        shieldCooldown = cd;
    }

    public void SetShieldDuration(float duration) {
        shieldDuration = duration;
    }
}
