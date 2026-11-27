////////#nullable enable
////////using System;
////////using System.Collections.Generic;
////////using System.IO;
////////using System.Linq;
////////using System.Text;
////////using System.Xml.Linq;

////////// ======================================================================= layout tree
////////abstract class N { }
////////abstract class Group : N { public List<N> Items = new(); }

////////sealed class Seq : Group
////////{
////////    public Seq(List<N> items) { Items = items; }
////////    // a block with an instance name needs one empty row above it (instance label)
////////    public int Pad => Items[Items.Count - 1] is Leaf l && l.Kind == "Block" && l.Instance.Length > 0 ? 1 : 0;
////////}
////////sealed class Par : Group { public Par(List<N> items) { Items = items; } }    // OR  (merging)
////////sealed class Fork : Group { public Fork(List<N> items) { Items = items; } }  // parallel outputs

////////sealed class Pin { public string Role = "", As = "", Si = ""; public int Row; public bool En; }

////////sealed class Leaf : N
////////{
////////    public string Kind = "";   // Contact | Coil | Block | DataSource | DataSink
////////    public string Operand = "";
////////    public bool Neg;
////////    public string Edge = "none", Latch = "none";
////////    public string Type = "", Instance = "";
////////    public List<Pin> Ins = new(), Outs = new();                 // block pins (emission order)
////////    public List<Leaf?> InDs = new();                            // parallel to Ins: data source leaf
////////    public List<List<Leaf>> OutSinks = new();                   // parallel to Outs: data sink leaves
////////    public List<(int pin, N expr)> Extra = new();               // extra power-flow inputs (TONR "R")
////////    public List<List<Leaf>> ExtraSrc = new();                   // parallel to Extra: leaves feeding it
////////    public int W, X, Row, DsW, SinkW, FlowInRow, FlowOutIdx, FlowOutRow, SinkPin;
////////    public int[] OutIds = new int[1];
////////    public List<Leaf> Src = new();
////////    public Leaf? SinkOwner;
////////    public bool Terminal;

////////    public int Y => Kind == "Block" ? 38 * Row : 38 * Row + 18;
////////    public int Rows => Kind == "Block" ? Math.Max(Ins.Max(p => p.Row), Outs.Max(p => p.Row)) + 1 : 1;
////////}

////////sealed class Ep { public string Kind = "", Uid = "", Pin = ""; }
////////sealed class PartInfo
////////{
////////    public string Uid = "", Name = "", Instance = "";
////////    public HashSet<string> Neg = new();
////////    public int Card = 2;
////////    public Leaf? Leaf;
////////}

////////// ======================================================================= one network -> one <Rung>
////////sealed class RungBuilder
////////{
////////    readonly Dictionary<string, string> access = new();
////////    readonly Dictionary<string, PartInfo> parts = new();
////////    readonly List<PartInfo> order = new();
////////    readonly Dictionary<string, Ep> driver = new();
////////    readonly Dictionary<string, List<string>> sinkMap = new();   // "uid|outPin" -> variables written
////////    readonly HashSet<string> used = new();                        // output pins that feed another part
////////    readonly Dictionary<string, N?> memo = new();

////////    static readonly HashSet<string> OutPinNames = new() { "out", "eno", "out1", "out2", "out3", "Q", "ET", "QU", "QD", "CV" };
////////    static readonly HashSet<string> Arith = new() { "Add", "Sub", "Mul", "Div", "Mod" };
////////    static readonly HashSet<string> Cmp = new() { "Eq", "Ne", "Lt", "Le", "Gt", "Ge" };
////////    static readonly HashSet<string> Timers = new() { "TON", "TOF", "TP", "TONR" };

////////    static bool IsBlock(string n) => n == "Move" || n == "Not" || Arith.Contains(n) || Cmp.Contains(n) || Timers.Contains(n);
////////    static string? FlowOutSi(string n) =>
////////        n == "Move" || Arith.Contains(n) ? "eno" :
////////        Cmp.Contains(n) || n == "Not" ? "out" :
////////        Timers.Contains(n) ? "Q" : null;

////////    static IEnumerable<XElement> E(XElement p, string n) => p.Elements().Where(x => x.Name.LocalName == n);
////////    static XElement? F(XElement p, string n) => E(p, n).FirstOrDefault();
////////    static string Names(XElement p) => string.Join(".", E(p, "Component").Select(c => (string)c.Attribute("Name")!));

////////    // width model measured from your LD file (proportional font, 30 px steps)
////////    public static int TextWidth(string s, int min)
////////    {
////////        double u = 0;
////////        foreach (char c in s) u += "iltrfjI.".IndexOf(c) >= 0 ? 0.5 : 1.0;
////////        int w = 60 + 30 * (int)Math.Ceiling(Math.Max(0.0, u - 6.75) / 5.0);
////////        return Math.Max(w, min);
////////    }

////////    // ------------------------------------------------------------------ parsing
////////    public RungBuilder(XElement flg)
////////    {
////////        foreach (var el in F(flg, "Parts")!.Elements())
////////        {
////////            string uid = (string)el.Attribute("UId")!;
////////            if (el.Name.LocalName == "Access") access[uid] = AccessText(el);
////////            else if (el.Name.LocalName == "Part")
////////            {
////////                var p = new PartInfo { Uid = uid, Name = (string)el.Attribute("Name")! };
////////                foreach (var ng in E(el, "Negated")) p.Neg.Add((string)ng.Attribute("Name")!);
////////                var inst = F(el, "Instance");
////////                if (inst != null) p.Instance = Names(inst);
////////                var card = E(el, "TemplateValue").FirstOrDefault(t => (string?)t.Attribute("Name") == "Card");
////////                if (card != null) p.Card = int.Parse(card.Value);
////////                parts[uid] = p; order.Add(p);
////////            }
////////        }
////////        foreach (var w in E(F(flg, "Wires")!, "Wire"))
////////        {
////////            var eps = new List<Ep>();
////////            foreach (var c in w.Elements())
////////                switch (c.Name.LocalName)
////////                {
////////                    case "Powerrail": eps.Add(new Ep { Kind = "Rail" }); break;
////////                    case "IdentCon": eps.Add(new Ep { Kind = "Ident", Uid = (string)c.Attribute("UId")! }); break;
////////                    case "NameCon": eps.Add(new Ep { Kind = "Name", Uid = (string)c.Attribute("UId")!, Pin = (string)c.Attribute("Name")! }); break;
////////                    case "OpenCon": eps.Add(new Ep { Kind = "Open" }); break;
////////                }
////////            Ep? src = eps.FirstOrDefault(e => e.Kind == "Rail")
////////                   ?? eps.FirstOrDefault(e => e.Kind == "Name" && OutPinNames.Contains(e.Pin))
////////                   ?? eps.FirstOrDefault(e => e.Kind == "Ident");
////////            if (src == null) continue;
////////            bool nameDest = false;
////////            foreach (var e in eps)
////////            {
////////                if (ReferenceEquals(e, src)) continue;
////////                if (e.Kind == "Name") { driver[e.Uid + "|" + e.Pin] = src; nameDest = true; }
////////                else if (e.Kind == "Ident" && src.Kind == "Name" && access.TryGetValue(e.Uid, out var txt))
////////                {
////////                    string key = src.Uid + "|" + src.Pin;
////////                    if (!sinkMap.TryGetValue(key, out var lst)) sinkMap[key] = lst = new List<string>();
////////                    lst.Add(txt);
////////                }
////////            }
////////            if (src.Kind == "Name" && nameDest) used.Add(src.Uid + "|" + src.Pin);
////////        }
////////    }

////////    static string SymText(XElement sym)
////////    {
////////        var l = new List<string>();
////////        foreach (var c in E(sym, "Component"))
////////        {
////////            string s = (string)c.Attribute("Name")!;
////////            if ((string?)c.Attribute("AccessModifier") == "Array")
////////                s += "[" + string.Join(",", E(c, "Access").Select(AccessText)) + "]";
////////            l.Add(s);
////////        }
////////        return string.Join(".", l);
////////    }

////////    static string AccessText(XElement a)
////////    {
////////        var sym = F(a, "Symbol");
////////        if (sym != null) return SymText(sym);
////////        var c = F(a, "Constant");
////////        if (c == null) return "";
////////        string t = F(c, "ConstantType")?.Value ?? "";
////////        string v = F(c, "ConstantValue")?.Value ?? "";
////////        if (t.Equals("Bool", StringComparison.OrdinalIgnoreCase)) return v.Equals("true", StringComparison.OrdinalIgnoreCase) || v == "1" ? "TRUE" : "FALSE";
////////        if (v.StartsWith("T#", StringComparison.OrdinalIgnoreCase)) return "T#" + v.Substring(2).ToLowerInvariant();
////////        return v;
////////    }

////////    string Operand(PartInfo p, string pin)
////////    {
////////        if (!driver.TryGetValue(p.Uid + "|" + pin, out var s))
////////            throw new InvalidOperationException($"{p.Name} (UId {p.Uid}): pin '{pin}' is not wired");
////////        if (s.Kind != "Ident" || !access.TryGetValue(s.Uid, out var t))
////////            throw new NotSupportedException($"{p.Name} (UId {p.Uid}): pin '{pin}' is driven by another block; only variables/constants are supported");
////////        return t;
////////    }
////////    string OperandAny(PartInfo p, params string[] pins)
////////    {
////////        foreach (var pin in pins) if (driver.ContainsKey(p.Uid + "|" + pin)) return Operand(p, pin);
////////        throw new InvalidOperationException($"{p.Name} (UId {p.Uid}): none of the pins {string.Join("/", pins)} is wired");
////////    }

////////    // ------------------------------------------------------------------ leaves
////////    static Pin Mk(string role, string asn, string si, int row, bool en = false) => new Pin { Role = role, As = asn, Si = si, Row = row, En = en };

////////    Leaf MakeLeaf(PartInfo p)
////////    {
////////        if (p.Leaf != null) return p.Leaf;
////////        Leaf l;
////////        switch (p.Name)
////////        {
////////            case "Contact": l = new Leaf { Kind = "Contact", Operand = Operand(p, "operand"), Neg = p.Neg.Contains("operand") }; break;
////////            case "PBox": l = new Leaf { Kind = "Contact", Operand = Operand(p, "bit"), Edge = "rising" }; break;
////////            case "NBox": l = new Leaf { Kind = "Contact", Operand = Operand(p, "bit"), Edge = "falling" }; break;
////////            case "Coil":
////////            case "SCoil":
////////            case "RCoil":
////////                l = new Leaf
////////                {
////////                    Kind = "Coil",
////////                    Operand = Operand(p, "operand"),
////////                    Neg = p.Neg.Contains("operand"),
////////                    Terminal = true,
////////                    Latch = p.Name == "SCoil" ? "set" : p.Name == "RCoil" ? "reset" : "none"
////////                };
////////                break;
////////            default:
////////                if (!IsBlock(p.Name)) throw new NotSupportedException($"LAD part '{p.Name}' (UId {p.Uid}) is not supported");
////////                l = MakeBlock(p);
////////                break;
////////        }
////////        if (l.Kind == "Contact" || l.Kind == "Coil") l.W = TextWidth(l.Operand, 90);
////////        p.Leaf = l;
////////        return l;
////////    }

////////    Leaf MakeBlock(PartInfo p)
////////    {
////////        var ins = new List<Pin>(); var outs = new List<Pin>();
////////        string asType = p.Name.ToUpperInvariant(); int w = 120;

////////        if (p.Name == "Move")
////////        {
////////            ins.Add(Mk("DataIn", "IN1", "in", 1)); ins.Add(Mk("FlowIn", "EN", "en", 0, true));
////////            outs.Add(Mk("DataOut", "OUT1", "out1", 1)); outs.Add(Mk("FlowOut", "ENO", "eno", 0, true));
////////        }
////////        else if (Arith.Contains(p.Name))
////////        {
////////            ins.Add(Mk("FlowIn", "EN", "en", 0, true));
////////            for (int i = 1; i <= Math.Max(2, p.Card); i++) ins.Add(Mk("DataIn", "IN" + i, "in" + i, i));
////////            outs.Add(Mk("FlowOut", "ENO", "eno", 0, true)); outs.Add(Mk("DataOut", "OUT", "out", 1));
////////        }
////////        else if (Cmp.Contains(p.Name))
////////        {
////////            ins.Add(Mk("FlowIn", "EN", "pre", 0, true)); ins.Add(Mk("DataIn", "IN1", "in1", 1)); ins.Add(Mk("DataIn", "IN2", "in2", 2));
////////            outs.Add(Mk("Aux", "ENO", "", 0, true)); outs.Add(Mk("FlowOut", "OUT", "out", 1));   // OUT carries the power flow
////////        }
////////        else if (p.Name == "Not")
////////        {
////////            w = 90;
////////            ins.Add(Mk("FlowIn", "IN", "in", 0)); outs.Add(Mk("FlowOut", "OUT", "out", 0));
////////        }
////////        else // timers
////////        {
////////            w = TextWidth(p.Instance, 120);
////////            ins.Add(Mk("FlowIn", "IN", "IN", 0));
////////            if (p.Name == "TONR") ins.Add(Mk("FlowIn", "R", "R", 1));
////////            ins.Add(Mk("DataIn", "PT", "PT", ins.Count));
////////            outs.Add(Mk("FlowOut", "Q", "Q", 0)); outs.Add(Mk("DataOut", "ET", "ET", 1));
////////        }

////////        var l = new Leaf { Kind = "Block", Type = asType, Instance = p.Instance, W = w, Ins = ins, Outs = outs };
////////        l.OutIds = new int[outs.Count];
////////        l.FlowOutIdx = outs.FindIndex(o => o.Role == "FlowOut");
////////        l.FlowOutRow = l.FlowOutIdx >= 0 ? outs[l.FlowOutIdx].Row : 0;
////////        l.FlowInRow = ins.First(i => i.Role == "FlowIn").Row;

////////        foreach (var pin in ins)
////////        {
////////            if (pin.Role == "DataIn")
////////            {
////////                string t = pin.Si == "in" ? OperandAny(p, "in", "in1") : Operand(p, pin.Si);
////////                l.InDs.Add(new Leaf { Kind = "DataSource", Operand = t, W = TextWidth(t, 60) });
////////            }
////////            else l.InDs.Add(null);
////////        }
////////        for (int i = 0; i < outs.Count; i++)
////////        {
////////            var lst = new List<Leaf>();
////////            if (outs[i].Role == "DataOut" && sinkMap.TryGetValue(p.Uid + "|" + outs[i].Si, out var names))
////////                foreach (var n in names) lst.Add(new Leaf { Kind = "DataSink", Operand = n, W = TextWidth(n, 60), SinkOwner = l, SinkPin = i });
////////            l.OutSinks.Add(lst);
////////        }
////////        l.DsW = l.InDs.Where(d => d != null).Select(d => d!.W).DefaultIfEmpty(0).Max();
////////        l.SinkW = l.OutSinks.SelectMany(s => s).Select(s => s.W).DefaultIfEmpty(0).Max();
////////        string? fo = FlowOutSi(p.Name);
////////        l.Terminal = fo == null || !used.Contains(p.Uid + "|" + fo);
////////        return l;
////////    }

////////    static Leaf MakeTrue() => new Leaf { Kind = "Contact", Operand = "TRUE", W = 90 };

////////    // ------------------------------------------------------------------ expression tree
////////    static List<N> AsList(N? n) => n == null ? new List<N>() : n is Seq s ? new List<N>(s.Items) : new List<N> { n };
////////    static N? Make(List<N> items) => items.Count == 0 ? null : items.Count == 1 ? items[0] : new Seq(items);
////////    static N Single(List<N> l) => l.Count == 1 ? l[0] : new Seq(l);
////////    static N? Cat(N? a, N? b) { var l = AsList(a); l.AddRange(AsList(b)); return Make(l); }

////////    N? ExprIn(PartInfo p, string pin)
////////    {
////////        if (!driver.TryGetValue(p.Uid + "|" + pin, out var src))
////////        {
////////            if (pin == "en" || pin == "pre") return null;       // unwired EN = always enabled
////////            throw new InvalidOperationException($"{p.Name} (UId {p.Uid}): input '{pin}' is not wired");
////////        }
////////        if (src.Kind == "Rail") return null;
////////        if (src.Kind != "Name") throw new NotSupportedException($"{p.Name} (UId {p.Uid}): input '{pin}' is driven by a variable, not by logic");
////////        var sp = parts[src.Uid];
////////        if (IsBlock(sp.Name) && src.Pin != FlowOutSi(sp.Name))
////////            throw new NotSupportedException($"{sp.Name} (UId {sp.Uid}): output '{src.Pin}' is used as a signal; only '{FlowOutSi(sp.Name)}' is supported");
////////        return ExprOut(sp);
////////    }

////////    N? ExprOut(PartInfo p)
////////    {
////////        if (memo.TryGetValue(p.Uid, out var m)) return m;
////////        N? r;
////////        switch (p.Name)
////////        {
////////            case "Contact":
////////            case "PBox":
////////            case "NBox":
////////                r = Cat(ExprIn(p, "in"), MakeLeaf(p));
////////                break;
////////            case "O":
////////                {
////////                    var br = new List<N?>();
////////                    for (int i = 1; i <= p.Card; i++) br.Add(ExprIn(p, "in" + i));
////////                    r = MakePar(br);
////////                    break;
////////                }
////////            case "A":
////////                {
////////                    N? acc = null;
////////                    for (int i = 1; i <= p.Card; i++) acc = Cat(acc, ExprIn(p, "in" + i));
////////                    r = acc;
////////                    break;
////////                }
////////            default:
////////                if (IsBlock(p.Name))
////////                {
////////                    var leaf = MakeLeaf(p);
////////                    if (leaf.Ins.Count(i => i.Role == "FlowIn") > 1)
////////                        throw new NotSupportedException($"{p.Name} (UId {p.Uid}): a block with two power-flow inputs must be the last element of a rung");
////////                    r = Cat(ExprIn(p, leaf.Ins.First(i => i.Role == "FlowIn").Si), leaf);
////////                    break;
////////                }
////////                throw new NotSupportedException($"LAD part '{p.Name}' (UId {p.Uid}) is not supported");
////////        }
////////        memo[p.Uid] = r;
////////        return r;
////////    }

////////    static N? MakePar(List<N?> branches)
////////    {
////////        if (branches.Any(b => b == null)) return null;             // OR with the rail = always true
////////        var lists = branches.Select(AsList).ToList();
////////        var prefix = new List<N>();
////////        while (lists.All(l => l.Count > 0) && lists.All(l => ReferenceEquals(l[0], lists[0][0])))
////////        {
////////            prefix.Add(lists[0][0]);
////////            lists = lists.Select(l => l.Skip(1).ToList()).ToList();
////////        }
////////        if (lists.Any(l => l.Count == 0)) return Make(prefix);
////////        N core = lists.Count == 1 ? Single(lists[0]) : new Par(lists.Select(Single).ToList());
////////        prefix.AddRange(AsList(core));
////////        return Make(prefix);
////////    }

////////    bool IsTermPart(PartInfo p) =>
////////        p.Name == "Coil" || p.Name == "SCoil" || p.Name == "RCoil" || (IsBlock(p.Name) && MakeLeaf(p).Terminal);

////////    N BuildRoot()
////////    {
////////        var chains = new List<List<N>>();
////////        foreach (var t in order.Where(p => IsTermPart(p)).ToList())
////////        {
////////            var leaf = MakeLeaf(t);
////////            string inPin = leaf.Kind == "Coil" ? "in" : leaf.Ins.First(i => i.Role == "FlowIn").Si;
////////            if (leaf.Kind == "Block" && leaf.Extra.Count == 0)
////////                for (int i = 0; i < leaf.Ins.Count; i++)
////////                    if (leaf.Ins[i].Role == "FlowIn" && leaf.Ins[i].Si != inPin)
////////                        leaf.Extra.Add((i, ExprIn(t, leaf.Ins[i].Si) ?? (N)MakeTrue()));
////////            chains.Add(AsList(Cat(ExprIn(t, inPin), leaf)));
////////        }
////////        if (chains.Count == 0) throw new InvalidOperationException("network has no coil / output block");
////////        if (chains.Count == 1) return new Seq(chains[0]);

////////        var trunk = new List<N>();
////////        while (chains.All(c => c.Count > 1 && ReferenceEquals(c[0], chains[0][0])))
////////        {
////////            trunk.Add(chains[0][0]);
////////            chains = chains.Select(c => c.Skip(1).ToList()).ToList();
////////        }
////////        if (trunk.Count == 0) trunk.Add(MakeTrue());                // several outputs straight from the rail
////////        trunk.Add(new Fork(chains.Select(c => (N)new Seq(c)).ToList()));
////////        return new Seq(trunk);
////////    }

////////    // ------------------------------------------------------------------ geometry
////////    static int Shift(N n) => n is Leaf l ? (l.Kind == "Block" && l.FlowOutIdx >= 0 ? l.FlowOutRow : 0)
////////                           : n is Seq s ? s.Items.Sum(x => Shift(x)) : 0;

////////    static int H(N n)
////////    {
////////        switch (n)
////////        {
////////            case Leaf l: return l.Rows;
////////            case Seq s:
////////                {
////////                    int r = 0, h = 0;
////////                    foreach (var it in s.Items) { h = Math.Max(h, r + H(it)); r += Shift(it); }
////////                    return s.Pad + h;
////////                }
////////            case Group g: return g.Items.Sum(x => H(x));
////////            default: return 1;
////////        }
////////    }
////////    static int Wd(N n)
////////    {
////////        switch (n)
////////        {
////////            case Leaf l: return l.Terminal ? 0 : l.Kind == "Block" ? l.DsW + l.W + l.SinkW : l.W;
////////            case Seq s: return s.Items.Sum(x => Wd(x));
////////            case Group g: return g.Items.Max(x => Wd(x));
////////            default: return 0;
////////        }
////////    }
////////    static void Place(N n, int x, int row)
////////    {
////////        switch (n)
////////        {
////////            case Leaf l: l.X = x + (l.Kind == "Block" ? l.DsW : 0); l.Row = row; break;
////////            case Seq s:
////////                {
////////                    int r = row + s.Pad, cx = x;
////////                    foreach (var it in s.Items) { Place(it, cx, r); cx += Wd(it); r += Shift(it); }
////////                    break;
////////                }
////////            case Group g:
////////                {
////////                    int r = row;
////////                    foreach (var it in g.Items) { Place(it, x, r); r += H(it); }
////////                    break;
////////                }
////////        }
////////    }
////////    static IEnumerable<Leaf> Leaves(N n) => n is Leaf l ? new[] { l } : ((Group)n).Items.SelectMany(Leaves);
////////    static List<Leaf> Entries(N n) => n switch
////////    {
////////        Leaf l => new List<Leaf> { l },
////////        Seq s => Entries(s.Items[0]),
////////        Group g => g.Items.SelectMany(Entries).ToList(),
////////        _ => new List<Leaf>()
////////    };
////////    static List<Leaf> Exits(N n) => n switch
////////    {
////////        Leaf l => new List<Leaf> { l },
////////        Seq s => Exits(s.Items[s.Items.Count - 1]),
////////        Group g => g.Items.SelectMany(Exits).ToList(),
////////        _ => new List<Leaf>()
////////    };
////////    static void Link(N n)
////////    {
////////        if (n is Leaf) return;
////////        var g = (Group)n;
////////        foreach (var c in g.Items) Link(c);
////////        if (n is Seq s)
////////            for (int i = 1; i < s.Items.Count; i++)
////////            {
////////                var ex = Exits(s.Items[i - 1]);
////////                foreach (var e in Entries(s.Items[i])) e.Src.AddRange(ex);
////////            }
////////    }
////////    static int Right(Leaf l) => l.X + l.W + (l.Kind == "Block" ? l.SinkW : 0);
////////    static int Foot(Leaf l) => l.Kind == "Block" ? l.DsW + l.W + (l.SinkW > 0 ? l.SinkW : 30) : l.W;
////////    static IEnumerable<int> FlowIds(IEnumerable<Leaf> src) => src.Select(s => s.OutIds[s.FlowOutIdx]).Distinct().OrderBy(i => i);

////////    // ------------------------------------------------------------------ output
////////    static void Line(StringBuilder sb, int ind, string s) => sb.Append(' ', ind).Append(s).Append("\r\n");
////////    static string Esc(string s) => System.Security.SecurityElement.Escape(s) ?? "";

////////    public static string EmptyRung(int evalOrder, string label, string doc)
////////    {
////////        var sb = new StringBuilder();
////////        Line(sb, 12, $"<Rung evaluationOrder=\"{evalOrder}\" label=\"{Esc(label)}\">");
////////        Line(sb, 14, $"<Documentation xsi:type=\"SimpleText\"><![CDATA[{doc.Replace("]]>", "]] >")}]]></Documentation>");
////////        Line(sb, 14, "<LdObject xsi:type=\"LeftPowerRail\">");
////////        Line(sb, 16, "<RelPosition x=\"0\" y=\"0\" />");
////////        Line(sb, 16, "<Size x=\"3\" y=\"38\" />");
////////        Line(sb, 16, "<ConnectionPointOut connectionPointOutId=\"0\">");
////////        Line(sb, 18, "<RelPosition x=\"3\" y=\"28\" />");
////////        Line(sb, 16, "</ConnectionPointOut>");
////////        Line(sb, 14, "</LdObject>");
////////        Line(sb, 14, "<LdObject xsi:type=\"RightPowerRail\">");
////////        Line(sb, 16, "<RelPosition x=\"1053\" y=\"0\" />");
////////        Line(sb, 16, "<Size x=\"3\" y=\"38\" />");
////////        Line(sb, 16, "<ConnectionPointIn>");
////////        Line(sb, 18, "<RelPosition x=\"0\" y=\"28\" />");
////////        Line(sb, 16, "</ConnectionPointIn>");
////////        Line(sb, 14, "</LdObject>");
////////        Line(sb, 12, "</Rung>");
////////        return sb.ToString();
////////    }

////////    public string Emit(int evalOrder, string label, string doc)
////////    {
////////        var root = BuildRoot();
////////        var main = Leaves(root).ToList();
////////        var terms = main.Where(l => l.Terminal).ToList();
////////        var termBlocks = terms.Where(l => l.Kind == "Block").ToList();
////////        var extras = new List<N>();
////////        foreach (var t in termBlocks) foreach (var e in t.Extra) extras.Add(e.expr);
////////        var leaves = new List<Leaf>(main);
////////        foreach (var e in extras) leaves.AddRange(Leaves(e));

////////        Place(root, 3, 0);
////////        int nextRow = H(root);
////////        foreach (var e in extras) { Place(e, 3, nextRow); nextRow += H(e); }

////////        int maxLeft = leaves.Where(l => !l.Terminal).Select(Right).DefaultIfEmpty(3).Max();
////////        int maxFoot = terms.Select(Foot).DefaultIfEmpty(0).Max();
////////        int railX = Math.Max(1053, maxLeft + maxFoot);               // right power rail is aligned at x = 1053
////////        foreach (var t in terms)
////////            t.X = t.Kind == "Block" ? railX - (t.SinkW > 0 ? t.SinkW : 30) - t.W : railX - t.W;

////////        var side = new List<Leaf>();                                 // data sources and data sinks
////////        foreach (var b in leaves.Where(l => l.Kind == "Block"))
////////        {
////////            for (int i = 0; i < b.Ins.Count; i++)
////////            {
////////                var d = b.InDs[i];
////////                if (d == null) continue;
////////                d.Row = b.Row + b.Ins[i].Row; d.X = b.X - d.W; side.Add(d);
////////            }
////////            for (int i = 0; i < b.Outs.Count; i++)
////////                foreach (var s in b.OutSinks[i]) { s.Row = b.Row + b.Outs[i].Row; s.X = b.X + b.W; side.Add(s); }
////////        }
////////        var all = leaves.Concat(side).ToList();
////////        int rows = all.Max(l => l.Row + l.Rows);

////////        Link(root);
////////        foreach (var e in extras) Link(e);
////////        foreach (var t in termBlocks) { t.ExtraSrc.Clear(); foreach (var e in t.Extra) t.ExtraSrc.Add(Exits(e.expr)); }

////////        var entries = Entries(root);
////////        foreach (var e in extras) entries.AddRange(Entries(e));
////////        var entrySet = new HashSet<Leaf>(entries);
////////        var consumed = new HashSet<Leaf>();
////////        foreach (var l in all) foreach (var s in l.Src) consumed.Add(s);
////////        foreach (var t in termBlocks) foreach (var lst in t.ExtraSrc) foreach (var s in lst) consumed.Add(s);

////////        // connection ids: used left-rail rows first, then all objects ordered by x then y
////////        int next = 1;
////////        var railId = new int[rows];
////////        var entryRows = new HashSet<int>(entries.Select(e => e.Row + e.FlowInRow));
////////        for (int r = 0; r < rows; r++) railId[r] = entryRows.Contains(r) ? next++ : 0;
////////        foreach (var l in all.OrderBy(x => x.X).ThenBy(x => x.Y))
////////        {
////////            if (l.Kind == "DataSink") continue;
////////            if (l.Kind == "Block")
////////            {
////////                foreach (int i in Enumerable.Range(0, l.Outs.Count).OrderBy(i => l.Outs[i].Row))
////////                {
////////                    bool conn = i == l.FlowOutIdx ? (consumed.Contains(l) || l.Terminal) : l.OutSinks[i].Count > 0;
////////                    l.OutIds[i] = conn ? next++ : 0;
////////                }
////////            }
////////            else l.OutIds[0] = next++;
////////        }

////////        var rightIn = new int[rows];
////////        foreach (var t in terms)
////////            if (t.FlowOutIdx >= 0) rightIn[t.Row + t.FlowOutRow] = t.OutIds[t.FlowOutIdx];

////////        IEnumerable<int> Refs(Leaf l)
////////        {
////////            if (l.Src.Count > 0) return FlowIds(l.Src);
////////            if (entrySet.Contains(l)) return new[] { railId[l.Row + l.FlowInRow] };
////////            return Array.Empty<int>();
////////        }

////////        var sb = new StringBuilder();
////////        Line(sb, 12, $"<Rung evaluationOrder=\"{evalOrder}\" label=\"{Esc(label)}\">");
////////        Line(sb, 14, $"<Documentation xsi:type=\"SimpleText\"><![CDATA[{doc.Replace("]]>", "]] >")}]]></Documentation>");

////////        Line(sb, 14, "<LdObject xsi:type=\"LeftPowerRail\">");
////////        Line(sb, 16, "<RelPosition x=\"0\" y=\"0\" />");
////////        Line(sb, 16, $"<Size x=\"3\" y=\"{38 * rows}\" />");
////////        for (int r = 0; r < rows; r++)
////////        {
////////            Line(sb, 16, $"<ConnectionPointOut connectionPointOutId=\"{railId[r]}\">");
////////            Line(sb, 18, $"<RelPosition x=\"3\" y=\"{28 + 38 * r}\" />");
////////            Line(sb, 16, "</ConnectionPointOut>");
////////        }
////////        Line(sb, 14, "</LdObject>");

////////        void AnalogData(int ind)
////////        {
////////            Line(sb, ind, "<AddData>");
////////            Line(sb, ind + 2, "<Data name=\"BR_AnalogValue\" handleUnknown=\"implementation\">");
////////            Line(sb, ind + 4, "<AnalogValue value=\"true\" />");
////////            Line(sb, ind + 2, "</Data>");
////////            Line(sb, ind, "</AddData>");
////////        }
////////        void EnEnoData(int ind)
////////        {
////////            Line(sb, ind, "<AddData>");
////////            Line(sb, ind + 2, "<Data name=\"BR_IsEnEnoParameter\" handleUnknown=\"implementation\">");
////////            Line(sb, ind + 4, "<IsEnEnoParameter value=\"true\" />");
////////            Line(sb, ind + 2, "</Data>");
////////            Line(sb, ind, "</AddData>");
////////        }

////////        foreach (var l in all.OrderBy(x => x.X).ThenBy(x => x.Y))
////////        {
////////            switch (l.Kind)
////////            {
////////                case "Contact":
////////                case "Coil":
////////                    {
////////                        string at = $"operand=\"{Esc(l.Operand)}\"" + (l.Neg ? " negated=\"true\"" : "") + $" edge=\"{l.Edge}\"" +
////////                                    (l.Kind == "Coil" ? $" latch=\"{l.Latch}\"" : "");
////////                        Line(sb, 14, $"<LdObject xsi:type=\"{l.Kind}\" {at}>");
////////                        Line(sb, 16, $"<RelPosition x=\"{l.X}\" y=\"{l.Y}\" />");
////////                        Line(sb, 16, $"<Size x=\"{l.W}\" y=\"20\" />");
////////                        Line(sb, 16, "<ConnectionPointIn>");
////////                        Line(sb, 18, "<RelPosition x=\"0\" y=\"10\" />");
////////                        foreach (int id in Refs(l)) Line(sb, 18, $"<Connection refConnectionPointOutId=\"{id}\" />");
////////                        Line(sb, 16, "</ConnectionPointIn>");
////////                        Line(sb, 16, $"<ConnectionPointOut connectionPointOutId=\"{l.OutIds[0]}\">");
////////                        Line(sb, 18, $"<RelPosition x=\"{l.W}\" y=\"10\" />");
////////                        Line(sb, 16, "</ConnectionPointOut>");
////////                        Line(sb, 14, "</LdObject>");
////////                        break;
////////                    }
////////                case "DataSource":
////////                    {
////////                        Line(sb, 14, $"<FbdObject xsi:type=\"DataSource\" identifier=\"{Esc(l.Operand)}\">");
////////                        AnalogData(16);
////////                        Line(sb, 16, $"<RelPosition x=\"{l.X}\" y=\"{l.Y}\" />");
////////                        Line(sb, 16, $"<Size x=\"{l.W}\" y=\"20\" />");
////////                        Line(sb, 16, $"<ConnectionPointOut connectionPointOutId=\"{l.OutIds[0]}\">");
////////                        Line(sb, 18, $"<RelPosition x=\"{l.W}\" y=\"10\" />");
////////                        Line(sb, 16, "</ConnectionPointOut>");
////////                        Line(sb, 14, "</FbdObject>");
////////                        break;
////////                    }
////////                case "DataSink":
////////                    {
////////                        Line(sb, 14, $"<FbdObject xsi:type=\"DataSink\" identifier=\"{Esc(l.Operand)}\">");
////////                        AnalogData(16);
////////                        Line(sb, 16, $"<RelPosition x=\"{l.X}\" y=\"{l.Y}\" />");
////////                        Line(sb, 16, $"<Size x=\"{l.W}\" y=\"20\" />");
////////                        Line(sb, 16, "<ConnectionPointIn>");
////////                        Line(sb, 18, "<RelPosition x=\"0\" y=\"10\" />");
////////                        Line(sb, 18, $"<Connection refConnectionPointOutId=\"{l.SinkOwner!.OutIds[l.SinkPin]}\" />");
////////                        Line(sb, 16, "</ConnectionPointIn>");
////////                        Line(sb, 14, "</FbdObject>");
////////                        break;
////////                    }
////////                case "Block":
////////                    {
////////                        Line(sb, 14, $"<FbdObject xsi:type=\"Block\" typeName=\"{l.Type}\"" +
////////                                     (l.Instance.Length > 0 ? $" instanceName=\"{Esc(l.Instance)}\"" : "") + ">");
////////                        Line(sb, 16, $"<RelPosition x=\"{l.X}\" y=\"{l.Y}\" />");
////////                        Line(sb, 16, $"<Size x=\"{l.W}\" y=\"{38 * l.Rows}\" />");
////////                        Line(sb, 16, "<InputVariables>");
////////                        int flowSeen = 0;
////////                        for (int i = 0; i < l.Ins.Count; i++)
////////                        {
////////                            var pin = l.Ins[i];
////////                            Line(sb, 18, $"<InputVariable parameterName=\"{pin.As}\">");
////////                            if (pin.En) EnEnoData(20);
////////                            Line(sb, 20, "<ConnectionPointIn>");
////////                            Line(sb, 22, $"<RelPosition x=\"0\" y=\"{28 + 38 * pin.Row}\" />");
////////                            IEnumerable<int> ids;
////////                            if (pin.Role == "DataIn") ids = new[] { l.InDs[i]!.OutIds[0] };
////////                            else if (flowSeen++ == 0) ids = Refs(l);
////////                            else
////////                            {
////////                                int ex = l.Extra.FindIndex(e => e.pin == i);
////////                                ids = ex >= 0 && ex < l.ExtraSrc.Count ? FlowIds(l.ExtraSrc[ex]) : Array.Empty<int>();
////////                            }
////////                            foreach (int id in ids) Line(sb, 22, $"<Connection refConnectionPointOutId=\"{id}\" />");
////////                            Line(sb, 20, "</ConnectionPointIn>");
////////                            Line(sb, 18, "</InputVariable>");
////////                        }
////////                        Line(sb, 16, "</InputVariables>");
////////                        Line(sb, 16, "<OutputVariables>");
////////                        for (int i = 0; i < l.Outs.Count; i++)
////////                        {
////////                            var pin = l.Outs[i];
////////                            Line(sb, 18, $"<OutputVariable parameterName=\"{pin.As}\">");
////////                            if (pin.En) EnEnoData(20);
////////                            Line(sb, 20, $"<ConnectionPointOut connectionPointOutId=\"{l.OutIds[i]}\">");
////////                            Line(sb, 22, $"<RelPosition x=\"{l.W}\" y=\"{28 + 38 * pin.Row}\" />");
////////                            Line(sb, 20, "</ConnectionPointOut>");
////////                            Line(sb, 18, "</OutputVariable>");
////////                        }
////////                        Line(sb, 16, "</OutputVariables>");
////////                        Line(sb, 14, "</FbdObject>");
////////                        break;
////////                    }
////////            }
////////        }

////////        Line(sb, 14, "<LdObject xsi:type=\"RightPowerRail\">");
////////        Line(sb, 16, $"<RelPosition x=\"{railX}\" y=\"0\" />");
////////        Line(sb, 16, $"<Size x=\"3\" y=\"{38 * rows}\" />");
////////        for (int r = 0; r < rows; r++)
////////        {
////////            Line(sb, 16, "<ConnectionPointIn>");
////////            Line(sb, 18, $"<RelPosition x=\"0\" y=\"{28 + 38 * r}\" />");
////////            if (rightIn[r] != 0) Line(sb, 18, $"<Connection refConnectionPointOutId=\"{rightIn[r]}\" />");
////////            Line(sb, 16, "</ConnectionPointIn>");
////////        }
////////        Line(sb, 14, "</LdObject>");
////////        Line(sb, 12, "</Rung>");
////////        return sb.ToString();
////////    }
////////}

////////// ======================================================================= program
////////static class Program
////////{
////////    // ===== settings =====
////////    // File names (without or with ".xml") in your Downloads folder. Command-line arguments override this list.
////////    static readonly string[] DefaultInputs =
////////    {
////////        "PR_ModeManageNoMode_Siemens 1 (1)",
////////        "PR_CycleTime 1",
////////    };

////////    static IEnumerable<XElement> E(XElement p, string n) => p.Elements().Where(x => x.Name.LocalName == n);
////////    static XElement? F(XElement p, string n) => E(p, n).FirstOrDefault();

////////    static string? Ml(XElement unit, string comp)
////////    {
////////        var ol = F(unit, "ObjectList");
////////        var t = ol == null ? null : E(ol, "MultilingualText").FirstOrDefault(x => (string?)x.Attribute("CompositionName") == comp);
////////        var tol = t == null ? null : F(t, "ObjectList");
////////        if (tol == null) return null;
////////        var items = E(tol, "MultilingualTextItem")
////////            .Select(i => F(i, "AttributeList"))
////////            .Where(a => a != null)
////////            .Select(a => (cult: F(a!, "Culture")?.Value ?? "", text: F(a!, "Text")?.Value ?? ""))
////////            .Where(x => !string.IsNullOrWhiteSpace(x.text)).ToList();
////////        if (items.Count == 0) return null;
////////        var pick = items.FirstOrDefault(x => x.cult == "en-US");
////////        return pick.text ?? items[0].text;
////////    }

////////    // valid AS label: letters/digits/_ only, max 32 chars, unique inside the file
////////    static string MakeLabel(string raw, int index, HashSet<string> usedLabels)
////////    {
////////        var chars = raw.Trim().Select(c => char.IsLetterOrDigit(c) && c < 128 ? c : '_').ToArray();
////////        var sb = new StringBuilder();
////////        foreach (char c in chars) { if (c == '_' && sb.Length > 0 && sb[sb.Length - 1] == '_') continue; sb.Append(c); }
////////        string b = sb.ToString().Trim('_');
////////        if (b.Length == 0) b = "Network_" + index;
////////        if (char.IsDigit(b[0])) b = "_" + b;
////////        if (b.Length > 32) b = b.Substring(0, 32).TrimEnd('_');
////////        string label = b; int n = 2;
////////        while (!usedLabels.Add(label))
////////        {
////////            string suf = "_" + n++;
////////            label = (b.Length + suf.Length > 32 ? b.Substring(0, 32 - suf.Length) : b) + suf;
////////        }
////////        return label;
////////    }

////////    static int Main(string[] args)
//////////////    {
//////////////        string downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
//////////////        Console.WriteLine("Downloads folder: " + downloads);
//////////////        int rc = 0;
//////////////        foreach (var name in args.Length > 0 ? args : DefaultInputs)
//////////////        {
//////////////            try
//////////////            {
//////////////                Convert(downloads, name);
//////////////                GenerateFunFileFromXml(downloads, name);
//////////////            }
//////////////            catch (Exception ex)
//////////////            {
//////////////                Console.Error.WriteLine($"ERROR [{name}]: {ex.Message}");
//////////////                rc = 1;
//////////////            }
//////////////        }
//////////////        return rc;
//////////////    }

//////////////    static void Convert(string downloads, string name)
//////////////    {
//////////////        string? input = new[] { Path.Combine(downloads, name), Path.Combine(downloads, name + ".xml") }.FirstOrDefault(File.Exists)
//////////////                        ?? Directory.GetFiles(downloads, name + "*").FirstOrDefault(f => !f.EndsWith("_LD.ld", StringComparison.OrdinalIgnoreCase));
//////////////        if (input == null) throw new FileNotFoundException($"input '{name}' not found in {downloads}");
//////////////        Console.WriteLine($"Reading  {input}");

//////////////        var doc = XDocument.Load(input);
//////////////        var fb = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "SW.Blocks.FB")
//////////////                 ?? throw new InvalidDataException("no SW.Blocks.FB element - not a TIA Openness FB export");
//////////////        string fbName = F(F(fb, "AttributeList")!, "Name")?.Value ?? "FB";

//////////////        var rungs = new List<string>();
//////////////        var warnings = new List<string>();
//////////////        var usedLabels = new HashSet<string>();
//////////////        int ord = 1;

//////////////        // optional: hand-made rung 1 (e.g. a network that was commented out in the Siemens project)
//////////////        string rung1 = Path.Combine(downloads, fbName + "_Rung1.xml");
//////////////        if (File.Exists(rung1)) { rungs.Add(File.ReadAllText(rung1).TrimEnd() + "\r\n"); ord++; }

//////////////        int netNo = 0;
//////////////        foreach (var unit in E(F(fb, "ObjectList")!, "SW.Blocks.CompileUnit"))
//////////////        {
//////////////            var ns = F(F(unit, "AttributeList")!, "NetworkSource");
//////////////            var flg = ns == null ? null : F(ns, "FlgNet");
//////////////            if (flg == null) continue;                              // header-comment unit (empty NetworkSource)
//////////////            netNo++;

//////////////            string title = Ml(unit, "Title") ?? "", cmt = Ml(unit, "Comment") ?? "";
//////////////            string label = MakeLabel(cmt != "" ? cmt : title, netNo, usedLabels);
//////////////            string text = title != "" ? title : cmt;

//////////////            try { rungs.Add(new RungBuilder(flg).Emit(ord, label, text)); }
//////////////            catch (Exception ex)
//////////////            {
//////////////                warnings.Add($"network {netNo} '{label}': {ex.Message}");
//////////////                rungs.Add(RungBuilder.EmptyRung(ord, label, $"{text} [NOT CONVERTED: {ex.Message}]"));
//////////////            }
//////////////            ord++;
//////////////        }

//////////////        var sb = new StringBuilder();
//////////////        void L(int ind, string s) => sb.Append(' ', ind).Append(s).Append("\r\n");
//////////////        L(0, "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
//////////////        L(0, "<?AutomationStudio FileVersion=\"6.0\"?>");
//////////////        L(0, "<Project xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xhtml=\"http://www.w3.org/1999/xhtml\" schemaVersion=\"1.0\" xmlns=\"www.iec.ch/public/TC65SC65BWG7TF10\">");
//////////////        L(2, "<FileHeader companyName=\"B&amp;R Industrial Automation GmbH\" productName=\"Automation Studio\" productVersion=\"6.0\" />");
//////////////        L(2, "<ContentHeader name=\"\" creationDateTime=\"0001-01-01T00:00:00\">");
//////////////        L(4, "<CoordinateInfo />");
//////////////        L(4, "<AddData>");
//////////////        L(6, "<Data name=\"BR_EditorMode\" handleUnknown=\"implementation\">");
//////////////        L(8, "<EditorMode value=\"Default\" />");
//////////////        L(6, "</Data>");
//////////////        L(6, "<Data name=\"BR_AlignRightPowerRails\" handleUnknown=\"implementation\">");
//////////////        L(8, "<AlignRightPowerRails value=\"true\" />");
//////////////        L(6, "</Data>");
//////////////        L(4, "</AddData>");
//////////////        L(2, "</ContentHeader>");
//////////////        L(2, "<Types>");
//////////////        L(4, "<GlobalNamespace>");
//////////////        L(6, $"<FunctionBlock name=\"{System.Security.SecurityElement.Escape(fbName)}\">");
//////////////        L(8, "<Documentation xsi:type=\"SimpleText\"><![CDATA[TODO: Add your comment here]]></Documentation>");
//////////////        L(8, "<MainBody>");
//////////////        L(10, "<BodyContent xsi:type=\"LD\">");
//////////////        foreach (var r in rungs) sb.Append(r);
//////////////        L(10, "</BodyContent>");
//////////////        L(8, "</MainBody>");
//////////////        L(6, "</FunctionBlock>");
//////////////        L(4, "</GlobalNamespace>");
//////////////        L(2, "</Types>");
//////////////        L(2, "<Instances />");
//////////////        L(0, "</Project>");

//////////////        string outPath = Path.Combine(downloads, fbName + "_LD.ld");
//////////////        File.WriteAllText(outPath, sb.ToString(), new UTF8Encoding(false));
//////////////        Console.WriteLine($"Done: {rungs.Count} rungs ({rungs.Count - warnings.Count} converted, {warnings.Count} not converted) -> {outPath}");
//////////////        foreach (var w in warnings) Console.WriteLine("  WARNING " + w);
//////////////    }
//////////////}










#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

// ======================================================================= layout tree
abstract class N { }
abstract class Group : N { public List<N> Items = new(); }
sealed class FunVariable
{
    public string Name = "";
    public string Type = "";
    public string Comment = "";
}

sealed class FunSection
{
    public string Name = "";
    public List<FunVariable> Members = new();
}
sealed class Seq : Group
{
    public Seq(List<N> items) { Items = items; }
    // a block with an instance name needs one empty row above it (instance label)
    public int Pad => Items[Items.Count - 1] is Leaf l && l.Kind == "Block" && l.Instance.Length > 0 ? 1 : 0;
}
sealed class Par : Group { public Par(List<N> items) { Items = items; } }    // OR  (merging)
sealed class Fork : Group { public Fork(List<N> items) { Items = items; } }  // parallel outputs

sealed class Pin { public string Role = "", As = "", Si = ""; public int Row; public bool En; }

sealed class Leaf : N
{
    public string Kind = "";   // Contact | Coil | Block | DataSource | DataSink
    public string Operand = "";
    public bool Neg;
    public string Edge = "none", Latch = "none";
    public string Type = "", Instance = "";
    public List<Pin> Ins = new(), Outs = new();                 // block pins (emission order)
    public List<Leaf?> InDs = new();                            // parallel to Ins: data source leaf
    public List<List<Leaf>> OutSinks = new();                   // parallel to Outs: data sink leaves
    public List<(int pin, N expr)> Extra = new();               // extra power-flow inputs (TONR "R")
    public List<List<Leaf>> ExtraSrc = new();                   // parallel to Extra: leaves feeding it
    public int W, X, Row, DsW, SinkW, FlowInRow, FlowOutIdx, FlowOutRow, SinkPin;
    public int[] OutIds = new int[1];
    public List<Leaf> Src = new();
    public string CompareExpr = "";
    public Leaf? SinkOwner;
    public bool Terminal;
    public int Y => Kind == "Block" ? 38 * Row : 38 * Row + 18;
    public int Rows => Kind == "Block" ? Math.Max(Ins.Max(p => p.Row), Outs.Max(p => p.Row)) + 1 : 1;
}

sealed class Ep { public string Kind = "", Uid = "", Pin = ""; }
sealed class PartInfo
{
    public string Uid = "", Name = "", Instance = "";
    public HashSet<string> Neg = new();
    public int Card = 2;
    public Leaf? Leaf;
    public string CallName = "";   // name of the called FC / FB
    public string CallType = "";
    public List<(string Name,string Section, string Type)> Parameters = new();
}

// ======================================================================= one network -> one <Rung>
sealed class RungBuilder
{
    readonly Dictionary<string, string> access = new();
    readonly Dictionary<string, PartInfo> parts = new();
    readonly List<PartInfo> order = new();
    readonly Dictionary<string, Ep> driver = new();
    readonly Dictionary<string, List<string>> sinkMap = new();   // "uid|outPin" -> variables written
    readonly HashSet<string> used = new();                        // output pins that feed another part
    readonly Dictionary<string, N?> memo = new();
   

    // layout state (a RungBuilder is single-use)
    N root = null!;
    List<Leaf> main = new(), leaves = new(), terms = new(), termBlocks = new();
    List<N> extras = new();
    int railX;

    static readonly HashSet<string> OutPinNames = new() { "out", "eno", "out1", "out2", "out3", "Q", "ET", "QU", "QD", "CV" };
    static readonly HashSet<string> Arith = new(StringComparer.OrdinalIgnoreCase) { "Add", "Sub", "Mul", "Div", "Mod" };
    static readonly HashSet<string> Cmp = new(StringComparer.OrdinalIgnoreCase) { "Eq", "Ne", "Lt", "Le", "Gt", "Ge" };
    static readonly HashSet<string> Timers = new(StringComparer.OrdinalIgnoreCase) { "TON", "TOF", "TP", "TONR" };

    static bool IsBlock(string n) =>
     n.Equals("Call", StringComparison.OrdinalIgnoreCase) ||
     n.Equals("Move", StringComparison.OrdinalIgnoreCase) ||
     n.Equals("Not", StringComparison.OrdinalIgnoreCase) ||
     Arith.Contains(n) ||
     Cmp.Contains(n) ||
     Timers.Contains(n);

    static string? FlowOutSi(string n) =>
        n.Equals("Move", StringComparison.OrdinalIgnoreCase) || Arith.Contains(n) ? "eno" :
        n.Equals("Call", StringComparison.OrdinalIgnoreCase) ? "eno" :
        Cmp.Contains(n) ? "out" :
        n.Equals("Not", StringComparison.OrdinalIgnoreCase) ? "out" :
        Timers.Contains(n) ? "Q" :
        null;

    static IEnumerable<XElement> E(XElement p, string n) => p.Elements().Where(x => x.Name.LocalName == n);
    static XElement? F(XElement p, string n) => E(p, n).FirstOrDefault();
    static string Names(XElement p) => string.Join(".", E(p, "Component").Select(c => (string)c.Attribute("Name")!));

    // width model measured from your LD file (proportional font, 30 px steps)
    public static int TextWidth(string s, int min)
    {
        double u = 0;
        foreach (char c in s) u += "iltrfjI.".IndexOf(c) >= 0 ? 0.5 : 1.0;
        int w = 60 + 30 * (int)Math.Ceiling(Math.Max(0.0, u - 6.75) / 5.0);
        return Math.Max(w, min);
    }

    // ------------------------------------------------------------------ parsing
    public RungBuilder(XElement flg)
    {
        foreach (var el in F(flg, "Parts")!.Elements())
        {
            string uid = (string)el.Attribute("UId")!;
            if (el.Name.LocalName == "Access") access[uid] = AccessText(el);
            else if (el.Name.LocalName == "Part")
            {
                var p = new PartInfo { Uid = uid, Name = (string)el.Attribute("Name")! };
                foreach (var ng in E(el, "Negated")) p.Neg.Add((string)ng.Attribute("Name")!);
                var inst = F(el, "Instance");
                if (inst != null) p.Instance = Names(inst);
                var card = E(el, "TemplateValue").FirstOrDefault(t => (string?)t.Attribute("Name") == "Card");
                if (card != null) p.Card = int.Parse(card.Value);
                parts[uid] = p; order.Add(p);
            }
            else if (el.Name.LocalName == "Call")
            {
                var callInfo = F(el, "CallInfo");
                var p = new PartInfo
                {
                    Uid = uid,
                    Name = "Call",
                    CallName = (string?)callInfo?.Attribute("Name") ?? "",
                    CallType = (string?)callInfo?.Attribute("BlockType") ?? ""
                };
                if (callInfo != null)
                {
                    var inst = F(callInfo, "Instance");                 // only an FB call has an instance
                    if (inst != null) p.Instance = Names(inst);
                    foreach (var prm in E(callInfo, "Parameter"))
                        p.Parameters.Add(((string)prm.Attribute("Name")!,
                                          (string?)prm.Attribute("Section") ?? "",
                                          (string?)prm.Attribute("Type") ?? ""));
                }
                parts[uid] = p;
                order.Add(p);
            }
        }
        foreach (var w in E(F(flg, "Wires")!, "Wire"))
        {
            var eps = new List<Ep>();
            foreach (var c in w.Elements())
                switch (c.Name.LocalName)
                {
                    case "Powerrail": eps.Add(new Ep { Kind = "Rail" }); break;
                    case "IdentCon": eps.Add(new Ep { Kind = "Ident", Uid = (string)c.Attribute("UId")! }); break;
                    case "NameCon": eps.Add(new Ep { Kind = "Name", Uid = (string)c.Attribute("UId")!, Pin = (string)c.Attribute("Name")! }); break;
                    case "OpenCon": eps.Add(new Ep { Kind = "Open" }); break;
                }
            Ep? src = eps.FirstOrDefault(e => e.Kind == "Rail")
                   ?? eps.FirstOrDefault(e => e.Kind == "Name" && OutPinNames.Contains(e.Pin))
                   ?? eps.FirstOrDefault(e => e.Kind == "Ident");
            if (src == null) continue;
            bool nameDest = false;
            foreach (var e in eps)
            {
                if (ReferenceEquals(e, src)) continue;
                if (e.Kind == "Name") { driver[e.Uid + "|" + e.Pin] = src; nameDest = true; }
                else if (e.Kind == "Ident" && src.Kind == "Name" && access.TryGetValue(e.Uid, out var txt))
                {
                    string key = src.Uid + "|" + src.Pin;
                    if (!sinkMap.TryGetValue(key, out var lst)) sinkMap[key] = lst = new List<string>();
                    lst.Add(txt);
                }
            }
            if (src.Kind == "Name" && nameDest) used.Add(src.Uid + "|" + src.Pin);
        }
    }

    // is this pin an output of its part (= the source of the wire)?
    bool IsPinSource(Ep e)
    {
        if (parts.TryGetValue(e.Uid, out var cp) && cp.Name == "Call")
            return cp.Parameters.Any(x => x.Name == e.Pin && IsCallOutputSection(x.Section));
        return OutPinNames.Contains(e.Pin);
    }

    static bool IsCallOutputSection(string s) =>
        s.Equals("Output", StringComparison.OrdinalIgnoreCase) ||
        s.Equals("InOut", StringComparison.OrdinalIgnoreCase) ||
        s.Equals("Return", StringComparison.OrdinalIgnoreCase);

    static string SymText(XElement sym)
    {
        var l = new List<string>();
        foreach (var c in E(sym, "Component"))
        {
            string s = (string)c.Attribute("Name")!;
            if ((string?)c.Attribute("AccessModifier") == "Array")
                s += "[" + string.Join(",", E(c, "Access").Select(AccessText)) + "]";
            l.Add(s);
        }
        return string.Join(".", l);
    }

    static string AccessText(XElement a)
    {
        var sym = F(a, "Symbol");
        if (sym != null) return SymText(sym);
        var c = F(a, "Constant");
        if (c == null) return "";
        string t = F(c, "ConstantType")?.Value ?? "";
        string v = F(c, "ConstantValue")?.Value ?? "";
        if (t.Equals("Bool", StringComparison.OrdinalIgnoreCase)) return v.Equals("true", StringComparison.OrdinalIgnoreCase) || v == "1" ? "TRUE" : "FALSE";
        if (v.StartsWith("T#", StringComparison.OrdinalIgnoreCase)) return "T#" + v.Substring(2).ToLowerInvariant();
        return v;
    }

    string Operand(PartInfo p, string pin)
    {
        if (!driver.TryGetValue(p.Uid + "|" + pin, out var s))
            throw new InvalidOperationException($"{p.Name} (UId {p.Uid}): pin '{pin}' is not wired");
        if (s.Kind != "Ident" || !access.TryGetValue(s.Uid, out var t))
            throw new NotSupportedException($"{p.Name} (UId {p.Uid}): pin '{pin}' is driven by another block; only variables/constants are supported");
        return t;
    }
    string OperandAny(PartInfo p, params string[] pins)
    {
        foreach (var pin in pins) if (driver.ContainsKey(p.Uid + "|" + pin)) return Operand(p, pin);
        throw new InvalidOperationException($"{p.Name} (UId {p.Uid}): none of the pins {string.Join("/", pins)} is wired");
    }

    // ------------------------------------------------------------------ leaves
    static Pin Mk(string role, string asn, string si, int row, bool en = false) => new Pin { Role = role, As = asn, Si = si, Row = row, En = en };

    Leaf MakeLeaf(PartInfo p)
    {
        if (p.Leaf != null) return p.Leaf;
        Leaf l;
        switch (p.Name)
        {
            case "Contact": l = new Leaf { Kind = "Contact", Operand = Operand(p, "operand"), Neg = p.Neg.Contains("operand") }; break;
            case "PBox": l = new Leaf { Kind = "Contact", Operand = Operand(p, "bit"), Edge = "rising" }; break;
            case "NBox": l = new Leaf { Kind = "Contact", Operand = Operand(p, "bit"), Edge = "falling" }; break;
            case "Coil":
            case "SCoil":
            case "RCoil":
                l = new Leaf
                {
                    Kind = "Coil",
                    Operand = Operand(p, "operand"),
                    Neg = p.Neg.Contains("operand"),
                    Terminal = true,
                    Latch = p.Name == "SCoil" ? "set" : p.Name == "RCoil" ? "reset" : "none"
                };
                break;

            case "Eq":
            case "Ne":
            case "Lt":
            case "Le":
            case "Gt":
            case "Ge":
                {
                    string op = p.Name switch
                    {
                        "Eq" => "=",
                        "Ne" => "<>",
                        "Lt" => "<",
                        "Le" => "<=",
                        "Gt" => ">",
                        "Ge" => ">=",
                        _ => ""
                    };

                    l = new Leaf
                    {
                        Kind = "Block",
                        Type = "Compare",
                        CompareExpr = $"{Operand(p, "in1")}{op}{Operand(p, "in2")}",
                        W = 180
                    };

                    l.Ins.Add(Mk("FlowIn", "EN", "pre", 0, true));
                    l.InDs.Add(null);                                   // parallel to Ins

                    l.Outs.Add(Mk("FlowOut", "ENO", "", 0, true));
                    //l.Outs.Add(Mk("FlowOut", "", "out", 1)); // This is original
                    l.Outs.Add(Mk("FlowOut", "", "", 1));
                    l.OutSinks.Add(new List<Leaf>());                   // parallel to Outs
                    l.OutSinks.Add(new List<Leaf>());

                    l.OutIds = new int[2];
                    //l.FlowOutIdx = 1;
                    //l.FlowOutRow = 1;

                    l.FlowOutIdx = 0;
                    l.FlowOutRow = 0;

                    l.FlowInRow = 0;

                    break;
                }

            default:
                if (!IsBlock(p.Name)) throw new NotSupportedException($"LAD part '{p.Name}' (UId {p.Uid}) is not supported");
                l = MakeBlock(p);
                break;
        }
        if (l.Kind == "Contact" || l.Kind == "Coil") l.W = TextWidth(l.Operand, 90);
        p.Leaf = l;
        return l;
    }

    Leaf MakeCallBlock(PartInfo p)
    {
        var ins = new List<Pin>();
        var outs = new List<Pin>();

        ins.Add(Mk("FlowIn", "EN", "en", 0, true));

        int row = 1;

        foreach (var prm in p.Parameters)
        {
            if (prm.Section == "Input")
            {
                ins.Add(
                    Mk("DataIn",
                        prm.Name,
                        prm.Name,
                        row++)
                );
            }
        }

        outs.Add(
            Mk("FlowOut",
                "ENO",
                "eno",
                0,
                true));

        row = 1;

        foreach (var prm in p.Parameters)
        {
            if (prm.Section == "Output" ||
               prm.Section == "InOut")
            {
                outs.Add(
                    Mk("DataOut",
                        prm.Name,
                        prm.Name,
                        row++)
                );
            }
        }

        var l = new Leaf
        {
            Kind = "Block",
            Type = "FC",
            Instance = p.Instance,
            W = TextWidth(p.Instance, 240),
            Ins = ins,
            Outs = outs
        };

        l.OutIds = new int[outs.Count];

        l.FlowOutIdx = 0;
        l.FlowOutRow = 0;
        l.FlowInRow = 0;

        return l;
    }


    Leaf MakeBlock(PartInfo p)
    {
        var ins = new List<Pin>(); var outs = new List<Pin>();
        string asType = p.Name.ToUpperInvariant();
        int w = 120;
        bool isCall = p.Name.Equals("Call", StringComparison.OrdinalIgnoreCase);

        if (p.Name.Equals("Move", StringComparison.OrdinalIgnoreCase))
        {
            ins.Add(Mk("DataIn", "IN1", "in", 1)); ins.Add(Mk("FlowIn", "EN", "en", 0, true));
            outs.Add(Mk("DataOut", "OUT1", "out1", 1)); outs.Add(Mk("FlowOut", "ENO", "eno", 0, true));
        }
        else if (Arith.Contains(p.Name))
        {
            ins.Add(Mk("FlowIn", "EN", "en", 0, true));
            for (int i = 1; i <= Math.Max(2, p.Card); i++) ins.Add(Mk("DataIn", "IN" + i, "in" + i, i));
            outs.Add(Mk("FlowOut", "ENO", "eno", 0, true)); outs.Add(Mk("DataOut", "OUT", "out", 1));
        }
        else if (p.Name.Equals("Not", StringComparison.OrdinalIgnoreCase))
        {
            w = 90;
            ins.Add(Mk("FlowIn", "IN", "in", 0)); outs.Add(Mk("FlowOut", "OUT", "out", 0));
        }
        else if (isCall)
        {
            asType = p.CallName;                                   // typeName = name of the called FC / FB
            w = Math.Max(150, TextWidth(p.CallName, 120) + 30);

            ins.Add(Mk("FlowIn", "EN", "en", 0, true));
            int inRow = 1;
            foreach (var prm in p.Parameters)
                if (prm.Section.Equals("Input", StringComparison.OrdinalIgnoreCase))
                    ins.Add(Mk("DataIn", prm.Name, prm.Name, inRow++));

            outs.Add(Mk("FlowOut", "ENO", "eno", 0, true));
            int outRow = 1;
            foreach (var prm in p.Parameters)
                if (IsCallOutputSection(prm.Section))
                    outs.Add(Mk("DataOut", prm.Name, prm.Name, outRow++));
        }
        else // timers
        {
            w = TextWidth(p.Instance, 120);
            ins.Add(Mk("FlowIn", "IN", "IN", 0));
            if (p.Name.Equals("TONR", StringComparison.OrdinalIgnoreCase)) ins.Add(Mk("FlowIn", "R", "R", 1));
            ins.Add(Mk("DataIn", "PT", "PT", ins.Count));
            outs.Add(Mk("FlowOut", "Q", "Q", 0)); outs.Add(Mk("DataOut", "ET", "ET", 1));
        }

        var l = new Leaf
        {
            Kind = "Block",
            Type = asType,
            Instance = p.Instance,                                  // for a Call: only an FB instance, empty for an FC
            W = w,
            Ins = ins,
            Outs = outs
        };
        l.OutIds = new int[outs.Count];
        l.FlowOutIdx = outs.FindIndex(o => o.Role == "FlowOut");
        l.FlowOutRow = l.FlowOutIdx >= 0 ? outs[l.FlowOutIdx].Row : 0;
        l.FlowInRow = ins.First(i => i.Role == "FlowIn").Row;

        foreach (var pin in ins)
        {
            if (pin.Role == "DataIn")
            {
                if (isCall && !driver.ContainsKey(p.Uid + "|" + pin.Si)) { l.InDs.Add(null); continue; }   // unwired input: empty pin
                string t = pin.Si == "in" ? OperandAny(p, "in", "in1") : Operand(p, pin.Si);
                l.InDs.Add(new Leaf { Kind = "DataSource", Operand = t, W = TextWidth(t, 60) });
            }
            else l.InDs.Add(null);
        }
        for (int i = 0; i < outs.Count; i++)
        {
            var lst = new List<Leaf>();
            if (outs[i].Role == "DataOut" && sinkMap.TryGetValue(p.Uid + "|" + outs[i].Si, out var names))
                foreach (var n in names) lst.Add(new Leaf { Kind = "DataSink", Operand = n, W = TextWidth(n, 60), SinkOwner = l, SinkPin = i });
            l.OutSinks.Add(lst);
        }
        l.DsW = l.InDs.Where(d => d != null).Select(d => d!.W).DefaultIfEmpty(0).Max();
        l.SinkW = l.OutSinks.SelectMany(s => s).Select(s => s.W).DefaultIfEmpty(0).Max();
        string? fo = FlowOutSi(p.Name);
        l.Terminal = fo == null || !used.Contains(p.Uid + "|" + fo);
        return l;
    }

    static Leaf MakeTrue() => new Leaf { Kind = "Contact", Operand = "TRUE", W = 90 };

    // ------------------------------------------------------------------ expression tree
    static List<N> AsList(N? n) => n == null ? new List<N>() : n is Seq s ? new List<N>(s.Items) : new List<N> { n };
    static N? Make(List<N> items) => items.Count == 0 ? null : items.Count == 1 ? items[0] : new Seq(items);
    static N Single(List<N> l) => l.Count == 1 ? l[0] : new Seq(l);
    static N? Cat(N? a, N? b) { var l = AsList(a); l.AddRange(AsList(b)); return Make(l); }

    N? ExprIn(PartInfo p, string pin)
    {
        if (!driver.TryGetValue(p.Uid + "|" + pin, out var src))
        {
            if (pin == "en" || pin == "pre") return null;       // unwired EN = always enabled
            throw new InvalidOperationException($"{p.Name} (UId {p.Uid}): input '{pin}' is not wired");
        }
        if (src.Kind == "Rail") return null;
        if (src.Kind != "Name") throw new NotSupportedException($"{p.Name} (UId {p.Uid}): input '{pin}' is driven by a variable, not by logic");
        var sp = parts[src.Uid];
        if (IsBlock(sp.Name) && src.Pin != FlowOutSi(sp.Name))
            throw new NotSupportedException($"{sp.Name} (UId {sp.Uid}): output '{src.Pin}' is used as a signal; only '{FlowOutSi(sp.Name)}' is supported");
        return ExprOut(sp);
    }

    N? ExprOut(PartInfo p)
    {
        if (memo.TryGetValue(p.Uid, out var m)) return m;
        N? r;
        switch (p.Name)
        {
            case "Contact":
            case "PBox":
            case "NBox":
                r = Cat(ExprIn(p, "in"), MakeLeaf(p));
                break;

            case "Eq":
            case "Ne":
            case "Lt":
            case "Le":
            case "Gt":
            case "Ge":
                r = Cat(ExprIn(p, "pre"), MakeLeaf(p));
                break;

            case "O":
                {
                    var br = new List<N?>();
                    for (int i = 1; i <= p.Card; i++) br.Add(ExprIn(p, "in" + i));
                    r = MakePar(br);
                    break;
                }
            case "A":
                {
                    N? acc = null;
                    for (int i = 1; i <= p.Card; i++) acc = Cat(acc, ExprIn(p, "in" + i));
                    r = acc;
                    break;
                }
            default:
                if (IsBlock(p.Name))
                {
                    var leaf = MakeLeaf(p);
                    if (leaf.Ins.Count(i => i.Role == "FlowIn") > 1)
                        throw new NotSupportedException($"{p.Name} (UId {p.Uid}): a block with two power-flow inputs must be the last element of a rung");
                    r = Cat(ExprIn(p, leaf.Ins.First(i => i.Role == "FlowIn").Si), leaf);
                    break;
                }
                throw new NotSupportedException($"LAD part '{p.Name}' (UId {p.Uid}) is not supported");
        }
        memo[p.Uid] = r;
        return r;
    }

    static N? MakePar(List<N?> branches)
    {
        if (branches.Any(b => b == null)) return null;             // OR with the rail = always true
        var lists = branches.Select(AsList).ToList();
        var prefix = new List<N>();
        while (lists.All(l => l.Count > 0) && lists.All(l => ReferenceEquals(l[0], lists[0][0])))
        {
            prefix.Add(lists[0][0]);
            lists = lists.Select(l => l.Skip(1).ToList()).ToList();
        }
        if (lists.Any(l => l.Count == 0)) return Make(prefix);
        N core = lists.Count == 1 ? Single(lists[0]) : new Par(lists.Select(Single).ToList());
        prefix.AddRange(AsList(core));
        return Make(prefix);
    }

    bool IsTermPart(PartInfo p) =>
        p.Name == "Coil" || p.Name == "SCoil" || p.Name == "RCoil" || (IsBlock(p.Name) && MakeLeaf(p).Terminal);

    // chains (one per output) -> trunk + forks; shared first elements are factored out recursively.
    // A branch that is only an output block/coil, behind a first branch that has contacts, gets its own
    // TRUE contact next to the trunk (this is what AS does itself). Without it the wire from the trunk
    // would have to cross the first branch and AS draws a break / connection markers.
    N BuildTree(List<List<N>> chains, bool top)
    {
        if (chains.Count == 1) return new Seq(chains[0]);

        var trunk = new List<N>();
        while (chains.All(c => c.Count > 1 && ReferenceEquals(c[0], chains[0][0])))
        {
            trunk.Add(chains[0][0]);
            chains = chains.Select(c => c.Skip(1).ToList()).ToList();
        }

        // lines that all start with their own contact are wired straight to the left power rail (as in TIA);
        // a TRUE contact is added only when some line starts with an output block / coil
        bool allStartWithContact = chains.All(c => (c[0] is Leaf cl && cl.Kind == "Contact") || c[0] is Par);
        if (trunk.Count == 0 && top && !allStartWithContact) trunk.Add(MakeTrue());

        var groups = new List<List<List<N>>>();
        foreach (var ch in chains)
        {
            List<List<N>>? found = null;
            if (ch.Count > 1) found = groups.FirstOrDefault(x => x[0].Count > 1 && ReferenceEquals(x[0][0], ch[0]));
            if (found != null) found.Add(ch); else groups.Add(new List<List<N>> { ch });
        }
        bool firstLone = groups[0].Count == 1 && groups[0][0].Count == 1;

        var branches = new List<N>();
        foreach (var g in groups)
        {
            if (g.Count == 1)
            {
                var one = g[0];
                if (!firstLone && one.Count == 1 && one[0] is Leaf lf && lf.Terminal)
                    branches.Add(new Seq(new List<N> { MakeTrue(), one[0] }));
                else
                    branches.Add(new Seq(one));
            }
            else
            {
                var items = new List<N> { g[0][0] };
                var sub = (Seq)BuildTree(g.Select(x => x.Skip(1).ToList()).ToList(), false);
                items.AddRange(sub.Items);
                branches.Add(new Seq(items));
            }
        }
        trunk.Add(new Fork(branches));
        return new Seq(trunk);
    }

    N BuildRoot()
    {
        var chains = new List<List<N>>();
        foreach (var t in order.Where(p => IsTermPart(p)).ToList())
        {
            var leaf = MakeLeaf(t);
            string inPin = leaf.Kind == "Coil" ? "in" : leaf.Ins.First(i => i.Role == "FlowIn").Si;
            if (leaf.Kind == "Block" && leaf.Extra.Count == 0)
                for (int i = 0; i < leaf.Ins.Count; i++)
                    if (leaf.Ins[i].Role == "FlowIn" && leaf.Ins[i].Si != inPin)
                        leaf.Extra.Add((i, ExprIn(t, leaf.Ins[i].Si) ?? (N)MakeTrue()));
            chains.Add(AsList(Cat(ExprIn(t, inPin), leaf)));
        }
        if (chains.Count == 0) throw new InvalidOperationException("network has no coil / output block");
        return BuildTree(chains, true);
    }

    // ------------------------------------------------------------------ geometry
    static int Shift(N n) => n is Leaf l ? (l.Kind == "Block" && l.FlowOutIdx >= 0 ? l.FlowOutRow : 0)
                           : n is Seq s ? s.Items.Sum(x => Shift(x)) : 0;

    static int H(N n)
    {
        switch (n)
        {
            case Leaf l: return l.Rows;
            case Seq s:
                {
                    int r = 0, h = 0;
                    foreach (var it in s.Items) { h = Math.Max(h, r + H(it)); r += Shift(it); }
                    return s.Pad + h;
                }
            case Group g: return g.Items.Sum(x => H(x));
            default: return 1;
        }
    }
    static int Wd(N n)
    {
        switch (n)
        {
            case Leaf l: return l.Terminal ? 0 : l.Kind == "Block" ? l.DsW + l.W + l.SinkW : l.W;
            case Seq s: return s.Items.Sum(x => Wd(x));
            case Group g: return g.Items.Max(x => Wd(x));
            default: return 0;
        }
    }
    static void Place(N n, int x, int row)
    {
        switch (n)
        {
            case Leaf l: l.X = x + (l.Kind == "Block" ? l.DsW : 0); l.Row = row; break;
            case Seq s:
                {
                    int r = row + s.Pad, cx = x;
                    foreach (var it in s.Items) { Place(it, cx, r); cx += Wd(it); r += Shift(it); }
                    break;
                }
            case Group g:
                {
                    int r = row;
                    foreach (var it in g.Items) { Place(it, x, r); r += H(it); }
                    break;
                }
        }
    }
    static IEnumerable<Leaf> Leaves(N n) => n is Leaf l ? new[] { l } : ((Group)n).Items.SelectMany(Leaves);
    static List<Leaf> Entries(N n) => n switch
    {
        Leaf l => new List<Leaf> { l },
        Seq s => Entries(s.Items[0]),
        Group g => g.Items.SelectMany(Entries).ToList(),
        _ => new List<Leaf>()
    };
    static List<Leaf> Exits(N n) => n switch
    {
        Leaf l => new List<Leaf> { l },
        Seq s => Exits(s.Items[s.Items.Count - 1]),
        Group g => g.Items.SelectMany(Exits).ToList(),
        _ => new List<Leaf>()
    };
    static void Link(N n)
    {
        if (n is Leaf) return;
        var g = (Group)n;
        foreach (var c in g.Items) Link(c);
        if (n is Seq s)
            for (int i = 1; i < s.Items.Count; i++)
            {
                var ex = Exits(s.Items[i - 1]);
                foreach (var e in Entries(s.Items[i])) e.Src.AddRange(ex);
            }
    }
    static int Right(Leaf l) => l.X + l.W + (l.Kind == "Block" ? l.SinkW : 0);
    static int Foot(Leaf l) => l.Kind == "Block" ? l.DsW + l.W + (l.SinkW > 0 ? l.SinkW : 30) : l.W;
    static IEnumerable<int> FlowIds(IEnumerable<Leaf> src) => src.Select(s => s.OutIds[s.FlowOutIdx]).Distinct().OrderBy(i => i);

    // builds the tree and places everything; returns the natural right-rail x of this rung
    int Layout(int fixedRail)
    {
        root = BuildRoot();
        main = Leaves(root).ToList();
        terms = main.Where(l => l.Terminal).ToList();
        termBlocks = terms.Where(l => l.Kind == "Block").ToList();
        extras = new List<N>();
        foreach (var t in termBlocks) foreach (var e in t.Extra) extras.Add(e.expr);
        leaves = new List<Leaf>(main);
        foreach (var e in extras) leaves.AddRange(Leaves(e));

        Place(root, 3, 0);
        // extra power-flow inputs (TONR reset) start on the row of their pin
        foreach (var t in termBlocks)
            foreach (var e in t.Extra) Place(e.expr, 3, t.Row + t.Ins[e.pin].Row);

        int maxLeft = leaves.Where(l => !l.Terminal).Select(Right).DefaultIfEmpty(3).Max();
        int maxFoot = terms.Select(Foot).DefaultIfEmpty(0).Max();
        int natural = Math.Max(1053, maxLeft + maxFoot);
        railX = fixedRail > 0 ? Math.Max(fixedRail, natural) : natural;
        foreach (var t in terms)
            t.X = t.Kind == "Block" ? railX - (t.SinkW > 0 ? t.SinkW : 30) - t.W : railX - t.W;
        return natural;
    }

    public int Natural() => Layout(0);

    // ------------------------------------------------------------------ output
    static void Line(StringBuilder sb, int ind, string s) => sb.Append(' ', ind).Append(s).Append("\r\n");
    static string Esc(string s) => System.Security.SecurityElement.Escape(s) ?? "";

    public static string EmptyRung(int evalOrder, string label, string doc, int rail)
    {
        var sb = new StringBuilder();
        string lab = label.Length > 0 ? $" label=\"{Esc(label)}\"" : "";
        Line(sb, 12, $"<Rung evaluationOrder=\"{evalOrder}\"{lab}>");
        Line(sb, 14, $"<Documentation xsi:type=\"SimpleText\"><![CDATA[{doc.Replace("]]>", "]] >")}]]></Documentation>");
        Line(sb, 14, "<LdObject xsi:type=\"LeftPowerRail\">");
        Line(sb, 16, "<RelPosition x=\"0\" y=\"0\" />");
        Line(sb, 16, "<Size x=\"3\" y=\"38\" />");
        Line(sb, 16, "<ConnectionPointOut connectionPointOutId=\"0\">");
        Line(sb, 18, "<RelPosition x=\"3\" y=\"28\" />");
        Line(sb, 16, "</ConnectionPointOut>");
        Line(sb, 14, "</LdObject>");
        Line(sb, 14, "<LdObject xsi:type=\"RightPowerRail\">");
        Line(sb, 16, $"<RelPosition x=\"{rail}\" y=\"0\" />");
        Line(sb, 16, "<Size x=\"3\" y=\"38\" />");
        Line(sb, 16, "<ConnectionPointIn>");
        Line(sb, 18, "<RelPosition x=\"0\" y=\"28\" />");
        Line(sb, 16, "</ConnectionPointIn>");
        Line(sb, 14, "</LdObject>");
        Line(sb, 12, "</Rung>");
        return sb.ToString();
    }

    public string Emit(int evalOrder, string label, string doc, int fixedRail)
    {
        Layout(fixedRail);

        var side = new List<Leaf>();                                 // data sources and data sinks
        foreach (var b in leaves.Where(l => l.Kind == "Block"))
        {
            for (int i = 0; i < b.Ins.Count; i++)
            {
                var d = b.InDs[i];
                if (d == null) continue;
                d.Row = b.Row + b.Ins[i].Row; d.X = b.X - d.W; side.Add(d);
            }
            for (int i = 0; i < b.Outs.Count; i++)
                foreach (var s in b.OutSinks[i]) { s.Row = b.Row + b.Outs[i].Row; s.X = b.X + b.W; side.Add(s); }
        }
        var all = leaves.Concat(side).ToList();
        int rows = all.Max(l => l.Row + l.Rows);

        Link(root);
        foreach (var e in extras) Link(e);
        foreach (var t in termBlocks) { t.ExtraSrc.Clear(); foreach (var e in t.Extra) t.ExtraSrc.Add(Exits(e.expr)); }

        var entries = Entries(root);
        foreach (var e in extras) entries.AddRange(Entries(e));
        var entrySet = new HashSet<Leaf>(entries);
        var consumed = new HashSet<Leaf>();
        foreach (var l in all) foreach (var s in l.Src) consumed.Add(s);
        foreach (var t in termBlocks) foreach (var lst in t.ExtraSrc) foreach (var s in lst) consumed.Add(s);

        // connection ids: used left-rail rows first, then all objects ordered by x then y
        int next = 1;
        var railId = new int[rows];
        var entryRows = new HashSet<int>(entries.Select(e => e.Row + e.FlowInRow));
        for (int r = 0; r < rows; r++) railId[r] = entryRows.Contains(r) ? next++ : 0;
        foreach (var l in all.OrderBy(x => x.X).ThenBy(x => x.Y))
        {
            if (l.Kind == "DataSink") continue;
            if (l.Kind == "Block")
            {
                foreach (int i in Enumerable.Range(0, l.Outs.Count).OrderBy(i => l.Outs[i].Row))
                {
                    bool conn = i == l.FlowOutIdx ? (consumed.Contains(l) || l.Terminal) : l.OutSinks[i].Count > 0;
                    l.OutIds[i] = conn ? next++ : 0;
                }
            }
            else l.OutIds[0] = next++;
        }

        var rightIn = new int[rows];
        foreach (var t in terms)
            if (t.FlowOutIdx >= 0) rightIn[t.Row + t.FlowOutRow] = t.OutIds[t.FlowOutIdx];

        IEnumerable<int> Refs(Leaf l)
        {
            if (l.Src.Count > 0) return FlowIds(l.Src);
            if (entrySet.Contains(l)) return new[] { railId[l.Row + l.FlowInRow] };
            return Array.Empty<int>();
        }

        var sb = new StringBuilder();
        string lab = label.Length > 0 ? $" label=\"{Esc(label)}\"" : "";
        Line(sb, 12, $"<Rung evaluationOrder=\"{evalOrder}\"{lab}>");
        Line(sb, 14, $"<Documentation xsi:type=\"SimpleText\"><![CDATA[{doc.Replace("]]>", "]] >")}]]></Documentation>");

        Line(sb, 14, "<LdObject xsi:type=\"LeftPowerRail\">");
        Line(sb, 16, "<RelPosition x=\"0\" y=\"0\" />");
        Line(sb, 16, $"<Size x=\"3\" y=\"{38 * rows}\" />");
        for (int r = 0; r < rows; r++)
        {
            Line(sb, 16, $"<ConnectionPointOut connectionPointOutId=\"{railId[r]}\">");
            Line(sb, 18, $"<RelPosition x=\"3\" y=\"{28 + 38 * r}\" />");
            Line(sb, 16, "</ConnectionPointOut>");
        }
        Line(sb, 14, "</LdObject>");

        void AnalogData(int ind)
        {
            Line(sb, ind, "<AddData>");
            Line(sb, ind + 2, "<Data name=\"BR_AnalogValue\" handleUnknown=\"implementation\">");
            Line(sb, ind + 4, "<AnalogValue value=\"true\" />");
            Line(sb, ind + 2, "</Data>");
            Line(sb, ind, "</AddData>");
        }
        void EnEnoData(int ind)
        {
            Line(sb, ind, "<AddData>");
            Line(sb, ind + 2, "<Data name=\"BR_IsEnEnoParameter\" handleUnknown=\"implementation\">");
            Line(sb, ind + 4, "<IsEnEnoParameter value=\"true\" />");
            Line(sb, ind + 2, "</Data>");
            Line(sb, ind, "</AddData>");
        }

        foreach (var l in all.OrderBy(x => x.X).ThenBy(x => x.Y))
        {
            switch (l.Kind)
            {
                case "Contact":
                case "Coil":
                    {
                        string at = $"operand=\"{Esc(l.Operand)}\"" + (l.Neg ? " negated=\"true\"" : "") + $" edge=\"{l.Edge}\"" +
                                    (l.Kind == "Coil" ? $" latch=\"{l.Latch}\"" : "");
                        Line(sb, 14, $"<LdObject xsi:type=\"{l.Kind}\" {at}>");
                        Line(sb, 16, $"<RelPosition x=\"{l.X}\" y=\"{l.Y}\" />");
                        Line(sb, 16, $"<Size x=\"{l.W}\" y=\"20\" />");
                        Line(sb, 16, "<ConnectionPointIn>");
                        Line(sb, 18, "<RelPosition x=\"0\" y=\"10\" />");
                        foreach (int id in Refs(l)) Line(sb, 18, $"<Connection refConnectionPointOutId=\"{id}\" />");
                        Line(sb, 16, "</ConnectionPointIn>");
                        Line(sb, 16, $"<ConnectionPointOut connectionPointOutId=\"{l.OutIds[0]}\">");
                        Line(sb, 18, $"<RelPosition x=\"{l.W}\" y=\"10\" />");
                        Line(sb, 16, "</ConnectionPointOut>");
                        Line(sb, 14, "</LdObject>");
                        break;
                    }
                case "DataSource":
                    {
                        Line(sb, 14, $"<FbdObject xsi:type=\"DataSource\" identifier=\"{Esc(l.Operand)}\">");
                        AnalogData(16);
                        Line(sb, 16, $"<RelPosition x=\"{l.X}\" y=\"{l.Y}\" />");
                        Line(sb, 16, $"<Size x=\"{l.W}\" y=\"20\" />");
                        Line(sb, 16, $"<ConnectionPointOut connectionPointOutId=\"{l.OutIds[0]}\">");
                        Line(sb, 18, $"<RelPosition x=\"{l.W}\" y=\"10\" />");
                        Line(sb, 16, "</ConnectionPointOut>");
                        Line(sb, 14, "</FbdObject>");
                        break;
                    }
                case "DataSink":
                    {
                        Line(sb, 14, $"<FbdObject xsi:type=\"DataSink\" identifier=\"{Esc(l.Operand)}\">");
                        AnalogData(16);
                        Line(sb, 16, $"<RelPosition x=\"{l.X}\" y=\"{l.Y}\" />");
                        Line(sb, 16, $"<Size x=\"{l.W}\" y=\"20\" />");
                        Line(sb, 16, "<ConnectionPointIn>");
                        Line(sb, 18, "<RelPosition x=\"0\" y=\"10\" />");
                        Line(sb, 18, $"<Connection refConnectionPointOutId=\"{l.SinkOwner!.OutIds[l.SinkPin]}\" />");
                        Line(sb, 16, "</ConnectionPointIn>");
                        Line(sb, 14, "</FbdObject>");
                        break;
                    }
                case "Block":
                    {
                        Line(sb, 14, $"<FbdObject xsi:type=\"Block\" typeName=\"{l.Type}\"" +
                                     (l.Instance.Length > 0 ? $" instanceName=\"{Esc(l.Instance)}\"" : "") + ">");

                        if (l.Type == "Compare")
                        {
                            Line(sb, 16, "<AddData>");
                            Line(sb, 18, "<Data name=\"BR_CompareBlock\" handleUnknown=\"implementation\">");
                            Line(sb, 20, $"<CompareBlock><![CDATA[{l.CompareExpr}]]></CompareBlock>");
                            Line(sb, 18, "</Data>");
                            Line(sb, 16, "</AddData>");
                        }

                        Line(sb, 16, $"<RelPosition x=\"{l.X}\" y=\"{l.Y}\" />");
                        Line(sb, 16, $"<Size x=\"{l.W}\" y=\"{38 * l.Rows}\" />");
                        Line(sb, 16, "<InputVariables>");
                        int flowSeen = 0;
                        for (int i = 0; i < l.Ins.Count; i++)
                        {
                            var pin = l.Ins[i];
                            Line(sb, 18, $"<InputVariable parameterName=\"{pin.As}\">");
                            if (pin.En) EnEnoData(20);
                            Line(sb, 20, "<ConnectionPointIn>");
                            Line(sb, 22, $"<RelPosition x=\"0\" y=\"{28 + 38 * pin.Row}\" />");
                            IEnumerable<int> ids;
                            if (pin.Role == "DataIn") ids = l.InDs[i] != null ? new[] { l.InDs[i]!.OutIds[0] } : Array.Empty<int>();
                            else if (flowSeen++ == 0) ids = Refs(l);
                            else
                            {
                                int ex = l.Extra.FindIndex(e => e.pin == i);
                                ids = ex >= 0 && ex < l.ExtraSrc.Count ? FlowIds(l.ExtraSrc[ex]) : Array.Empty<int>();
                            }
                            foreach (int id in ids) Line(sb, 22, $"<Connection refConnectionPointOutId=\"{id}\" />");
                            Line(sb, 20, "</ConnectionPointIn>");
                            Line(sb, 18, "</InputVariable>");
                        }
                        Line(sb, 16, "</InputVariables>");
                        Line(sb, 16, "<OutputVariables>");
                        for (int i = 0; i < l.Outs.Count; i++)
                        {
                            var pin = l.Outs[i];
                            Line(sb, 18, $"<OutputVariable parameterName=\"{pin.As}\">");
                            if (pin.En) EnEnoData(20);
                            Line(sb, 20, $"<ConnectionPointOut connectionPointOutId=\"{l.OutIds[i]}\">");
                            Line(sb, 22, $"<RelPosition x=\"{l.W}\" y=\"{28 + 38 * pin.Row}\" />");
                            Line(sb, 20, "</ConnectionPointOut>");
                            Line(sb, 18, "</OutputVariable>");
                        }
                        Line(sb, 16, "</OutputVariables>");
                        Line(sb, 14, "</FbdObject>");
                        break;
                    }
            }
        }

        Line(sb, 14, "<LdObject xsi:type=\"RightPowerRail\">");
        Line(sb, 16, $"<RelPosition x=\"{railX}\" y=\"0\" />");
        Line(sb, 16, $"<Size x=\"3\" y=\"{38 * rows}\" />");
        for (int r = 0; r < rows; r++)
        {
            Line(sb, 16, "<ConnectionPointIn>");
            Line(sb, 18, $"<RelPosition x=\"0\" y=\"{28 + 38 * r}\" />");
            if (rightIn[r] != 0) Line(sb, 18, $"<Connection refConnectionPointOutId=\"{rightIn[r]}\" />");
            Line(sb, 16, "</ConnectionPointIn>");
        }
        Line(sb, 14, "</LdObject>");
        Line(sb, 12, "</Rung>");
        return sb.ToString();
    }
}

// ======================================================================= program
public static class BnrXmlToLdEngine
{
    // ===== settings =====
    static readonly string[] DefaultInputs =
    {
        "PR_RASM_Mitsubishi"
    };
    const bool EmitLabels = false;   // your original LD has no rung labels; true = labels from the Siemens comments
    const int RailMin = 1413;        // right power rail used by AS for your original LD (0 = use the computed width)

    static IEnumerable<XElement> E(XElement p, string n) => p.Elements().Where(x => x.Name.LocalName == n);
    static XElement? F(XElement p, string n) => E(p, n).FirstOrDefault();

    static string? Ml(XElement unit, string comp)
    {
        var ol = F(unit, "ObjectList");
        var t = ol == null ? null : E(ol, "MultilingualText").FirstOrDefault(x => (string?)x.Attribute("CompositionName") == comp);
        var tol = t == null ? null : F(t, "ObjectList");
        if (tol == null) return null;
        var items = E(tol, "MultilingualTextItem")
            .Select(i => F(i, "AttributeList"))
            .Where(a => a != null)
            .Select(a => (cult: F(a!, "Culture")?.Value ?? "", text: F(a!, "Text")?.Value ?? ""))
            .Where(x => !string.IsNullOrWhiteSpace(x.text)).ToList();
        if (items.Count == 0) return null;
        var pick = items.FirstOrDefault(x => x.cult == "en-US");
        return pick.text ?? items[0].text;
    }

    // valid AS label: letters/digits/_ only, max 32 chars, unique inside the file
    static string MakeLabel(string raw, int index, HashSet<string> usedLabels)
    {
        var chars = raw.Trim().Select(c => char.IsLetterOrDigit(c) && c < 128 ? c : '_').ToArray();
        var sb = new StringBuilder();
        foreach (char c in chars) { if (c == '_' && sb.Length > 0 && sb[sb.Length - 1] == '_') continue; sb.Append(c); }
        string b = sb.ToString().Trim('_');
        if (b.Length == 0) b = "Network_" + index;
        if (char.IsDigit(b[0])) b = "_" + b;
        if (b.Length > 32) b = b.Substring(0, 32).TrimEnd('_');
        string label = b; int n = 2;
        while (!usedLabels.Add(label))
        {
            string suf = "_" + n++;
            label = (b.Length + suf.Length > 32 ? b.Substring(0, 32 - suf.Length) : b) + suf;
        }
        return label;
    }

    static void GenerateFunFile(XElement fb, string outputFolder, string fbName)
    {
        var sections = new List<FunSection>();

        var interfaceSections = fb
            .Descendants()
            .Where(x => x.Name.LocalName == "Section")
            .ToList();

        if (!interfaceSections.Any())
            return;

        foreach (var sec in interfaceSections)
        {
            var section = new FunSection
            {
                Name = (string?)sec.Attribute("Name") ?? ""
            };

            foreach (var member in sec.Elements()
                                      .Where(x => x.Name.LocalName == "Member"))
            {
                string name =
                    (string?)member.Attribute("Name") ?? "";

                string type =
                    GetDataType(member);

                string comment =
                    member.Descendants()
                          .FirstOrDefault(x => x.Name.LocalName == "Comment")
                          ?.Value ?? "";

                if (!string.IsNullOrWhiteSpace(name))
                {
                    section.Members.Add(new FunVariable
                    {
                        Name = name,
                        Type = type,
                        Comment = comment
                    });
                }
            }

            sections.Add(section);
        }

        var sb = new StringBuilder();

        sb.AppendLine(
            "{REDUND_ERROR} FUNCTION_BLOCK " +
            fbName +
            " (*TODO: Add your comment here*) (*$GROUP=User,$CAT=User,$GROUPICON=User.png,$CATICON=User.png*)");

        foreach (var sec in sections)
        {
            string secName = sec.Name.ToUpperInvariant();

            switch (secName)
            {
                case "INPUT":
                    sb.AppendLine("\tVAR_INPUT");
                    break;

                case "OUTPUT":
                    sb.AppendLine("\tVAR_OUTPUT");
                    break;

                case "INOUT":
                    sb.AppendLine("\tVAR_IN_OUT");
                    break;

                case "STATIC":
                case "TEMP":
                case "CONSTANT":
                    sb.AppendLine("\tVAR");
                    break;

                default:
                    continue;
            }

            foreach (var v in sec.Members)
            {
                sb.AppendLine(
                    $"\t\t{v.Name} : {v.Type};" +
                    (string.IsNullOrWhiteSpace(v.Comment)
                        ? ""
                        : $" (*{v.Comment}*)"));
            }

            sb.AppendLine("\tEND_VAR");
        }

        sb.AppendLine("END_FUNCTION_BLOCK");

        string funPath = Path.Combine(outputFolder, fbName + ".fun");

        File.WriteAllText(
            funPath,
            sb.ToString(),
            new UTF8Encoding(false));

        Console.WriteLine("FUN Generated -> " + funPath);
    }

    static string GetDataType(XElement member)
    {
        string? type = (string?)member.Attribute("Datatype");

        if (!string.IsNullOrWhiteSpace(type))
        {
            type = type.Replace("DInt", "DINT")
                       .Replace("Int", "INT")
                       .Replace("Bool", "BOOL")
                       .Replace("Time", "TIME")
                       .Replace("SInt", "SINT");

            if (type.Contains("Array["))
            {
                type = type.Replace("Array[", "ARRAY[")
                           .Replace("] of ", "] OF ");
            }

            return type;
        }

        return "BOOL";
    }

    static void GenerateFunFileFromXml(string downloads, string name)
    {
        string? input = new[]
        {
            Path.Combine(downloads, name),
            Path.Combine(downloads, name + ".xml")
        }.FirstOrDefault(File.Exists)
          ?? Directory.GetFiles(downloads, name + "*")
                      .FirstOrDefault(f => !f.EndsWith("_LD.ld", StringComparison.OrdinalIgnoreCase));

        if (input == null)
            return;

        var doc = XDocument.Load(input);

        // Get FB node
        var fb = doc.Descendants()
                    .FirstOrDefault(e => e.Name.LocalName == "SW.Blocks.FB");

        if (fb == null)
            return;

        // Get FB name
        string fbName =
            F(F(fb, "AttributeList")!, "Name")?.Value ?? "FB";

        // Get Sections node
        var sections = fb.Descendants()
                         .FirstOrDefault(e => e.Name.LocalName == "Sections");

        if (sections == null)
            return;

        GenerateFunFile(sections, downloads, fbName);
    }

    // Integration adapter: runs the original conversion methods in an isolated working folder.
    // The original parser, rung construction, layout and output-generation logic remain unchanged.
    public static IReadOnlyList<string> ConvertFileFromHost(string inputPath, string outputFolder)
    {
        if (!File.Exists(inputPath))
            throw new FileNotFoundException("Input Siemens XML file was not found.", inputPath);

        Directory.CreateDirectory(outputFolder);
        string workingFolder = Path.Combine(Path.GetTempPath(), "PLCConversionSuite_BNR_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workingFolder);
        try
        {
            string inputName = Path.GetFileName(inputPath);
            File.Copy(inputPath, Path.Combine(workingFolder, inputName), true);

            // Preserve the optional hand-authored first-rung companion file supported by the original tool.
            string sourceFolder = Path.GetDirectoryName(Path.GetFullPath(inputPath)) ?? "";
            foreach (string sidecar in Directory.GetFiles(sourceFolder, "*_Rung1.xml"))
                File.Copy(sidecar, Path.Combine(workingFolder, Path.GetFileName(sidecar)), true);

            Convert(workingFolder, inputName);
            GenerateFunFileFromXml(workingFolder, inputName);

            var generated = Directory.GetFiles(workingFolder)
                .Where(f => f.EndsWith("_LD.ld", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".fun", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var copied = new List<string>();
            foreach (string file in generated)
            {
                string destination = Path.Combine(outputFolder, Path.GetFileName(file));
                File.Copy(file, destination, true);
                copied.Add(destination);
            }
            return copied;
        }
        finally
        {
            try { Directory.Delete(workingFolder, true); } catch { }
        }
    }

    public static int RunStandalone(string[] args)
    {
        string downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        Console.WriteLine("Downloads folder: " + downloads);
        int rc = 0;
        foreach (var name in args.Length > 0 ? args : DefaultInputs)
        {
            try
            {
                Convert(downloads, name);
                GenerateFunFileFromXml(downloads, name);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERROR [{name}]: {ex.Message}");
                rc = 1;
            }
        }
        return rc;
    }

    static void Convert(string downloads, string name)
    {
        string? input = new[] { Path.Combine(downloads, name), Path.Combine(downloads, name + ".xml") }.FirstOrDefault(File.Exists)
                        ?? Directory.GetFiles(downloads, name + "*").FirstOrDefault(f => !f.EndsWith("_LD.ld", StringComparison.OrdinalIgnoreCase));
        if (input == null) throw new FileNotFoundException($"input '{name}' not found in {downloads}");
        Console.WriteLine($"Reading  {input}");

        var doc = XDocument.Load(input);
        var fb = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "SW.Blocks.FB" || e.Name.LocalName == "SW.Blocks.FC")
                 ?? throw new InvalidDataException("no SW.Blocks.FB element - not a TIA Openness FB export");
        string fbName = F(F(fb, "AttributeList")!, "Name")?.Value ?? "FB";

        var nets = new List<(XElement unit, XElement flg)>();
        foreach (var unit in E(F(fb, "ObjectList")!, "SW.Blocks.CompileUnit"))
        {
            var ns = F(F(unit, "AttributeList")!, "NetworkSource");
            var flg = ns == null ? null : F(ns, "FlgNet");
            if (flg != null) nets.Add((unit, flg));                 // header-comment unit has an empty NetworkSource
        }

        // pass 1: one common right power rail for all rungs = the widest rung (AlignRightPowerRails)
        int rail = Math.Max(1053, RailMin);
        foreach (var n in nets)
            try { rail = Math.Max(rail, new RungBuilder(n.flg).Natural()); } catch { }
        Console.WriteLine($"Right power rail x = {rail}");

        // pass 2: emit
        var rungs = new List<string>();
        var warnings = new List<string>();
        var usedLabels = new HashSet<string>();
        int ord = 1;

        // optional: hand-made rung 1 (e.g. a network that was commented out in the Siemens project)
        string rung1 = Path.Combine(downloads, fbName + "_Rung1.xml");
        if (File.Exists(rung1)) { rungs.Add(File.ReadAllText(rung1).TrimEnd() + "\r\n"); ord++; }

        int netNo = 0;
        foreach (var n in nets)
        {
            netNo++;
            string title = Ml(n.unit, "Title") ?? "", cmt = Ml(n.unit, "Comment") ?? "";
            string text = title != "" ? title : cmt;
            string label = EmitLabels ? MakeLabel(cmt != "" ? cmt : title, netNo, usedLabels) : "";

            try { rungs.Add(new RungBuilder(n.flg).Emit(ord, label, text, rail)); }
            catch (Exception ex)
            {
                warnings.Add($"network {netNo} '{text}': {ex.Message}");
                rungs.Add(RungBuilder.EmptyRung(ord, label, $"{text} [NOT CONVERTED: {ex.Message}]", rail));
            }
            ord++;
        }

        var sb = new StringBuilder();
        void L(int ind, string s) => sb.Append(' ', ind).Append(s).Append("\r\n");
        L(0, "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        L(0, "<?AutomationStudio FileVersion=\"6.0\"?>");
        L(0, "<Project xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xhtml=\"http://www.w3.org/1999/xhtml\" schemaVersion=\"1.0\" xmlns=\"www.iec.ch/public/TC65SC65BWG7TF10\">");
        L(2, "<FileHeader companyName=\"B&amp;R Industrial Automation GmbH\" productName=\"Automation Studio\" productVersion=\"6.0\" />");
        L(2, "<ContentHeader name=\"\" creationDateTime=\"0001-01-01T00:00:00\">");
        L(4, "<CoordinateInfo />");
        L(4, "<AddData>");
        L(6, "<Data name=\"BR_EditorMode\" handleUnknown=\"implementation\">");
        L(8, "<EditorMode value=\"Default\" />");
        L(6, "</Data>");
        L(6, "<Data name=\"BR_AlignRightPowerRails\" handleUnknown=\"implementation\">");
        L(8, "<AlignRightPowerRails value=\"true\" />");
        L(6, "</Data>");
        L(4, "</AddData>");
        L(2, "</ContentHeader>");
        L(2, "<Types>");
        L(4, "<GlobalNamespace>");
        L(6, $"<FunctionBlock name=\"{System.Security.SecurityElement.Escape(fbName)}\">");
        L(8, "<Documentation xsi:type=\"SimpleText\"><![CDATA[TODO: Add your comment here]]></Documentation>");
        L(8, "<MainBody>");
        L(10, "<BodyContent xsi:type=\"LD\">");
        foreach (var r in rungs) sb.Append(r);
        L(10, "</BodyContent>");
        L(8, "</MainBody>");
        L(6, "</FunctionBlock>");
        L(4, "</GlobalNamespace>");
        L(2, "</Types>");
        L(2, "<Instances />");
        L(0, "</Project>");

        string outPath = Path.Combine(downloads, fbName + "_LD.ld");
        File.WriteAllText(outPath, sb.ToString(), new UTF8Encoding(false));
        Console.WriteLine($"Done: {rungs.Count} rungs ({rungs.Count - warnings.Count} converted, {warnings.Count} not converted) -> {outPath}");
        foreach (var w in warnings) Console.WriteLine("  WARNING " + w);
    }
}