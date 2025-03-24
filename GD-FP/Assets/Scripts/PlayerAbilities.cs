using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MEC;

public class PlayerAbilities : MonoBehaviour
{
    // Attack prefabs
    [SerializeField] private GameObject blade;
    [SerializeField] private GameObject trap;
    [SerializeField] private GameObject shield;

    // Longer blade prefab
    [SerializeField] private GameObject longBlade;

    // Starter settings
    [SerializeField] private float startingBladeCooldown;
    [SerializeField] private float startingBladeLength;
    [SerializeField] private int startingBladeDamage;

    [SerializeField] private float startingTrapCooldown;
    [SerializeField] private int startingTrapDamage;

    [SerializeField] private float startingShieldCooldown;
    [SerializeField] private float startingShieldDuration;

    // Current settings
    private float bladeCooldownTime;
    private float bladeLength;
    private int bladeDamage;
    

    private float trapCooldownTime;
    private int trapDamage;

    private float shieldCooldownTime;
    private float shieldDuration;

    // Availability
    private float bladeCd;

    private bool trapUnlocked = false;
    private float trapCd;

    private bool shieldUnlocked = false;
    private float shieldCd;


    // Script refs
    private PlayerMovement pMove;
    
    // UI refs
    private Slider bladeCdSlider;
    private Slider trapCdSlider;
    private Slider shieldCdSlider;

    // CD icon sprite refs
    [SerializeField] private Sprite trapSprite;
    [SerializeField] private Sprite shieldSprite;
    [SerializeField] private Sprite lockSprite;

    void Start() {
        EventManager.onNewUniverse += InitializeAbilities;

        pMove = gameObject.GetComponent<PlayerMovement>();

        GameObject cooldownIcons = GameObject.FindWithTag("CooldownIcons");
        bladeCdSlider = cooldownIcons.transform.GetChild(0).gameObject.GetComponent<Slider>();
        trapCdSlider = cooldownIcons.transform.GetChild(1).gameObject.GetComponent<Slider>();
        shieldCdSlider = cooldownIcons.transform.GetChild(2).gameObject.GetComponent<Slider>();
    }

    public void InitializeAbilities() {
        bladeCooldownTime = startingBladeCooldown;
        bladeLength = startingBladeLength;
        bladeDamage = startingBladeDamage;

        trapCooldownTime = startingTrapCooldown;
        trapDamage = startingTrapDamage;

        shieldCooldownTime = startingShieldCooldown;
        shieldDuration = startingShieldDuration;

        trapUnlocked = false;
        shieldUnlocked = false;

        bladeCdSlider.maxValue = startingBladeCooldown;
        trapCdSlider.maxValue = startingTrapCooldown;
        shieldCdSlider.maxValue = startingShieldCooldown;
        
        trapCdSlider.value = startingTrapCooldown;
        shieldCdSlider.value = startingShieldCooldown;

    }

    // blade
    private void OnAction1() {
        if (bladeCd <= 0) {
            // Spawn blade and set its damage
            GameObject bladeInstance = Instantiate(blade, transform.position, Quaternion.identity, transform);
            bladeInstance.GetComponent<Blade>().SetDamage(bladeDamage);

            EventManager.BladeUse();

            // Start the cooldown
            bladeCd = bladeCooldownTime;
            Timing.RunCoroutine(_BladeCooldown());
        } 
    }

    // trap
    private void OnAction2() {
        if (trapUnlocked && trapCd <= 0) {
            // Queue the dash
            pMove.QueueDash();

            // Remove the last trap
            Destroy(GameObject.FindWithTag("Trap"));

            // Spawn trap and set its damage
            GameObject trapInstance = Instantiate(trap, transform.position, Quaternion.identity);
            trapInstance.GetComponent<Trap>().SetDamage(trapDamage);

            EventManager.TrapUse();

            // Start the cooldown
            trapCd = trapCooldownTime;
            Timing.RunCoroutine(_TrapCooldown());
        }
    }

    // shield
    private void OnAction3() {
        if (shieldUnlocked && shieldCd <= 0) {
            // Spawn shield and set its duration
            GameObject shieldInstance = Instantiate(shield, transform.position, Quaternion.identity, transform);
            shieldInstance.GetComponent<Shield>().SetDuration(shieldDuration);

            EventManager.ShieldUse(shieldDuration);

            // Start the cooldown
            shieldCd = shieldCooldownTime;
            Timing.RunCoroutine(_ShieldCooldown());
        }
    }

    private IEnumerator<float> _BladeCooldown() {
        while (bladeCd > 0) {
            yield return Timing.WaitForOneFrame;
            bladeCd -= Time.deltaTime;
            bladeCdSlider.value = Mathf.Max(0, bladeCd);
        }
    }

    private IEnumerator<float> _TrapCooldown() {
        while (trapCd > 0) {
            yield return Timing.WaitForOneFrame;
            trapCd -= Time.deltaTime;
            trapCdSlider.value = Mathf.Max(0, trapCd);
        }
    }

    private IEnumerator<float> _ShieldCooldown() {
        while (shieldCd > 0) {
            yield return Timing.WaitForOneFrame;
            shieldCd -= Time.deltaTime;
            shieldCdSlider.value = Mathf.Max(0, shieldCd);
        }
    }

    public void UnlockTrap() {
        trapUnlocked = true;
        trapCd = 0;
        trapCdSlider.value = 0;
        Transform trapImage = GameObject.FindWithTag("CooldownIcons").transform.GetChild(1).GetChild(0);
        trapImage.GetComponent<Image>().sprite = trapSprite;
        trapImage.localScale = new Vector3(0.75f, 0.75f, 1);
    }

    public void UnlockShield() {
        shieldUnlocked = true;
        shieldCd = 0;
        shieldCdSlider.value = 0;
        Transform shieldImage = GameObject.FindWithTag("CooldownIcons").transform.GetChild(2).GetChild(0);
        shieldImage.GetComponent<Image>().sprite = shieldSprite;
        shieldImage.localScale = new Vector3(0.75f, 0.75f, 1);
    }

    public void SetBladeCooldown(float cd) {
        bladeCooldownTime = cd;
        bladeCdSlider.maxValue = cd;
    }

    public void SetBladeLength(float length) {
        bladeLength = length;
        blade = longBlade;
    }

    public void SetBladeDamage(int damage) {
        bladeDamage = damage;
    }

    public void SetTrapCooldown(float cd) {
        trapCooldownTime = cd;
        trapCdSlider.maxValue = cd;
    }

    public void SetTrapDamage(int damage) {
        trapDamage = damage;
    }

    public void SetShieldCooldown(float cd) {
        shieldCooldownTime = cd;
        shieldCdSlider.maxValue = cd;
    }

    public void SetShieldDuration(float duration) {
        shieldDuration = duration;
    }

    void OnRestartGame() {
        EventManager.PlayAgain();
    }
}
