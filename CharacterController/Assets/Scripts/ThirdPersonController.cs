using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    #region Variables
    [Header("Movement")]
    [SerializeField] bool canMove = true;
    [SerializeField] float accelerationTime;
    [SerializeField] float brakeTime;
    [SerializeField] float moveSpeed;
    [SerializeField] float moveSpeedMax;
    [SerializeField] float rotationSpeed = 720;
    //[SerializeField] float rotationToNormalSlopeSpeed = 100;

    [Header("Ground")]
    [SerializeField] bool isGrounded;
    [SerializeField] Transform groundTransform;
    [SerializeField] float groundRange;

    [Header("Slope")]
    [SerializeField] float raySlopeLength = 2;
    [SerializeField] LayerMask whatIsGround;
    //[SerializeField] bool isSliding;
    [SerializeField] float slideAcceleration;
    [SerializeField] float slideSpeedMax;
    [SerializeField] float slideBrake;
    [SerializeField] Transform[] groundRays;

    [Header("Jump")]
    [Tooltip("How much greater to be the number gravity slower is")]
    [SerializeField] float timeToJumpApex = .4f;
    [SerializeField] float maxJumpHeight = 4;
    [SerializeField] float minJumpHeight = 1;
    [Space]
    [SerializeField] float jumpBufferTime = 0.2f;
    float? lastJumpedTime;
    [SerializeField] float coyoteTime = 0.2f;
    float? lastGroundedTime;
    [Space]
    [SerializeField] float fallSpeedMax = -10f;

    [Header("WallJump")]
    [SerializeField] Transform wallRayPos;
    [SerializeField] bool isWallJumping;
    [SerializeField] bool isWallSliding;
    [SerializeField] float wallRayLength;
    [SerializeField] float wallJumpHeight;
    float walllJumpVelocity;
    [SerializeField] float wallForce;
    [SerializeField] float wallSmoothTime;
    [SerializeField] float wallSlideSpeed;

    [Header("Dash")]
    [SerializeField] bool isDashing;
    [SerializeField] float dashForce;
    [SerializeField] float dashSmoothTime;
    [SerializeField] float dashCooldown;

    [Header("Physics")]
    [SerializeField] float mass;
    [SerializeField] float impactMagnitudeMin = 0.2f;
    float impactSmoothTimeCurrent;

    [Header("LedgeClimb")]
    [SerializeField] bool isInLedge;
    [SerializeField] Transform ledgePos;
    [SerializeField] Transform ledgePosUp;
    [SerializeField] float ledgeLength;
    [Space]
    [SerializeField] float ledgeForce;
    [SerializeField] float ledgeBrake;
    [Space]
    [SerializeField] float ledgeJumpHeight;
    float ledgeJumpVelocity;

    [Header("Particles")]
    [SerializeField] ParticleSystem dustGround;
    [SerializeField] ParticleSystem dustWall;

    [Header("Discarded")]

    [Header("Trampolin Bounce")]
    [SerializeField] float trampolinHeight;
    float trampolinVelocity;


    [Header("Ladder")]
    [SerializeField] LayerMask whatIsLadder;
    [SerializeField] bool isClimbingLadder;
    [SerializeField] float climbLadderSpeed;
    [SerializeField] float climbLadderJumpHeight;
    float climbLadderJumpVelocity;
    //[SerializeField] float ladderOffset = 0.65f;

    [Header("Triple Jump")]
    [SerializeField] float secondJumpHeight;
    float secondJumpVelocity;
    [SerializeField] float thirdJumpHeight;
    float thirdJumpVelocity;
    [SerializeField] float tripleJumpCounter;
    [SerializeField] float tripleJumpMaxTime;

    //Jump
    float maxJumpVelocity;
    float minJumpVelocity;

    //Movement
    Vector3 moveInput;
    Vector3 moveDir;
    Vector3 velocity;
    float moveMagnitude;
    float speedSmoothing;
    Vector3 velocitySmoothing;

    //Gravity
    float gravity;

    //Impact
    Vector3 impact;
    Vector3 dashSmoothing;

    //Components
    Camera cam;
    CharacterController controller;
    Animator animator;

    public bool IsGrounded { get => isGrounded; set => isGrounded = value; }
    public bool IsInLedge { get => isInLedge; set => isInLedge = value; }
    #endregion

    void Awake()
    {
        cam = Camera.main;
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

    }

    // Start is called before the first frame update
    void Start()
    {
        ApplyNewVelocities();
    }


    // Update is called once per frame
    void Update()
    {
        if (canMove && Input.GetButtonDown("Start"))
            PauseMenu.instance.TogglePause();

        if (PauseMenu.instance.IsPaused)
            return;

        if (canMove)
        {
            Inputs();

            if (!IsInLedge)
            {
                Jump();
                Movement();
                Rotate();

                Dash();
                WallJump();
            }
            LedgeClimb();
        }

        ConsumeImpact();

        Velocity();

        Animations();

        isGrounded = Physics.CheckSphere(groundTransform.position, groundRange, whatIsGround);
    }

    #region Other
    void Animations()
    {
        animator.SetFloat("magnitude", moveMagnitude, 0.5f, Time.deltaTime);
        animator.SetFloat("vSpeed", velocity.y, 0.5f, Time.deltaTime);
        animator.SetBool("isGrounded", isGrounded);
    }

    void Inputs()
    {
        //Movement Axis
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        moveInput = new Vector3(h, 0, v);

        //Jump Buffer
        if (Input.GetButtonDown("Jump"))
            lastJumpedTime = Time.time;
    }

    public void ApplyNewVelocities()
    {
        gravity = -((2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2));


        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = FixedJumpHeight(minJumpHeight);

        secondJumpVelocity = FixedJumpHeight(secondJumpHeight);
        thirdJumpVelocity = FixedJumpHeight(thirdJumpHeight);

        walllJumpVelocity = FixedJumpHeight(wallJumpHeight);

        climbLadderJumpVelocity = FixedJumpHeight(climbLadderJumpHeight);

        ledgeJumpVelocity = FixedJumpHeight(ledgeJumpHeight);

        trampolinVelocity = FixedJumpHeight(trampolinHeight);
    }

    #endregion

    #region Basic Movement
    void Movement()
    {
        moveDir = moveInput;

        //Magnitud entre 0 y 1 para que la velocidad sea distinta según cuanto inclinemos el stick
        moveMagnitude = Mathf.Clamp01(moveDir.magnitude);

        //Normalizar: hacer que la magnitud sea 1 para que tenga la misma velocidad en diagonal
        moveDir.Normalize();

        //Suma al vector de movimiento el eje de ángulo de arriba de la cámara para que se mueve según su rotación
        moveDir = Quaternion.AngleAxis(cam.transform.rotation.eulerAngles.y, Vector3.up) * moveDir;

        //Acceleration
        if (moveMagnitude >= 0.1f)
            moveSpeed = Mathf.SmoothDamp(moveSpeed * moveMagnitude, moveSpeedMax, ref speedSmoothing, accelerationTime);
        else
            moveSpeed = 0;
    }

    void Jump()
    {
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
            velocity.y = 0;
        }

        if (IsPossibleToJump())
        {
            velocity.y = maxJumpVelocity;
            AudioManager.instance.Play("jump");
        }

        if (Input.GetButtonUp("Jump") && velocity.y > minJumpVelocity)
            velocity.y = minJumpVelocity;
    }

    void Velocity()
    {
        //Movement
        if (canMove)
        {
            velocity = new Vector3(
                moveDir.x * moveSpeed,
                velocity.y,
                moveDir.z * moveSpeed);
        }

        //Air speed
        if (!IsInLedge && !isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
            if (velocity.y <= -fallSpeedMax)
                velocity.y = -fallSpeedMax;
        }

        if (controller.collisionFlags == CollisionFlags.Above)
            velocity.y = 0;

        //Adjust velocity to the slope
        velocity = GetSlopeTilting();

        //Apply movement
        controller.Move(velocity * Time.deltaTime);
    }

    void Rotate()
    {
        if (moveInput != Vector3.zero && !isClimbingLadder && isGrounded)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            Quaternion rotateTowards = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, rotateTowards.eulerAngles.y, transform.eulerAngles.z);

            //transform.rotation = rotateTowards;
            //float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            //float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSmoothVelocity, rotationSmothTime);
        }
    }

    float FixedJumpHeight(float height)
    {
        return Mathf.Sqrt(2 * Mathf.Abs(gravity) * height);
    }

    bool IsRaycastInGround()
    {
        foreach (Transform ray in groundRays)
            if (Physics.Raycast(ray.position, Vector3.down, raySlopeLength, whatIsGround))
                return true;
        return false;
    }

    RaycastHit GetGroundRaycast()
    {
        RaycastHit theHit = new RaycastHit();
        foreach (Transform ray in groundRays)
            if (Physics.Raycast(ray.position, Vector3.down, out RaycastHit hit, raySlopeLength, whatIsGround))
                theHit = hit;
        return theHit;
    }

    Vector3 GetSlopeTilting()
    {
        //Detect slope
        if (IsRaycastInGround() && isGrounded)
        {
            //Get slope info
            float angle = Vector3.Angle(GetGroundRaycast().normal, Vector3.up);
            Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, GetGroundRaycast().normal);
            Vector3 adjustedVelocity = slopeRotation * velocity;

            //Slope Movement
            if (angle < controller.slopeLimit)
            {
                if (adjustedVelocity.y < 0)
                    return adjustedVelocity;
            }
            //Slide slope
            else if (velocity.y <= 0 && !IsPossibleToJump())
            {
                velocity.y -= slideAcceleration * Time.deltaTime;
                if (velocity.y <= -slideSpeedMax)
                    velocity.y = -slideSpeedMax;
                return Vector3.ProjectOnPlane(velocity, GetGroundRaycast().normal);
            }
        }
        return velocity;
    }

    #endregion

    #region Buffer And Coyote Times

    bool IsMaxTime(float? lastTime, float maxTime)
    {
        return Time.time - lastTime <= maxTime;
    }

    bool IsJumpBuffer()
    {
        return IsMaxTime(lastJumpedTime, jumpBufferTime);
    }

    bool IsCoyoteTime()
    {
        return IsMaxTime(lastGroundedTime, coyoteTime) && velocity.y <= 0;
    }

    bool IsPossibleToJump()
    {
        bool condition = IsCoyoteTime() && IsJumpBuffer();
        if (condition)
            ResetBufferAndCoyoteTimes();
        return condition;
    }

    void ResetJumpTime()
    {
        lastJumpedTime = null;
    }

    void ResetCoyoteTime()
    {
        lastGroundedTime = null;
    }

    void ResetBufferAndCoyoteTimes()
    {
        ResetJumpTime();
        ResetCoyoteTime();
    }

    #endregion

    #region Advanced Movement
    void WallJump()
    {
        if (Physics.Raycast(wallRayPos.position, transform.forward, out RaycastHit hit, wallRayLength, whatIsGround))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle == 90 && !isGrounded)
            {
                if (isWallJumping)
                {
                    velocity.y = 0;
                    isWallJumping = false;
                }

                if (velocity.y < 0)
                {
                    if (IsJumpBuffer())
                    {
                        Debug.Log("WALLJUMP: wall jump performed!");

                        ResetJumpTime();

                        transform.forward = hit.normal;

                        AddImpact(transform.forward, wallForce, wallSmoothTime);

                        velocity.y = walllJumpVelocity;

                        isWallJumping = true;

                        animator.SetTrigger("wallJump");

                        AudioManager.instance.Play("jump");
                    }
                    else
                    {
                        float dotDir = Vector3.Dot(transform.forward, moveDir);
                        if (dotDir >= 0 && dotDir <= 1 && moveDir.magnitude > 0.1f)
                        {
                            print("WALLJUMP: slide down");
                            transform.forward = -hit.normal;
                            isWallSliding = true;
                            velocity.y = -wallSlideSpeed;
                            if (!dustWall.isEmitting)
                                dustWall.Play();
                            AudioManager.instance.PlayOneShot("footstep");
                        }
                        else
                        {
                            isWallSliding = false;
                        }
                    }
                }
            }

        }
        else
        {
            isWallSliding = false;
            dustWall.Stop();
        }

        if (isGrounded)
            isWallSliding = false;

        if (isGrounded || velocity.y <= 0)
            isWallJumping = false;
    }

    void Dash()
    {
        if (isDashing || isWallSliding)
            return;

        if (Input.GetButtonDown("Dash"))
        {
            //Fallo encontado: no saltaba despues de hacer un dash porque aqui puse que la direccion
            //tambien fuera hacia arriba (+ Vector.3), incluso tambien en el aire.
            if (IsCoyoteTime())
            {
                ResetCoyoteTime();
                velocity.y = minJumpVelocity;
                print("Dash");
            }

            AddImpact(transform.forward, dashForce, dashSmoothTime);

            isDashing = true;

            Invoke("ResetDash", dashCooldown);

            animator.SetTrigger("dash");

            AudioManager.instance.Play("dash");
        }

    }

    void ResetDash()
    {
        isDashing = false;
    }

    void LedgeClimb()
    {
        Collider[] walls = Physics.OverlapSphere(ledgePos.position, ledgeLength, whatIsGround);

        if (walls.Length != 0)
        {
            Debug.Log("LEDGE detect ground in a radius");

            Vector3 direction = walls[0].transform.position - transform.position;
            direction.y = 0;

            Debug.DrawRay(ledgePosUp.position, direction, Color.red);
            Debug.DrawRay(ledgePos.position, direction, Color.red);

            //Important: the wall must have the pivot in the center for that the raycasts work
            if (
                !Physics.Raycast(ledgePosUp.position, direction, wallRayLength, whatIsGround)
                &&
                Physics.Raycast(ledgePos.position, direction, out RaycastHit hitLedge, ledgeLength, whatIsGround)
                &&
                !isGrounded
                &&
                velocity.y <= 0
                )
            {
                print("LEDGE Grabbed");

                if (IsInLedge)
                {
                    if (IsJumpBuffer())
                    {
                        print("LEDGE Climb");

                        ResetJumpTime();
                        velocity.y = ledgeJumpVelocity;
                        IsInLedge = false;
                    }
                }
                else
                {
                    float angle = Vector3.Angle(hitLedge.normal, Vector3.up);
                    if (angle == 90)
                    {
                        print("LEDGE Hang");

                        transform.forward = -hitLedge.normal;
                        velocity = Vector3.zero;
                        moveDir = Vector3.zero;
                        IsInLedge = true;
                        dustWall.Stop();
                        AudioManager.instance.Play("footstep");
                    }
                }
            }
            else
            {
                IsInLedge = false;
            }
        }
        else
        {
            IsInLedge = false;
        }

        animator.SetBool("isInLedge", IsInLedge);
    }

    #endregion

    #region Impact
    void ConsumeImpact()
    {
        if (impact.magnitude > impactMagnitudeMin)
        {
            controller.Move(impact * Time.deltaTime);
        }
        impact = Vector3.SmoothDamp(impact, Vector3.zero, ref dashSmoothing, impactSmoothTimeCurrent);

        //impact = Vector3.Lerp(impact, Vector3.zero, brakeCurrent * Time.deltaTime);
    }

    public void AddImpact(Vector3 dir, float force, float smoothTime)
    {
        impactSmoothTimeCurrent = smoothTime;

        dir.Normalize();

        if (dir.y < 0) dir.y = -dir.y;

        impact += dir.normalized * force / mass;
    }

    public void BounceTrampolin()
    {
        velocity.y = trampolinVelocity;
    }

    #endregion

    void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, Vector3.down * raySlopeLength);

        foreach (Transform ray in groundRays)
            Gizmos.DrawRay(ray.position, Vector3.down * raySlopeLength);

        Gizmos.DrawRay(wallRayPos.position, transform.forward * wallRayLength);

        //Gizmos.DrawWireSphere(ledgePos.position, ledgeLength);
        //Gizmos.DrawRay(ledgePos.position, transform.forward * ledgeLength);
        //Gizmos.DrawRay(ledgePosUp.position, transform.forward * ledgeLength);

        Gizmos.DrawWireSphere(groundTransform.position, groundRange);

        Gizmos.DrawRay(transform.position, moveDir * 2);

    }

    #region Descartado
    void Ladder()
    {
        //Start to grab ladder
        if (Physics.Raycast(wallRayPos.position, transform.forward, out RaycastHit hit, wallRayLength, whatIsLadder))
        {
            if (!isClimbingLadder && !isGrounded)
            {
                print("Coge escalera");

                isClimbingLadder = true;
                transform.forward = -hit.normal;
                velocity = Vector3.zero;
            }
        }
        else if (isClimbingLadder)
        {
            print("Deja escalera");

            isClimbingLadder = false;
            if (moveInput.z > 0.1f)
                velocity.y = climbLadderJumpVelocity;

        }

        //While it is in the ladder
        if (isClimbingLadder)
        {
            //Movement
            velocity.y = moveInput.z * climbLadderSpeed * Time.deltaTime;

            //Jump
            if (IsJumpBuffer())
            {
                print("Saltar desde escalera.");
                ResetJumpTime();
                AddBackwardImpact();
                isClimbingLadder = false;
            }

            //Drop ladder in the ground
            if (isGrounded)
                isClimbingLadder = false;
        }
    }

    void TripleJump()
    {
        if (IsPossibleToJump())
        {
            print("IsPossibleToJump");

            CancelInvoke("ResetTripleJumpCounter");

            tripleJumpCounter++;

            if (tripleJumpCounter <= 1)
            {
                velocity.y = maxJumpVelocity;
                print("Normal Jump");
            }
            if (tripleJumpCounter == 2)
            {
                velocity.y = secondJumpVelocity;
                print("Second Jump");
            }
            if (tripleJumpCounter == 3)
            {
                if (moveMagnitude >= 0.1f)
                {
                    velocity.y = thirdJumpVelocity;
                    Invoke("ResetTripleJumpCounter", tripleJumpMaxTime);
                    print("Third Jump");
                }
                else
                {
                    velocity.y = maxJumpVelocity;
                    ResetTripleJumpCounter();
                    print("Normal Jump");
                }
            }
        }

        //Regulable jump in normal and second jump
        if (Input.GetButtonUp("Jump") && velocity.y > minJumpVelocity && tripleJumpCounter < 3)
            velocity.y = minJumpVelocity;

        //Timer to reset triple jump counter on touch ground
        if (controller.collisionFlags == CollisionFlags.Below && velocity.y < -2)
            Invoke("ResetTripleJumpCounter", tripleJumpMaxTime);
    }

    void JumpBack()
    {
        if (moveInput.magnitude >= 0.1f)
        {
            if (Mathf.Clamp(moveInput.x, -1, 1) < Mathf.Clamp(velocity.x, -1, 1))
            {
                print("TurnAround");
            }
        }
    }

    void ResetCanMove()
    {
        canMove = true;
    }

    void AddBackwardImpact()
    {
        //AddImpact(-transform.forward, wallForce, wallSmoothTime);
        transform.rotation = Quaternion.LookRotation(-transform.forward, Vector3.up);
        velocity.y = walllJumpVelocity;
        lastJumpedTime = null;
    }

    void ResetTripleJumpCounter()
    {
        tripleJumpCounter = 0;
    }
    #endregion
}

/*
var ray = new Ray(transform.position, Vector3.down);
if (Physics.Raycast(ray, out RaycastHit hitInfo, raySlopeLength, whatIsGround))
{
    var slopeRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
    var adjustedVelocity = slopeRotation * velocity;

    if (adjustedVelocity.y < 0)
    {
        return adjustedVelocity;
    }
}

return velocity;

Velocity()
        if (canMove)
        {
            //if (!isSliding)
            velocity = moveDir * moveSpeed;
        }
        // Bug del salto pared hasta el infinito y más alla
        // Ocurria porque movdir (0), moveMagnitude(0) y moveSpeed (6) al multiplicarlo y luego darle la velocidad Y por frame entonces hace que multiplique por la moverSpeed
        // Solucion: simplemente que si no puiede moverse la velocity es cero y ya,
        //else
        //    velocity = Vector3.zero;

        velocity = GetSlopeTilting();

        velocity.y += ySpeed;
        //}
    Vector3 AdjustVelocityToSlope(Vector3 velocity)
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, raySlopeLength, whatIsGround) && isGrounded)
        {
            var slopeRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * velocity;

            float angle = Vector3.Angle(hit.normal, Vector3.up);

            if (angle < controller.slopeLimit)
                if (slopeRotation.y < 0)
                    return slopeRotation;
        }

        return velocity;

    }

    Vector3 SlopeSlideVelocity()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, raySlopeLength, whatIsGround) && isGrounded && ySpeed <= 0)
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);

            if (angle >= controller.slopeLimit)
            {
                isSliding = true;
                return Vector3.ProjectOnPlane(new Vector3(moveDir.x * moveSpeed, ySpeed, moveDir.z * moveSpeed), hit.normal);
            }
        }
        isSliding = false;
        return Vector3.zero;
    }
*/