using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using Appointment.Models;
using Appointment.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using System.Data;
namespace Appointment.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddAppointment()
        {
            var data = fetchData();
            ViewData["ServicesList"] = data;
            return View();
        }

        [Route("Home/AddAppointment/{id}")]
        public IActionResult AddAppointment(int id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");
            ServicesModel account = null;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var query = "SELECT * FROM services WHERE id = @id";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                account = new ServicesModel
                                {
                                    Id = reader["id"]?.ToString(),
                                    Title = reader["title"]?.ToString(),
                                    Description = reader["description"]?.ToString(),
                                };
                            }
                        }
                    }
                }

                if (account == null)
                {
                    return NotFound(); // Return a 404 if no account is found
                }
                var data = fetchDataDentistSchedule();
                ViewData["DentistScheduleList"] = data;
                return View(account);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Route("Home/AddAppointment/{id?}")]
        public IActionResult AddAppointment(string id, string users_id, string dentist_schedule_id, string appointment_time)
        {
            if (string.IsNullOrWhiteSpace(users_id) || string.IsNullOrWhiteSpace(dentist_schedule_id) || string.IsNullOrWhiteSpace(appointment_time))
            {
                ViewData["Message"] = "All fields are required.";
                return View();
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");

            try
            {

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for existing appointment
                    var checkQuery = "SELECT COUNT(1) FROM schedule_appointment WHERE appointment_time = @AppointmentTime";
                    using (var checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@AppointmentTime", appointment_time);
                        var existingCount = (int)checkCommand.ExecuteScalar();

                        if (existingCount > 0 && string.IsNullOrWhiteSpace(id))
                        {
                            ViewData["Message"] = "An appointment with this dentist and time already exists.";
                            return View();
                        }
                    }

                        var insertQuery = @"INSERT INTO schedule_appointment (users_id, dentist_schedule_id, appointment_time, status) VALUES (@UsersId, @DentistScheduleId, @AppointmentTime, @Status)";
                        using (var insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@UsersId", users_id);
                            insertCommand.Parameters.AddWithValue("@DentistScheduleId", dentist_schedule_id);  // Consider passing an array if multiple are selected
                            insertCommand.Parameters.AddWithValue("@AppointmentTime", appointment_time);
                            insertCommand.Parameters.AddWithValue("@Status", "Pending");
                            insertCommand.ExecuteNonQuery();
                        }

                        ViewData["Message"] = "Appointment successfully created!";
                    



                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError($"SQL Error: {sqlEx.Message}");
                ViewData["Message"] = "A database error occurred.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                ViewData["Message"] = "An unexpected error occurred.";
            }

            return View();
        }

        private List<DentistScheduleModel> fetchDataDentistSchedule()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");
            var dataList = new List<DentistScheduleModel>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM dentist_schedule INNER JOIN users ON dentist_schedule.users_id=users.id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = new DentistScheduleModel
                                {
                                    Id = reader["id"]?.ToString() ?? "Unknown",
                                    UsersId  = reader["users_id"]?.ToString() ?? "Unknown",
                                    ServicesId = reader["services_id"]?.ToString() ?? "Unknown",
                                    DateAvailable = reader["date_available"]?.ToString() ?? "Unknown",
                                    StartTime = reader["start_time"]?.ToString() ?? "Unknown",
                                    EndTime = reader["end_time"]?.ToString() ?? "Unknown",
                                    FirstName = reader["firstname"]?.ToString() ?? "Unknown",
                                    LastName = reader["lastname"]?.ToString() ?? "Unknown"
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


        private List<ServicesModel> fetchData()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");
            var dataList = new List<ServicesModel>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM services";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = new ServicesModel
                                {
                                    Id = reader["id"]?.ToString() ?? "Unknown",
                                    Title = reader["title"]?.ToString() ?? "Unknown",
                                    Description = reader["description"]?.ToString() ?? "Unknown",
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

        public IActionResult Privacy()
        {
            return View();
        }

    }
}
