using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Sentries.Events;

[Serializable, NetSerializable]
public sealed partial class MCSentryDisassembleDoAfterEvent : SimpleDoAfterEvent;
