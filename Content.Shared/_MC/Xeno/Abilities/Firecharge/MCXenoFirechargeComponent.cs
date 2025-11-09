using Content.Shared.Chat.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Firecharge;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoFirechargeComponent : Component
{
    [DataField, AutoNetworkedField]
    public float MaxDistance = 3;

    [DataField, AutoNetworkedField]
    public int Strength = 45;

    [DataField, AutoNetworkedField]
    public ProtoId<EmotePrototype> Emote = "XenoRoar";
}
