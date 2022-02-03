/// <summary>Used to give objects usage behaviours.</summary>
public interface IUsable {
    UsageType UsePrimary();
    UsageType UseSecondary();
}