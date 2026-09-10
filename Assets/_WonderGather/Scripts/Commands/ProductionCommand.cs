namespace WonderGather
{
    public readonly struct ProductionCommand
    {
        private readonly bool cancelLast;
        private readonly UnitBlueprint unit;
        public ProductionCommand(bool cancelLast=false,UnitBlueprint unit=null){this.cancelLast=cancelLast;this.unit=unit;}
        public bool Execute(UnitProducer producer)=>producer!=null && (cancelLast?producer.CancelLast():producer.Enqueue(unit));
    }
}
