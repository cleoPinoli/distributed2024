using System;
using System.Threading.Tasks;
using Orleans;
using StackExchange.Redis;


namespace IoTSystem.Grains
{
    public class StanzaGrain : Grain, IStanzaGrain
    {

        public async Task InitSystem( string nr_lum, string nr_temp, string nr_umi)
        {

            var stanzaID = this.GetPrimaryKeyString();

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");

            Console.WriteLine("StanzaID: " + stanzaID);

            int lum, temp, umi;
            bool flagLum  = int.TryParse(nr_lum, out lum);
            bool flagTemp = int.TryParse(nr_temp, out temp);
            bool flagUmi  = int.TryParse(nr_umi, out umi);
            if (!(flagLum && flagTemp && flagUmi))
            {
                Console.WriteLine("Errore: parametri sensore/i invalidi");
            }
            else {

                int dbnum = 0;
                           
                // inizializzo i grain
                string chiaveCollettore = "CollettoreStanza" + stanzaID;
                var collettore = GrainFactory.GetGrain<ICollettoreGrain>(chiaveCollettore);
                await collettore.SetDbNum(dbnum);
                dbnum++;


                
                for (int i = 1; i <= lum; i++)
                {
                    var newGrain = GrainFactory.GetGrain<ISensoreGrain>("SensoreLuce" + i + "Stanza" + stanzaID);
                    await newGrain.SetTipo("Luce");
                    Console.WriteLine("---------------------------------------------------------");
                    Console.WriteLine($"valore collettorekey {chiaveCollettore}");
                    Console.WriteLine("---------------------------------------------------------");
                    await newGrain.SetCollettoreKey(chiaveCollettore);
                    await newGrain.SetDbNum(dbnum);
                }
                for (int i = 1; i <= temp; i++)
                {
                    var newGrain = GrainFactory.GetGrain<ISensoreGrain>("SensoreTemp" + i + "Stanza" + stanzaID);
                    await newGrain.SetTipo("Temperatura");
                    Console.WriteLine("---------------------------------------------------------");
                    Console.WriteLine($"valore collettorekey {chiaveCollettore}");
                    Console.WriteLine("---------------------------------------------------------");
                    await newGrain.SetCollettoreKey(chiaveCollettore);
                    await newGrain.SetDbNum(dbnum);
                }
                for (int i = 1; i <= umi; i++)
                {
                    var newGrain = GrainFactory.GetGrain<ISensoreGrain>("SensoreUmi" + i + "Stanza" + stanzaID);
                    await newGrain.SetTipo("Umidita");
                    Console.WriteLine("---------------------------------------------------------");
                    Console.WriteLine($"valore collettorekey {chiaveCollettore}");
                    Console.WriteLine("---------------------------------------------------------");
                    await newGrain.SetCollettoreKey(chiaveCollettore);
                    await newGrain.SetDbNum(dbnum);
                }


                // per semplicità di implementazione, gli attuatori sono stati fissati a 1 per tipo.
                var chiaveGrain = "AttuatoreLuceStanze" + stanzaID;
                var newAttuatoreLuce = GrainFactory.GetGrain<IAttuatoreGrain>(chiaveGrain);
                await newAttuatoreLuce.SetTipo("Luce");
                await collettore.SetAttuatoreLuce(chiaveGrain);

                chiaveGrain = "AttuatoreTempStanza" + stanzaID;
                var newAttuatoreTemp = GrainFactory.GetGrain<IAttuatoreGrain>(chiaveGrain);
                await newAttuatoreTemp.SetTipo("Temperatura");
                await collettore.SetAttuatoreTemp(chiaveGrain);


                chiaveGrain = "AttuatoreUmiStanza" + stanzaID;
                var newAttuatoreUmi = GrainFactory.GetGrain<IAttuatoreGrain>(chiaveGrain);
                await newAttuatoreUmi.SetTipo("Umidita");
                await collettore.SetAttuatoreUmi(chiaveGrain);


            }
        }

        public async Task SettaSoglie(string tipo, double soglia)
        {
            var stanzaID = this.GetPrimaryKeyString();
            var chiaveCollettore = "CollettoreStanza" + stanzaID;
            var collettore = GrainFactory.GetGrain<ICollettoreGrain>(chiaveCollettore);

            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine($"TIPO {tipo}  -  Settata soglia  {soglia}");
            Console.WriteLine("---------------------------------------------------------");

            switch (tipo)
            {
                case "luce":
                    await collettore.SetSogliaLuce(soglia);
                    break;
                case "temp":
                    await collettore.SetSogliaTemp(soglia);
                    break;
                case "umi":
                    await collettore.SetSogliaUmi(soglia);
                    break;
            }
        }
    }
}
