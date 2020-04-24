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
    public float maxPitch = 50.0f;    
    public bool invertedYaw = false;
    public bool invertedPitch = true;    

    [Header("MOVMENT")]
    public float speed = 10.0f;
    public KeyCode leftKeyCode = KeyCode.A;
    public KeyCode rightKeyCode = KeyCode.D;
    public KeyCode upKeyCode = KeyCode.W;
    public KeyCode downKeyCode = KeyCode.S;
    public KeyCode interactKey = KeyCode.E;

    [Header("RUN & JUMP")]
    public KeyCode runKeyCode = KeyCode.LeftShift;
    public KeyCode jumpKeyCode = KeyCode.Space;
    public float fastSpeedMultiplier = 1.2f;
    public float jumpSpeed = 5;

    [Header("SHOOT")]
    public KeyCode reloadKeyCode = KeyCode.R;
    public float shootARCooldown = 0.3f;
    public int mouseShootButton = 0;
    public float rangeAR = 100f;
    public float fireRateAR = 10f;    
    public float damageAR = 50;
    public float shootForce = 100;

    [Header("TESTING")]
    public KeyCode godMode = KeyCode.Alpha9;
}
