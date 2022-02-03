/// <summary>Used to give entities the ability to interact with objects that implement IInteractable interface.</summary>
public interface ICanInteract {
    float InteractionDistance { get; }
    bool Interact(IInteractable interactable);
}