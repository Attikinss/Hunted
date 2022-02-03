using UnityEngine;

public sealed class CrewMember : Pawn, ICanUse, ICanInteract {

    public float InteractionDistance { get => m_InteractionDistance; }

    [Header("Crew Member Properties")]
    [SerializeField, Tooltip("The distance in which the crew member can interact with objects in the world")]
    private float m_InteractionDistance = 1.5f;

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
        return false;
    }

    public bool UsePrimary() {
        return false;
    }

    public bool UseSecondary() {
        return false;
    }
}