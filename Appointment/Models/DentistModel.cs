using Microsoft.AspNetCore.Mvc;

namespace Appointment.Models
{
    public class DentistModel : Controller
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }


}
