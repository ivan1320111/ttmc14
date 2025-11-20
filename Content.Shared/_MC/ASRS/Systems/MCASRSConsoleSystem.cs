using System.Linq;
using Content.Shared._MC.ASRS.Components;
using Content.Shared._MC.ASRS.Ui;
using Content.Shared._RMC14.Marines.Roles.Ranks;

namespace Content.Shared._MC.ASRS.Systems;

public sealed class MCASRSConsoleSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = null!;

    [Dependency] private readonly SharedRankSystem _rmcRank = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCASRSConsoleComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MCASRSConsoleComponent, MCASRSSendRequestMessage>(OnRequestMessage);
    }

    private void OnInit(Entity<MCASRSConsoleComponent> entity, ref ComponentInit args)
    {
        entity.Comp.CachedEntries.Clear();
        foreach (var category in entity.Comp.Categories)
        {
            entity.Comp.CachedEntries.AddRange(category.Entries);
        }
    }

    private void OnRequestMessage(Entity<MCASRSConsoleComponent> entity, ref MCASRSSendRequestMessage args)
    {
        if (!ValidateRequestMessage(entity, args))
            return;

        var name = GetRequesterName(args.Actor);
        var request = new MCASRSRequest
        {
            Requester = name,
            Reason = args.Reason,
            Contents = args.Contents,
        };

        entity.Comp.Requests.Add(request);
        Dirty(entity);

        _userInterface.SetUiState(entity.Owner, MCASRSConsoleUi.Key, new MCASRSConsoleBuiState(entity.Comp.Requests));
    }

    private string GetRequesterName(EntityUid userUid)
    {
        return _rmcRank.GetSpeakerFullRankName(userUid) ?? Name(userUid);
    }

    private static bool ValidateRequestMessage(Entity<MCASRSConsoleComponent> entity, MCASRSSendRequestMessage args)
    {
        return args.Reason != string.Empty
               && args.Contents.Keys.All(entry => entity.Comp.CachedEntries.Contains(entry));
    }
}
