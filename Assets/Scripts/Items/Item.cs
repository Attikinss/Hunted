using UnityEngine;

public abstract class Item : MonoBehaviour, IUsable {
    
    public abstract UsageType UsePrimary();
    public abstract UsageType UseSecondary();
}