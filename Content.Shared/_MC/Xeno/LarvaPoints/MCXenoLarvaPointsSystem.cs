using Content.Shared._RMC14.Xenonids.Hive;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.LarvaPoints;

public sealed class MCXenoLarvaPointsSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedXenoHiveSystem _hive = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var mapQuery = EntityQueryEnumerator<HiveComponent>();
        while (mapQuery.MoveNext(out var entity, out var cycle))
        {
            if (!cycle.Running)
                continue;

            if (cycle.LarvaPoints == cycle.NeedLarvaPointsForBurrowedLarva)
            {
                _hive.AddBurrowedLarvaCount(new Entity<HiveComponent>(entity, cycle), 1);
                _hive.SetLarvaPoints(new Entity<HiveComponent>(entity, cycle), 0);
            }
        }
    }
}
