using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using Appointment.Models;
using Appointment.Helpers;
namespace Appointment.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _configuration;

        public AdminController(ILogger<AdminController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Employees()
        {
            var data = fetchData();
            ViewData["AccountList"] = data; // Pass data to the View.
            return View();
        }


        private List<AccountModel> fetchData()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");
            var dataList = new List<AccountModel>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM users";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = new AccountModel
                                {
                                    FirstName = reader["firstname"]?.ToString() ?? "Unknown",
                                    MiddleName = reader["middlename"]?.ToString() ?? "Unknown",
                                    LastName = reader["lastname"]?.ToString() ?? "Unknown",
                                    Email = reader["email"]?.ToString() ?? "Unknown"
                                };
                                dataList.Add(data);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching data: {ex.Message}");
            }

            return dataList;
        }
    }
}
