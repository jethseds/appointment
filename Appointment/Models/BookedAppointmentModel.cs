using Microsoft.AspNetCore.Mvc;

namespace Appointment.Models
{
    public class BookedAppointmentModel : Controller
    {
        public string Id { get; set; } = string.Empty;
        public string UsersId { get; set; } = string.Empty;
        public string ServicesId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string DentistId { get; set; } = string.Empty;
        public string DateAvailable { get; set; } = string.Empty;
        public string AppointmentTime { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }


}
