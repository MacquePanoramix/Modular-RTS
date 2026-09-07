using UnityEngine;

namespace WonderGather
{
    public interface IUnitCommand { bool Execute(UnitMotor unit); }

    public readonly struct MoveCommand : IUnitCommand
    {
        public Vector3 Destination { get; }
        public MoveCommand(Vector3 destination) => Destination = destination;
        public bool Execute(UnitMotor unit) => unit != null && unit.TryMove(Destination);
    }

    // Future order sources (AI, replay, network) enter through this boundary.
    public sealed class CommandDispatcher
    {
        public bool Dispatch(IUnitCommand command, UnitMotor unit) => command != null && command.Execute(unit);
        public bool Dispatch(GroupMoveCommand command, System.Collections.Generic.IReadOnlyList<SelectableUnit> units)
            => command != null && command.Execute(units);
    }
}
