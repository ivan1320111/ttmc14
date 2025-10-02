using Content.Shared._MC.FTL.Events;
using Content.Shared._MC.Operation;

namespace Content.Shared._MC.Shuttle;

public abstract class MCShuttleSystem : EntitySystem
{
    [Dependency] private readonly MCOperationSystem _mcOperation = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCShuttleComponent, MCFTLEndEvent>(OnFTLEnd);
    }

    private void OnFTLEnd(Entity<MCShuttleComponent> ent, ref MCFTLEndEvent args)
    {
        _mcOperation.Start();
    }
}
