using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : AController
{
    public PlayerModel playerModel;
    private Vector3 initPos;
    [HideInInspector] public bool godMode;

    public Transform m_PitchControllerTransform;

    [HideInInspector] public float yaw;
    [HideInInspector] public float pitch;

    CharacterController characterController;

    [HideInInspector] public bool moving;

    [HideInInspector] public bool atShop = false;
    private bool uncontrolable = false;

    private float nextTimeToFireAR = 0f;
    public LayerMask m_ShootLayerMask;
    public GameObject bulletARPrefab;
    public GameObject bulletSpawner;

    [HideInInspector] public int currentARChargerAmmoCount;

    /*public LayerMask m_ShootLayerMask;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;
    public GameObject impactHole;*/

    [HideInInspector] public bool onGround;
    [HideInInspector] public float verticalSpeed;
    [HideInInspector] public float currentHealth;
    [HideInInspector] public float currentShield;

    private PlayerState currentState;

    void Awake()
    {
        ChangeState(new PSMovement(this));
        initPos = transform.position;

        playerModel = Instantiate(playerModel);

        //ROTATION
        yaw = transform.rotation.eulerAngles.y;
        pitch = m_PitchControllerTransform.localRotation.eulerAngles.x;

        //MOVEMENT
        characterController = GetComponent<CharacterController>();

        //MOUSE
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Use this for initialization
    public void StartGame()
    {
        transform.position = initPos;

        pitch = 0;

        currentARChargerAmmoCount = playerModel.MAX_CHARGER_AMMO_AR;
        currentHealth = playerModel.MAX_HEALTH;
        currentShield = playerModel.MAX_SHIELD;

        ChangeState(new PSMovement(this));
    }

    // Update is called once per frame
    void Update()
    {
        currentState.Update(this);
        AnyStateUpdate();
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
    }

    public void TakeDamage(int damage, int whoAttacked)
    {
        if (godMode) return;

        currentHealth -= damage;
        gc.uiController.ChangeLife(currentHealth);

        if (currentHealth <= 0 && !godMode)
        {
            ChangeState(new PSDead(this));
            return;
        }

        float rnd = Random.Range(1, 5);
        AudioManager.instance.Play("PlayerHit" + rnd);

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
        if (Input.GetKey(playerModel.upKeyCode))
            l_Movement = l_Forward;
        else if (Input.GetKey(playerModel.downKeyCode))
            l_Movement = -l_Forward;

        if (Input.GetKey(playerModel.rightKeyCode))
            l_Movement += l_Right;
        else if (Input.GetKey(playerModel.leftKeyCode))
            l_Movement -= l_Right;

        //RUN && JUMP
        //…
        float l_SpeedMultiplier = 1.0f;
        if (Input.GetKey(playerModel.runKeyCode))
            l_SpeedMultiplier = playerModel.fastSpeedMultiplier;
        //…

        //…
        if (onGround && Input.GetKeyDown(playerModel.jumpKeyCode))
        {
            verticalSpeed = playerModel.jumpSpeed;
        }



        //M0VEMENT
        l_Movement.Normalize();
        l_Movement = l_Movement * Time.deltaTime * playerModel.speed * l_SpeedMultiplier;

        if (l_Movement == Vector3.zero) moving = false;
        else moving = true;

        //GRAVITY
        //…
        verticalSpeed += Physics.gravity.y * Time.deltaTime;
        l_Movement.y = verticalSpeed * Time.deltaTime;


        //JUMP
        CollisionFlags l_CollisionFlags = characterController.Move(l_Movement);

        if ((l_CollisionFlags & CollisionFlags.Below) != 0)
        {
            onGround = true;
            verticalSpeed = 0.0f;
        }
        else
            onGround = false;

        if ((l_CollisionFlags & CollisionFlags.Above) != 0 && verticalSpeed > 0.0f)
            verticalSpeed = 0.0f;
    }

    public void ShootAR()
    {
        if (!CanShootAR()) return;
        nextTimeToFireAR = Time.time + 1 / playerModel.fireRateAR;
        /*muzzleFlash.Play();
        muzzleFlash.GetComponentInChildren<WFX_LightFlicker>().StartCoroutine("Flicker");*/
        /*RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, range, m_ShootLayerMask))
        {
            Target target = hit.transform.GetComponent<Target>();
            if (target != null && target.readyToShoot)
            {
                target.Shooted();
                gc.ChangeShootingRangePoints(target.points);
            }

            HitCollider hitCollider = hit.collider.GetComponent<HitCollider>();
            if (hitCollider != null)
            {
                hitCollider.droneEnemy.Hit(hitCollider.hitColliderType == HitCollider.THitColliderType.HEAD ?
                damage : damage / 4, hitCollider.hitColliderType == HitCollider.THitColliderType.BODY ? false : true);
            }

            GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            GameObject impactHoleGO = Instantiate(impactHole, new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z), Quaternion.LookRotation(hit.normal));

            impactHoleGO.transform.parent = hit.transform;

            limitHoles.Add(impactHoleGO);

            if (limitHoles.Count == 25)
            {
                Destroy(limitHoles[0]);
                limitHoles.Remove(limitHoles[0]);
            }

            GameController.instance.destroyObjects.Add(impactHoleGO);

            Destroy(impact, 2f);
            Destroy(impactHoleGO, 20f);
        }*/
        GameObject bulletAR = Instantiate(bulletARPrefab, bulletSpawner.transform.position, bulletSpawner.transform.rotation);
        bulletAR.GetComponent<Rigidbody>().AddForce(bulletSpawner.transform.forward * playerModel.shootForce * bulletAR.GetComponent<Rigidbody>().mass, ForceMode.Impulse);

        currentARChargerAmmoCount--;
        gc.uiController.ChangeARAmmo(currentARChargerAmmoCount);

        if (currentARChargerAmmoCount == 0 /* i animacio de dispar acaba*/) ReloadAR();
    }

    bool CanShootAR()
    {
        if (currentARChargerAmmoCount != 0) return true;
        else return false;
    }

    public void ReloadAR()
    {
        currentARChargerAmmoCount = playerModel.MAX_CHARGER_AMMO_AR;
        gc.uiController.ChangeARAmmo(currentARChargerAmmoCount);
    }

    public bool CanReload()
    {
        if (currentARChargerAmmoCount < playerModel.MAX_CHARGER_AMMO_AR) return true;
        else return false;
    }

    private void ApplyDamage()
    {

    }

    public void Shop(bool show)
    {
        gc.uiController.shopInterface.SetActive(!gc.uiController.shopInterface.activeSelf);
        if (show)
        {
            ChangeState(new PSUncontrolable(this));
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            ChangeState(new PSMovement(this));
            Cursor.lockState = CursorLockMode.Locked;
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
