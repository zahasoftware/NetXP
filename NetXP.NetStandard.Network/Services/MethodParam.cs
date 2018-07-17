namespace NetXP.NetStandard.Network.Services
{
    public class MethodParam
    {
        public MethodParam()
        {

        }
        public MethodParam(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }
        public string Name { get; set; }
        public object Value { get; set; }

    }
}