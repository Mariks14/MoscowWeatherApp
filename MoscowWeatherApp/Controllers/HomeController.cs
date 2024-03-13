using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoscowWeatherApp.Models;
using MoscowWeatherApp.Models.EFContext;
using MoscowWeatherApp.Models.EFModels;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Diagnostics;
using System.Drawing.Printing;

namespace MoscowWeatherApp.Controllers
{
    public class HomeController : Controller
    {
        private  ILogger<HomeController> _logger;
        private  IWebHostEnvironment _appEnvironment;
        private  ApplicationContext _context;
        

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment appEnvironment, ApplicationContext context)
        {
            _logger = logger;
            _appEnvironment = appEnvironment;
            _context = context;
        }

        public IActionResult Index()
        {
            Console.WriteLine(_context.Weather.Count());
            foreach (var item in _context.FileModel)
            {
                Console.WriteLine(item.Id);
            }
            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            foreach (var item in _context.Weather)
            {
                Console.WriteLine(item.Id);
            }

            return View();
        }

        public IActionResult FormAddFile()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddFile(IFormFileCollection uploads)
        {
            bool AllIncorrect = true;
            List<string> filesnames = new List<string>();
            foreach (var uploadedFile in uploads)
            {
                if (uploadedFile == null || uploadedFile.Length == 0)
                {
                    continue;
                }

                 Console.WriteLine(uploadedFile.FileName);

                if (!(Path.GetExtension(uploadedFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(uploadedFile.FileName).Equals(".xls", StringComparison.OrdinalIgnoreCase)))
                {
                    return RedirectToAction("CommentPage", new { CommentType = "FileError", FilesNames = filesnames });
                }
                
                List<Weather> weatherDataList = new List<Weather>();
                try
                {
                    

                    // Читаем данные из потока IFormFile
                    using (Stream stream = uploadedFile.OpenReadStream())
                    {
                        // Создаем книгу Excel
                        XSSFWorkbook workbook = new XSSFWorkbook(stream);

                        // Получаем первый лист из книги
                        ISheet sheet = workbook.GetSheetAt(0);

                        // Проходим по всем строкам в листе
                        for (int i = 5; i <= sheet.LastRowNum; i++)
                        {
                            IRow row = sheet.GetRow(i);
                            if (row != null) // Проверяем, что строка не пустая
                            {
                                Weather weatherData = new Weather();

                                // Заполняем объект Weather данными из Excel строки
                                //weatherData.Id = (int)row.GetCell(0).NumericCellValue;
                                weatherData.Date = row.GetCell(0)?.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(row.GetCell(0))
                                                        ? DateTime.FromOADate(row.GetCell(0).NumericCellValue).ToString("dd.MM.yyyy")
                                                        : string.IsNullOrWhiteSpace(row.GetCell(0)?.StringCellValue)
                                                            ? ""
                                                            : row.GetCell(0).StringCellValue;
                                //string.IsNullOrWhiteSpace(row.GetCell(0)?.StringCellValue) ? "" : row.GetCell(0).StringCellValue; 
                                weatherData.Time = string.IsNullOrWhiteSpace(row.GetCell(1)?.StringCellValue) ? "" : row.GetCell(1).StringCellValue; 
                                weatherData.Temperature = row.GetCell(2).CellType == CellType.Numeric ? (int)row.GetCell(2).NumericCellValue : 0; 
                                weatherData.humidity = row.GetCell(3).CellType == CellType.Numeric ? (int)row.GetCell(3).NumericCellValue : 0; 
                                weatherData.Td = row.GetCell(4).CellType == CellType.Numeric ? (int)row.GetCell(4).NumericCellValue : 0; 
                                weatherData.pressure = row.GetCell(5).CellType == CellType.Numeric ? (int)row.GetCell(5).NumericCellValue : 0; 
                                weatherData.windDirection = row.GetCell(6).StringCellValue;
                                weatherData.windSpeed = row.GetCell(7).CellType == CellType.Numeric ? (int)row.GetCell(7).NumericCellValue : 0; 
                                weatherData.cloudCover = row.GetCell(8).CellType == CellType.Numeric ? (int)row.GetCell(8).NumericCellValue : 0; 
                                weatherData.h = row.GetCell(9).CellType == CellType.Numeric ? (int)row.GetCell(9).NumericCellValue : 0; 
                                weatherData.VV = row.GetCell(10).CellType == CellType.Numeric ? (int)row.GetCell(10).NumericCellValue : 0;     //в этой ячейке написаны цыфры но тип в этом столбце text в exel файле
                                weatherData.WeatherPhenomena = string.IsNullOrWhiteSpace(row.GetCell(11)?.StringCellValue) ? "" : row.GetCell(11).StringCellValue; 

                                Console.WriteLine($"Date :{weatherData.Date}, Time : {weatherData.Time}, Temperature : {weatherData.Temperature}, humidity : {weatherData.humidity}");
                                // Добавляем объект Weather в список
                                weatherDataList.Add(weatherData);
                            }
                        }
                    }
                    AllIncorrect = false;

                    // путь к папке Files
                    string path = "/Files/" + uploadedFile.FileName;
                    // сохраняем файл в папку Files в каталоге wwwroot
                    // тут не учитывается 2 одинаковых файла (с одинаковым названием), так что они друг друга перезапишут
                    using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                    {
                        await uploadedFile.CopyToAsync(fileStream);
                    }
                    FileModel file = new FileModel { Name = uploadedFile.FileName, Path = path };

                    // сохранение в бд
                    await _context.FileModel.AddAsync(file);

                    await _context.Weather.AddRangeAsync(weatherDataList);
                    await _context.SaveChangesAsync();

                    filesnames.Add(uploadedFile.FileName);
                }
                catch (Exception)
                {
                    Console.WriteLine("error");
                    throw;
                    continue;
                }

                if (AllIncorrect)
                {
                    return RedirectToAction("CommentPage", new { CommentType = "FileError", FilesNames = filesnames });
                }
                 
                
            }





            return RedirectToAction("CommentPage", new { CommentType = "Ok", FilesNames = filesnames });
        }

        public  IActionResult WeatherTables(string sort = "year", int page = 1)
        {
            ViewBag.Sort = sort;

            int pageSize = 5;

            List<WeatherTemp> averWeathByYear = new List<WeatherTemp>();

            if (sort =="month")
            {
                averWeathByYear = _context.Weather
                .GroupBy(w => w.Date.Substring(3, 7)) // Группируем по году
                .Select(g => new WeatherTemp
                {
                    Year = g.Key,
                    AverageTemperature = (int)g.Average(w => w.Temperature),
                    AverageHumidity = (int)g.Average(w => w.humidity),
                    AverageTd = (int)g.Average(w => w.Td),
                    AveragePressure = (int)g.Average(w => w.pressure),
                    AverageWindSpeed = (int)g.Average(w => w.windSpeed),
                    AverageCloudCover = (int)g.Average(w => w.cloudCover),
                    AverageH = (int)g.Average(w => w.h),
                    AverageVV = (int)g.Average(w => w.VV),
                })
                .OrderByDescending(g => g.Year) // Сортируем по убыванию года
                .ToList();
            }
            else
            {
                averWeathByYear = _context.Weather
                .GroupBy(w => w.Date.Substring(6, 4)) // Группируем по году
                .Select(g => new WeatherTemp
                {
                    Year = g.Key,
                    AverageTemperature = (int)g.Average(w => w.Temperature),
                    AverageHumidity = (int)g.Average(w => w.humidity),
                    AverageTd = (int)g.Average(w => w.Td),
                    AveragePressure = (int)g.Average(w => w.pressure),
                    AverageWindSpeed = (int)g.Average(w => w.windSpeed),
                    AverageCloudCover = (int)g.Average(w => w.cloudCover),
                    AverageH = (int)g.Average(w => w.h),
                    AverageVV = (int)g.Average(w => w.VV),
                })
                .OrderByDescending(g => g.Year) // Сортируем по убыванию года
                .ToList();
            }

            

            var count = averWeathByYear.Count();
            var items = averWeathByYear.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            PageViewModel pageViewModel = new PageViewModel(count, page, pageSize);
            IndexViewModel viewModel = new IndexViewModel(items, pageViewModel);

            return View(viewModel);
        }

        public IActionResult CommentPage(string CommentType, List<string> FilesNames )
        {
            
            ViewBag.CommentType = CommentType;
            ViewBag.FilesNames = FilesNames;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
