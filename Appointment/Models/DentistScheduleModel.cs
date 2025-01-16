using Microsoft.AspNetCore.Mvc;

namespace Appointment.Models
{
    public class DentistScheduleModel : Controller
    {
        public string Id { get; set; } = string.Empty;
        public string UsersId { get; set; } = string.Empty;
        public string ServicesId { get; set; } = string.Empty;
        public string DateAvailable { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }


}
