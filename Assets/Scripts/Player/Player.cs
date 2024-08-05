using System;
using System.Collections;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    // this script controls everything about the player e.g. position, state, look, interact.
    public static Player instance { get; private set; }
    public bool 
        lookActive = true,
        moveActive = true,
        newMovement,
        frictionType;
    public float
        groundedAccelerate = 15f,
        airAccelerate = 15f,
        acceleration = 15f,
        maxVelocityGrounded = 6.5f,
        maxVelocityAir = 6.5f;
    [Range(0.01f, 1f)] public float 
        friction1 = 0.7f;
    [Range(1f, 5f)] public float 
        friction2 = 1f;
    [Space] public bool 
        moveFixedUpdate;
    public Vector2 
        mouseDelta,
        mouseDeltaMultiplier = Vector2.one,
        lookSensitivity,
        playerEulerAngles;
    public Vector3
        movementDirection;//, fakeVelocity;
    public Transform
        cameraTransformReadOnly;
    public int[]
        gridIndex = new int[2]{ 0, 0 };
    public float 
        movementSpeedReadOnly { get; private set; }
    public Rigidbody 
        rb { get; private set; }
    public Game.Directions 
        facingDirection;
    public float 
        playerSpeed;
    public bool sprinting { get; private set; }
    public bool crouched { get; private set; }
    public bool grounded { get; private set; }

    [SerializeField, Range(0f, 0.99f)] float 
        friction = 0.85f;
    [SerializeField, Range(0f, 0.1f)] float 
        forceToApplyFriction = 0.1f,
        flatvelMin = 0.1f;
    [SerializeField] float
        walkSpeed = 4f,
        sprintSpeed = 6.5f,
        cameraHeight = 0.825f,
        movementAcceleration = 0.1f,
        movementDecceleration = 0.05f,
        jumpForce = 5f,
        playerHeightCM = 180f,
        playerCrouchHeightCM = 100f,
        groundedRayDistance = 1f,
        movementRampTime;
    [SerializeField] int 
        fakeVelocityDecimals = 4;
    [SerializeField] GameObject
        playerCapsule,
        groundedRayOrigin;

    Vector3 
        smoothInputVelocity,
        smoothInput,
        walkingAnimVector,
        //cameraPositionOutput,
        positionLastFrame;
    RigidbodyConstraints
        rbConstraintsDefault;
    Vector3 
        debugForce,
        debugFlatvel;
    bool 
        teleported;

    void Awake()
    {
        instance = this;
        Application.targetFrameRate = 165;
        QualitySettings.vSyncCount = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
        rbConstraintsDefault = rb.constraints;     
    }
    void Start()
    {
        gridIndex = transform.position.WorldPositionToGridIndex();
    }
    void Update()
    {
        //Vector3 speedVector = movementDirection.Multiply(playerCapsule.transform.InverseTransformDirection(rb.velocity));
        //Vector3 speedVector = playerCapsule.transform.InverseTransformDirection(rb.velocity);
        //playerSpeed = MathF.Round(speedVector.x + speedVector.z, 4);
        playerSpeed = MathF.Round(playerCapsule.transform.InverseTransformDirection(rb.velocity).z, 4);
        if (moveActive && !moveFixedUpdate) { Move(); }
        Crouch();
        cameraTransformReadOnly.position = CameraHandler.mainCamera.transform.position;
        movementSpeedReadOnly = sprinting ? sprintSpeed : walkSpeed;
        GroundedCheck();
        facingDirection = Extensions.EulerToCardinal(cameraTransformReadOnly.eulerAngles.y);
    }
    void FixedUpdate()
    {
        if (moveActive && moveFixedUpdate) { Move(); }
    }
    void LateUpdate()
    {
        if (lookActive) { Look(); }
        if (WorldGen.instance.mazeRenderAuto)
        {
            int[] gridPositionOld = gridIndex;
            gridIndex = transform.position.WorldPositionToGridIndex();
            if (!gridIndex.EqualTo(gridPositionOld)) { ChunkRenderer.instance.MazeRenderUpdate(); }
        }
    }
    /// <summary>
    /// Controls the camera view of the player - where they are looking
    /// </summary>
    void Look()
    {
        playerEulerAngles += mouseDelta * lookSensitivity;
        playerEulerAngles.y = Math.Clamp(playerEulerAngles.y, -90f, 90f);

        Quaternion newRotation = Quaternion.AngleAxis(playerEulerAngles.x, Vector3.up)
                                * Quaternion.AngleAxis(playerEulerAngles.y, Vector3.left);

        cameraTransformReadOnly.localRotation = newRotation;
        playerCapsule.transform.localEulerAngles = new Vector3(0f, newRotation.eulerAngles.y, 0f);
        CameraHandler.mainCamera.transform.localEulerAngles = new Vector3(newRotation.eulerAngles.x, 0f, 0f);
    }
    /// <summary>
    /// <para>Manually sets the rotation of the player</para>
    /// <para>Used instead of Player.instance.transform.eulerAngles = Vector3</para>
    /// </summary>
    /// <param name="eulerAngles"></param>
    public void LookSet(Vector3 eulerAngles) => playerEulerAngles = eulerAngles;
    void Move()
    {
        
        // ---------- new ------------

        if (newMovement)
        {
            float maxVelocity = movementSpeedReadOnly;
            Vector3 movementDirectionGlobal = playerCapsule.transform.TransformDirection(movementDirection);
            if (grounded)
            {
                //maxVelocity = maxVelocityGrounded;
                //acceleration = groundedAccelerate;
                // apply friction
                float speed = rb.velocity.magnitude;
                    
                if (speed <= flatvelMin) { rb.velocity = Vector3.zero; }

                if (frictionType)
                {
                    if (speed > 0) // Scale the velocity based on friction.
                    {
                        rb.velocity *= (speed - (speed * friction2 * Time.fixedDeltaTime)) / speed;
                    }

                }
                else
                {
                    rb.velocity *= friction1;
                }
            }
            else
            {
                //maxVelocity = maxVelocityAir;
                //acceleration = airAccelerate;
            }
            float projVel = Vector3.Dot(rb.velocity, movementDirectionGlobal); // Vector projection of Current velocity onto accelDir.
            float accelVel = acceleration * Time.fixedDeltaTime; // Accelerated velocity in direction of movment

            // If necessary, truncate the accelerated velocity so the vector projection does not exceed max_velocity
            if (projVel + accelVel > maxVelocity) { accelVel = maxVelocity - projVel; }
            rb.velocity += movementDirectionGlobal * accelVel;
        }

        // ---------- old ------------

        else
        {
            //movementDirection == Vector3.zero ? movementDecceleration : movementAcceleration;
            smoothInput = Vector3.SmoothDamp(smoothInput, movementDirection, ref smoothInputVelocity, movementRampTime);
            Vector3
                smoothedInputDirectional = playerCapsule.transform.TransformDirection(smoothInput),
                force = new(smoothedInputDirectional.x, 0.0f, smoothedInputDirectional.z),
                flatvel = rb.velocity;
            flatvel.y = 0.0f;
            if (force.magnitude > forceToApplyFriction)
            {
                //if (OnValidSlope())
                {
                    flatvel += force;
                    flatvel = Vector3.ClampMagnitude(flatvel, movementSpeedReadOnly);
                    rb.velocity = new(flatvel.x, rb.velocity.y, flatvel.z);
                }
            }
            else
            {
                if (grounded)
                {
                    flatvel *= friction;
                    if (flatvel.magnitude <= flatvelMin) { flatvel = Vector3.zero; }
                    rb.velocity = new(flatvel.x, rb.velocity.y, flatvel.z);
                }
            }
            debugFlatvel = flatvel;
            debugForce = force;
        }
        
        //float acceleration = movementDirection != Vector3.zero ? movementAcceleration : movementDecceleration;
        //smoothInput = Vector3.SmoothDamp(smoothInput, movementDirection, ref smoothInputVelocity, acceleration);
        //rb.MovePosition(rb.position + (movementSpeedReadOnly * Time.deltaTime * playerCapsule.transform.TransformDirection(smoothInput)));
        //Vector3 velocity = (movementSpeedReadOnly * Time.deltaTime) * playerCapsule.transform.TransformDirection(smoothInput);
        //rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
    }
    void GroundedCheck()
    {
        grounded = MathF.Round(rb.velocity.y, 4) == 0; ;

        if (Physics.Raycast(new Ray(groundedRayOrigin.transform.position, Vector3.down), out RaycastHit groundedRay, groundedRayDistance))
        {
            
        };
        //Debug.DrawLine(groundedRayOrigin.transform.position, groundedRayOrigin.transform.position + Vector3.down * groundedRayDistance, Color.red);

    }
    public void Jump()
    {
        if (!grounded) { return; }
        rb.AddForce(jumpForce * Vector3.up, ForceMode.VelocityChange);
        //rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
    }
    /// <summary>
    /// Handles player height and crouch method
    /// </summary>
    void Crouch()
    {
        playerCapsule.transform.localScale = new Vector3(playerCapsule.transform.localScale.x, (crouched ? playerCrouchHeightCM : playerHeightCM) / 200, playerCapsule.transform.localScale.z);
        //cameraTransformReadOnly.localPosition = new Vector3(0f, cameraHeight, 0f);
        //CameraHandler.mainCamera.transform.position = new Vector3(CameraHandler.mainCamera.transform.position.x, cameraTransformReadOnly.position.y, CameraHandler.mainCamera.transform.position.z) + cameraPositionOutput;
    }
    /// <summary>
    /// Instantly teleports the player to the given position and rotation
    /// </summary>
    /// <param name="worldSpacePosition"></param>
    /// <param name="worldSpaceEulerAngles"></param> 
    public void TeleportInstant(Vector3 worldSpacePosition, Vector3 worldSpaceEulerAngles)
    {
        rb.velocity = Vector3.zero;
        rb.position = worldSpacePosition;
        playerEulerAngles.x = worldSpaceEulerAngles.y;
        float yAngle = 0;
        if (worldSpaceEulerAngles.x == 0) { yAngle = 0; }
        else if (worldSpaceEulerAngles.x > 0 && worldSpaceEulerAngles.x <= 90) { yAngle = 0 - worldSpaceEulerAngles.x; }
        else if (worldSpaceEulerAngles.x < 360 && worldSpaceEulerAngles.x >= 270) { yAngle = 360 - worldSpaceEulerAngles.x; }
        playerEulerAngles.y = yAngle;
        //Debug.Log("teleported to; pos: " + worldSpacePosition + ", inrot: " + worldSpaceEulerAngles + ", outrot: " + playerEulerAngles);
    }
    public void SetSprint(bool state) 
    {
        if (!state & movementDirection != Vector3.zero) { return; }
        sprinting = state;
    }
    public void SetCrouch(bool state) => crouched = state;
    public void PlayerFreeze(bool state) => rb.constraints = state ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.FreezeRotation;
    public StringBuilder debugGetStats()
    {
        return new StringBuilder(uiDebug.str_playerTitle)
            .Append(uiDebug.str_targetFPS).Append(Application.targetFrameRate).Append(uiDebug.str_vSync).Append(QualitySettings.vSyncCount)
            //.Append(uiDebug.str_mouseRotation).Append(mouseDelta.ToStringBuilder()).Append(uiDebug.str_multiply).Append(mouseDeltaMultiplier.ToStringBuilder())
            .Append(uiDebug.str_lookSensitivity).Append(lookSensitivity.ToStringBuilder())
            .Append("\nplayerHeightCM = ").Append(playerHeightCM.ToString())
            .Append("\ncapsuleScale = ").Append(playerCapsule.transform.localScale.ToStringBuilder())
            .Append("\nmovementDirection = ").Append(movementDirection.ToStringBuilder())
            .Append("\nwalkingAnimVector = ").Append(walkingAnimVector.ToStringBuilder())
            //.Append("\nvelocityFake = ").Append(fakeVelocity.ToStringBuilder())
            .Append("\ngrounded = ").Append(grounded.ToString())
            //.Append("\nforce = ").Append(debugForce.Round(4).ToStringBuilder()).Append(", magnitude = ").Append(MathF.Round(debugForce.magnitude, 4))
            //.Append("\nflatvel = ").Append(debugFlatvel.Round(4).ToStringBuilder()).Append(", magnitude = ").Append(MathF.Round(debugFlatvel.magnitude, 4))
            .Append("\nvelocity = ").Append(rb.velocity.Round(4).ToStringBuilder())
            .Append("\nspeed = ").Append(playerSpeed.ToString()).Append("m/s\n");
            //.Append("\n playerDirection = ").Append(facingDirection).Append(" = ").Append(facingDirection.Euler());
    }
}   