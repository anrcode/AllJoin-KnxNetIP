using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;
using SparkAlljoyn;


namespace KnxNetIPAdapter
{
    internal class OpenWeatherData
    {
        public TimeSpan Sunrise { get; set; }

        public TimeSpan Sunset { get; set; }

        public OpenWeatherSample Current { get; set; }

        public List<OpenWeatherSample> Forecast { get; set; }
    }

    internal class OpenWeatherSample
    {
        public string Symbol { get; set; }

        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public double Temperature { get; set; }

        public double TemperatureMin { get; set; }

        public double TemperatureMax { get; set; }

        public double Humidity { get; set; }

        public double Pressure { get; set; }

        public double WindSpeed { get; set; }

        public double WindDirection { get; set; }

        public double Clouds { get; set; }
    }

    internal class OpenWeather : BridgeAdapterDevice<KnxAdapter>
    {
        public OpenWeather() : base(null, "OpenWeather", "KNX", "KNX Switch", "1.0", "1234", "1234")
        {
            // http://api.openweathermap.org/data/2.5/weather?lat=47.104359&lon=7.3639&APPID=44db6a862fba0b067b1930da0d769e98&units=metric
            // http://api.openweathermap.org/data/2.5/forecast?q=schnottwil&mode=xml&appid=44db6a862fba0b067b1930da0d769e98
            
            Task.Run(async () =>
            {
                while(true)
                {
                    try
                    {
                        await Task.Delay(20000);
                        await AquireCurrentState();
                    }
                    catch (Exception)
                    {
                    }
                }
            });
        }

        public async Task<bool> AquireCurrentState()
        {
            var data = await FetchWeatherData(47.104f, 7.36f, "6bca1c62f76272f24225dae1d97d0886");

            var weatherData = new OpenWeatherData();

            var sys = data.GetNamedObject("sys");
            var sunriseValue = sys.GetNamedNumber("sunrise", 0);
            weatherData.Sunrise = UnixTimeStampToDateTime(sunriseValue).TimeOfDay;
            var sunsetValue = sys.GetNamedNumber("sunset", 0);
            weatherData.Sunset = UnixTimeStampToDateTime(sunsetValue).TimeOfDay;

            weatherData.Current = new OpenWeatherSample();
            weatherData.Forecast = new List<OpenWeatherSample>();

            var main = data.GetNamedObject("main");
            var wind = data.GetNamedObject("wind");
            weatherData.Current = new OpenWeatherSample()
            {
                Temperature = main.GetNamedNumber("temp", 0),
                TemperatureMin = main.GetNamedNumber("temp_min", 0),
                TemperatureMax = main.GetNamedNumber("temp_max", 0),
                Humidity = main.GetNamedNumber("humidity", 0),
                Pressure = main.GetNamedNumber("pressure", 0),
                WindSpeed = wind.GetNamedNumber("speed", 0),
                WindDirection = wind.GetNamedNumber("deg", 0)
            };

            var weather = data.GetNamedArray("weather");
            var _situation = weather.First().GetObject().GetNamedValue("id");

            var forecast = await FetchWeatherForecast(47.104f, 7.36f, "6bca1c62f76272f24225dae1d97d0886");
            var list = forecast.GetNamedArray("list");
            foreach(var entry in list)
            {
                data = entry.GetObject();
                main = data.GetNamedObject("main");
                wind = data.GetNamedObject("wind");

                weatherData.Forecast.Add(new OpenWeatherSample()
                {
                    Temperature = main.GetNamedNumber("temp", 0),
                    TemperatureMin = main.GetNamedNumber("temp_min", 0),
                    TemperatureMax = main.GetNamedNumber("temp_max", 0),
                    Humidity = main.GetNamedNumber("humidity", 0),
                    Pressure = main.GetNamedNumber("pressure", 0),
                    WindSpeed = wind.GetNamedNumber("speed", 0),
                    WindDirection = wind.GetNamedNumber("deg", 0)
                });

            }

            return true;
        }

        private async Task<JsonObject> FetchWeatherData(float lat, float lon, string appId)
        {
            var dataUri = new Uri(string.Format("http://api.openweathermap.org/data/2.5/weather?lat={0}&lon={1}&APPID={2}&units=metric", lat, lon, appId));
            using (var httpClient = new HttpClient())
            using (var result = await httpClient.GetAsync(dataUri))
            {
                var jsonContent = await result.Content.ReadAsStringAsync();
                return JsonObject.Parse(jsonContent);
            }
        }

        private async Task<JsonObject> FetchWeatherForecast(float lat, float lon, string appId)
        {
            var dataUri = new Uri(string.Format("http://api.openweathermap.org/data/2.5/forecast?lat={0}&lon={1}&APPID={2}&units=metric", lat, lon, appId));
            using (var httpClient = new HttpClient())
            using (var result = await httpClient.GetAsync(dataUri))
            {
                var jsonContent = await result.Content.ReadAsStringAsync();
                return JsonObject.Parse(jsonContent);
            }
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var buffer = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return buffer.AddSeconds(unixTimeStamp).ToLocalTime();
        }
    }
}
