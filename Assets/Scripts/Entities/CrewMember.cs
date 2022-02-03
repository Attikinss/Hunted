using UnityEngine;
using UnityEngine.InputSystem;

public sealed class CrewMember : Pawn, ICanUse, ICanInteract {

    public float InteractionDistance { get => m_InteractionDistance; }

    [Header("Crew Member Properties")]
    [SerializeField, Tooltip("The distance in which the crew member can interact with objects in the world")]
    private float m_InteractionDistance = 2.5f;
    private Inventory m_Inventory;
    private Item m_HeldItem;

    private void Awake() => OnAwake();
    private void Update() => OnUpdate();

    protected override void OnAwake() {
        base.OnAwake();

        m_Inventory = new Inventory(this);
    }

    protected override void OnUpdate() {
        base.OnUpdate();
    }

    public override bool Attach(PlayerInput input, bool forceOverride = false) {
        if (!base.Attach(input, forceOverride)) {
            // Could not detach parent class
            return false;
        }

        // Subscribe to input events
        input.actions["Interact"].performed += Interact;
        input.actions["Primary"].performed += UsePrimary;
        input.actions["Secondary"].performed += UseSecondary;

        return true;
    }

    public override void Detach(PlayerInput input) {
        base.Detach(input);

        // Unsubscribe from input events
        input.actions["Interact"].performed -= Interact;
        input.actions["Primary"].performed -= UsePrimary;
        input.actions["Secondary"].performed -= UseSecondary;
    }

    public GameObject GetObjectInView() {
        // Create ray from view camera
        Ray ray = new Ray(m_PawnView.transform.position, m_PawnView.transform.forward);

        // Find object within interaction distance
        if (Physics.Raycast(ray, out RaycastHit hitInfo, m_InteractionDistance)) {
            Debug.Log($"Object in view: [{hitInfo.transform.gameObject.name}]");
            return hitInfo.transform.gameObject;
        }

        return null;
    }

    public bool Interact(IInteractable interactable) {
        if (interactable != null) {
            InteractionType interaction = interactable.Interact();
            return interaction != InteractionType.NONE;
        }

        return false;
    }

    public bool UsePrimary(IUsable usable) {
        if (usable != null) {
            UsageType usage = usable.UsePrimary();
            return usage != UsageType.NONE;
        }

        return false;
    }

    public bool UseSecondary(IUsable usable) {
        if (usable != null) {
            UsageType usage = usable.UseSecondary();
            return usage != UsageType.NONE;
        }

        return false;
    }

    private void Interact(InputAction.CallbackContext ctx) {
        GameObject obj = GetObjectInView();
        if (obj != null && obj.TryGetComponent(out IInteractable interactable)) {
            // TODO: Do something with return value
            Interact(interactable);
        }
    }

    private void UsePrimary(InputAction.CallbackContext ctx) {
        UsePrimary(m_HeldItem);
    }

    private void UseSecondary(InputAction.CallbackContext ctx) {
        UseSecondary(m_HeldItem);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(m_PawnView.transform.position, m_PawnView.transform.position + m_PawnView.transform.forward * InteractionDistance);
    }
}