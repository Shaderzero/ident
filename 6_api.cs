using System;
using Microsoft.AspNetCore.Mvc;

namespace LogSuite.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        [HttpGet("Test")]
        public IActionResult Get([FromQuery(Name = "YearMonth")] string query)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                var array = query.Split('.');
                if (array.Length == 2)
                {
                    try
                    {
                        int month = Convert.ToInt32(array[0]);
                        int year = Convert.ToInt32(array[1]);
                        if (month >= 1 && month <= 12 && year >= 1900 && year <= 2100)
                        {
                            TestFilter filter = new TestFilter
                            {
                                YearMonth = new YearMonth
                                {
                                    Month = month,
                                    Year = year
                                }
                            };
                            return Ok(Test(filter));
                        }
                        throw new Exception();
                    }
                    catch (Exception ex)
                    {
                        return Ok(Test(null));
                    }
                }
            }
            return Ok(Test(null));
        }

        public string Test(TestFilter filter)
        {
            return filter?.YearMonth?.Display ?? "unknown";
        }

    }

    public class YearMonth
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Display => $"{Month}.{Year}";
    }
    public class TestFilter
    {
        public YearMonth YearMonth { get; set; }
    }
}