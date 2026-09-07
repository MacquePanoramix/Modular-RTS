using System.Collections.Generic;
namespace WonderGather
{
    public sealed class ReturnSuppliesCommand
    {
        private readonly ResourceDepot depot;
        public ReturnSuppliesCommand(ResourceDepot depot) => this.depot = depot;
        public int Execute(IReadOnlyList<SelectableUnit> units)
        {
            int accepted = 0;
            foreach (var unit in units)
                if (unit != null && unit.TryGetComponent<Gatherer>(out var worker) && worker.ReturnToDepot(depot)) accepted++;
            return accepted;
        }
    }
    public sealed class GatherCommand
    {
        private readonly ResourceNode resource;
        public GatherCommand(ResourceNode resource) => this.resource = resource;
        public int Execute(IReadOnlyList<SelectableUnit> units)
        {
            int accepted = 0;
            foreach (var unit in units)
                if (unit != null && unit.TryGetComponent<Gatherer>(out var worker) && worker.Gather(resource)) accepted++;
            return accepted;
        }
    }
}
