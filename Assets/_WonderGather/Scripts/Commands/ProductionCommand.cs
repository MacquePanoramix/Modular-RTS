namespace WonderGather
{
    public readonly struct ProductionCommand
    {
        private readonly bool cancelLast;
        public ProductionCommand(bool cancelLast=false)=>this.cancelLast=cancelLast;
        public bool Execute(UnitProducer producer)=>producer!=null && (cancelLast?producer.CancelLast():producer.Enqueue());
    }
}
