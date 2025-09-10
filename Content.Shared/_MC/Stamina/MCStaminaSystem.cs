using Content.Shared._RMC14.Movement;
using Content.Shared._RMC14.Stun;
using Content.Shared.Administration.Logs;
using Content.Shared.Alert;
using Content.Shared.Database;
using Content.Shared.Damage;
using Content.Shared.Movement.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Projectiles;
using Content.Shared.Rejuvenate;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.StatusEffect;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Wieldable.Components;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Shared._MC.Stamina;

public sealed partial class MCStaminaSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedStutteringSystem _stutter = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly RMCSizeStunSystem _sizeStun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCStaminaComponent, ComponentStartup>(OnStaminaStartup);
        SubscribeLocalEvent<MCStaminaComponent, RejuvenateEvent>(OnStaminaRejuvenate);

        SubscribeLocalEvent<MCStaminaDamageOnHitComponent, MeleeHitEvent>(OnStaminaOnHit);

        SubscribeLocalEvent<MCStaminaDamageOnCollideComponent, ProjectileHitEvent>(OnStaminaOnProjectileHit);
        SubscribeLocalEvent<MCStaminaDamageOnCollideComponent, ThrowDoHitEvent>(OnStaminaOnThrowHit);
    }

    private void OnStaminaStartup(Entity<MCStaminaComponent> ent, ref ComponentStartup args)
    {
        SetStaminaAlert(ent);
    }

    private void OnStaminaRejuvenate(Entity<MCStaminaComponent> ent, ref RejuvenateEvent args)
    {
        DoStaminaDamage((ent, ent.Comp), -ent.Comp.Max, false);
    }

    public override void Update(float frameTime)
    {
        if (_net.IsClient)
            return;

        var time = _timing.CurTime;

        var query = EntityQueryEnumerator<MCStaminaComponent>();

        while (query.MoveNext(out var uid, out var stamina))
        {
            if (stamina.Current == stamina.Max)
                continue;

            if (TryComp<Content.Shared._MC.Stamina.MCStaminaActiveComponent>(uid, out var active))
            {
                if (active.ZeroSprintLock && TryComp<InputMoverComponent>(uid, out var input) && input.Sprinting)
                {
                    continue;
                }
            }

            if (time >= stamina.NextRegen)
                DoStaminaDamage((uid, stamina), -stamina.RegenPerTick);

            if (stamina.Current <= -25)
            {
                if (TryComp<DamageableComponent>(uid, out var dmg))
                {
                    var dmgSys = EntitySystem.Get<DamageableSystem>();
                    var spec = new DamageSpecifier();
                    spec.DamageDict["Asphyxiation"] = 2.5f * frameTime;
                    dmgSys.TryChangeDamage(uid, spec, true, true, dmg);
                }
            }
        }
    }

    public void DoStaminaDamage(Entity<MCStaminaComponent?> ent, double amount, bool visual = true, bool updateRegenTimer = true)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        ent.Comp.Current = Math.Clamp(ent.Comp.Current - amount, -40, ent.Comp.Max);

        if (ent.Comp.Current <= -10)
        {
            _sizeStun.TryKnockOut(ent, TimeSpan.FromSeconds(5), true);
        }
        if (updateRegenTimer)
            ent.Comp.NextRegen = _timing.CurTime + (amount > 0 ? ent.Comp.RestPeriod : ent.Comp.TimeBetweenChecks);
        SetStaminaAlert((ent, ent.Comp));
    }

    private void OnStaminaOnHit(Entity<MCStaminaDamageOnHitComponent> ent, ref MeleeHitEvent args)
    {
        if (ent.Comp.RequiresWield && TryComp<WieldableComponent>(ent.Owner, out var wieldable) && !wieldable.Wielded)
            return;

        if (!args.IsHit ||
            !args.HitEntities.Any() ||
            ent.Comp.Damage <= 0f)
        {
            return;
        }

        var stamQuery = GetEntityQuery<MCStaminaComponent>();
        var toHit = new List<(EntityUid Entity, MCStaminaComponent Component)>();

        foreach (var hit in args.HitEntities)
        {
            if (!stamQuery.TryGetComponent(hit, out var stam))
                continue;

            toHit.Add((hit, stam));
        }

        var damage = ent.Comp.Damage;

        foreach (var (hit, comp) in toHit)
        {
            DoStaminaDamage(hit, damage / toHit.Count, true);
        }
    }

    private void OnStaminaOnProjectileHit(Entity<MCStaminaDamageOnCollideComponent> ent, ref ProjectileHitEvent args)
    {
        OnCollide(ent, args.Target);
    }

    private void OnStaminaOnThrowHit(Entity<MCStaminaDamageOnCollideComponent> ent, ref ThrowDoHitEvent args)
    {
        OnCollide(ent, args.Target);
    }

    private void OnCollide(Entity<MCStaminaDamageOnCollideComponent> ent, EntityUid target)
    {
        if (!TryComp<MCStaminaComponent>(target, out var stam))
            return;

        DoStaminaDamage((target, stam), ent.Comp.Damage, true);
    }

    private void SetStaminaAlert(Entity<MCStaminaComponent> ent)
    {
        var level = 0;
        var thresholds = ent.Comp.TierThresholds;
        if (thresholds != null && thresholds.Length > 0)
        {
            for (var i = 0; i < thresholds.Length; i++)
            {
                if (ent.Comp.Current <= thresholds[i])
                    level = i;
            }
            _alerts.ShowAlert(ent, ent.Comp.StaminaAlert, (short)((thresholds.Length - 1) - level));
        }
        else
        {
            _alerts.ShowAlert(ent, ent.Comp.StaminaAlert, 0);
        }
    }
}
