using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : AController
{
    private enum Weapon
    {
        rifle,
        shotgun,
        launcher
    }

    public PlayerModel playerModel;
    public GameObject gun;
    private Vector3 initPos;
    [HideInInspector] public bool godMode;

    public Transform m_PitchControllerTransform;

    [HideInInspector] public float yaw;
    [HideInInspector] public float pitch;

    CharacterController characterController;

    [HideInInspector] public bool moving;
    [HideInInspector] public bool crouching;
    [HideInInspector] public bool dashing;
    [HideInInspector] public int cash;
    [HideInInspector] public bool canRecover;
    private Vector3 previousMovement;
    private float actualDashTime;
    private float recoveringFromDash;

    [HideInInspector] public bool atShop = false;
    private bool uncontrolable = false;

    private float currentShieldDelay;
    private bool shieldDelay;

    public LayerMask placeDefenseMask;

    private float nextTimeToFireAR = 0f;
    public LayerMask shootLayerMask;
    public GameObject bulletARPrefab;
    public GameObject grenadePrefab;
    public GameObject stickyGrenadePrefab;
    public GameObject bulletSpawner;
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
    private bool empActive;
    private float currentEmpDuration;
    private bool hasNormalGrenade;

    [HideInInspector] public int currentARChargerAmmoCount;

    /*public LayerMask m_ShootLayerMask;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;*/
    public GameObject impactHole;
    public LineRenderer lineRenderer;

    [Header("DEFENSES")]
    public Transform pointAttachDefense;
    public GameObject minePrefab;
    public GameObject terrainTurretPrefab;
    public GameObject airTurretPrefab;

    private float gravityMultiplier = 1;

    [HideInInspector] public bool onGround;
    [HideInInspector] public float verticalSpeed;
    [HideInInspector] public float currentHealth;
    [HideInInspector] public float currentShield;
    [HideInInspector] public bool shooting;
    [HideInInspector] public bool withDefense;
    [HideInInspector] public string defense;
    [HideInInspector] public GameObject attachedDefense;
    [HideInInspector] public bool gliding;
    [HideInInspector] public bool readyToGlind;

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
    }

    // Use this for initialization
    public void StartGame()
    {
        GameEvents.instance.onTransitionStart += OnTransitionStart;

        transform.position = initPos;

        pitch = 0;

        cash = playerModel.INITIAL_CASH;

        actualWeapon = Weapon.rifle;

        gc.uiController.ChangeCash(cash);
        gc.uiController.ChangeAROverheat(actualOverheat);

        currentARChargerAmmoCount = playerModel.MAX_CHARGER_AMMO_AR;
        currentHealth = playerModel.MAX_HEALTH;
        currentShield = playerModel.MAX_SHIELD;

        StartCoroutine(RecoverShield());

        ChangeState(new PSMovement(this));
    }

    // Update is called once per frame
    void Update()
    {
        currentState.Update(this);
        AnyStateUpdate();
        //Debug.Log(currentState);
        //Debug.Log(actualWeapon);
        //Debug.Log("Gliding: " + gliding);
        //Debug.Log(readyToGlind);
        //Debug.Log(onGround);
        if (Input.GetKeyDown(KeyCode.M)) TakeDamage(20, 0);
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
        //#if UNITY_EDITOR
        if (Input.GetKeyDown(playerModel.godMode))
        {
            godMode = !godMode;
            Debug.Log("God Mode: " + (godMode ? "enabled" : "disabled"));
        }
        //#endif 
        if (currentShieldDelay > 0 && shieldDelay) currentShieldDelay -= Time.deltaTime;
        else if (currentShieldDelay <= 0 && shieldDelay)
        {
            canRecover = true;
            shieldDelay = false;
        } 

        if (empActive)
        {
            currentEmpDuration -= Time.deltaTime;
            if (currentEmpDuration <= 0)
            {
                empActive = false;
                currentEmpDuration = playerModel.empDuration;
            }
        }

        if (Input.GetKeyDown(playerModel.pauseKey)) gc.uiController.Pause();
        if (Input.GetKeyDown(KeyCode.Y)) gc.shopController.MoveShop(true);
        if (Input.GetKeyDown(KeyCode.U)) gc.shopController.MoveShop(false);
    }

    public void TakeDamage(float damage, int whoAttacked)
    {
        if (godMode) return;

        if (currentShield > 0)
        {
            currentShield -= damage;
            gc.uiController.ChangeShield(currentShield);
        }
        else
        {
            currentHealth -= damage;
            gc.uiController.ChangeHealth(currentHealth);

            if (currentHealth <= 0 && !godMode)
            {
                ChangeState(new PSDead(this));
                return;
            }
        }
        canRecover = false;
        shieldDelay = true;
        currentShieldDelay = playerModel.secondsToWaitShield;
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

        //GRAVITY
        //…
        if (Input.GetKeyDown(playerModel.jumpKeyCode) != onGround && !readyToGlind) readyToGlind = true;
        if (verticalSpeed <= 0 && Input.GetKey(playerModel.jumpKeyCode) && !onGround && readyToGlind)
        {
            readyToGlind = false;
            gliding = true;
        }        
        if (!gliding) verticalSpeed += Physics.gravity.y * 1.5f * Time.deltaTime;

        //Debug.Log(verticalSpeed);

        if (verticalSpeed <= 0 && Input.GetKey(playerModel.jumpKeyCode) != onGround && gliding) l_Movement.y = playerModel.verticalGlideSpeed * Time.deltaTime;
        else l_Movement.y = verticalSpeed * Time.deltaTime;



        //JUMP
        CollisionFlags l_CollisionFlags = characterController.Move(l_Movement);

        if ((l_CollisionFlags & CollisionFlags.Below) != 0)
        {
            gliding = false;
            onGround = true;
            verticalSpeed = 0.0f;
        }
        else
            onGround = false;

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
        } 
    }

    public void UseDirectHability(int num)
    {
        SlotInfo sInfo = gc.shopController.habilitySlots[num].gameObject.GetComponent<SlotInfo>();
        if (sInfo.charges > 0) HabilityEffect(sInfo, num);
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

    public void HabilityEffect(SlotInfo sInfo, int num)
    {
        switch (sInfo.content)
        {
            case "Jetpack":
                verticalSpeed = playerModel.jetpackVerticalSpeed;
                if (gliding) gliding = false;
                sInfo.Consume();
                break;
            case "Grenade": 
                if (!lineRenderer.gameObject.activeSelf)
                {
                    grenadesSlotNum = num;
                    grenadeAmmo = sInfo.charges;
                    if (actualWeapon != Weapon.launcher) actualWeapon = Weapon.launcher;
                    if (withDefense)
                    {
                        DestroyDefense();
                        gun.SetActive(true);
                    }
                    hasNormalGrenade = true;
                    ChangeState(new PSGrenadeLauncher(this));
                }                
                break;
            case "Laser":
                if (!withDefense && !lineRenderer.gameObject.activeSelf)
                {
                    previousState = currentState;
                    sInfo.Consume();
                    ChangeState(new PSLaser(this));
                }                
                break;
            case "Health":
                if (!withDefense && !lineRenderer.gameObject.activeSelf && currentHealth < playerModel.MAX_HEALTH)
                {
                    currentHealth += playerModel.instantHealthCure;
                    if (currentHealth > playerModel.MAX_HEALTH) currentHealth = playerModel.MAX_HEALTH;
                    gc.uiController.ChangeHealth(currentHealth);
                    sInfo.Consume();
                }
                break;
            case "StickyGrenade":
                if (!lineRenderer.gameObject.activeSelf)
                {
                    grenadesSlotNum = num;
                    grenadeAmmo = sInfo.charges;
                    if (actualWeapon != Weapon.launcher) actualWeapon = Weapon.launcher;
                    if (withDefense)
                    {
                        DestroyDefense();
                        gun.SetActive(true);
                    }
                    hasNormalGrenade = false;
                    ChangeState(new PSGrenadeLauncher(this));
                }
                break;
            case "EMP":
                empActive = true;
                if (!withDefense && !lineRenderer.gameObject.activeSelf && empActive)
                {
                    Collider[] colliders = Physics.OverlapSphere(transform.position, playerModel.empRadius);

                    foreach (Collider nearbyObject in colliders)
                    {
                        GroundEnemy gEnemy = nearbyObject.GetComponent<GroundEnemy>();
                        if (gEnemy != null) Debug.Log("ground aturded"); //gEnemy.Aturd();
                        else
                        {
                            FlyingEnemy fEnemy = nearbyObject.GetComponent<FlyingEnemy>();
                            if (fEnemy != null) Debug.Log("fly aturded");// fEnemy.Aturd();
                        }
                    }
                    sInfo.Consume();
                }
                break;
        }
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
                foreach (BoxCollider collider in attachedDefense.GetComponentsInChildren<BoxCollider>()) collider.enabled = false;
                break;
            case "AirTurret":
                attachedDefense = Instantiate(airTurretPrefab, pointAttachDefense.position, pointAttachDefense.rotation);
                attachedDefense.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                attachedDefense.GetComponent<AirTurretController>().placed = false;
                attachedDefense.GetComponent<AirTurretController>().impactZone.enabled = false;
                foreach (BoxCollider collider in attachedDefense.GetComponentsInChildren<BoxCollider>()) collider.enabled = false;
                foreach (CapsuleCollider col in attachedDefense.GetComponentsInChildren<CapsuleCollider>()) col.enabled = false;
                break;
        }
    }

    public void UpdateAttachedDefense()
    {
        Vector3 eulerAngles = pointAttachDefense.rotation.eulerAngles;
        attachedDefense.transform.position = pointAttachDefense.position;
        attachedDefense.gameObject.transform.localEulerAngles = new Vector3(0.0f, eulerAngles.y, eulerAngles.z);
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
                    break;
                case "TerrainTurret":
                    GameObject terrainTurret = Instantiate(terrainTurretPrefab, hit.point, attachedDefense.transform.rotation);
                    terrainTurret.GetComponent<TerrainTurretController>().placed = true;
                    break;
                case "AirTurret":
                    GameObject airTurret = Instantiate(airTurretPrefab, hit.point, attachedDefense.transform.rotation);
                    airTurret.GetComponent<AirTurretController>().placed = true;
                    break;
            }
            DestroyDefense();
            gun.SetActive(true);
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
        gc.uiController.ChangeHealth(currentHealth);
        gc.uiController.ChangeShield(currentShield);
        gc.shopController.ResetHabilitiesAndDefenses();
        if (actualWeapon == Weapon.launcher)
        {
            actualWeapon = pastWeapon;
            currentState = previousState;
        } 
    }

    public void IncreaseCash(int cash)
    {
        this.cash += cash;
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

    public void Shoot()
    {
        switch (actualWeapon)
        {
            case Weapon.rifle:
                actualARShootCooldown = playerModel.shootARCooldown;
                GameObject bulletAR;
                RaycastHit hitAR;
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitAR, Mathf.Infinity, shootLayerMask))
                {
                    Vector3 dir = hitAR.point - bulletSpawner.transform.position;
                    dir.Normalize();
                    bulletAR = Instantiate(bulletARPrefab, bulletSpawner.transform.position, bulletSpawner.transform.rotation);
                    bulletAR.transform.forward = dir;
                    //bulletAR.GetComponent<Rigidbody>().AddForce(dir * playerModel.shootForceAR * bulletAR.GetComponent<Rigidbody>().mass, ForceMode.Impulse);
                }
                else
                {
                    bulletAR = Instantiate(bulletARPrefab, bulletSpawner.transform.position, Camera.main.transform.rotation);
                    //bulletAR.GetComponent<Rigidbody>().AddForce(bulletSpawner.transform.forward * playerModel.shootForceAR * bulletAR.GetComponent<Rigidbody>().mass, ForceMode.Impulse);
                }               

                actualOverheat += playerModel.bulletOverheat;

                gc.uiController.ChangeAROverheat(actualOverheat);
                if (actualOverheat >= playerModel.maxOverheatAR)
                {
                    actualOverheat = playerModel.maxOverheatAR;
                    saturatedAR = true;
                }
                break;
            case Weapon.shotgun:
                /*muzzleFlash.Play();
                muzzleFlash.GetComponentInChildren<WFX_LightFlicker>().StartCoroutine("Flicker");*/

                for (int i = 0; i < pellets.Count; i++)
                {
                    pellets[i] = Random.rotation;
                    Quaternion rot = Quaternion.RotateTowards(bulletSpawner.transform.rotation, pellets[i], playerModel.shotgunSpreadAngle);
                    RaycastHit hit; 
                    if (Physics.Raycast(Camera.main.transform.position, rot * Vector3.forward, out hit, playerModel.rangeShotgun, shootLayerMask))
                    {
                        Debug.DrawRay(Camera.main.transform.position, rot * Vector3.forward, Color.red);   
                        
                        //GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                        GameObject impactHoleGO = Instantiate(impactHole, new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z), Quaternion.LookRotation(hit.normal));

                        impactHoleGO.transform.parent = hit.transform;

                        GroundEnemy gEnemy = hit.collider.GetComponent<GroundEnemy>();
                        if (gEnemy != null) gEnemy.TakeDamage(playerModel.shotgunDamage);
                        else
                        {
                            FlyingEnemy fEnemy = hit.collider.GetComponent<FlyingEnemy>();
                            if (fEnemy != null) fEnemy.TakeDamage(playerModel.shotgunDamage);
                        }

                        /*imitHoles.Add(impactHoleGO);

                        if (limitHoles.Count == 25)
                        {
                            Destroy(limitHoles[0]);
                            limitHoles.Remove(limitHoles[0]);
                        }

                        GameController.instance.destroyObjects.Add(impactHoleGO);*/

                        //Destroy(impact, 2f);
                        Destroy(impactHoleGO, 20f);
                    }
                    actualShotgunShootCooldown = playerModel.shootShotgunCooldown;
                    shotgunShotted = true;
                }                
                break;
            case Weapon.launcher:
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
        SlotInfo sInfo = gc.shopController.habilitySlots[grenadesSlotNum].GetComponent<SlotInfo>();
        sInfo.Consume();
        grenadeAmmo--;       
    }

    public void CheckWeaponToShoot()
    {
        switch (actualWeapon)
        {
            case Weapon.rifle:
                 if(CanShootAR()) ChangeState(new PSShoot(this));
                break;
            case Weapon.shotgun:
                if (actualShotgunShootCooldown == 0) ChangeState(new PSShotgun(this));
                break;
            case Weapon.launcher:
                break;
        }
    }

    public void CoolOverheat()
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
       
        if (Time.time > 0.1f) gc.uiController.ChangeAROverheat(actualOverheat);
    }

    public bool CanShootAR()
    {
        if (saturatedAR) return false;
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
                pastWeapon = Weapon.rifle;
                actualWeapon = Weapon.shotgun;
                //animation
                break;
            case Weapon.shotgun:
                pastWeapon = Weapon.shotgun;
                actualWeapon = Weapon.rifle;
                //animation
                break;
            case Weapon.launcher:
                actualWeapon = pastWeapon;
                ChangeState(new PSMovement(this));
                //animation
                break;
        }
    }

    public void Die()
    {
        gc.uiController.GameOver(true);
    }

    //COLLISIONS
    private void OnCollisionEnter(Collision collision)
    {

    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "Shop")
        {
            gc.uiController.ShowInteractiveText("Press [" + playerModel.interactKey + "] to enter the shop");
            atShop = true;
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("EnemyAttack"))
        {
            if (collision.gameObject.GetComponentInParent<GroundEnemy>() != null) TakeDamage(collision.gameObject.GetComponentInParent<GroundEnemy>().damage, 0);
        }
        if (collision.tag == "CenterPlatform")
        {
            gc.roundController.OnTransitionTriggerEnter();
        }
    }

    private void OnTriggerStay(Collider collision)
    {
        if (collision.tag == "Lava")
        {
            TakeDamage (playerModel.lavaDamage * Time.deltaTime, 0);
        }
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

}
