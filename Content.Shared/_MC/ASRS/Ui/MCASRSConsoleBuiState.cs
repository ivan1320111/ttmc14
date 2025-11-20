using Robust.Shared.Serialization;

namespace Content.Shared._MC.ASRS.Ui;

[Serializable, NetSerializable]
public sealed class MCASRSConsoleBuiState : BoundUserInterfaceState
{
    public readonly List<MCASRSRequest> Requests;

    public MCASRSConsoleBuiState(List<MCASRSRequest> requests)
    {
        Requests = requests;
    }
}
