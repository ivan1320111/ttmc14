using Content.Shared._MC.Stamina;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Physics.Components;

namespace Content.Server._MC.Stamina;

public sealed class MCStaminaActiveSystem : EntitySystem
{
    [Dependency] private readonly MCStaminaSystem _stamina = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speed = default!;
    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCStaminaActiveComponent, RefreshMovementSpeedModifiersEvent>(OnRefresh);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

    var query = EntityQueryEnumerator<MCStaminaComponent, MCStaminaActiveComponent, InputMoverComponent>();
    while (query.MoveNext(out var uid, out var stamina, out var active, out var input))
    {
        if (!TryComp<PhysicsComponent>(uid, out var phys))
            continue;

        if (stamina.Current <= 0 && !active.ZeroSprintLock)
        {
            active.ZeroSprintLock = true;
            _speed.RefreshMovementSpeedModifiers(uid);
            if (TryComp<InputMoverComponent>(uid, out var mover))
            {
                var moverController = EntitySystem.Get<Content.Shared.Movement.Systems.SharedMoverController>();
                moverController.SetSprinting((uid, mover), 0, true);
            }
        }

        if (active.ZeroSprintLock && stamina.Current >= 50)
        {
            if (input.Sprinting)
            {
                if (TryComp<InputMoverComponent>(uid, out var mover))
                {
                    var moverController = EntitySystem.Get<Content.Shared.Movement.Systems.SharedMoverController>();
                    moverController.SetSprinting((uid, mover), 0, true);
                }
            }
            else
            {
                active.ZeroSprintLock = false;
                _speed.RefreshMovementSpeedModifiers(uid);
            }
        }

        if (active.ZeroSprintLock)
        {
            if (input.Sprinting && TryComp<InputMoverComponent>(uid, out var mover))
            {
                var moverController = EntitySystem.Get<Content.Shared.Movement.Systems.SharedMoverController>();
                moverController.SetSprinting((uid, mover), 0, true);
            }
            continue;
        }

        if (input.Sprinting && !active.Slowed && phys.LinearVelocity.Length() > 0.1f && stamina.Current > 0)
        {
            _stamina.DoStaminaDamage((uid, stamina), active.RunStaminaDamage, false);
        }

        if (stamina.Current >= active.SlowThreshold && !active.Slowed)
        {
            active.Slowed = true;
            active.Change = true;
            _speed.RefreshMovementSpeedModifiers(uid);
            continue;
        }

        if (stamina.Current <= active.ReviveStaminaLevel && active.Slowed)
        {
            active.Slowed = false;
            active.Change = true;
            _speed.RefreshMovementSpeedModifiers(uid);
            continue;
        }
    }
    }

    private void OnRefresh(EntityUid uid, MCStaminaActiveComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (component.ZeroSprintLock)
        {
            args.ModifySpeed(args.WalkSpeedModifier, args.WalkSpeedModifier);
            return;
        }
        if (!component.Change)
            return;
        args.ModifySpeed(args.WalkSpeedModifier, args.SprintSpeedModifier);
    }
}
