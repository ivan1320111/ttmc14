using Content.Shared._RMC14.Weapons.Ranged.IFF;
using Content.Shared.Roles;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Rules;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCDistressSignalRuleComponent : Component, IXenoMapRuleComponent
{
    [DataField]
    public EntProtoId<IFFFactionComponent> MarineFaction = "FactionMarine";

    [DataField]
    public ProtoId<JobPrototype> QueenJob = "MCXenoQueen";

    [DataField]
    public ProtoId<JobPrototype> ShrikeJob = "MCXenoShrike";

    [DataField]
    public EntProtoId QueenEnt = "MCXenoQueen";

    [DataField]
    public EntProtoId ShrikeEnt = "MCXenoShrike";

    [DataField]
    public ProtoId<JobPrototype> XenoSelectableJob = "MCXenoSelectableXeno";

    [DataField]
    public EntProtoId LarvaEnt = "MCXenoLarva";

    [DataField]
    public ResPath Thunderdome = new("/Maps/_RMC14/thunderdome.yml");

    [DataField]
    public EntityUid? XenoMap { get; set; }

    // Marine

    [DataField]
    public TimeSpan MarineRespawnTime = TimeSpan.FromMinutes(15);

    // Xenos

    [DataField]
    public List<EntProtoId> XenoRestrictedCastes = new();

    [DataField]
    public TimeSpan XenoRespawnTime = TimeSpan.FromMinutes(3);

    [DataField]
    public TimeSpan XenoSwapTimer = TimeSpan.FromMinutes(5);
}
