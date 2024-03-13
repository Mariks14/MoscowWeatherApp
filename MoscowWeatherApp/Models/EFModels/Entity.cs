namespace MoscowWeatherApp.Models.EFModels
{
    public class Entity
    {
    }

    public class FileModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }

    public class Weather
    {
        public int Id { get; set; }
        public string? Date { get; set; }
        public string? Time { get; set; }
        public int? Temperature { get; set;}
        public int? humidity { get; set; }
        public int? Td { get; set; }
        public int? pressure { get; set; }
        public string? windDirection { get; set; }
        public int? windSpeed { get; set; }
        public int? cloudCover { get; set; }
        public int? h { get; set;}
        public int? VV { get; set; }
        public string? WeatherPhenomena { get; set;}
    }
}
