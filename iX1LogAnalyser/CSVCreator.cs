using DevExpress.Mvvm.Native;
using DevExpress.Xpf.Editors.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static iX1LogAnalyser.Classes;

namespace iX1LogAnalyser
{
    public class CSVCreator
    {
        bool NewVersion { get; set; } = false;

        string WhatVersion { get; set; }
        #region TOD

        List<TOD> TODs { get; set; } = new List<TOD>();
        List<oldTOD> oldTODs { get; set; } = new List<oldTOD>();

        

        public string CreateTODFile(string filename, string path)
        {
            try
            {
                TODs = new List<TOD>();
                oldTODs = new List<oldTOD>();

                TOD? todglobal = null;
                TOD? todlocal = null;

                oldTOD? runstart = null;
                oldTOD? runend = null;
                oldTOD? discover = null;
                oldTOD? todGlobal = null;
                oldTOD? todLocal = null;

                int run = 0; int transaction = 0;

                using (var fStream = File.OpenRead(filename))
                {
                    using (var stream = new StreamReader(fStream, Encoding.UTF8, true))
                    {
                        String? line;
                        String[] row = { };

                        while ((line = stream.ReadLine()) != null)
                        {
                            row = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (row.Count() < 5)
                                continue;
                            var version = Regex.Split(row[4], @"\t+|[().]");
                            version = Array.FindAll(version, item => !string.IsNullOrEmpty(item));
                            if (version.Count() > 50 || version.Count() < 3 || version.Where(item => int.TryParse(item, out _)).Count() != version.Count())
                                continue;

                            if (int.Parse(version[0]) >= 8 && int.Parse(version[1]) >= 3 && int.Parse(version[2]) >= 3)
                            {
                                NewVersion = true;
                                WhatVersion = row[4].Replace(")", "");
                                switch (line)
                                {
                                    case string l1 when l1.Contains("TOD config try") && !l1.Contains("completed") && !l1.Contains("Enable VLan") && !l1.Contains("Disable VLan"):
                                        todlocal = new TOD();
                                        todlocal.Type = l1.Contains("Localized") ? $"Localized TOD {row[9]}" : $"Localized TOD {row[8]}";
                                        todlocal.App = row[2];
                                        todlocal.Start = DateTime.Parse($"{row[0]} {row[1]}");
                                        break;
                                    case string l2 when l2.Contains("Start device Time-of-day verification"):
                                        todglobal = new TOD();
                                        if (int.Parse(version[0]) >= 8 && int.Parse(version[1]) >= 3 && int.Parse(version[2]) >= 4)
                                            todglobal.Type = "Localized TOD";
                                        else
                                            todglobal.Type = "Global TOD";
                                        todglobal.App = row[2];
                                        todglobal.Start = DateTime.Parse($"{row[0]} {row[1]}");
                                        break;
                                    case string l3 when l3.Contains("Config Dev0:") && l3.Contains("and Devn:"):
                                        if (todlocal != null)
                                        {
                                            if (todlocal.Dev0 == null)
                                                todlocal.Dev0 = int.Parse(row[7].Replace(",", ""));
                                            if (todlocal.DevN == null)
                                                todlocal.DevN = int.Parse(row[row.Length - 1]);
                                        }
                                        else if (todglobal != null)
                                        {
                                            if (todglobal.Dev0 == null)
                                                todglobal.Dev0 = int.Parse(row[7].Replace(",", ""));
                                            if (todglobal.DevN == null)
                                                todglobal.DevN = int.Parse(row[row.Length - 1]);
                                        }
                                        break;
                                    case string l4 when l4.Contains("NoResponse:") && l4.Contains("StatusFail:"):
                                        if (todlocal != null)
                                            todlocal.SkewDetermineNoR = int.Parse(row[6]);
                                        else if (todglobal != null)
                                            todglobal.SkewDetermineNoR = int.Parse(row[6]);
                                        break;
                                    case string l5 when l5.Contains("Run") && l5.Contains("Total boxes"):
                                        if (todlocal != null)
                                        {
                                            if (todlocal.Boxes == null)
                                                todlocal.Boxes = int.Parse(row[9].Replace(",", ""));
                                            if (todlocal.ConfBoxes == null)
                                                todlocal.ConfBoxes = int.Parse(row[row.Length - 1]);
                                            todlocal.DevConf = int.Parse(row[11]);
                                        }
                                        else if (todglobal != null)
                                        {
                                            if (todglobal.Boxes == null)
                                                todglobal.Boxes = int.Parse(row[9].Replace(",", ""));
                                            if (todglobal.ConfBoxes == null)
                                                todglobal.ConfBoxes = int.Parse(row[row.Length - 1]);
                                            todglobal.DevConf = int.Parse(row[11]);
                                        }
                                        break;
                                    case string l6 when l6.Contains("TOD config try") && l6.Contains("completed"):
                                        if (todlocal != null)
                                        {
                                            todlocal.End = DateTime.Parse($"{row[0]} {row[1]}");
                                            TODs.Add(todlocal);
                                            todlocal = null;
                                        }
                                        break;
                                    case string l7 when l7.Contains("Complete. Total boxes:"):
                                        if (todlocal != null)
                                        {
                                            todlocal.Boxes = int.Parse(row[8]);
                                            todlocal.ConfBoxes = int.Parse(row[row.Length - 1]);
                                        }
                                        else if (todglobal != null)
                                        {
                                            todglobal.Boxes = int.Parse(row[8]);
                                            todglobal.ConfBoxes = int.Parse(row[row.Length - 1]);
                                            todglobal.End = DateTime.Parse($"{row[0]} {row[1]}");
                                            TODs.Add(todglobal);
                                            todglobal = null;
                                        }
                                        break;
                                    case string l8 when l8.Contains("LocalizedTodConfiguration"):
                                        if (todglobal != null)
                                        {
                                            todglobal.End = DateTime.Parse($"{row[0]} {row[1]}");
                                            TODs.Add(todglobal);
                                            todglobal = null;
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                NewVersion = false;
                                WhatVersion = row[4].Replace(")","");
                                switch (line)
                                {
                                    case string l1 when l1.Contains("Run test...") || l1.Contains("iX1 Map started"):
                                        if (runend != null)
                                        {
                                            oldTODs.Add(runend);
                                            runend = null;
                                        }
                                        var time = DateTime.Parse(row[0] + " " + row[1]);

                                        run++;
                                        transaction = 0;

                                        runstart = new oldTOD()
                                        {
                                            App = row[2],
                                            Action = "Run start",
                                            Run = run,
                                            Transaction = transaction,
                                            Time = time
                                        };

                                        oldTODs.Add(runstart);
                                        break;
                                    case string l2 when l2.Contains("TOD... END"):
                                        if (runstart != null)
                                        {
                                            time = DateTime.Parse(row[0] + " " + row[1]);
                                            var latestrunstart = oldTODs.Where(tod => tod.Action == "Run start").LastOrDefault() ?? runstart;
                                            runend = new oldTOD()
                                            {
                                                App = row[2],
                                                Action = "Run end",
                                                Run = run,
                                                Transaction = transaction,
                                                Time = time,
                                                RunTime = new DateTime((time - latestrunstart.Time).Ticks)
                                            };
                                        }
                                        break;
                                    case string l3 when l3.Contains("DiscoverNetworkMp"):
                                        if (discover == null)
                                        {
                                            time = DateTime.Parse(row[0] + " " + row[1]);
                                            discover = new oldTOD()
                                            {   
                                                App = row[2],
                                                Action = "Discover done",
                                                Run = run,
                                                Transaction = transaction,
                                                Time = time,
                                                ActionTime = time
                                            };
                                        }
                                        else
                                        {
                                            if (oldTODs.Count() > 2 && oldTODs.ElementAt(oldTODs.Count - 2).Action == "Run end" && runstart != null && oldTODs.ElementAt(oldTODs.Count - 1) == runstart)
                                            {
                                                oldTODs.Remove(discover);
                                                oldTODs.Remove(runstart);
                                                oldTODs = oldTODs.SkipLast(1).ToList();
                                            }

                                        }
                                        break;
                                    case string l4 when l4.Contains("boxes in defined MpNetwork"):
                                        time = DateTime.Parse(row[0] + " " + row[1]);
                                        if (discover != null)
                                        {
                                            discover.Boxes = Int32.Parse(row[5]);
                                            discover.Discovered = Int32.Parse(row[11]);
                                            discover.ActionTime = new DateTime((time - discover.Time).Ticks);

                                            if (oldTODs.LastOrDefault() != discover)
                                                oldTODs.Add(discover);
                                        }
                                        break;
                                    case string l5 when l5.Contains("TOD configuration processor"):
                                        var vlan = row[13].Remove(1);
                                        bool global = Int32.Parse(vlan) == 0;
                                        time = DateTime.Parse(row[0] + " " + row[1]);
                                        if (global)
                                        {
                                            transaction = 1;
                                            todGlobal = new oldTOD()
                                            {   
                                                App = row[2],
                                                Action = "TOD global done",
                                                Run = run,
                                                Transaction = transaction,
                                                Time = time,
                                                ActionTime = time
                                            };
                                        }
                                        else
                                        {
                                            transaction++;
                                            todLocal = new oldTOD()
                                            {
                                                App = row[2],
                                                Action = "TOD local done",
                                                Run = run,
                                                Transaction = transaction,
                                                Time = time,
                                                ActionTime = time
                                            };
                                        }
                                        break;
                                    case string l6 when l6.Contains("TOD config VLan") && l6.Contains(": Complete"):
                                        vlan = row[8].Remove(1);
                                        global = Int32.Parse(vlan) == 0;
                                        time = DateTime.Parse(row[0] + " " + row[1]);
                                        if (discover != null)
                                        {
                                            discover = null;
                                        }

                                        if (global && todglobal != null)
                                        {
                                            todGlobal.Targetted = Int32.Parse(row[12]);
                                            todGlobal.Suceeded = Int32.Parse(row[15]);
                                            todGlobal.ActionTime = new DateTime((time - todGlobal.Time).Ticks);

                                            oldTODs.Add(todGlobal);
                                            todGlobal = null;
                                        }
                                        else if (todlocal != null)
                                        {
                                            todLocal.Targetted = Int32.Parse(row[12]);
                                            todLocal.Suceeded = Int32.Parse(row[15]);
                                            todLocal.ActionTime = new DateTime((time - todLocal.Time).Ticks);

                                            oldTODs.Add(todLocal);
                                            todLocal = null;
                                        }
                                        break;
                                }
                            }
                            
                        }
                    }
                    if (runend != null)
                    {
                        if (discover != null)
                        {
                            discover = null;
                        }
                        oldTODs.Add(runend);
                        runend = null;
                    }
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }

            try
            {
                using (TextWriter writer = File.CreateText(path))
                {
                    WriteHeaders(writer);
                    string data = "";
                    if(NewVersion)
                        foreach (var tod in TODs)
                        {
                            data = FormatDataItem(tod);
                            writer.WriteLine(data);
                        }
                    else
                    {
                        foreach (var tod in oldTODs)
                        {
                            data = OldFormatDataItem(tod);
                            writer.WriteLine(data);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return "";
        }

        private string FormatDataItem(TOD item)
        {
            string data = "";
            for (var field = TOD_FIELDS.APP; field < TOD_FIELDS.FIELDS_END; field++)
            {
                switch (field)
                {
                    case TOD_FIELDS.APP:
                        data += item.App + ",";
                        break;
                    case TOD_FIELDS.CHOICE:
                        data += item.Type + ",";
                        break;
                    case TOD_FIELDS.START:
                        data += item.Start.ToString("HH:mm:ss") + ",";
                        break;
                    case TOD_FIELDS.END:
                        data += item.End.ToString("HH:mm:ss") + ",";
                        break;
                    case TOD_FIELDS.ELASPED:
                        data += item.Elapsed.TotalSeconds + ",";
                        break;
                    case TOD_FIELDS.DEV0:
                        data += item.Dev0 == null ? "N/A," :item.Dev0 + ",";
                        break;
                    case TOD_FIELDS.DEVN:
                        data += item.DevN == null ? "N/A," : item.DevN + ",";
                        break;
                    case TOD_FIELDS.DEV_CONF:
                        data += item.Dev0 == null ? "N/A," : item.DevConf + ",";
                        break;
                    case TOD_FIELDS.SKEWDETERMINE:
                        data += item.SkewDetermineNoR + ",";
                        break;
                    case TOD_FIELDS.CONF_BOXES:
                        data += item.ConfBoxes + ",";
                        break;
                    case TOD_FIELDS.TOTAL_BOXES:
                        data += item.Boxes + ",";
                        break;
                    case TOD_FIELDS.COMMENTS:
                        data += item.Comments + ",";
                        break;
                    case TOD_FIELDS.FIELDS_END:
                        break;
                }
            }
            return data;
        }

        private enum TOD_FIELDS
        {
            APP,
            CHOICE,
            START,
            END,
            ELASPED,
            DEV0,
            DEVN,
            DEV_CONF,
            SKEWDETERMINE,
            TOTAL_BOXES,
            CONF_BOXES,
            COMMENTS,
            FIELDS_END
        };

        private string[] TOD_STRING = new string[(int)TOD_FIELDS.FIELDS_END]
        {
            "App",
            "Type",
            "Start",
            "End",
            "Elapsed Time",
            "Dev 0",
            "Dev n",
            "Dev0 and DevN Configured",
            "SkewDetermine No Response",
            "Total Boxes",
            "Configured Boxes",
            "Comments"
        };

        private string OldFormatDataItem(oldTOD item)
        {
            string data = "";
            for (var field = OLD_TOD_FIELDS.APP; field < OLD_TOD_FIELDS.FIELDS_END; field++)
            {
                switch (field)
                {
                    case OLD_TOD_FIELDS.APP:
                        data += item.App + ",";
                        break;
                    case OLD_TOD_FIELDS.RUN_NB:
                        data += item.Run + ",";
                        break;
                    case OLD_TOD_FIELDS.TOD_TRANSACTION:

                        data += item.Transaction + ",";
                        break;
                    case OLD_TOD_FIELDS.ACTION:
                        data += item.Action + ",";
                        break;
                    case OLD_TOD_FIELDS.WHEN:
                        data += item.Time.ToString("HH:mm:ss") + ",";
                        break;
                    case OLD_TOD_FIELDS.ACTION_TIME:
                        data += item.ActionTime.HasValue ? item.ActionTime.Value.ToString("HH:mm:ss") + "," : ",";
                        break;
                    case OLD_TOD_FIELDS.RUN_TIME:
                        data +=  item.RunTime.HasValue ? item.RunTime.Value.ToString("HH:mm:ss") + "," : ",";
                        break;
                    case OLD_TOD_FIELDS.BOXES:
                        data += item.Boxes + ",";
                        break;
                    case OLD_TOD_FIELDS.DISCOVERED:
                        data += item.Discovered + ",";
                        break;
                    case OLD_TOD_FIELDS.TOD_TARGETTED:
                        data += item.Targetted + ",";
                        break;
                    case OLD_TOD_FIELDS.TOD_SUCCEEDED:
                        data += item.Suceeded + ",";
                        break;
                    case OLD_TOD_FIELDS.TOD_FAILED:
                        data += item.Failed + ",";
                        break;
                    case OLD_TOD_FIELDS.COMMENTS:
                        data += item.Comments + ",";
                        break;
                    case OLD_TOD_FIELDS.FIELDS_END:
                        break;
                }
            }
            return data;
        }

        private enum OLD_TOD_FIELDS
        {
            APP,
            RUN_NB,
            TOD_TRANSACTION,
            ACTION,
            WHEN,
            ACTION_TIME,
            RUN_TIME,
            BOXES,
            DISCOVERED,
            TOD_TARGETTED,
            TOD_SUCCEEDED,
            TOD_FAILED,
            COMMENTS,
            FIELDS_END
        };

        private string[] OLD_TOD_STRING = new string[(int)OLD_TOD_FIELDS.FIELDS_END]
        {   
            "App",
            "Run #",
            "TOD Transaction #",
            "Action",
            "When",
            "Action Duration",
            "Run Duration",
            "Boxes",
            "Discovered",
            "TOD Targeted",
            "TOD Succeeded",
            "TOD Failed",
            "Comments"
        };
        #endregion

        #region Line Break


        public int RAMCount = 0, PSUCount = 0, FTUCount = 0;
        List<LogBreak> LogBreaks { get; set; } = new List<LogBreak>();
        Dictionary<string, (int d, int b)> Stats { get; set; } = new Dictionary<string, (int d, int b)>();
        public string CreateLogBreak(string filename, string path)
        {
            try
            {
                LogBreaks = new List<LogBreak>();
                Stats = new Dictionary<string, (int d, int b)>();
                using (var fStream = File.OpenRead(filename))
                {
                    using (var stream = new StreamReader(fStream, Encoding.UTF8, true))
                    {
                        String? line;
                        String[] row;
                        LogBreak? logBreak = null;

                        while ((line = stream.ReadLine()) != null)
                        {
                            row = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (row.Count() > 2 && row[2] == "iX1Vib")
                            {
                                switch (line)
                                {
                                    case string l1 when l1.Contains("Line break"):
                                        if (l1.Contains("between"))
                                        {
                                            if (logBreak != null)
                                            {
                                                logBreak.Type = "SameEvent";
                                                LogBreaks.Add(logBreak);
                                                logBreak = null;
                                            }

                                            if (!l1.Contains("UNKNOWN"))
                                            {
                                                logBreak = new LogBreak();
                                                string checker = "";
                                                if (row[8].Contains("R"))
                                                    checker += "R";
                                                else if (row[8].Contains("P"))
                                                    checker += "P";
                                                else if (row[8].Contains("T"))
                                                    checker += "T";

                                                if (row[10].Contains("R"))
                                                    checker += "R";
                                                else if (row[10].Contains("P"))
                                                    checker += "P";
                                                else if (row[10].Contains("T"))
                                                    checker += "T";

                                                if (checker.Count() >= 2)
                                                    logBreak.Comments = "Recheck only " + checker;
                                                logBreak.T1 = new DateTime(row[0].TryConvertToDateTime().Ticks + row[1].TryConvertToDateTime().Ticks);
                                                if (row.Count() > 14 && row[14].Contains("/") && row[16].Contains("/"))
                                                {
                                                    logBreak.Line = (Int32)Double.Parse(row[14].Split("/")[0]);
                                                    logBreak.Point = (Int32)Double.Parse(row[16].Split("/")[0]);
                                                }


                                            }
                                            else
                                            {
                                                logBreak = new LogBreak();
                                                logBreak.Comments = "Unknown Location or Equipment";
                                                logBreak.T1 = new DateTime(row[0].TryConvertToDateTime().Ticks + row[1].TryConvertToDateTime().Ticks);
                                            }
                                        }

                                        break;
                                    case string l2 when l2.Contains("Ending Slipsweep"):
                                        var created = false;
                                        if (logBreak != null)
                                        {
                                            if (logBreak.T2 == null && logBreak.Comments != "Unknown Location or Equipment")
                                                logBreak.T2 = new DateTime(row[0].TryConvertToDateTime().Ticks + row[1].TryConvertToDateTime().Ticks);

                                            LogBreaks.Add(logBreak);
                                            logBreak = null;
                                            created = true;
                                        }
                                        foreach (var log in LogBreaks)
                                        {
                                            if (log.T2 != null && !created)
                                            {
                                                if (log.T5 != null && log.T6 == null && log.DurationRecord < TimeSpan.FromMinutes(4))
                                                    log.T6 = new DateTime(row[0].TryConvertToDateTime().Ticks + row[1].TryConvertToDateTime().Ticks);
                                                else if (log.T3 != null && log.T4 == null)
                                                    log.T4 = new DateTime(row[0].TryConvertToDateTime().Ticks + row[1].TryConvertToDateTime().Ticks);

                                                if (log.DurationTroubleShooting > TimeSpan.FromMinutes(10) || log.DurationTroubleShooting2 > TimeSpan.FromMinutes(10))
                                                    log.Type = "Line Break";
                                                else
                                                    log.Type = "Line Drop";
                                            }
                                        }
                                        break;
                                    case string l3 when l3.Contains("Enable All Sub Lines"):
                                        foreach (var log in LogBreaks)
                                        {
                                            if (log.T4 != null && log.T5 == null && log.DurationRecord < TimeSpan.FromMinutes(4))
                                                log.T5 = new DateTime(row[0].TryConvertToDateTime().Ticks + row[1].TryConvertToDateTime().Ticks);
                                            else if (log.T2 != null && log.T3 == null)
                                                log.T3 = new DateTime(row[0].TryConvertToDateTime().Ticks + row[1].TryConvertToDateTime().Ticks);
                                        }
                                        break;
                                    case string l4 when l4.Contains("to todBoxList"):
                                        if (row[6].Contains("R"))
                                            RAMCount++;
                                        else if (row[6].Contains("T"))
                                            FTUCount++;
                                        else if (row[6].Contains("P"))
                                            PSUCount++;
                                        break;
                                }
                            }
                        }
                        if (logBreak != null)
                        {
                            LogBreaks.Add(logBreak);
                            logBreak = null;
                        }

                        var copyLogs = LogBreaks.ToList();
                        foreach (var log in LogBreaks)
                        {
                            if (log.T3 != null && log.T4 == null)
                                log.Comments = "No End of Acquisition";
                            var type = copyLogs.FirstOrDefault(item => item.Type.Contains("Break") || item.Type.Contains("Drop") || item.Type.Contains("Line Unknown"));
                            if (log.Type.Contains("SameEvent") && type != null)
                                log.Type = type.Type;
                            else if (log.Type.Contains("SameEvent"))
                            {
                                log.Type = "Line Unknown";
                            }
                            copyLogs.Remove(log);
                        }
                    }
                }

                //Statistics   
                Stats.Add("Lines", (LogBreaks.Where(item => item.Type.Contains("Drop")).Count(), LogBreaks.Where(item => item.Type.Contains("Break")).Count()));
                Stats.Add("RR", (LogBreaks.Where(item => item.Type.Contains("Drop") && item.Comments.Contains("RR")).Count(),
                    LogBreaks.Where(item => item.Type.Contains("Break") && item.Comments.Contains("RR")).Count()));
                Stats.Add("PR", (LogBreaks.Where(item => item.Type.Contains("Drop") && item.Comments.Contains("PR")).Count(),
                    LogBreaks.Where(item => item.Type.Contains("Break") && item.Comments.Contains("PR")).Count()));
                Stats.Add("RP", (LogBreaks.Where(item => item.Type.Contains("Drop") && item.Comments.Contains("RP")).Count(),
                    LogBreaks.Where(item => item.Type.Contains("Break") && item.Comments.Contains("RP")).Count()));
                Stats.Add("PP", (LogBreaks.Where(item => item.Type.Contains("Drop") && item.Comments.Contains("PP")).Count()
                    , LogBreaks.Where(item => item.Type.Contains("Break") && item.Comments.Contains("PP")).Count()));
                Stats.Add("TT", (LogBreaks.Where(item => item.Type.Contains("Drop") && item.Comments.Contains("TT")).Count()
                    , LogBreaks.Where(item => item.Type.Contains("Break") && item.Comments.Contains("TT")).Count()));
                Stats.Add("TR", (LogBreaks.Where(item => item.Type.Contains("Drop") && item.Comments.Contains("TR")).Count()
                    , LogBreaks.Where(item => item.Type.Contains("Break") && item.Comments.Contains("TR")).Count()));
                Stats.Add("RT", (LogBreaks.Where(item => item.Type.Contains("Drop") && item.Comments.Contains("RT")).Count()
                    , LogBreaks.Where(item => item.Type.Contains("Break") && item.Comments.Contains("RT")).Count()));
                Stats.Add("PT", (LogBreaks.Where(item => item.Type.Contains("Drop") && item.Comments.Contains("PT")).Count()
                    , LogBreaks.Where(item => item.Type.Contains("Break") && item.Comments.Contains("PT")).Count()));
                Stats.Add("TP", (LogBreaks.Where(item => item.Type.Contains("Drop") && item.Comments.Contains("TP")).Count()
                    , LogBreaks.Where(item => item.Type.Contains("Break") && item.Comments.Contains("TP")).Count()));

            }
            catch (Exception e)
            {
                return e.ToString();
            }

            try
            {
                using (TextWriter writer = File.CreateText(path))
                {
                    WriteHeaders(writer, "log");
                    string data = "";
                    foreach (var log in LogBreaks)
                    {
                        data = FormatDataItem(log);
                        writer.WriteLine(data);
                    }
                    WriteStats(writer);
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return "";
        }





        private string FormatDataItem(LogBreak item)
        {
            string data = "";
            for (var field = LOG_FIELDS.TYPE; field < LOG_FIELDS.FIELDS_END; field++)
            {
                switch (field)
                {

                    case LOG_FIELDS.TYPE:
                        data += item.Type + ",";
                        break;
                    case LOG_FIELDS.BREAK:
                        data += $"{(item.T1.HasValue ? item.T1.Value.ToString("HH:mm:ss") : "")},";
                        break;
                    case LOG_FIELDS.START:
                        data += $"{(item.T2.HasValue ? item.T2.Value.ToString("HH:mm:ss") : "") },";
                        break;
                    case LOG_FIELDS.END:
                        data += $"{(item.T5.HasValue ? item.T5.Value.ToString("HH:mm:ss") : (item.T3.HasValue ? item.T3.Value.ToString("HH:mm:ss") : "")) },";
                        break;
                    case LOG_FIELDS.DURATION:
                        data += item.DurationTroubleShooting2.HasValue ? new DateTime(item.DurationTroubleShooting2.Value.Ticks).ToString("HH:mm:ss") + "," : (item.DurationTroubleShooting.HasValue ? new DateTime(item.DurationTroubleShooting.Value.Ticks).ToString("HH:mm:ss") + "," : ",");
                        break;
                    case LOG_FIELDS.LINE:
                        data += $"{ item.Line.ToString() ?? "unknown" },";
                        break;
                    case LOG_FIELDS.POINT:
                        data += $"{ item.Point.ToString() ?? "unknown" },";
                        break;
                    case LOG_FIELDS.COMMENTS:
                        data += item.Comments + ",";
                        break;
                    case LOG_FIELDS.FIELDS_END:
                        break;
                }
            }
            return data;
        }

        private enum LOG_FIELDS
        {
            TYPE,
            BREAK,
            START,
            END,
            DURATION,
            LINE,
            POINT,
            COMMENTS,
            FIELDS_END
        };

        private string[] LOG_STRING = new string[(int)LOG_FIELDS.FIELDS_END]
        {
            "Type",
            "Breaking At",
            "Start",
            "End",
            "Duration Trouble Shooting",
            "Line",
            "Point",
            "Comments"
        };
        #endregion

        #region Sort File


        List<SortItem> SortItems { get; set; } = new List<SortItem>();

        public void SpatialCoordinates(SortItem sortItem)
        {
            double x = 0, y = 0;
            if (sortItem.Device == "FTU")
            {
                if (sortItem.Port.Contains("A"))
                    x = -1.5;
                if (sortItem.Port.Contains("B"))
                    x = 1.5;
                if (sortItem.Port.Contains("L") || sortItem.Port.Contains("R"))
                {
                    if (sortItem.Truckside)
                        y = 1.5;
                    else
                        y = -1.5;

                    if (sortItem.TruckPort != 'L' && sortItem.TruckPort != 'R')
                    {
                        if (sortItem.Port.Contains("L"))
                            y = -1;
                        if (sortItem.Port.Contains("R"))
                            y = 1;                        
                    }
                }
            }
            else
            {
                if (sortItem.Port.Contains("L") || sortItem.Port.Contains("R"))
                {
                    if (!sortItem.Truckside)
                        x = 1.5;
                    else
                        x = -1.5;
                }
            }
            sortItem.x = x;
            sortItem.y = y;
        }

        public void ReadSort(string filename)
        {
            
            using (var fStream = File.OpenRead(filename))
            {
                using (var stream = new StreamReader(fStream, Encoding.UTF8, true))
                {
                    String? line;
                    String[] row;
                    SortItem? initialItem = null, sortItem = null;

                    string serial = "";
                    string Change = "D";
                    int count = 0;
                    while ((line = stream.ReadLine()) != null)
                    {
                        count++;
                        if (line.Contains("LineInterfaceStats:"))
                        {
                            initialItem = null;
                            string[] splits = Regex.Split(line, @"\s+|=|[()]");
                            if (splits.Count() > 1)
                            {
                                initialItem = new SortItem();
                                serial = splits[2];
                                initialItem.DebugLine = count;
                                initialItem.Serial = serial;
                                initialItem.MSGid = int.Parse(splits[4]);
                                initialItem.Device = splits[6];
                                initialItem.TruckPort = splits[8].First();

                            }
                        }
                        else if (!line.Contains("UpTim") && !line.Contains("Box") && !line.Contains("N-pkts") && line != "" && initialItem != null)
                        {
                            sortItem = new SortItem();
                            sortItem.Serial = initialItem.Serial;
                            sortItem.MSGid = initialItem.MSGid;
                            sortItem.Device = initialItem.Device;
                            sortItem.TruckPort = initialItem.TruckPort;
                            string[] splits = Regex.Split(line, @"\s+");
                            if (splits[1].Contains("IN"))
                            {
                                sortItem.Port = splits[1].Replace(":", "");
                                sortItem.Packets = Int32.Parse(splits[2].Trim());
                                sortItem.Truckside = initialItem.TruckPort == sortItem.Port.First();
                                sortItem.DebugLine = count;
                                sortItem.CRC = Int32.Parse(splits[3].Trim());
                                sortItem.Overflow = Int32.Parse(splits[4].Trim());
                                sortItem.Timeout = Int32.Parse(splits[5].Trim());

                                SpatialCoordinates(sortItem);
                                SortItems.Add(sortItem);
                                sortItem = null;
                            }
                        }
                        else if (line.Contains("UpTime/Micro"))
                        {
                            string[] splits = Regex.Split(line, @"^([TPR]\d+) UpTime\/Micro=([\d\-]+)\/.+ +Line: (\d+|) +Flags: +(\d+|N\/A) - +(\d+|N\/A)(\s*([a-zA-Z\-]+)|)");
                            if (line.Contains("N/A"))
                            {
                                foreach (var sort in SortItems.Where(item => item.Serial == splits[1]))
                                {
                                    sort.Comment = splits[splits.Count() - 2];
                                    sort.Uptime = TimeSpan.FromMilliseconds(double.Parse(splits[2]));
                                    sort.x = null;
                                    sort.y = null;
                                }
                            }
                            else
                            {
                                foreach (var sort in SortItems.Where(item => item.Serial == splits[1]))
                                {
                                    sort.Comment = splits[splits.Count() - 2];
                                    sort.Uptime = TimeSpan.FromMilliseconds(double.Parse(splits[2]));
                                    sort.Line = int.Parse(splits[3]);
                                    sort.Flag = int.Parse(splits[4]);
                                    sort.x += sort.Flag;
                                    sort.y += sort.Line;

                                }
                            }
                        }
                    }

                }
            }
        }

        public string CreateSort(string filename, string path)
        {
            try
            {
                SortItems = new List<SortItem>();
                ReadSort(filename);
                
            }
            catch (Exception e)
            {
                return $"Error: {e}";
            }

            try
            {
                using (TextWriter writer = File.CreateText(path))
                {
                    WriteHeaders(writer, "sort");
                    string data = "";
                    foreach (var sort in SortItems)
                    {
                        data = FormatDataItem(sort);
                        writer.WriteLine(data);
                    }
                }
            }
            catch (Exception e)
            {
                return $"Error: {e}";
            }
            return "";
        }

        private string FormatDataItem(SortItem item)
        {
            string data = "";
            for (var field = SORT_FIELDS.DEBUGLINE; field < SORT_FIELDS.FIELDS_END; field++)
            {
                switch (field)
                {
                    case SORT_FIELDS.DEBUGLINE:
                        data += $"{item.DebugLine},";
                        break;
                    case SORT_FIELDS.MSGID:
                        data += $"{item.MSGid},";
                        break;
                    case SORT_FIELDS.DEVICE:
                        data += $"{item.Device},";
                        break;
                    case SORT_FIELDS.SERIAL:
                        data += $"{item.Serial},";
                        break;
                    case SORT_FIELDS.PORT:
                        data += $"{item.Port},";
                        break;
                    case SORT_FIELDS.TRUCKSIDE:
                        data += $"{item.Truckside},";
                        break;
                    case SORT_FIELDS.PACKETS:
                        data += $"{item.Packets},";
                        break;
                    case SORT_FIELDS.CRC:
                        data += $"{item.CRC},";
                        break;
                    case SORT_FIELDS.OVERFLOW:
                        data += $"{item.Overflow},";
                        break;
                    case SORT_FIELDS.TIMEOUT:
                        data += $"{item.Timeout},";
                        break;
                    case SORT_FIELDS.UPTIME:
                        data += $"{(item.Uptime.HasValue ? item.Uptime.Value.TotalMilliseconds:null)},";
                        break;
                    case SORT_FIELDS.UPTIME_CONF:
                        data += $"{(item.Uptime.HasValue ? new DateTime(item.Uptime.Value.Ticks).ToString("hh:mm:ss.fff") : null)},";
                        break;
                    case SORT_FIELDS.LINE:
                        data += $"{item.Line},";
                        break;
                    case SORT_FIELDS.FLAG:
                        data += $"{item.Flag},";
                        break;
                    case SORT_FIELDS.COMMENT:
                        data += $"{item.Comment},";
                        break;
                    case SORT_FIELDS.X:
                        data += $"{item.x},";
                        break;
                    case SORT_FIELDS.Y:
                        data += $"{item.y}";
                        break;
                    case SORT_FIELDS.FIELDS_END:
                        break;
                }
            }
            return data;
        }

        private enum SORT_FIELDS
        {
            DEBUGLINE,
            MSGID,
            DEVICE,
            SERIAL,
            PORT,
            TRUCKSIDE,
            PACKETS,
            CRC,
            OVERFLOW,
            TIMEOUT,
            UPTIME,
            UPTIME_CONF,
            LINE,
            FLAG,
            COMMENT,
            X,
            Y,
            FIELDS_END
        };

        private string[] SORT_STRING = new string[(int)SORT_FIELDS.FIELDS_END]
        {
            "Debug Line",
            "Msg ID",
            "Device",
            "Serial",
            "Port",
            "Truckside",
            "Packets",
            "CRC",
            "Overflow",
            "Timeout",
            "Uptime (ms)",
            "Uptime (hh:m:ss)",
            "Line",
            "Flag",
            "Comment",
            "X",
            "Y"
        };
        #endregion

        #region Retry

        public Dictionary<string,List<Retry>> Retries { get; set; } = new Dictionary<string,List<Retry>>();
        public string CreateRetriesTOD(string debuglog, string path)
        {
            try
            {
                String? line;
                String[] row;
                Retries = new Dictionary<string, List<Retry>>();
                Retry? retry = null;
                Retry? activity = null;
                int dev = 0;
                using (var debugStream = File.OpenRead(debuglog))
                {
                    using (var Dstream = new StreamReader(debugStream, Encoding.UTF8, true))
                    {
                        while((line = Dstream.ReadLine()) != null)
                        {
                            row = Regex.Split(line, @"\s+|[()]");
                            row = Array.FindAll(row, item => !string.IsNullOrEmpty(item));
                            if (row.Count() < 4 || (row[2] != "iX1Map" && row[2] != "iX1Vib") || row[3].Split(".").Count()>4)
                                continue;
                            switch (line)
                            {
                                case string l1 when NewVersion ? l1.Contains("TOD config VLan 0") : l1.Contains("(VLan 0)"):
                                    activity = new Retry();
                                    activity.Activity = "Global";
                                    break;
                                case string l2 when l2.Contains("TOD config try") && !l2.Contains("Enable") && !l2.Contains("Disable") && !l2.Contains("completed"):
                                    if (activity != null && activity.Activity == "Global")
                                    {
                                        activity = null;
                                        retry = null;
                                        dev = 0;
                                        if (Retries.Count() > 0 && Retries.LastOrDefault().Value.Count() > 0)
                                            Retries.LastOrDefault().Value.LastOrDefault().Status = "devn";
                                    }
                                    activity = new Retry();
                                    activity.Activity = l2.Contains("Localized") ? $"Localized {row[8]}" : $"Localized {row[7]}";
                                    break;
                                case string l3 when l3.Contains("Complete. Total boxes:"):
                                    activity = null;
                                    retry = null;
                                    dev = 0;
                                    if (Retries.Count() >0 && Retries.LastOrDefault().Value.Count() > 0)
                                        Retries.LastOrDefault().Value.LastOrDefault().Status = "devn";
                                    break;
                                case string l4 when l4.Contains("Last MessageId") || l4.Contains("Expected MessageID") || (l4.Contains("FailAwaySide") && !l4.Contains("FailAwaySide:")):
                                    if (activity != null)
                                    {
                                        retry = new Retry();
                                        retry.Activity = activity.Activity; 
                                        retry.DateTime = $"{row[0]} {row[1]}".TryConvertToDateTime();
                                        retry.App = row[2];
                                        
                                        retry.Status = (dev++ == 0) ? "dev0" : "devi";
                                        if (l4.Contains("FailAwaySide") && l4.Contains("NoResponse"))
                                            retry.Comment = "FailAwaySide NoResponse";
                                        else if (l4.Contains("NoResponse"))
                                            retry.Comment = "NoResponse";
                                        else if (l4.Contains("FailAwaySide"))
                                            retry.Comment = "FailAwaySide";

                                        if (row.Count() > 11 || row[row.Length-1] == "FailAwaySide")
                                        {
                                            retry.Serial = row[4];
                                            
                                            if (retry.Comment != "FailAwaySide NoResponse")
                                            {
                                                retry.Line = int.Parse(row[5]) / 100;
                                                retry.Flag = int.Parse(row[6]) / 100;
                                            }
                                        }
                                        else
                                        {
                                            retry.Serial = row[5];
                                            if (retry.Comment != "FailAwaySide NoResponse")
                                            {
                                                retry.Line = int.Parse(row[6]) / 100;
                                                retry.Flag = int.Parse(row[7]) / 100;
                                            }
                                        }
                                        
                                        if (Retries.ContainsKey(retry.Serial))
                                            Retries[retry.Serial].Add(retry);
                                        else
                                            Retries.Add(retry.Serial,new List<Retry> { retry });
                                    }
                                    break;

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }

            try
            {
                using (TextWriter writer = File.CreateText(path))
                {
                    WriteHeaders(writer, "retry");
                    string data = "";
                    int maxcount = 0;
                    if (Retries.Count() > 0)
                    {
                        maxcount = Retries.Values.Max(item => item.Count());
                        for (int i = 0; i < maxcount; i++)
                        {
                            foreach (var list in Retries.Values)
                            {
                                if (list.Count > i)
                                {
                                    data = FormatDataItem(list[i]);
                                    writer.WriteLine(data);
                                }
                            }
                        }
                    }
                    
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }

            return "";
        }

        private string FormatDataItem(Retry item)
        {
            string data = "";
            for (var field = RETRY_FIELDS.DATETIME; field < RETRY_FIELDS.FIELDS_END; field++)
            {
                switch (field)
                {
                    case RETRY_FIELDS.DATETIME:
                        data += $"{item.DateTime},";
                        break;
                    case RETRY_FIELDS.APP:
                        data += $"{item.App},";
                        break;
                    case RETRY_FIELDS.ACTIVITY:
                        data += $"{item.Activity},";
                        break;
                    case RETRY_FIELDS.SERIAL:
                        data += $"{item.Serial},";
                        break;
                    case RETRY_FIELDS.STATUS:
                        data += $"{item.Status},";
                        break;
                    case RETRY_FIELDS.LINE:
                        data += $"{item.Line},";
                        break;
                    case RETRY_FIELDS.FLAG:
                        data += $"{item.Flag},";
                        break;
                    case RETRY_FIELDS.COMMENT:
                        data += $"{item.Comment}";
                        break;
                }
            }
            return data;
        }

        private enum RETRY_FIELDS
        {
            DATETIME,
            APP,
            ACTIVITY,
            SERIAL,
            STATUS,
            LINE,
            FLAG,
            COMMENT,
            FIELDS_END
        };

        private string[] RETRY_STRING = new string[(int)RETRY_FIELDS.FIELDS_END]
        {
            "Datetime",
            "App",
            "Activity",
            "Serial",
            "Status",
            "Line",
            "Flag",
            "Comment"
        };
        #endregion

        public CSVCreator()
        {}

        public void WriteHeaders(TextWriter writer, string format = "tod")
        {
            string header = "";
            if (format == "tod")
            {
                if (NewVersion)
                    for (var field = TOD_FIELDS.APP; field < TOD_FIELDS.FIELDS_END; field++)
                        header += field == TOD_FIELDS.FIELDS_END - 1 ? TOD_STRING[(int)field] : TOD_STRING[(int)field] + ",";
                else
                    for (var field = OLD_TOD_FIELDS.APP; field < OLD_TOD_FIELDS.FIELDS_END; field++)
                        header += field == OLD_TOD_FIELDS.FIELDS_END - 1 ? OLD_TOD_STRING[(int)field] : OLD_TOD_STRING[(int)field] + ",";
            }
            else if (format == "log")
            {
                for (var field = LOG_FIELDS.TYPE; field < LOG_FIELDS.FIELDS_END; field++)
                    header += field == LOG_FIELDS.FIELDS_END - 1 ? LOG_STRING[(int)field] : LOG_STRING[(int)field] + ",";
            }
            else if (format == "sort")
            {
                for (var field = SORT_FIELDS.DEBUGLINE; field < SORT_FIELDS.FIELDS_END; field++)
                    header += field == SORT_FIELDS.FIELDS_END-1 ? SORT_STRING[(int)field] : SORT_STRING[(int)field] + ",";
            }
            else if (format == "retry")
            {
                for (var field = RETRY_FIELDS.DATETIME; field < RETRY_FIELDS.FIELDS_END; field++)
                    header += field == RETRY_FIELDS.FIELDS_END - 1 ? RETRY_STRING[(int)field] : RETRY_STRING[(int)field] + ",";
            }

            writer.WriteLine(header);
        }


        public void WriteStats(TextWriter writer)
        {
            writer.WriteLine("");
            writer.WriteLine($",TOTAL, Line Drops, Line Breaks, Unknown");
            writer.WriteLine($", {LogBreaks.Count}, {Stats["Lines"].d}, {Stats["Lines"].b}, {LogBreaks.Count - Stats["Lines"].b - Stats["Lines"].d}");
            Stats.Where(item => (item.Value.b > 0 || item.Value.d > 0) && item.Key != "Lines").ForEach(item => writer.WriteLine($"{item.Key},{ShowNumberOrPercent(item)}"));

            writer.WriteLine("");
            double Rcount = 0, Pcount = 0,
                Tcount = 0;
            Stats.Where(item => item.Key.Contains("R")).ForEach(item => Rcount += item.Value.d + item.Value.b);
            Stats.Where(item => item.Key.Contains("P")).ForEach(item => Pcount += item.Value.d + item.Value.b);
            Stats.Where(item => item.Key.Contains("T")).ForEach(item => Tcount += item.Value.d + item.Value.b);
            writer.WriteLine($"RAMs, {RAMCount}, {Rcount/RAMCount*100}%");
            writer.WriteLine($"PSUs, {PSUCount}, {Pcount/PSUCount*100}%");
            writer.WriteLine($"FTUs, {FTUCount}, {Tcount/FTUCount*100}%");
        }

        public string ShowNumberOrPercent(KeyValuePair<string,(int d, int b)> item)
        {
            return $"{(item.Value.d.TryConvertToDouble() + item.Value.b.TryConvertToDouble()) / LogBreaks.Count * 100}%,{(item.Value.d.TryConvertToDouble() / Stats["Lines"].d) * 100}%,{(item.Value.b.TryConvertToDouble() / Stats["Lines"].b) * 100}%";
        }

    }
}
