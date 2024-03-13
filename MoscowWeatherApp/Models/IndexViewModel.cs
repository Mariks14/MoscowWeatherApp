using MoscowWeatherApp.Models.EFModels;

namespace MoscowWeatherApp.Models
{
    public class IndexViewModel
    {
        //public IEnumerable<> Users { get; }
        public IEnumerable<WeatherTemp> List { get; }
        //public List<RakooEvents> Awdaw { get;}
        public PageViewModel PageViewModel { get; }


        public IndexViewModel(IEnumerable<WeatherTemp> list, PageViewModel viewModel)
        {
            //RakooClass.rakooMonthGlobal
            List = list;
            PageViewModel = viewModel;
        }
    }
}
