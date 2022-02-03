/// <summary>Used to give entities the ability to use objects that implement IUsable interface.</summary>
public interface ICanUse {
    bool UsePrimary(IUsable usable);
    bool UseSecondary(IUsable usable);
}