using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerModel", menuName = "Models/Player", order = 0)]
public class PlayerModel : ScriptableObject
{
    [Header("INIT_VALUES")]
    public float MAX_HEALTH;
    public float MAX_SHIELD;
    public int MAX_CHARGER_AMMO_AR;
    public int INITIAL_CASH;

    [Header("ROTATION")]
    public float yawRotationalSpeed;
    public float pitchRotationalSpeed;
    public float minPitch;
    public float maxPitch;    
    public bool invertedYaw;
    public bool invertedPitch;    

    [Header("MOVEMENT")]
    public float normalSpeed;
    public float dashSpeedMultiplier;
    public float recoverDashMultiplier;
    public float crouchSpeedMultiplier;
    public KeyCode leftKeyCode;
    public KeyCode rightKeyCode;
    public KeyCode upKeyCode;
    public KeyCode downKeyCode;
    public KeyCode interactKey;
    public KeyCode dashKey;
    public KeyCode crouchKey;
    public KeyCode alternativeCrouchKey;
    public Vector3 normalControllerCenter;
    public Vector3 crouchedControllerCenter;
    public float normalControllerHeigh;
    public float crouchedControllerHeigh;
    public float dashTime;
    public float recoveringFromDashTime;
    public float shieldRegenTickRate;
    public float shieldRegenPerTick;
    public float secondsToWaitShield;

    [Header("RUN & JUMP")]
    public KeyCode runKeyCode;
    public KeyCode jumpKeyCode;
    public float fastSpeedMultiplier;
    public float jumpSpeed;
    public float verticalGlideSpeed;

    [Header("SHOOT")]
    public KeyCode changeWeaponKey;
    public int alternativeChangeWeapon;
    public int mouseShootButton;

    [Header("ASSAULT RIFLE")]
    public float shootARCooldown;
    public float rangeAR; 
    public float shootForceAR;
    public float maxOverheatAR;
    public float bulletOverheat;
    public float overHeatNormalReload;
    public float overHeatSlowReload;

    [Header("SHOTGUN")]
    public float shootShotgunCooldown;
    public float rangeShotgun;
    public float shotgunDamage;
    public int pelletCount;
    public float shotgunSpreadAngle;

    [Header("LAUNCHER")]
    public float launcherCooldown;
    public float shootForceLauncher;

    [Header("HABILITIES")]
    public KeyCode hability1Key;
    public KeyCode hability2Key;
    public KeyCode hability3Key;
    public KeyCode hability4Key;
    public float jetpackVerticalSpeed;
    public float resetFallVelocity;
    public float laserDistance;
    public float laserTime;
    public float laserDamage;
    public float instantHealthCure;
    public float empRadius;
    public float empDuration;

    [Header("DEFENSES")]
    public KeyCode defenseKey;
    public float distancePlaceDefense;

    [Header("OTHER")]
    public KeyCode pauseKey;
    public float lavaDamage;

    [Header("TESTING")]
    public KeyCode godMode;
}
