using Microsoft.AspNetCore.Mvc;

namespace Appointment.Models
{
    public class PatientMedicalHistoryModel : Controller
    {
        public string Id { get; set; } = string.Empty;
        public string UsersId { get; set; } = string.Empty;
        public string QOne { get; set; } = string.Empty;
        public string QTwo { get; set; } = string.Empty;
        public string QThree { get; set; } = string.Empty;
        public string QFour { get; set; } = string.Empty;
        public string QFive { get; set; } = string.Empty;

    }


}
