using System;

namespace iX1LogAnalyser
{
    public class Classes
    {
        public class oldTOD
        {
            public DateTime Time { get; set; }
            public string App { get; set; } = "";
            public string Action { get; set; } = "";
            public int Run { get; set; }
            public int Transaction { get; set; }
            public int? Boxes { get; set; }
            public int? Discovered { get; set; }
            public int? Targetted { get; set; }
            public int? Suceeded { get; set; }
            public int? Failed => (Targetted != null && Suceeded != null) ? Targetted - Suceeded : null;
            public string Comments { get; set; } = "";
            public DateTime? ActionTime { get; set; }
            public DateTime? RunTime { get; set; }
        }

        public class TOD
        {
            public string Type { get; set; }
            public string App { get; set; } = "";
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public TimeSpan Elapsed => End - Start;
            public int? Dev0 { get; set; }
            public int? DevN { get; set; }
            public int? DevConf { get; set; }
            public int? SkewDetermineNoR { get; set; }
            public int? Boxes { get; set; }
            public int? ConfBoxes { get; set; }
            public string Comments { get; set; } = "";
        }

        public class LogBreak
        {
            public string Type { get; set; } = "Line Unknown";

            /** LineBreak spotted*/
            public DateTime? T1 { get; set; }
            /** Next end of Shooting */
            public DateTime? T2 { get; set; }

            /** Start Shooting 1*/
            public DateTime? T3 { get; set; }
            /** End of Shooting 1*/
            public DateTime? T4 { get; set; }
            public TimeSpan? DurationTroubleShooting =>
                T3.HasValue && T2.HasValue ? new TimeSpan(Math.Abs((T3.Value - T2.Value).Ticks)) : null;

            public TimeSpan? DurationRecord =>
                T4.HasValue && T3.HasValue ? new TimeSpan(Math.Abs((T4.Value - T3.Value).Ticks)) : null;
            /** Start Shooting 2 */
            public DateTime? T5 { get; set; }
            /** End of Shooting 2*/
            public DateTime? T6 { get; set; }
            public TimeSpan? DurationTroubleShooting2 =>
                T5.HasValue && T2.HasValue ? new TimeSpan(Math.Abs((T5.Value - T2.Value).Ticks)) : null;

            public int? Line { get; set; }
            public int? Point { get; set; }
            public string Comments { get; set; } = "";
        }

        public class SortItem 
        { 
            public int? DebugLine { get; set; }
            public int? MSGid { get; set; }

            public string Device { get; set; } = "";
            public string Serial { get; set; } = "";
            public bool Truckside { get; set; }
            public char TruckPort { get; set; }
            public string Port { get; set; } = "";
            public int? Packets { get; set; }
            public int? CRC { get; set; }
            public int? Overflow { get; set; }
            public int? Timeout { get; set; }
            public TimeSpan? Uptime { get; set; }
            public int? Line { get; set; }
            public int? Flag { get; set; }
            public string Comment{ get; set; } = "";
            public double? x { get; set; }
            public double? y { get; set; }
        }

        public class Retry
        {
            public DateTime DateTime { get; set; }

            public string App { get; set; } = "";
            public string Activity { get; set; } = "";
            public string Serial { get; set; } = "";

            public string Status { get; set; } = "";
            public int? Line { get; set; }
            public int? Flag { get; set; }
            public string? Comment { get; set; }
        }
    }
}
