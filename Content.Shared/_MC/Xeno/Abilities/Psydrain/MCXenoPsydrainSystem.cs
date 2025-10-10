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
using Content.Shared.Mobs;
using Robust.Shared.Audio.Systems;

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

    private void OnAction(Entity<MCXenoPsydrainComponent> entity, ref MCXenoPsydrainActionEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;

<<<<<<< Updated upstream
<<<<<<< Updated upstream
        if (args.Handled)
=======
        if (!_rmcActions.TryUseAction(entity, args.Action, entity))
        {
            args.Handled = true;
>>>>>>> Stashed changes
=======
        if (!_rmcActions.TryUseAction(entity, args.Action, entity))
        {
            args.Handled = true;
            return;
        }

        if (!_xenoPlasma.HasPlasmaPopup(entity.Owner, entity.Comp.PlasmaNeed))
        {
            args.Handled = true;
            return;
        }

        if (!TryComp<MobStateComponent>(target, out var mobState))
        {
            args.Handled = true;
            return;
        }

        if (!_xenoHive.HasHive(entity.Owner))
        {
            _popup.PopupEntity(Loc.GetString("psydrain-dont-have-hive"), entity, entity, PopupType.MediumXeno);
>>>>>>> Stashed changes
            return;
        }

<<<<<<< Updated upstream
        if (!_psydrainableQuery.TryComp(target, out var psydrainableComponent))
        {
<<<<<<< Updated upstream
            _popup.PopupClient(Loc.GetString("psydrain-not-human"), entity, entity, PopupType.MediumXeno);
=======
        if (!_xenoPlasma.HasPlasmaPopup(entity.Owner, entity.Comp.PlasmaNeed))
        {
            args.Handled = true;
            return;
        }

        if (!TryComp<MobStateComponent>(target, out var mobState))
        {
            args.Handled = true;
            return;
        }

        if (!_xenoHive.HasHive(entity.Owner))
        {
            _popup.PopupEntity(Loc.GetString("psydrain-dont-have-hive"), entity, entity, PopupType.MediumXeno);
>>>>>>> Stashed changes
            return;
        }

        if (!psydrainableComponent.Available)
        {
<<<<<<< Updated upstream
            _popup.PopupClient(Loc.GetString("someone-already-psydrained"), entity, entity, PopupType.MediumXeno);
=======
=======
>>>>>>> Stashed changes
            _popup.PopupEntity(Loc.GetString("psydrain-not-human"), entity, entity, PopupType.MediumXeno);
            return;
        }

        if (mobState.PsyDrained)
        {
            _popup.PopupEntity(Loc.GetString("someone-already-psydrained"), entity, entity, PopupType.MediumXeno);
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
            return;
        }

        if (!_mobState.IsDead(target))
        {
<<<<<<< Updated upstream
<<<<<<< Updated upstream
            var notDead = Loc.GetString("psydrain-not-dead");
            _popup.PopupClient(notDead, entity, entity, PopupType.MediumXeno);
=======
            _popup.PopupEntity(Loc.GetString("psydrain-not-dead"), entity, entity, PopupType.MediumXeno);
>>>>>>> Stashed changes
=======
            _popup.PopupEntity(Loc.GetString("psydrain-not-dead"), entity, entity, PopupType.MediumXeno);
>>>>>>> Stashed changes
            return;
        }

        if (_flammable.IsOnFire(entity.Owner))
        {
<<<<<<< Updated upstream
<<<<<<< Updated upstream
            _popup.PopupClient(Loc.GetString("psydrain-our-fire"), entity, entity, PopupType.MediumXeno);
            return;
        }

        if (!_xenoHive.HasHive(entity.Owner))
        {
            _popup.PopupClient(Loc.GetString("psydrain-dont-have-hive"), entity, entity, PopupType.MediumXeno);
            return;
        }

        if (!_rmcActions.CanUseActionPopup(entity, args.Action, entity))
            return;

        args.Handled = true;

        _popup.PopupClient(Loc.GetString("being-psydrained", ("entity", entity), ("target", target)), entity, entity, PopupType.MediumXeno);
        _audio.PlayPredicted(entity.Comp.SoundDrain, entity, entity);

=======
            _popup.PopupEntity(Loc.GetString("psydrain-our-fire"), entity, entity, PopupType.MediumXeno);
            return;
        }

>>>>>>> Stashed changes
=======
            _popup.PopupEntity(Loc.GetString("psydrain-our-fire"), entity, entity, PopupType.MediumXeno);
            return;
        }

>>>>>>> Stashed changes
        var ev = new MCXenoPsydrainDoAfterEvent();
        var doAfter = new DoAfterArgs(EntityManager, entity, entity.Comp.Delay, ev, entity, target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            CancelDuplicate = true,
            DuplicateCondition = DuplicateConditions.SameEvent,
            RequireCanInteract = true,
            BreakOnRest = true,
        };

<<<<<<< Updated upstream
<<<<<<< Updated upstream
        if (_doAfter.TryStartDoAfter(doAfter))
            return;

        _popup.PopupClient(Loc.GetString("doAfter-canceled-owner"), entity, entity, PopupType.MediumXeno);

        // No, it doesn't work that way.
        // You need to save the result _audio.PlayPvs(entity.Comp.SoundDrain, entity);
        // and stop it, but I'm too lazy to do that. TODO
        // _audio.Stop(entity);
    }

    public override void Initialize()
    {
        base.Initialize();

        _psydrainableQuery = GetEntityQuery<MCXenoPsydrainableComponent>();

        SubscribeLocalEvent<MCXenoPsydrainComponent, MCXenoPsydrainActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoPsydrainComponent, MCXenoPsydrainDoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<MCXenoPsydrainableComponent, MobStateChangedEvent>(OnPsydrainableStateChanged);
=======
=======
>>>>>>> Stashed changes
        _popup.PopupEntity(Loc.GetString("being-psydrained", ("entity", entity), ("target", target)),
            entity,
            entity,
            PopupType.MediumXeno);
        _audio.PlayPvs(entity.Comp.SoundDrain, entity);

        if (_doAfter.TryStartDoAfter(doAfter))
            args.Handled = true;
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
    }


    private void OnDoAfter(Entity<MCXenoPsydrainComponent> entity, ref MCXenoPsydrainDoAfterEvent args)
    {
<<<<<<< Updated upstream
        if (args.Handled)
=======
        if (args.Target is not { } target)
            return;

        if (args.Cancelled || args.Handled)
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
            return;

        if (args.Cancelled)
        {
            _popup.PopupClient(Loc.GetString("doAfter-canceled-owner"), entity, entity, PopupType.MediumXeno);
            return;
        }

        if (args.Target is not { } target)
            return;

        if (!_psydrainableQuery.TryComp(target, out var psydrainableComponent))
            return;

        if (!psydrainableComponent.Available)
        {
            _popup.PopupClient(Loc.GetString("someone-already-psydrained"), entity, entity, PopupType.MediumXeno);
            return;
        }

        psydrainableComponent.Available = false;
        Dirty(target, psydrainableComponent);

        args.Handled = true;

        // Effects
        _audio.PlayPredicted(entity.Comp.SoundDrainEnd, entity, entity);
        _popup.PopupClient(Loc.GetString("end-drain-owner", ("target", target)), entity, entity, PopupType.MediumXeno);
        _jittering.DoJitter(entity, entity.Comp.JitteringDelayOwner, true, entity.Comp.AmplitudeOwner, entity.Comp.FrequencyOwner);
        _jittering.DoJitter(target, entity.Comp.JitteringDelayTarget, true, entity.Comp.AmplitudeTarget, entity.Comp.FrequencyTarget);

<<<<<<< Updated upstream
        // Damage
        _damageable.TryChangeDamage(target, entity.Comp.Damage);
=======
        _damageable.TryChangeDamage(target, entity.Comp.CloneDamage);
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes

        // Biomass
        _mcXenoBiomass.Add(target, entity.Comp.BiomassGain);

        // Plasma
        _xenoPlasma.TryRemovePlasma(entity.Owner, entity.Comp.PlasmaNeed);
<<<<<<< Updated upstream

        // Hive reward
        _xenoHive.AddLarvaPointsOwner(entity, entity.Comp.LarvaPointsGain);

        const int rewardMin = 30;
        const int rewardMax = 90;

        var psypointReward = int.Clamp(rewardMin + (MCStatusSystem.HighPlayerPop - _mcStatus.ActivePlayerCount) / MCStatusSystem.HighPlayerPop * (rewardMax - rewardMin), rewardMin, rewardMax);
        _xenoHive.AddPsypointsFromOwner(entity, "Strategic", psypointReward);
        _xenoHive.AddPsypointsFromOwner(entity, "Tactical", psypointReward / 4);
=======
        _xenoHive.AddPsypointsFromOwner(entity, entity.Comp.PsypointType, entity.Comp.PsypointGain);
        _mcXenoBiomassSystem.AddBiomassValue(biomassEntity, entity.Comp.BiomassGain);
        mobState.PsyDrained = true;
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes

        _adminLogger.Add(LogType.Action,
            LogImpact.Medium,
            $"{ToPrettyString(entity.Owner):player} successfully used Psy Drain on {ToPrettyString(target):target} " +
            $"at {Transform(target).Coordinates:coordinates}. " +
            $"Larva points gained: {entity.Comp.LarvaPointsGain}, " +
            $"Psy points gained: {entity.Comp.PsypointGain}, " +
<<<<<<< Updated upstream
<<<<<<< Updated upstream
            $"Damage applied: {entity.Comp.Damage}");
    }

    private void OnPsydrainableStateChanged(Entity<MCXenoPsydrainableComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.OldMobState != MobState.Dead || args.NewMobState == MobState.Dead)
            return;

        ent.Comp.Available = true;
        Dirty(ent);
=======
=======
>>>>>>> Stashed changes
            $"Biomass points gained: {entity.Comp.BiomassGain}, " +
            $"Damage applied: {entity.Comp.CloneDamage}");
>>>>>>> Stashed changes
    }
}
