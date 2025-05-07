using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyCycleCLI.Model
{
    internal class StudyCycle
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public double DailyStudyHours { get; set; }
        public double WeeklyStudyHours { get; set; }
        public int FactorOne { get; set; }
        public int FactorTwo { get; set; }
        public int FactorThree { get; set; }
        public int CompletedTimes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastStudiedAt { get; set; }

        public StudyCycle() { }
    }
}
