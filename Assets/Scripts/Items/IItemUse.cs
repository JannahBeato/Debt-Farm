using Unity.VisualScripting.FullSerializer;

public interface IItemUse
{
    int EnergyCost { get; }
    bool ConsumesItem { get; } // seeds = true, tools = false

    // Return true ONLY if the action actually happened
    bool TryUse(UseContext context);
}