using Orleans;
using System.Threading.Tasks;

namespace IoTSystem.Grains
{
    public class SensoreState
    {
        public int DbNum { get; set; }
        public string CollettoreKey { get; set; }
        public string Tipo { get; set; }
    }

    internal interface ISensoreGrain : IGrainWithStringKey
    {
        Task SetTipo(string tipo);

        Task SetDbNum(int dbNum);
        Task SetCollettoreKey(string collettoreKey);
        Task<int> GetDbNum();
        Task<string> GetCollettoreKey();
        Task<string> GetTipo();

        Task ReadValueFromSensor();
        Task SaveValueToDB(string value);

        Task SendValueAverage();

    }
}
