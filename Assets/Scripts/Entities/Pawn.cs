using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public abstract class Pawn : MonoBehaviour {

    [Header("Pawn Propterties")]
    [SerializeField, Tooltip("The camera that represents the pawn's view")] protected Camera m_PawnView;
    [SerializeField, Tooltip("How quickly the pawn and its view camera can turn")] protected float m_LookSensitivity = 60.0f;
    [SerializeField, Tooltip("The pawn's walking/crouch movement speed")] protected float m_CrouchSpeed = 2.0f;
    [SerializeField, Tooltip("The pawn's default movment speed")] protected float m_DefaultSpeed = 4.0f;
    [SerializeField, Tooltip("The pawn's run speed")] protected float m_RunSpeed = 7.5f;
    [SerializeField, Tooltip("How quickly the pawn accelerates to its target speed")] protected float m_Acceleration = 20.0f;
    [SerializeField, Tooltip("How strong the pawn's jump is, resulting in more/less height")] protected float m_JumpForce = 5.5f;
    [SerializeField, Tooltip("Speed modifier for when pawn is grounded.")] protected float m_OnGroundMoveModifier = 1.0f;
    [SerializeField, Tooltip("Speed modifier for when pawn is in air.")] protected float m_InAirMoveModifier = 0.1f;
    [SerializeField, Tooltip("[DEBUG ONLY] The pawn's current movement speed")] protected float m_MoveSpeed = 0.0f;
    [SerializeField, Tooltip("[DEBUG ONLY] The pawn's current movement velocity")] protected Vector3 m_MoveVelocity = Vector3.zero;

    /// <summary>If true the pawn is currently crouching.</summary>
    public bool IsCrouching { get; private set; }
    
    /// <summary>If true the pawn is currently sprinting.</summary>
    public bool IsSprinting { get; private set; }
    
    /// <summary>If true the pawn is currently grounded.</summary>
    public bool IsGrounded { get; private set; }

    /// <summary>The cached user input used to control the pawn's movement.</summary>
    private Vector2 m_MoveInput = Vector2.zero;

    /// <summary>The cached user input used to control the pawn's look/rotation.</summary>
    private Vector2 m_LookInput = Vector2.zero;
    
    /// <summary>If true the pawn will jump next time <see cref="Jump"/> is called.</summary>
    private bool m_Jump = false;

    /// <summary>If true the pawn is able to jump.</summary>
    private bool m_JumpReady = true;

    /// <summary>The currently linked input handler controlling the pawn.</summary>
    protected PlayerInput m_InputOverride;

    /// <summary>The character controller used for process the pawn's movement.</summary>
    protected CharacterController m_Controller;

    protected virtual void OnAwake() {
         m_Controller = GetComponent<CharacterController>();
    }

    protected virtual void OnStart() {
    }

    protected virtual void OnUpdate() {
        Look();
        Jump();
        Move();
    }
    
    /// <summary>Links an input handler to this pawn allowing control over it.</summary>
    /// <param name="input">The input handler to link to.</param>
    /// <param name="forceOverride">If true, the currently linked input handler will be detatched.</param>
    /// <returns>True if input handler successfully attached to pawn.</returns>
    public bool Attach(PlayerInput input, bool forceOverride = false) {
        if (m_InputOverride != null) {
            if (forceOverride) {
                Detach(m_InputOverride);
            }
            else {
                // Input does not have authority to detach existing input handler
                return false;
            }
        }

        // Subscribe to input events
        input.actions["Crouch"].performed += SetCrouch;
        input.actions["Crouch"].canceled += SetCrouch;
        input.actions["Jump"].performed += SetJump;
        input.actions["Look"].performed += SetLookInput;
        input.actions["Move"].performed += SetMoveInput;
        input.actions["Move"].canceled += SetMoveInput;
        input.actions["Sprint"].performed += SetSprint;
        input.actions["Sprint"].canceled += SetSprint;

        // Input handler successfully linked
        m_InputOverride = input;
        return true;
    }

    /// <summary>Unlinks an input handler from this pawn; releasing control over it.</summary>
    /// <param name="input">The input handler to unlink from.</param>
    public void Detach(PlayerInput input) {
        
        // Unsubscribe from input events
        input.actions["Crouch"].performed -= SetCrouch;
        input.actions["Crouch"].canceled -= SetCrouch;
        input.actions["Jump"].performed -= SetJump;      
        input.actions["Look"].performed -= SetLookInput;
        input.actions["Move"].performed -= SetMoveInput;
        input.actions["Move"].canceled -= SetMoveInput;
        input.actions["Sprint"].performed -= SetSprint;
        input.actions["Sprint"].canceled -= SetSprint;
    }

    /// <summary>Moves the pawn.</summary>
    protected virtual void Move() {
        // Calculate target movement speed
        m_MoveSpeed = IsCrouching ? m_CrouchSpeed : IsSprinting ? m_RunSpeed : m_DefaultSpeed;
        
        // Apply in air movement mod to movement velocity
        float movementMod = IsGrounded ? m_OnGroundMoveModifier : m_InAirMoveModifier * 0.5f;
        m_MoveVelocity += (transform.right * m_MoveInput.x + transform.forward * m_MoveInput.y)
            * m_Acceleration * Time.deltaTime * m_MoveSpeed * movementMod;

        // Apply gravity
        m_MoveVelocity += Physics.gravity * Time.deltaTime;

        // Clamp velocity ignoring Y component
        float yVelocity = m_MoveVelocity.y;
        m_MoveVelocity.y = 0.0f;
        m_MoveVelocity = Vector3.ClampMagnitude(m_MoveVelocity, m_MoveSpeed);
        m_MoveVelocity.y = yVelocity;

        // Move pawn and check grounded state
        m_Controller.Move(m_MoveVelocity * Time.deltaTime);
        IsGrounded = m_Controller.isGrounded;

        // Decelerate over time ignoring Y component
        float deccelerationMod = IsGrounded ? m_OnGroundMoveModifier : m_InAirMoveModifier;
        m_MoveVelocity -= m_MoveVelocity * m_Acceleration * deccelerationMod * Time.deltaTime;
        m_MoveVelocity.y = yVelocity;
    }
    
    /// <summary>Rotates the pawn and its view camera.</summary>
    protected virtual void Look() {
        // Rotate pawn
        transform.rotation = Quaternion.Euler(0.0f, m_LookInput.x, 0.0f);

        // Rotate camera if one is assigned
        if (m_PawnView != null)
            m_PawnView.transform.localRotation = Quaternion.Euler(-m_LookInput.y, 0.0f, 0.0f);
    }
    
    /// <summary>Applies a jump velocity to the pawn.</summary>
    protected virtual void Jump() {
        if (IsGrounded)
        {
            // Downwards force is always applied by gravity so it needs to be clamped to
            // ensure that if the pawn were to run off the edge of a surface their velocity
            // isn't so ridiculously high that it appears as if they're teleporting
            m_MoveVelocity.y = -2.0f;// Mathf.Clamp(m_MoveVelocity.y, -2.0f, m_JumpForce);

            if (m_Jump)
            {
                m_Jump = false;
                StartCoroutine(JumpCooldown(0.25f));

                // Apply jump velocity
                m_MoveVelocity.y = m_JumpForce;
            }
        }
    }

    /// <summary>A timer for the pawn's jump ability.</summary>
    /// <param name="cooldownTime">The delay before the pawn can jump again.</param>
    protected IEnumerator JumpCooldown(float cooldownTime)
    {
        m_JumpReady = false;
        yield return new WaitForSeconds(cooldownTime);
        m_JumpReady = true;
    }

    /// <summary>Prapares the pawn to jump if possible.</summary>
    public void SetJump() {
        m_Jump = m_JumpReady;
    }

    /// <summary>Sets the pawn's crouch state.</summary>
    /// <param name="crouch">If true pawn will crouch</param>
    public void SetCrouch(bool crouch) {
        IsCrouching = crouch;
    }

    /// <summary>Sets the pawn's sprint state.</summary>
    /// <remarks>
    /// <br>NOTE: Pawn can only begin sprinting if not crouched (see: <see cref="SetCrouch"/>)</br>
    /// </remarks>
    /// <param name="sprint">If true pawn will sprint.</param>
    public void SetSprint(bool sprint) {
        IsSprinting = sprint && !IsCrouching;
    }

    /// <summary>Sets the pawn's movement direction relative to its forward direction.</summary>
    /// <remarks>
    /// <br>NOTE: Input is automatically normalized to help prevent unpredictable movement.</br>
    /// </remarks>
    /// <param name="moveInput">If true pawn will sprint.</param>
    public void SetMoveInput(Vector2 moveInput) {
        m_MoveInput = moveInput.normalized;
    }

    /// <summary>Accumulates look input to rotate pawn.</summary>
    /// <remarks>
    /// <br>NOTE: Vertical input is clamped between -90 and 90.</br>
    /// </remarks>
    /// <param name="look">If true pawn will sprint.</param>
    public void SetLookInput(Vector2 lookInput) {
        m_LookInput += lookInput;
        m_LookInput.y = Mathf.Clamp(m_LookInput.y, -90.0f, 90.0f);
    }

    /// <summary>Input callback passthrough method for jumping.</summary>
    /// <param name="ctx">The input action context.</param>
    private void SetJump(InputAction.CallbackContext ctx) {
        SetJump();
    }

    /// <summary>Input callback passthrough method for crouching.</summary>
    /// <param name="ctx">The input action context.</param>
    private void SetCrouch(InputAction.CallbackContext ctx) {
        SetCrouch(ctx.ReadValueAsButton());
    }

    /// <summary>Input callback passthrough method for sprinting.</summary>
    /// <param name="ctx">The input action context.</param>
    private void SetSprint(InputAction.CallbackContext ctx) {
        SetSprint(ctx.ReadValueAsButton());
    }

    /// <summary>Input callback passthrough method for movement.</summary>
    /// <param name="ctx">The input action context.</param>
    private void SetMoveInput(InputAction.CallbackContext ctx) {
        SetMoveInput(ctx.ReadValue<Vector2>());
    }

    /// <summary>Input callback passthrough method for looking.</summary>
    /// <param name="ctx">The input action context.</param>
    private void SetLookInput(InputAction.CallbackContext ctx) {
        SetLookInput(ctx.ReadValue<Vector2>());
    }
}