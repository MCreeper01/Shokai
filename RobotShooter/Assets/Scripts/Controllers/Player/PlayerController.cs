using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : AController {

    public PlayerModel playerModel;
    private Vector3 initPos;
    [HideInInspector] public bool godMode;

    public Transform m_PitchControllerTransform;

    [HideInInspector] public float yaw;
    [HideInInspector] public float pitch;

    CharacterController characterController;

    [HideInInspector] public int currentChargerAmmoCount;

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

        //ROTATION
        yaw = transform.rotation.eulerAngles.y;
        pitch = m_PitchControllerTransform.localRotation.eulerAngles.x;

        //MOVEMENT
        characterController = GetComponent<CharacterController>();

        //MOUSE
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Use this for initialization
    public void StartGame () {

        playerModel = Instantiate(playerModel);
        transform.position = initPos;

        pitch = 0;

        currentHealth = playerModel.MAX_HEALTH;
        currentShield = playerModel.MAX_SHIELD;        

        ChangeState(new PSMovement(this));
    }
	
	// Update is called once per frame
	void Update () {
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
        AudioManager.instance.Pause("Footsteps");

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
    

    private void ApplyDamage()
    {
        
    }

    

    public void Die()
    {
        gc.uiController.GameOver(true);
    }

    //COLLISIONS
    private void OnCollisionEnter2D(Collision2D collision)
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        
    }
    
}
