namespace MoscowWeatherApp.Models
{
    public class MyClasses
    {
    }

    public class WeatherTemp
    {
        public string Year { get; set; }
        public int AverageTemperature { get; set; }
        public int AverageHumidity { get; set; }
        public int AverageTd { get; set; }
        public int AveragePressure { get; set; }
        //public string AverageWindDirection { get; set; }        ?
        public int AverageWindSpeed { get; set; }
        public int AverageCloudCover { get; set; }
        public int AverageH { get; set; }
        public int AverageVV { get; set; }
        //public string WeatherPhenomena { get; set; }     ?
    }
}
