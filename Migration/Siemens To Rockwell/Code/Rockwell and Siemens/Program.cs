//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Xml.Linq;
//using System.Windows.Forms;


//namespace RockwellSiemensConverters
//{
//    class Program
//    {
//        [STAThread]
//        static void Main(string[] args)
//        {
//            Application.EnableVisualStyles();
//            Application.SetCompatibleTextRenderingDefault(false);

//            Console.WriteLine("Pick operation:");
//            Console.WriteLine("1 - Rockwell (L5X/RLL) -> Siemens FB XML");
//            Console.WriteLine("2 - Siemens FB XML -> Rockwell (simple RLL representation)");
//            Console.Write("Choice: ");
//            var ch = Console.ReadLine();

//            if (ch == "1")
//            {
//                using (OpenFileDialog ofd = new OpenFileDialog())
//                {
//                    ofd.Filter = "Rockwell L5X files (*.L5X)|*.L5X|All files (*.*)|*.*";
//                    ofd.Title = "Select Rockwell L5X File";
//                    if (ofd.ShowDialog() != DialogResult.OK) return;
//                    var rockwellFile = ofd.FileName;
//                    string outDir = Path.Combine(Environment.CurrentDirectory, "SiemensFiles");
//                    Directory.CreateDirectory(outDir);
//                    string outFile = Path.Combine(outDir, Path.GetFileNameWithoutExtension(rockwellFile) + "_Siemens.xml");
//                    RockwellToSiemensConverter.Convert(rockwellFile, outFile);
//                    Console.WriteLine("Saved Siemens XML to: " + outFile);
//                }
//            }
//            else if (ch == "2")
//            {
//                using (OpenFileDialog ofd = new OpenFileDialog())
//                {
//                    ofd.Filter = "Siemens XML files (*.xml)|*.xml|All files (*.*)|*.*";
//                    ofd.Title = "Select Siemens FB XML File";
//                    if (ofd.ShowDialog() != DialogResult.OK) return;
//                    var siemensFile = ofd.FileName;
//                    string outDir = Path.Combine(Environment.CurrentDirectory, "RockwellFiles");
//                    Directory.CreateDirectory(outDir);
//                    string outFile = Path.Combine(outDir, Path.GetFileNameWithoutExtension(siemensFile) + "_Rockwell.RLL.L5X");
//                    SiemensToRockwellConverter.Convert(siemensFile, outFile);
//                    Console.WriteLine("Saved Rockwell-like RLL to: " + outFile);
//                }
//            }
//            else
//            {
//                Console.WriteLine("Invalid choice.");
//            }

//            Console.WriteLine("Done. Press Enter to exit.");
//            Console.ReadLine();
//        }
//    }

//    static class RockwellToSiemensConverter
//    {
//        static readonly XNamespace nsInterface = "http://www.siemens.com/automation/Openness/SW/Interface/v5";
//        static readonly XNamespace nsFlgNetV4 = "http://www.siemens.com/automation/Openness/SW/NetworkSource/FlgNet/v4";
//        static readonly XNamespace nsFlgNetV3 = "http://www.siemens.com/automation/Openness/SW/NetworkSource/FlgNet/v3";

//        public static void Convert(string rockwellFile, string siemensOutputPath)
//        {
//            var rockwellDoc = XDocument.Load(rockwellFile);

//            var siemensDoc = new XDocument(new XDeclaration("1.0", "utf-8", null),
//                new XElement("Document",
//                    new XElement("Engineering", new XAttribute("version", "V17")),
//                    new XElement("DocumentInfo",
//                        new XElement("Created", DateTime.UtcNow.ToString("o")),
//                        new XElement("ExportSetting", "None")
//                    ),
//                    BuildFbElement(rockwellDoc, rockwellFile)
//                )
//            );

//            siemensDoc.Save(siemensOutputPath);
//        }

//        static XElement BuildFbElement(XDocument rockwellDoc, string rockwellFile)
//        {
//            return new XElement("SW.Blocks.FB",
//                new XAttribute("ID", "0"),
//                new XElement("AttributeList",
//                    // Interface
//                    new XElement("Interface",
//                        new XElement(nsInterface + "Sections",
//                            CreateSection("Input", rockwellDoc, "Parameter", nsInterface),
//                            CreateSection("Output", rockwellDoc, "Parameter", nsInterface),
//                            CreateSection("InOut", rockwellDoc, "Parameter", nsInterface),
//                            CreateSection("Static", rockwellDoc, "LocalTag", nsInterface),
//                            CreateSection("Temp", rockwellDoc, "LocalTag", nsInterface, onlyTemp: true),
//                            CreateSection("Constant", rockwellDoc, "Constant", nsInterface)
//                        )
//                    ),
//                    new XElement("Name", Path.GetFileNameWithoutExtension(rockwellFile) + "__Converted"),
//                    new XElement("Number", "1"),
//                    new XElement("ProgrammingLanguage", "LAD")
//                ),
//                new XElement("ObjectList",
//                    new XElement("MultilingualText", new XAttribute("ID", ShortId()), new XAttribute("CompositionName", "Comment"),
//                        new XElement("ObjectList",
//                            new XElement("MultilingualTextItem", new XAttribute("ID", ShortId()), new XAttribute("CompositionName", "Items"),
//                                new XElement("AttributeList",
//                                    new XElement("Culture", "en-US"),
//                                    new XElement("Text", "")
//                                )
//                            )
//                        )
//                    ),
//                    GenerateCompileUnits(rockwellDoc)
//                )
//            );
//        }

//        static XElement CreateSection(string sectionName, XDocument rockwellDoc, string tagType, XNamespace ns, bool onlyTemp = false)
//        {
//            IEnumerable<XElement> members = Enumerable.Empty<XElement>();

//            if (rockwellDoc != null)
//            {
//                if (onlyTemp && tagType == "LocalTag")
//                {
//                    members = rockwellDoc.Descendants("LocalTag")
//                        .Where(x =>
//                        {
//                            var usageAttr = (string)x.Attribute("Usage");
//                            if (!string.IsNullOrEmpty(usageAttr) && usageAttr.Equals("Temp", StringComparison.OrdinalIgnoreCase)) return true;
//                            var name = (string)x.Attribute("Name");
//                            return !string.IsNullOrEmpty(name) && name.StartsWith("t", StringComparison.OrdinalIgnoreCase);
//                        });
//                }
//                else if (tagType == "LocalTag")
//                    members = rockwellDoc.Descendants("LocalTag");
//                else if (tagType == "Constant")
//                    members = rockwellDoc.Descendants("Constant");
//                else
//                    members = rockwellDoc.Descendants(tagType)
//                        .Where(p => string.Equals((string)p.Attribute("Usage"), sectionName, StringComparison.OrdinalIgnoreCase));
//            }

//            return new XElement(ns + "Section",
//                new XAttribute("Name", sectionName),
//                members.Select(p =>
//                    new XElement(ns + "Member",
//                        new XAttribute("Name", (string)p.Attribute("Name") ?? ""),
//                        new XAttribute("Datatype", MapDataType((string)p.Attribute("DataType") ?? (string)p.Attribute("DataTypeName") ?? "BOOL"))
//                    )
//                )
//            );
//        }

//        static string MapDataType(string rockwellType)
//        {
//            if (string.IsNullOrWhiteSpace(rockwellType)) return "Bool";
//            return rockwellType.ToUpper() switch
//            {
//                "BOOL" => "Bool",
//                "INT" => "Int",
//                "DINT" => "DInt",
//                "REAL" => "Real",
//                "STRING" => "String",
//                "WORD" => "Word",
//                "DWORD" => "DWord",
//                _ => "Bool",
//            };
//        }

//        static string ShortId() => Guid.NewGuid().ToString("N").Substring(0, 2).ToUpper();

//        static IEnumerable<XElement> GenerateCompileUnits(XDocument rockwellDoc)
//        {
//            var compileUnits = new List<XElement>();
//            int baseCompileId = 3;
//            int globalPartUid = 21;
//            int globalWireUid = 28;

//            var rllRungs = rockwellDoc.Descendants("RLLContent").Descendants("Rung").ToList();
//            var simpleRungs = rockwellDoc.Descendants("Rung").ToList();
//            IEnumerable<XElement> rungNodes = rllRungs.Any() ? rllRungs : (simpleRungs.Any() ? simpleRungs : rockwellDoc.Descendants("Instruction").Select(i => new XElement("Rung", i)).ToList());

//            int unitIndex = 0;
//            foreach (var rung in rungNodes)
//            {
//                unitIndex++;

//                var contactOccurrences = new List<(string token, string opType)>();
//                string coilName = null;
//                var textNode = rung.Element("Text");
//                if (textNode != null)
//                {
//                    var txt = (textNode.Value ?? "").Trim();
//                    ParseRllTextAdvanced(txt, contactOccurrences, ref coilName);
//                }

//                if (!contactOccurrences.Any())
//                {
//                    var inputs = rung.Descendants("Input")
//                        .Select(x => (token: (string)x.Attribute("TagName") ?? (string)x.Attribute("Name"), opType: "contact"))
//                        .Where(t => !string.IsNullOrWhiteSpace(t.token))
//                        .ToList();
//                    if (inputs.Any()) contactOccurrences.AddRange(inputs);
//                    else
//                    {
//                        var tagAttrs = rung.Descendants().Attributes("TagName").Select(a => a.Value).Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
//                        foreach (var t in tagAttrs) contactOccurrences.Add((t, "contact"));
//                    }
//                }

//                if (string.IsNullOrWhiteSpace(coilName))
//                {
//                    var outTag = rung.Descendants("Output").Select(x => (string)x.Attribute("TagName") ?? (string)x.Attribute("Name")).FirstOrDefault();
//                    if (!string.IsNullOrWhiteSpace(outTag)) coilName = outTag;
//                }

//                if (string.IsNullOrWhiteSpace(coilName) && contactOccurrences.Any())
//                    coilName = contactOccurrences.Select(c => c.token).First();

//                var parts = new List<XElement>();
//                var wires = new List<XElement>();
//                var accessUids = new List<int>();
//                var operandUids = new List<int>();

//                foreach (var occ in contactOccurrences)
//                {
//                    if (occ.opType == "contact")
//                    {
//                        int accessUid = globalPartUid++;
//                        accessUids.Add(accessUid);
//                        parts.Add(new XElement("Access", new XAttribute("Scope", "LocalVariable"), new XAttribute("UId", accessUid.ToString()),
//                            new XElement("Symbol", new XElement("Component", new XAttribute("Name", occ.token)))))
//                        ;
//                        int contactUid = globalPartUid++;
//                        operandUids.Add(contactUid);
//                        parts.Add(new XElement("Part", new XAttribute("Name", "Contact"), new XAttribute("UId", contactUid.ToString())));
//                        wires.Add(new XElement("Wire", new XAttribute("UId", (globalWireUid++).ToString()),
//                            new XElement("IdentCon", new XAttribute("UId", accessUid.ToString())),
//                            new XElement("NameCon", new XAttribute("UId", contactUid.ToString()), new XAttribute("Name", "operand"))
//                        ));
//                    }
//                    else if (occ.opType == "cmp" || occ.opType == "func")
//                    {
//                        var m = Regex.Match(occ.token, @"^(?<op>[A-Za-z_0-9]+)\s*\(\s*(?<args>.*)\s*\)$");
//                        if (!m.Success) continue;
//                        var op = m.Groups["op"].Value.ToUpperInvariant();
//                        var args = SplitTopLevelArgs(m.Groups["args"].Value);

//                        var argAccesses = new List<int>();
//                        var argOperandParts = new List<int>();
//                        foreach (var a in args)
//                        {
//                            var cleaned = a.Trim();
//                            if (IsNumericLiteral(cleaned))
//                            {
//                                int litUid = globalPartUid++;
//                                argAccesses.Add(litUid);
//                                parts.Add(new XElement("Access", new XAttribute("Scope", "LiteralConstant"), new XAttribute("UId", litUid.ToString()),
//                                    new XElement("Constant",
//                                        new XElement("ConstantType", InferConstantType(cleaned)),
//                                        new XElement("ConstantValue", cleaned),
//                                        new XElement("StringAttribute", new XAttribute("Name", "Format"), "Dec_signed")
//                                    )
//                                ));
//                            }
//                            else
//                            {
//                                int accessUid = globalPartUid++;
//                                argAccesses.Add(accessUid);
//                                parts.Add(new XElement("Access", new XAttribute("Scope", "LocalVariable"), new XAttribute("UId", accessUid.ToString()),
//                                    new XElement("Symbol", new XElement("Component", new XAttribute("Name", cleaned)))));
//                            }
//                        }

//                        string partName = op switch
//                        {
//                            "EQ" => "Eq",
//                            "NE" => "Ne",
//                            "GT" => "Gt",
//                            "LT" => "Lt",
//                            "ADD" => "Add",
//                            "MUL" => "Mul",
//                            "SUB" => "Sub",
//                            "DIV" => "Div",
//                            _ => op
//                        };

//                        var partUid = globalPartUid++;
//                        var partElem = new XElement("Part", new XAttribute("Name", partName), new XAttribute("UId", partUid.ToString()));
//                        if (partName == "Add" || partName == "Mul" || partName == "Sub" || partName == "Div")
//                        {
//                            partElem.Add(new XElement("TemplateValue", new XAttribute("Name", "Card"), new XAttribute("Type", "Cardinality"), argAccesses.Count.ToString()));
//                            partElem.Add(new XElement("TemplateValue", new XAttribute("Name", "SrcType"), new XAttribute("Type", "Type"), args.Any(a => a.Contains(".") || a.Contains("0")) ? "Real" : "Int"));
//                        }
//                        else if (partName == "Eq" || partName == "Gt" || partName == "Ne" || partName == "Lt")
//                        {
//                            partElem.Add(new XElement("TemplateValue", new XAttribute("Name", "SrcType"), new XAttribute("Type", "Type"), "Int"));
//                        }
//                        parts.Add(partElem);

//                        for (int i = 0; i < argAccesses.Count; i++)
//                        {
//                            wires.Add(new XElement("Wire", new XAttribute("UId", (globalWireUid++).ToString()),
//                                new XElement(argAccesses[i] >= 0 ? "IdentCon" : "IdentCon", new XAttribute("UId", argAccesses[i].ToString())),
//                                new XElement("NameCon", new XAttribute("UId", partUid.ToString()), new XAttribute("Name", "in" + (i + 1)))
//                            ));
//                        }

//                        int outPartUid = globalPartUid++;
//                        operandUids.Add(outPartUid);
//                        parts.Add(new XElement("Part", new XAttribute("Name", "Contact"), new XAttribute("UId", outPartUid.ToString())));

//                        wires.Add(new XElement("Wire", new XAttribute("UId", (globalWireUid++).ToString()),
//                            new XElement("NameCon", new XAttribute("UId", partUid.ToString()), new XAttribute("Name", "out")),
//                            new XElement("NameCon", new XAttribute("UId", outPartUid.ToString()), new XAttribute("Name", "in"))
//                        ));
//                    }
//                }

//                int oUid = 0;
//                if (operandUids.Count >= 2)
//                {
//                    oUid = globalPartUid++;
//                    parts.Add(new XElement("Part", new XAttribute("Name", "O"), new XAttribute("UId", oUid.ToString()),
//                        new XElement("TemplateValue", new XAttribute("Name", "Card"), new XAttribute("Type", "Cardinality"), operandUids.Count.ToString())));
//                    for (int i = 0; i < operandUids.Count; i++)
//                    {
//                        wires.Add(new XElement("Wire", new XAttribute("UId", (globalWireUid++).ToString()),
//                            new XElement("NameCon", new XAttribute("UId", operandUids[i].ToString()), new XAttribute("Name", "out")),
//                            new XElement("NameCon", new XAttribute("UId", oUid.ToString()), new XAttribute("Name", "in" + (i + 1)))));
//                    }
//                    var coilUid = globalPartUid++;
//                    parts.Add(new XElement("Part", new XAttribute("Name", "Coil"), new XAttribute("UId", coilUid.ToString())));
//                    wires.Add(new XElement("Wire", new XAttribute("UId", (globalWireUid++).ToString()),
//                        new XElement("NameCon", new XAttribute("UId", oUid.ToString()), new XAttribute("Name", "out")),
//                        new XElement("NameCon", new XAttribute("UId", coilUid.ToString()), new XAttribute("Name", "in"))
//                    ));
//                    if (!string.IsNullOrWhiteSpace(coilName))
//                    {
//                        int coilAccessUid = globalPartUid++;
//                        parts.Add(new XElement("Access", new XAttribute("Scope", "LocalVariable"), new XAttribute("UId", coilAccessUid.ToString()),
//                            new XElement("Symbol", new XElement("Component", new XAttribute("Name", coilName)))));
//                        wires.Add(new XElement("Wire", new XAttribute("UId", (globalWireUid++).ToString()),
//                            new XElement("IdentCon", new XAttribute("UId", coilAccessUid.ToString())),
//                            new XElement("NameCon", new XAttribute("UId", coilUid.ToString()), new XAttribute("Name", "operand"))
//                        ));
//                    }
//                }
//                else if (operandUids.Count == 1)
//                {
//                    var coilUid = globalPartUid++;
//                    parts.Add(new XElement("Part", new XAttribute("Name", "Coil"), new XAttribute("UId", coilUid.ToString())));
//                    wires.Add(new XElement("Wire", new XAttribute("UId", (globalWireUid++).ToString()),
//                        new XElement("NameCon", new XAttribute("UId", operandUids[0].ToString()), new XAttribute("Name", "out")),
//                        new XElement("NameCon", new XAttribute("UId", coilUid.ToString()), new XAttribute("Name", "in"))
//                    ));
//                    if (!string.IsNullOrWhiteSpace(coilName))
//                    {
//                        int coilAccessUid = globalPartUid++;
//                        parts.Add(new XElement("Access", new XAttribute("Scope", "LocalVariable"), new XAttribute("UId", coilAccessUid.ToString()),
//                            new XElement("Symbol", new XElement("Component", new XAttribute("Name", coilName)))));
//                        wires.Add(new XElement("Wire", new XAttribute("UId", (globalWireUid++).ToString()),
//                            new XElement("IdentCon", new XAttribute("UId", coilAccessUid.ToString())),
//                            new XElement("NameCon", new XAttribute("UId", coilUid.ToString()), new XAttribute("Name", "operand"))
//                        ));
//                    }
//                }

//                var powerWire = new XElement("Wire", new XAttribute("UId", (globalWireUid++).ToString()), new XElement("Powerrail"));
//                foreach (var c in operandUids.Take(2))
//                    powerWire.Add(new XElement("NameCon", new XAttribute("UId", c.ToString()), new XAttribute("Name", "in")));
//                wires.Add(powerWire);

//                var flgNet = new XElement(nsFlgNetV4 + "FlgNet",
//                    new XElement(nsFlgNetV4 + "Parts", parts),
//                    new XElement(nsFlgNetV4 + "Wires", wires)
//                );

//                var compileUnit = new XElement("SW.Blocks.CompileUnit",
//                    new XAttribute("ID", (baseCompileId + unitIndex - 1).ToString("X")),
//                    new XAttribute("CompositionName", "CompileUnits"),
//                    new XElement("AttributeList",
//                        new XElement("NetworkSource", flgNet),
//                        new XElement("ProgrammingLanguage", "LAD")
//                    ),
//                    new XElement("ObjectList",
//                        new XElement("MultilingualText", new XAttribute("ID", ShortId()), new XAttribute("CompositionName", "Comment"),
//                            new XElement("ObjectList",
//                                new XElement("MultilingualTextItem", new XAttribute("ID", ShortId()), new XAttribute("CompositionName", "Items"),
//                                    new XElement("AttributeList",
//                                        new XElement("Culture", "en-US"),
//                                        new XElement("Text", "")
//                                    )
//                                )
//                            )
//                        ),
//                        new XElement("MultilingualText", new XAttribute("ID", ShortId()), new XAttribute("CompositionName", "Title"),
//                            new XElement("ObjectList",
//                                new XElement("MultilingualTextItem", new XAttribute("ID", ShortId()), new XAttribute("CompositionName", "Items"),
//                                    new XElement("AttributeList",
//                                        new XElement("Culture", "en-US"),
//                                        new XElement("Text", GetRungTitle(rung))
//                                    )
//                                )
//                            )
//                        )
//                    )
//                );

//                compileUnits.Add(compileUnit);
//            }

//            return compileUnits;
//        }

//        static List<string> SplitTopLevelArgs(string args)
//        {
//            var result = new List<string>();
//            int depth = 0;
//            int last = 0;
//            for (int i = 0; i < args.Length; i++)
//            {
//                if (args[i] == '(') depth++;
//                else if (args[i] == ')') depth--;
//                else if (args[i] == ',' && depth == 0)
//                {
//                    result.Add(args.Substring(last, i - last));
//                    last = i + 1;
//                }
//            }
//            if (last < args.Length) result.Add(args.Substring(last));
//            return result.Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
//        }

//        static string InferConstantType(string literal)
//        {
//            if (literal.Contains(".") || literal.Contains("E") || literal.Contains("e")) return "Real";
//            if (int.TryParse(literal, out _)) return "Int";
//            return "Int";
//        }

//        static void ParseRllTextAdvanced(string text, List<(string token, string opType)> outTokens, ref string coilName)
//        {
//            if (string.IsNullOrWhiteSpace(text)) return;
//            var t = text.Trim();
//            if (t.EndsWith(";")) t = t.Substring(0, t.Length - 1);

//            var mCoil = Regex.Match(t, @"(?<op>OTL|OTU|OTE)\s*\(\s*(?<n>[^)]+)\s*\)", RegexOptions.IgnoreCase);
//            if (mCoil.Success) coilName = mCoil.Groups["n"].Value.Trim();
//            else
//            {
//                var mm = Regex.Matches(t, @"\(\s*(?<n>[^)]+)\s*\)");
//                if (mm.Count > 0) coilName = mm[mm.Count - 1].Groups["n"].Value.Trim();
//            }

//            var outer = Regex.Match(t, @"\[(?<inner>.*)\]");
//            string inner = outer.Success ? outer.Groups["inner"].Value : t;

//            var tokens = SplitTopLevelArgs(inner);

//            foreach (var tok in tokens)
//            {
//                var trimmed = tok.Trim();
//                // check XIC/XIO
//                var mXic = Regex.Match(trimmed, @"XIC\s*\(\s*(?<n>[^)]+)\s*\)", RegexOptions.IgnoreCase);
//                var mXio = Regex.Match(trimmed, @"XIO\s*\(\s*(?<n>[^)]+)\s*\)", RegexOptions.IgnoreCase);
//                if (mXic.Success)
//                {
//                    outTokens.Add((mXic.Groups["n"].Value.Trim(), "contact"));
//                    continue;
//                }
//                if (mXio.Success)
//                {
//                    outTokens.Add((mXio.Groups["n"].Value.Trim(), "contact")); 
//                    continue;
//                }

//                var mFunc = Regex.Match(trimmed, @"^(?<fn>[A-Za-z_0-9]+)\s*\(.*\)$");
//                if (mFunc.Success)
//                {
//                    outTokens.Add((trimmed, "cmp")); // mark as cmp/func
//                    continue;
//                }

//                if (!string.IsNullOrWhiteSpace(trimmed))
//                {
//                    outTokens.Add((trimmed, "contact"));
//                }
//            }
//        }

//        static string GetRungTitle(XElement rung)
//        {
//            var text = rung.Element("Text")?.Value;
//            if (!string.IsNullOrWhiteSpace(text)) return text.Trim();
//            var instr = rung.Descendants("Instruction").FirstOrDefault();
//            if (instr != null) return (string)instr.Attribute("Name") ?? (string)instr.Attribute("Type") ?? "";
//            var comment = rung.Descendants("Comment").FirstOrDefault();
//            return comment?.Value ?? "";
//        }
//    }

//    static class SiemensToRockwellConverter
//    {
//        public static void Convert(string siemensXmlPath, string rockwellOutputPath)
//        {
//            var doc = XDocument.Load(siemensXmlPath);
//            var sb = new System.Text.StringBuilder();

//            var compileUnits = doc.Descendants("SW.Blocks.CompileUnit");
//            int idx = 0;
//            foreach (var cu in compileUnits)
//            {
//                idx++;
//                sb.AppendLine($"-- CompileUnit {cu.Attribute("ID")?.Value ?? idx.ToString()} --");
//                var network = cu.Descendants().FirstOrDefault(e => e.Name.LocalName == "FldNet" || e.Name.LocalName == "FlgNet");
//                var flgNet = cu.Descendants().FirstOrDefault(e => e.Name.LocalName == "FlgNet");
//                if (flgNet == null)
//                {
//                    sb.AppendLine(" (no FlgNet)");
//                    sb.AppendLine();
//                    continue;
//                }

//                var parts = flgNet.Descendants().FirstOrDefault(e => e.Name.LocalName == "Parts")?.Elements().ToList() ?? new List<XElement>();
//                var wires = flgNet.Descendants().FirstOrDefault(e => e.Name.LocalName == "Wires")?.Elements().ToList() ?? new List<XElement>();

//                var partByUid = parts.Where(p => p.Attribute("UId") != null).ToDictionary(p => (string)p.Attribute("UId"), p => p);
//                var nameConTargets = new Dictionary<string, List<(string nameCon, string fromUid)>>();

//                foreach (var w in wires)
//                {
//                    var uid = (string)w.Attribute("UId") ?? "";
//                    foreach (var nc in w.Elements().Where(x => x.Name.LocalName == "NameCon"))
//                    {
//                        var toUid = (string)nc.Attribute("UId");
//                        var nameAttr = (string)nc.Attribute("Name") ?? "";
//                        if (!nameConTargets.ContainsKey(toUid)) nameConTargets[toUid] = new List<(string, string)>();
//                        nameConTargets[toUid].Add((nameAttr, uid));
//                    }
//                    // IdentCon -> maps Access UId to wire
//                    foreach (var ic in w.Elements().Where(x => x.Name.LocalName == "IdentCon"))
//                    {
//                        var identUid = (string)ic.Attribute("UId");
//                        if (!nameConTargets.ContainsKey(identUid)) nameConTargets[identUid] = new List<(string, string)>();
//                        nameConTargets[identUid].Add(("ident", uid));
//                    }
//                }

//                var coilParts = parts.Where(p => string.Equals((string)p.Attribute("Name"), "Coil", StringComparison.OrdinalIgnoreCase)).ToList();
//                if (!coilParts.Any())
//                {
//                    sb.AppendLine("No coil found; listing parts:");
//                    foreach (var p in parts) sb.AppendLine(p.ToString(SaveOptions.DisableFormatting));
//                    sb.AppendLine();
//                    continue;
//                }

//                foreach (var coil in coilParts)
//                {
//                    var coilUid = (string)coil.Attribute("UId");
//                    var incoming = wires.Where(w => w.Elements().Any(e => e.Name.LocalName == "NameCon" && (string)e.Attribute("UId") == coilUid)).ToList();
//                    string sourceUid = null;
//                    foreach (var w in incoming)
//                    {
//                        foreach (var nc in w.Elements().Where(e => e.Name.LocalName == "NameCon"))
//                        {
//                            var uidAttr = (string)nc.Attribute("UId");
//                            if (uidAttr != coilUid)
//                            {
//                                sourceUid = uidAttr;
//                                break;
//                            }
//                        }
//                        if (sourceUid != null) break;
//                    }

//                    string rll = ReconstructExpressionForUid(sourceUid, partByUid, wires);
//                    if (string.IsNullOrWhiteSpace(rll)) rll = "(unknown)";
//                    var coilAccess = wires.Where(w => w.Elements().Any(e => e.Name.LocalName == "NameCon" && (string)e.Attribute("UId") == coilUid && (string)e.Attribute("Name") == "operand"))
//                        .SelectMany(w => w.Elements().Where(e => e.Name.LocalName == "IdentCon"))
//                        .Select(ic => (string)ic.Attribute("UId")).FirstOrDefault();
//                    string coilName = null;
//                    if (coilAccess != null && partByUid.ContainsKey(coilAccess))
//                    {
//                        var accessElem = partByUid[coilAccess];
//                        var comp = accessElem.Descendants().FirstOrDefault(e => e.Name.LocalName == "Component");
//                        if (comp != null) coilName = (string)comp.Attribute("Name");
//                    }

//                    if (string.IsNullOrWhiteSpace(coilName))
//                        coilName = "COIL_" + coilUid;

//                    sb.AppendLine($"{rll} -> OTE({coilName});");
//                }

//                var title = cu.Descendants().FirstOrDefault(e => e.Attribute("CompositionName")?.Value == "Title");
//                if (title != null)
//                {
//                    var text = title.Descendants("Text").FirstOrDefault()?.Value;
//                    if (!string.IsNullOrWhiteSpace(text))
//                        sb.AppendLine("/* " + text.Trim() + " */");
//                }

//                sb.AppendLine();
//            }

//            File.WriteAllText(rockwellOutputPath, sb.ToString());
//        }

//        static string ReconstructExpressionForUid(string uid, Dictionary<string, XElement> partByUid, List<XElement> wires)
//        {
//            if (uid == null) return null;
//            if (!partByUid.ContainsKey(uid)) return null;
//            var part = partByUid[uid];
//            var name = (string)part.Attribute("Name") ?? "";
//            if (string.Equals(name, "Contact", StringComparison.OrdinalIgnoreCase))
//            {
//                var wire = wires.FirstOrDefault(w => w.Elements().Any(e => e.Name.LocalName == "NameCon" && (string)e.Attribute("UId") == uid));
//                if (wire != null)
//                {
//                    var ident = wire.Elements().FirstOrDefault(e => e.Name.LocalName == "IdentCon");
//                    if (ident != null)
//                    {
//                        var accessUid = (string)ident.Attribute("UId");
//                        if (partByUid.ContainsKey(accessUid))
//                        {
//                            var comp = partByUid[accessUid].Descendants().FirstOrDefault(e => e.Name.LocalName == "Component");
//                            if (comp != null) return "XIC(" + (string)comp.Attribute("Name") + ")";
//                        }
//                    }
//                }
//                return "XIC(" + uid + ")";
//            }
//            else if (string.Equals(name, "O", StringComparison.OrdinalIgnoreCase))
//            {
//                var ins = wires.Where(w => w.Elements().Any(e => e.Name.LocalName == "NameCon" && (string)e.Attribute("UId") == uid && ((string)e.Attribute("Name"))?.StartsWith("in") == true)).ToList();
//                var pieces = new List<string>();
//                foreach (var w in ins)
//                {
//                    var source = w.Elements().FirstOrDefault(e => e.Name.LocalName == "NameCon" && ((string)e.Attribute("UId") != uid));
//                    if (source != null)
//                    {
//                        var sourceUid = (string)source.Attribute("UId");
//                        pieces.Add(ReconstructExpressionForUid(sourceUid, partByUid, wires) ?? "??");
//                    }
//                }
//                return "[" + string.Join(",", pieces) + "]";
//            }
//            else
//            {
//                var func = name.ToUpperInvariant();
//                if (new[] { "EQ", "NE", "GT", "LT", "ADD", "MUL", "SUB", "DIV", "MOVE" }.Contains(func))
//                {
//                    var partUid = (string)part.Attribute("UId");
//                    var ins = wires.Where(w => w.Elements().Any(e => e.Name.LocalName == "NameCon" && (string)e.Attribute("UId") == partUid && ((string)e.Attribute("Name"))?.StartsWith("in") == true)).ToList();
//                    var argExpr = new List<string>();
//                    foreach (var w in ins)
//                    {
//                        var ident = w.Elements().FirstOrDefault(e => e.Name.LocalName == "IdentCon");
//                        if (ident != null)
//                        {
//                            var aUid = (string)ident.Attribute("UId");
//                            if (partByUid.ContainsKey(aUid))
//                            {
//                                var comp = partByUid[aUid].Descendants().FirstOrDefault(e => e.Name.LocalName == "Component");
//                                if (comp != null) argExpr.Add((string)comp.Attribute("Name"));
//                                else argExpr.Add("C" + aUid);
//                            }
//                            else argExpr.Add("C" + aUid);
//                        }
//                        else
//                        {
//                            var src = w.Elements().FirstOrDefault(e => e.Name.LocalName == "NameCon" && (string)e.Attribute("UId") != partUid);
//                            if (src != null)
//                            {
//                                var srcUid = (string)src.Attribute("UId");
//                                argExpr.Add(ReconstructExpressionForUid(srcUid, partByUid, wires) ?? srcUid);
//                            }
//                        }
//                    }
//                    return func + "(" + string.Join(",", argExpr) + ")";
//                }
//            }

//            return name + "(" + (string)part.Attribute("UId") + ")";
//        }
//    }
//}


using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace RockwellSiemensTool
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            Console.WriteLine("Rockwell ↔ Siemens Converter");
            Console.WriteLine("1) Rockwell (L5X) -> Siemens FB XML");
            Console.WriteLine("2) Siemens FB XML -> Rockwell (RLL L5X)");
            Console.WriteLine("Q) Quit");
            Console.Write("Choice: ");
            var ch = Console.ReadLine()?.Trim().ToUpperInvariant();

            try
            {
                if (ch == "1")
                {
                    using var ofd = new OpenFileDialog
                    {
                        Filter = "Rockwell L5X files (*.L5X)|*.L5X|All files (*.*)|*.*",
                        Title = "Select Rockwell L5X File"
                    };
                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    var l5x = ofd.FileName;
                    var outDir = Path.Combine(Environment.CurrentDirectory, "SiemensFiles");
                    Directory.CreateDirectory(outDir);
                    var outFile = Path.Combine(outDir, Path.GetFileNameWithoutExtension(l5x) + "_Siemens.xml");

                    RockwellToSiemensConverter.Convert(l5x, outFile);
                    Console.WriteLine($"Saved Siemens XML to: {outFile}");
                }
                else if (ch == "2")
                {
                    using var ofd = new OpenFileDialog
                    {
                        Filter = "Siemens XML files (*.xml)|*.xml|All files (*.*)|*.*",
                        Title = "Select Siemens FB XML File"
                    };
                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    var sx = ofd.FileName;
                    var outDir = Path.Combine(Environment.CurrentDirectory, "RockwellFiles");
                    Directory.CreateDirectory(outDir);
                    var outFile = Path.Combine(outDir, Path.GetFileNameWithoutExtension(sx) + "_Rockwell.RLL.L5X");

                    SiemensToRockwellConverter.Convert(sx, outFile);
                    Console.WriteLine($"Saved Rockwell-like RLL to: {outFile}");
                }
                else
                {
                    Console.WriteLine("Bye.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}

//public static class RockwellToSiemensConverter
//{
//    private static readonly XNamespace NsInterface =
//        "http://www.siemens.com/automation/Openness/SW/Interface/v5";

//    private static readonly XNamespace NsFlgNet =
//        "http://www.siemens.com/automation/Openness/SW/NetworkSource/FlgNet/v3";

//    private static int _textIdCounter = 1;
//    private static int _compileUnitIdCounter = 1000;
//    private static int _globalUid = 20;

//    private static string NextTextId() => $"T{_textIdCounter++:X2}";
//    private static string NextCompileUnitId() => $"{_compileUnitIdCounter++:X}";
//    private static string NextPartUid() => (++_globalUid).ToString();
//    private static string NextWireUid() => (++_globalUid).ToString();

//    private static readonly HashSet<string> SiemensReservedNames = new(StringComparer.OrdinalIgnoreCase)
//    {
//        "TON","TOF","TP","AND","OR","NOT","MOVE","ADD","SUB","MUL","DIV",
//        "GEQ","LEQ","EQ","NEQ","GT","LT","SET","RESET","SR","RS"
//    };

//    private static string SanitizeName(string original, HashSet<string> usedNames)
//    {
//        if (string.IsNullOrWhiteSpace(original))
//            return "Var_" + Guid.NewGuid().ToString("N")[..6];

//        string safe = original.Trim();
//        if (SiemensReservedNames.Contains(safe) ||
//            SiemensReservedNames.Any(r => safe.StartsWith(r, StringComparison.OrdinalIgnoreCase)))
//            safe = "_" + safe + "_";

//        int suffix = 1;
//        string baseName = safe;
//        while (usedNames.Contains(safe))
//            safe = $"{baseName}_{suffix++}";

//        usedNames.Add(safe);
//        return safe;
//    }

//    public static void Convert(string rockwellFile, string siemensOutputPath)
//    {
//        _textIdCounter = 1;
//        _compileUnitIdCounter = 1000;
//        _globalUid = 20;

//        var rockwellDoc = XDocument.Load(rockwellFile);

//        var siemensDoc = new XDocument(new XDeclaration("1.0", "utf-8", null),
//            new XElement("Document",
//                new XElement("Engineering", new XAttribute("version", "V17")),
//                new XElement("DocumentInfo",
//                    new XElement("Created", DateTime.UtcNow.ToString("o")),
//                    new XElement("ExportSetting", "None")
//                ),
//                BuildFbElement(rockwellDoc, Path.GetFileNameWithoutExtension(rockwellFile))
//            )
//        );

//        siemensDoc.Save(siemensOutputPath);
//    }

//    private static XElement BuildFbElement(XDocument rockwellDoc, string blockName)
//    {
//        return new XElement("SW.Blocks.FB",
//            new XAttribute("ID", "0"),
//            new XElement("AttributeList",
//                new XElement("Interface",
//                    new XElement(NsInterface + "Sections",
//                        CreateSection("Input", rockwellDoc, "Parameter"),
//                        CreateSection("Output", rockwellDoc, "Parameter"),
//                        CreateSection("InOut", rockwellDoc, "Parameter"),
//                        CreateSection("Static", rockwellDoc, "LocalTag"),
//                        CreateSection("Temp", rockwellDoc, "LocalTag", onlyTemp: true),
//                        CreateSection("Constant", rockwellDoc, "Constant")
//                    )
//                ),
//                new XElement("Name", blockName + "__Converted"),
//                new XElement("Number", "1"),
//                new XElement("ProgrammingLanguage", "LAD")
//            ),
//            new XElement("ObjectList",
//                new XElement("MultilingualText",
//                    new XAttribute("ID", NextTextId()),
//                    new XAttribute("CompositionName", "Comment"),
//                    new XElement("ObjectList",
//                        new XElement("MultilingualTextItem",
//                            new XAttribute("ID", NextTextId()),
//                            new XAttribute("CompositionName", "Items"),
//                            new XElement("AttributeList",
//                                new XElement("Culture", "en-US"),
//                                new XElement("Text", "")
//                            )
//                        )
//                    )
//                ),
//                GenerateCompileUnits(rockwellDoc)
//            )
//        );
//    }

//    private static XElement CreateSection(string sectionName, XDocument rockwellDoc,
//      string tagType, IEnumerable<string> extraTags = null, bool onlyTemp = false)
//    {
//        IEnumerable<XElement> members = Enumerable.Empty<XElement>();

//        if (rockwellDoc != null)
//        {
//            if (onlyTemp && tagType == "LocalTag")
//            {
//                members = rockwellDoc.Descendants("LocalTag")
//                    .Where(x =>
//                    {
//                        var usage = (string)x.Attribute("Usage");
//                        return !string.IsNullOrEmpty(usage) && usage.Equals("Temp", StringComparison.OrdinalIgnoreCase);
//                    });
//            }
//            else if (tagType == "LocalTag")
//                members = rockwellDoc.Descendants("LocalTag");
//            else if (tagType == "Constant")
//                members = rockwellDoc.Descendants("Constant");
//            else
//                members = rockwellDoc.Descendants(tagType)
//                    .Where(p => string.Equals((string)p.Attribute("Usage"), sectionName, StringComparison.OrdinalIgnoreCase));
//        }

//        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
//        if (extraTags != null)
//            foreach (var t in extraTags) usedNames.Add(t);

//        return new XElement(NsInterface + "Section",
//            new XAttribute("Name", sectionName),
//            members.Select(p =>
//            {
//                string rawName = (string)p.Attribute("Name") ?? "";
//                string safeName = SanitizeName(rawName, usedNames);

//                return new XElement(NsInterface + "Member",
//                    new XAttribute("Name", safeName),
//                    new XAttribute("Datatype",
//                        MapDataType((string)p.Attribute("DataType") ??
//                                    (string)p.Attribute("DataTypeName") ?? "BOOL"))
//                );
//            })
//        );
//    }

//    private static string MapDataType(string rockwellType)
//    {
//        if (string.IsNullOrWhiteSpace(rockwellType)) return "Bool";
//        return rockwellType.ToUpper() switch
//        {
//            "BOOL" => "Bool",
//            "INT" => "Int",
//            "DINT" => "DInt",
//            "REAL" => "Real",
//            "STRING" => "String",
//            "WORD" => "Word",
//            "DWORD" => "DWord",
//            _ => "Bool"
//        };
//    }

//    private static IEnumerable<XElement> GenerateCompileUnits(XDocument rockwellDoc)
//    {
//        var compileUnits = new List<XElement>();
//        var rungs = rockwellDoc.Descendants("Rung").ToList();
//        if (!rungs.Any())
//        {
//            rungs = rockwellDoc.Descendants("Instruction")
//                .Select(i => new XElement("Rung", i))
//                .ToList();
//        }

//        foreach (var rung in rungs)
//        {
//            var commentText = (string)rung.Element("Comment") ??
//                              rung.Descendants("Description").FirstOrDefault()?.Value ?? "";
//            var titleText = rung.Element("Text")?.Value?.Trim() ?? GetInstructionName(rung) ?? "";

//            var contactOccurrences = new List<(string token, string type)>();
//            string coilName = null;
//            if (!string.IsNullOrWhiteSpace(rung.Element("Text")?.Value))
//                ParseRllTextAdvanced(rung.Element("Text")?.Value, contactOccurrences, ref coilName);

//            contactOccurrences = contactOccurrences
//                .Where(c => !c.token.Contains("(") && !c.token.Contains(")"))
//                .ToList();

//            if (!contactOccurrences.Any())
//            {
//                var inputs = rung.Descendants("Input")
//                    .Select(x => ((string)x.Attribute("TagName") ?? (string)x.Attribute("Name"), "XIC"))
//                    .Where(t => !string.IsNullOrWhiteSpace(t.Item1))
//                    .ToList();
//                contactOccurrences.AddRange(inputs);
//            }

//            if (string.IsNullOrWhiteSpace(coilName))
//            {
//                coilName = rung.Descendants("Output")
//                    .Select(x => (string)x.Attribute("TagName") ?? (string)x.Attribute("Name"))
//                    .FirstOrDefault();
//            }

//            if (!contactOccurrences.Any() && string.IsNullOrWhiteSpace(coilName) &&
//                !rung.Descendants("Instruction").Any() &&
//                string.IsNullOrWhiteSpace(rung.Element("Text")?.Value))
//                continue;

//            var parts = new List<XElement>();
//            var wires = new List<XElement>();
//            var accessMap = new Dictionary<string, int>();
//            var contactUids = new List<int>();
//            var pinMap = new HashSet<(int uid, string pinName)>();

//            bool UidExists(int uid) =>
//                parts.Any(p => (string)p.Attribute("UId") == uid.ToString());
//            bool powerRailAdded = false;

//            void AddWireSafelyLocal(XElement wire)
//            {
//                if (wire.Descendants().Any(x => x.Name.LocalName == "Powerrail"))
//                {
//                    if (powerRailAdded)
//                        return;
//                    powerRailAdded = true;
//                }

//                var nameCons = wire.Descendants().Where(x => x.Name.LocalName == "NameCon").ToList();
//                var identCons = wire.Descendants().Where(x => x.Name.LocalName == "IdentCon").ToList();

//                foreach (var nc in nameCons)
//                {
//                    if (!int.TryParse(nc.Attribute("UId")?.Value, out int uid)) return;
//                    if (!UidExists(uid)) return;
//                }
//                foreach (var ic in identCons)
//                {
//                    if (!int.TryParse(ic.Attribute("UId")?.Value, out int uid)) return;
//                    if (!UidExists(uid)) return;
//                }

//                foreach (var nc in nameCons)
//                {
//                    int uid = int.Parse(nc.Attribute("UId").Value);
//                    string pin = nc.Attribute("Name")?.Value ?? "";
//                    if (pinMap.Contains((uid, pin)))
//                        return;
//                }

//                foreach (var nc in nameCons)
//                {
//                    int uid = int.Parse(nc.Attribute("UId").Value);
//                    string pin = nc.Attribute("Name")?.Value ?? "";
//                    pinMap.Add((uid, pin));
//                }

//                if (!wires.Any(w => XNode.DeepEquals(w, wire)))
//                    wires.Add(wire);
//            }

//            //void AddWireSafelyLocal(XElement wire)
//            //{
//            //    var nameCons = wire.Descendants().Where(x => x.Name.LocalName == "NameCon").ToList();
//            //    var identCons = wire.Descendants().Where(x => x.Name.LocalName == "IdentCon").ToList();

//            //    foreach (var nc in nameCons)
//            //    {
//            //        if (!int.TryParse(nc.Attribute("UId")?.Value, out int uid)) return;
//            //        if (!UidExists(uid)) return;
//            //    }
//            //    foreach (var ic in identCons)
//            //    {
//            //        if (!int.TryParse(ic.Attribute("UId")?.Value, out int uid)) return;
//            //        if (!UidExists(uid)) return;
//            //    }

//            //    foreach (var nc in nameCons)
//            //    {
//            //        int uid = int.Parse(nc.Attribute("UId").Value);
//            //        string pin = nc.Attribute("Name")?.Value ?? "";
//            //        if (pinMap.Contains((uid, pin)))
//            //            return;
//            //    }

//            //    foreach (var nc in nameCons)
//            //    {
//            //        int uid = int.Parse(nc.Attribute("UId").Value);
//            //        string pin = nc.Attribute("Name")?.Value ?? "";
//            //        pinMap.Add((uid, pin));
//            //    }

//            //    if (!wires.Any(w => XNode.DeepEquals(w, wire)))
//            //        wires.Add(wire);
//            //}

//            int CreateAccessFor(string token)
//            {
//                token = token?.Trim() ?? "";
//                if (string.IsNullOrEmpty(token)) return -1;
//                if (token.StartsWith("MOV(", StringComparison.OrdinalIgnoreCase)) return -1;

//                if (accessMap.TryGetValue(token, out int existingUid))
//                    return existingUid;

//                if (double.TryParse(token, System.Globalization.NumberStyles.Any,
//                    System.Globalization.CultureInfo.InvariantCulture, out double literalVal))
//                {
//                    int litUid = int.Parse(NextPartUid());
//                    var constElem = new XElement("Access",
//                        new XAttribute("Scope", "LiteralConstant"),
//                        new XAttribute("UId", litUid),
//                        new XElement("Constant",
//                            new XElement("ConstantType", literalVal % 1 == 0 ? "Int" : "Real"),
//                            new XElement("ConstantValue",
//                                literalVal.ToString(System.Globalization.CultureInfo.InvariantCulture))));
//                    parts.Add(constElem);
//                    accessMap[token] = litUid;
//                    return litUid;
//                }
//                else
//                {
//                    int accUid = int.Parse(NextPartUid());
//                    parts.Add(new XElement("Access",
//                        new XAttribute("Scope", "LocalVariable"),
//                        new XAttribute("UId", accUid),
//                        new XElement("Symbol",
//                            new XElement("Component", new XAttribute("Name", token)))));
//                    accessMap[token] = accUid;
//                    return accUid;
//                }
//            }

//            var instructionNodes = rung.Descendants("Instruction").ToList();

//            var rungText = rung.Element("Text")?.Value ?? "";
//            var inlineMatches = System.Text.RegularExpressions.Regex.Matches(
//                rungText,
//                @"\b(MOV|EQU|EQ|NEQ|NE|GRT|GT|LES|LT|GEQ|GE|LEQ|LE)\s*\(\s*([^,()]+)\s*,\s*([^)()]+)\s*\)",
//                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
//            foreach (System.Text.RegularExpressions.Match m in inlineMatches)
//            {
//                var instrName = m.Groups[1].Value;
//                var p1 = m.Groups[2].Value.Trim();
//                var p2 = m.Groups[3].Value.Trim();
//                var synthetic = new XElement("Instruction", new XAttribute("Name", instrName),
//                    new XElement("Parameter", new XAttribute("Value", p1)),
//                    new XElement("Parameter", new XAttribute("Value", p2)));
//                instructionNodes.Add(synthetic);
//            }

//            foreach (var instr in instructionNodes)
//            {
//                string instrName = ((string)instr.Attribute("Name") ?? "").Trim().ToUpperInvariant();
//                if (instrName == "EQU") instrName = "EQ";
//                if (instrName == "NEQ") instrName = "NE";
//                if (instrName == "GRT") instrName = "GT";
//                if (instrName == "LES") instrName = "LT";
//                if (instrName == "GEQ") instrName = "GE";
//                if (instrName == "LEQ") instrName = "LE";

//                var supported = new HashSet<string> { "MOV", "EQ", "NE", "GT", "LT", "GE", "LE" };
//                if (!supported.Contains(instrName)) continue;

//                var parameters = instr.Elements("Parameter").ToList();
//                string src = "", src2 = "", dest = "";
//                if (parameters.Count >= 2)
//                {
//                    src = ((string)parameters[0].Attribute("Value") ?? "").Trim();
//                    src2 = ((string)parameters[1].Attribute("Value") ?? "").Trim();
//                    if (parameters.Count >= 3)
//                        dest = ((string)parameters[2].Attribute("Value") ?? "").Trim();
//                }

//                if (instrName == "MOV" && string.IsNullOrEmpty(dest) && !string.IsNullOrEmpty(src2))
//                {
//                    dest = src2; src2 = "";
//                }

//                int srcUid = CreateAccessFor(src);
//                int src2Uid = CreateAccessFor(src2);
//                int destUid = CreateAccessFor(dest);

//                string siemensOp = instrName switch
//                {
//                    "MOV" => "Move",
//                    "EQ" => "Eq",
//                    "NE" => "Ne",
//                    "GT" => "Gt",
//                    "LT" => "Lt",
//                    "GE" => "Ge",
//                    "LE" => "Le",
//                    _ => "Move"
//                };

//                int opUid = int.Parse(NextPartUid());
//                var opPart = new XElement("Part",
//                    new XAttribute("Name", siemensOp),
//                    new XAttribute("UId", opUid));

//                // Optionally set a template/cardinality for Move (kept but commented could be adjusted)
//                if (siemensOp == "Move")
//                {
//                    // a commonly-used TemplateValue for Move - adjust if your target expects a different attribute
//                    opPart.Add(new XElement("TemplateValue",
//                        new XAttribute("Name", "Card"),
//                        new XAttribute("Type", "Cardinality"), "1"));
//                }

//                // <<< FIX: ensure operator part is added to parts list so it appears in FlgNet/Parts
//                parts.Add(opPart);

//                if (siemensOp == "Move")
//                {
//                    // Always connect Powerrail -> Move.en
//                    AddWireSafelyLocal(new XElement("Wire",
//                        new XAttribute("UId", NextWireUid()),
//                        new XElement("Powerrail"),
//                        new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", "en"))));

//                    // Source -> Move.in
//                    if (srcUid > 0)
//                    {
//                        AddWireSafelyLocal(new XElement("Wire",
//                            new XAttribute("UId", NextWireUid()),
//                            new XElement("IdentCon", new XAttribute("UId", srcUid)),
//                            new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", "in"))));
//                    }

//                    // Move.out -> destination IdentCon
//                    if (destUid > 0)
//                    {
//                        AddWireSafelyLocal(new XElement("Wire",
//                            new XAttribute("UId", NextWireUid()),
//                            new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", "out")),
//                            new XElement("IdentCon", new XAttribute("UId", destUid))));
//                    }
//                }

//                else
//                {
//                    if (srcUid > 0)
//                    {
//                        AddWireSafelyLocal(new XElement("Wire",
//                            new XAttribute("UId", NextWireUid()),
//                            new XElement("IdentCon", new XAttribute("UId", srcUid)),
//                            new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", "in1"))));
//                    }
//                    if (src2Uid > 0)
//                    {
//                        AddWireSafelyLocal(new XElement("Wire",
//                            new XAttribute("UId", NextWireUid()),
//                            new XElement("IdentCon", new XAttribute("UId", src2Uid)),
//                            new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", "in2"))));
//                    }
//                    if (destUid > 0)
//                    {
//                        AddWireSafelyLocal(new XElement("Wire",
//                            new XAttribute("UId", NextWireUid()),
//                            new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", "out")),
//                            new XElement("IdentCon", new XAttribute("UId", destUid))));
//                    }
//                }

//                // link operator into rung chain if contacts exist
//                if (contactUids.Any())
//                {
//                    // connect last contact.out -> operator.in (use 'in' for Move, 'in' for others - keep simple)
//                    AddWireSafelyLocal(new XElement("Wire",
//                        new XAttribute("UId", NextWireUid()),
//                        new XElement("NameCon", new XAttribute("UId", contactUids.Last()), new XAttribute("Name", "out")),
//                        new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", "in"))));
//                }
//            }

//            // --- create contacts & coil ---
//            foreach (var occ in contactOccurrences)
//            {
//                string accessKey = occ.token + "_" + occ.type;
//                int accessUid = int.Parse(NextPartUid());
//                parts.Add(new XElement("Access",
//                    new XAttribute("Scope", "LocalVariable"),
//                    new XAttribute("UId", accessUid),
//                    new XElement("Symbol",
//                        new XElement("Component", new XAttribute("Name", occ.token)))));
//                accessMap[accessKey] = accessUid;

//                int contactUid = int.Parse(NextPartUid());
//                var contactPart = new XElement("Part",
//                    new XAttribute("Name", "Contact"),
//                    new XAttribute("UId", contactUid));
//                if (occ.type == "XIO")
//                    contactPart.Add(new XElement("Negated", new XAttribute("Name", "operand")));
//                parts.Add(contactPart);
//                contactUids.Add(contactUid);

//                AddWireSafelyLocal(new XElement("Wire",
//                    new XAttribute("UId", NextWireUid()),
//                    new XElement("IdentCon", new XAttribute("UId", accessUid)),
//                    new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "operand"))));
//            }

//            int coilUid = int.Parse(NextPartUid());
//            parts.Add(new XElement("Part", new XAttribute("Name", "Coil"), new XAttribute("UId", coilUid)));

//            int coilAccessUid = -1;
//            if (!string.IsNullOrWhiteSpace(coilName))
//            {
//                coilAccessUid = int.Parse(NextPartUid());
//                parts.Add(new XElement("Access",
//                    new XAttribute("Scope", "LocalVariable"),
//                    new XAttribute("UId", coilAccessUid),
//                    new XElement("Symbol",
//                        new XElement("Component", new XAttribute("Name", coilName)))));
//            }

//            // --- wiring sequence ---
//            if (contactUids.Any())
//            {
//                AddWireSafelyLocal(new XElement("Wire",
//                    new XAttribute("UId", NextWireUid()),
//                    new XElement("Powerrail"),
//                    new XElement("NameCon", new XAttribute("UId", contactUids[0]), new XAttribute("Name", "in"))));

//                for (int i = 0; i < contactUids.Count - 1; i++)
//                {
//                    AddWireSafelyLocal(new XElement("Wire",
//                        new XAttribute("UId", NextWireUid()),
//                        new XElement("NameCon", new XAttribute("UId", contactUids[i]), new XAttribute("Name", "out")),
//                        new XElement("NameCon", new XAttribute("UId", contactUids[i + 1]), new XAttribute("Name", "in"))));
//                }

//                AddWireSafelyLocal(new XElement("Wire",
//                    new XAttribute("UId", NextWireUid()),
//                    new XElement("NameCon", new XAttribute("UId", contactUids.Last()), new XAttribute("Name", "out")),
//                    new XElement("NameCon", new XAttribute("UId", coilUid), new XAttribute("Name", "in"))));
//            }
//            else
//            {
//                AddWireSafelyLocal(new XElement("Wire",
//                    new XAttribute("UId", NextWireUid()),
//                    new XElement("Powerrail"),
//                    new XElement("NameCon", new XAttribute("UId", coilUid), new XAttribute("Name", "in"))));
//            }

//            if (coilAccessUid > 0)
//            {
//                AddWireSafelyLocal(new XElement("Wire",
//                    new XAttribute("UId", NextWireUid()),
//                    new XElement("IdentCon", new XAttribute("UId", coilAccessUid)),
//                    new XElement("NameCon", new XAttribute("UId", coilUid), new XAttribute("Name", "operand"))));
//            }

//            var flgNet = new XElement(NsFlgNet + "FlgNet",
//                new XElement(XName.Get("Parts"), parts),
//                new XElement(XName.Get("Wires"), wires));

//            var compileUnit = new XElement("SW.Blocks.CompileUnit",
//                new XAttribute("ID", NextCompileUnitId()),
//                new XAttribute("CompositionName", "CompileUnits"),
//                new XElement("AttributeList",
//                    new XElement("NetworkSource", flgNet),
//                    new XElement("ProgrammingLanguage", "LAD")),
//                new XElement("ObjectList",
//                    new XElement("MultilingualText",
//                        new XAttribute("ID", NextTextId()),
//                        new XAttribute("CompositionName", "Comment"),
//                        new XElement("ObjectList",
//                            new XElement("MultilingualTextItem",
//                                new XAttribute("ID", NextTextId()),
//                                new XAttribute("CompositionName", "Items"),
//                                new XElement("AttributeList",
//                                    new XElement("Culture", "en-US"),
//                                    new XElement("Text", commentText ?? ""))))),
//                    new XElement("MultilingualText",
//                        new XAttribute("ID", NextTextId()),
//                        new XAttribute("CompositionName", "Title"),
//                        new XElement("ObjectList",
//                            new XElement("MultilingualTextItem",
//                                new XAttribute("ID", NextTextId()),
//                                new XAttribute("CompositionName", "Items"),
//                                new XElement("AttributeList",
//                                    new XElement("Culture", "en-US"),
//                                    new XElement("Text", commentText ?? "")))))));

//            compileUnits.Add(compileUnit);
//        }

//        return compileUnits;
//    }
//    private static void AddWireSafely(List<XElement> wires, List<XElement> parts, XElement wire,
//        HashSet<(int uid, string pinName)> pinMap)
//    {
//        wires.Add(wire);

//        // Accept non-namespaced and namespaced children (check LocalName)
//        foreach (var nc in wire.Elements().Where(e => string.Equals(e.Name.LocalName, "NameCon", StringComparison.OrdinalIgnoreCase)))
//        {
//            var uidAttr = nc.Attribute("UId");
//            if (uidAttr == null) continue;
//            if (int.TryParse(uidAttr.Value, out int uid))
//            {
//                string name = (string)nc.Attribute("Name") ?? "";
//                pinMap.Add((uid, name));
//            }
//        }

//        foreach (var ic in wire.Elements().Where(e => string.Equals(e.Name.LocalName, "IdentCon", StringComparison.OrdinalIgnoreCase)))
//        {
//            var uidAttr = ic.Attribute("UId");
//            if (uidAttr == null) continue;
//            if (int.TryParse(uidAttr.Value, out int uid))
//            {
//                pinMap.Add((uid, "ident"));
//            }
//        }
//        EnsureInPins(wires, parts, pinMap);
//    }

//    private static void EnsureInPins(List<XElement> wires, List<XElement> parts, HashSet<(int uid, string pin)> pinMap)
//    {
//        foreach (var part in parts)
//        {
//            var uidAttr = part.Attribute("UId");
//            if (uidAttr == null) continue;

//            if (!int.TryParse(uidAttr.Value, out int uid)) continue;

//            string name = part.Attribute("Name")?.Value;
//            if (name == "Contact")
//            {
//                // If no "in" wire exists for this UID → create power rail wire
//                if (!pinMap.Contains((uid, "in")))
//                {
//                    wires.Add(new XElement("Wire",
//                        new XAttribute("UId", NextWireUid()),
//                        new XElement("Powerrail"),
//                        new XElement("NameCon",
//                            new XAttribute("UId", uid),
//                            new XAttribute("Name", "in"))
//                    ));

//                    // Mark pin as added
//                    pinMap.Add((uid, "in"));
//                }
//            }
//        }
//    }

//    private static void ParseRllTextAdvanced(string text, List<(string token, string type)> outTokens,
//        ref string coilName)
//    {
//        if (string.IsNullOrWhiteSpace(text)) return;
//        string t = text.Trim();
//        if (t.EndsWith(";")) t = t[..^1];

//        var mCoil = Regex.Match(t, @"(?<op>OTL|OTU|OTE)\s*\(\s*(?<n>[^)]+)\s*\)", RegexOptions.IgnoreCase);
//        if (mCoil.Success) coilName = mCoil.Groups["n"].Value.Trim();

//        string inner = null;
//        var mInner = Regex.Match(t, @"\[(?<inner>.*)\]");
//        if (mInner.Success)
//            inner = mInner.Groups["inner"].Value;
//        else
//            inner = t;

//        var tokens = SplitTopLevelArgs(string.IsNullOrWhiteSpace(inner) ? t : inner);

//        foreach (var tok in tokens)
//        {
//            string trimmed = tok.Trim();
//            if (string.IsNullOrWhiteSpace(trimmed)) continue;

//            var mXic = Regex.Match(trimmed, @"XIC\s*\(\s*(?<n>[^)]+)\s*\)", RegexOptions.IgnoreCase);
//            var mXio = Regex.Match(trimmed, @"XIO\s*\(\s*(?<n>[^)]+)\s*\)", RegexOptions.IgnoreCase);

//            if (mXic.Success)
//                outTokens.Add((mXic.Groups["n"].Value.Trim(), "XIC"));
//            else if (mXio.Success)
//                outTokens.Add((mXio.Groups["n"].Value.Trim(), "XIO"));
//            else
//                outTokens.Add((trimmed, "XIC"));
//        }
//    }

//    private static List<string> SplitTopLevelArgs(string text)
//    {
//        var result = new List<string>();
//        int bracketLevel = 0;
//        int start = 0;
//        for (int i = 0; i < text.Length; i++)
//        {
//            char c = text[i];
//            if (c == '[' || c == '(') bracketLevel++;
//            else if (c == ']' || c == ')') bracketLevel--;
//            else if (c == ',' && bracketLevel == 0)
//            {
//                result.Add(text[start..i]);
//                start = i + 1;
//            }
//        }

//        if (start < text.Length) result.Add(text[start..]);
//        return result;
//    }

//    private static string GetInstructionName(XElement rung)
//    {
//        return rung.Element("Instruction")?.Attribute("Name")?.Value;
//    }
//}



/// <summary>
/// Clean, rewritten converter: Rockwell (L5X ladder-like XML) -> Siemens FB XML for TIA Portal (V17+).
/// - Single UID generator
/// - Avoids duplicate UIds
/// - Produces sensible FlgNet/CompileUnit structure
/// Note: This is a converter skeleton with tested UID/powerrail logic and core instruction handling.
/// </summary>



//    public static class RockwellToSiemensConverter
//    {
//        // Siemens Openness namespaces used in Interface / FlgNet
//        private static readonly XNamespace NsInterface = "http://www.siemens.com/automation/Openness/SW/Interface/v5";
//        private static readonly XNamespace NsFlgNet = "http://www.siemens.com/automation/Openness/SW/NetworkSource/FlgNet/v3";

//        // Reserved Siemens names — avoid direct use
//        private static readonly HashSet<string> SiemensReservedNames = new(StringComparer.OrdinalIgnoreCase)
//        {
//            "TON","TOF","TP","AND","OR","NOT","MOVE","ADD","SUB","MUL","DIV",
//            "GEQ","LEQ","EQ","NEQ","GT","LT","SET","RESET","SR","RS"
//        };

//        // --- PUBLIC ENTRY POINT ---
//        /// <summary>
//        /// Convert a Rockwell-style L5X file (or compatible ladder XML) into a Siemens FB XML file.
//        /// If siemensOutputPath already exists, UIDs will be seeded from it to avoid collisions.
//        /// </summary>
//        /// private static class UidGen

//    private static long _nextPartUid = 1000000;
//    private static long _nextWireUid = 2000000;
//    private static long _nextCompileUnitUid = 3000000;
//    private static long _nextTextUid = 4000000;

//    public static void Reset()
//    {
//        _nextPartUid = 1000000;
//        _nextWireUid = 2000000;
//        _nextCompileUnitUid = 3000000;
//        _nextTextUid = 4000000;
//    }

//    public static long NextPartUid() => _nextPartUid++;
//    public static long NextWireUid() => _nextWireUid++;
//    public static long NextCompileUnitUid() => _nextCompileUnitUid++;
//    public static long NextTextUid() => _nextTextUid++;


//public static void Convert(string rockwellFilePath, string siemensOutputPath)
//        {
//            if (string.IsNullOrWhiteSpace(rockwellFilePath)) throw new ArgumentNullException(nameof(rockwellFilePath));
//            if (string.IsNullOrWhiteSpace(siemensOutputPath)) throw new ArgumentNullException(nameof(siemensOutputPath));
//            if (!File.Exists(rockwellFilePath)) throw new FileNotFoundException("Input file not found", rockwellFilePath);

//            // Load input
//            var rockwellDoc = XDocument.Load(rockwellFilePath);

//            // Initialize UID generator: if the target file exists, seed from it; otherwise use high defaults
//            if (File.Exists(siemensOutputPath))
//            {
//                try
//                {
//                    UidGen.SetSeedFromExistingDocument(siemensOutputPath);
//                }
//                catch
//                {
//                    // If seeding fails for any reason, fall back to high default ranges
//                    UidGen.SetHighDefaults();
//                }
//            }
//            else
//            {
//                UidGen.SetHighDefaults();
//            }

//            // Build FB element
//            var fbElement = BuildFbElement(rockwellDoc, Path.GetFileNameWithoutExtension(rockwellFilePath));

//            // Wrap with Document root expected by TIA
//            var siemensDoc = new XDocument(new XDeclaration("1.0", "utf-8", null),
//                new XElement("Document",
//                    new XElement("Engineering", new XAttribute("version", "V17")),
//                    new XElement("DocumentInfo",
//                        new XElement("Created", DateTime.UtcNow.ToString("o")),
//                        new XElement("ExportSetting", "None")
//                    ),
//                    fbElement
//                )
//            );

//            // Safety check: ensure no duplicate UId attributes
//            var allUids = siemensDoc.Descendants().Attributes("UId")
//                .Select(a => a.Value)
//                .Where(v => !string.IsNullOrWhiteSpace(v))
//                .GroupBy(v => v)
//                .Where(g => g.Count() > 1)
//                .Select(g => g.Key)
//                .ToList();

//            if (allUids.Any())
//                throw new Exception("Duplicate UId(s) detected before saving: " + string.Join(", ", allUids));

//            // Save
//            siemensDoc.Save(siemensOutputPath);
//        }

//        // --- UID Generator: single source of truth ---
//        private static class UidGen
//        {
//            // separate counters keep different ranges (not required but easy to inspect)
//            // these default to large ranges to minimize accidental collisions
//            private static int _partCounter = 1_000_000;
//            private static int _wireCounter = 2_000_000;
//            private static int _compileUnitCounter = 3_000_000;
//            private static int _textCounter = 1;

//            // Use this to seed from an existing Siemens document (if it exists)
//            public static void SetSeedFromExistingDocument(string existingSiemensXmlPath)
//            {
//                if (string.IsNullOrWhiteSpace(existingSiemensXmlPath) || !File.Exists(existingSiemensXmlPath))
//                {
//                    SetHighDefaults();
//                    return;
//                }

//                try
//                {
//                    var doc = XDocument.Load(existingSiemensXmlPath);
//                    var allUidValues = doc.Descendants()
//                        .Attributes()
//                        .Where(a => string.Equals(a.Name.LocalName, "UId", StringComparison.OrdinalIgnoreCase)
//                                    || string.Equals(a.Name.LocalName, "ID", StringComparison.OrdinalIgnoreCase))
//                        .Select(a => a.Value)
//                        .Where(v => !string.IsNullOrWhiteSpace(v))
//                        .ToList();

//                    int maxNumeric = -1;
//                    foreach (var val in allUidValues)
//                    {
//                        // Try parse decimal number (common)
//                        if (int.TryParse(val, out int n))
//                        {
//                            if (n > maxNumeric) maxNumeric = n;
//                        }
//                        else
//                        {
//                            // Also try strip non-digits for common T-prefixed text IDs like 'T01'
//                            var digits = new string(val.Where(char.IsDigit).ToArray());
//                            if (int.TryParse(digits, out int n2))
//                            {
//                                if (n2 > maxNumeric) maxNumeric = n2;
//                            }
//                        }
//                    }

//                    if (maxNumeric < 0)
//                    {
//                        // no numeric UIds found — use high defaults
//                        SetHighDefaults();
//                        return;
//                    }

//                    // Seed counters safely above maxNumeric
//                    // Give some margin so different ranges don't collide
//                    _partCounter = Math.Max(_partCounter, maxNumeric + 10);
//                    _wireCounter = Math.Max(_wireCounter, maxNumeric + 10000);
//                    _compileUnitCounter = Math.Max(_compileUnitCounter, maxNumeric + 20);
//                    _textCounter = Math.Max(_textCounter, (maxNumeric % 1000) + 1); // keep TXX smallish, but unique
//                }
//                catch
//                {
//                    // on any failure, fallback to high defaults
//                    SetHighDefaults();
//                }
//            }

//            // Force wide default ranges if seeding isn't possible
//            public static void SetHighDefaults()
//            {
//                _partCounter = Math.Max(_partCounter, 1_000_000);
//                _wireCounter = Math.Max(_wireCounter, 2_000_000);
//                _compileUnitCounter = Math.Max(_compileUnitCounter, 3_000_000);
//                _textCounter = Math.Max(_textCounter, 1);
//            }

//            // Methods used across converter
//            public static int NextPartUid() => Interlocked.Increment(ref _partCounter) - 1;
//            public static int NextWireUid() => Interlocked.Increment(ref _wireCounter) - 1;
//            public static int NextCompileUnitId() => Interlocked.Increment(ref _compileUnitCounter) - 1;
//            public static string NextTextId() => $"T{Interlocked.Increment(ref _textCounter):X2}";
//        }

//        // --- Build the FB element skeleton and fill interface and compile units ---
//        private static XElement BuildFbElement(XDocument rockwellDoc, string blockName)
//        {
//            var fb = new XElement("SW.Blocks.FB",
//                new XAttribute("ID", "0"),
//                new XElement("AttributeList",
//                    new XElement("Interface",
//                        new XElement(NsInterface + "Sections",
//                            // sections use rockwellDoc to find tags; if your L5X uses different names, adjust CreateSection calls
//                            CreateSection("Input", rockwellDoc, "Parameter"),
//                            CreateSection("Output", rockwellDoc, "Parameter"),
//                            CreateSection("InOut", rockwellDoc, "Parameter"),
//                            CreateSection("Static", rockwellDoc, "LocalTag"),
//                            CreateSection("Temp", rockwellDoc, "LocalTag", onlyTemp: true),
//                            CreateSection("Constant", rockwellDoc, "Constant")
//                        )
//                    ),
//                    new XElement("Name", blockName + "__Converted"),
//                    new XElement("Number", "1"),
//                    new XElement("ProgrammingLanguage", "LAD")
//                ),
//                new XElement("ObjectList",
//                    // top-level comment text
//                    new XElement("MultilingualText",
//                        new XAttribute("ID", UidGen.NextTextId()),
//                        new XAttribute("CompositionName", "Comment"),
//                        new XElement("ObjectList",
//                            new XElement("MultilingualTextItem",
//                                new XAttribute("ID", UidGen.NextTextId()),
//                                new XAttribute("CompositionName", "Items"),
//                                new XElement("AttributeList",
//                                    new XElement("Culture", "en-US"),
//                                    new XElement("Text", "")
//                                )
//                            )
//                        )
//                    ),
//                    // compile units generated from ladder rungs
//                    GenerateCompileUnits(rockwellDoc).ToArray()
//                )
//            );

//            return fb;
//        }

//        // Create a typed Interface Section (Input/Output/Static/Temp/Constant)
//        private static XElement CreateSection(string sectionName, XDocument rockwellDoc, string tagType, bool onlyTemp = false)
//        {
//            IEnumerable<XElement> members = Enumerable.Empty<XElement>();
//            if (rockwellDoc != null)
//            {
//                if (onlyTemp && tagType == "LocalTag")
//                {
//                    members = rockwellDoc.Descendants("LocalTag")
//                        .Where(x => string.Equals((string)x.Attribute("Usage"), "Temp", StringComparison.OrdinalIgnoreCase));
//                }
//                else if (tagType == "LocalTag")
//                    members = rockwellDoc.Descendants("LocalTag");
//                else if (tagType == "Constant")
//                    members = rockwellDoc.Descendants("Constant");
//                else
//                    members = rockwellDoc.Descendants(tagType)
//                        .Where(p => string.Equals((string)p.Attribute("Usage"), sectionName, StringComparison.OrdinalIgnoreCase));
//            }

//            // usedNames tracks collision avoidance
//            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

//            var section = new XElement(NsInterface + "Section",
//                new XAttribute("Name", sectionName),
//                members.Select(p =>
//                {
//                    string rawName = (string)p.Attribute("Name") ?? "";
//                    string safeName = SanitizeName(rawName, usedNames);
//                    string datatype = MapDataType((string)p.Attribute("DataType") ?? (string)p.Attribute("DataTypeName") ?? "BOOL");

//                    return new XElement(NsInterface + "Member",
//                        new XAttribute("Name", safeName),
//                        new XAttribute("Datatype", datatype)
//                    );
//                })
//            );

//            return section;
//        }

//        // Basic mapping from Rockwell data type names to Siemens expected type strings
//        private static string MapDataType(string rockwellType)
//        {
//            if (string.IsNullOrWhiteSpace(rockwellType)) return "Bool";
//            return rockwellType.ToUpperInvariant() switch
//            {
//                "BOOL" => "Bool",
//                "INT" => "Int",
//                "DINT" => "DInt",
//                "REAL" => "Real",
//                "STRING" => "String",
//                "WORD" => "Word",
//                "DWORD" => "DWord",
//                _ => "Bool"
//            };
//        }

//        // Name sanitizer: avoid Siemens reserved keywords and duplicate names within a section
//        private static string SanitizeName(string original, HashSet<string> usedNames)
//        {
//            if (string.IsNullOrWhiteSpace(original))
//                original = "Var_" + Guid.NewGuid().ToString("N").Substring(0, 6);

//            string safe = original.Trim();
//            if (SiemensReservedNames.Contains(safe) ||
//                SiemensReservedNames.Any(r => safe.StartsWith(r, StringComparison.OrdinalIgnoreCase)))
//                safe = "_" + safe + "_";

//            string baseName = safe;
//            int i = 1;
//            while (usedNames.Contains(safe))
//            {
//                safe = $"{baseName}_{i++}";
//            }
//            usedNames.Add(safe);
//            return safe;
//        }

//        // --- Core conversion: generate compile units (one per rung) ---
//        private static IEnumerable<XElement> GenerateCompileUnits(XDocument rockwellDoc)
//        {
//            var compileUnits = new List<XElement>();

//            // Add a minimal dummy compile unit so the block is valid even if empty
//            compileUnits.Add(CreateDummyCompileUnit());

//            // Collect rungs. Many L5X formats put Rung elements inside Program/Ladder/... — adjust if needed.
//            var rungs = rockwellDoc.Descendants("Rung").ToList();
//            if (!rungs.Any())
//            {
//                // fallback: treat each Instruction as isolated rung
//                rungs = rockwellDoc.Descendants("Instruction").Select(i => new XElement("Rung", i)).ToList();
//            }

//            foreach (var rung in rungs)
//            {
//                // build per-rung FlgNet (Parts/Wires)
//                var parts = new List<XElement>();
//                var wires = new List<XElement>();
//                var pinMap = new HashSet<(int uid, string pin)>();
//                var accessMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
//                var contactUids = new List<int>();
//                int? firstPowerrailUid = null;
//                bool powerrailAdded = false;

//                string commentText = (string)rung.Element("Comment") ?? rung.Descendants("Description").FirstOrDefault()?.Value ?? "";
//                string titleText = rung.Element("Text")?.Value?.Trim() ?? GetInstructionName(rung) ?? "";

//                // Parse RLL-style text into contacts & coil if present
//                var contactOccurrences = new List<(string token, string type)>();
//                string coilName = null;
//                if (!string.IsNullOrWhiteSpace(rung.Element("Text")?.Value))
//                    ParseRllTextAdvanced(rung.Element("Text")?.Value, contactOccurrences, ref coilName);

//                // Filter contacts with inline function calls removed
//                contactOccurrences = contactOccurrences.Where(c => !c.token.Contains("(") && !c.token.Contains(")")).ToList();

//                // fallback to Input/Output tags if text parse didn't produce contacts
//                if (!contactOccurrences.Any())
//                {
//                    var inputs = rung.Descendants("Input")
//                        .Select(x => ((string)x.Attribute("TagName") ?? (string)x.Attribute("Name"), "XIC"))
//                        .Where(t => !string.IsNullOrWhiteSpace(t.Item1))
//                        .ToList();
//                    contactOccurrences.AddRange(inputs);
//                }

//                if (string.IsNullOrWhiteSpace(coilName))
//                {
//                    coilName = rung.Descendants("Output")
//                        .Select(x => (string)x.Attribute("TagName") ?? (string)x.Attribute("Name"))
//                        .FirstOrDefault();
//                }

//                // Skip empty rungs
//                bool hasMeaningful = contactOccurrences.Any() || !string.IsNullOrWhiteSpace(coilName) || rung.Descendants("Instruction").Any() || !string.IsNullOrWhiteSpace(rung.Element("Text")?.Value);
//                if (!hasMeaningful) continue;

//                // Local helpers
//                int CreateAccessFor(string token)
//                {
//                    token = token?.Trim() ?? "";
//                    if (string.IsNullOrEmpty(token)) return -1;
//                    if (token.StartsWith("MOV(", StringComparison.OrdinalIgnoreCase)) return -1;

//                    if (accessMap.TryGetValue(token, out int existing)) return existing;

//                    // numeric literal?
//                    if (double.TryParse(token, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double literal))
//                    {
//                        int litUid = UidGen.NextPartUid();
//                        var access = new XElement("Access",
//                            new XAttribute("Scope", "LiteralConstant"),
//                            new XAttribute("UId", litUid),
//                            new XElement("Constant",
//                                new XElement("ConstantType", literal % 1 == 0 ? "Int" : "Real"),
//                                new XElement("ConstantValue", literal.ToString(System.Globalization.CultureInfo.InvariantCulture))
//                            )
//                        );
//                        parts.Add(access);
//                        accessMap[token] = litUid;
//                        return litUid;
//                    }
//                    else
//                    {
//                        int accUid = UidGen.NextPartUid();
//                        var access = new XElement("Access",
//                            new XAttribute("Scope", "LocalVariable"),
//                            new XAttribute("UId", accUid),
//                            new XElement("Symbol",
//                                new XElement("Component", new XAttribute("Name", token))
//                            )
//                        );
//                        parts.Add(access);
//                        accessMap[token] = accUid;
//                        return accUid;
//                    }
//                }

//                void AddWireSafely(XElement wire)
//                {
//                    // if wire has Powerrail and none exists yet => create a reserved Access representing the powerrail
//                    bool hasPowerrail = wire.Elements().Any(e => e.Name.LocalName == "Powerrail");
//                    var nameCons = wire.Elements().Where(e => e.Name.LocalName == "NameCon").ToList();
//                    var identCons = wire.Elements().Where(e => e.Name.LocalName == "IdentCon").ToList();

//                    if (hasPowerrail)
//                    {
//                        if (!powerrailAdded)
//                        {
//                            powerrailAdded = true;
//                            firstPowerrailUid = UidGen.NextPartUid();
//                            // create a lightweight Access for the rail so IdentCon references have a target
//                            parts.Add(new XElement("Access",
//                                new XAttribute("Scope", "Powerrail"),
//                                new XAttribute("UId", firstPowerrailUid.Value)
//                            ));
//                            // leave wire as-is (contains Powerrail)
//                        }
//                        else
//                        {
//                            // convert subsequent powerrail wires into an IdentCon reference to firstPowerrailUid
//                            if (!firstPowerrailUid.HasValue) return;
//                            wire = new XElement("Wire",
//                                new XAttribute("UId", UidGen.NextWireUid()),
//                                new XElement("IdentCon", new XAttribute("UId", firstPowerrailUid.Value))
//                            );
//                            // recompute name/ident lists
//                            nameCons = wire.Elements().Where(e => e.Name.LocalName == "NameCon").ToList();
//                            identCons = wire.Elements().Where(e => e.Name.LocalName == "IdentCon").ToList();
//                            hasPowerrail = false;
//                        }
//                    }

//                    // Validate referenced UIds exist in parts
//                    bool PartExists(int uid) => parts.Any(p => (string)p.Attribute("UId") == uid.ToString());
//                    foreach (var nc in nameCons)
//                    {
//                        if (!int.TryParse((string)nc.Attribute("UId"), out int uid)) return;
//                        if (!PartExists(uid)) return;
//                    }
//                    foreach (var ic in identCons)
//                    {
//                        if (!int.TryParse((string)ic.Attribute("UId"), out int uid)) return;
//                        if (!PartExists(uid)) return;
//                    }

//                    // Avoid duplicate pin mappings
//                    foreach (var nc in nameCons)
//                    {
//                        int uid = int.Parse(nc.Attribute("UId").Value);
//                        string pin = (string)nc.Attribute("Name") ?? "";
//                        if (pinMap.Contains((uid, pin))) return;
//                    }

//                    // Need at least two connections
//                    int connectionCount = nameCons.Count + identCons.Count + (hasPowerrail ? 1 : 0);
//                    if (connectionCount < 2) return;

//                    // Update pinMap
//                    foreach (var nc in nameCons)
//                    {
//                        int uid = int.Parse(nc.Attribute("UId").Value);
//                        string pin = (string)nc.Attribute("Name") ?? "";
//                        pinMap.Add((uid, pin));
//                    }
//                    foreach (var ic in identCons)
//                    {
//                        int uid = int.Parse(ic.Attribute("UId").Value);
//                        pinMap.Add((uid, "ident"));
//                    }

//                    // prevent duplicate wires (structural equality)
//                    if (!wires.Any(w => XNode.DeepEquals(w, wire)))
//                        wires.Add(wire);
//                }

//                // Add operator parts for MOV/EQ/NE/GT/LT/GE/LE discovered inside Instruction nodes or inline text
//                var instructionNodes = rung.Descendants("Instruction").ToList();

//                // also detect inline instructions in rung.Text like MOV(a,b)
//                var rungText = rung.Element("Text")?.Value ?? "";
//                var inlineMatches = Regex.Matches(rungText, @"\b(MOV|EQU|EQ|NEQ|NE|GRT|GT|LES|LT|GEQ|GE|LEQ|LE)\s*\(\s*([^,()]+)\s*,\s*([^)()]+)\s*\)", RegexOptions.IgnoreCase);
//                foreach (Match m in inlineMatches)
//                {
//                    var instrName = m.Groups[1].Value;
//                    var p1 = m.Groups[2].Value.Trim();
//                    var p2 = m.Groups[3].Value.Trim();
//                    var synthetic = new XElement("Instruction", new XAttribute("Name", instrName),
//                        new XElement("Parameter", new XAttribute("Value", p1)),
//                        new XElement("Parameter", new XAttribute("Value", p2)));
//                    instructionNodes.Add(synthetic);
//                }

//                foreach (var instr in instructionNodes)
//                {
//                    string instrName = ((string)instr.Attribute("Name") ?? "").Trim().ToUpperInvariant();
//                    if (instrName == "EQU") instrName = "EQ";
//                    if (instrName == "NEQ") instrName = "NE";
//                    if (instrName == "GRT") instrName = "GT";
//                    if (instrName == "LES") instrName = "LT";
//                    if (instrName == "GEQ") instrName = "GE";
//                    if (instrName == "LEQ") instrName = "LE";

//                    var supported = new HashSet<string> { "MOV", "EQ", "NE", "GT", "LT", "GE", "LE" };
//                    if (!supported.Contains(instrName)) continue;

//                    var parameters = instr.Elements("Parameter").ToList();
//                    string src = "", src2 = "", dest = "";
//                    if (parameters.Count >= 2)
//                    {
//                        src = ((string)parameters[0].Attribute("Value") ?? "").Trim();
//                        src2 = ((string)parameters[1].Attribute("Value") ?? "").Trim();
//                        if (parameters.Count >= 3)
//                            dest = ((string)parameters[2].Attribute("Value") ?? "").Trim();
//                    }

//                    if (instrName == "MOV" && string.IsNullOrEmpty(dest) && !string.IsNullOrEmpty(src2))
//                    {
//                        dest = src2; src2 = "";
//                    }

//                    int srcUid = CreateAccessFor(src);
//                    int src2Uid = CreateAccessFor(src2);
//                    int destUid = CreateAccessFor(dest);

//                    string siemensOp = instrName switch
//                    {
//                        "MOV" => "Move",
//                        "EQ" => "Eq",
//                        "NE" => "Ne",
//                        "GT" => "Gt",
//                        "LT" => "Lt",
//                        "GE" => "Ge",
//                        "LE" => "Le",
//                        _ => "Move"
//                    };

//                    int opUid = UidGen.NextPartUid();
//                    var opPart = new XElement("Part",
//                        new XAttribute("Name", siemensOp),
//                        new XAttribute("UId", opUid)
//                    );
//                    // optional template value for Move
//                    if (siemensOp == "Move")
//                    {
//                        opPart.Add(new XElement("TemplateValue", new XAttribute("Name", "Card"), new XAttribute("Type", "Cardinality"), "1"));
//                    }
//                    parts.Add(opPart);

//                    // Connect operator pins with wires (Move uses en,in,out; comparators use in1,in2,out)
//                    if (siemensOp == "Move")
//                    {
//                        // Powerrail -> Move.en
//                        AddWireSafely(new XElement("Wire",
//                            new XAttribute("UId", UidGen.NextWireUid()),
//                            new XElement("Powerrail"),
//                            new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", "en"))
//                        ));

//                        if (srcUid > 0)
//                        {
//                            AddWireSafely(new XElement("Wire",
//                                new XAttribute("UId", UidGen.NextWireUid()),
//                                new XElement("IdentCon", new XAttribute("UId", srcUid)),
//                                new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", "in"))
//                            ));
//                        }

//                        if (destUid > 0)
//                        {
//                            AddWireSafely(new XElement("Wire",
//                                new XAttribute("UId", UidGen.NextWireUid()),
//                                new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", "out")),
//                                new XElement("IdentCon", new XAttribute("UId", destUid))
//                            ));
//                        }
//                    }
//                    else
//                    {
//                        // comparator
//                        if (srcUid > 0)
//                        {
//                            AddWireSafely(new XElement("Wire",
//                                new XAttribute("UId", UidGen.NextWireUid()),
//                                new XElement("IdentCon", new XAttribute("UId", srcUid)),
//                                new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", "in1"))
//                            ));
//                        }
//                        if (src2Uid > 0)
//                        {
//                            AddWireSafely(new XElement("Wire",
//                                new XAttribute("UId", UidGen.NextWireUid()),
//                                new XElement("IdentCon", new XAttribute("UId", src2Uid)),
//                                new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", "in2"))
//                            ));
//                        }
//                        if (destUid > 0)
//                        {
//                            AddWireSafely(new XElement("Wire",
//                                new XAttribute("UId", UidGen.NextWireUid()),
//                                new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", "out")),
//                                new XElement("IdentCon", new XAttribute("UId", destUid))
//                            ));
//                        }
//                    }

//                    // Link operator into chain if contacts exist: last contact.out -> operator.in(in1)
//                    if (contactUids.Any())
//                    {
//                        string operatorInPin = siemensOp == "Move" ? "in" : "in1";
//                        if (!pinMap.Contains((opUid, operatorInPin)))
//                        {
//                            AddWireSafely(new XElement("Wire",
//                                new XAttribute("UId", UidGen.NextWireUid()),
//                                new XElement("NameCon", new XAttribute("UId", contactUids.Last()), new XAttribute("Name", "out")),
//                                new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", operatorInPin))
//                            ));
//                        }
//                    }

//                    // ensure operator has at least one input; otherwise connect powerrail -> operator.in/in1
//                    if (!pinMap.Contains((opUid, siemensOp == "Move" ? "in" : "in1")))
//                    {
//                        if (!powerrailAdded)
//                        {
//                            AddWireSafely(new XElement("Wire",
//                                new XAttribute("UId", UidGen.NextWireUid()),
//                                new XElement("Powerrail"),
//                                new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", siemensOp == "Move" ? "in" : "in1"))
//                            ));
//                        }
//                        else
//                        {
//                            AddWireSafely(new XElement("Wire",
//                                new XAttribute("UId", UidGen.NextWireUid()),
//                                new XElement("IdentCon", new XAttribute("UId", firstPowerrailUid ?? UidGen.NextPartUid())),
//                                new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", siemensOp == "Move" ? "in" : "in1"))
//                            ));
//                        }
//                    }
//                } // end instructions handling

//                // --- create contacts & coil ---
//                foreach (var occ in contactOccurrences)
//                {
//                    string accessKey = occ.token + "_" + occ.type;
//                    int accessUid = CreateAccessFor(occ.token); // ensures Access entry exists
//                    // create contact part
//                    int contactUid = UidGen.NextPartUid();
//                    var contactPart = new XElement("Part",
//                        new XAttribute("Name", "Contact"),
//                        new XAttribute("UId", contactUid)
//                    );
//                    if (occ.type == "XIO")
//                        contactPart.Add(new XElement("Negated", new XAttribute("Name", "operand")));
//                    parts.Add(contactPart);
//                    contactUids.Add(contactUid);

//                    // wire Access -> Contact.operand
//                    AddWireSafely(new XElement("Wire",
//                        new XAttribute("UId", UidGen.NextWireUid()),
//                        new XElement("IdentCon", new XAttribute("UId", accessUid)),
//                        new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "operand"))
//                    ));
//                }

//                // coil part
//                int coilUid = UidGen.NextPartUid();
//                parts.Add(new XElement("Part", new XAttribute("Name", "Coil"), new XAttribute("UId", coilUid)));

//                int coilAccessUid = -1;
//                if (!string.IsNullOrWhiteSpace(coilName))
//                {
//                    coilAccessUid = CreateAccessFor(coilName);
//                }

//                // Wiring sequence for contacts -> coil
//                if (contactUids.Any())
//                {
//                    // powerrail -> first contact.in
//                    AddWireSafely(new XElement("Wire",
//                        new XAttribute("UId", UidGen.NextWireUid()),
//                        new XElement("Powerrail"),
//                        new XElement("NameCon", new XAttribute("UId", contactUids[0]), new XAttribute("Name", "in"))
//                    ));

//                    // chain contacts
//                    for (int i = 0; i < contactUids.Count - 1; i++)
//                    {
//                        AddWireSafely(new XElement("Wire",
//                            new XAttribute("UId", UidGen.NextWireUid()),
//                            new XElement("NameCon", new XAttribute("UId", contactUids[i]), new XAttribute("Name", "out")),
//                            new XElement("NameCon", new XAttribute("UId", contactUids[i + 1]), new XAttribute("Name", "in"))
//                        ));
//                    }

//                    // last contact.out -> coil.in
//                    AddWireSafely(new XElement("Wire",
//                        new XAttribute("UId", UidGen.NextWireUid()),
//                        new XElement("NameCon", new XAttribute("UId", contactUids.Last()), new XAttribute("Name", "out")),
//                        new XElement("NameCon", new XAttribute("UId", coilUid), new XAttribute("Name", "in"))
//                    ));
//                }
//                else
//                {
//                    // no contacts -> powerrail -> coil.in
//                    AddWireSafely(new XElement("Wire",
//                        new XAttribute("UId", UidGen.NextWireUid()),
//                        new XElement("Powerrail"),
//                        new XElement("NameCon", new XAttribute("UId", coilUid), new XAttribute("Name", "in"))
//                    ));
//                }

//                // coil operand mapping to Access if present
//                if (coilAccessUid > 0)
//                {
//                    AddWireSafely(new XElement("Wire",
//                        new XAttribute("UId", UidGen.NextWireUid()),
//                        new XElement("IdentCon", new XAttribute("UId", coilAccessUid)),
//                        new XElement("NameCon", new XAttribute("UId", coilUid), new XAttribute("Name", "operand"))
//                    ));
//                }

//                // Ensure all Contact/Coil parts have 'in' pin wires (avoid dangling)
//                EnsureInPins(wires, parts, pinMap, ref powerrailAdded, ref firstPowerrailUid);

//                // Build FlgNet element using the FlgNet namespace
//                var flgNet = new XElement(NsFlgNet + "FlgNet",
//                    new XElement("Parts", parts),
//                    new XElement("Wires", wires)
//                );

//                // Build compile unit
//                var compileUnit = new XElement("SW.Blocks.CompileUnit",
//                    new XAttribute("ID", UidGen.NextCompileUnitId().ToString()),
//                    new XAttribute("CompositionName", "CompileUnits"),
//                    new XElement("AttributeList",
//                        new XElement("NetworkSource", flgNet),
//                        new XElement("ProgrammingLanguage", "LAD")
//                    ),
//                    new XElement("ObjectList",
//                        // comment
//                        new XElement("MultilingualText",
//                            new XAttribute("ID", UidGen.NextTextId()),
//                            new XAttribute("CompositionName", "Comment"),
//                            new XElement("ObjectList",
//                                new XElement("MultilingualTextItem",
//                                    new XAttribute("ID", UidGen.NextTextId()),
//                                    new XAttribute("CompositionName", "Items"),
//                                    new XElement("AttributeList",
//                                        new XElement("Culture", "en-US"),
//                                        new XElement("Text", commentText ?? "")
//                                    )
//                                )
//                            )
//                        ),
//                        // title
//                        new XElement("MultilingualText",
//                            new XAttribute("ID", UidGen.NextTextId()),
//                            new XAttribute("CompositionName", "Title"),
//                            new XElement("ObjectList",
//                                new XElement("MultilingualTextItem",
//                                    new XAttribute("ID", UidGen.NextTextId()),
//                                    new XAttribute("CompositionName", "Items"),
//                                    new XElement("AttributeList",
//                                        new XElement("Culture", "en-US"),
//                                        new XElement("Text", titleText ?? "")
//                                    )
//                                )
//                            )
//                        )
//                    )
//                );

//                compileUnits.Add(compileUnit);
//            } // end foreach rung

//            return compileUnits;
//        }

//        // Create a minimal dummy compile unit (ensures block is parseable even with no real networks)
//        private static XElement CreateDummyCompileUnit()
//        {
//            int constUid = UidGen.NextPartUid();
//            int coilUid = UidGen.NextPartUid();
//            int wire1 = UidGen.NextWireUid();
//            int wire2 = UidGen.NextWireUid();

//            var flgNet = new XElement(NsFlgNet + "FlgNet",
//                new XElement("Parts",
//                    new XElement("Access", new XAttribute("Scope", "LiteralConstant"), new XAttribute("UId", constUid),
//                        new XElement("Constant",
//                            new XElement("ConstantType", "Bool"),
//                            new XElement("ConstantValue", "false")
//                        )
//                    ),
//                    new XElement("Part", new XAttribute("Name", "Coil"), new XAttribute("UId", coilUid))
//                ),
//                new XElement("Wires",
//                    new XElement("Wire",
//                        new XAttribute("UId", wire1),
//                        new XElement("Powerrail"),
//                        new XElement("NameCon", new XAttribute("UId", coilUid), new XAttribute("Name", "in"))
//                    ),
//                    new XElement("Wire",
//                        new XAttribute("UId", wire2),
//                        new XElement("IdentCon", new XAttribute("UId", constUid)),
//                        new XElement("NameCon", new XAttribute("UId", coilUid), new XAttribute("Name", "operand"))
//                    )
//                )
//            );

//            var cu = new XElement("SW.Blocks.CompileUnit",
//                new XAttribute("ID", UidGen.NextCompileUnitId().ToString()),
//                new XAttribute("CompositionName", "CompileUnits"),
//                new XElement("AttributeList",
//                    new XElement("NetworkSource", flgNet),
//                    new XElement("ProgrammingLanguage", "LAD")
//                ),
//                new XElement("ObjectList",
//                    CreateMultilingualTextBlock("", "Comment"),
//                    CreateMultilingualTextBlock("", "Title")
//                )
//            );

//            return cu;
//        }

//        // Ensure all Contact and Coil parts have an 'in' pin: add a wire if missing.
//        private static void EnsureInPins(List<XElement> wires, List<XElement> parts, HashSet<(int uid, string pin)> pinMap, ref bool powerrailAdded, ref int? firstPowerrailUid)
//        {
//            bool powerrailExists = wires.Any(w => w.Elements().Any(e => e.Name.LocalName == "Powerrail"));
//            foreach (var part in parts)
//            {
//                var uidAttr = part.Attribute("UId");
//                if (uidAttr == null) continue;
//                if (!int.TryParse(uidAttr.Value, out int uid)) continue;
//                string name = part.Attribute("Name")?.Value;
//                if (string.Equals(name, "Contact", StringComparison.OrdinalIgnoreCase) ||
//                    string.Equals(name, "Coil", StringComparison.OrdinalIgnoreCase))
//                {
//                    if (!pinMap.Contains((uid, "in")))
//                    {
//                        XElement newWire;
//                        if (!powerrailExists)
//                        {
//                            // Create a wire that includes a Powerrail child, and set up the firstPowerrailUid
//                            newWire = new XElement("Wire",
//                                new XAttribute("UId", UidGen.NextWireUid()),
//                                new XElement("Powerrail"),
//                                new XElement("NameCon", new XAttribute("UId", uid), new XAttribute("Name", "in"))
//                            );

//                            // register first powerrail Access if not already
//                            if (!powerrailAdded)
//                            {
//                                powerrailAdded = true;
//                                firstPowerrailUid = UidGen.NextPartUid();
//                                parts.Add(new XElement("Access", new XAttribute("Scope", "Powerrail"), new XAttribute("UId", firstPowerrailUid.Value)));
//                            }

//                            powerrailExists = true;
//                        }
//                        else
//                        {
//                            // reference existing rail by IdentCon to avoid duplicate Powerrail nodes
//                            newWire = new XElement("Wire",
//                                new XAttribute("UId", UidGen.NextWireUid()),
//                                new XElement("IdentCon", new XAttribute("UId", firstPowerrailUid ?? UidGen.NextPartUid())),
//                                new XElement("NameCon", new XAttribute("UId", uid), new XAttribute("Name", "in"))
//                            );
//                        }

//                        if (!wires.Any(w => XNode.DeepEquals(w, newWire)))
//                            wires.Add(newWire);

//                        pinMap.Add((uid, "in"));
//                    }
//                }
//            }
//        }

//        // Helper to create multilingual text element
//        private static XElement CreateMultilingualTextBlock(string text, string compositionName)
//        {
//            return new XElement("MultilingualText",
//                new XAttribute("ID", UidGen.NextTextId()),
//                new XAttribute("CompositionName", compositionName),
//                new XElement("ObjectList",
//                    new XElement("MultilingualTextItem",
//                        new XAttribute("ID", UidGen.NextTextId()),
//                        new XAttribute("CompositionName", "Items"),
//                        new XElement("AttributeList",
//                            new XElement("Culture", "en-US"),
//                            new XElement("Text", text ?? "")
//                        )
//                    )
//                )
//            );
//        }

//        // Parse simplified RLL-style text into contacts (XIC/XIO) and coil names.
//        private static void ParseRllTextAdvanced(string text, List<(string token, string type)> outTokens, ref string coilName)
//        {
//            if (string.IsNullOrWhiteSpace(text)) return;
//            var t = text.Trim();
//            if (t.EndsWith(";")) t = t[..^1];

//            var mCoil = Regex.Match(t, @"(?<op>OTL|OTU|OTE|OT|OTL|OTU)\s*\(\s*(?<n>[^)]+)\s*\)", RegexOptions.IgnoreCase);
//            if (mCoil.Success)
//                coilName = mCoil.Groups["n"].Value.Trim();

//            string inner = null;
//            var mInner = Regex.Match(t, @"\[(?<inner>.*)\]");
//            if (mInner.Success) inner = mInner.Groups["inner"].Value;
//            else inner = t;

//            var tokens = SplitTopLevelArgs(string.IsNullOrWhiteSpace(inner) ? t : inner);
//            foreach (var tok in tokens)
//            {
//                var trimmed = tok.Trim();
//                if (string.IsNullOrWhiteSpace(trimmed)) continue;

//                var mXic = Regex.Match(trimmed, @"XIC\s*\(\s*(?<n>[^)]+)\s*\)", RegexOptions.IgnoreCase);
//                var mXio = Regex.Match(trimmed, @"XIO\s*\(\s*(?<n>[^)]+)\s*\)", RegexOptions.IgnoreCase);

//                if (mXic.Success) outTokens.Add((mXic.Groups["n"].Value.Trim(), "XIC"));
//                else if (mXio.Success) outTokens.Add((mXio.Groups["n"].Value.Trim(), "XIO"));
//                else outTokens.Add((trimmed, "XIC"));
//            }
//        }

//        // Split top-level args by comma but respect brackets/parentheses
//        private static List<string> SplitTopLevelArgs(string text)
//        {
//            var result = new List<string>();
//            if (string.IsNullOrEmpty(text)) return result;
//            int level = 0;
//            int start = 0;
//            for (int i = 0; i < text.Length; i++)
//            {
//                char c = text[i];
//                if (c == '[' || c == '(') level++;
//                else if (c == ']' || c == ')') level--;
//                else if (c == ',' && level == 0)
//                {
//                    result.Add(text[start..i]);
//                    start = i + 1;
//                }
//            }
//            if (start < text.Length) result.Add(text[start..]);
//            return result;
//        }

//        private static string GetInstructionName(XElement rung) => rung.Element("Instruction")?.Attribute("Name")?.Value;
//    }


//public static class RockwellToSiemensConverter
//{
//    private static readonly XNamespace NsInterface =
//        "http://www.siemens.com/automation/Openness/SW/Interface/v5";

//    private static readonly XNamespace NsFlgNet =
//        "http://www.siemens.com/automation/Openness/SW/NetworkSource/FlgNet/v3";

//    private static int _textIdCounter;
//    private static int _compileUnitIdCounter;
//    private static int _partUidCounter;
//    private static int _wireUidCounter;

//    private static string NextTextId() => $"T{_textIdCounter++:X2}"; // T01, T02, ...
//    private static int NextCompileUnitIdInt() => _compileUnitIdCounter++;
//    private static int NextPartUidInt() => _partUidCounter++;
//    private static int NextWireUidInt() => _wireUidCounter++;

//    private static readonly HashSet<string> SiemensReservedNames = new(StringComparer.OrdinalIgnoreCase)
//    {
//        "TON","TOF","TP","AND","OR","NOT","MOVE","ADD","SUB","MUL","DIV",
//        "GEQ","LEQ","EQ","NEQ","GT","LT","SET","RESET","SR","RS"
//    };

//    private static string SanitizeName(string original, HashSet<string> usedNames)
//    {
//        if (string.IsNullOrWhiteSpace(original))
//            return "Var_" + Guid.NewGuid().ToString("N")[..6];

//        string safe = original.Trim();
//        if (SiemensReservedNames.Contains(safe) ||
//            SiemensReservedNames.Any(r => safe.StartsWith(r, StringComparison.OrdinalIgnoreCase)))
//            safe = "_" + safe + "_";

//        int suffix = 1;
//        string baseName = safe;
//        while (usedNames.Contains(safe))
//            safe = $"{baseName}_{suffix++}";
//        usedNames.Add(safe);
//        return safe;
//    }

public static class RockwellToSiemensConverter
{
    private static readonly XNamespace NsInterface =
        "http://www.siemens.com/automation/Openness/SW/Interface/v5";

    private static readonly XNamespace NsFlgNet =
        "http://www.siemens.com/automation/Openness/SW/NetworkSource/FlgNet/v3";

    private static int _textIdCounter;
    private static int _compileUnitIdCounter;
    private static int _globalUidCounter;


    private static string NextTextId() => $"T{_textIdCounter++:X2}";
    private static int NextCompileUnitIdInt() => _compileUnitIdCounter++;
    private static int NextGlobalUid() => ++_globalUidCounter;


    private static readonly HashSet<string> SiemensReservedNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "TON","TOF","TP","AND","OR","NOT","MOVE","ADD","SUB","MUL","DIV",
            "GEQ","LEQ","EQ","NEQ","GT","LT","SET","RESET","SR","RS"
        };

    private static string SanitizeName(string original, HashSet<string> usedNames)
    {
        if (string.IsNullOrWhiteSpace(original))
            return "Var_" + Guid.NewGuid().ToString("N")[..6];

        string safe = original.Trim();
        if (SiemensReservedNames.Contains(safe) ||
            SiemensReservedNames.Any(r => safe.StartsWith(r, StringComparison.OrdinalIgnoreCase)))
            safe = "_" + safe + "_";

        int suffix = 1;
        string baseName = safe;
        while (usedNames.Contains(safe))
            safe = $"{baseName}_{suffix++}";
        usedNames.Add(safe);
        return safe;
    }

    public static void Convert(string rockwellFile, string siemensOutputPath)
    {
        _textIdCounter = 1;
        _compileUnitIdCounter = 1000;
        _globalUidCounter = 1;

        string fileName = Path.GetFileNameWithoutExtension(rockwellFile);

        var rockwellDoc = XDocument.Load(rockwellFile);

        createList(rockwellDoc, fileName);
        
        var siemensDoc = new XDocument(new XDeclaration("1.0", "utf-8", null),
            new XElement("Document",
                new XElement("Engineering", new XAttribute("version", "V17")),
                new XElement("DocumentInfo",
                    new XElement("Created", DateTime.UtcNow.ToString("o")),
                    new XElement("ExportSetting", "None")
                ),
                BuildFbElement(rockwellDoc, Path.GetFileNameWithoutExtension(rockwellFile))
            )
        );

        siemensDoc.Save(siemensOutputPath);
    }
    private static XElement BuildFbElement(XDocument rockwellDoc, string blockName)
    {
        var fb = new XElement(
            XName.Get("SW.Blocks.FB", "http://www.siemens.com/automation/Openness/SW/Blocks/FB/v5"),
                        new XAttribute("ID", "0"),
            new XElement("AttributeList",
                new XElement("Interface",
                    new XElement(NsInterface + "Sections",
                        CreateSection("Input", rockwellDoc, "Parameter"),
                        CreateSection("Output", rockwellDoc, "Parameter"),
                        CreateSection("InOut", rockwellDoc, "Parameter"),
                        CreateSection("Static", rockwellDoc, "LocalTag"),
                        CreateSection("Temp", rockwellDoc, "LocalTag", onlyTemp: true),
                        CreateSection("Constant", rockwellDoc, "Constant")
                    )
                ),
                new XElement("Name", blockName + "__Converted"),
                new XElement("Number", "1"),
                new XElement("ProgrammingLanguage", "LAD")
            ),
            new XElement("ObjectList",
                new XElement("MultilingualText",
                    new XAttribute("ID", NextTextId()),
                    new XAttribute("CompositionName", "Comment"),
                    new XElement("ObjectList",
                        new XElement("MultilingualTextItem",
                            new XAttribute("ID", NextTextId()),
                            new XAttribute("CompositionName", "Items"),
                            new XElement("AttributeList",
                                new XElement("Culture", "en-US"),
                                new XElement("Text", "")
                            )
                        )
                    )
                ),
                GenerateCompileUnits(rockwellDoc)
            )
        );

        return fb;
    }

    public static void createList(XDocument rockwellDoc, string rocfileName)
    {
        var localTags = rockwellDoc.Descendants("Parameter");
        var rung = rockwellDoc.Descendants("Rung");
        var sublocalTags = rockwellDoc.Descendants("Parameter");
        List<string> dataTypeList = new List<string>();

        var members = new List<(string Name, string DataType)>();

        var memberNodes = rockwellDoc.Descendants("Member");

        foreach (var m in memberNodes)
        {
            string name = m.Attribute("Name")?.Value ?? "";
            string datatype = m.Attribute("DataType")?.Value ?? "";

            if (!string.IsNullOrWhiteSpace(name) &&
                !string.IsNullOrWhiteSpace(datatype))
            {
                members.Add((name, datatype));
            }
        }


        foreach (var p in localTags)
        {
            string dataType = p.Attribute("DataType")?.Value;

            if (!string.IsNullOrEmpty(dataType) && dataType.Equals("BOOL", StringComparison.OrdinalIgnoreCase))
            {
                string name = p.Attribute("Name")?.Value;
                string tagType = p.Attribute("TagType")?.Value;
                string usage = p.Attribute("Usage")?.Value;
                string radix = p.Attribute("Radix")?.Value;
                string required = p.Attribute("Required")?.Value;
                string visible = p.Attribute("Visible")?.Value;
                string externalAccess = p.Attribute("ExternalAccess")?.Value;

                // Description (CDATA text)
                string description = p.Element("Description")?.Value?.Trim();

                string row =
                    $"{name}, {tagType}, {dataType}, {usage}, {radix}, {required}, {visible}, {externalAccess}, {description}";

                dataTypeList.Add(row);
            }
            else
            {
                string name = p.Attribute("Name")?.Value;
                string tagType = p.Attribute("TagType")?.Value;
                string usage = p.Attribute("Usage")?.Value;
                string radix = p.Attribute("Radix")?.Value;
                string required = p.Attribute("Required")?.Value;
                string visible = p.Attribute("Visible")?.Value;
                string externalAccess = p.Attribute("ExternalAccess")?.Value;

                string description = p.Element("Description")?.Value?.Trim();

                
                string row =
                    $"{name}, {tagType}, {dataType}, {usage}, {radix}, {required}, {visible}, {externalAccess}, {description}"; //, {name + "." + tagType + "-" + dataType + "/Logic - *"}

                dataTypeList.Add(row);

                var datatypeNode = rockwellDoc
    .Descendants("DataType")
    .FirstOrDefault(x => (string)x.Attribute("Name") == dataType);

                if (datatypeNode == null)
                {
                    continue;
                }
                var menber = datatypeNode.Descendants("Member");

                foreach (var m1 in menber)
                {
                    string type = m1.Attribute("DataType")?.Value;
                    if (type == "BOOL" || type == "BIT" || type == "SINT")
                    {
                        string tname = m1.Attribute("Name")?.Value;
                        string ttagType = m1.Attribute("TagType")?.Value;
                        string tusage = m1.Attribute("Usage")?.Value;
                        string tradix = m1.Attribute("Radix")?.Value;
                        string trequired = m1.Attribute("Required")?.Value;
                        string tvisible = m1.Attribute("Visible")?.Value;
                        string texternalAccess = m1.Attribute("ExternalAccess")?.Value;

                        string tdescription = m1.Element("Description")?.Value?.Trim();

                        var resultList = new List<(int RungNumber, Dictionary<string, int> InstructionCounts)>();
                        
                        foreach (var run in rung)
                        {
                            int rungNumber = 0;
                            var numAttr = run.Attribute("Number");
                            if (numAttr != null)
                                int.TryParse(numAttr.Value, out rungNumber);

                            var textElement = run.Element("Text");
                            if (textElement == null) continue;

                            string text = textElement.Value;

                            var matches = Regex.Matches(text, @"\b(XIC|XIO|OTE|OTL|OTU|ONS)\(([^)]+)\)");

                            Dictionary<string, int> instructionCount = new Dictionary<string, int>();

                            foreach (Match m in matches)
                            {
                                string instruction = m.Groups[1].Value;
                                string tag = m.Groups[2].Value;
                                if (tag == name + "." + tname)
                                {
                                    if (!instructionCount.ContainsKey(instruction))
                                        instructionCount[instruction] = 0;

                                    instructionCount[instruction]++;
                                    resultList.Add((rungNumber, instructionCount));
                                }
                            }
                        }
                        string logic = "";
                        foreach (var d1 in resultList)
                        {
                            string keys = string.Join(",", d1.InstructionCounts.Keys);
                            logic += $"{d1.RungNumber}({keys})";
                        }

                        if (!string.IsNullOrEmpty(logic))
                        {
                            string trow =
                            $"{name}, {tname}, {ttagType}, {type}, {tusage}, {tradix}, {trequired}, {tvisible}, {texternalAccess}, {tdescription}, {name + "." + tname + " - " + dataType + " /Logic - *" + logic}";

                            trow = trow.Replace("\n", " ");

                            dataTypeList.Add(trow);
                        }
                        else
                        {
                            string trow =
                            $"{name}, {tname}, {ttagType}, {type}, {tusage}, {tradix}, {trequired}, {tvisible}, {texternalAccess}, {tdescription}";

                            trow = trow.Replace("\n", " ");

                            dataTypeList.Add(trow);
                        }
                    }
                    else
                    {
                        string tname = m1.Attribute("Name")?.Value;
                        string ttagType = m1.Attribute("TagType")?.Value;
                        string tusage = m1.Attribute("Usage")?.Value;
                        string tradix = m1.Attribute("Radix")?.Value;
                        string trequired = m1.Attribute("Required")?.Value;
                        string tvisible = m1.Attribute("Visible")?.Value;
                        string texternalAccess = m1.Attribute("ExternalAccess")?.Value;
                        
                        string tdescription = p.Element("Description")?.Value?.Trim();

                        string tttrow =
                            $"{name}, {tname}, {ttagType}, {type}, {tusage}, {tradix}, {trequired}, {tvisible}, {texternalAccess}, {tdescription}";

                        dataTypeList.Add(tttrow);

                        var datatypeNode1 = rockwellDoc
    .Descendants("DataType")
    .FirstOrDefault(x => (string)x.Attribute("Name") == type);

                        if (datatypeNode1 == null)
                            continue;
                        var menber1 = datatypeNode1.Descendants("Member");

                        foreach (var m2 in menber1)
                        {
                            string ttype = m2.Attribute("DataType")?.Value;
                            if (ttype == "BOOL" || ttype == "BIT" || ttype == "SINT" || ttype == "INT" || ttype == "DINT")
                            {
                                string ttname = m2.Attribute("Name")?.Value;
                                string tttagType = m2.Attribute("TagType")?.Value;
                                string ttusage = m2.Attribute("Usage")?.Value;
                                string ttradix = m2.Attribute("Radix")?.Value;
                                string ttrequired = m2.Attribute("Required")?.Value;
                                string ttvisible = m2.Attribute("Visible")?.Value;
                                string ttexternalAccess = m2.Attribute("ExternalAccess")?.Value;

                                string ttdescription = m2.Element("Description")?.Value?.Trim();

                                var resultList = new List<(int RungNumber, Dictionary<string, int> InstructionCounts)>();

                                foreach (var run in rung)
                                {
                                    int rungNumber = 0;
                                    var numAttr = run.Attribute("Number");
                                    if (numAttr != null)
                                        int.TryParse(numAttr.Value, out rungNumber);

                                    var textElement = run.Element("Text");
                                    if (textElement == null) continue;

                                    string text = textElement.Value;

                                    var matches = Regex.Matches(text, @"\b(XIC|XIO|OTE|OTL|OTU|ONS)\(([^)]+)\)");

                                    Dictionary<string, int> instructionCount = new Dictionary<string, int>();

                                    foreach (Match m in matches)
                                    {
                                        string instruction = m.Groups[1].Value;
                                        string tag = m.Groups[2].Value;
                                        if (tag == name + "." + tname + "." + ttname)
                                        {
                                            if (!instructionCount.ContainsKey(instruction))
                                                instructionCount[instruction] = 0;

                                            instructionCount[instruction]++;
                                            resultList.Add((rungNumber, instructionCount));
                                        }
                                    }
                                }
                                string logic = "";
                                foreach (var d1 in resultList)
                                {
                                    string keys = string.Join(",", d1.InstructionCounts.Keys);
                                    logic += $"{d1.RungNumber}({keys})";
                                }
                                //string trow =
                                //    $"{name}, {tname}, {ttagType}, {type}, {tusage}, {tradix}, {trequired}, {tvisible}, {texternalAccess}, {tdescription}, {name + "." + tname + "." + ttname + " - " + dataType + " /Logic - *" + logic}";


                                if (!string.IsNullOrEmpty(logic))
                                {
                                    string ttrow =
                                    $"{name},{tname}, {ttname}, {tttagType}, {type}, {ttusage}, {ttradix}, {ttrequired}, {ttvisible}, {ttexternalAccess}, {ttdescription}, {name + "." + tname + "." + ttname + " - " + dataType + " /Logic - *" + logic}";
                                    ttrow = ttrow.Replace("\n", " ");

                                    dataTypeList.Add(ttrow);
                                }
                                else
                                {
                                    string ttrow =
                                    $"{name},{tname}, {ttname}, {tttagType}, {type}, {ttusage}, {ttradix}, {ttrequired}, {ttvisible}, {ttexternalAccess}, {ttdescription}";
                                    ttrow = ttrow.Replace("\n", " ");

                                    dataTypeList.Add(ttrow);
                                }                                
                            }
                            else
                            {

                            }
                        }
                    }
                }
            }
        }
        dataTypeList = dataTypeList
    .Select(x => x.Replace("\n", " ").Replace("\r", " "))
    .ToList();
        string currentDir = Directory.GetCurrentDirectory();


        string fileName = $"{rocfileName}_datatype_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

        string fullPath = Path.Combine(currentDir, fileName);

        System.IO.File.WriteAllLines(fullPath, dataTypeList);
    }
    private static XElement CreateSection(string sectionName, XDocument rockwellDoc, string tagType, IEnumerable<string>? extraTags = null, bool onlyTemp = false)
    {        
        IEnumerable<XElement> members = Enumerable.Empty<XElement>();

        if (rockwellDoc != null)
        {
            if (onlyTemp && tagType == "LocalTag")
            {
                members = rockwellDoc.Descendants("LocalTag")
                    .Where(x => string.Equals((string?)x.Attribute("Usage"), "Temp", StringComparison.OrdinalIgnoreCase));
            }
            else if (tagType == "LocalTag")
            {
                members = rockwellDoc.Descendants("LocalTag");
            }
            else if (tagType == "Constant")
            {
                members = rockwellDoc.Descendants("Constant");
            }
            else
            {
                members = rockwellDoc.Descendants(tagType)
                    .Where(p => string.Equals((string?)p.Attribute("Usage"), sectionName, StringComparison.OrdinalIgnoreCase));
            }
        }

        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (extraTags != null)
            foreach (var t in extraTags) usedNames.Add(t);

        return new XElement(NsInterface + "Section",
            new XAttribute("Name", sectionName),
            members.Select(p =>
            {
                string rawName = (string?)p.Attribute("Name") ?? "";
                string safeName = SanitizeName(rawName, usedNames);
                string mapped = MapDataType((string?)p.Attribute("DataType") ?? (string?)p.Attribute("DataTypeName") ?? "BOOL");

                return new XElement(NsInterface + "Member",
                    new XAttribute("Name", safeName),
                    new XAttribute("Datatype", mapped)
                );
            })
        );
    }

    private static string MapDataType(string rockwellType)
    {
        if (string.IsNullOrWhiteSpace(rockwellType)) return "Bool";
        return rockwellType.ToUpperInvariant() switch
        {
            "BOOL" => "Bool",
            "INT" => "Int",
            "DINT" => "DInt",
            "REAL" => "Real",
            "STRING" => "String",
            "WORD" => "Word",
            "DWORD" => "DWord",
            _ => "Bool"
        };
    }

    private static IEnumerable<XElement> GenerateCompileUnits(XDocument rockwellDoc)
    {
        var compileUnits = new List<XElement>();

        var rungs = rockwellDoc.Descendants("Rung").ToList();
        if (!rungs.Any())
        {
            rungs = rockwellDoc.Descendants("Instruction")
                .Select(i => new XElement("Rung", i))
                .ToList();
        }

        if (!rungs.Any())
        {
            int constUid = NextGlobalUid();

            int coilUid = NextGlobalUid();

            int w1 = NextGlobalUid();

            int w2 = NextGlobalUid();

            var dummyFlg = new XElement(NsFlgNet + "FlgNet",
                new XElement("Parts",
                    new XElement("Access",
                        new XAttribute("Scope", "LiteralConstant"),
                        new XAttribute("UId", constUid),
                        new XElement("Constant",
                            new XElement("ConstantType", "Bool"),
                            new XElement("ConstantValue", "false")
                        )
                    ),
                    new XElement("Part", new XAttribute("Name", "Coil"), new XAttribute("UId", coilUid))
                ),
                new XElement("Wires",
                    new XElement("Wire",
                        new XAttribute("UId", w1),
                        new XElement("Powerrail"),
                        new XElement("NameCon", new XAttribute("UId", coilUid), new XAttribute("Name", "in"))
                    ),
                    new XElement("Wire",
                        new XAttribute("UId", w2),
                        new XElement("IdentCon", new XAttribute("UId", constUid)),
                        new XElement("NameCon", new XAttribute("UId", coilUid), new XAttribute("Name", "operand"))
                    )
                )
            );

            compileUnits.Add(MakeCompileUnit(dummyFlg, comment: string.Empty, title: string.Empty));
            return compileUnits;
        }

        foreach (var rung in rungs)
        {
            var commentText = (string?)rung.Element("Comment") ??
                              rung.Descendants("Description").FirstOrDefault()?.Value ?? "";
            var titleText = rung.Element("Text")?.Value?.Trim() ?? GetInstructionName(rung) ?? "";


            var test = rung.Element("Text").Value;

            var cdataNode = rung.Element("Text").Nodes().OfType<XCData>().FirstOrDefault();
            string rawCData = cdataNode != null
                ? "<![CDATA[" + cdataNode.Value + "]]>"
                : "";
            int count = CountOpenBrackets(rawCData);

            if (count > 2)
            {
                //orRung(rawCData);
            }
            var contactOccurrences = new List<(string token, string type)>();
            string? coilName = null;
            if (!string.IsNullOrWhiteSpace(rung.Element("Text")?.Value))
                ParseRllTextAdvanced(rung.Element("Text")!.Value, contactOccurrences, ref coilName);

            contactOccurrences = contactOccurrences
                .Where(c => !c.token.Contains("(") && !c.token.Contains(")"))
                .ToList();

            if (!contactOccurrences.Any())
            {
                var inputs = rung.Descendants("Input")
                    .Select(x => (((string?)x.Attribute("TagName") ?? (string?)x.Attribute("Name"))!, "XIC"))
                    .Where(t => !string.IsNullOrWhiteSpace(t.Item1))
                    .ToList();
                contactOccurrences.AddRange(inputs);
            }

            if (string.IsNullOrWhiteSpace(coilName))
            {
                coilName = rung.Descendants("Output")
                    .Select(x => (string?)x.Attribute("TagName") ?? (string?)x.Attribute("Name"))
                    .FirstOrDefault();
            }

            if (!contactOccurrences.Any() && string.IsNullOrWhiteSpace(coilName) &&
                !rung.Descendants("Instruction").Any() &&
                string.IsNullOrWhiteSpace(rung.Element("Text")?.Value))
                continue;

            var parts = new List<XElement>();
            var wires = new List<XElement>();
            var accessMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var contactUids = new List<int>();
            var pinMap = new HashSet<(int uid, string pinName)>();
            bool powerRailAdded = false;

            void AddWireSafelyLocal(XElement wire)
            {
                bool hasPowerrail = wire.Elements().Any(e => e.Name.LocalName == "Powerrail");
                var nameCons = wire.Elements().Where(e => e.Name.LocalName == "NameCon").ToList();
                var identCons = wire.Elements().Where(e => e.Name.LocalName == "IdentCon").ToList();

                if (hasPowerrail)
                {
                    if (!powerRailAdded)
                    {
                        powerRailAdded = true;
                    }
                    else
                    {
                        wire = new XElement("Wire",
                            nameCons,
                            identCons
                        );
                        hasPowerrail = false;
                    }
                }

                bool UidExistsLocal(int uid) => parts.Any(p => (string?)p.Attribute("UId") == uid.ToString());
                foreach (var nc in nameCons)
                {
                    if (!int.TryParse((string?)nc.Attribute("UId"), out int uid)) return;
                    if (!UidExistsLocal(uid)) return;
                }
                foreach (var ic in identCons)
                {
                    if (!int.TryParse((string?)ic.Attribute("UId"), out int uid)) return;
                    if (!UidExistsLocal(uid)) return;
                }

                foreach (var nc in nameCons)
                {
                    int uid = int.Parse(nc.Attribute("UId")!.Value);
                    string pin = (string?)nc.Attribute("Name") ?? "";
                    if (pinMap.Contains((uid, pin)))
                        return;
                }

                int connectionCount = nameCons.Count + identCons.Count + (hasPowerrail ? 1 : 0);
                if (connectionCount < 2) return;
                
                foreach (var nc in nameCons)
                {
                    int uid = int.Parse(nc.Attribute("UId")!.Value);
                    string pin = (string?)nc.Attribute("Name") ?? "";
                    pinMap.Add((uid, pin));
                }
                foreach (var ic in identCons)
                {
                    int uid = int.Parse(ic.Attribute("UId")!.Value);
                    pinMap.Add((uid, "ident"));
                }

                if (!wires.Any(w => XNode.DeepEquals(w, wire)))
                    wires.Add(wire);
            }

            int CreateAccessFor(string token)
            {
                token = token?.Trim() ?? "";
                if (string.IsNullOrEmpty(token)) return -1;
                if (token.StartsWith("MOV(", StringComparison.OrdinalIgnoreCase)) return -1;

                if (accessMap.TryGetValue(token, out int existingUid))
                    return existingUid;

                if (double.TryParse(token, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double literalVal))
                {
                    int litUid = NextGlobalUid();
                    var constElem = new XElement("Access",
                            new XAttribute("Scope", "LiteralConstant"),
                            new XAttribute("UId", litUid),
                            new XElement("Constant",
                                new XElement("ConstantType", literalVal % 1 == 0 ? "Int" : "Real"),
                                new XElement("ConstantValue",
                                    literalVal.ToString(System.Globalization.CultureInfo.InvariantCulture))));
                    parts.Add(constElem);
                    accessMap[token] = litUid;
                    return litUid;
                }
                else
                {
                    int accUid = NextGlobalUid();
                    parts.Add(new XElement("Access",
                        new XAttribute("Scope", "LocalVariable"),
                        new XAttribute("UId", accUid),
                        new XElement("Symbol",
                            new XElement("Component", new XAttribute("Name", token)))));
                    accessMap[token] = accUid;
                    return accUid;
                }
            }

            void EnsureOperatorHasInputIfRequired(int operatorUid, string operatorType)
            {
                string requiredIn = operatorType == "Move" ? "in" : "in1";
                if (pinMap.Contains((operatorUid, requiredIn))) return;

                if (contactUids.Any())
                {
                    int lastContact = contactUids.Last();
                    AddWireSafelyLocal(new XElement("Wire",
                        new XAttribute("UId", NextGlobalUid()),
                        new XElement("NameCon", new XAttribute("UId", lastContact), new XAttribute("Name", "out")),
                        new XElement("NameCon", new XAttribute("UId", operatorUid), new XAttribute("Name", requiredIn))));
                }
                else
                {
                    AddWireSafelyLocal(new XElement("Wire",
                        new XAttribute("UId", NextGlobalUid()),
                        new XElement("Powerrail"),
                        new XElement("NameCon", new XAttribute("UId", operatorUid), new XAttribute("Name", requiredIn))));
                }
            }

            var instructionNodes = rung.Descendants("Instruction").ToList();
            var rungText = rung.Element("Text")?.Value ?? "";
            var inlineMatches = Regex.Matches(
                rungText,
                @"\b(MOV|EQU|EQ|NEQ|NE|GRT|GT|LES|LT|GEQ|GE|LEQ|LE)\s*\(\s*([^,()]+)\s*,\s*([^)()]+)\s*\)",
                RegexOptions.IgnoreCase);
            foreach (Match m in inlineMatches)
            {
                var instrName = m.Groups[1].Value;
                var p1 = m.Groups[2].Value.Trim();
                var p2 = m.Groups[3].Value.Trim();
                var synthetic = new XElement("Instruction", new XAttribute("Name", instrName),
                    new XElement("Parameter", new XAttribute("Value", p1)),
                    new XElement("Parameter", new XAttribute("Value", p2)));
                instructionNodes.Add(synthetic);
            }

            foreach (var instr in instructionNodes)
            {
                string instrName = ((string?)instr.Attribute("Name") ?? "").Trim().ToUpperInvariant();
                if (instrName == "EQU") instrName = "EQ";
                if (instrName == "NEQ") instrName = "NE";
                if (instrName == "GRT") instrName = "GT";
                if (instrName == "LES") instrName = "LT";
                if (instrName == "GEQ") instrName = "GE";
                if (instrName == "LEQ") instrName = "LE";

                var supported = new HashSet<string> { "MOV", "EQ", "NE", "GT", "LT", "GE", "LE" };
                if (!supported.Contains(instrName)) continue;

                var parameters = instr.Elements("Parameter").ToList();
                string src = "", src2 = "", dest = "";
                if (parameters.Count >= 2)
                {
                    src = ((string?)parameters[0].Attribute("Value") ?? "").Trim();
                    src2 = ((string?)parameters[1].Attribute("Value") ?? "").Trim();
                    if (parameters.Count >= 3)
                        dest = ((string?)parameters[2].Attribute("Value") ?? "").Trim();
                }

                if (instrName == "MOV" && string.IsNullOrEmpty(dest) && !string.IsNullOrEmpty(src2))
                { dest = src2; src2 = ""; }

                int srcUid = CreateAccessFor(src);
                int src2Uid = CreateAccessFor(src2);
                int destUid = CreateAccessFor(dest);

                string siemensOp = instrName switch
                {
                    "MOV" => "Move",
                    "EQ" => "Eq",
                    "NE" => "Ne",
                    "GT" => "Gt",
                    "LT" => "Lt",
                    "GE" => "Ge",
                    "LE" => "Le",
                    _ => "Move"
                };

                int opUid = NextGlobalUid();
                var opPart = new XElement("Part",
                    new XAttribute("Name", siemensOp),
                    new XAttribute("UId", opUid));

                if (siemensOp == "Move")
                {
                    opPart.Add(new XElement("TemplateValue",
                        new XAttribute("Name", "Card"),
                        new XAttribute("Type", "Cardinality"), "1"));
                }

                parts.Add(opPart);

                if (siemensOp == "Move")
                {
                    AddWireSafelyLocal(new XElement("Wire",
                        new XAttribute("UId", NextGlobalUid()),
                        new XElement("Powerrail"),
                        new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", "en"))));

                    if (srcUid > 0)
                    {
                        AddWireSafelyLocal(new XElement("Wire",
                            new XAttribute("UId", NextGlobalUid()),
                            new XElement("IdentCon", new XAttribute("UId", srcUid)),
                            new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", "in"))));
                    }
                    if (destUid > 0)
                    {
                        AddWireSafelyLocal(new XElement("Wire",
                            new XAttribute("UId", NextGlobalUid()),
                            new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", "out")),
                            new XElement("IdentCon", new XAttribute("UId", destUid))));
                    }
                }
                else
                {
                    if (srcUid > 0)
                    {
                        AddWireSafelyLocal(new XElement("Wire",
                            new XAttribute("UId", NextGlobalUid()),
                            new XElement("IdentCon", new XAttribute("UId", srcUid)),
                            new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", "in1"))));
                    }
                    if (src2Uid > 0)
                    {
                        AddWireSafelyLocal(new XElement("Wire",
                            new XAttribute("UId", NextGlobalUid()),
                            new XElement("IdentCon", new XAttribute("UId", src2Uid)),
                            new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", "in2"))));
                    }
                    if (destUid > 0)
                    {
                        AddWireSafelyLocal(new XElement("Wire",
                            new XAttribute("UId", NextGlobalUid()),
                            new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", "out")),
                            new XElement("IdentCon", new XAttribute("UId", destUid))));
                    }
                }

                if (contactUids.Any() && !pinMap.Contains((opUid, siemensOp == "Move" ? "in" : "in1")))
                {
                    AddWireSafelyLocal(new XElement("Wire",
                        new XAttribute("UId", NextGlobalUid()),
                        new XElement("NameCon", new XAttribute("UId", contactUids.Last()), new XAttribute("Name", "out")),
                        new XElement("NameCon", new XAttribute("UId", opUid), new XAttribute("Name", siemensOp == "Move" ? "in" : "in1"))));
                }

                EnsureOperatorHasInputIfRequired(opUid, siemensOp);
            }

            foreach (var occ in contactOccurrences)
            {
                int accessUid = NextGlobalUid();
                parts.Add(new XElement("Access",
                    new XAttribute("Scope", "LocalVariable"),
                    new XAttribute("UId", accessUid),
                    new XElement("Symbol",
                        new XElement("Component", new XAttribute("Name", occ.token)))));

                int contactUid = NextGlobalUid();
                var contactPart = new XElement("Part",
                    new XAttribute("Name", "Contact"),
                    new XAttribute("UId", contactUid));
                if (occ.type.Equals("XIO", StringComparison.OrdinalIgnoreCase))
                    contactPart.Add(new XElement("Negated", new XAttribute("Name", "operand")));
                parts.Add(contactPart);
                contactUids.Add(contactUid);

                AddWireSafelyLocal(new XElement("Wire",
                    new XAttribute("UId", NextGlobalUid()),
                    new XElement("IdentCon", new XAttribute("UId", accessUid)),
                    new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "operand"))));
            }

            int coilPartUid = NextGlobalUid();
            parts.Add(new XElement("Part", new XAttribute("Name", "Coil"), new XAttribute("UId", coilPartUid)));

            int coilAccessUid = -1;
            if (count > 2)
            {
                var parts1 = new List<XElement>();
                var wires1 = new List<XElement>();

                string rll = cdataNode?.Value ?? rung.Element("Text")?.Value ?? "";

                string text = rll.Trim();
                const string cdataStart = "<![CDATA[";
                const string cdataEnd = "]]>";

                if (text.StartsWith(cdataStart, StringComparison.OrdinalIgnoreCase) && text.EndsWith(cdataEnd))
                    text = text.Substring(cdataStart.Length, text.Length - cdataStart.Length - cdataEnd.Length).Trim();

                int openIdx = text.IndexOf('[');
                int closeIdx = text.LastIndexOf(']');
                string insideSection = text.Substring(openIdx + 1, closeIdx - openIdx - 1).Trim();
                var groups = insideSection
    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
    .Select(g => g.Trim())
    .Where(g => !string.IsNullOrWhiteSpace(g))
    .ToList();

                bool allAreCoils = groups.All(g => g.Contains("OTE", StringComparison.OrdinalIgnoreCase));
                //if (count > 5)
                //    orRung(rll, parts1, wires1, NextGlobalUid, true);
                if (!allAreCoils)
                    orRung(rll, parts1, wires1, NextGlobalUid);
                else if(allAreCoils)
                    orRung(rll, parts1, wires1, NextGlobalUid, "allCoilInside");
                
                var flgNetOr = new XElement(NsFlgNet + "FlgNet",
                    new XElement("Parts", DistinctByUid(parts1)),
                    new XElement("Wires", wires1.Distinct(new WireComparer()))
                );

                compileUnits.Add(MakeCompileUnit(flgNetOr, commentText, titleText));
                continue;
            }

            if (!string.IsNullOrWhiteSpace(coilName))
            {
                coilAccessUid = NextGlobalUid();
                parts.Add(new XElement("Access",
                    new XAttribute("Scope", "LocalVariable"),
                    new XAttribute("UId", coilAccessUid),
                    new XElement("Symbol",
                        new XElement("Component", new XAttribute("Name", coilName)))));
            }

            if (contactUids.Any())
            {
                AddWireSafelyLocal(new XElement("Wire",
                    new XAttribute("UId", NextGlobalUid()),
                    new XElement("Powerrail"),
                    new XElement("NameCon", new XAttribute("UId", contactUids[0]), new XAttribute("Name", "in"))));

                for (int i = 0; i < contactUids.Count - 1; i++)
                {
                    AddWireSafelyLocal(new XElement("Wire",
                        new XAttribute("UId", NextGlobalUid()),
                        new XElement("NameCon", new XAttribute("UId", contactUids[i]), new XAttribute("Name", "out")),
                        new XElement("NameCon", new XAttribute("UId", contactUids[i + 1]), new XAttribute("Name", "in"))));
                }

                AddWireSafelyLocal(new XElement("Wire",
                    new XAttribute("UId", NextGlobalUid()),
                    new XElement("NameCon", new XAttribute("UId", contactUids.Last()), new XAttribute("Name", "out")),
                    new XElement("NameCon", new XAttribute("UId", coilPartUid), new XAttribute("Name", "in"))));
            }
            else
            {
                AddWireSafelyLocal(new XElement("Wire",
                    new XAttribute("UId", NextGlobalUid()),
                    new XElement("Powerrail"),
                    new XElement("NameCon", new XAttribute("UId", coilPartUid), new XAttribute("Name", "in"))));
            }

            if (coilAccessUid > 0)
            {
                AddWireSafelyLocal(new XElement("Wire",
                    new XAttribute("UId", NextGlobalUid()),
                    new XElement("IdentCon", new XAttribute("UId", coilAccessUid)),
                    new XElement("NameCon", new XAttribute("UId", coilPartUid), new XAttribute("Name", "operand"))));
            }

            var cleanedWires = new List<XElement>();
            foreach (var w in wires)
                AddWireSafelyGlobal(cleanedWires, w);

            EnsureInPins(cleanedWires, parts, pinMap);

            cleanedWires.RemoveAll(w => w.Elements().Count(e =>
                e.Name.LocalName == "Powerrail" ||
                e.Name.LocalName == "NameCon" ||
                e.Name.LocalName == "IdentCon") < 2);

            var flgNet = new XElement(NsFlgNet + "FlgNet",
                new XElement("Parts", parts),
                new XElement("Wires", cleanedWires));

            compileUnits.Add(MakeCompileUnit(flgNet, commentText, titleText));
        }

        return compileUnits;
    }
    public static void orRungCreate(string rung, List<string> parts)
    {
    }
    public static IEnumerable<XElement> DistinctByUid(IEnumerable<XElement> src)
    {
        return src
            .GroupBy(e => (string?)e.Attribute("UId"))
            .Select(g => g.First());
    }
    public class WireComparer : IEqualityComparer<XElement>
    {
        public bool Equals(XElement? x, XElement? y)
        {
            return XNode.DeepEquals(x, y);
        }

        public int GetHashCode(XElement obj)
        {
            return obj.ToString().GetHashCode();
        }
    }
    //public static void orRung(string rung, List<XElement> partsData)
    //{

    //    const string cdataStart = "<![CDATA[";
    //    const string cdataEnd = "]]>";

    //    if (rung.Contains(cdataStart) && rung.Contains(cdataEnd))
    //    {
    //        int s = rung.IndexOf(cdataStart) + cdataStart.Length;
    //        int e = rung.IndexOf(cdataEnd);
    //        rung = rung.Substring(s, e - s).Trim();
    //    }

    //    // 2. Find brackets
    //    int open = rung.IndexOf('[');
    //    int close = rung.IndexOf(']');

    //    if (open == -1 || close == -1)
    //    {
    //        Console.WriteLine("Invalid format.");
    //        return;
    //    }

    //    // 3. Extract sections
    //    string beforeSection = rung.Substring(0, open).Trim();
    //    string insideSection = rung.Substring(open + 1, close - open - 1).Trim();
    //    string afterSection = rung.Substring(close + 1).Trim().TrimEnd(';');

    //    // 4. Extract multiple BEFORE nodes
    //    // Pattern: WORD(ANYTHING)
    //    var beforeList = System.Text.RegularExpressions.Regex
    //        .Matches(beforeSection, @"\w+\([^)]*\)")
    //        .Select(m => m.Value)
    //        .ToList();

    //    // 5. Split INSIDE by comma
    //    var insideList = insideSection
    //        .Split(',')
    //        .Select(x => x.Trim())
    //        .Where(x => !string.IsNullOrWhiteSpace(x))
    //        .ToList();

    //    // 6. AFTER: also extract patterns if exist
    //    var afterList = System.Text.RegularExpressions.Regex
    //        .Matches(afterSection, @"\w+\([^)]*\)")
    //        .Select(m => m.Value)
    //        .ToList();
    //    int uid = 999;
    //    List<string> beforeElement = new List<string>();
    //    if (beforeList.Count() !=0)
    //    {
    //        foreach(var i in beforeList)
    //        {
    //            var match = Regex.Match(i, @"\(([^)]*)\)");
    //            if (!match.Success) continue;

    //            string operand = match.Groups[1].Value;

    //            if (i.StartsWith("XIO"))
    //            {
    //                beforeElement.Add("Operand," + operand);
    //            }
    //            else if (i.StartsWith("XIC"))
    //            {
    //                beforeElement.Add("Contact," + operand);
    //            }
    //            else if (i.StartsWith("OTE"))
    //            {
    //                beforeElement.Add("Coil," + operand);
    //            }
    //        }
    //    }

    //    List<List<string>> insideListElement = new List<List<string>>();

    //    foreach (var line in insideList)
    //    {
    //        // Extract instruction tokens like: XIC(...), XIO(...), OTE(...)
    //        var instructions = Regex.Matches(line, @"\w+\([^)]*\)")
    //                                .Cast<Match>()
    //                                .Select(m => m.Value)
    //                                .ToList();

    //        List<string> pair = new List<string>();

    //        foreach (var inst in instructions)
    //        {
    //            // Extract operand inside parentheses
    //            var match = Regex.Match(inst, @"\(([^)]*)\)");
    //            if (!match.Success) continue;

    //            string operand = match.Groups[1].Value;

    //            if (inst.StartsWith("XIO"))
    //            {
    //                pair.Add("Operand," + operand);
    //            }
    //            else if (inst.StartsWith("XIC"))
    //            {
    //                pair.Add("Contact," + operand);
    //            }
    //            else if (inst.StartsWith("OTE"))
    //            {
    //                pair.Add("Coil," + operand);
    //            }
    //        }

    //        // Add whole line as ONE node containing multiple subnodes
    //        insideListElement.Add(pair);
    //    }

    //    List<string> afterElement = new List<string>();
    //    if (afterList.Count() != 0)
    //    {
    //        foreach (var i in afterList)
    //        {
    //            var match = Regex.Match(i, @"\(([^)]*)\)");
    //            if (!match.Success) continue;

    //            string operand = match.Groups[1].Value;

    //            if (i.StartsWith("XIO"))
    //            {
    //                afterElement.Add("Operand," + operand);
    //            }
    //            else if (i.StartsWith("XIC"))
    //            {
    //                afterElement.Add("Contact," + operand);
    //            }
    //            else if (i.StartsWith("OTE"))
    //            {
    //                afterElement.Add("Coil," + operand);
    //            }
    //        }
    //    }
    //    int UID = 999;

    //    if (beforeElement.Count()==0)
    //    {

    //    }
    //    //if (beforeElement.Count()==0)
    //    //{
    //    //    List<string> parts = new List<string>();
    //    //    bool coilNotPresent = !insideListElement.Any(x => x.Contains("Coil,"));
    //    //    if (coilNotPresent)
    //    //    {
    //    //        parts.Add( $"<Part Name=\"O\" UId=\"{++UID}\">\r\n      <TemplateValue Name=\"Card\" Type=\"Cardinality\">{insideList.Count()}</TemplateValue>\r\n    </Part>");

    //    //        foreach (var i in insideListElement)
    //    //        {

    //    //            for (int j = 0; j<i.Count(); j++)
    //    //            {
    //    //                if (i[j].StartsWith("Contact"))
    //    //                {
    //    //                    parts.Add($"\n<Access Scope=\"LocalVariable\" UId=\"{++UID}\">\r\n      <Symbol>\r\n        <Component Name=\"{i[j].Split(',')[1]}\" />\r\n      </Symbol>\r\n    </Access>\r\n<Part Name=\"Contact\" UId=\"{++UID}\" />");
    //    //                }
    //    //                if (i[j].StartsWith("Operand"))
    //    //                {
    //    //                    parts.Add($"\n<Access Scope=\"LocalVariable\" UId=\"{++UID}\">\r\n      <Symbol>\r\n        <Component Name=\"{i[j].Split(',')[1]}\" />\r\n      </Symbol>\r\n    </Access>\r\n<Part Name=\"Contact\" UId=\"{++UID}\">\r\n      <Negated Name=\"operand\" />\r\n    </Part>");
    //    //                }
    //    //            }
    //    //        }

    //    //        foreach (var i in afterElement)
    //    //        {
    //    //            if (i.StartsWith("Contact"))
    //    //            {
    //    //                parts.Add($"\n<Access Scope=\"LocalVariable\" UId=\"{++UID}\">\r\n      <Symbol>\r\n        <Component Name=\"{i.Split(',')[1]}\" />\r\n      </Symbol>\r\n    </Access>\r\n<Part Name=\"Contact\" UId=\"{++UID}\" />");
    //    //            }
    //    //            if (i.StartsWith("Operand"))
    //    //            {
    //    //                parts.Add($"\n<Access Scope=\"LocalVariable\" UId=\"{++UID}\">\r\n      <Symbol>\r\n        <Component Name=\"{i.Split(',')[1]}\" />\r\n      </Symbol>\r\n    </Access>\r\n<Part Name=\"Contact\" UId=\"{++UID}\">\r\n      <Negated Name=\"operand\" />\r\n    </Part>");
    //    //            }
    //    //            if (i.StartsWith("Coil"))
    //    //            {
    //    //                parts.Add($"\n<Access Scope=\"LocalVariable\" UId=\"{++UID}\">\r\n      <Symbol>\r\n        <Component Name=\"{i.Split(',')[1]}\" />\r\n      </Symbol>\r\n    </Access>\r\n<Part Name=\"Coil\" UId=\"{++UID}\" />");
    //    //            }



    //    //        }
    //    //        if (parts.Count() == 3)
    //    //        {
    //    //            List<string> wire = new List<string>();
    //    //        }
    //    //    }
    //    //}
    //}
    public static void orRung(
        string rawCData,
        List<XElement> parts,
        List<XElement> wires,
        Func<int> NextGlobalUid,
        string allCoilInside)
    {
        if (string.IsNullOrWhiteSpace(rawCData)) return;

        // Clean CDATA
        string text = rawCData.Trim();
        if (text.StartsWith("<![CDATA[") && text.EndsWith("]]>"))
            text = text.Substring(9, text.Length - 12).Trim();

        // Extract before [ and inside ]
        int openIdx = text.IndexOf('[');
        int closeIdx = text.LastIndexOf(']');
        if (openIdx < 0 || closeIdx < 0 || closeIdx <= openIdx)
            return;

        string before = text.Substring(0, openIdx).Trim();
        string inside = text.Substring(openIdx + 1, closeIdx - openIdx - 1).Trim();

        // FIRST MAIN CONTACT
        Match firstMatch = Regex.Match(before, @"(XIC|XIO)\(([^)]*)\)");
        if (!firstMatch.Success) return;

        string firstType = firstMatch.Groups[1].Value;
        string firstOperand = firstMatch.Groups[2].Value;

        // FIRST ACCESS
        int accBefore = NextGlobalUid();
        parts.Add(
            new XElement("Access",
                new XAttribute("Scope", "LocalVariable"),
                new XAttribute("UId", accBefore),
                new XElement("Symbol",
                    new XElement("Component",
                        new XAttribute("Name", firstOperand))))
        );

        // FIRST CONTACT PART
        int firstContactUid = NextGlobalUid();
        var firstContact = new XElement("Part",
            new XAttribute("Name", "Contact"),
            new XAttribute("UId", firstContactUid));

        if (firstType == "XIO")
            firstContact.Add(new XElement("Negated", new XAttribute("Name", "operand")));

        parts.Add(firstContact);

        // Powerrail → first contact
        int wPR = NextGlobalUid();
        wires.Add(new XElement("Wire",
            new XAttribute("UId", wPR),
            new XElement("Powerrail"),
            new XElement("NameCon",
                new XAttribute("UId", firstContactUid),
                new XAttribute("Name", "in"))
        ));

        // first operand → first contact
        int wOP = NextGlobalUid();
        wires.Add(new XElement("Wire",
            new XAttribute("UId", wOP),
            new XElement("IdentCon", new XAttribute("UId", accBefore)),
            new XElement("NameCon",
                new XAttribute("UId", firstContactUid),
                new XAttribute("Name", "operand"))
        ));

        var groups = inside.Split(',')
                   .Select(x => x.Trim())
                   .Where(x => x.Length > 0)
                   .ToList();

        List<int> branchContactUids = new List<int>();

        foreach (var group in groups)
        {
            // Contact inside branch
            Match mc = Regex.Match(group, @"(XIC|XIO)\(([^)]*)\)");
            if (mc.Success)
            {
                string cType = mc.Groups[1].Value;
                string contactOperand = mc.Groups[2].Value;

                // ALWAYS two Access entries for branch contacts
                int acc1 = NextGlobalUid();
                parts.Add(new XElement("Access",
                    new XAttribute("Scope", "LocalVariable"),
                    new XAttribute("UId", acc1),
                    new XElement("Symbol",
                        new XElement("Component", new XAttribute("Name", contactOperand)))
                ));

                //int acc2 = NextGlobalUid();
                //parts.Add(new XElement("Access",
                //    new XAttribute("Scope", "LocalVariable"),
                //    new XAttribute("UId", acc2),
                //    new XElement("Symbol",
                //        new XElement("Component", new XAttribute("Name", contactOperand)))
                //));

                // *** MISSING CONTACT PART FIXED HERE ***
                int contactUid = NextGlobalUid();
                XElement cPart = new XElement("Part",
                    new XAttribute("Name", "Contact"),
                    new XAttribute("UId", contactUid));

                if (cType == "XIO")
                    cPart.Add(new XElement("Negated", new XAttribute("Name", "operand")));

                parts.Add(cPart);
                branchContactUids.Add(contactUid);

                // Now detect coil
                Match mcCoil = Regex.Match(group, @"OTE\(([^)]*)\)");
                if (!mcCoil.Success) continue;
                string coilOperand = mcCoil.Groups[1].Value;

                int accCoil = NextGlobalUid();
                parts.Add(new XElement("Access",
                    new XAttribute("Scope", "LocalVariable"),
                    new XAttribute("UId", accCoil),
                    new XElement("Symbol",
                        new XElement("Component", new XAttribute("Name", coilOperand)))
                ));

                int coilUid = NextGlobalUid();
                parts.Add(new XElement("Part",
                    new XAttribute("Name", "Coil"),
                    new XAttribute("UId", coilUid)));

                // Contact → Coil
                wires.Add(new XElement("Wire",
                    new XAttribute("UId", NextGlobalUid()),
                    new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "out")),
                    new XElement("NameCon", new XAttribute("UId", coilUid), new XAttribute("Name", "in"))
                ));

                // Coil operand wire
                wires.Add(new XElement("Wire",
                    new XAttribute("UId", NextGlobalUid()),
                    new XElement("IdentCon", new XAttribute("UId", accCoil)),
                    new XElement("NameCon", new XAttribute("UId", coilUid), new XAttribute("Name", "operand"))
                ));

                // Operand → Contact
                wires.Add(new XElement("Wire",
                    new XAttribute("UId", NextGlobalUid()),
                    new XElement("IdentCon", new XAttribute("UId", acc1)),
                    new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "operand"))
                ));
                continue;
            }
            Match mcCoilOnly = Regex.Match(group, @"OTE\(([^)]*)\)");
            if (mcCoilOnly.Success)
            {
                string coilOperand = mcCoilOnly.Groups[1].Value;

                // Access for coil operand
                int accCoil = NextGlobalUid();
                parts.Add(new XElement("Access",
                    new XAttribute("Scope", "LocalVariable"),
                    new XAttribute("UId", accCoil),
                    new XElement("Symbol",
                        new XElement("Component", new XAttribute("Name", coilOperand)))
                ));

                // Part: Coil
                int coilUid = NextGlobalUid();
                parts.Add(new XElement("Part",
                    new XAttribute("Name", "Coil"),
                    new XAttribute("UId", coilUid)));

                // Wire: FIRST contact OUT → this coil IN
                wires.Add(new XElement("Wire",
                    new XAttribute("UId", NextGlobalUid()),
                    new XElement("NameCon", new XAttribute("UId", firstContactUid), new XAttribute("Name", "out")),
                    new XElement("NameCon", new XAttribute("UId", coilUid), new XAttribute("Name", "in"))
                ));

                // Wire: Coil operand
                wires.Add(new XElement("Wire",
                    new XAttribute("UId", NextGlobalUid()),
                    new XElement("IdentCon", new XAttribute("UId", accCoil)),
                    new XElement("NameCon", new XAttribute("UId", coilUid), new XAttribute("Name", "operand"))
                ));

                continue;
            }
        }

        // FIRST OUT → ALL branch contacts
        var wPar = new XElement("Wire",
            new XAttribute("UId", NextGlobalUid()));

        wPar.Add(new XElement("NameCon",
            new XAttribute("UId", firstContactUid),
            new XAttribute("Name", "out")));

        foreach (var uid in branchContactUids)
        {
            wPar.Add(new XElement("NameCon",
                new XAttribute("UId", uid),
                new XAttribute("Name", "in")));
        }

        wires.Add(wPar);
    }


    public static void orRung(
    string rawCData,
    List<XElement> parts,
    List<XElement> wires,
    Func<int> NextGlobalUid)
    {
        if (string.IsNullOrWhiteSpace(rawCData)) return;

        const string cdataStart = "<![CDATA[";
        const string cdataEnd = "]]>";
        string text = rawCData.Trim();
        if (text.StartsWith(cdataStart, StringComparison.OrdinalIgnoreCase) && text.EndsWith(cdataEnd))
            text = text.Substring(cdataStart.Length, text.Length - cdataStart.Length - cdataEnd.Length).Trim();

        int openIdx = text.IndexOf('[');
        int closeIdx = text.LastIndexOf(']');
        if (openIdx < 0 || closeIdx < 0 || closeIdx <= openIdx) return;

        string beforeSection = text.Substring(0, openIdx).Trim();
        string insideSection = text.Substring(openIdx + 1, closeIdx - openIdx - 1).Trim();
        string afterSection = text.Substring(closeIdx + 1).Trim().TrimEnd(';');

        if (!insideSection.Contains(",")) return;

        var groups = insideSection
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(g => g.Trim())
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .ToList();

        var beforeGroups = beforeSection
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(g => g.Trim())
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .ToList();

        var afterGroup = afterSection
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(g => g.Trim())
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .ToList();

        if (!groups.Any()) return;

        var instrPattern = new Regex(@"\b\w+\([^)]*\)");

        var orElementUID = new List<int>();
        var powerrailUids = new List<int>();
        var beforepowerrailUids = new List<int>();

        var contactUids = new List<int>();
        var beforecontactUids = new List<int>();

        var contactAccessUids = new List<int>();
        var beforecontactAccessUids = new List<int>();

        string coilName = null;
        int coilAccessUid = -1;
        int coilPartUid = -1;

        if (beforeGroups.Count() !=0)
        {
            foreach (var before in beforeGroups)
            {
                var instMatches = instrPattern.Matches(before).Cast<Match>().Select(m => m.Value).ToList();
                if (!instMatches.Any()) continue;

                string chosenInst = instMatches.FirstOrDefault(x => x.StartsWith("XIC", StringComparison.OrdinalIgnoreCase)
                                                                  || x.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
                                    ?? instMatches.First();

                var opMatch = Regex.Match(chosenInst, @"\(([^)]*)\)");
                if (!opMatch.Success) continue;
                string operand = opMatch.Groups[1].Value.Trim();
                if (string.IsNullOrWhiteSpace(operand)) continue;

                int accessUid = NextGlobalUid();
                var accessElem = new XElement("Access",
                    new XAttribute("Scope", "LocalVariable"),
                    new XAttribute("UId", accessUid),
                    new XElement("Symbol",
                        new XElement("Component", new XAttribute("Name", operand)))
                );
                parts.Add(accessElem);
                beforecontactAccessUids.Add(accessUid);

                int contactUid = NextGlobalUid();
                var contactPart = new XElement("Part", new XAttribute("Name", "Contact"), new XAttribute("UId", contactUid));
                if (chosenInst.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
                {
                    contactPart.Add(new XElement("Negated", new XAttribute("Name", "operand")));
                }
                parts.Add(contactPart);
                beforecontactUids.Add(contactUid);
                beforepowerrailUids.Add(contactUid);


                var pattern = @"\b(XIC|XIO)\([^)]*\)";

                var matches = Regex.Matches(before, pattern, RegexOptions.IgnoreCase);
                List<string> moreGroup = new List<string>();
                foreach (Match match in matches)
                {
                    string instruction = match.Value.Trim();

                    if (instruction.StartsWith("XIC", StringComparison.OrdinalIgnoreCase))
                    {
                        moreGroup.Add(instruction);
                    }
                    else if (instruction.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
                    {
                        moreGroup.Add(instruction);
                    }
                }
                if (moreGroup.Count() > 1)
                {
                    for (int k = 1; k < moreGroup.Count(); k++)
                    {
                        opMatch = Regex.Match(moreGroup[k], @"\(([^)]*)\)");
                        operand = opMatch.Groups[1].Value.Trim();
                        accessUid = NextGlobalUid();
                        accessElem = new XElement("Access",
                            new XAttribute("Scope", "LocalVariable"),
                            new XAttribute("UId", accessUid),
                            new XElement("Symbol",
                                new XElement("Component", new XAttribute("Name", operand)))
                        );
                        parts.Add(accessElem);
                        //contactAccessUids.Add(accessUid);
                        contactUid = NextGlobalUid();
                        contactPart = new XElement("Part", new XAttribute("Name", "Contact"), new XAttribute("UId", contactUid));
                        // If original token was XIO, mark negated operand
                        if (chosenInst.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
                        {
                            contactPart.Add(new XElement("Negated", new XAttribute("Name", "operand")));
                        }
                        parts.Add(contactPart);
                        beforecontactUids.Add(contactUid);
                        beforecontactAccessUids.Add(accessUid);
                    }
                }                
            }
        }
        foreach (var group in groups)
        {
            var instMatches = instrPattern.Matches(group).Cast<Match>().Select(m => m.Value).ToList();
            if (!instMatches.Any()) continue;

            string chosenInst = instMatches.FirstOrDefault(x => x.StartsWith("XIC", StringComparison.OrdinalIgnoreCase)
                                                              || x.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
                                ?? instMatches.First();

            var opMatch = Regex.Match(chosenInst, @"\(([^)]*)\)");
            if (!opMatch.Success) continue;
            string operand = opMatch.Groups[1].Value.Trim();
            if (string.IsNullOrWhiteSpace(operand)) continue;

            int accessUid = NextGlobalUid();
            var accessElem = new XElement("Access",
                new XAttribute("Scope", "LocalVariable"),
                new XAttribute("UId", accessUid),
                new XElement("Symbol",
                    new XElement("Component", new XAttribute("Name", operand)))
            );
            parts.Add(accessElem);
            contactAccessUids.Add(accessUid);
            
            int contactUid = NextGlobalUid();
            var contactPart = new XElement("Part", new XAttribute("Name", "Contact"), new XAttribute("UId", contactUid));
            if (chosenInst.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
            {
                contactPart.Add(new XElement("Negated", new XAttribute("Name", "operand")));
            }
            parts.Add(contactPart);
            contactUids.Add(contactUid);
            powerrailUids.Add(contactUid);            

            var pattern = @"\b(XIC|XIO)\([^)]*\)";

            var matches = Regex.Matches(group, pattern, RegexOptions.IgnoreCase);
            List<string> moreGroup = new List<string>();
            foreach (Match match in matches)
            {
                string instruction = match.Value.Trim();

                if (instruction.StartsWith("XIC", StringComparison.OrdinalIgnoreCase))
                {
                    moreGroup.Add(instruction);
                }
                else if (instruction.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
                {
                    moreGroup.Add(instruction);
                }
            }
            if (moreGroup.Count()>1)
            {
                for (int k = 1;k < moreGroup.Count();k++)
                {
                    opMatch = Regex.Match(moreGroup[k], @"\(([^)]*)\)");
                    operand = opMatch.Groups[1].Value.Trim();
                    accessUid = NextGlobalUid();
                    accessElem = new XElement("Access",
                        new XAttribute("Scope", "LocalVariable"),
                        new XAttribute("UId", accessUid),
                        new XElement("Symbol",
                            new XElement("Component", new XAttribute("Name", operand)))
                    );
                    parts.Add(accessElem);
                    contactAccessUids.Add(accessUid);
                    contactUid = NextGlobalUid();
                    contactPart = new XElement("Part", new XAttribute("Name", "Contact"), new XAttribute("UId", contactUid));
                    if (chosenInst.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
                    {
                        contactPart.Add(new XElement("Negated", new XAttribute("Name", "operand")));
                    }
                    parts.Add(contactPart);
                    contactUids.Add(contactUid);
                    
                    if(k == moreGroup.Count()-1)
                    { 
                        orElementUID.Add(contactUid);
                    }
                }
            }
            else
            {
                orElementUID.Add(contactUid);
            }

        }

        var coilMatch = Regex.Match(afterSection, @"\bOTE\(([^)]*)\)", RegexOptions.IgnoreCase);
        try
        {

            if (coilMatch.Success)
            {
                coilName = coilMatch.Groups[1].Value.Trim();
                if (!string.IsNullOrWhiteSpace(coilName))
                {
                    coilAccessUid = NextGlobalUid();
                    var coilAccessElem = new XElement("Access",
                        new XAttribute("Scope", "LocalVariable"),
                        new XAttribute("UId", coilAccessUid),
                        new XElement("Symbol",
                            new XElement("Component", new XAttribute("Name", coilName)))
                    );
                    parts.Add(coilAccessElem);

                    coilPartUid = NextGlobalUid();
                    parts.Add(new XElement("Part", new XAttribute("Name", "Coil"), new XAttribute("UId", coilPartUid)));
                }
            }

            int orUid = NextGlobalUid();
            bool allAreCoils = groups.All(g => g.Contains("OTE", StringComparison.OrdinalIgnoreCase));

            if (!allAreCoils)
            {
                var orPart = new XElement("Part", new XAttribute("Name", "O"), new XAttribute("UId", orUid),
                    new XElement("TemplateValue", new XAttribute("Name", "Card"), new XAttribute("Type", "Cardinality"), orElementUID.Count.ToString())
                );
                parts.Add(orPart);
            }
            int powerWireUid = NextGlobalUid();
            var powerWire = new XElement("Wire", new XAttribute("UId", powerWireUid));
            powerWire.Add(new XElement("Powerrail"));
            if (beforeSection.Count() == 0)
            {
                foreach (var cUid in powerrailUids)
                    powerWire.Add(new XElement("NameCon", new XAttribute("UId", cUid), new XAttribute("Name", "in")));
                wires.Add(powerWire);
            }
            else
            {
                foreach (var cUid in beforepowerrailUids)
                    powerWire.Add(new XElement("NameCon", new XAttribute("UId", cUid), new XAttribute("Name", "in")));
                wires.Add(powerWire);
                //powerWire.Add(new XElement("NameCon", new XAttribute("UId", cUid), new XAttribute("Name", "in")));
                //wires.Add(powerWire);
            }
            
            for (int i = 0; i < beforecontactUids.Count; i++)
            {
                int accessUid = beforecontactAccessUids[i];
                int contactUid = beforecontactUids[i];
                int wUid = NextGlobalUid();
                var w = new XElement("Wire", new XAttribute("UId", wUid),
                    new XElement("IdentCon", new XAttribute("UId", accessUid)),
                    new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "operand"))
                );
                wires.Add(w);
            }
            for (int i = 0; i < contactUids.Count; i++)
            {
                int accessUid = contactAccessUids[i];
                int contactUid = contactUids[i];
                int wUid = NextGlobalUid();
                var w = new XElement("Wire", new XAttribute("UId", wUid),
                    new XElement("IdentCon", new XAttribute("UId", accessUid)),
                    new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "operand"))
                );
                wires.Add(w);
            }

            int orInCount = 1;
            if (beforeGroups.Count() > 0)
            {
                if (beforecontactUids.Count() >= 1)
                {
                    for (int j = 0; j < beforecontactUids.Count; j++)
                    {
                        if (beforecontactUids.Count() == 1 && powerrailUids[0] == contactUids[0])
                        {
                            int contactUid = beforecontactUids[j];

                            int wUid = NextGlobalUid();

                            var w = new XElement("Wire", new XAttribute("UId", wUid));

                            w.Add(new XElement("NameCon",
                                    new XAttribute("UId", contactUid),
                                    new XAttribute("Name", "out")));

                            foreach (var innerEle in powerrailUids)
                            {
                                w.Add(new XElement("NameCon",
                                        new XAttribute("UId", innerEle),
                                        new XAttribute("Name", "in")));
                            }

                            wires.Add(w);
                        }
                        else if (j != beforecontactUids.Count()-1)
                        {
                            int contactUid = beforecontactUids[j];
                            int wUid = NextGlobalUid();
                            var w = new XElement("Wire", new XAttribute("UId", wUid),
                                new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "out")),
                                new XElement("NameCon", new XAttribute("UId", beforecontactUids[j + 1]), new XAttribute("Name", "in"))
                            );
                            wires.Add(w);
                        }
                        else if (j == beforecontactUids.Count()-1)
                        {
                            int contactUid = beforecontactUids[j];

                            int wUid = NextGlobalUid();

                            var w = new XElement("Wire", new XAttribute("UId", wUid));

                            w.Add(new XElement("NameCon",
                                    new XAttribute("UId", contactUid),
                                    new XAttribute("Name", "out")));

                            foreach (var innerEle in powerrailUids)
                            {
                                w.Add(new XElement("NameCon",
                                        new XAttribute("UId", innerEle),
                                        new XAttribute("Name", "in")));
                            }

                            wires.Add(w);
                        }
                    }
                    for (int i = 0; i < contactUids.Count; i++)
                    {
                        try
                        {
                            if (!orElementUID.Contains(contactUids[i]))
                            {
                                int contactUid = contactUids[i];
                                int wUid = NextGlobalUid();
                                var w = new XElement("Wire", new XAttribute("UId", wUid),
                                    new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "out")),
                                    new XElement("NameCon", new XAttribute("UId", contactUids[i + 1]), new XAttribute("Name", "in"))
                                );
                                wires.Add(w);
                            }
                            else
                            {
                                int contactUid = contactUids[i];
                                int wUid = NextGlobalUid();
                                var w = new XElement("Wire", new XAttribute("UId", wUid),
                                    new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "out")),
                                    new XElement("NameCon", new XAttribute("UId", orUid), new XAttribute("Name", "in" + (orInCount)))
                                );
                                wires.Add(w);
                                orInCount++;
                            }
                        }
                        catch
                        {
                            string text1 = text;
                        }
                    }
                }
                else
                {
                }
            }
            else
            {
                for (int i = 0; i < contactUids.Count; i++)
                {
                    try
                    {
                        if (!orElementUID.Contains(contactUids[i]))
                        {
                            int contactUid = contactUids[i];
                            int wUid = NextGlobalUid();
                            var w = new XElement("Wire", new XAttribute("UId", wUid),
                                new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "out")),
                                new XElement("NameCon", new XAttribute("UId", contactUids[i + 1]), new XAttribute("Name", "in"))
                            );
                            wires.Add(w);
                        }
                        else
                        {
                            int contactUid = contactUids[i];
                            int wUid = NextGlobalUid();
                            var w = new XElement("Wire", new XAttribute("UId", wUid),
                                new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "out")),
                                new XElement("NameCon", new XAttribute("UId", orUid), new XAttribute("Name", "in" + (orInCount)))
                            );
                            wires.Add(w);
                            orInCount++;
                        }
                    }
                    catch
                    {
                        string text1 = text;
                    }
                }
            }

            if (coilPartUid > 0)
            {
                int wUid = NextGlobalUid();
                wires.Add(new XElement("Wire", new XAttribute("UId", wUid),
                    new XElement("NameCon", new XAttribute("UId", orUid), new XAttribute("Name", "out")),
                    new XElement("NameCon", new XAttribute("UId", coilPartUid), new XAttribute("Name", "in"))
                ));

                int wUid2 = NextGlobalUid();
                wires.Add(new XElement("Wire", new XAttribute("UId", wUid2),
                    new XElement("IdentCon", new XAttribute("UId", coilAccessUid)),
                    new XElement("NameCon", new XAttribute("UId", coilPartUid), new XAttribute("Name", "operand"))
                ));
            }
        }
        catch
        {
            string text2 = text;
        }

    }

    private record ContactTok(string Type, string Operand);
    private record Branch(List<ContactTok> Contacts, string? Coil);
    public static List<object> ParseBlocks(string s)
    {
        List<object> result = new();
        StringBuilder token = new();
        int level = 0;

        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];

            if (c == '[')
            {
                if (level == 0 && token.Length > 0)
                {
                    result.Add(token.ToString().Trim());
                    token.Clear();
                }

                level++;
                if (level == 1)
                {
                    int j = i + 1, d = 1;
                    while (j < s.Length && d > 0)
                    {
                        if (s[j] == '[') d++;
                        else if (s[j] == ']') d--;
                        j++;
                    }

                    result.Add(ParseBlocks(s.Substring(i + 1, j - i - 2)));
                    i = j - 1;
                    level = 0;
                    continue;
                }
            }
            else if (c == ']' && level > 0)
            {
                level--;
            }
            else if (level == 0)
            {
                token.Append(c);
            }
        }
        if (token.Length > 0)
            result.Add(token.ToString().Trim());

        return result;
    }

    public static void orRung(
    string rawCData,
    List<XElement> parts,
    List<XElement> wires,
    Func<int> NextGlobalUid, bool multiple)
    {
        if (string.IsNullOrWhiteSpace(rawCData)) return;

        var blocks = ParseBlocks(rawCData);
        const string cdataStart = "<![CDATA[";
        const string cdataEnd = "]]>";
        string text = rawCData.Trim();

        if (text.StartsWith(cdataStart, StringComparison.OrdinalIgnoreCase) && text.EndsWith(cdataEnd))
            text = text.Substring(cdataStart.Length, text.Length - cdataStart.Length - cdataEnd.Length).Trim();

        int openIdx = text.IndexOf('[');
        int closeIdx = text.LastIndexOf(']');
        if (openIdx < 0 || closeIdx < 0 || closeIdx <= openIdx) return;

        string beforeSection = text.Substring(0, openIdx).Trim();
        string insideSection = text.Substring(openIdx + 1, closeIdx - openIdx - 1).Trim();
        string afterSection = text.Substring(closeIdx + 1).Trim().TrimEnd(';');

        if (!insideSection.Contains(",")) return;

        var groups = insideSection
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(g => g.Trim())
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .ToList();

        var beforeGroups = beforeSection.Length == 0
            ? new List<string>()
            : beforeSection
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(g => g.Trim())
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .ToList();

        if (!groups.Any()) return;

        var instrPattern = new Regex(@"\b\w+\([^)]*\)", RegexOptions.IgnoreCase);

        var innerContactUids = new List<int>();
        var innerContactAccessUids = new List<int>();
        var innerCoilPartUids = new List<int>();
        var innerCoilAccessUids = new List<int>();
        
        var prefixContactUids = new List<int>();
        var prefixContactAccessUids = new List<int>();

        if (beforeGroups.Any())
        {
            foreach (var pref in beforeGroups)
            {
                var instMatches = instrPattern.Matches(pref).Cast<Match>().Select(m => m.Value).ToList();
                if (!instMatches.Any()) continue;

                string chosenInst = instMatches.FirstOrDefault(x => x.StartsWith("XIC", StringComparison.OrdinalIgnoreCase)
                                                                  || x.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
                                    ?? instMatches.First();

                var opMatch = Regex.Match(chosenInst, @"\(([^)]*)\)");
                if (!opMatch.Success) continue;
                string operand = opMatch.Groups[1].Value.Trim();
                if (string.IsNullOrWhiteSpace(operand)) continue;

                int accessUid = NextGlobalUid();
                var accessElem = new XElement("Access",
                    new XAttribute("Scope", "LocalVariable"),
                    new XAttribute("UId", accessUid),
                    new XElement("Symbol",
                        new XElement("Component", new XAttribute("Name", operand)))
                );
                parts.Add(accessElem);
                prefixContactAccessUids.Add(accessUid);

                int contactUid = NextGlobalUid();
                var contactPart = new XElement("Part", new XAttribute("Name", "Contact"), new XAttribute("UId", contactUid));
                if (chosenInst.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
                {
                    contactPart.Add(new XElement("Negated", new XAttribute("Name", "operand")));
                }
                parts.Add(contactPart);
                prefixContactUids.Add(contactUid);
            }
        }

        foreach (var g in groups)
        {
            var matches = instrPattern.Matches(g).Cast<Match>().Select(m => m.Value).ToList();
            if (!matches.Any()) continue;

            var contactTokens = matches.Where(m => m.StartsWith("XIC", StringComparison.OrdinalIgnoreCase)
                                                || m.StartsWith("XIO", StringComparison.OrdinalIgnoreCase)).ToList();

            if (!contactTokens.Any()) continue;

            foreach (var token in contactTokens)
            {
                var opMatch = Regex.Match(token, @"\(([^)]*)\)");
                if (!opMatch.Success) continue;
                string operand = opMatch.Groups[1].Value.Trim();
                if (string.IsNullOrWhiteSpace(operand)) continue;

                int accessUid = NextGlobalUid();
                var accessElem = new XElement("Access",
                    new XAttribute("Scope", "LocalVariable"),
                    new XAttribute("UId", accessUid),
                    new XElement("Symbol",
                        new XElement("Component", new XAttribute("Name", operand)))
                );
                parts.Add(accessElem);
                innerContactAccessUids.Add(accessUid);

                int contactUid = NextGlobalUid();
                var contactPart = new XElement("Part", new XAttribute("Name", "Contact"), new XAttribute("UId", contactUid));
                if (token.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
                {
                    contactPart.Add(new XElement("Negated", new XAttribute("Name", "operand")));
                }
                parts.Add(contactPart);
                innerContactUids.Add(contactUid);
            }
        }

        int orPartUid = NextGlobalUid();
        var orPart = new XElement("Part", new XAttribute("Name", "O"), new XAttribute("UId", orPartUid));
        var cardValue = innerContactUids.Count > 0 ? innerContactUids.Count.ToString() : "1";
        orPart.Add(new XElement("TemplateValue", new XAttribute("Name", "Card"), new XAttribute("Type", "Cardinality"), cardValue));
        parts.Add(orPart);

        var coilPattern = new Regex(@"\bOTE\([^)]*\)", RegexOptions.IgnoreCase);
        var coilMatches = coilPattern.Matches(afterSection).Cast<Match>().Select(m => m.Value).ToList();

        foreach (var coilTok in coilMatches)
        {
            var opMatch = Regex.Match(coilTok, @"\(([^)]*)\)");
            if (!opMatch.Success) continue;
            string operand = opMatch.Groups[1].Value.Trim();
            if (string.IsNullOrWhiteSpace(operand)) continue;

            int accessUid = NextGlobalUid();
            var accessElem = new XElement("Access",
                new XAttribute("Scope", "LocalVariable"),
                new XAttribute("UId", accessUid),
                new XElement("Symbol",
                    new XElement("Component", new XAttribute("Name", operand)))
            );
            parts.Add(accessElem);
            innerCoilAccessUids.Add(accessUid);

            int coilPartUid = NextGlobalUid();
            var coilPart = new XElement("Part", new XAttribute("Name", "Coil"), new XAttribute("UId", coilPartUid));
            parts.Add(coilPart);
            innerCoilPartUids.Add(coilPartUid);
        }

        if (!innerCoilPartUids.Any() && !string.IsNullOrWhiteSpace(afterSection))
        {
            var fallbackMatch = instrPattern.Match(afterSection);
            if (fallbackMatch.Success)
            {
                var tok = fallbackMatch.Value;
                var opMatch = Regex.Match(tok, @"\(([^)]*)\)");
                if (opMatch.Success)
                {
                    string operand = opMatch.Groups[1].Value.Trim();
                    int accessUid = NextGlobalUid();
                    var accessElem = new XElement("Access",
                        new XAttribute("Scope", "LocalVariable"),
                        new XAttribute("UId", accessUid),
                        new XElement("Symbol",
                            new XElement("Component", new XAttribute("Name", operand)))
                    );
                    parts.Add(accessElem);
                    innerCoilAccessUids.Add(accessUid);

                    int coilPartUid = NextGlobalUid();
                    var coilPart = new XElement("Part", new XAttribute("Name", "Coil"), new XAttribute("UId", coilPartUid));
                    parts.Add(coilPart);
                    innerCoilPartUids.Add(coilPartUid);
                }
            }
        }

        if (prefixContactUids.Any())
        {
            int wireUid = NextGlobalUid();
            var wire = new XElement("Wire", new XAttribute("UId", wireUid));
            wire.Add(new XElement("Powerrail"));
            foreach (var pc in prefixContactUids)
            {
                wire.Add(new XElement("NameCon", new XAttribute("UId", pc), new XAttribute("Name", "in")));
            }
            wires.Add(wire);
        }

        for (int i = 0; i < innerContactUids.Count; i++)
        {
            int contactPartUid = innerContactUids[i];
            int accessUid = innerContactAccessUids.ElementAtOrDefault(i);

            int wireUid = NextGlobalUid();
            var wire = new XElement("Wire", new XAttribute("UId", wireUid));
            if (accessUid != 0)
                wire.Add(new XElement("IdentCon", new XAttribute("UId", accessUid)));
            wire.Add(new XElement("NameCon", new XAttribute("UId", contactPartUid), new XAttribute("Name", "operand")));
            wires.Add(wire);
        }

        for (int i = 0; i < innerContactUids.Count; i++)
        {
            int contactPartUid = innerContactUids[i];
            int wireUid = NextGlobalUid();
            var wire = new XElement("Wire", new XAttribute("UId", wireUid));
            wire.Add(new XElement("NameCon", new XAttribute("UId", contactPartUid), new XAttribute("Name", "out")));
        
            wire.Add(new XElement("NameCon", new XAttribute("UId", orPartUid), new XAttribute("Name", $"in{i + 1}")));
            wires.Add(wire);
        }

        if (innerCoilPartUids.Any())
        {        
            int wireUid = NextGlobalUid();
            var wire = new XElement("Wire", new XAttribute("UId", wireUid));
            wire.Add(new XElement("NameCon", new XAttribute("UId", orPartUid), new XAttribute("Name", "out")));
            foreach (var coilPartUid in innerCoilPartUids)
            {
                wire.Add(new XElement("NameCon", new XAttribute("UId", coilPartUid), new XAttribute("Name", "in")));
            }
            wires.Add(wire);

            for (int i = 0; i < innerCoilPartUids.Count; i++)
            {
                int coilPartUid = innerCoilPartUids[i];
                int coilAccessUid = innerCoilAccessUids.ElementAtOrDefault(i);
                if (coilAccessUid == 0) continue;

                int wUid = NextGlobalUid();
                var w = new XElement("Wire", new XAttribute("UId", wUid));
                w.Add(new XElement("IdentCon", new XAttribute("UId", coilAccessUid)));
                w.Add(new XElement("NameCon", new XAttribute("UId", coilPartUid), new XAttribute("Name", "operand")));
                wires.Add(w);
            }
        }

        if (prefixContactUids.Any() && prefixContactAccessUids.Any())
        {
            for (int i = 0; i < prefixContactUids.Count && i < prefixContactAccessUids.Count; i++)
            {
                int pContactUid = prefixContactUids[i];
                int pAccessUid = prefixContactAccessUids[i];

                int wUid = NextGlobalUid();
                var w = new XElement("Wire", new XAttribute("UId", wUid));
                w.Add(new XElement("IdentCon", new XAttribute("UId", pAccessUid)));
                w.Add(new XElement("NameCon", new XAttribute("UId", pContactUid), new XAttribute("Name", "operand")));
                wires.Add(w);
            }

            if (innerContactUids.Any())
            {
                int wUid = NextGlobalUid();
                var w = new XElement("Wire", new XAttribute("UId", wUid));
                foreach (var p in prefixContactUids)
                {
                    w.Add(new XElement("NameCon", new XAttribute("UId", p), new XAttribute("Name", "out")));
                }
                w.Add(new XElement("NameCon", new XAttribute("UId", innerContactUids[0]), new XAttribute("Name", "in")));
                wires.Add(w);
            }
        }

    }

    //public static void orRung(
    //string rawCData,
    //List<XElement> parts,
    //List<XElement> wires,
    //Func<int> NextGlobalUid,bool multiple)
    //{
    //    if (string.IsNullOrWhiteSpace(rawCData)) return;

    //    var blocks = ParseBlocks(rawCData);
    //    const string cdataStart = "<![CDATA[";
    //    const string cdataEnd = "]]>";
    //    string text = rawCData.Trim();        

    //    if (text.StartsWith(cdataStart, StringComparison.OrdinalIgnoreCase) && text.EndsWith(cdataEnd))
    //        text = text.Substring(cdataStart.Length, text.Length - cdataStart.Length - cdataEnd.Length).Trim();

    //    int openIdx = text.IndexOf('[');
    //    int closeIdx = text.LastIndexOf(']');
    //    if (openIdx < 0 || closeIdx < 0 || closeIdx <= openIdx) return;

    //    string beforeSection = text.Substring(0, openIdx).Trim();
    //    string insideSection = text.Substring(openIdx + 1, closeIdx - openIdx - 1).Trim();
    //    string afterSection = text.Substring(closeIdx + 1).Trim().TrimEnd(';');

    //    if (!insideSection.Contains(",")) return;

    //    var groups = insideSection
    //        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
    //        .Select(g => g.Trim())
    //        .Where(g => !string.IsNullOrWhiteSpace(g))
    //        .ToList();

    //    var beforeGroups = beforeSection.Length == 0
    //        ? new List<string>()
    //        : beforeSection
    //            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
    //            .Select(g => g.Trim())
    //            .Where(g => !string.IsNullOrWhiteSpace(g))
    //            .ToList();

    //    if (!groups.Any()) return;

    //    var instrPattern = new Regex(@"\b\w+\([^)]*\)", RegexOptions.IgnoreCase);

    //    var innerContactUids = new List<int>();
    //    var innerContactAccessUids = new List<int>();
    //    var innerCoilPartUids = new List<int>();
    //    var innerCoilAccessUids = new List<int>();

    //    var prefixContactUids = new List<int>();
    //    var beforecontactAccessUids = new List<int>();
    //    var beforecontactUids = new List<int>();
    //    var beforepowerrailUids = new List<int>();
    //    var prefixContactAccessUids = new List<int>();
    //    var prefixContactUids1 = new List<List<int>>();
    //    var prefixContactAccessUids1 = new List<List<int>>();
    //    var afterContactUids1 = new List<List<int>>();
    //    var afterContactAccessUids1 = new List<List<int>>();
    //    var contactAccessUids = new List<int>();
    //    var contactUids = new List<int>();
    //    var powerrailUids = new List<int>();
    //    var orElementUID = new List<int>();
    //    foreach (var blk in blocks)
    //    {
    //        if (blk is string s)
    //        {
    //            Console.WriteLine("Item: " + s);
    //        }
    //        else if (blk is List<object> list)
    //        {
    //            Console.WriteLine("Block:");
    //            foreach (var b1 in list)
    //            {
    //                if (b1 is string s1)
    //                {
    //                    var instMatches = instrPattern.Matches(s1).Cast<Match>().Select(m => m.Value).ToList();
    //                    if (!instMatches.Any()) continue;

    //                    string chosenInst = instMatches.FirstOrDefault(x => x.StartsWith("XIC", StringComparison.OrdinalIgnoreCase)
    //                                                                      || x.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
    //                                        ?? instMatches.First();

    //                    var opMatch = Regex.Match(chosenInst, @"\(([^)]*)\)");
    //                    if (!opMatch.Success) continue;
    //                    string operand = opMatch.Groups[1].Value.Trim();
    //                    if (string.IsNullOrWhiteSpace(operand)) continue;

    //                    int accessUid = NextGlobalUid();
    //                    var accessElem = new XElement("Access",
    //                        new XAttribute("Scope", "LocalVariable"),
    //                        new XAttribute("UId", accessUid),
    //                        new XElement("Symbol",
    //                            new XElement("Component", new XAttribute("Name", operand)))
    //                    );
    //                    parts.Add(accessElem);
    //                    beforecontactAccessUids.Add(accessUid);

    //                    int contactUid = NextGlobalUid();
    //                    var contactPart = new XElement("Part", new XAttribute("Name", "Contact"), new XAttribute("UId", contactUid));
    //                    if (chosenInst.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
    //                    {
    //                        contactPart.Add(new XElement("Negated", new XAttribute("Name", "operand")));
    //                    }
    //                    parts.Add(contactPart);
    //                    beforecontactUids.Add(contactUid);
    //                    beforepowerrailUids.Add(contactUid);

    //                    var pattern = @"\b(XIC|XIO)\([^)]*\)";

    //                    var matches = Regex.Matches(s1, pattern, RegexOptions.IgnoreCase);
    //                    List<string> moreGroup = new List<string>();
    //                    foreach (Match match in matches)
    //                    {
    //                        string instruction = match.Value.Trim();

    //                        if (instruction.StartsWith("XIC", StringComparison.OrdinalIgnoreCase))
    //                        {
    //                            moreGroup.Add(instruction);
    //                        }
    //                        else if (instruction.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
    //                        {
    //                            moreGroup.Add(instruction);
    //                        }
    //                    }
    //                    if (moreGroup.Count() > 1)
    //                    {
    //                        for (int k = 1; k < moreGroup.Count(); k++)
    //                        {
    //                            opMatch = Regex.Match(moreGroup[k], @"\(([^)]*)\)");
    //                            operand = opMatch.Groups[1].Value.Trim();
    //                            accessUid = NextGlobalUid();
    //                            accessElem = new XElement("Access",
    //                                new XAttribute("Scope", "LocalVariable"),
    //                                new XAttribute("UId", accessUid),
    //                                new XElement("Symbol",
    //                                    new XElement("Component", new XAttribute("Name", operand)))
    //                            );
    //                            parts.Add(accessElem);
    //                            contactUid = NextGlobalUid();
    //                            contactPart = new XElement("Part", new XAttribute("Name", "Contact"), new XAttribute("UId", contactUid));
    //                            if (chosenInst.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
    //                            {
    //                                contactPart.Add(new XElement("Negated", new XAttribute("Name", "operand")));
    //                            }
    //                            parts.Add(contactPart);
    //                            beforecontactUids.Add(contactUid);
    //                            beforecontactAccessUids.Add(accessUid);
    //                        }
    //                    }
    //                }                
    //                else if (b1 is List<object> list1)
    //                {
    //                    foreach (var b2 in list1)
    //                    {
    //                        var instMatches = instrPattern.Matches(b2.ToString()).Cast<Match>().Select(m => m.Value).ToList();
    //                        if (!instMatches.Any()) continue;

    //                        string chosenInst = instMatches.FirstOrDefault(x => x.StartsWith("XIC", StringComparison.OrdinalIgnoreCase)
    //                                                                          || x.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
    //                                            ?? instMatches.First();

    //                        var opMatch = Regex.Match(chosenInst, @"\(([^)]*)\)");
    //                        if (!opMatch.Success) continue;
    //                        string operand = opMatch.Groups[1].Value.Trim();
    //                        if (string.IsNullOrWhiteSpace(operand)) continue;

    //                        int accessUid = NextGlobalUid();
    //                        var accessElem = new XElement("Access",
    //                            new XAttribute("Scope", "LocalVariable"),
    //                            new XAttribute("UId", accessUid),
    //                            new XElement("Symbol",
    //                                new XElement("Component", new XAttribute("Name", operand)))
    //                        );
    //                        parts.Add(accessElem);
    //                        contactAccessUids.Add(accessUid);

    //                        int contactUid = NextGlobalUid();
    //                        var contactPart = new XElement("Part", new XAttribute("Name", "Contact"), new XAttribute("UId", contactUid));
    //                        if (chosenInst.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
    //                        {
    //                            contactPart.Add(new XElement("Negated", new XAttribute("Name", "operand")));
    //                        }
    //                        parts.Add(contactPart);
    //                        contactUids.Add(contactUid);
    //                        powerrailUids.Add(contactUid);

    //                        var pattern = @"\b(XIC|XIO)\([^)]*\)";

    //                        var matches = Regex.Matches(b2.ToString(), pattern, RegexOptions.IgnoreCase);
    //                        List<string> moreGroup = new List<string>();
    //                        foreach (Match match in matches)
    //                        {
    //                            string instruction = match.Value.Trim();

    //                            if (instruction.StartsWith("XIC", StringComparison.OrdinalIgnoreCase))
    //                            {
    //                                moreGroup.Add(instruction);
    //                            }
    //                            else if (instruction.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
    //                            {
    //                                moreGroup.Add(instruction);
    //                            }
    //                        }
    //                        if (moreGroup.Count() > 1)
    //                        {
    //                            for (int k = 1; k < moreGroup.Count(); k++)
    //                            {
    //                                opMatch = Regex.Match(moreGroup[k], @"\(([^)]*)\)");
    //                                operand = opMatch.Groups[1].Value.Trim();
    //                                accessUid = NextGlobalUid();
    //                                accessElem = new XElement("Access",
    //                                    new XAttribute("Scope", "LocalVariable"),
    //                                    new XAttribute("UId", accessUid),
    //                                    new XElement("Symbol",
    //                                        new XElement("Component", new XAttribute("Name", operand)))
    //                                );
    //                                parts.Add(accessElem);
    //                                contactAccessUids.Add(accessUid);
    //                                contactUid = NextGlobalUid();
    //                                contactPart = new XElement("Part", new XAttribute("Name", "Contact"), new XAttribute("UId", contactUid));

    //                                if (chosenInst.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
    //                                {
    //                                    contactPart.Add(new XElement("Negated", new XAttribute("Name", "operand")));
    //                                }
    //                                parts.Add(contactPart);
    //                                contactUids.Add(contactUid);

    //                                if (k == moreGroup.Count() - 1)
    //                                {
    //                                    orElementUID.Add(contactUid);
    //                                }
    //                            }
    //                        }
    //                        else
    //                        {
    //                            orElementUID.Add(contactUid);
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }


    //    foreach (var bg in beforeGroups)
    //    {
    //        var instMatches = instrPattern.Matches(bg).Cast<Match>().Select(m => m.Value).ToList();
    //        if (!instMatches.Any()) continue;

    //        string chosen = instMatches.FirstOrDefault(x => x.StartsWith("XIC", StringComparison.OrdinalIgnoreCase)
    //                                                    || x.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
    //                        ?? instMatches.First();

    //        var opM = Regex.Match(chosen, @"\(([^)]*)\)");
    //        if (!opM.Success) continue;
    //        string operand = opM.Groups[1].Value.Trim();
    //        if (string.IsNullOrWhiteSpace(operand)) continue;

    //        int accessUid = NextGlobalUid();
    //        parts.Add(new XElement("Access",
    //            new XAttribute("Scope", "LocalVariable"),
    //            new XAttribute("UId", accessUid),
    //            new XElement("Symbol",
    //                new XElement("Component", new XAttribute("Name", operand)))
    //        ));
    //        prefixContactAccessUids.Add(accessUid);

    //        int contactUid = NextGlobalUid();
    //        var contactPart = new XElement("Part", new XAttribute("Name", "Contact"), new XAttribute("UId", contactUid));
    //        if (chosen.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
    //            contactPart.Add(new XElement("Negated", new XAttribute("Name", "operand")));
    //        parts.Add(contactPart);
    //        prefixContactUids.Add(contactUid);
    //    }

    //    foreach (var group in groups)
    //    {
    //        var instMatches = instrPattern.Matches(group).Cast<Match>().Select(m => m.Value).ToList();
    //        if (!instMatches.Any())
    //        {
    //            innerContactAccessUids.Add(-1);
    //            innerContactUids.Add(-1);
    //            innerCoilAccessUids.Add(-1);
    //            innerCoilPartUids.Add(-1);
    //            continue;
    //        }

    //        string chosenContact = instMatches.FirstOrDefault(x => x.StartsWith("XIC", StringComparison.OrdinalIgnoreCase)
    //                                                             || x.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
    //                               ?? instMatches.First();
    //        var contactOpM = Regex.Match(chosenContact, @"\(([^)]*)\)");
    //        if (!contactOpM.Success)
    //        {
    //            innerContactAccessUids.Add(-1);
    //            innerContactUids.Add(-1);
    //            innerCoilAccessUids.Add(-1);
    //            innerCoilPartUids.Add(-1);
    //            continue;
    //        }

    //        string contactOperand = contactOpM.Groups[1].Value.Trim();
    //        if (string.IsNullOrWhiteSpace(contactOperand))
    //        {
    //            innerContactAccessUids.Add(-1);
    //            innerContactUids.Add(-1);
    //            innerCoilAccessUids.Add(-1);
    //            innerCoilPartUids.Add(-1);
    //            continue;
    //        }

    //        int contactAccessUid = NextGlobalUid();
    //        parts.Add(new XElement("Access",
    //            new XAttribute("Scope", "LocalVariable"),
    //            new XAttribute("UId", contactAccessUid),
    //            new XElement("Symbol",
    //                new XElement("Component", new XAttribute("Name", contactOperand)))
    //        ));
    //        innerContactAccessUids.Add(contactAccessUid);

    //        int contactPartUid = NextGlobalUid();
    //        var contactPartElem = new XElement("Part", new XAttribute("Name", "Contact"), new XAttribute("UId", contactPartUid));
    //        if (chosenContact.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
    //            contactPartElem.Add(new XElement("Negated", new XAttribute("Name", "operand")));
    //        parts.Add(contactPartElem);
    //        innerContactUids.Add(contactPartUid);

    //        var coilMatch = Regex.Match(group, @"\bOTE\(([^)]*)\)", RegexOptions.IgnoreCase);
    //        if (coilMatch.Success)
    //        {
    //            string coilOperand = coilMatch.Groups[1].Value.Trim();
    //            if (!string.IsNullOrWhiteSpace(coilOperand))
    //            {
    //                int coilAccessUid = NextGlobalUid();
    //                parts.Add(new XElement("Access",
    //                    new XAttribute("Scope", "LocalVariable"),
    //                    new XAttribute("UId", coilAccessUid),
    //                    new XElement("Symbol",
    //                        new XElement("Component", new XAttribute("Name", coilOperand)))
    //                ));
    //                innerCoilAccessUids.Add(coilAccessUid);

    //                int coilPartUid = NextGlobalUid();
    //                parts.Add(new XElement("Part", new XAttribute("Name", "Coil"), new XAttribute("UId", coilPartUid)));
    //                innerCoilPartUids.Add(coilPartUid);
    //            }
    //            else
    //            {
    //                innerCoilAccessUids.Add(-1);
    //                innerCoilPartUids.Add(-1);
    //            }
    //        }
    //        else
    //        {
    //            innerCoilAccessUids.Add(-1);
    //            innerCoilPartUids.Add(-1);
    //        }
    //    }

    //    bool groupsAreAllCoilsOnly = true;
    //    foreach (var g in groups)
    //    {
    //        if (Regex.IsMatch(g, @"\bXIC\b|\bXIO\b", RegexOptions.IgnoreCase))
    //        {
    //            groupsAreAllCoilsOnly = false;
    //            break;
    //        }
    //    }

    //    int orUid = -1;
    //    var nonEmptyContactUids = innerContactUids.Where(u => u > 0).ToList();
    //    if (!groupsAreAllCoilsOnly && nonEmptyContactUids.Count > 0)
    //    {
    //        orUid = NextGlobalUid();
    //        var orPart = new XElement("Part", new XAttribute("Name", "O"), new XAttribute("UId", orUid),
    //            new XElement("TemplateValue", new XAttribute("Name", "Card"), new XAttribute("Type", "Cardinality"), nonEmptyContactUids.Count.ToString())
    //        );
    //        parts.Add(orPart);
    //    }

    //    if (prefixContactUids.Count > 0)
    //    {
    //        int powerWireUid = NextGlobalUid();
    //        var powerWire = new XElement("Wire", new XAttribute("UId", powerWireUid));
    //        powerWire.Add(new XElement("Powerrail"));
    //        powerWire.Add(new XElement("NameCon", new XAttribute("UId", prefixContactUids[0]), new XAttribute("Name", "in")));
    //        wires.Add(powerWire);

    //        for (int i = 0; i < prefixContactAccessUids.Count; i++)
    //        {
    //            int aUid = prefixContactAccessUids[i];
    //            int cUid = prefixContactUids[i];
    //            if (aUid <= 0 || cUid <= 0) continue;
    //            int wUid = NextGlobalUid();
    //            wires.Add(new XElement("Wire", new XAttribute("UId", wUid),
    //                new XElement("IdentCon", new XAttribute("UId", aUid)),
    //                new XElement("NameCon", new XAttribute("UId", cUid), new XAttribute("Name", "operand"))
    //            ));
    //        }

    //        var busWireUid = NextGlobalUid();
    //        var busWire = new XElement("Wire", new XAttribute("UId", busWireUid));
    //        busWire.Add(new XElement("NameCon", new XAttribute("UId", prefixContactUids[0]), new XAttribute("Name", "out")));
    //        foreach (var cUid in innerContactUids.Where(u => u > 0))
    //            busWire.Add(new XElement("NameCon", new XAttribute("UId", cUid), new XAttribute("Name", "in")));
    //        wires.Add(busWire);

    //        for (int i = 0; i < innerCoilPartUids.Count; i++)
    //        {
    //            if (innerCoilPartUids[i] > 0 && (innerContactUids[i] <= 0))
    //            {
    //                int wUid = NextGlobalUid();
    //                wires.Add(new XElement("Wire", new XAttribute("UId", wUid),
    //                    new XElement("NameCon", new XAttribute("UId", prefixContactUids[0]), new XAttribute("Name", "out")),
    //                    new XElement("NameCon", new XAttribute("UId", innerCoilPartUids[i]), new XAttribute("Name", "in"))
    //                ));
    //            }
    //        }
    //    }
    //    else
    //    {
    //        int powerWireUid = NextGlobalUid();
    //        var powerWire = new XElement("Wire", new XAttribute("UId", powerWireUid));
    //        powerWire.Add(new XElement("Powerrail"));
    //        foreach (var cUid in innerContactUids.Where(u => u > 0))
    //            powerWire.Add(new XElement("NameCon", new XAttribute("UId", cUid), new XAttribute("Name", "in")));
    //        wires.Add(powerWire);
    //    }

    //    for (int i = 0; i < innerContactUids.Count; i++)
    //    {
    //        int accessUid = innerContactAccessUids[i];
    //        int contactUid = innerContactUids[i];
    //        if (accessUid <= 0 || contactUid <= 0) continue;

    //        int wUid = NextGlobalUid();
    //        wires.Add(new XElement("Wire", new XAttribute("UId", wUid),
    //            new XElement("IdentCon", new XAttribute("UId", accessUid)),
    //            new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "operand"))
    //        ));
    //    }

    //    for (int i = 0; i < innerContactUids.Count; i++)
    //    {
    //        int contactUid = innerContactUids[i];
    //        int coilPartUid = innerCoilPartUids[i];

    //        if (contactUid <= 0) continue;

    //        if (orUid > 0 && coilPartUid <= 0)
    //        {
    //            int wUid = NextGlobalUid();
    //            wires.Add(new XElement("Wire", new XAttribute("UId", wUid),
    //                new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "out")),
    //                new XElement("NameCon", new XAttribute("UId", orUid), new XAttribute("Name", "in" + (innerContactUids.Take(i + 1).Count(u => u > 0))))
    //            ));
    //        }
    //        else if (coilPartUid > 0)
    //        {
    //            int wUid = NextGlobalUid();
    //            wires.Add(new XElement("Wire", new XAttribute("UId", wUid),
    //                new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "out")),
    //                new XElement("NameCon", new XAttribute("UId", coilPartUid), new XAttribute("Name", "in"))
    //            ));
    //        }
    //    }

    //    for (int i = 0; i < innerCoilAccessUids.Count; i++)
    //    {
    //        int coilAccessUid = innerCoilAccessUids[i];
    //        int coilPartUid = innerCoilPartUids[i];
    //        if (coilAccessUid <= 0 || coilPartUid <= 0) continue;

    //        int wUid = NextGlobalUid();
    //        wires.Add(new XElement("Wire", new XAttribute("UId", wUid),
    //            new XElement("IdentCon", new XAttribute("UId", coilAccessUid)),
    //            new XElement("NameCon", new XAttribute("UId", coilPartUid), new XAttribute("Name", "operand"))
    //        ));
    //    }

    //    var afterCoilMatch = Regex.Match(afterSection, @"\bOTE\(([^)]*)\)", RegexOptions.IgnoreCase);
    //    if (afterCoilMatch.Success && orUid > 0)
    //    {
    //        string coilName = afterCoilMatch.Groups[1].Value.Trim();
    //        if (!string.IsNullOrWhiteSpace(coilName))
    //        {
    //            int coilAccessUid = NextGlobalUid();
    //            parts.Add(new XElement("Access",
    //                new XAttribute("Scope", "LocalVariable"),
    //                new XAttribute("UId", coilAccessUid),
    //                new XElement("Symbol",
    //                    new XElement("Component", new XAttribute("Name", coilName)))
    //            ));

    //            int coilPartUid = NextGlobalUid();
    //            parts.Add(new XElement("Part", new XAttribute("Name", "Coil"), new XAttribute("UId", coilPartUid)));

    //            int wUid = NextGlobalUid();
    //            wires.Add(new XElement("Wire", new XAttribute("UId", wUid),
    //                new XElement("NameCon", new XAttribute("UId", orUid), new XAttribute("Name", "out")),
    //                new XElement("NameCon", new XAttribute("UId", coilPartUid), new XAttribute("Name", "in"))
    //            ));

    //            int wUid2 = NextGlobalUid();
    //            wires.Add(new XElement("Wire", new XAttribute("UId", wUid2),
    //                new XElement("IdentCon", new XAttribute("UId", coilAccessUid)),
    //                new XElement("NameCon", new XAttribute("UId", coilPartUid), new XAttribute("Name", "operand"))
    //            ));
    //        }
    //    }

    //}

    //public static void orRung(
    //string rawCData,
    //List<XElement> parts,
    //List<XElement> wires,
    //Func<int> NextGlobalUid)
    //{
    //    if (string.IsNullOrWhiteSpace(rawCData)) return;

    //    const string cdataStart = "<![CDATA[";
    //    const string cdataEnd = "]]>";
    //    string text = rawCData.Trim();
    //    if (text.StartsWith(cdataStart, StringComparison.OrdinalIgnoreCase) && text.EndsWith(cdataEnd))
    //        text = text.Substring(cdataStart.Length, text.Length - cdataStart.Length - cdataEnd.Length).Trim();

    //    int openIdx = text.IndexOf('[');
    //    int closeIdx = text.LastIndexOf(']');
    //    if (openIdx < 0 || closeIdx < 0 || closeIdx <= openIdx) return;

    //    string beforeSection = text.Substring(0, openIdx).Trim();
    //    string insideSection = text.Substring(openIdx + 1, closeIdx - openIdx - 1).Trim();
    //    string afterSection = text.Substring(closeIdx + 1).Trim().TrimEnd(';');

    //    if (!insideSection.Contains(",")) return;

    //    var groups = insideSection
    //        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
    //        .Select(g => g.Trim())
    //        .Where(g => !string.IsNullOrWhiteSpace(g))
    //        .ToList();
    //    if (!groups.Any()) return;

    //    var instrPattern = new Regex(@"\b\w+\([^)]*\)");

    //    int EnsureAccessFor(string componentName)
    //    {
    //        if (string.IsNullOrWhiteSpace(componentName)) return -1;
    //        var existing = parts
    //            .Where(x => x.Name.LocalName == "Access")
    //            .FirstOrDefault(a =>
    //            {
    //                var sym = a.Element("Symbol")?.Element("Component");
    //                return sym != null && string.Equals((string?)sym.Attribute("Name"), componentName, StringComparison.OrdinalIgnoreCase);
    //            });

    //        if (existing != null)
    //            return int.Parse(existing.Attribute("UId")!.Value);

    //        int uid = NextGlobalUid();
    //        var access = new XElement("Access",
    //            new XAttribute("Scope", "LocalVariable"),
    //            new XAttribute("UId", uid),
    //            new XElement("Symbol",
    //                new XElement("Component", new XAttribute("Name", componentName))));
    //        parts.Add(access);
    //        return uid;
    //    }

    //    bool groupsContainCoil = groups.Any(g => Regex.IsMatch(g, @"\bOTE\s*\(", RegexOptions.IgnoreCase));

    //    if (groupsContainCoil)
    //    {
    //        int beforeAccessUid = -1;
    //        int beforeContactUid = -1;
    //        bool beforeIsNegated = false;

    //        if (!string.IsNullOrWhiteSpace(beforeSection))
    //        {
    //            var m = instrPattern.Match(beforeSection);
    //            if (m.Success)
    //            {
    //                string beforeInst = m.Value;

    //                var opMatch = Regex.Match(beforeInst, @"\(([^)]*)\)");
    //                if (opMatch.Success)
    //                {
    //                    string beforeOperand = opMatch.Groups[1].Value.Trim();
    //                    beforeAccessUid = EnsureAccessFor(beforeOperand);

    //                    beforeContactUid = NextGlobalUid();
    //                    var beforePart = new XElement("Part", new XAttribute("Name", "Contact"), new XAttribute("UId", beforeContactUid));
    //                    if (beforeInst.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
    //                    {
    //                        beforePart.Add(new XElement("Negated", new XAttribute("Name", "operand")));
    //                        beforeIsNegated = true;
    //                    }
    //                    parts.Add(beforePart);

    //                    wires.Add(new XElement("Wire", new XAttribute("UId", NextGlobalUid()),
    //                        new XElement("IdentCon", new XAttribute("UId", beforeAccessUid)),
    //                        new XElement("NameCon", new XAttribute("UId", beforeContactUid), new XAttribute("Name", "operand"))
    //                    ));
    //                }
    //            }
    //        }

    //        foreach (var grp in groups)
    //        {
    //            var tokens = instrPattern.Matches(grp).Cast<Match>().Select(mm => mm.Value).ToList();
    //            if (!tokens.Any()) continue;

    //            string contactToken = tokens.FirstOrDefault(t => t.StartsWith("XIC", StringComparison.OrdinalIgnoreCase) || t.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
    //                                   ?? tokens.First();

    //            var contactOpMatch = Regex.Match(contactToken, @"\(([^)]*)\)");
    //            if (!contactOpMatch.Success) continue;
    //            string contactOperand = contactOpMatch.Groups[1].Value.Trim();

    //            int contactAccessUid = EnsureAccessFor(contactOperand);
    //            int contactUid = NextGlobalUid();
    //            var contactPart = new XElement("Part", new XAttribute("Name", "Contact"), new XAttribute("UId", contactUid));
    //            if (contactToken.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
    //                contactPart.Add(new XElement("Negated", new XAttribute("Name", "operand")));
    //            parts.Add(contactPart);

    //            wires.Add(new XElement("Wire", new XAttribute("UId", NextGlobalUid()),
    //                new XElement("IdentCon", new XAttribute("UId", contactAccessUid)),
    //                new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "operand"))
    //            ));

    //            var coilMatch = Regex.Match(grp, @"\bOTE\(([^)]*)\)", RegexOptions.IgnoreCase);
    //            if (coilMatch.Success)
    //            {
    //                string coilName = coilMatch.Groups[1].Value.Trim();
    //                int coilAccessUid = EnsureAccessFor(coilName);
    //                int coilPartUid = NextGlobalUid();
    //                parts.Add(new XElement("Part", new XAttribute("Name", "Coil"), new XAttribute("UId", coilPartUid)));

    //                var chainPowerWire = new XElement("Wire", new XAttribute("UId", NextGlobalUid()));
    //                chainPowerWire.Add(new XElement("Powerrail"));

    //                if (beforeContactUid > 0)
    //                {
    //                    chainPowerWire.Add(new XElement("NameCon", new XAttribute("UId", beforeContactUid), new XAttribute("Name", "in")));
    //                    wires.Add(chainPowerWire);

    //                    wires.Add(new XElement("Wire", new XAttribute("UId", NextGlobalUid()),
    //                        new XElement("NameCon", new XAttribute("UId", beforeContactUid), new XAttribute("Name", "out")),
    //                        new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "in"))
    //                    ));
    //                }
    //                else
    //                {
    //                    chainPowerWire.Add(new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "in")));
    //                    wires.Add(chainPowerWire);
    //                }

    //                wires.Add(new XElement("Wire", new XAttribute("UId", NextGlobalUid()),
    //                    new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "out")),
    //                    new XElement("NameCon", new XAttribute("UId", coilPartUid), new XAttribute("Name", "in"))
    //                ));

    //                wires.Add(new XElement("Wire", new XAttribute("UId", NextGlobalUid()),
    //                    new XElement("IdentCon", new XAttribute("UId", coilAccessUid)),
    //                    new XElement("NameCon", new XAttribute("UId", coilPartUid), new XAttribute("Name", "operand"))
    //                ));
    //            }
    //            else
    //            {
    //            }
    //        }

    //        return;
    //    } 
    
    //    var contactUids = new List<int>();
    //    var contactAccessUids = new List<int>();

    //    if (!string.IsNullOrWhiteSpace(beforeSection))
    //    {
    //        var m = instrPattern.Match(beforeSection);
    //        if (m.Success)
    //        {
    //            string inst = m.Value;
    //            var opMatch = Regex.Match(inst, @"\(([^)]*)\)");
    //            if (opMatch.Success)
    //            {
    //                string operand = opMatch.Groups[1].Value.Trim();
    //                int accessUid = EnsureAccessFor(operand);
    //                contactAccessUids.Add(accessUid);

    //                int contactUid = NextGlobalUid();
    //                var contactPart = new XElement("Part", new XAttribute("Name", "Contact"), new XAttribute("UId", contactUid));
    //                if (inst.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
    //                    contactPart.Add(new XElement("Negated", new XAttribute("Name", "operand")));
    //                parts.Add(contactPart);
    //                contactUids.Add(contactUid);

    //                wires.Add(new XElement("Wire", new XAttribute("UId", NextGlobalUid()),
    //                    new XElement("IdentCon", new XAttribute("UId", accessUid)),
    //                    new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "operand"))
    //                ));
    //            }
    //        }
    //    }

    //    foreach (var group in groups)
    //    {
    //        var instMatches = instrPattern.Matches(group).Cast<Match>().Select(m => m.Value).ToList();
    //        if (!instMatches.Any()) continue;

    //        string chosenInst = instMatches.FirstOrDefault(x => x.StartsWith("XIC", StringComparison.OrdinalIgnoreCase)
    //                                                          || x.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
    //                            ?? instMatches.First();

    //        var opMatch = Regex.Match(chosenInst, @"\(([^)]*)\)");
    //        if (!opMatch.Success) continue;
    //        string operand = opMatch.Groups[1].Value.Trim();

    //        int accessUid = EnsureAccessFor(operand);
    //        contactAccessUids.Add(accessUid);

    //        int contactUid = NextGlobalUid();
    //        var contactPart = new XElement("Part", new XAttribute("Name", "Contact"), new XAttribute("UId", contactUid));
    //        if (chosenInst.StartsWith("XIO", StringComparison.OrdinalIgnoreCase))
    //            contactPart.Add(new XElement("Negated", new XAttribute("Name", "operand")));
    //        parts.Add(contactPart);
    //        contactUids.Add(contactUid);

    //        wires.Add(new XElement("Wire", new XAttribute("UId", NextGlobalUid()),
    //            new XElement("IdentCon", new XAttribute("UId", accessUid)),
    //            new XElement("NameCon", new XAttribute("UId", contactUid), new XAttribute("Name", "operand"))
    //        ));
    //    }

    //    var coilMatchAfter = Regex.Match(afterSection, @"\bOTE\(([^)]*)\)", RegexOptions.IgnoreCase);
    //    int coilPartUidAfter = -1;
    //    int coilAccessUidAfter = -1;
    //    if (coilMatchAfter.Success)
    //    {
    //        string coilName = coilMatchAfter.Groups[1].Value.Trim();
    //        coilAccessUidAfter = EnsureAccessFor(coilName);
    //        coilPartUidAfter = NextGlobalUid();
    //        parts.Add(new XElement("Part", new XAttribute("Name", "Coil"), new XAttribute("UId", coilPartUidAfter)));
    //    }

    //    if (contactUids.Count > 0)
    //    {
    //        int orUid = NextGlobalUid();
    //        parts.Add(new XElement("Part", new XAttribute("Name", "O"), new XAttribute("UId", orUid),
    //            new XElement("TemplateValue", new XAttribute("Name", "Card"), new XAttribute("Type", "Cardinality"), contactUids.Count.ToString())
    //        ));

    //        var powerWire = new XElement("Wire", new XAttribute("UId", NextGlobalUid()));
    //        powerWire.Add(new XElement("Powerrail"));
    //        foreach (var c in contactUids)
    //            powerWire.Add(new XElement("NameCon", new XAttribute("UId", c), new XAttribute("Name", "in")));
    //        wires.Add(powerWire);

    //        for (int i = 0; i < contactUids.Count; i++)
    //        {
    //            wires.Add(new XElement("Wire", new XAttribute("UId", NextGlobalUid()),
    //                new XElement("NameCon", new XAttribute("UId", contactUids[i]), new XAttribute("Name", "out")),
    //                new XElement("NameCon", new XAttribute("UId", orUid), new XAttribute("Name", "in" + (i + 1)))
    //            ));
    //        }

    //        if (coilPartUidAfter > 0)
    //        {
    //            wires.Add(new XElement("Wire", new XAttribute("UId", NextGlobalUid()),
    //                new XElement("NameCon", new XAttribute("UId", orUid), new XAttribute("Name", "out")),
    //                new XElement("NameCon", new XAttribute("UId", coilPartUidAfter), new XAttribute("Name", "in"))
    //            ));

    //            wires.Add(new XElement("Wire", new XAttribute("UId", NextGlobalUid()),
    //                new XElement("IdentCon", new XAttribute("UId", coilAccessUidAfter)),
    //                new XElement("NameCon", new XAttribute("UId", coilPartUidAfter), new XAttribute("Name", "operand"))
    //            ));
    //        }
    //    }
    //}
    public static (int openCount, int closeCount) CountBrackets(string text)
    {
        int open = text.Count(c => c == '[');
        int close = text.Count(c => c == ']');
        return (open, close);
    }
    public static int CountOpenBrackets(string text)
    {
        return text.Count(c => c == '[');
    }
    private static XElement MakeCompileUnit(XElement flgNet, string comment, string title)
    {
        return new XElement("SW.Blocks.CompileUnit",
            new XAttribute("ID", NextCompileUnitIdInt().ToString()),
            new XAttribute("CompositionName", "CompileUnits"),
            new XElement("AttributeList",
                new XElement("NetworkSource", flgNet),
                new XElement("ProgrammingLanguage", "LAD")
            ),
            new XElement("ObjectList",
                CreateMultilingualTextBlock(comment ?? string.Empty, "Comment"),
                CreateMultilingualTextBlock(title ?? string.Empty, "Title")
            )
        );
    }

    private static XElement CreateMultilingualTextBlock(string text, string compositionName)
    {
        return new XElement("MultilingualText",
            new XAttribute("ID", NextTextId()),
            new XAttribute("CompositionName", compositionName),
            new XElement("ObjectList",
                new XElement("MultilingualTextItem",
                    new XAttribute("ID", NextTextId()),
                    new XAttribute("CompositionName", "Items"),
                    new XElement("AttributeList",
                        new XElement("Culture", "en-US"),
                        new XElement("Text", text ?? string.Empty)
                    )
                )
            )
        );
    }
    private static void AddWireSafelyGlobal(List<XElement> wires, XElement wire)
    {
        var outNode = wire.Elements().FirstOrDefault(e => e.Attribute("Name")?.Value == "out");
        var inNode = wire.Elements().FirstOrDefault(e => e.Attribute("Name")?.Value == "in");

        if (outNode != null && inNode != null)
        {
            var outUid = outNode.Attribute("UId")?.Value;
            var inUid = inNode.Attribute("UId")?.Value;

            if (!string.IsNullOrEmpty(outUid) && outUid == inUid)
                return;
        }

        int connectionCount = wire.Elements()
            .Count(e => e.Name.LocalName == "NameCon" ||
                        e.Name.LocalName == "IdentCon" ||
                        e.Name.LocalName == "Powerrail");

        if (connectionCount < 2)
            return;

        if (!wires.Any(w => XNode.DeepEquals(w, wire)))
            wires.Add(wire);
    }

    private static void EnsureInPins(List<XElement> wires, List<XElement> parts, HashSet<(int uid, string pinName)> pinMap)
    {
        var partInfos = parts
            .Select(p => new { Uid = (int?)p.Attribute("UId"), Name = (string?)p.Attribute("Name") })
            .Where(x => x.Uid.HasValue)
            .ToList();

        bool powerrailExists = wires.Any(w => w.Elements().Any(e => e.Name.LocalName == "Powerrail"));

        foreach (var part in partInfos)
        {
            string name = part.Name ?? "";
            if (name.Equals("Contact", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("Coil", StringComparison.OrdinalIgnoreCase))
            {
                if (!pinMap.Contains((part.Uid!.Value, "in")))
                {
                    XElement newWire;

                    if (!powerrailExists)
                    {
                        newWire = new XElement("Wire",
                            new XAttribute("UId", NextGlobalUid()),
                            new XElement("Powerrail"),
                            new XElement("NameCon", new XAttribute("UId", part.Uid!.Value), new XAttribute("Name", "in")));
                        powerrailExists = true;
                    }
                    else
                    {
                        var lastPartUid = partInfos
                            .LastOrDefault(p => p.Uid != part.Uid &&
                                (p.Name?.Equals("Contact", StringComparison.OrdinalIgnoreCase) == true ||
                                 p.Name?.Equals("Coil", StringComparison.OrdinalIgnoreCase) == true))
                            ?.Uid ?? part.Uid!.Value;

                        newWire = new XElement("Wire",
                            new XAttribute("UId", NextGlobalUid()),
                            new XElement("NameCon", new XAttribute("UId", lastPartUid), new XAttribute("Name", "out")),
                            new XElement("NameCon", new XAttribute("UId", part.Uid!.Value), new XAttribute("Name", "in")));
                    }

                    if (!wires.Any(w => XNode.DeepEquals(w, newWire)))
                        wires.Add(newWire);

                    pinMap.Add((part.Uid!.Value, "in"));
                }
            }
        }
    }

    private static void ParseRllTextAdvanced(string text, List<(string token, string type)> outTokens,
            ref string? coilName)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        string t = text.Trim();
        if (t.EndsWith(";")) t = t[..^1];

        var mCoil = Regex.Match(t, @"(?<op>OTL|OTU|OTE)\s*\(\s*(?<n>[^)]+)\s*\)", RegexOptions.IgnoreCase);
        if (mCoil.Success) coilName = mCoil.Groups["n"].Value.Trim();

        string inner;
        var mInner = Regex.Match(t, @"\[(?<inner>.*)\]");
        inner = mInner.Success ? mInner.Groups["inner"].Value : t;

        var tokens = SplitTopLevelArgs(string.IsNullOrWhiteSpace(inner) ? t : inner);
        foreach (var tok in tokens)
        {
            string trimmed = tok.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            var mXic = Regex.Match(trimmed, @"XIC\s*\(\s*(?<n>[^)]+)\s*\)", RegexOptions.IgnoreCase);
            var mXio = Regex.Match(trimmed, @"XIO\s*\(\s*(?<n>[^)]+)\s*\)", RegexOptions.IgnoreCase);

            if (mXic.Success)
                outTokens.Add((mXic.Groups["n"].Value.Trim(), "XIC"));
            if (mXio.Success)
                outTokens.Add((mXio.Groups["n"].Value.Trim(), "XIO"));
            else
                outTokens.Add((trimmed, "XIC"));
        }
    }

    private static List<string> SplitTopLevelArgs(string text)
    {
        var result = new List<string>();
        int bracketLevel = 0;
        int start = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '[' || c == '(') bracketLevel++;
            else if (c == ']' || c == ')') bracketLevel--;
            else if (c == ',' && bracketLevel == 0)
            {
                result.Add(text[start..i]);
                start = i + 1;
            }
        }
        if (start < text.Length) result.Add(text[start..]);
        return result;
    }
    private static string? GetInstructionName(XElement rung)
    {
        return rung.Element("Instruction")?.Attribute("Name")?.Value;
    }
}

//    private static string GetInstructionName(XElement rung)
//    {
//        return rung.Element("Text")?.Value;
//    }

//    private static void ParseRllTextAdvanced(string text, List<(string token, string type)> contactOccurrences, ref string coilName)
//    {
//        if (string.IsNullOrWhiteSpace(text)) return;

//        var parts = text.Split(new[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
//        foreach (var p in parts)
//        {
//            var t = p.Trim();
//            if (t.StartsWith("!"))
//                contactOccurrences.Add((t.Substring(1), "XIO"));
//            else if (t.Contains(".") || char.IsLetter(t[0]))
//                contactOccurrences.Add((t, "XIC"));
//        }

//        var idx = text.IndexOf("->");
//        if (idx >= 0)
//        {
//            var after = text.Substring(idx + 2).Trim();
//            var tok = after.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
//            if (!string.IsNullOrEmpty(tok)) coilName = tok;
//        }
//    }
//}

//private static IEnumerable<XElement> GenerateCompileUnits(XDocument rockwellDoc)
//{
//    var compileUnits = new List<XElement>();

//    var rungs = rockwellDoc.Descendants("Rung").ToList();
//    if (!rungs.Any())
//    {
//        rungs = rockwellDoc.Descendants("Instruction")
//            .Select(i => new XElement("Rung", i))
//            .ToList();
//    }

//    foreach (var rung in rungs)
//    {
//        var commentText = (string)rung.Element("Comment") ??
//                          rung.Descendants("Description").FirstOrDefault()?.Value ?? "";
//        var titleText = rung.Element("Text")?.Value?.Trim() ?? GetInstructionName(rung) ?? "";

//        var contactOccurrences = new List<(string token, string type)>();
//        string coilName = null;
//        if (!string.IsNullOrWhiteSpace(rung.Element("Text")?.Value))
//            ParseRllTextAdvanced(rung.Element("Text")?.Value, contactOccurrences, ref coilName);

//        if (!contactOccurrences.Any())
//        {
//            var inputs = rung.Descendants("Input")
//                .Select(x => ((string)x.Attribute("TagName") ?? (string)x.Attribute("Name"), "XIC"))
//                .Where(t => !string.IsNullOrWhiteSpace(t.Item1))
//                .ToList();
//            contactOccurrences.AddRange(inputs);
//        }

//        if (string.IsNullOrWhiteSpace(coilName))
//        {
//            coilName = rung.Descendants("Output")
//                .Select(x => (string)x.Attribute("TagName") ?? (string)x.Attribute("Name"))
//                .FirstOrDefault();
//        }

//        if (!contactOccurrences.Any() && string.IsNullOrWhiteSpace(coilName) && !rung.Descendants("Instruction").Any())
//            continue;

//        var parts = new List<XElement>();
//        var wires = new List<XElement>();
//        var accessMap = new Dictionary<string, int>();
//        var contactUids = new List<int>();
//        var pinMap = new HashSet<(int uid, string pinName)>();
//        var instructions = rung.Descendants("Instruction")
//            .Where(i =>
//            {
//                string name = (string)i.Attribute("Name") ?? "";
//                return name.Equals("MOV", StringComparison.OrdinalIgnoreCase) ||
//                       name.Equals("EQU", StringComparison.OrdinalIgnoreCase) ||
//                       name.Equals("NEQ", StringComparison.OrdinalIgnoreCase) ||
//                       name.Equals("GRT", StringComparison.OrdinalIgnoreCase) ||
//                       name.Equals("LES", StringComparison.OrdinalIgnoreCase) ||
//                       name.Equals("GEQ", StringComparison.OrdinalIgnoreCase) ||
//                       name.Equals("LEQ", StringComparison.OrdinalIgnoreCase);
//            })
//            .ToList();

//        foreach (var instr in instructions)
//        {
//            string instrName = (string)instr.Attribute("Name") ?? "";
//            var parameters = instr.Descendants("Parameter").ToList();
//            if (parameters.Count < 2)
//                continue;

//            string src1 = (string)parameters[0].Attribute("Value") ?? "";
//            string src2 = parameters.Count > 1 ? (string)parameters[1].Attribute("Value") ?? "" : "";
//            string dest = parameters.Count > 2 ? (string)parameters[2].Attribute("Value") ?? "" : "";

//            int src1Uid = int.Parse(NextPartUid());
//            int src2Uid = !string.IsNullOrWhiteSpace(src2) ? int.Parse(NextPartUid()) : -1;
//            int destUid = !string.IsNullOrWhiteSpace(dest) ? int.Parse(NextPartUid()) : -1;

//            if (!string.IsNullOrWhiteSpace(src1))
//            {
//                parts.Add(new XElement(XName.Get("Access"),
//                    new XAttribute("Scope", "LocalVariable"),
//                    new XAttribute("UId", src1Uid),
//                    new XElement(XName.Get("Symbol"),
//                        new XElement(XName.Get("Component"), new XAttribute("Name", src1)))));
//            }

//            if (src2Uid > 0 && !string.IsNullOrWhiteSpace(src2))
//            {
//                parts.Add(new XElement(XName.Get("Access"),
//                    new XAttribute("Scope", "LocalVariable"),
//                    new XAttribute("UId", src2Uid),
//                    new XElement(XName.Get("Symbol"),
//                        new XElement(XName.Get("Component"), new XAttribute("Name", src2)))));
//            }

//            if (destUid > 0 && !string.IsNullOrWhiteSpace(dest))
//            {
//                parts.Add(new XElement(XName.Get("Access"),
//                    new XAttribute("Scope", "LocalVariable"),
//                    new XAttribute("UId", destUid),
//                    new XElement(XName.Get("Symbol"),
//                        new XElement(XName.Get("Component"), new XAttribute("Name", dest)))));
//            }

//            string siemensOp = instrName.ToUpper() switch
//            {
//                "MOV" => "Move",
//                "EQU" => "Eq",
//                "NEQ" => "Ne",
//                "GRT" => "Gt",
//                "LES" => "Lt",
//                "GEQ" => "Ge",
//                "LEQ" => "Le",
//                _ => "Move"
//            };

//            int opUid = int.Parse(NextPartUid());
//            var opPart = new XElement(XName.Get("Part"),
//                new XAttribute("Name", siemensOp),
//                new XAttribute("UId", opUid));

//            if (siemensOp == "Move")
//            {
//                opPart.Add(new XElement(XName.Get("TemplateValue"),
//                    new XAttribute("Name", "Card"),
//                    new XAttribute("Type", "Cardinality"),
//                    "1"));
//            }
//            else
//            {
//                opPart.Add(new XElement(XName.Get("TemplateValue"),
//                    new XAttribute("Name", "SrcType"),
//                    new XAttribute("Type", "Type"),
//                    "Int"));
//            }

//            parts.Add(opPart);

//            AddWireSafely(wires, parts,
//                new XElement(XName.Get("Wire"),
//                    new XAttribute("UId", NextWireUid()),
//                    new XElement(XName.Get("Powerrail")),
//                    new XElement(XName.Get("NameCon"), new XAttribute("UId", opUid), new XAttribute("Name", "pre"))),
//                pinMap);

//            if (siemensOp == "Move")
//            {
//                AddWireSafely(wires, parts,
//                    new XElement(XName.Get("Wire"),
//                        new XAttribute("UId", NextWireUid()),
//                        new XElement(XName.Get("IdentCon"), new XAttribute("UId", src1Uid)),
//                        new XElement(XName.Get("NameCon"), new XAttribute("UId", opUid), new XAttribute("Name", "in"))),
//                    pinMap);

//                if (destUid > 0)
//                {
//                    AddWireSafely(wires, parts,
//                        new XElement(XName.Get("Wire"),
//                            new XAttribute("UId", NextWireUid()),
//                            new XElement(XName.Get("IdentCon"), new XAttribute("UId", destUid)),
//                            new XElement(XName.Get("NameCon"), new XAttribute("UId", opUid), new XAttribute("Name", "out1"))),
//                        pinMap);
//                }
//            }
//            else
//            {
//                AddWireSafely(wires, parts,
//                    new XElement(XName.Get("Wire"),
//                        new XAttribute("UId", NextWireUid()),
//                        new XElement(XName.Get("IdentCon"), new XAttribute("UId", src1Uid)),
//                        new XElement(XName.Get("NameCon"), new XAttribute("UId", opUid), new XAttribute("Name", "in1"))),
//                    pinMap);

//                if (src2Uid > 0)
//                {
//                    AddWireSafely(wires, parts,
//                        new XElement(XName.Get("Wire"),
//                            new XAttribute("UId", NextWireUid()),
//                            new XElement(XName.Get("IdentCon"), new XAttribute("UId", src2Uid)),
//                            new XElement(XName.Get("NameCon"), new XAttribute("UId", opUid), new XAttribute("Name", "in2"))),
//                        pinMap);
//                }

//                if (destUid > 0)
//                {
//                    AddWireSafely(wires, parts,
//                        new XElement(XName.Get("Wire"),
//                            new XAttribute("UId", NextWireUid()),
//                            new XElement(XName.Get("NameCon"), new XAttribute("UId", opUid), new XAttribute("Name", "out")),
//                            new XElement(XName.Get("IdentCon"), new XAttribute("UId", destUid))),
//                        pinMap);
//                }
//            }
//        }

//        foreach (var occ in contactOccurrences)
//        {
//            string accessKey = occ.token + "_" + occ.type;

//            int accessUid = int.Parse(NextPartUid());
//            parts.Add(new XElement(XName.Get("Access"),
//                new XAttribute("Scope", "LocalVariable"),
//                new XAttribute("UId", accessUid),
//                new XElement(XName.Get("Symbol"),
//                new XElement(XName.Get("Component"), new XAttribute("Name", occ.token)))));

//            accessMap[accessKey] = accessUid;

//            int contactUid = int.Parse(NextPartUid());
//            var contactPart = new XElement(XName.Get("Part"),
//                new XAttribute("Name", "Contact"),
//                new XAttribute("UId", contactUid));

//            if (occ.type == "XIO") // inverted contact
//                contactPart.Add(new XElement(XName.Get("Negated"), new XAttribute("Name", "operand")));

//            parts.Add(contactPart);
//            contactUids.Add(contactUid);
//        }

//        int coilUid = int.Parse(NextPartUid());
//        parts.Add(new XElement(XName.Get("Part"), new XAttribute("Name", "Coil"), new XAttribute("UId", coilUid)));

//        int coilAccessUid = -1;
//        if (!string.IsNullOrWhiteSpace(coilName))
//        {
//            coilAccessUid = int.Parse(NextPartUid());
//            parts.Add(new XElement(XName.Get("Access"),
//                new XAttribute("Scope", "LocalVariable"),
//                new XAttribute("UId", coilAccessUid),
//                new XElement(XName.Get("Symbol"),
//                new XElement(XName.Get("Component"), new XAttribute("Name", coilName)))));
//        }

//        for (int i = 0; i < contactOccurrences.Count; i++)
//        {
//            var occ = contactOccurrences[i];
//            string accessKey = occ.token + "_" + occ.type;
//            if (!accessMap.TryGetValue(accessKey, out int accessUid))
//                continue;

//            int contactUid = contactUids[i];
//            if (parts.Any(p => (string)p.Attribute("UId") == accessUid.ToString()) &&
//                parts.Any(p => (string)p.Attribute("UId") == contactUid.ToString()))
//            {
//                AddWireSafely(wires, parts,
//                    new XElement(XName.Get("Wire"),
//                        new XAttribute("UId", NextWireUid()),
//                        new XElement(XName.Get("IdentCon"), new XAttribute("UId", accessUid)),
//                        new XElement(XName.Get("NameCon"), new XAttribute("UId", contactUid),
//                                                     new XAttribute("Name", "operand"))),
//                    pinMap);
//            }
//        }

//        if (contactUids.Any())
//        {
//            AddWireSafely(wires, parts,
//                new XElement(XName.Get("Wire"),
//                    new XAttribute("UId", NextWireUid()),
//                    new XElement(XName.Get("Powerrail")),
//                    new XElement(XName.Get("NameCon"), new XAttribute("UId", contactUids[0]),
//                                                 new XAttribute("Name", "in"))),
//                pinMap);

//            for (int i = 0; i < contactUids.Count - 1; i++)
//            {
//                AddWireSafely(wires, parts,
//                    new XElement(XName.Get("Wire"),
//                        new XAttribute("UId", NextWireUid()),
//                        new XElement(XName.Get("NameCon"), new XAttribute("UId", contactUids[i]),
//                                                     new XAttribute("Name", "out")),
//                        new XElement(XName.Get("NameCon"), new XAttribute("UId", contactUids[i + 1]),
//                                                     new XAttribute("Name", "in"))),
//                    pinMap);
//            }

//            AddWireSafely(wires, parts,
//                new XElement(XName.Get("Wire"),
//                    new XAttribute("UId", NextWireUid()),
//                    new XElement(XName.Get("NameCon"), new XAttribute("UId", contactUids.Last()),
//                                                 new XAttribute("Name", "out")),
//                    new XElement(XName.Get("NameCon"), new XAttribute("UId", coilUid),
//                                                 new XAttribute("Name", "in"))),
//                pinMap);
//        }
//        else
//        {
//            AddWireSafely(wires, parts,
//                new XElement(XName.Get("Wire"),
//                    new XAttribute("UId", NextWireUid()),
//                    new XElement(XName.Get("Powerrail")),
//                    new XElement(XName.Get("NameCon"), new XAttribute("UId", coilUid),
//                                                 new XAttribute("Name", "in"))),
//                pinMap);
//        }

//        if (coilAccessUid > 0 &&
//            parts.Any(p => (string)p.Attribute("UId") == coilAccessUid.ToString()) &&
//            parts.Any(p => (string)p.Attribute("UId") == coilUid.ToString()))
//        {
//            AddWireSafely(wires, parts,
//                new XElement(XName.Get("Wire"),
//                    new XAttribute("UId", NextWireUid()),
//                    new XElement(XName.Get("IdentCon"), new XAttribute("UId", coilAccessUid)),
//                    new XElement(XName.Get("NameCon"), new XAttribute("UId", coilUid),
//                                                 new XAttribute("Name", "operand"))),
//                pinMap);
//        }

//        foreach (var p in parts)
//        {
//            var uidAttr = p.Attribute("UId");
//            if (uidAttr == null) continue;
//            if (!int.TryParse(uidAttr.Value, out int uid)) continue;
//            if (!pinMap.Any(x => x.uid == uid))
//            {
//                AddWireSafely(wires, parts,
//                    new XElement(XName.Get("Wire"),
//                        new XAttribute("UId", NextWireUid()),
//                        new XElement(XName.Get("Powerrail")),
//                        new XElement(XName.Get("NameCon"), new XAttribute("UId", uid),
//                                                     new XAttribute("Name", "in"))),
//                    pinMap);
//            }
//        }

//        var flgNet = new XElement(NsFlgNet + "FlgNet",
//            new XElement(XName.Get("Parts"), parts),
//            new XElement(XName.Get("Wires"), wires));

//        var compileUnit = new XElement("SW.Blocks.CompileUnit",
//            new XAttribute("ID", NextCompileUnitId()),
//            new XAttribute("CompositionName", "CompileUnits"),
//            new XElement("AttributeList",
//                new XElement("NetworkSource", flgNet),
//                new XElement("ProgrammingLanguage", "LAD")),
//            new XElement("ObjectList",
//                new XElement("MultilingualText",
//                    new XAttribute("ID", NextTextId()),
//                    new XAttribute("CompositionName", "Comment"),
//                    new XElement("ObjectList",
//                        new XElement("MultilingualTextItem",
//                            new XAttribute("ID", NextTextId()),
//                            new XAttribute("CompositionName", "Items"),
//                            new XElement("AttributeList",
//                                new XElement("Culture", "en-US"),
//                                new XElement("Text", commentText ?? "")
//                            )
//                        )
//                    )
//                ),
//                new XElement("MultilingualText",
//                    new XAttribute("ID", NextTextId()),
//                    new XAttribute("CompositionName", "Title"),
//                    new XElement("ObjectList",
//                        new XElement("MultilingualTextItem",
//                            new XAttribute("ID", NextTextId()),
//                            new XAttribute("CompositionName", "Items"),
//                            new XElement("AttributeList",
//                                new XElement("Culture", "en-US"),
//                                new XElement("Text", commentText ?? "")
//                            )
//                        )
//                    )
//                )
//            )
//        );

//        compileUnits.Add(compileUnit);
//    }

//    return compileUnits;
//}
//var moveInstructions = rung.Descendants("Instruction")
//    .Where(i => string.Equals((string)i.Attribute("Name"), "MOV", StringComparison.OrdinalIgnoreCase))
//    .ToList();

//foreach (var mov in moveInstructions)
//{
//    var parameters = mov.Descendants("Parameter").ToList();
//    if (parameters.Count >= 2)
//    {
//        string source = (string)parameters[0].Attribute("Value") ?? "";
//        string destination = (string)parameters[1].Attribute("Value") ?? "";

//        int srcUid = int.Parse(NextPartUid());
//        parts.Add(new XElement(XName.Get("Access"),
//            new XAttribute("Scope", "LocalVariable"),
//            new XAttribute("UId", srcUid),
//            new XElement(XName.Get("Symbol"),
//                new XElement(XName.Get("Component"), new XAttribute("Name", source)))));

//        int dstUid = int.Parse(NextPartUid());
//        parts.Add(new XElement(XName.Get("Access"),
//            new XAttribute("Scope", "LocalVariable"),
//            new XAttribute("UId", dstUid),
//            new XElement(XName.Get("Symbol"),
//                new XElement(XName.Get("Component"), new XAttribute("Name", destination)))));

//        int moveUid = int.Parse(NextPartUid());
//        parts.Add(new XElement(XName.Get("Part"),
//            new XAttribute("Name", "Move"),
//            new XAttribute("UId", moveUid),
//            new XElement(XName.Get("TemplateValue"),
//                new XAttribute("Name", "Card"),
//                new XAttribute("Type", "Cardinality"),
//                "1")));

//        AddWireSafely(wires, parts,
//            new XElement(XName.Get("Wire"),
//                new XAttribute("UId", NextWireUid()),
//                new XElement(XName.Get("IdentCon"), new XAttribute("UId", srcUid)),
//                new XElement(XName.Get("NameCon"), new XAttribute("UId", moveUid), new XAttribute("Name", "in"))),
//            pinMap);

//        AddWireSafely(wires, parts,
//            new XElement(XName.Get("Wire"),
//                new XAttribute("UId", NextWireUid()),
//                new XElement(XName.Get("IdentCon"), new XAttribute("UId", dstUid)),
//                new XElement(XName.Get("NameCon"), new XAttribute("UId", moveUid), new XAttribute("Name", "out1"))),
//            pinMap);
//    }
//}

//private static IEnumerable<XElement> GenerateCompileUnits(XDocument rockwellDoc)
//{
//    var compileUnits = new List<XElement>();

//    var rungs = rockwellDoc.Descendants("Rung").ToList();
//    if (!rungs.Any())
//    {
//        rungs = rockwellDoc.Descendants("Instruction")
//            .Select(i => new XElement("Rung", i))
//            .ToList();
//    }

//    foreach (var rung in rungs)
//    {
//        var commentText = (string)rung.Element("Comment") ??
//                          rung.Descendants("Description").FirstOrDefault()?.Value ?? "";
//        var titleText = rung.Element("Text")?.Value?.Trim() ?? GetInstructionName(rung) ?? "";

//        var contactOccurrences = new List<(string token, string type)>();
//        string coilName = null;
//        if (!string.IsNullOrWhiteSpace(rung.Element("Text")?.Value))
//            ParseRllTextAdvanced(rung.Element("Text")?.Value, contactOccurrences, ref coilName);

//        if (!contactOccurrences.Any())
//        {
//            var inputs = rung.Descendants("Input")
//                .Select(x => ((string)x.Attribute("TagName") ?? (string)x.Attribute("Name"), "XIC"))
//                .Where(t => !string.IsNullOrWhiteSpace(t.Item1))
//                .ToList();
//            contactOccurrences.AddRange(inputs);
//        }

//        if (string.IsNullOrWhiteSpace(coilName))
//        {
//            coilName = rung.Descendants("Output")
//                .Select(x => (string)x.Attribute("TagName") ?? (string)x.Attribute("Name"))
//                .FirstOrDefault();
//        }

//        if (!contactOccurrences.Any() && string.IsNullOrWhiteSpace(coilName))
//            continue;

//        var parts = new List<XElement>();
//        var wires = new List<XElement>();
//        var accessMap = new Dictionary<string, int>();
//        var contactUids = new List<int>();
//        var pinMap = new HashSet<(int uid, string pinName)>();

//        foreach (var occ in contactOccurrences)
//        {
//            string accessKey = occ.token + "_" + occ.type;

//            int accessUid = int.Parse(NextPartUid());
//            parts.Add(new XElement(XName.Get("Access"),
//                new XAttribute("Scope", "LocalVariable"),
//                new XAttribute("UId", accessUid),
//                new XElement(XName.Get("Symbol"),
//                new XElement(XName.Get("Component"), new XAttribute("Name", occ.token)))));

//            accessMap[accessKey] = accessUid;

//            int contactUid = int.Parse(NextPartUid());
//            var contactPart = new XElement(XName.Get("Part"),
//                new XAttribute("Name", "Contact"),
//                new XAttribute("UId", contactUid));

//            if (occ.type == "XIO")
//            {
//                contactPart.Add(new XElement(XName.Get("Negated"), new XAttribute("Name", "operand")));
//            }

//            parts.Add(contactPart);
//            contactUids.Add(contactUid);
//        }

//        int coilUid = int.Parse(NextPartUid());
//        parts.Add(new XElement(XName.Get("Part"), new XAttribute("Name", "Coil"), new XAttribute("UId", coilUid)));

//        int coilAccessUid = -1;
//        if (!string.IsNullOrWhiteSpace(coilName))
//        {
//            coilAccessUid = int.Parse(NextPartUid());
//            parts.Add(new XElement(XName.Get("Access"),
//                new XAttribute("Scope", "LocalVariable"),
//                new XAttribute("UId", coilAccessUid),
//                new XElement(XName.Get("Symbol"),
//                new XElement(XName.Get("Component"), new XAttribute("Name", coilName)))));
//        }

//        //for (int i = 0; i < contactOccurrences.Count; i++)
//        //{
//        //    var occ = contactOccurrences[i];
//        //    string accessKey = occ.token + "_" + occ.type;
//        //    if (!accessMap.TryGetValue(accessKey, out int accessUid))
//        //        continue;

//        //    int contactUid = contactUids[i];

//        //    AddWireSafely(wires, parts,
//        //        new XElement(XName.Get("Wire"),
//        //            new XAttribute("UId", NextWireUid()),
//        //            new XElement(XName.Get("IdentCon"), new XAttribute("UId", accessUid)),
//        //            new XElement(XName.Get("NameCon"), new XAttribute("UId", contactUid),
//        //                                         new XAttribute("Name", "operand"))),
//        //        pinMap);
//        //}
//        for (int i = 0; i < contactOccurrences.Count; i++)
//        {
//            var occ = contactOccurrences[i];
//            string accessKey = occ.token + "_" + occ.type;

//            if (!accessMap.TryGetValue(accessKey, out int accessUid))
//                continue;

//            int contactUid = contactUids[i];

//            if (parts.Any(p => (string)p.Attribute("UId") == accessUid.ToString()) &&
//                parts.Any(p => (string)p.Attribute("UId") == contactUid.ToString()))
//            {
//                AddWireSafely(wires, parts,
//                    new XElement(XName.Get("Wire"),
//                        new XAttribute("UId", NextWireUid()),
//                        new XElement(XName.Get("IdentCon"), new XAttribute("UId", accessUid)),
//                        new XElement(XName.Get("NameCon"), new XAttribute("UId", contactUid),
//                                                     new XAttribute("Name", "operand"))),
//                    pinMap);
//            }
//        }

//        if (contactUids.Any())
//        {
//            AddWireSafely(wires, parts,
//                new XElement(XName.Get("Wire"),
//                    new XAttribute("UId", NextWireUid()),
//                    new XElement(XName.Get("Powerrail")),
//                    new XElement(XName.Get("NameCon"), new XAttribute("UId", contactUids[0]),
//                                                 new XAttribute("Name", "in"))),
//                pinMap);

//            for (int i = 0; i < contactUids.Count - 1; i++)
//            {
//                AddWireSafely(wires, parts,
//                    new XElement(XName.Get("Wire"),
//                        new XAttribute("UId", NextWireUid()),
//                        new XElement(XName.Get("NameCon"), new XAttribute("UId", contactUids[i]),
//                                                     new XAttribute("Name", "out")),
//                        new XElement(XName.Get("NameCon"), new XAttribute("UId", contactUids[i + 1]),
//                                                     new XAttribute("Name", "in"))),
//                    pinMap);
//            }

//            AddWireSafely(wires, parts,
//                new XElement(XName.Get("Wire"),
//                    new XAttribute("UId", NextWireUid()),
//                    new XElement(XName.Get("NameCon"), new XAttribute("UId", contactUids.Last()),
//                                                 new XAttribute("Name", "out")),
//                    new XElement(XName.Get("NameCon"), new XAttribute("UId", coilUid),
//                                                 new XAttribute("Name", "in"))),
//                pinMap);
//        }
//        else
//        {
//            AddWireSafely(wires, parts,
//                new XElement(XName.Get("Wire"),
//                    new XAttribute("UId", NextWireUid()),
//                    new XElement(XName.Get("Powerrail")),
//                    new XElement(XName.Get("NameCon"), new XAttribute("UId", coilUid),
//                                                 new XAttribute("Name", "in"))),
//                pinMap);
//        }

//        //if (coilAccessUid > 0)
//        //{
//        //    AddWireSafely(wires, parts,
//        //        new XElement(XName.Get("Wire"),
//        //            new XAttribute("UId", NextWireUid()),
//        //            new XElement(XName.Get("IdentCon"), new XAttribute("UId", coilAccessUid)),
//        //            new XElement(XName.Get("NameCon"), new XAttribute("UId", coilUid),
//        //                                         new XAttribute("Name", "operand"))),
//        //        pinMap);
//        //}

//        if (coilAccessUid > 0 &&
//            parts.Any(p => (string)p.Attribute("UId") == coilAccessUid.ToString()) &&
//            parts.Any(p => (string)p.Attribute("UId") == coilUid.ToString()))
//        {
//            AddWireSafely(wires, parts,
//                new XElement(XName.Get("Wire"),
//                    new XAttribute("UId", NextWireUid()),
//                    new XElement(XName.Get("IdentCon"), new XAttribute("UId", coilAccessUid)),
//                    new XElement(XName.Get("NameCon"), new XAttribute("UId", coilUid),
//                                                 new XAttribute("Name", "operand"))),
//                pinMap);
//        }

//        foreach (var p in parts)
//        {
//            var uidAttr = p.Attribute("UId");
//            if (uidAttr == null) continue;
//            if (!int.TryParse(uidAttr.Value, out int uid)) continue;
//            if (!pinMap.Any(x => x.uid == uid))
//            {
//                AddWireSafely(wires, parts,
//                    new XElement(XName.Get("Wire"),
//                        new XAttribute("UId", NextWireUid()),
//                        new XElement(XName.Get("Powerrail")),
//                        new XElement(XName.Get("NameCon"), new XAttribute("UId", uid),
//                                                     new XAttribute("Name", "in"))),
//                    pinMap);
//            }
//        }

//        var flgNet = new XElement(NsFlgNet + "FlgNet",
//            new XElement(XName.Get("Parts"), parts),
//            new XElement(XName.Get("Wires"), wires));

//        var compileUnit = new XElement("SW.Blocks.CompileUnit",
//            new XAttribute("ID", NextCompileUnitId()),
//            new XAttribute("CompositionName", "CompileUnits"),
//            new XElement("AttributeList",
//                new XElement("NetworkSource", flgNet),
//                new XElement("ProgrammingLanguage", "LAD")),
//            new XElement("ObjectList",
//                new XElement("MultilingualText",
//                    new XAttribute("ID", NextTextId()),
//                    new XAttribute("CompositionName", "Comment"),
//                    new XElement("ObjectList",
//                        new XElement("MultilingualTextItem",
//                            new XAttribute("ID", NextTextId()),
//                            new XAttribute("CompositionName", "Items"),
//                            new XElement("AttributeList",
//                                new XElement("Culture", "en-US"),
//                                new XElement("Text", commentText ?? "")
//                            )
//                        )
//                    )
//                ),
//                new XElement("MultilingualText",
//                    new XAttribute("ID", NextTextId()),
//                    new XAttribute("CompositionName", "Title"),
//                    new XElement("ObjectList",
//                        new XElement("MultilingualTextItem",
//                            new XAttribute("ID", NextTextId()),
//                            new XAttribute("CompositionName", "Items"),
//                            new XElement("AttributeList",
//                                new XElement("Culture", "en-US"),
//                                new XElement("Text", commentText ?? "")
//                            )
//                        )
//                    )
//                )
//            )
//        );

//        compileUnits.Add(compileUnit);
//    }

//    return compileUnits;
//}

//public static class RockwellToSiemensConverter
//{
//    private static readonly XNamespace NsInterface =
//        "http://www.siemens.com/automation/Openness/SW/Interface/v5";
//    private static readonly XNamespace NsFlgNetV4 =
//        "http://www.siemens.com/automation/Openness/SW/NetworkSource/FlgNet/v4";

//    private static int _textIdCounter = 1;
//    private static int _compileUnitIdCounter = 1000;
//    private static int _globalUid = 20;

//    private static string NextTextId() => $"T{_textIdCounter++:X2}";
//    private static string NextCompileUnitId() => $"{_compileUnitIdCounter++:X}";
//    private static string NextPartUid() => (++_globalUid).ToString();
//    private static string NextWireUid() => (++_globalUid).ToString();

//    private static readonly HashSet<string> SiemensReservedNames = new(StringComparer.OrdinalIgnoreCase)
//        {
//            "TON","TOF","TP","AND","OR","NOT","MOVE","ADD","SUB","MUL","DIV",
//            "GEQ","LEQ","EQ","NEQ","GT","LT","SET","RESET","SR","RS"
//        };

//    private static string SanitizeName(string original, HashSet<string> usedNames)
//    {
//        if (string.IsNullOrWhiteSpace(original))
//            return "Var_" + Guid.NewGuid().ToString("N")[..6];

//        string safe = original.Trim();
//        if (SiemensReservedNames.Contains(safe) ||
//            SiemensReservedNames.Any(r => safe.StartsWith(r, StringComparison.OrdinalIgnoreCase)))
//            safe = "_" + safe + "_";

//        int suffix = 1;
//        string baseName = safe;
//        while (usedNames.Contains(safe))
//            safe = $"{baseName}_{suffix++}";

//        usedNames.Add(safe);
//        return safe;
//    }

//    public static void Convert(string rockwellFile, string siemensOutputPath)
//    {
//        _textIdCounter = 1;
//        _compileUnitIdCounter = 1000;
//        _globalUid = 20;

//        var rockwellDoc = XDocument.Load(rockwellFile);

//        var siemensDoc = new XDocument(new XDeclaration("1.0", "utf-8", null),
//            new XElement("Document",
//                new XElement("Engineering", new XAttribute("version", "V17")),
//                new XElement("DocumentInfo",
//                    new XElement("Created", DateTime.UtcNow.ToString("o")),
//                    new XElement("ExportSetting", "None")
//                ),
//                BuildFbElement(rockwellDoc, Path.GetFileNameWithoutExtension(rockwellFile))
//            )
//        );

//        siemensDoc.Save(siemensOutputPath);
//    }

//    private static XElement BuildFbElement(XDocument rockwellDoc, string blockName)
//    {
//        return new XElement("SW.Blocks.FB",
//            new XAttribute("ID", "0"),
//            new XElement("AttributeList",
//                new XElement("Interface",
//                    new XElement(NsInterface + "Sections",
//                        CreateSection("Input", rockwellDoc, "Parameter"),
//                        CreateSection("Output", rockwellDoc, "Parameter"),
//                        CreateSection("InOut", rockwellDoc, "Parameter"),
//                        CreateSection("Static", rockwellDoc, "LocalTag"),
//                        CreateSection("Temp", rockwellDoc, "LocalTag", onlyTemp: true),
//                        CreateSection("Constant", rockwellDoc, "Constant")
//                    )
//                ),
//                new XElement("Name", blockName + "__Converted"),
//                new XElement("Number", "1"),
//                new XElement("ProgrammingLanguage", "LAD")
//            ),
//            new XElement("ObjectList",
//                new XElement("MultilingualText",
//                    new XAttribute("ID", NextTextId()),
//                    new XAttribute("CompositionName", "Comment"),
//                    new XElement("ObjectList",
//                        new XElement("MultilingualTextItem",
//                            new XAttribute("ID", NextTextId()),
//                            new XAttribute("CompositionName", "Items"),
//                            new XElement("AttributeList",
//                                new XElement("Culture", "en-US"),
//                                new XElement("Text", "")
//                            )
//                        )
//                    )
//                ),
//                GenerateCompileUnits(rockwellDoc)
//            )
//        );
//    }

//    private static XElement CreateSection(string sectionName, XDocument rockwellDoc,
//      string tagType, IEnumerable<string> extraTags = null, bool onlyTemp = false)
//    {
//        IEnumerable<XElement> members = Enumerable.Empty<XElement>();

//        if (rockwellDoc != null)
//        {
//            if (onlyTemp && tagType == "LocalTag")
//            {
//                members = rockwellDoc.Descendants("LocalTag")
//                    .Where(x =>
//                    {
//                        var usage = (string)x.Attribute("Usage");
//                        return !string.IsNullOrEmpty(usage) && usage.Equals("Temp", StringComparison.OrdinalIgnoreCase);
//                    });
//            }
//            else if (tagType == "LocalTag")
//                members = rockwellDoc.Descendants("LocalTag");
//            else if (tagType == "Constant")
//                members = rockwellDoc.Descendants("Constant");
//            else
//                members = rockwellDoc.Descendants(tagType)
//                    .Where(p => string.Equals((string)p.Attribute("Usage"), sectionName, StringComparison.OrdinalIgnoreCase));
//        }

//        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
//        if (extraTags != null)
//            foreach (var t in extraTags) usedNames.Add(t);

//        return new XElement(NsInterface + "Section",
//            new XAttribute("Name", sectionName),
//            members.Select(p =>
//            {
//                string rawName = (string)p.Attribute("Name") ?? "";
//                string safeName = SanitizeName(rawName, usedNames);

//                return new XElement(NsInterface + "Member",
//                    new XAttribute("Name", safeName),
//                    new XAttribute("Datatype",
//                        MapDataType((string)p.Attribute("DataType") ??
//                                    (string)p.Attribute("DataTypeName") ?? "BOOL"))
//                );
//            })
//        );
//    }

//    private static string MapDataType(string rockwellType)
//    {
//        if (string.IsNullOrWhiteSpace(rockwellType)) return "Bool";
//        return rockwellType.ToUpper() switch
//        {
//            "BOOL" => "Bool",
//            "INT" => "Int",
//            "DINT" => "DInt",
//            "REAL" => "Real",
//            "STRING" => "String",
//            "WORD" => "Word",
//            "DWORD" => "DWord",
//            _ => "Bool"
//        };
//    }

//    private static IEnumerable<XElement> GenerateCompileUnits(XDocument rockwellDoc)
//    {
//        var compileUnits = new List<XElement>();

//        var rungs = rockwellDoc.Descendants("Rung").ToList();
//        if (!rungs.Any())
//        {
//            rungs = rockwellDoc.Descendants("Instruction")
//                .Select(i => new XElement("Rung", i))
//                .ToList();
//        }

//        foreach (var rung in rungs)
//        {
//            var commentText = (string)rung.Element("Comment") ??
//                              rung.Descendants("Description").FirstOrDefault()?.Value ?? "";
//            var titleText = rung.Element("Text")?.Value?.Trim() ?? GetInstructionName(rung) ?? "";

//            var contactOccurrences = new List<(string token, string type)>(); // type = "XIC" or "XIO"
//            string coilName = null;
//            if (!string.IsNullOrWhiteSpace(rung.Element("Text")?.Value))
//                ParseRllTextAdvanced(rung.Element("Text")?.Value, contactOccurrences, ref coilName);

//            if (!contactOccurrences.Any())
//            {
//                var inputs = rung.Descendants("Input")
//                    .Select(x => ((string)x.Attribute("TagName") ?? (string)x.Attribute("Name"), "XIC"))
//                    .Where(t => !string.IsNullOrWhiteSpace(t.Item1))
//                    .ToList();
//                contactOccurrences.AddRange(inputs);
//            }

//            if (string.IsNullOrWhiteSpace(coilName))
//            {
//                coilName = rung.Descendants("Output")
//                    .Select(x => (string)x.Attribute("TagName") ?? (string)x.Attribute("Name"))
//                    .FirstOrDefault();
//            }

//            if (!contactOccurrences.Any() && string.IsNullOrWhiteSpace(coilName))
//                continue;

//            var parts = new List<XElement>();
//            var wires = new List<XElement>();
//            var accessMap = new Dictionary<string, int>(); // Unique key = token + "_" + type
//            var contactUids = new List<int>();
//            var pinMap = new HashSet<(int uid, string pinName)>();

//            foreach (var occ in contactOccurrences)
//            {
//                string accessKey = occ.token + "_" + occ.type;

//                int accessUid = int.Parse(NextPartUid());
//                parts.Add(new XElement("Access",
//                    new XAttribute("Scope", "LocalVariable"),
//                    new XAttribute("UId", accessUid),
//                    new XElement("Symbol",
//                        new XElement("Component", new XAttribute("Name", occ.token)))));

//                accessMap[accessKey] = accessUid;

//                int contactUid = int.Parse(NextPartUid());
//                var contactPart = new XElement("Part",
//                    new XAttribute("Name", "Contact"),
//                    new XAttribute("UId", contactUid));

//                if (occ.type == "XIO")
//                    //contactPart.Add(new XElement("Inverted", "true"));
//                    contactPart.Add(
//    new XElement("Equation",
//        new XElement("Negated", "true")
//    )
//);
//                parts.Add(contactPart);
//                contactUids.Add(contactUid);
//            }

//            int coilUid = int.Parse(NextPartUid());
//            parts.Add(new XElement("Part", new XAttribute("Name", "Coil"), new XAttribute("UId", coilUid)));

//            int coilAccessUid = -1;
//            if (!string.IsNullOrWhiteSpace(coilName))
//            {
//                coilAccessUid = int.Parse(NextPartUid());
//                parts.Add(new XElement("Access",
//                    new XAttribute("Scope", "LocalVariable"),
//                    new XAttribute("UId", coilAccessUid),
//                    new XElement("Symbol",
//                        new XElement("Component", new XAttribute("Name", coilName)))));
//            }

//            for (int i = 0; i < contactOccurrences.Count; i++)
//            {
//                var occ = contactOccurrences[i];
//                string accessKey = occ.token + "_" + occ.type; // use unique key
//                int accessUid = accessMap[accessKey];
//                int contactUid = contactUids[i];

//                AddWireSafely(wires, parts,
//                    new XElement("Wire",
//                        new XAttribute("UId", NextWireUid()),
//                        new XElement("IdentCon", new XAttribute("UId", accessUid)),
//                        new XElement("NameCon", new XAttribute("UId", contactUid),
//                                                     new XAttribute("Name", "operand"))),
//                    pinMap);
//            }

//            if (contactUids.Any())
//            {
//                AddWireSafely(wires, parts,
//                    new XElement("Wire",
//                        new XAttribute("UId", NextWireUid()),
//                        new XElement("Powerrail"),
//                        new XElement("NameCon", new XAttribute("UId", contactUids[0]),
//                                                     new XAttribute("Name", "in"))),
//                    pinMap);

//                for (int i = 0; i < contactUids.Count - 1; i++)
//                {
//                    AddWireSafely(wires, parts,
//                        new XElement("Wire",
//                            new XAttribute("UId", NextWireUid()),
//                            new XElement("NameCon", new XAttribute("UId", contactUids[i]),
//                                                         new XAttribute("Name", "out")),
//                            new XElement("NameCon", new XAttribute("UId", contactUids[i + 1]),
//                                                         new XAttribute("Name", "in"))),
//                        pinMap);
//                }

//                AddWireSafely(wires, parts,
//                    new XElement("Wire",
//                        new XAttribute("UId", NextWireUid()),
//                        new XElement("NameCon", new XAttribute("UId", contactUids.Last()),
//                                                     new XAttribute("Name", "out")),
//                        new XElement("NameCon", new XAttribute("UId", coilUid),
//                                                     new XAttribute("Name", "in"))),
//                    pinMap);
//            }
//            else
//            {
//                AddWireSafely(wires, parts,
//                    new XElement("Wire",
//                        new XAttribute("UId", NextWireUid()),
//                        new XElement("Powerrail"),
//                        new XElement("NameCon", new XAttribute("UId", coilUid),
//                                                     new XAttribute("Name", "in"))),
//                    pinMap);
//            }

//            if (coilAccessUid > 0)
//            {
//                AddWireSafely(wires, parts,
//                    new XElement("Wire",
//                        new XAttribute("UId", NextWireUid()),
//                        new XElement("IdentCon", new XAttribute("UId", coilAccessUid)),
//                        new XElement("NameCon", new XAttribute("UId", coilUid),
//                                                     new XAttribute("Name", "operand"))),
//                    pinMap);
//            }

//            foreach (var p in parts)
//            {
//                int uid = int.Parse((string)p.Attribute("UId"));
//                if (!pinMap.Any(x => x.uid == uid))
//                {
//                    AddWireSafely(wires, parts,
//                        new XElement("Wire",
//                            new XAttribute("UId", NextWireUid()),
//                            new XElement("Powerrail"),
//                            new XElement("NameCon", new XAttribute("UId", uid),
//                                                         new XAttribute("Name", "in"))),
//                        pinMap);
//                }
//            }

//            var flgNet = new XElement(NsFlgNetV4 + "FlgNet",
//                new XElement(NsFlgNetV4 + "Parts", parts),
//                new XElement(NsFlgNetV4 + "Wires", wires));

//            var compileUnit = new XElement("SW.Blocks.CompileUnit",
//                new XAttribute("ID", NextCompileUnitId()),
//                new XAttribute("CompositionName", "CompileUnits"),
//                new XElement("AttributeList",
//                    new XElement("NetworkSource", flgNet),
//                    new XElement("ProgrammingLanguage", "LAD")),
//                new XElement("ObjectList",
//                    new XElement("MultilingualText",
//                        new XAttribute("ID", NextTextId()),
//                        new XAttribute("CompositionName", "Comment"),
//                        new XElement("ObjectList",
//                            new XElement("MultilingualTextItem",
//                                new XAttribute("ID", NextTextId()),
//                                new XAttribute("CompositionName", "Items"),
//                                new XElement("AttributeList",
//                                    new XElement("Culture", "en-US"),
//                                    new XElement("Text", commentText ?? "")
//                                )
//                            )
//                        )
//                    ),
//                    new XElement("MultilingualText",
//                        new XAttribute("ID", NextTextId()),
//                        new XAttribute("CompositionName", "Title"),
//                        new XElement("ObjectList",
//                            new XElement("MultilingualTextItem",
//                                new XAttribute("ID", NextTextId()),
//                                new XAttribute("CompositionName", "Items"),
//                                new XElement("AttributeList",
//                                    new XElement("Culture", "en-US"),
//                                    new XElement("Text", commentText ?? "")
//                                )
//                            )
//                        )
//                    )
//                )
//            );

//            compileUnits.Add(compileUnit);
//        }

//        return compileUnits;
//    }


//    private static void AddWireSafely(List<XElement> wires, List<XElement> parts, XElement wire,
//        HashSet<(int uid, string pinName)> pinMap)
//    {
//        wires.Add(wire);

//        foreach (var nc in wire.Elements("NameCon"))
//        {
//            int uid = int.Parse((string)nc.Attribute("UId"));
//            string name = (string)nc.Attribute("Name");
//            pinMap.Add((uid, name));
//        }

//        foreach (var ic in wire.Elements("IdentCon"))
//        {
//            int uid = int.Parse((string)ic.Attribute("UId"));
//            pinMap.Add((uid, "ident"));
//        }
//    }

//    private static void ParseRllTextAdvanced(string text, List<(string token, string type)> outTokens,
//        ref string coilName)
//    {
//        if (string.IsNullOrWhiteSpace(text)) return;
//        string t = text.Trim();
//        if (t.EndsWith(";")) t = t[..^1];

//        var mCoil = Regex.Match(t, @"(?<op>OTL|OTU|OTE)\s*\(\s*(?<n>[^)]+)\s*\)",
//            RegexOptions.IgnoreCase);
//        if (mCoil.Success) coilName = mCoil.Groups["n"].Value.Trim();

//        string inner = Regex.Match(t, @"\[(?<inner>.*)\]").Groups["inner"].Value;
//        var tokens = SplitTopLevelArgs(string.IsNullOrWhiteSpace(inner) ? t : inner);

//        foreach (var tok in tokens)
//        {
//            string trimmed = tok.Trim();
//            if (string.IsNullOrWhiteSpace(trimmed)) continue;

//            var mXic = Regex.Match(trimmed, @"XIC\s*\(\s*(?<n>[^)]+)\s*\)", RegexOptions.IgnoreCase);
//            var mXio = Regex.Match(trimmed, @"XIO\s*\(\s*(?<n>[^)]+)\s*\)", RegexOptions.IgnoreCase);

//            if (mXic.Success)
//                outTokens.Add((mXic.Groups["n"].Value.Trim(), "XIC"));
//            else if (mXio.Success)
//                outTokens.Add((mXio.Groups["n"].Value.Trim(), "XIO"));
//            else
//                outTokens.Add((trimmed, "XIC"));
//        }
//    }

//    private static List<string> SplitTopLevelArgs(string text)
//    {
//        var result = new List<string>();
//        int bracketLevel = 0;
//        int start = 0;
//        for (int i = 0; i < text.Length; i++)
//        {
//            if (text[i] == '[' || text[i] == '(') bracketLevel++;
//            else if (text[i] == ']' || text[i] == ')') bracketLevel--;
//            else if (text[i] == ',' && bracketLevel == 0)
//            {
//                result.Add(text[start..i]);
//                start = i + 1;
//            }
//        }

//        if (start < text.Length) result.Add(text[start..]);
//        return result;
//    }

//    private static string GetInstructionName(XElement rung)
//    {
//        return rung.Element("Instruction")?.Attribute("Name")?.Value;
//    }
//}


public static class SiemensToRockwellConverter
{
    public static void Convert(string siemensXmlPath, string rockwellOutputPath)
    {
        var doc = XDocument.Load(siemensXmlPath);
        var sb = new StringBuilder();

        var compileUnits = doc.Descendants("SW.Blocks.CompileUnit").ToList();
        if (!compileUnits.Any())
        {
            compileUnits = doc.Descendants().Where(e => e.Name.LocalName == "CompileUnit" || e.Name.LocalName == "SW.Blocks.CompileUnit").ToList();
        }

        int idx = 0;
        foreach (var cu in compileUnits)
        {
            idx++;
            sb.AppendLine($"-- CompileUnit {(string)cu.Attribute("ID") ?? idx.ToString()} --");

            var flgNet = cu.Descendants().FirstOrDefault(e => e.Name.LocalName == "FlgNet");
            if (flgNet == null)
            {
                sb.AppendLine("(no FlgNet for this compile unit)");
                sb.AppendLine();
                continue;
            }

            var parts = flgNet.Descendants().FirstOrDefault(e => e.Name.LocalName == "Parts")?.Elements().ToList() ?? new List<XElement>();
            var wires = flgNet.Descendants().FirstOrDefault(e => e.Name.LocalName == "Wires")?.Elements().ToList() ?? new List<XElement>();
            var partByUid = parts.Where(p => p.Attribute("UId") != null).ToDictionary(p => (string)p.Attribute("UId"), p => p);

            var coilParts = parts.Where(p => string.Equals((string)p.Attribute("Name"), "Coil", StringComparison.OrdinalIgnoreCase)).ToList();
            if (!coilParts.Any())
            {
                sb.AppendLine("(no Coil found)");
                sb.AppendLine();
                continue;
            }

            foreach (var coil in coilParts)
            {
                var coilUid = (string)coil.Attribute("UId");
                var coilWire = wires.FirstOrDefault(w => w.Elements().Any(e => e.Name.LocalName == "NameCon" && (string)e.Attribute("UId") == coilUid));
                if (coilWire == null)
                {
                    sb.AppendLine("(can't find wire to coil)");
                    continue;
                }

                string sourcePartUid = null;
                foreach (var nc in coilWire.Elements().Where(e => e.Name.LocalName == "NameCon"))
                {
                    var uid = (string)nc.Attribute("UId");
                    if (uid != coilUid) { sourcePartUid = uid; break; }
                }

                var expr = ReconstructExpressionForUid(sourcePartUid, partByUid, wires);
                if (string.IsNullOrWhiteSpace(expr)) expr = "(unknown)";
                var coilAccessUid = wires.Where(w => w.Elements().Any(e => e.Name.LocalName == "NameCon" && (string)e.Attribute("UId") == coilUid && (string)e.Attribute("Name") == "operand"))
                    .SelectMany(w => w.Elements().Where(e => e.Name.LocalName == "IdentCon"))
                    .Select(ic => (string)ic.Attribute("UId")).FirstOrDefault();
                string coilName = null;
                if (coilAccessUid != null && partByUid.ContainsKey(coilAccessUid))
                {
                    var comp = partByUid[coilAccessUid].Descendants().FirstOrDefault(e => e.Name.LocalName == "Component");
                    if (comp != null) coilName = (string)comp.Attribute("Name");
                }
                if (string.IsNullOrWhiteSpace(coilName)) coilName = "COIL_" + coilUid;

                sb.AppendLine($"{expr} -> OTE({coilName});");

                var comment = cu.Descendants().FirstOrDefault(e => e.Attribute("CompositionName")?.Value == "Comment");
                if (comment != null)
                {
                    var text = comment.Descendants("Text").FirstOrDefault()?.Value;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        sb.AppendLine("/* " + text.Trim() + " */");
                    }
                }
            }

            sb.AppendLine();
        }

        System.IO.File.WriteAllText(rockwellOutputPath, sb.ToString());
    }

    private static string ReconstructExpressionForUid(string uid, Dictionary<string, XElement> partByUid, List<XElement> wires)
    {
        if (string.IsNullOrWhiteSpace(uid) || !partByUid.ContainsKey(uid)) return null;
        var part = partByUid[uid];
        var name = (string)part.Attribute("Name") ?? "";

        if (string.Equals(name, "Contact", StringComparison.OrdinalIgnoreCase))
        {
            var wire = wires.FirstOrDefault(w => w.Elements().Any(e => e.Name.LocalName == "NameCon" && (string)e.Attribute("UId") == uid));
            if (wire != null)
            {
                var ident = wire.Elements().FirstOrDefault(e => e.Name.LocalName == "IdentCon");
                if (ident != null)
                {
                    var accessUid = (string)ident.Attribute("UId");
                    if (partByUid.ContainsKey(accessUid))
                    {
                        var comp = partByUid[accessUid].Descendants().FirstOrDefault(e => e.Name.LocalName == "Component");
                        if (comp != null) return "XIC(" + (string)comp.Attribute("Name") + ")";
                    }
                }
            }
            return "XIC(" + uid + ")";
        }
        else if (string.Equals(name, "O", StringComparison.OrdinalIgnoreCase))
        {
            var inWires = wires.Where(w => w.Elements().Any(e => e.Name.LocalName == "NameCon" && (string)e.Attribute("UId") == uid && ((string)e.Attribute("Name"))?.StartsWith("in") == true)).ToList();
            var pieces = new List<string>();
            foreach (var w in inWires)
            {
                var src = w.Elements().FirstOrDefault(e => e.Name.LocalName == "NameCon" && (string)e.Attribute("UId") != uid);
                if (src != null)
                {
                    var srcUid = (string)src.Attribute("UId");
                    pieces.Add(ReconstructExpressionForUid(srcUid, partByUid, wires) ?? "??");
                }
            }
            return "[" + string.Join(",", pieces) + "]";
        }
        else
        {
            var func = name.ToUpperInvariant();
            if (new[] { "EQ", "NE", "GT", "LT", "ADD", "MUL", "SUB", "DIV", "MOVE" }.Contains(func))
            {
                var partUid = (string)part.Attribute("UId");
                var inWires = wires.Where(w => w.Elements().Any(e => e.Name.LocalName == "NameCon" && (string)e.Attribute("UId") == partUid && ((string)e.Attribute("Name"))?.StartsWith("in") == true)).ToList();
                var args = new List<string>();
                foreach (var w in inWires)
                {
                    var ident = w.Elements().FirstOrDefault(e => e.Name.LocalName == "IdentCon");
                    if (ident != null)
                    {
                        var aUid = (string)ident.Attribute("UId");
                        if (partByUid.ContainsKey(aUid))
                        {
                            var comp = partByUid[aUid].Descendants().FirstOrDefault(e => e.Name.LocalName == "Component");
                            if (comp != null) args.Add((string)comp.Attribute("Name"));
                            else args.Add("A" + aUid);
                        }
                        else args.Add("A" + aUid);
                    }
                    else
                    {
                        var src = w.Elements().FirstOrDefault(e => e.Name.LocalName == "NameCon" && (string)e.Attribute("UId") != partUid);
                        if (src != null)
                        {
                            args.Add(ReconstructExpressionForUid((string)src.Attribute("UId"), partByUid, wires) ?? "??");
                        }
                    }
                }
                return func + "(" + string.Join(",", args) + ")";
            }
        }

        return name + "(" + (string)part.Attribute("UId") + ")";
    }
}