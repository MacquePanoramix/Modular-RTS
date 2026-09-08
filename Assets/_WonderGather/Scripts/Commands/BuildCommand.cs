namespace WonderGather
{
    public sealed class BuildCommand
    {
        private readonly BuildingSite site;
        public BuildCommand(BuildingSite site) => this.site=site;
        public bool Execute(Builder worker) => worker!=null && worker.Build(site);
    }
}
