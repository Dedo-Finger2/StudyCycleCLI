using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyCycleCLI.Model
{
    internal class StudyCycleSubject
    {
        public int Id { get; set; }
        public int StudyCycleId { get; set; }
        public string Title { get; set; }
        public Difficulty Difficulty { get; set; }
        public AmountOfContent AmountOfContent { get; set; }
        public double Weight { get; set; }
        public int StudiedHours { get; set; }
        public int MaxStudyHours { get; set; }
        public int CompletedTimes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastStudiedAt { get; set; }
    }
}
