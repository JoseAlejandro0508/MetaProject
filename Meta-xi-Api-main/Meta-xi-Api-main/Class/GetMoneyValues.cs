using Newtonsoft.Json.Linq;

namespace Meta.Application;

public class GetMoneyValues
{
    public async Task<decimal> GetMoneyValueAsync(string name)
    {
        string url = "";
        if(name == "trx"){
            url = "https://api.coingecko.com/api/v3/simple/price?ids=tron&vs_currencies=usd";
        }
        else if(name == "tether"){
            url = "https://min-api.cryptocompare.com/data/price?fsym=USDT&tsyms=USD";
        }
        else{
            url = "https://min-api.cryptocompare.com/data/price?fsym=USD&tsyms=COP";
        }
        
        using (var client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Respuesta de la API: {jsonResponse}");
                JObject? data = JObject.Parse(jsonResponse);
                Console.WriteLine(data);
                decimal? value = name switch{
                    "trx" => data["tron"]?["usd"]?.Value<decimal>(),
                    "tether" => data["USD"]?.Value<decimal>(),
                    "cop"=> data["COP"]?.Value<decimal>(),
                    _ => throw new Exception("Moneda no encontrada en la respuesta")
                };

                if (value.HasValue)
                {
                    return value.Value;
                }
                else
                {
                    throw new Exception($"Moneda {name} no encontrada en la respuesta");
                }
            }
            else
            {
                throw new Exception("Error al obtener la respuesta");
            }
        }
    }
}