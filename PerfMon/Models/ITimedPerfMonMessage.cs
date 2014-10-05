using System;

namespace Glimpse.Orchard.PerfMon.Models
{
    public interface ITimedPerfMonMessage
    {
        TimeSpan Duration { get; set; }
        TimeSpan Offset { get; set; }
        DateTime StartTime { get; set; }
    }
}