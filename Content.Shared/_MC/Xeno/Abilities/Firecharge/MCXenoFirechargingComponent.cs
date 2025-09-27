using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Firecharge;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoFirechargingComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan End;

    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new()
        {
            { "Heat", 10.0f }
        }
    };

    [DataField, AutoNetworkedField]
    public DamageSpecifier DamagePerStack = new()
    {
        DamageDict = new()
        {
            { "Heat", 5.0f }
        }
    };
}
