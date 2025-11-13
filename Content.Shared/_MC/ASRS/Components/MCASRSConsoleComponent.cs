using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.ASRS.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCASRSConsoleComponent : Component
{
    [DataField, AutoNetworkedField, AlwaysPushInheritance]
    public List<MCASRSCategory> Categories = new();
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class MCASRSCategory
{
    [DataField]
    public string Name = string.Empty;

    [DataField]
    public List<MCASRSEntry> Entries = new();
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class MCASRSEntry
{
    [DataField]
    public string? Name;

    [DataField]
    public int Cost;

    [DataField]
    public EntProtoId? Crate;

    [DataField]
    public List<EntProtoId> Entities = new();
}
