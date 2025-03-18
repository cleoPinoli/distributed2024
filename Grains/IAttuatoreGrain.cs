using System.Threading.Tasks;
using Orleans;

namespace IoTSystem.Grains
{
    public class AttuatoreState
    {
        public string Tipo { get; set; }
        public bool StatoLuce { get; set; }
        public bool StatoUmi { get; set; }
        public bool StatoTempCaldo { get; set; }
        public bool StatoTempFreddo { get; set; }
    }
    internal interface IAttuatoreGrain : IGrainWithStringKey
    {
        Task SetTipo(string tipo);
        Task SetStatoLuce(bool stato);
        Task SetStatoUmi(bool stato);
        Task SetStatoTempCaldo(bool stato);
        Task SetStatoTempFreddo(bool stato);

        Task ChangeStatus(string tipo, bool toStatus);
        Task ChangeStatusTermico(bool riscaldamentoToStatus, bool raffreddamentoToStatus);
        Task GetStatoAttuatore();




    }
}
