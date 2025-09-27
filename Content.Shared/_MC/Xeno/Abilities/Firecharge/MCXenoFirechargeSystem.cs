using System.Numerics;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Pulling;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared._RMC14.Emote;
using Content.Shared.Damage;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Atmos.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Firecharge;

public sealed class MCXenoFirechargeSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly RMCPullingSystem _rmcPulling = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedXenoHiveSystem _xenoHive = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedRMCEmoteSystem _rmcEmote = default!;


    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();

        _physicsQuery = GetEntityQuery<PhysicsComponent>();

        SubscribeLocalEvent<MCXenoFirechargeComponent, MCXenoFirechargeActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoFirechargingComponent, StartCollideEvent>(OnHit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCXenoFirechargingComponent>();
        while (query.MoveNext(out var entityUid, out var chargeComponent))
        {
            if (_timing.CurTime < chargeComponent.End)
                continue;

            Stop(entityUid);
        }
    }

    private void OnAction(Entity<MCXenoFirechargeComponent> entity, ref MCXenoFirechargeActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_rmcActions.TryUseAction(entity, args.Action, entity))
            return;

        args.Handled = true;

        if (!_physicsQuery.TryGetComponent(entity, out var physicsComponent))
            return;

        if (EnsureComp<MCXenoFirechargingComponent>(entity, out var chargeComponent))
            return;

        var origin = _transform.GetMapCoordinates(entity);
        var target = _transform.ToMapCoordinates(args.Target);
        var direction = target.Position - origin.Position;

        if (direction == Vector2.Zero)
            return;

        var length = direction.Length();
        var distance = Math.Clamp(length, 0.1f, entity.Comp.MaxDistance);

        direction *= distance / length;

        var impulse = direction.Normalized() * entity.Comp.Strength * physicsComponent.Mass;

        _rmcPulling.TryStopAllPullsFromAndOn(entity);

        _physics.ApplyLinearImpulse(entity, impulse, body: physicsComponent);
        _physics.SetBodyStatus(entity, physicsComponent, BodyStatus.InAir);

        var duration = _timing.CurTime + TimeSpan.FromSeconds(direction.Length() / entity.Comp.Strength);

        chargeComponent.End = duration;
        _rmcEmote.TryEmoteWithChat(entity, entity.Comp.Emote);
        Dirty(entity, chargeComponent);
    }

    private void OnHit(Entity<MCXenoFirechargingComponent> entity, ref StartCollideEvent args)
    {
        if (_xenoHive.FromSameHive(entity.Owner, args.OtherEntity))
            return;

        if (!TryComp<MCXenoFirechargeComponent>(entity, out var chargeComponent))
            return;

        if (!HasComp<MobStateComponent>(args.OtherEntity) || _mobState.IsDead(args.OtherEntity) || HasComp<XenoComponent>(args.OtherEntity))
            return;

        _damageable.TryChangeDamage(args.OtherEntity, entity.Comp.Damage);

        if (TryComp<FlammableComponent>(args.OtherEntity, out var fireStacksComp))
        {
            float fireStacks = fireStacksComp.FireStacks;

            _damageable.TryChangeDamage(args.OtherEntity, entity.Comp.Damage + fireStacks * entity.Comp.DamagePerStack);

            fireStacksComp.FireStacks = 0;
            Dirty(args.OtherEntity, fireStacksComp);
        }

        Stop(entity);
    }

    private void Stop(EntityUid entityUid)
    {
        if (!_physicsQuery.TryGetComponent(entityUid, out var physics))
            return;

        _physics.SetLinearVelocity(entityUid, Vector2.Zero, body: physics);
        _physics.SetBodyStatus(entityUid, physics, BodyStatus.OnGround);

        RemCompDeferred<MCXenoFirechargingComponent>(entityUid);
    }
}
