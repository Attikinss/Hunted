public interface ICanInteract {
    float InteractionDistance { get; }
    bool Interact(IInteractable interactable);
}