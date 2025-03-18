using Orleans;
using System.Threading.Tasks;

namespace IoTSystem.Grains
{
    internal interface IStanzaGrain : IGrainWithStringKey
    {
        Task InitSystem( string nr_lum, string nr_temp, string nr_umi);

        Task SettaSoglie(string tipo, double valore);

    }
}
