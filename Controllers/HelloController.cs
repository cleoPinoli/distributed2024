using Microsoft.AspNetCore.Mvc;
using Orleans;
using System.Threading.Tasks;
using IoTSystem.Grains;


namespace Controllers
{
    [ApiController]
    [Route("")]
    public class HelloController : ControllerBase
    {
        private readonly IGrainFactory _grainFactory;

        public HelloController(IGrainFactory grainFactory)
        {
            _grainFactory = grainFactory;
        }


        [HttpGet("avviaStanza/{stanzaID}/{SensoriLum}/{SensoriTemp}/{SensoriUmi}")]
        public async Task AvviaStanza(string stanzaID, string SensoriLum, string SensoriTemp, string SensoriUmi) {

            var myStanza = _grainFactory.GetGrain<IStanzaGrain>(stanzaID);
            await myStanza.InitSystem( SensoriLum, SensoriTemp, SensoriUmi);
        }


        [HttpGet("rilevaTemp/{stanzaID}/{Sensore}/{Valore}")]
        public async Task rilevaTemp(string stanzaID, string sensore, string valore)
        {
            var mysensore = _grainFactory.GetGrain<ISensoreGrain>("SensoreTemp" + sensore + "Stanza" + stanzaID);

            await mysensore.SaveValueToDB(valore);
        }


        [HttpGet("rilevaLuce/{stanzaID}/{Sensore}/{Valore}")]
        public async Task rilevaLuce(string stanzaID, string sensore, string valore)
        {
            var mysensore = _grainFactory.GetGrain<ISensoreGrain>("SensoreLuce" + sensore + "Stanza" + stanzaID);

            await mysensore.SaveValueToDB(valore);
        }


        [HttpGet("rilevaUmi/{stanzaID}/{Sensore}/{Valore}")]
        public async Task rilevaUmi(string stanzaID, string sensore, string valore)
        {
            var mysensore = _grainFactory.GetGrain<ISensoreGrain>("SensoreUmi" + sensore + "Stanza" + stanzaID);

            await mysensore.SaveValueToDB(valore);
        }


        [HttpGet("inviaMedia/{tipo}/{stanzaID}/{Sensore}")]
        public async Task inviaMedia(string tipo, string stanzaID, string sensore)
        {
            ISensoreGrain mysensore;
            switch (tipo)
            {
                case "temp":
                    mysensore = _grainFactory.GetGrain<ISensoreGrain>("SensoreTemp" + sensore + "Stanza" + stanzaID);
                    await mysensore.SendValueAverage();
                    break;
                case "luce":
                    mysensore = _grainFactory.GetGrain<ISensoreGrain>("SensoreLuce" + sensore + "Stanza" + stanzaID);
                    await mysensore.SendValueAverage();
                    break;
                case "umi":
                    mysensore = _grainFactory.GetGrain<ISensoreGrain>("SensoreUmi" + sensore + "Stanza" + stanzaID);
                    await mysensore.SendValueAverage();
                    break;
            }
        }


        [HttpGet("settasoglia/{stanzaID}/{Tipo}/{Valore}")]
        public async Task settaSoglia(string stanzaID, string tipo, double valore)
        {
            var myStanza = _grainFactory.GetGrain<IStanzaGrain>(stanzaID);
            await myStanza.SettaSoglie(tipo, valore);
        }


        [HttpGet("collettoreElabora/{stanzaID}")]
        public async Task collettoreElabora(string stanzaID)
        {
            var collettore = _grainFactory.GetGrain<ICollettoreGrain>("CollettoreStanza" + stanzaID);
            await collettore.ComputeMetaAverage("Luce");
            await collettore.ComputeMetaAverage("Temperatura");
            await collettore.ComputeMetaAverage("Umidita");
        }
        

        [HttpGet("statoAttuatori/{stanzaID}")]
        public async Task statoAttuatori(string stanzaID)
        {
            var collettore = _grainFactory.GetGrain<ICollettoreGrain>("CollettoreStanza" + stanzaID);

            await collettore.GetStatoAttuatori();
        }


        [HttpGet("setStagione/{stanzaID}/{stagione}")]

        public async Task SetStagione(string stanzaID, string stagione) {
            var collettore = _grainFactory.GetGrain<ICollettoreGrain>("CollettoreStanza" + stanzaID);
            await collettore.SetModoTemp(stagione);
        }

    }
}