using System;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using StackExchange.Redis;

namespace IoTSystem.Grains
{
    internal class SensoreGrain : Grain, ISensoreGrain
    {

        private readonly IPersistentState<SensoreState> _state;

        

        private string _tipo;

        private readonly ConnectionMultiplexer _redis;

        public SensoreGrain( [PersistentState("sensoreState", "Default")] IPersistentState<SensoreState> state, ConnectionMultiplexer redis)
        {

            _state = state;
            _redis = redis;
        }

        public async Task SetTipo(string tipo)
        {
            _state.State.Tipo = tipo;
            await _state.WriteStateAsync();
        }

        public async Task SetDbNum(int dbNum)
        {
            _state.State.DbNum = dbNum;
            await _state.WriteStateAsync();
        }

        public async Task SetCollettoreKey(string collettoreKey)
        {
            _state.State.CollettoreKey = collettoreKey;
            await _state.WriteStateAsync();
        }

        public Task<int> GetDbNum()
        {
            return Task.FromResult(_state.State.DbNum);
        }

        public Task<string> GetCollettoreKey()
        {
            return Task.FromResult(_state.State.CollettoreKey);
        }

        public Task<string> GetTipo()
        {
            return Task.FromResult(_state.State.Tipo);
        }

        public Task ReadValueFromSensor()
        {
            
            throw new NotImplementedException();
        }

        

        public async Task SaveValueToDB(string value)
        {
            // Ottieni il database Redis
            int _dbNum = await GetDbNum();
            IDatabase db = _redis.GetDatabase(_dbNum);

            string timestampKey = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            // Scrivi il valore nel database
            await db.StringSetAsync(_state.State.Tipo+"."+timestampKey, value, TimeSpan.FromSeconds(300));


            
        }

        
        public async Task SendValueAverage()
        {
            IDatabase db = _redis.GetDatabase(_state.State.DbNum);
            var server = _redis.GetServer("localhost", 6379); // localhost hard-coded, modifica se remoto
            var keys = server.Keys(database: _state.State.DbNum, pattern: _state.State.Tipo+".*").ToArray();
            var redisValues = keys.Select(key => db.StringGet(key)).ToArray();

            // Filtra i valori validi e converte in numeri
            var numericValues = redisValues.Where(v => v.HasValue).Select(v => (double)v);

            // Calcola la media
            if (numericValues.Count() < 2)
            {
                //sono presenti meno di 3 valori, attendo
                Console.Write($"sensore: {this.GetPrimaryKeyString()} - meno di 2 rilevamenti presenti. media inaffidabile");
            }
            else
            {
                double average = numericValues.Average();
                Console.WriteLine($"I valori sono: {string.Join(", ", numericValues)}");
                Console.WriteLine($"sensore: {this.GetPrimaryKeyString()} - La media dei valori è: {average}");

                Console.WriteLine($"VALORE CHIAVE COLLETTORE {_state.State.CollettoreKey}");
                var collettore = GrainFactory.GetGrain<ICollettoreGrain>(_state.State.CollettoreKey);
                await collettore.ReceiveAverageFromSensor(_state.State.Tipo, average);
            }

            

        }
    }
}
