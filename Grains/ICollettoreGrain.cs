using Orleans;
using System.Threading.Tasks;

namespace IoTSystem.Grains
{
    public class CollettoreState
    {
        public int DbNum { get; set; }
        public string AttuatoreLuce { get; set; }
        public string AttuatoreTemp { get; set; }
        public string AttuatoreUmi { get; set; }
        public double SogliaLuce { get; set; }
        public double SogliaTemp { get; set; }
        public string ModoTemp { get; set; }
        public double SogliaUmi { get; set; }


    }
    internal interface ICollettoreGrain : IGrainWithStringKey
    {
        Task SetDbNum(int dbNum);
        Task<int> GetDbNum();   
        Task SetAttuatoreLuce(string attuatoreLuce);
        Task<string> GetAttuatoreLuce();
        Task SetAttuatoreTemp(string attuatoreTemp);
        Task<string> GetAttuatoreTemp();
        Task SetAttuatoreUmi(string attuatoreUmi);
        Task<string> GetAttuatoreUmi();
        Task SetSogliaLuce(double sogliaLuce);
        Task<double> GetSogliaLuce();
        Task SetSogliaTemp(double sogliaTemp);
        Task<double> GetSogliaTemp();
        Task SetModoTemp(string modoTemp);
        Task<string> GetModoTemp();
        Task SetSogliaUmi(double sogliaUmi);
        Task<double> GetSogliaUmi();



        Task ReceiveAverageFromSensor(string tipo, double valore);

        // Meta Average nel senso che è una media delle medie, quindi una meta-media :D
        Task ComputeMetaAverage(string tipo);

        Task GetStatoAttuatori();
    }
}
