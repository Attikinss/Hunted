using UnityEngine;

public abstract class Item : MonoBehaviour, IUsable, IInteractable {

    protected virtual void OnAwake() { }
    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }

    public abstract UsageType UsePrimary();
    public abstract UsageType UseSecondary();

    public virtual InteractionType Interact(Pawn interactor) {
        CrewMember cm = interactor as CrewMember;
        if (cm != null) {
            bool added = cm.AddToInventory(this);
            return InteractionType.PICK_UP;
        }

        return InteractionType.NONE;
    }
}