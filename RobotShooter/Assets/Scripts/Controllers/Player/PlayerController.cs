using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shokai.Items;
using FMOD.Studio;
using UnityEngine.Audio;
 
public class PlayerController : AController
{
    private enum Weapon
    {
        rifle = 1,
        shotgun,
        launcher
    }

    public InventoryController inventory;

    public PlayerModel playerModel;
    public GameObject gun;
    public Transform laserPoint;
    private Vector3 initPos;
    [HideInInspector] public bool godMode;

    public Animator anim;

    public Transform m_PitchControllerTransform;

    public Transform slopePos1;
    public Transform slopePos2;
    public Transform slopePos3;
    public Transform slopePos4;

    [HideInInspector] public float yaw;
    [HideInInspector] public float pitch;

    CharacterController characterController;

    [HideInInspector] public bool moving;
    [HideInInspector] public bool crouching;
    [HideInInspector] public bool dashing;
    [HideInInspector] public int cash;
    [HideInInspector] public int score;
    [HideInInspector] public bool canRecover;
    private Vector3 previousMovement;
    private float actualDashTime;
    private float recoveringFromDash;

    [HideInInspector] public bool atShop = false;
    [HideInInspector] public bool shoping;
    private bool uncontrolable = false;

    [HideInInspector] public float nextTimeToFireAR = 0f;
    [HideInInspector] public float nextTimeToFireShotgun = 0f;

    private float currentShieldDelay;
    private bool shieldDelay;

    public Vector3 hitNormal;

    public LayerMask electricZoneDetectionMask;
    public LayerMask geometryDetectionMask;
    public LayerMask slopeMask;
    public LayerMask placeDefenseMask;

    
    public LayerMask shootLayerMask;
    public GameObject bulletARPrefab;
    public GameObject grenadePrefab;
    public GameObject stickyGrenadePrefab;
    public GameObject bulletSpawner;
    public GameObject EMP;
    public GameObject electricParticles;
    public GameObject healingParticles;
    [HideInInspector] public float actualOverheat = 0;
    [HideInInspector] public bool saturatedAR = false;
    [HideInInspector] public float actualARShootCooldown = 0;
    [HideInInspector] public float actualShotgunShootCooldown = 0;
    [HideInInspector] public bool shotgunShotted = false;
    private Weapon actualWeapon;
    private Weapon pastWeapon;
    List<Quaternion> pellets;
    private int grenadesSlotNum;
    private int defenseSlotNum;
    [HideInInspector] public int grenadeAmmo;
    //private bool empActive;
    //private float currentEmpDuration;
    private float currentCooldownWaitToStart;
    private bool hasNormalGrenade;
    private EventInstance electricSound;
    private EventInstance jetpackGlideSound;
    private EventInstance warningHealthSound;
    private bool brokenShieldDone = false;
    private bool groundImpactDone = true;
    private bool warningHealthDone = false;
    private AudioSource source;
    private float runTimeSound = 0.5f;
    private bool canRunSound = true;

    [HideInInspector] public bool waitCooldown;
    [HideInInspector] public int currentARChargerAmmoCount;

    //public LayerMask m_ShootLayerMask;
    public ParticleSystem muzzleFlashAR;
    public ParticleSystem muzzleFlashShotgun;
    //public GameObject impactEffect;
    public GameObject impactShotgunHole;
    public LineRenderer lineRenderer;
    //public GameObject laserBeam;


    [Header("DEFENSES")]
    public Transform pointAttachDefense;
    public GameObject minePrefab;
    public GameObject terrainTurretPrefab;
    public GameObject airTurretPrefab;

    private float gravityMultiplier = 1;

    [HideInInspector] public int actualMineDefenses;
    [HideInInspector] public int actualTTurretDefenses;
    [HideInInspector] public int actualFTurretDefenses;

    [HideInInspector] public bool onGround;
    [HideInInspector] public float verticalSpeed;
    [HideInInspector] public float currentHealth;
    [HideInInspector] public float currentShield;
    [HideInInspector] public bool shooting;
    [HideInInspector] public bool withDefense;
    [HideInInspector] public string defense;
    [HideInInspector] public GameObject attachedDefense;
    [HideInInspector] public bool gliding;
    [HideInInspector] public bool readyToGlid;

    [HideInInspector] public PlayerState currentState;
    [HideInInspector] public PlayerState previousState;

    void Awake()
    {
        ChangeState(new PSMovement(this));
        initPos = transform.position;

        playerModel = Instantiate(playerModel);

        //ROTATION
        yaw = transform.rotation.eulerAngles.y;
        pitch = m_PitchControllerTransform.localRotation.eulerAngles.x;

        pellets = new List<Quaternion>(playerModel.pelletCount);
        for(int i = 0; i < playerModel.pelletCount; i++)
        {
            pellets.Add(Quaternion.Euler(Vector3.zero));
        }

        //MOVEMENT
        characterController = GetComponent<CharacterController>();

        characterController.center = playerModel.normalControllerCenter;
        characterController.height = playerModel.normalControllerHeigh;

        lineRenderer.gameObject.SetActive(false);

        //MOUSE
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //SOUND
        source = GetComponent<AudioSource>();
        AudioManager.instance.unitySources.Add(source);
    }

    // Use this for initialization
    public void StartGame()
    {
        GameEvents.instance.onTransitionStart += OnTransitionStart;

        transform.position = initPos;
        electricParticles.SetActive(false);
        healingParticles.SetActive(false);

        pitch = 0;

        cash = playerModel.INITIAL_CASH;
        score = cash;

        actualWeapon = Weapon.rifle;
        gc.uiController.ChangePointer((int)actualWeapon);

        gc.uiController.ChangeCash(cash);
        gc.uiController.ChangeAROverheat(actualOverheat);

        currentARChargerAmmoCount = playerModel.MAX_CHARGER_AMMO_AR;
        currentHealth = playerModel.MAX_HEALTH;
        currentShield = playerModel.MAX_SHIELD;

        currentCooldownWaitToStart = playerModel.cooldownWaitToStart;

        StartCoroutine(RecoverShield());

        anim.SetInteger("weaponToChange", (int)actualWeapon);

        ChangeState(new PSMovement(this));
    }

    // Update is called once per frame
    void Update()
    {       
        if (gc != null && gc.uiController != null && (gc.uiController.paused)) return;
        currentState.Update(this);
        AnyStateUpdate();
        //Debug.Log(currentState);
        //Debug.Log(actualWeapon);
        //Debug.Log("Gliding: " + gliding);
        //Debug.Log(readyToGlind);
        //Debug.Log(onGround);        
    }

    private void FixedUpdate()
    {
        currentState.FixedUpdate(this);
    }

    private void LateUpdate()
    {
        currentState.CheckTransition(this);
    }

    public void ChangeState(PlayerState ps)
    {
        //if (rb2D != null) rb2D.velocity = Vector2.zero;

        currentState = ps;
    }

    void AnyStateUpdate()
    {
        Debug.Log(actualWeapon);
        if (currentShieldDelay > 0 && shieldDelay) currentShieldDelay -= Time.deltaTime;
        else if (currentShieldDelay <= 0 && shieldDelay)
        {
            canRecover = true;
            shieldDelay = false;
        } 
        
        if (waitCooldown)
        {
            currentCooldownWaitToStart -= Time.deltaTime;
            if (currentCooldownWaitToStart <= 0)
            {
                waitCooldown = false;
                currentCooldownWaitToStart = playerModel.cooldownWaitToStart;
            }
        }

        CheckForElectricZone();
        CheckForGeometry();
    }

    void CheckForElectricZone()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, -Vector3.up, playerModel.electricZoneDetectionRange, electricZoneDetectionMask);
        for (int i = 0; i < hits.Length; i++)
        {
            LayerMask layer = hits[i].collider.gameObject.layer;
            if (layer == LayerMask.NameToLayer("ElectricZone"))
            {
                electricParticles.SetActive(true);
                if (!AudioManager.instance.isPlaying(electricSound))
                {
                    electricSound = AudioManager.instance.PlayEvent("BeingElectrocuted", transform.position);
                }                
                TakeDamage(playerModel.electrocuteDamage * Time.deltaTime, 0);
                break;
            }
        }
        if (hits.Length <= 0)
        {
            electricSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            electricParticles.SetActive(false);
        }
        
    }

    void CheckForGeometry()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, -Vector3.up, playerModel.electricZoneDetectionRange, geometryDetectionMask);
        for (int i = 0; i < hits.Length; i++)
        {
            LayerMask layer = hits[i].collider.gameObject.layer;
            if (layer == LayerMask.NameToLayer("Geometry"))
            {
                transform.SetParent(hits[i].collider.transform);
                break;
            }
        }
        if (hits.Length <= 0) transform.SetParent(null);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (-Vector3.up * playerModel.electricZoneDetectionRange));
    }

    public void TakeDamage(float damage, int whoAttacked)
    {
        if (godMode) return;

        if (currentShield > 0)
        {
            brokenShieldDone = false;
            warningHealthDone = false;            
            currentShield -= damage;
            gc.uiController.ChangeShield(currentShield);
        }
        else
        {
            if (!brokenShieldDone)
            {
                AudioManager.instance.PlayOneShotSound("BrokenEnergyShield", transform.position);
                brokenShieldDone = true;
            }
            currentHealth -= damage;
            gc.uiController.ChangeHealth(currentHealth);

            if (currentHealth <= 30 && !warningHealthDone)
            {
                source.clip = AudioManager.instance.clips[0];
                source.volume *= AudioManager.instance.fXVolume * AudioManager.instance.masterVolume;
                source.Play();
                warningHealthDone = true;
            }

            if (currentHealth <= 0 && !godMode)
            {
                if (!(currentState is PSDead)) ChangeState(new PSDead(this));
                return;
            }
        }
        canRecover = false;
        shieldDelay = true;
        currentShieldDelay = playerModel.secondsToWaitShield;
    }

    public void StartDeath()
    {
        StartCoroutine(Death());
    }

    IEnumerator Death()
    {
        m_PitchControllerTransform.gameObject.GetComponent<Animation>().Play("PlayerDeath");
        yield return new WaitForSeconds(1);
        GameManager.instance.uiController.StartFadeIn();
        yield return new WaitForSeconds(2);
        Time.timeScale = 0;
        GameManager.instance.uiController.GameOver(true);
    }

    private IEnumerator Inmunity(float time)
    {
        godMode = true;
        yield return new WaitForSeconds(time);
        godMode = false;
    }

    public void Aim()
    {
        float l_MouseAxisY = Input.GetAxis("Mouse Y");
        pitch += l_MouseAxisY * -playerModel.pitchRotationalSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, playerModel.minPitch, playerModel.maxPitch);
        //…
        float l_MouseAxisX = Input.GetAxis("Mouse X");
        yaw += l_MouseAxisX * playerModel.yawRotationalSpeed * Time.deltaTime;
        //...
        transform.rotation = Quaternion.Euler(0.0f, yaw, 0.0f);
        m_PitchControllerTransform.localRotation = Quaternion.Euler(pitch, 0.0f, 0.0f);
    }

    public void Move()
    {
        float l_YawInRadians = yaw * Mathf.Deg2Rad;
        float l_Yaw90InRadians = (yaw + 90.0f) * Mathf.Deg2Rad;
        Vector3 l_Forward = new Vector3(Mathf.Sin(l_YawInRadians), 0.0f, Mathf.Cos(l_YawInRadians));
        Vector3 l_Right = new Vector3(Mathf.Sin(l_Yaw90InRadians), 0.0f, Mathf.Cos(l_Yaw90InRadians));
        Vector3 l_Movement = Vector3.zero;        

        if (!dashing)
        {
            if (Input.GetKey(playerModel.upKeyCode))
                l_Movement = l_Forward;
            else if (Input.GetKey(playerModel.downKeyCode))
                l_Movement = -l_Forward;

            if (Input.GetKey(playerModel.rightKeyCode))
                l_Movement += l_Right;
            else if (Input.GetKey(playerModel.leftKeyCode))
                l_Movement -= l_Right;
        }
        else l_Movement = previousMovement;            

        //RUN && JUMP
        //…
        float l_SpeedMultiplier;

        if (dashing) l_SpeedMultiplier = playerModel.dashSpeedMultiplier;
        else if (crouching)
        {
            l_SpeedMultiplier = playerModel.crouchSpeedMultiplier;
            characterController.center = playerModel.crouchedControllerCenter;
            characterController.height = playerModel.crouchedControllerHeigh;
        }
        else if (recoveringFromDash > 0) 
        {
            l_SpeedMultiplier = playerModel.recoverDashMultiplier;
        }
        else
        {
            l_SpeedMultiplier = 1;
            characterController.center = playerModel.normalControllerCenter;
            characterController.height = playerModel.normalControllerHeigh;
        } 
        //…

        //…
        if (onGround)
        {
            if (Input.GetKeyDown(playerModel.jumpKeyCode))
            {
                verticalSpeed = playerModel.jumpSpeed;
                if (crouching) crouching = false;
            }
            if (Input.GetKeyDown(playerModel.dashKey) && !dashing && !crouching && recoveringFromDash <= 0)
            {
                AudioManager.instance.PlayOneShotSound("MechaDash", transform.position);
                dashing = true;
                actualDashTime = playerModel.dashTime;
            }
            if ((Input.GetKeyDown(playerModel.crouchKey) || Input.GetKeyDown(playerModel.alternativeCrouchKey)) && !dashing && !crouching && onGround) crouching = true;
            else if ((Input.GetKeyDown(playerModel.crouchKey) || Input.GetKeyDown(playerModel.alternativeCrouchKey)) && crouching) crouching = false;
        }



        //M0VEMENT
        l_Movement.Normalize();
        l_Movement = l_Movement * Time.deltaTime * playerModel.normalSpeed * l_SpeedMultiplier;
        previousMovement = l_Movement;

        if (l_Movement == Vector3.zero) moving = false;
        else moving = true;

        if (moving && onGround && runTimeSound <= 0)
        {
            AudioManager.instance.PlayOneShotSound("ImpactWithGround", transform.position);
            //source.clip = AudioManager.instance.clips[5];
            //source.volume = 0.5f;
            //source.Play();
            ///source.volume = 1;
            runTimeSound = 0.5f;
        }
        runTimeSound -= Time.deltaTime;

        //GRAVITY
        //…
        if (Input.GetKeyDown(playerModel.jumpKeyCode) != onGround && !readyToGlid) readyToGlid = true;
        if (verticalSpeed <= 0 && Input.GetKey(playerModel.jumpKeyCode) && !onGround && readyToGlid)
        {
            readyToGlid = false;
            gliding = true;
            if (!AudioManager.instance.isPlaying(jetpackGlideSound))
            {
                jetpackGlideSound = AudioManager.instance.PlayEvent("JetpackGlide", transform.position);
            }
        }        
        if (!gliding) verticalSpeed += Physics.gravity.y * 1.5f * Time.deltaTime;

        //Debug.Log(verticalSpeed);

        if (verticalSpeed <= 0 && Input.GetKey(playerModel.jumpKeyCode) != onGround && gliding) l_Movement.y = playerModel.verticalGlideSpeed * Time.deltaTime;
        else l_Movement.y = verticalSpeed * Time.deltaTime;        

        hitNormal = Normal();

        if (Slope())
        {
            l_Movement.y = -characterController.height * playerModel.slopeForce * Time.deltaTime;
        }

        if (Slide())
        {
            l_Movement.x += hitNormal.x * 0.5f;
            l_Movement.z += hitNormal.z * 0.5f;
            l_Movement.y -= hitNormal.y * 0.5f;
        }

        if (onGround && l_Movement.y <= 0) l_Movement.y = -playerModel.stepOffset;

        //JUMP
        CollisionFlags l_CollisionFlags = characterController.Move(l_Movement);

        if ((l_CollisionFlags & CollisionFlags.Below) != 0)
        {
            gliding = false;
            onGround = true;
            verticalSpeed = 0;
            jetpackGlideSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            if (!groundImpactDone)
            {
                //AudioManager.instance.PlayOneShotSound("ImpactWithGround", transform.position);
                source.clip = AudioManager.instance.clips[5];
                source.volume *= AudioManager.instance.fXVolume * AudioManager.instance.masterVolume;
                source.Play();
                groundImpactDone = true;
            }
        }
        else
        {
            onGround = false;
            groundImpactDone = false;
        }
            

        if ((l_CollisionFlags & CollisionFlags.Above) != 0 && verticalSpeed > 0.0f)
            verticalSpeed = 0.0f;

        if (dashing) actualDashTime -= Time.deltaTime;

        if (recoveringFromDash > 0) recoveringFromDash -= Time.deltaTime;

        if (actualDashTime <= 0 && dashing == true) 
        {
            recoveringFromDash = playerModel.recoveringFromDashTime;
            dashing = false;        
        }
        if (gliding && Input.GetKeyUp(playerModel.jumpKeyCode))
        {
            gliding = false;
            verticalSpeed = playerModel.resetFallVelocity;
            jetpackGlideSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        } 
    }

    public void UseDirectHability(int num)
    {      
        if (inventory.ItemContainer.GetSlotQuantityByIndex(num) > 0) HabilityEffect(num);
    }

    public void UseDirectDeffense(int num)
    {
        SlotInfo sInfo = gc.shopController.defenseSlots[num].gameObject.GetComponent<SlotInfo>();
        if (sInfo.charges > 0) DefenseEffect(sInfo, num);
    }

    public void CheckHabilities()
    {
        if (Input.GetKeyDown(playerModel.hability1Key)) UseDirectHability(0);
        else if (Input.GetKeyDown(playerModel.hability2Key)) UseDirectHability(1);
        else if (Input.GetKeyDown(playerModel.hability3Key)) UseDirectHability(2);
        else if (Input.GetKeyDown(playerModel.hability4Key)) UseDirectHability(3);
        else if (Input.GetKeyDown(playerModel.defenseKey)) UseDirectDeffense(0);
    }

    public void HabilityEffect(int num)
    {
        switch (inventory.ItemContainer.GetItemNameByIndex(num))
        {
            case "Jetpack":
                AudioManager.instance.PlayOneShotSound("JetpackBoost", transform.position);
                verticalSpeed = playerModel.jetpackVerticalSpeed;
                if (gliding)
                {
                    gliding = false;
                    readyToGlid = true;
                } 
                inventory.ItemContainer.Consume(num);
                break;
            case "Grenade": 
                if (!lineRenderer.gameObject.activeSelf)
                {
                    grenadesSlotNum = num;
                    grenadeAmmo = inventory.ItemContainer.GetSlotQuantityByIndex(num);
                    if (actualWeapon != Weapon.launcher)
                    {
                        pastWeapon = actualWeapon;
                        actualWeapon = Weapon.launcher;
                        gc.uiController.ChangePointer((int)actualWeapon);
                        anim.SetInteger("weaponToChange", (int)actualWeapon);
                        anim.SetInteger("previousWeapon", (int)pastWeapon);
                        anim.SetTrigger("changeWeapon");
                    }
                    if (withDefense)
                    {
                        DestroyDefense();
                        gun.SetActive(true);
                        anim.SetInteger("weaponToChange", (int)actualWeapon);
                    }
                    hasNormalGrenade = true;
                    ChangeState(new PSGrenadeLauncher(this));
                }                
                break;
            case "Laser":
                if (!withDefense && !lineRenderer.gameObject.activeSelf)
                {
                    previousState = currentState;
                    inventory.ItemContainer.Consume(num);
                    ChangeState(new PSLaser(this));
                }                
                break;
            case "Health":
                if (!withDefense && !lineRenderer.gameObject.activeSelf && currentHealth < playerModel.MAX_HEALTH)
                {
                    AudioManager.instance.PlayOneShotSound("PowerUp", transform.position);
                    currentHealth += playerModel.instantHealthAmount;
                    if (currentHealth > playerModel.MAX_HEALTH) currentHealth = playerModel.MAX_HEALTH;
                    gc.uiController.ChangeHealth(currentHealth);
                    healingParticles.SetActive(true);
                    Invoke("StopHealingParticles", playerModel.healingParticlesTime);
                    inventory.ItemContainer.Consume(num);
                }
                break;
            case "StickyGrenade":
                if (!lineRenderer.gameObject.activeSelf)
                {
                    grenadesSlotNum = num;
                    grenadeAmmo = inventory.ItemContainer.GetSlotQuantityByIndex(num);
                    if (actualWeapon != Weapon.launcher)
                    {
                        pastWeapon = actualWeapon;
                        actualWeapon = Weapon.launcher;
                        gc.uiController.ChangePointer((int)actualWeapon);
                        anim.SetInteger("weaponToChange", (int)actualWeapon);
                        anim.SetInteger("previousWeapon", (int)pastWeapon);
                        anim.SetTrigger("changeWeapon");
                    } 
                    if (withDefense)
                    {
                        DestroyDefense();
                        gun.SetActive(true);
                        anim.SetInteger("weaponToChange", (int)actualWeapon);
                    }
                    hasNormalGrenade = false;
                    ChangeState(new PSGrenadeLauncher(this));
                }
                break;
            case "EMP":
                AudioManager.instance.PlayOneShotSound("EMP", transform.position);
                Instantiate(EMP, transform.position, Quaternion.identity);
                inventory.ItemContainer.Consume(num);
                break;
        }
    }

    void StopHealingParticles()
    {
        healingParticles.SetActive(false);
    }

    public void DefenseEffect(SlotInfo sInfo, int num)
    {
        defense = sInfo.content;
        switch (sInfo.content)
        {
            case "Mine":
                if (!lineRenderer.gameObject.activeSelf && !withDefense)
                {
                    previousState = currentState;
                    ChangeState(new PSDefense(this));
                    defenseSlotNum = num;
                }                
                break;
            case "TerrainTurret":
                if (!lineRenderer.gameObject.activeSelf && !withDefense)
                {
                    previousState = currentState;
                    ChangeState(new PSDefense(this));
                    defenseSlotNum = num;
                }
                break;
            case "AirTurret":
                if (!lineRenderer.gameObject.activeSelf && !withDefense)
                {
                    previousState = currentState;
                    ChangeState(new PSDefense(this));
                    defenseSlotNum = num;
                }
                break;
        }
    }

    public void PrepareDeffense()
    {
        switch (defense)
        {
            case "Mine":
                attachedDefense = Instantiate(minePrefab, pointAttachDefense.position, pointAttachDefense.rotation);
                attachedDefense.GetComponent<SphereCollider>().enabled = false;
                break;
            case "TerrainTurret":
                attachedDefense = Instantiate(terrainTurretPrefab, pointAttachDefense.position, pointAttachDefense.rotation);
                attachedDefense.GetComponent<TerrainTurretController>().placed = false;
                attachedDefense.GetComponent<TerrainTurretController>().impactZone.enabled = false;
                foreach (Collider collider in attachedDefense.GetComponentsInChildren<Collider>()) collider.enabled = false;
                break;
            case "AirTurret":
                attachedDefense = Instantiate(airTurretPrefab, pointAttachDefense.position, pointAttachDefense.rotation);
                attachedDefense.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                attachedDefense.GetComponent<AirTurretController>().placed = false;
                attachedDefense.GetComponent<AirTurretController>().impactZone.enabled = false;
                foreach (Collider collider in attachedDefense.GetComponentsInChildren<Collider>()) collider.enabled = false;
                break;
        }
    }

    public void UpdateAttachedDefense()
    {
        Vector3 eulerAngles = pointAttachDefense.rotation.eulerAngles;
        attachedDefense.transform.position = pointAttachDefense.position;
        attachedDefense.gameObject.transform.localEulerAngles = new Vector3(0.0f, eulerAngles.y, eulerAngles.z);
        if (attachedDefense == null)
        {
            ChangeState(previousState);
            gun.SetActive(true);
            anim.SetInteger("weaponToChange", (int)actualWeapon);
        }        
    }

    public void PlaceDeffense()
    {
        RaycastHit hit;
        if (Physics.Raycast(pointAttachDefense.position, -Vector3.up, out hit, playerModel.distancePlaceDefense, placeDefenseMask))
        {
            switch (defense)
            {
                case "Mine":
                    GameObject mine = Instantiate(minePrefab, hit.point, attachedDefense.transform.rotation);
                    mine.GetComponent<SphereCollider>().enabled = true;
                    AudioManager.instance.PlayOneShotSound("Mine", transform.position);
                    break;
                case "TerrainTurret":
                    GameObject terrainTurret = Instantiate(terrainTurretPrefab, hit.point, attachedDefense.transform.rotation);
                    terrainTurret.GetComponent<TerrainTurretController>().placed = true;
                    AudioManager.instance.PlayOneShotSound("PlacedDefense", transform.position);
                    break;
                case "AirTurret":
                    GameObject airTurret = Instantiate(airTurretPrefab, hit.point, attachedDefense.transform.rotation);
                    airTurret.GetComponent<AirTurretController>().placed = true;
                    AudioManager.instance.PlayOneShotSound("PlacedDefense", transform.position);
                    break;
            }
            
            DestroyDefense();
            gun.SetActive(true);
            anim.SetInteger("weaponToChange", (int)actualWeapon);
            SlotInfo sInfo = gc.shopController.defenseSlots[defenseSlotNum].GetComponent<SlotInfo>();
            sInfo.Consume();
            ChangeState(previousState);
        }
        
    }

    public void DestroyDefense()
    {
        withDefense = false;
        Destroy(attachedDefense);
    }

    public void OnTransitionStart()
    {
        currentHealth = playerModel.MAX_HEALTH;
        currentShield = playerModel.MAX_SHIELD;        
        actualMineDefenses = 0;
        actualTTurretDefenses = 0;
        actualFTurretDefenses = 0;
        gc.uiController.ChangeHealth(currentHealth);
        gc.uiController.ChangeShield(currentShield);
        gc.shopController.ResetHabilitiesAndDefenses();
        inventory.ItemContainer.ResetAllSlots();
        if (grenadeAmmo > 0) grenadeAmmo = 0;
        gc.DestroyDefenses();
        IncreaseCash(gc.roundController.roundPassedIncome);
    }

    public void IncreaseCash(int cash)
    {
        int maxScore = 9999999;

        this.cash += cash;
        if (this.cash > maxScore) this.cash = maxScore;
        score += cash;
        if (score > maxScore) score = maxScore;

        gc.uiController.ChangeCash(this.cash);
    }

    IEnumerator RecoverShield()
    {        
        yield return new WaitForSeconds(playerModel.shieldRegenTickRate);
        if (canRecover)
        {
            if (currentShield + playerModel.shieldRegenPerTick < playerModel.MAX_SHIELD)
            {
                currentShield += playerModel.shieldRegenPerTick;
                gc.uiController.ChangeShield(currentShield);
            }
            else currentShield = playerModel.MAX_SHIELD;            
        }
        StartCoroutine(RecoverShield());
    }

    IEnumerator WaitForRecoverShield()
    {
        yield return new WaitForSeconds(playerModel.secondsToWaitShield);
        canRecover = true;
    }

    public bool Slope()
    {
        if (onGround == false || Input.GetKeyDown(playerModel.jumpKeyCode))
        {
            return false;
        }
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, characterController.height, slopeMask.value))
        {
            if (hit.normal != Vector3.up) return true;
        }
        if (Physics.Raycast(slopePos1.position, Vector3.down, out hit, characterController.height, slopeMask.value))
        {
            if (hit.normal != Vector3.up) return true;
        }
        if (Physics.Raycast(slopePos2.position, Vector3.down, out hit, characterController.height, slopeMask.value))
        {
            if (hit.normal != Vector3.up) return true;
        }
        if (Physics.Raycast(slopePos3.position, Vector3.down, out hit, characterController.height, slopeMask.value))
        {
            if (hit.normal != Vector3.up) return true;
        }
        if (Physics.Raycast(slopePos4.position, Vector3.down, out hit, characterController.height, slopeMask.value))
        {
            if (hit.normal != Vector3.up) return true;
        }
        return false;
    }
    public bool Slide()
    {
        RaycastHit hit;
        if ((Vector3.Angle(Vector3.up, hitNormal) >= characterController.slopeLimit)) return true;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, playerModel.slopeRayDist, slopeMask))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Column") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy")) return true;
        }
        if (Physics.Raycast(slopePos1.position, Vector3.down, out hit, playerModel.slopeRayDist, slopeMask))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Column") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy")) return true;
        }
        if (Physics.Raycast(slopePos2.position, Vector3.down, out hit, playerModel.slopeRayDist, slopeMask))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Column") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy")) return true;
        }
        if (Physics.Raycast(slopePos3.position, Vector3.down, out hit, playerModel.slopeRayDist, slopeMask))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Column") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy")) return true;
        }
        if (Physics.Raycast(slopePos4.position, Vector3.down, out hit, playerModel.slopeRayDist, slopeMask))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Column") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy")) return true;
        }
        return false;
    }
    public Vector3 Normal()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, playerModel.slopeRayDist, slopeMask))
        {
            return hit.normal;
        }
        if (Physics.Raycast(slopePos1.position, Vector3.down, out hit, playerModel.slopeRayDist, slopeMask))
        {
            return hit.normal;
        }
        if (Physics.Raycast(slopePos2.position, Vector3.down, out hit, playerModel.slopeRayDist, slopeMask))
        {
            return hit.normal;
        }
        if (Physics.Raycast(slopePos3.position, Vector3.down, out hit, playerModel.slopeRayDist, slopeMask))
        {
            return hit.normal;
        }
        if (Physics.Raycast(slopePos4.position, Vector3.down, out hit, playerModel.slopeRayDist, slopeMask))
        {
            return hit.normal;
        }
        return hit.normal;
    }

    public void Shoot()
    {
        float multiplier = 1;        
        switch (actualWeapon)
        {
            case Weapon.rifle:
                anim.SetBool("shooting", true);
                muzzleFlashAR.Play();
                AudioManager.instance.PlayOneShotSound("AssaultRifleShot", transform.position);
                anim.speed = playerModel.fireRateAR;
                nextTimeToFireAR = Time.time + 1 / playerModel.fireRateAR;
                //StartCoroutine(EndMuzzle());
                actualARShootCooldown = playerModel.shootARCooldown;
                RaycastHit hitAR;
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitAR, Mathf.Infinity, shootLayerMask))
                {
                    Vector3 dir = hitAR.point - bulletSpawner.transform.position;
                    dir.Normalize();
                    GameObject bullet = gc.objectPoolerManager.ARBulletOP.GetPooledObject();
                    bullet.transform.position = bulletSpawner.transform.position;
                    bullet.transform.rotation = bulletSpawner.transform.rotation;
                    bullet.transform.forward = dir;
                    bullet.GetComponent<ARBulletController>().damage = playerModel.damageAR;
                    bullet.SetActive(true);                         
                }
                else
                {
                    GameObject bullet = gc.objectPoolerManager.ARBulletOP.GetPooledObject();
                    bullet.transform.position = bulletSpawner.transform.position;
                    bullet.transform.rotation = Camera.main.transform.rotation;
                    bullet.GetComponent<ARBulletController>().damage = playerModel.damageAR;
                    bullet.SetActive(true);
                    //bulletAR.GetComponent<Rigidbody>().AddForce(bulletSpawner.transform.forward * playerModel.shootForceAR * bulletAR.GetComponent<Rigidbody>().mass, ForceMode.Impulse);
                }               

                if (!godMode) actualOverheat += playerModel.bulletOverheat;

                gc.uiController.ChangeAROverheat(actualOverheat);
                if (actualOverheat >= playerModel.maxOverheatAR)
                {
                    AudioManager.instance.PlayOneShotSound("OverheatAR", transform.position);
                    actualOverheat = playerModel.maxOverheatAR;
                    saturatedAR = true;
                }
                break;
            case Weapon.shotgun:
                anim.SetBool("shooting", true);
                float damage;
                nextTimeToFireShotgun = Time.time + 1 / playerModel.fireRateShotgun;
                AudioManager.instance.PlayOneShotSound("ShotgunShot", transform.position);
                anim.speed = playerModel.fireRateShotgun;
                //StartCoroutine(EndMuzzle());
                for (int i = 0; i < pellets.Count; i++)
                {
                    multiplier = 1;
                    pellets[i] = Random.rotation;
                    Quaternion rot = Quaternion.RotateTowards(Camera.main.transform.rotation, pellets[i], playerModel.shotgunSpreadAngle);
                    RaycastHit hit; 
                    if (Physics.Raycast(Camera.main.transform.position, rot * Vector3.forward, out hit, playerModel.rangeShotgun, shootLayerMask))
                    {

                        if (hit.collider.tag == "CriticalBox") multiplier = playerModel.criticalMultiplier;
                        damage = playerModel.shotgunDamage * multiplier;
                        //GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));

                        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Geometry"))
                        {
                            GameObject impactHole = Instantiate(impactShotgunHole, new Vector3(hit.point.x, hit.point.y, hit.point.z), Quaternion.LookRotation(hit.normal));

                            impactHole.transform.parent = hit.transform;
                        }                        

                        GroundEnemy gEnemy = hit.collider.GetComponentInParent<GroundEnemy>();
                        if (gEnemy != null)
                        {
                            gEnemy.TakeDamage(damage);
                            if (multiplier == playerModel.criticalMultiplier)
                            {
                                IncreaseCash(gEnemy.criticalIncome);
                                //gEnemy.WhatHitSound("CriticalHit");
                                gEnemy.source.clip = AudioManager.instance.clips[4];
                                gEnemy.source.volume *= AudioManager.instance.fXVolume * AudioManager.instance.masterVolume;
                                gEnemy.source.Play();
                            }
                            else
                            {
                                gEnemy.source.clip = AudioManager.instance.clips[3];
                                gEnemy.source.volume *= AudioManager.instance.fXVolume * AudioManager.instance.masterVolume;
                                gEnemy.source.Play();
                            }
                        } 
                        else
                        {
                            FlyingEnemy fEnemy = hit.collider.GetComponentInParent<FlyingEnemy>();
                            if (fEnemy != null)
                            {
                                fEnemy.TakeDamage(damage);
                                if (multiplier == playerModel.criticalMultiplier)
                                {
                                    IncreaseCash(fEnemy.criticalIncome);
                                    //fEnemy.WhatHitSound("CriticalHit");
                                    fEnemy.source.clip = AudioManager.instance.clips[4];
                                    fEnemy.source.volume *= AudioManager.instance.fXVolume * AudioManager.instance.masterVolume;
                                    fEnemy.source.Play();
                                }
                                else
                                {
                                    fEnemy.source.clip = AudioManager.instance.clips[3];
                                    fEnemy.source.volume *= AudioManager.instance.fXVolume * AudioManager.instance.masterVolume;
                                    fEnemy.source.Play();
                                }
                            } 
                            else
                            {
                                TankEnemy tEnemy = hit.collider.GetComponentInParent<TankEnemy>();
                                if (tEnemy != null)
                                {
                                    tEnemy.TakeDamage(damage);
                                    if (multiplier == playerModel.criticalMultiplier)
                                    {
                                        IncreaseCash(tEnemy.criticalIncome);
                                        //tEnemy.WhatHitSound("CriticalHit");
                                        tEnemy.source.clip = AudioManager.instance.clips[4];
                                        tEnemy.source.volume *= AudioManager.instance.fXVolume * AudioManager.instance.masterVolume;
                                        tEnemy.source.Play();
                                    }
                                    else
                                    {
                                        tEnemy.source.clip = AudioManager.instance.clips[2];
                                        tEnemy.source.volume *= AudioManager.instance.fXVolume * AudioManager.instance.masterVolume;
                                        tEnemy.source.Play();
                                    }
                                } 
                            }
                        }

                        /*limitHoles.Add(impactHoleGO);

                        if (limitHoles.Count == 25)
                        {
                            Destroy(limitHoles[0]);
                            limitHoles.Remove(limitHoles[0]);
                        }

                        GameController.instance.destroyObjects.Add(impactHoleGO);*/

                        //Destroy(impact, 2f);
                    }
                    actualShotgunShootCooldown = playerModel.shootShotgunCooldown;
                    shotgunShotted = true;
                }                
                break;
            case Weapon.launcher:
                if (grenadeAmmo > 0) anim.SetTrigger("shootGL");
                AudioManager.instance.PlayOneShotSound("GrenadeLauncherShot", transform.position);
                if (hasNormalGrenade)
                {
                    GameObject grenade = Instantiate(grenadePrefab, bulletSpawner.transform.position, bulletSpawner.transform.rotation);
                    grenade.GetComponent<Rigidbody>().AddForce(bulletSpawner.transform.forward * playerModel.shootForceLauncher * grenade.GetComponent<Rigidbody>().mass, ForceMode.VelocityChange);
                }
                else
                {
                    GameObject stickyGrenade = Instantiate(stickyGrenadePrefab, bulletSpawner.transform.position, bulletSpawner.transform.rotation);
                    stickyGrenade.GetComponent<Rigidbody>().AddForce(bulletSpawner.transform.forward * playerModel.shootForceLauncher * stickyGrenade.GetComponent<Rigidbody>().mass, ForceMode.VelocityChange);
                }
                ChangeGrenadeAmmo();                
                break;
        }       
    }

    public void ChangeGrenadeAmmo()
    {
        inventory.ItemContainer.Consume(grenadesSlotNum);
        grenadeAmmo--;       
    }

    public void CheckWeaponToShoot()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("ChangeRight") || anim.GetCurrentAnimatorStateInfo(0).IsName("ChangeLeft"))
        {
            if (actualWeapon == Weapon.rifle) shooting = true;
            return;
        } 
        switch (actualWeapon)
        {            
            case Weapon.rifle:
                 if(!saturatedAR) ChangeState(new PSShoot(this));
                break;
            case Weapon.shotgun:
                if (Time.time >= nextTimeToFireShotgun) ChangeState(new PSShotgun(this));
                break;
            case Weapon.launcher:
                break;
        }
    }

    public void CoolOverheat()
    {
        if (!waitCooldown || saturatedAR)
        {
            if (actualOverheat > 0)
            {
                if (!saturatedAR)
                {
                    actualOverheat -= playerModel.overHeatNormalReload * Time.deltaTime;
                    if (actualOverheat < 0) actualOverheat = 0;
                }
                else
                {                    
                    actualOverheat -= playerModel.overHeatSlowReload * Time.deltaTime;
                    if (actualOverheat < 0) actualOverheat = 0;
                    if (actualOverheat <= 0) saturatedAR = false;
                }
            }

            if (gc != null && gc.uiController != null && Time.time > 0.1f) gc.uiController.ChangeAROverheat(actualOverheat);
        }        
    }

    public bool CanShootShotgun()
    {
        if (Time.time >= nextTimeToFireShotgun) return true;
        else return true;
    }

    private void ApplyDamage()
    {

    }

    public void Shop(bool show)
    {        
        gc.uiController.shopInterface.SetActive(!gc.uiController.shopInterface.activeSelf);
        Cursor.visible = show;
        if (show)
        {
            previousState = currentState;
            ChangeState(new PSUncontrolable(this));
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            ChangeState(previousState);
            Cursor.lockState = CursorLockMode.Locked;
        } 
    }

    public void ChangeWeapon()
    {
        switch (actualWeapon)
        {
            case Weapon.rifle:
                pastWeapon = actualWeapon;
                actualWeapon = Weapon.shotgun;
                //animation
                break;
            case Weapon.shotgun:
                pastWeapon = actualWeapon;
                actualWeapon = Weapon.rifle;
                //animation
                break;
            case Weapon.launcher:
                actualWeapon = pastWeapon;
                pastWeapon = Weapon.launcher;
                ChangeState(new PSMovement(this));
                //animation
                break;
        }
        gc.uiController.ChangePointer((int)actualWeapon);
        anim.SetInteger("weaponToChange", (int)actualWeapon);
        anim.SetInteger("previousWeapon", (int)pastWeapon);
        anim.SetTrigger("changeWeapon");
    }

    public void Die()
    {
        gc.uiController.GameOver(true);
    }

    //COLLISIONS

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "Shop")
        {
            gc.uiController.ShowInteractiveText("Press [" + playerModel.interactKey + "] to enter the shop");
            atShop = true;
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("EnemyAttack"))
        {
            if (collision.gameObject.GetComponentInParent<GroundEnemy>() != null)
            {
                TakeDamage(collision.gameObject.GetComponentInParent<GroundEnemy>().damage, 0);
                AudioManager.instance.PlayOneShotSound("ReceiveDamage", transform.position);
            }
        }
            
        if (collision.tag == "CenterPlatform")
        {
            gc.roundController.OnTransitionTriggerEnter();
        }
    }

    private void OnTriggerStay(Collider collision)
    {
        if (collision.tag == "DeathZone")
        {
            TakeDamage (currentHealth, 0);
        }        
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.tag == "Shop")
        {
            gc.uiController.HideInteractiveText();
            atShop = false;
        } 
    }

    void Debuging()
    {
#if UNITY_EDITOR
        //if (Input.GetKeyDown(KeyCode.Y)) gc.shopController.MoveShop(true);
        //if (Input.GetKeyDown(KeyCode.U)) gc.shopController.MoveShop(false);
#endif
    }
}
