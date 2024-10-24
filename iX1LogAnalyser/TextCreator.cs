using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static iX1LogAnalyser.Classes;

namespace iX1LogAnalyser
{
    public class TextCreator
    {
        Dictionary<string, string> lineInstances = new Dictionary<string, string>();
        List<string> serials = new List<string>();
        List<string> discovery, MPs, End = new List<string>();

        public TextCreator() { }

        public string SortLineInstance(string filename, string path)
        {
            ReadAndSplit(filename, path);

            try
            {
                using (TextWriter writer = File.CreateText(path))
                {
                    foreach (var disc in discovery)
                        writer.WriteLine(disc);

                    foreach (var serial in serials)
                    {
                        if (lineInstances.ContainsKey(serial))
                            writer.Write(lineInstances[serial]);
                    }
                    writer.WriteLine("");
                    writer.WriteLine("");
                    writer.WriteLine("");
                    foreach (var mp in MPs)
                        writer.WriteLine(mp);
                    foreach(var end in End)
                        writer.WriteLine(end);
                }
            }
            catch (Exception e)
            {
                return e.ToString();

            }
            return "";
        }

            public void ReadAndSplit(string filename, string path)
        {
            try
            {

                using (var fStream = File.OpenRead(filename))
                {
                    
                    using (var stream = new StreamReader(fStream, Encoding.UTF8, true))
                    {
                        string? line;
                        string[] row;

                        discovery = new List<string>(); MPs = new List<string>(); End = new List<string>();
                        string serial = "";
                        string Change = "D";
                        string notTrunkline = "";
                        string Trunkline = "";
                        string Trunk = "";
                        while ((line = stream.ReadLine()) != null)
                        {
                            if (line.Contains("LineInterfaceStats:"))
                                Change = "L";
                            else if (line.Contains("UpTime"))
                            {
                                if (serial != "" && Change == "L")
                                    lineInstances[serial] += Trunkline + notTrunkline;
                                Change = "M";
                            }
                            else if (line.Contains("Box"))
                                Change = "End";

                            switch (Change)
                            {
                                case "D":
                                    discovery.Add(line);
                                    break;
                                case "L":
                                    char[] separators = { '=', '(', ')'};
                                    string[] splits = line.Split(separators);
                                    if (splits.Count() > 1)
                                    {
                                        if (serial != "")
                                            lineInstances[serial] += Trunkline + notTrunkline;
                                        Trunkline = "";
                                        notTrunkline = "";
                                        serial = splits[1];
                                        string[] separated = splits[5].Split("  ");
                                        Trunk = splits[6];
                                        lineInstances.Add(serial, 
                                            $"{Environment.NewLine}{splits[0]}={splits[1]}{splits[3]}={splits[4]}={separated[0].ToUpper()}  {separated[1]}={splits[6]}");
                                    }
                                    else
                                    {
                                        if (line.Contains("N-pkts"))
                                            lineInstances[serial] += Environment.NewLine + line;
                                        else if (Trunk != "" && line.Contains($"{Trunk}-"))
                                            Trunkline += Environment.NewLine + line;
                                        else if (line != "") 
                                            notTrunkline += Environment.NewLine + line;
                                    }
                                    break;
                                case "M":
                                   
                                    var srl = line.Split(" ")[0];
                                    if (srl != null && srl != "")
                                        serials.Add(srl);
                                    MPs.Add(line);
                                    break;
                                case "End":
                                    End.Add(line);
                                    break;
                            }
                        }
                    }
                }
            }
            catch { }
        }
    }
}
