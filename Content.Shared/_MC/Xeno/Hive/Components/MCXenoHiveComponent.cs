using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Hive.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoHiveComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan RespawnTime = TimeSpan.FromMinutes(3);

    [DataField, AutoNetworkedField]
    public TimeSpan CasteSwapTime = TimeSpan.FromMinutes(5);

    [DataField, AutoNetworkedField]
    public List<EntProtoId> CaseEvolutionBlock = new()
    {
        // "MCXenoHiveMind",
        "MCXenoWraith",
    };

    [DataField, AutoNetworkedField]
    public Dictionary<EntProtoId, int> CasteEvolutionCountRequire = new()
    {
        { "MCXenoHivelord", 5 },
        { "MCXenoQueen", 10 },
        // { "MCXenoKing", 14 },
    };
}
