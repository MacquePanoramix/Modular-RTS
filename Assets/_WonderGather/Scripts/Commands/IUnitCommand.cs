using UnityEngine;

namespace WonderGather
{
    public interface IUnitCommand { bool Execute(UnitMotor unit); }

    public readonly struct MoveCommand : IUnitCommand
    {
        public Vector3 Destination { get; }
        public MoveCommand(Vector3 destination) => Destination = destination;
        public bool Execute(UnitMotor unit)
        {
            if (unit == null || !ReachableDestination.Plan(unit, Destination, out var path, out var point) || !unit.ApplyMove(path, point)) return false;
            if (unit.TryGetComponent<Builder>(out var builder)) builder.CancelOrder();
            if (unit.TryGetComponent<Gatherer>(out var worker)) worker.CancelOrder();
            return true;
        }
    }

    // Future order sources (AI, replay, network) enter through this boundary.
    public sealed class CommandDispatcher
    {
        public bool Dispatch(IUnitCommand command, UnitMotor unit) => command != null && command.Execute(unit);
        public bool Dispatch(ProductionCommand command, UnitProducer producer)=>command.Execute(producer);
        public bool Dispatch(BuildCommand command, Builder worker) => command!=null && command.Execute(worker);
        public int Dispatch(ReturnSuppliesCommand command, System.Collections.Generic.IReadOnlyList<SelectableUnit> units)
            => command == null ? 0 : command.Execute(units);
        public int Dispatch(GatherCommand command, System.Collections.Generic.IReadOnlyList<SelectableUnit> units)
            => command == null ? 0 : command.Execute(units);
        public bool Dispatch(GroupMoveCommand command, System.Collections.Generic.IReadOnlyList<SelectableUnit> units)
            => command != null && command.Execute(units);
    }
}
