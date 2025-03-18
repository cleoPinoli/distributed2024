using Orleans;
using Orleans.Runtime;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace IoTSystem.Grains
{
    internal class CollettoreGrain : Grain, ICollettoreGrain
    {
        private readonly IPersistentState<CollettoreState> _state;

        private IAttuatoreGrain _attLuce;
        private IAttuatoreGrain _attTemp;
        private IAttuatoreGrain _attUmi;

        private readonly ConnectionMultiplexer _redis;

        public CollettoreGrain([PersistentState("collettoreState", "Default")] IPersistentState<CollettoreState> state, ConnectionMultiplexer redis)
        {

            _state = state;
            _redis = redis;
        }
        public async Task SetDbNum(int dbNum)
        {
            _state.State.DbNum = dbNum;
            await _state.WriteStateAsync();
        }

        public async Task SetAttuatoreLuce(string attuatoreLuce)
        {
            _state.State.AttuatoreLuce = attuatoreLuce;
            await _state.WriteStateAsync();
        }

        public async Task SetAttuatoreTemp(string attuatoreTemp)
        {
            _state.State.AttuatoreTemp = attuatoreTemp;
            await _state.WriteStateAsync();
        }

        public async Task SetAttuatoreUmi(string attuatoreUmi)
        {
            _state.State.AttuatoreUmi = attuatoreUmi;
            await _state.WriteStateAsync();
        }

        public Task<string> GetAttuatoreLuce()
        {
            return Task.FromResult(_state.State.AttuatoreLuce);
        }

        public Task<string> GetAttuatoreTemp()
        {
            return Task.FromResult(_state.State.AttuatoreTemp);
        }

        public Task<string> GetAttuatoreUmi()
        {
            return Task.FromResult(_state.State.AttuatoreUmi);
        }

        public Task<int> GetDbNum()
        {
            return Task.FromResult(_state.State.DbNum);
        }
        public Task SetSogliaLuce(double sogliaLuce)
        {
            _state.State.SogliaLuce = sogliaLuce;
            return _state.WriteStateAsync();
        }
        public Task<double> GetSogliaLuce()
        {
            return Task.FromResult(_state.State.SogliaLuce);
        }

        public Task<double> GetSogliaTemp()
        {
            return Task.FromResult(_state.State.SogliaTemp);
        }

        public Task<double> GetSogliaUmi()
        {
            return Task.FromResult(_state.State.SogliaUmi);
        }

        public Task<string> GetModoTemp()
        {
            return Task.FromResult(_state.State.ModoTemp);
        }

        public Task SetModoTemp(string modoTemp)
        {
            _state.State.ModoTemp = modoTemp;
            return _state.WriteStateAsync();
        }
        public Task SetSogliaUmi(double sogliaUmi)
        {
            _state.State.SogliaUmi = sogliaUmi;
            return _state.WriteStateAsync();
        }
        public Task SetSogliaTemp(double sogliaTemp)
        {
            _state.State.SogliaTemp = sogliaTemp;
            return _state.WriteStateAsync();
        }

        public async Task GetStatoAttuatori()
        {
            _attLuce = GrainFactory.GetGrain<IAttuatoreGrain>(_state.State.AttuatoreLuce);
            _attTemp = GrainFactory.GetGrain<IAttuatoreGrain>(_state.State.AttuatoreTemp);
            _attUmi = GrainFactory.GetGrain<IAttuatoreGrain>(_state.State.AttuatoreUmi);
            await _attLuce.GetStatoAttuatore();
            await _attTemp.GetStatoAttuatore();
            await _attUmi.GetStatoAttuatore();
        }



        public async Task ComputeMetaAverage(string tipo)
        {
            IDatabase db = _redis.GetDatabase(_state.State.DbNum);
            var server = _redis.GetServer("localhost", 6379); // localhost hard-coded, modificare se remoto
            var keys = server.Keys(database: _state.State.DbNum, pattern: tipo + ".*").ToArray();
            var redisValues = keys.Select(key => db.StringGet(key)).ToArray();

            // Filtra i valori validi e convertili in numeri
            var numericValues = redisValues.Where(v => v.HasValue).Select(v => (double)v);

            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("TIPO " + tipo);

            // Calcola la media
            if (numericValues.Count() < 1)
            {
                //sono presenti meno di 3 valori, attendo
                Console.Write($"collettore: {this.GetPrimaryKeyString()} - nessun rilevamento presente.");
            }
            else
            {
                double average = numericValues.Average();
                IAttuatoreGrain attuatore;

                switch (tipo)
                {
                    case "Luce":
                        attuatore = GrainFactory.GetGrain<IAttuatoreGrain>(_state.State.AttuatoreLuce);
                        if (average > _state.State.SogliaLuce)
                        {
                            Console.WriteLine($"collettore: {this.GetPrimaryKeyString()} \n  Valore media{average}\n  Media superiore alla soglia di luce.\n  Invio segnale di SWITCH OFF all'attuatore");
                            await attuatore.ChangeStatus("luce", false);
                        }
                        else
                        {
                            Console.WriteLine($"collettore: {this.GetPrimaryKeyString()} \n  Valore media{average}\n  Media inferiore alla soglia di luce.\n  Invio segnale di SWITCH ON all'attuatore");
                            await attuatore.ChangeStatus("luce", true);
                        }
                        break;
                    case "Temperatura":
                        attuatore = GrainFactory.GetGrain<IAttuatoreGrain>(_state.State.AttuatoreTemp);
                        if (average < _state.State.SogliaTemp) {
                            //SOTTO SOGLIA
                            if (_state.State.ModoTemp == "inverno")
                            {
                                //MODO INVERNO T < SOGLIA, switchON    T > SOGLIA, switchOFF
                                Console.WriteLine($"collettore: {this.GetPrimaryKeyString()}\n  Modo INVERNO\n  Valore media{average}\n  Media inferiore alla soglia di temperatura.\n  Invio segnale di SWITCH ON all'attuatore CALDO.");
                                await attuatore.ChangeStatusTermico(true, false);
                            }
                            else
                            {
                                //MODO ESTATE T < SOGLIA, switchOFF    T > SOGLIA, switchON
                                Console.WriteLine($"collettore: {this.GetPrimaryKeyString()}\n  Modo ESTATE\n  Valore media{average}\n  Media inferiore alla soglia di temperatura.\n  Invio segnale di SWITCH OFF all'attuatore FREDDO.");
                                await attuatore.ChangeStatusTermico(false, false);
                            }
                        } else
                        {
                            //SOPRA SOGLIA
                            if (_state.State.ModoTemp == "inverno")
                            {
                                //MODO INVERNO T < SOGLIA, switchON    T> SOGLIA, switchOFF
                                Console.WriteLine($"collettore: {this.GetPrimaryKeyString()}\n  Modo INVERNO\n  Valore media{average}\n  Media superiore alla soglia di temperatura.\n  Invio segnale di SWITCH OFF all'attuatore CALDO.");
                                await attuatore.ChangeStatusTermico(false, false);
                            }
                            else
                            {
                                //MODO ESTATE T < SOGLIA, switchOFF    T> SOGLIA, switchON
                                Console.WriteLine($"collettore: {this.GetPrimaryKeyString()}\n  Modo ESTATE\n  Valore media{average}\n  Media superiore alla soglia di temperatura.\n  Invio segnale di SWITCH ON all'attuatore FREDDO.");
                                await attuatore.ChangeStatusTermico(false, true);
                            }
                        }
                        break;

                    case "Umidita":
                        attuatore = GrainFactory.GetGrain<IAttuatoreGrain>(_state.State.AttuatoreUmi);
                        if (average > _state.State.SogliaUmi)
                        {
                            Console.WriteLine($"collettore: {this.GetPrimaryKeyString()}\n  Valore media{average}\n  Media superiore alla soglia di umidità.\n  Invio segnale all'attuatore di SWITCH ON");
                            await attuatore.ChangeStatus("unidita", true);
                        }
                        else
                        {
                            Console.WriteLine($"collettore: {this.GetPrimaryKeyString()}\n  Valore media{average}\n  Media inferiore alla soglia di umidità.\n  Invio segnale all'attuatore di SWITCH OFF");
                            await attuatore.ChangeStatus("umidita", false);
                        }
                        break;

                }

            }

            Console.WriteLine("--------------------------------------------------");
        }

        public async Task ReceiveAverageFromSensor(string tipo, double valore)
        {
            // ottiene il database Redis
            int _dbNum = await GetDbNum();
            IDatabase db = _redis.GetDatabase(_dbNum);

            string timestampKey = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            // Scrivi il valore nel database
            await db.StringSetAsync(tipo + "." + timestampKey, valore, TimeSpan.FromSeconds(300));

        }

    }
}
