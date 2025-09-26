using Robust.Shared.GameStates;

namespace Content.Shared._MC.Stamina;
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCStaminaActiveComponent : Component
{
    [DataField]
    public bool ZeroSprintLock;

    /// <summary>
    /// Float on which our entity will be "stunned"
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SlowThreshold = 180f;

    /// <summary>
    /// Value to compare with StaminaDamage and set default sprint speed back.
    /// If Stamina damage will be less than this value - default sprint will be set.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ReviveStaminaLevel = 80f;

    /// <summary>
    /// Stamina damage to apply when entity is running
    /// </summary>
    [DataField, AutoNetworkedField]
    public float RunStaminaDamage = 0.25f;

    [AutoNetworkedField]
    public bool Change;

    /// <summary>
    /// If our entity is slowed already.
    /// Nothing will happen if you'll set it manually.
    /// </summary>
    [AutoNetworkedField]
    public bool Slowed = false;
}
