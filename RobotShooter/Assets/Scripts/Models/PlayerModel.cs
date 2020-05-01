using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerModel", menuName = "Models/Player", order = 0)]
public class PlayerModel : ScriptableObject
{
    [Header("INIT_VALUES")]
    public float MAX_HEALTH = 100;
    public float MAX_SHIELD = 100;
    public int MAX_CHARGER_AMMO_AR = 30;

    [Header("ROTATION")]
    public float yawRotationalSpeed = 360.0f;
    public float pitchRotationalSpeed = 180.0f;
    public float minPitch = -80.0f;
    public float maxPitch = 70.0f;    
    public bool invertedYaw = false;
    public bool invertedPitch = true;    

    [Header("MOVEMENT")]
    public float normalSpeed = 10.0f;
    public float dashSpeedMultiplier = 3.0f;
    public float crouchSpeedMultiplier = 0.5f;
    public KeyCode leftKeyCode = KeyCode.A;
    public KeyCode rightKeyCode = KeyCode.D;
    public KeyCode upKeyCode = KeyCode.W;
    public KeyCode downKeyCode = KeyCode.S;
    public KeyCode interactKey = KeyCode.E;
    public KeyCode dashKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode alternativeCrouchKey = KeyCode.C;
    public Vector3 normalControllerCenter = new Vector3(0, 0, 0);
    public Vector3 crouchedControllerCenter = new Vector3(0, 0.3f, 0);
    public float normalControllerHeigh = 2;
    public float crouchedControllerHeigh = 1.2f;
    public float dashTime = 0.5f;

    [Header("RUN & JUMP")]
    public KeyCode runKeyCode = KeyCode.LeftShift;
    public KeyCode jumpKeyCode = KeyCode.Space;
    public float fastSpeedMultiplier = 1.2f;
    public float jumpSpeed = 5;
    public float verticalGlideSpeed = -1;

    [Header("SHOOT")]
    public KeyCode changeWeaponKey = KeyCode.F;
    public int mouseShootButton = 0;

    [Header("ASSAULT RIFLE")]
    public float shootARCooldown = 0.1f;
    public float rangeAR = 100f;
    public float fireRateAR = 10f;    
    public float shootForceAR = 100;
    public float maxOverheatAR = 100;
    public float bulletOverheat = 2;
    public float overHeatNormalReload = 30;
    public float overHeatSlowReload = 15;

    [Header("SHOTGUN")]
    public float shootShotgunCooldown = 1f;
    public float rangeShotgun = 20f;
    public float shotgunDamage = 350f;
    public int pelletCount = 12;
    public float shotgunSpreadAngle = 10;

    [Header("LAUNCHER")]
    public float launcherCooldown = 0.7f;
    public float shootForceLauncher = 25f;

    [Header("HABILITIES")]
    public KeyCode hability1Key = KeyCode.Alpha1;
    public KeyCode hability2Key = KeyCode.Alpha2;
    public KeyCode hability3Key = KeyCode.Alpha3;
    public KeyCode hability4Key = KeyCode.Alpha4;
    public float jetpackVerticalSpeed = 10f;
    public float laserDistance = 250.0f;
    public float laserTime = 3;

    [Header("DEFENSES")]
    public KeyCode defenseKey = KeyCode.Q;
    public float distancePlaceDefense = 3f;

    [Header("TESTING")]
    public KeyCode godMode = KeyCode.Alpha9;
}
