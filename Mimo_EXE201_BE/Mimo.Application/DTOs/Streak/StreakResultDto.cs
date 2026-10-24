using System;

namespace Mimo.Application.DTOs.Streak
{
    public class StreakResultDto
    {
        public int Streak { get; set; }

        /// <summary>
        /// Maintained | Increased | Started | Frozen | Reset
        /// </summary>
        public string Status { get; set; } = "Maintained";

        public bool FreezeConsumed { get; set; }

        public int FreezesRemaining { get; set; }

        public DateOnly? LastActiveDate { get; set; }

        public DateOnly CurrentDate { get; set; }
    }
}
