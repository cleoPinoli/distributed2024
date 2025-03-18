using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;

namespace IoTSystem.Grains
{
    internal class AttuatoreGrain : Grain, IAttuatoreGrain
    {
        private readonly IPersistentState<AttuatoreState> _state;

        public AttuatoreGrain([PersistentState("attuatoreState", "Default")] IPersistentState<AttuatoreState> state)
        {
            _state = state;
        }

        public async Task SetTipo(string tipo)
        {
            _state.State.Tipo = tipo;
            await _state.WriteStateAsync();
        }
        public async Task SetStatoLuce(bool stato)
        {
            _state.State.StatoLuce = stato;
            await _state.WriteStateAsync();
        }
        public async Task SetStatoUmi(bool stato)
        {
            _state.State.StatoUmi = stato;
            await _state.WriteStateAsync();
        }
        public async Task SetStatoTempCaldo(bool stato)
        {
            _state.State.StatoTempCaldo = stato;
            await _state.WriteStateAsync();
        }
        public async Task SetStatoTempFreddo(bool stato)
        {
            _state.State.StatoTempFreddo = stato;
            await _state.WriteStateAsync();
        }

        public async Task ChangeStatus(string tipo, bool status)
        {
            switch (tipo)
            {
                case "luce":
                    SetStatoLuce(status);
                    break;
                case "umi":
                    SetStatoUmi(status);
                    break;
                case "temperatura":
                    SetStatoTempCaldo(status);
                break;
            }
        }

        public async Task ChangeStatusTermico(bool riscaldamentoToStatus, bool raffreddamentoToStatus)
        {
            await SetStatoTempCaldo(riscaldamentoToStatus);
            await SetStatoTempFreddo(raffreddamentoToStatus);
        }

        public async Task GetStatoAttuatore()
        {
            Console.WriteLine("--------------------------------------");
            switch (_state.State.Tipo)
            {
                case "Luce":
                    Console.WriteLine($"Attuatore {this.GetPrimaryKeyString()} - Stato luce: {_state.State.StatoLuce}");
                    break;
                case "Umidita":
                    Console.WriteLine($"Attuatore {this.GetPrimaryKeyString()} - Stato umidità: {_state.State.StatoUmi}");
                    break;
                case "Temperatura":
                    Console.WriteLine($"Attuatore {this.GetPrimaryKeyString()} - Stato riscaldamento: {_state.State.StatoTempCaldo} - Stato raffreddamento: {_state.State.StatoTempFreddo}");
                    break;
            }
            Console.WriteLine("--------------------------------------");
        }
    }

}
