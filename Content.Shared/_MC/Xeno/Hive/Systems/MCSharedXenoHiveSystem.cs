using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Mobs.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Hive.Systems;

public abstract partial class MCSharedXenoHiveSystem : MCEntitySystemSingleton<MCXenoHiveSingletonComponent>
{
    [Dependency] private readonly MobStateSystem _mobState = null!;

    [ViewVariables]
    public EntityUid? DefaultHive => Inst.Comp.DefaultHive;

    private EntityQuery<HiveComponent> _hiveQuery;
    private EntityQuery<HiveMemberComponent> _hiveMemberQuery;

    public override void Initialize()
    {
        base.Initialize();

        _hiveQuery = GetEntityQuery<HiveComponent>();
        _hiveMemberQuery = GetEntityQuery<HiveMemberComponent>();

        InitializeRuler();
    }


    public void SetCanEvolveWithoutLeader(Entity<HiveComponent?> entity, bool value)
    {
        if (!Resolve(entity, ref entity.Comp))
            return;

        entity.Comp.CanEvolveWithoutRuler = value;
        Dirty(entity);
    }

    public void SetCanCollapse(Entity<HiveComponent?> entity, bool value)
    {
        if (!Resolve(entity, ref entity.Comp))
            return;

        entity.Comp.CanCollapse = value;
        Dirty(entity);
    }

    public Dictionary<int, int> GetTiers(EntityUid hive)
    {
        if (!_hiveQuery.TryComp(hive, out var component))
            return new Dictionary<int, int>();

        var result = new Dictionary<int, int>();
        var query = EntityQueryEnumerator<XenoComponent, HiveMemberComponent>();
        while (query.MoveNext(out var uid, out var xenoComponent, out _))
        {
            if (_mobState.IsDead(uid))
                continue;

            if (!result.TryAdd(xenoComponent.Tier, 0))
                result[xenoComponent.Tier]++;
        }

        return result;
    }

    public int GetLiving(EntityUid hive, int minTier = 1)
    {
        var total = 0;
        var query = EntityQueryEnumerator<XenoComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_mobState.IsDead(uid))
                continue;

            if (_hiveMemberQuery.TryComp(uid, out var hiveMemberComponent) && hiveMemberComponent.Hive != hive)
                continue;

            if (comp.Tier < minTier)
                continue;

            total++;
        }

        return total;
    }
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoHiveSingletonComponent : Component
{
    #region Default hive

    [DataField, AutoNetworkedField]
    public string DefaultHiveName = "xeno hive";

    [DataField, AutoNetworkedField]
    public EntProtoId DefaultHiveId = "MCXenoHive";

    [DataField, AutoNetworkedField]
    public EntityUid? DefaultHive;

    #endregion
}
