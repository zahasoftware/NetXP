namespace NetXP.NetStandard.DependencyInjection
{
    public enum DILifeTime
    {
        Singleton,//Only One Instance
        Scoped,//Request life time
        Trasient//Always Unique
    }
}