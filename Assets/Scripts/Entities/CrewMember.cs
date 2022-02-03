using UnityEngine;

public sealed class CrewMember : Pawn, ICanUse, ICanInteract {

    public float InteractionDistance { get => m_InteractionDistance; }

    [Header("Crew Member Properties")]
    [SerializeField, Tooltip("The distance in which the crew member can interact with objects in the world")]
    private float m_InteractionDistance = 1.5f;
    private Inventory m_Inventory;

    private void Awake() => OnAwake();
    private void Update() => OnUpdate();

    protected override void OnAwake() {
        base.OnAwake();

        m_Inventory = new Inventory(this);
    }

    protected override void OnUpdate() {
        base.OnUpdate();
    }

    public GameObject GetObjectInView() {
        // Create ray from view camera
        Ray ray = new Ray(m_PawnView.transform.position, m_PawnView.transform.forward);

        // Find object within interaction distance
        if (Physics.Raycast(ray, out RaycastHit hitInfo, m_InteractionDistance)) {
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
}