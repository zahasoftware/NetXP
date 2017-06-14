namespace NetXP.NetStandard.DependencyInjection
{
    public enum LifeTime
    {
        Singleton,//Only One Instance
        Scoped,//Request life time
        Trasient//Always Unique
    }
}