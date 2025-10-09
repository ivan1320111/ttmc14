using Content.Shared._MC.Xeno.Biomass;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Atmos;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Jittering;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Psydrain;

public sealed class MCXenoPsydrainSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedRMCFlammableSystem _flammable = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedXenoHiveSystem _xenoHive = default!;
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;

    [Dependency] private readonly MCStatusSystem _mcStatus = default!;
    [Dependency] private readonly MCXenoBiomassSystem _mcXenoBiomass = default!;

    private EntityQuery<MCXenoPsydrainableComponent> _psydrainableQuery;

    public override void Initialize()
    {
        base.Initialize();

        _psydrainableQuery = GetEntityQuery<MCXenoPsydrainableComponent>();

        SubscribeLocalEvent<MCXenoPsydrainComponent, MCXenoPsydrainActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoPsydrainComponent, MCXenoPsydrainDoAfterEvent>(OnDoAfter);
    }

    private void OnAction(Entity<MCXenoPsydrainComponent> entity, ref MCXenoPsydrainActionEvent args)
    {
        var target = args.Target;

        if (args.Handled)
            return;

        if (!_psydrainableQuery.TryComp(target, out var psydrainableComponent))
        {
            _popup.PopupEntity(Loc.GetString("psydrain-not-human"), entity, entity, PopupType.MediumXeno);
            return;
        }

        if (!psydrainableComponent.Available)
        {
            _popup.PopupEntity(Loc.GetString("someone-already-psydrained"), entity, entity, PopupType.MediumXeno);
            return;
        }

        if (!_mobState.IsDead(target))
        {
            var notDead = Loc.GetString("psydrain-not-dead");
            _popup.PopupEntity(notDead, entity, entity, PopupType.MediumXeno);
            return;
        }

        if (_flammable.IsOnFire(entity.Owner))
        {
            _popup.PopupEntity(Loc.GetString("psydrain-our-fire"), entity, entity, PopupType.MediumXeno);
            return;
        }

        if (!_xenoHive.HasHive(entity.Owner))
        {
            _popup.PopupEntity(Loc.GetString("psydrain-dont-have-hive"), entity, entity, PopupType.MediumXeno);
            return;
        }

        if (!_rmcActions.CanUseActionPopup(entity, args.Action, entity))
            return;

        args.Handled = true;

        var ev = new MCXenoPsydrainDoAfterEvent();
        var doAfter = new DoAfterArgs(EntityManager, entity, entity.Comp.Delay, ev, entity, target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            BlockDuplicate = true,
            CancelDuplicate = true,
            RequireCanInteract = true,
            BreakOnRest = true,
        };

        _popup.PopupEntity(Loc.GetString("being-psydrained", ("entity", entity), ("target", target)), entity, entity, PopupType.MediumXeno);
        _audio.PlayPvs(entity.Comp.SoundDrain, entity);

        if (_doAfter.TryStartDoAfter(doAfter))
            return;

        _popup.PopupEntity(Loc.GetString("doAfter-canceled-owner"), entity, entity, PopupType.MediumXeno);
        _audio.Stop(entity);
    }

    private void OnDoAfter(Entity<MCXenoPsydrainComponent> entity, ref MCXenoPsydrainDoAfterEvent args)
    {
        if (args.Handled)
            return;

        if (args.Cancelled)
        {
            _popup.PopupEntity(Loc.GetString("doAfter-canceled-owner"), entity, entity, PopupType.MediumXeno);
            return;
        }

        if (args.Target is not { } target)
            return;

        if (!_psydrainableQuery.TryComp(target, out var psydrainableComponent))
            return;

        if (!psydrainableComponent.Available)
        {
            _popup.PopupEntity(Loc.GetString("someone-already-psydrained"), entity, entity, PopupType.MediumXeno);
            return;
        }

        args.Handled = true;

        _audio.PlayLocal(entity.Comp.SoundDrainEnd, entity, entity);

        _popup.PopupEntity(Loc.GetString("end-drain-owner", ("target", target)), entity, entity, PopupType.MediumXeno);
        _jittering.DoJitter(entity, entity.Comp.JitteringDelayOwner, true, entity.Comp.AmplitudeOwner, entity.Comp.FrequencyOwner);
        _jittering.DoJitter(target, entity.Comp.JitteringDelayTarget, true, entity.Comp.AmplitudeTarget, entity.Comp.FrequencyTarget);
        _damageable.TryChangeDamage(target, entity.Comp.CloneDamage);

        psydrainableComponent.Available = false;
        Dirty(target, psydrainableComponent);


        var biomassEntity = (target, biomass);
        _mcXenoBiomass.AddBiomassValue(biomassEntity, entity.Comp.BiomassGain);
        _xenoPlasma.TryRemovePlasma(entity.Owner, entity.Comp.PlasmaNeed);

        // Hive reward
        _xenoHive.AddLarvaPointsOwner(entity, entity.Comp.LarvaPointsGain);

        const int rewardMin = 30;
        const int rewardMax = 90;

        var psypointReward = int.Clamp(rewardMin + (MCStatusSystem.HighPlayerPop - _mcStatus.ActivePlayerCount) / MCStatusSystem.HighPlayerPop * (rewardMax - rewardMin), rewardMin, rewardMax);
        _xenoHive.AddPsypointsFromOwner(entity, "Strategic", psypointReward);
        _xenoHive.AddPsypointsFromOwner(entity, "Tactical", psypointReward / 4);

        _adminLogger.Add(LogType.Action,
            LogImpact.Medium,
            $"{ToPrettyString(entity.Owner):player} successfully used Psy Drain on {ToPrettyString(target):target} " +
            $"at {Transform(target).Coordinates:coordinates}. " +
            $"Larva points gained: {entity.Comp.LarvaPointsGain}, " +
            $"Psy points gained: {entity.Comp.PsypointGain}, " +
            $"Damage applied: {entity.Comp.CloneDamage}");
    }
}
