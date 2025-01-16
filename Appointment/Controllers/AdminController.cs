using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using Appointment.Models;
using Appointment.Helpers;
using Microsoft.AspNetCore.Mvc;


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

        public IActionResult DentistSchedule()
        {

            var data = fetchDataDentistSchedule();
            ViewData["DentistScheduleList"] = data;
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
                    var query = "SELECT *,dentist_schedule.id AS dentist_schedule_id FROM dentist_schedule INNER JOIN users ON dentist_schedule.users_id=users.id INNER JOIN services ON dentist_schedule.services_id=services.id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = new DentistScheduleModel
                                {
                                    Id = reader["dentist_schedule_id"]?.ToString() ?? "Unknown",
                                    DateAvailable = reader["date_available"]?.ToString() ?? "Unknown",
                                    StartTime = reader["start_time"]?.ToString() ?? "Unknown",
                                    EndTime = reader["end_time"]?.ToString() ?? "Unknown",
                                    FirstName = reader["firstname"]?.ToString() ?? "Unknown",
                                    LastName = reader["lastname"]?.ToString() ?? "Unknown",
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


        public IActionResult AddDentistSchedule()
        {
            var data2 = fetchDataDentist();
            ViewData["AccountList"] = data2;
            var data = fetchDataServices();
            ViewData["ServicesList"] = data;
            return View();
        }

        [Route("Admin/AddDentistSchedule/{id}")]
        public IActionResult AddDentistSchedule(int id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");
            DentistScheduleModel account = null;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var query = "SELECT *, dentist_schedule.id AS dentist_schedule_id FROM dentist_schedule INNER JOIN users ON dentist_schedule.users_id=users.id INNER JOIN services ON dentist_schedule.services_id=services.id WHERE dentist_schedule.id = @id";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                account = new DentistScheduleModel
                                {
                                    Id = reader["dentist_schedule_id"]?.ToString(),
                                    UsersId = reader["users_id"]?.ToString(),
                                    ServicesId = reader["services_id"]?.ToString(),
                                    DateAvailable = DateTime.Parse(reader["date_available"].ToString()).ToString("dd/MM/yyyy"),
                                    StartTime = reader["start_time"]?.ToString(),
                                    EndTime = reader["end_time"]?.ToString(),
                                    FirstName = reader["firstname"]?.ToString(),
                                    LastName = reader["lastname"]?.ToString(),
                                };
                            }
                        }
                    }
                }

                if (account == null)
                {
                    return NotFound(); // Return a 404 if no account is found
                }

                var data = fetchDataDentist();
                ViewData["AccountList"] = data;
                var data2 = fetchDataServices();
                ViewData["ServicesList"] = data2;

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
        [Route("Admin/AddDentistSchedule/{id?}")]

        public IActionResult AddDentistSchedule(string id, string users_id, string services_id, string date_available, string start_time, string end_time)
        {
            if (string.IsNullOrWhiteSpace(users_id) || string.IsNullOrWhiteSpace(services_id) || string.IsNullOrWhiteSpace(date_available) || string.IsNullOrWhiteSpace(start_time) ||
                string.IsNullOrWhiteSpace(end_time))
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

                    // Check if the email already exists in the database
                    var checkQuery = "SELECT COUNT(1) FROM dentist_schedule WHERE users_id = @UsersId AND services_id = @ServicesId AND date_available = @DateAvailable";
                    using (var checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@UsersId", users_id);
                        checkCommand.Parameters.AddWithValue("@ServicesId", services_id);
                        checkCommand.Parameters.AddWithValue("@DateAvailable", date_available);
                        var existingCount = (int)checkCommand.ExecuteScalar();

                        if (existingCount > 0 && string.IsNullOrWhiteSpace(id))
                        {
                            ViewData["Message"] = "An data with this dentist and date already exists.";
                            return View();
                        }
                    }

                    if (string.IsNullOrWhiteSpace(id))
                    {
                        // If `id` is empty, insert a new record
                        var insertQuery = @"
                    INSERT INTO dentist_schedule (users_id, services_id, date_available, start_time, end_time) 
                    VALUES (@UsersId, @ServicesId, @DateAvailable, @StartTime, @EndTime)";

                        using (var insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@UsersId", users_id);
                            insertCommand.Parameters.AddWithValue("@ServicesId", services_id);
                            insertCommand.Parameters.AddWithValue("@DateAvailable", date_available);
                            insertCommand.Parameters.AddWithValue("@StartTime", start_time);
                            insertCommand.Parameters.AddWithValue("@EndTime", end_time);

                            insertCommand.ExecuteNonQuery();
                        }

                        ViewData["Message"] = "Add successful!";
                    }
                    else
                    {
                        // If `id` is provided, update the existing record
                        var updateQuery = @"
                    UPDATE dentist_schedule 
                    SET users_id = @UsersId, services_id = @ServicesId, date_available = @DateAvailable, start_time = @StartTime, 
                        end_time = @EndTime WHERE id = @Id";

                        using (var updateCommand = new SqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@Id", id);
                            updateCommand.Parameters.AddWithValue("@UsersId", users_id);
                            updateCommand.Parameters.AddWithValue("@ServicesId", services_id);
                            updateCommand.Parameters.AddWithValue("@DateAvailable", date_available);
                            updateCommand.Parameters.AddWithValue("@StartTime", start_time);
                            updateCommand.Parameters.AddWithValue("@EndTime", end_time);

                            updateCommand.ExecuteNonQuery();
                        }

                        ViewData["Message"] = "Update successful!";
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError($"SQL Error during creation: {sqlEx.Message} \nStack Trace: {sqlEx.StackTrace}");
                ViewData["Message"] = $"Database error: {sqlEx.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"General Error during creation: {ex.Message} \nStack Trace: {ex.StackTrace}");
                ViewData["Message"] = "An unexpected error occurred during registration.";
            }

            return Redirect("/Admin/DentistSchedule");
        }


        [Route("Admin/DeleteDentistSchedule/{id}")]
        public IActionResult DeleteDentistSchedule(int id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if the record with the given id exists
                    var checkQuery = "SELECT COUNT(1) FROM dentist_schedule WHERE id = @Id";
                    using (var checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Id", id);
                        var existingCount = (int)checkCommand.ExecuteScalar();

                        if (existingCount == 0)
                        {
                            ViewData["Message"] = "No account found with the provided id.";
                            return View();
                        }
                    }

                    // If record exists, delete it
                    var deleteQuery = "DELETE FROM dentist_schedule WHERE id = @Id";
                    using (var deleteCommand = new SqlCommand(deleteQuery, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@Id", id);
                        deleteCommand.ExecuteNonQuery();
                    }

                    ViewData["Message"] = "Dentist Schedule deleted successfully!";
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError($"SQL Error during deletion: {sqlEx.Message} \nStack Trace: {sqlEx.StackTrace}");
                ViewData["Message"] = $"Database error: {sqlEx.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"General Error during deletion: {ex.Message} \nStack Trace: {ex.StackTrace}");
                ViewData["Message"] = "An unexpected error occurred during deletion.";
            }

            return View();
        }

        public IActionResult Addemployees()
        {
            return View();
        }

        [Route("Admin/Addemployees/{id}")]
        public IActionResult Addemployees(int id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");
            AccountModel account = null;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var query = "SELECT * FROM users WHERE id = @id";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                account = new AccountModel
                                {
                                    Id = reader["id"]?.ToString(),
                                    FirstName = reader["firstname"]?.ToString(),
                                    MiddleName = reader["middlename"]?.ToString(),
                                    LastName = reader["lastname"]?.ToString(),
                                    Email = reader["email"]?.ToString()
                                };
                            }
                        }
                    }
                }

                if (account == null)
                {
                    return NotFound(); // Return a 404 if no account is found
                }

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
        [Route("Admin/Addemployees/{id?}")]

        public IActionResult Addemployees(string id, string firstname, string middlename, string lastname, string email, string password, string type)
        {
            if (string.IsNullOrWhiteSpace(firstname) || string.IsNullOrWhiteSpace(middlename) || string.IsNullOrWhiteSpace(lastname) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(type))
            {
                ViewData["Message"] = "All fields are required.";
                return View();
            }

            var passwordHash = PasswordHelper.HashPassword(password);

            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if the email already exists in the database
                    var checkQuery = "SELECT COUNT(1) FROM users WHERE email = @Email";
                    using (var checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Email", email);
                        var existingCount = (int)checkCommand.ExecuteScalar();

                        if (existingCount > 0 && string.IsNullOrWhiteSpace(id))
                        {
                            ViewData["Message"] = "An account with this email already exists.";
                            return View();
                        }
                    }

                    if (string.IsNullOrWhiteSpace(id))
                    {
                        // If `id` is empty, insert a new record
                        var insertQuery = @"
                    INSERT INTO users (firstname, middlename, lastname, email, password, type) 
                    VALUES (@FirstName, @MiddleName, @LastName, @Email, @PasswordHash, @Type)";

                        using (var insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@FirstName", firstname);
                            insertCommand.Parameters.AddWithValue("@MiddleName", middlename);
                            insertCommand.Parameters.AddWithValue("@LastName", lastname);
                            insertCommand.Parameters.AddWithValue("@Email", email);
                            insertCommand.Parameters.AddWithValue("@PasswordHash", passwordHash);
                            insertCommand.Parameters.AddWithValue("@Type", type);

                            insertCommand.ExecuteNonQuery();
                        }

                        ViewData["Message"] = "Registration successful!";
                    }
                    else
                    {
                        // If `id` is provided, update the existing record
                        var updateQuery = @"
                    UPDATE users 
                    SET firstname = @FirstName, middlename = @MiddleName, lastname = @LastName, 
                        email = @Email, password = @PasswordHash, type = @Type
                    WHERE id = @Id";

                        using (var updateCommand = new SqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@Id", id);
                            updateCommand.Parameters.AddWithValue("@FirstName", firstname);
                            updateCommand.Parameters.AddWithValue("@MiddleName", middlename);
                            updateCommand.Parameters.AddWithValue("@LastName", lastname);
                            updateCommand.Parameters.AddWithValue("@Email", email);
                            updateCommand.Parameters.AddWithValue("@PasswordHash", passwordHash);
                            updateCommand.Parameters.AddWithValue("@Type", type);

                            updateCommand.ExecuteNonQuery();
                        }

                        ViewData["Message"] = "Update successful!";
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError($"SQL Error during registration: {sqlEx.Message} \nStack Trace: {sqlEx.StackTrace}");
                ViewData["Message"] = $"Database error: {sqlEx.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"General Error during registration: {ex.Message} \nStack Trace: {ex.StackTrace}");
                ViewData["Message"] = "An unexpected error occurred during registration.";
            }

            return Redirect("/Admin/Employees");
        }

        [Route("Admin/DeleteEmployee/{id}")]
        public IActionResult DeleteEmployee(int id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if the record with the given id exists
                    var checkQuery = "SELECT COUNT(1) FROM users WHERE id = @Id";
                    using (var checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Id", id);
                        var existingCount = (int)checkCommand.ExecuteScalar();

                        if (existingCount == 0)
                        {
                            ViewData["Message"] = "No account found with the provided id.";
                            return View();
                        }
                    }

                    // If record exists, delete it
                    var deleteQuery = "DELETE FROM users WHERE id = @Id";
                    using (var deleteCommand = new SqlCommand(deleteQuery, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@Id", id);
                        deleteCommand.ExecuteNonQuery();
                    }

                    ViewData["Message"] = "Employee deleted successfully!";
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError($"SQL Error during deletion: {sqlEx.Message} \nStack Trace: {sqlEx.StackTrace}");
                ViewData["Message"] = $"Database error: {sqlEx.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"General Error during deletion: {ex.Message} \nStack Trace: {ex.StackTrace}");
                ViewData["Message"] = "An unexpected error occurred during deletion.";
            }

            return View();
        }


        public IActionResult Employees()
        {
            var data = fetchData();
            ViewData["AccountList"] = data; // Pass data to the View.
            return View();
        }

        // Star Services
        public IActionResult Services()
        {
            var data = fetchDataServices();
            ViewData["ServicesList"] = data;
            return View();
        }
        public IActionResult AddServices()
        {
            return View();
        }

        [Route("Admin/AddServices/{id}")]
        public IActionResult AddServices(int id)
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
        [Route("Admin/AddServices/{id?}")]

        public IActionResult AddServices(string id, string title, string description)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
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

                    // Check if the email already exists in the database
                    var checkQuery = "SELECT COUNT(1) FROM services WHERE title = @Title";
                    using (var checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Title", title);
                        var existingCount = (int)checkCommand.ExecuteScalar();

                        if (existingCount > 0 && string.IsNullOrWhiteSpace(id))
                        {
                            ViewData["Message"] = "An account with this title already exists.";
                            return View();
                        }
                    }

                    if (string.IsNullOrWhiteSpace(id))
                    {
                        // If `id` is empty, insert a new record
                        var insertQuery = @" INSERT INTO services (title, description) VALUES (@Title, @Description)";

                        using (var insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@Title", title);
                            insertCommand.Parameters.AddWithValue("@Description", description);

                            insertCommand.ExecuteNonQuery();
                        }

                        ViewData["Message"] = "Registration successful!";
                    }
                    else
                    {
                        // If `id` is provided, update the existing record
                        var updateQuery = @"
                    UPDATE services SET title = @Title, description = @Description WHERE id = @Id";

                        using (var updateCommand = new SqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@Id", id);
                            updateCommand.Parameters.AddWithValue("@Title", title);
                            updateCommand.Parameters.AddWithValue("@Description", description);

                            updateCommand.ExecuteNonQuery();
                        }

                        ViewData["Message"] = "Update successful!";
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError($"SQL Error during creation: {sqlEx.Message} \nStack Trace: {sqlEx.StackTrace}");
                ViewData["Message"] = $"Database error: {sqlEx.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"General Error during creation: {ex.Message} \nStack Trace: {ex.StackTrace}");
                ViewData["Message"] = "An unexpected error occurred during creation.";
            }

            return Redirect("/Admin/Services");
        }

        [Route("Admin/DeleteServices/{id}")]
        public IActionResult DeleteServices(int id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if the record with the given id exists
                    var checkQuery = "SELECT COUNT(1) FROM services WHERE id = @Id";
                    using (var checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Id", id);
                        var existingCount = (int)checkCommand.ExecuteScalar();

                        if (existingCount == 0)
                        {
                            ViewData["Message"] = "No account found with the provided id.";
                            return View();
                        }
                    }

                    // If record exists, delete it
                    var deleteQuery = "DELETE FROM services WHERE id = @Id";
                    using (var deleteCommand = new SqlCommand(deleteQuery, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@Id", id);
                        deleteCommand.ExecuteNonQuery();
                    }

                    ViewData["Message"] = "Services deleted successfully!";
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError($"SQL Error during deletion: {sqlEx.Message} \nStack Trace: {sqlEx.StackTrace}");
                ViewData["Message"] = $"Database error: {sqlEx.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"General Error during deletion: {ex.Message} \nStack Trace: {ex.StackTrace}");
                ViewData["Message"] = "An unexpected error occurred during deletion.";
            }

            return View();
        }

        private List<ServicesModel> fetchDataServices()
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
        // End Services


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
                                    Id = reader["id"]?.ToString() ?? "Unknown",
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


        private List<AccountModel> fetchDataDentist()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");
            var dataList = new List<AccountModel>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM users WHERE type = 1";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = new AccountModel
                                {
                                    Id = reader["id"]?.ToString() ?? "Unknown",
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


        // Start Patient Schedule
        public IActionResult PatientSchedule()
        {

            var data = fetchDataPatientSchedule();
            ViewData["PatientScheduleList"] = data;
            return View();
        }

        private List<PatientScheduleModel> fetchDataPatientSchedule()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");
            var dataList = new List<PatientScheduleModel>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT *,schedule_appointment.id AS schedule_appointment_id FROM schedule_appointment INNER JOIN dentist_schedule ON schedule_appointment.dentist_schedule_id=dentist_schedule.id INNER JOIN users ON schedule_appointment.users_id=users.id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = new PatientScheduleModel
                                {
                                    Id = reader["schedule_appointment_id"]?.ToString() ?? "Unknown",
                                    UsersId = reader["users_id"]?.ToString() ?? "Unknown",
                                    DentistScheduleId = reader["dentist_schedule_id"]?.ToString() ?? "Unknown",
                                    DateAvailable = reader["date_available"]?.ToString() ?? "Unknown",
                                    AppointmentTime = reader["appointment_time"]?.ToString() ?? "Unknown",
                                    FirstName = reader["firstname"]?.ToString() ?? "Unknown",
                                    LastName = reader["lastname"]?.ToString() ?? "Unknown",
                                    Status = reader["status"]?.ToString() ?? "Unknown",
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
        [HttpPost]
        public IActionResult PatientSchedule(string patient_schedule_id, string status)
        {
            if (string.IsNullOrWhiteSpace(patient_schedule_id) || string.IsNullOrWhiteSpace(status))
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


                    var updateQuery = @"
                    UPDATE schedule_appointment SET status = @Status WHERE id = @Id";

                        using (var updateCommand = new SqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@Id", patient_schedule_id);
                            updateCommand.Parameters.AddWithValue("@Status", status);

                            updateCommand.ExecuteNonQuery();
                        }

                        ViewData["Message"] = "Update successful!";
                    
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError($"SQL Error during creation: {sqlEx.Message} \nStack Trace: {sqlEx.StackTrace}");
                ViewData["Message"] = $"Database error: {sqlEx.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"General Error during creation: {ex.Message} \nStack Trace: {ex.StackTrace}");
                ViewData["Message"] = "An unexpected error occurred during creation.";
            }

            return Redirect("/Admin/PatientSchedule");
        }
        // End Patient Schedule


    }
}
