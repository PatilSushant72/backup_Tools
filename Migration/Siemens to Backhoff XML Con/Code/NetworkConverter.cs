using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SiemensToBackhoff;

// ── Shared context ────────────────────────────────────────────────────────────

internal sealed class Tc6Names
{
    public readonly XNamespace Tc6 = "http://www.plcopen.org/xml/tc6_0200";
    public readonly XNamespace Xhtml = "http://www.w3.org/1999/xhtml";
}

internal sealed class LdContext
{
    public int NextId = 1;
    public int NewId() => NextId++;
    public List<string> Warnings { get; } = new();
    public Dictionary<string, string> EdgeBits { get; } = new(StringComparer.Ordinal);
    public int Contacts, Coils, Blocks;
    public int CurrentNetworkTopY = 0;
    public bool NeedsCompareSink;
}

internal readonly record struct Conn(int Ref, string? Fp);

internal sealed class Src
{
    public char Kind;   // 'R' rail | 'V' variable | 'P' part output
    public string Expr = "";
    public string Uid = "";
    public string Pin = "";
}

// ── NetworkConverter ──────────────────────────────────────────────────────────

internal sealed class NetworkConverter
{
    private static readonly Tc6Names N = new();

    private readonly LdContext _ctx;
    private readonly XElement _ld;
    private readonly Network _net;

    // ── Wire graph
    private readonly Dictionary<string, Part> _parts = new();
    private readonly Dictionary<(string, string), List<Src>> _sources = new();
    private readonly Dictionary<(string, string), string> _destVar = new();
    private readonly HashSet<(string, string)> _openOut = new();
    private readonly Dictionary<string, Dictionary<string, string>> _pinNames = new();

    // ── Emission state
    private readonly Dictionary<string, int> _done = new();
    private readonly Dictionary<string, List<Conn>> _orCache = new();
    private readonly HashSet<string> _active = new();
    private readonly HashSet<string> _absorbed = new();                 // contacts folded into an edge-block expression
    private readonly Dictionary<string, (string Clk, string? Contact)> _edgeClk = new();   // edge UId -> CLK variable (+ absorbed last contact)
    private readonly Dictionary<string, string> _edgeCond = new();      // edge-block UId -> combined condition


    // ── Structured rung data (for LayoutEngine)
    private readonly List<Rung> _rungs = new();
    private readonly List<int> _titleIds = new();
    private Rung? _currRung = null;

    // ── Instruction tables ────────────────────────────────────────────────────
    private static readonly Dictionary<string, string> Compare = new()
    { ["Lt"] = "LT", ["Gt"] = "GT", ["Le"] = "LE", ["Ge"] = "GE", ["Eq"] = "EQ", ["Ne"] = "NE" };
    private static readonly Dictionary<string, string> Arith = new()
    {
        ["Add"] = "ADD",
        ["Sub"] = "SUB",
        ["Mul"] = "MUL",
        ["Div"] = "DIV",
        ["Mod"] = "MOD",
        // word / bit operators: TwinCAT IEC operators with the same EN / In2 / In3 -> Out2 shape as ADD
        ["Shl"] = "SHL",
        ["Shr"] = "SHR",
        ["Rol"] = "ROL",
        ["Ror"] = "ROR",
        ["Or"] = "OR",
        ["And"] = "AND",
        ["Xor"] = "XOR"
    };
    // Siemens block-move / string instructions -> TwinCAT library functions (Tc2_System MEMSET / MEMCPY, LEN)
    private static readonly Dictionary<string, string> FuncBlocks = new()
    {
        ["FillBlockI"] = "MEMSET",
        ["FillBlock"] = "MEMSET",
        ["UFillBlock"] = "MEMSET",
        ["MoveBlockI"] = "MEMCPY",
        ["MoveBlock"] = "MEMCPY",
        ["UMoveBlock"] = "MEMCPY",
        ["Chars_TO_Strg"] = "MEMCPY",
        ["LEN"] = "LEN"
    };
    private static readonly Dictionary<string, string> Edge = new()
    { ["PBox"] = "R_TRIG", ["NBox"] = "F_TRIG" };
    private static bool IsMoveLike(string n) => n is "Move" or "Convert";
    private static readonly HashSet<string> Timers = new() { "TON", "TOF", "TP" };
    private static readonly HashSet<string> Counters = new() { "CTU", "CTD", "CTUD" };
    private static readonly HashSet<string> OutPins = new(StringComparer.OrdinalIgnoreCase)
        {"out","eno","q","et","cv","qu","qd","strg",
         "out1","out2","out3","out4","out5","out6","out7","out8","out9"};

    private static bool IsOut(string pin) => OutPins.Contains(pin.Trim());
    private static bool IsBlockKind(string n) =>
        Compare.ContainsKey(n) || Arith.ContainsKey(n) || Edge.ContainsKey(n) ||
        Timers.Contains(n) || Counters.Contains(n) || IsMoveLike(n) || n == "Not" || FuncBlocks.ContainsKey(n);

    public NetworkConverter(LdContext ctx, XElement ld, Network net)
    { _ctx = ctx; _ld = ld; _net = net; }

    // ── Entry ─────────────────────────────────────────────────────────────────
    public void Convert()
    {
        _firstLocalId = _ctx.NextId;                 // elements of THIS network start here
        int startCount = _ld.Elements().Count();
        NormalizeParts();
        foreach (var p in _net.Parts) _parts[p.UId] = p;
        BuildGraph();
        AddInstanceBits();

        PrepareEdgeConditions();      // <-- add
        PrepareNotCoils();
        PrepareNotCompare();

        AddNetworkTitle();

        // Emit all parts in execution order
        foreach (var uid in ExecutionOrder()) Emit(uid);

        // keep each rung together (see LdRungOrder): only the XML order changes, not ids or connections
        var netEls = _ld.Elements().Skip(startCount).ToList();
        if (LdRungOrder.Reorder(netEls))
        {
            foreach (var e in netEls) e.Remove();
            foreach (var e in netEls) _ld.Add(e);
        }

        if (RailOnlyToContacts) RailOnlyFeedsContacts(startCount);

        // Apply layout using structured rung data
        ApplyLayout();
    }

    // TwinCAT-authored files never connect a block or coil straight to the left power rail: only contacts start at the rail.
    // A block that sits directly on the rail (Siemens: rail -> MOVE/EQ/PBox ...) gets a shared "TRUE" contact in front of it.
    private const bool RailOnlyToContacts = false;   // blocks directly on the rail imported fine before; only enable if TwinCAT rejects them
    // false = MEMSET / MEMCPY arguments are the plain variables (as in the TwinCAT-authored file);
    // true  = ADR(dest) and count * SIZEOF(dest), correct for element types wider than one byte
    private const bool UseAdrSizeof = false;
    // Extra TRUE contacts the generator may add (each one is reported as a warning with the network number):
    //  BlockRootJunction  - ParallelBranch starting at a block gets a TRUE contact after the block (TwinCAT only starts branches at contacts)
    //  MergeFanOutJunction - one merge feeding several elements is drawn once into a TRUE contact
    private const bool BlockRootJunction = true;
    private const bool MergeFanOutJunction = true;
    // false = TON/TOF/TP call as before (rung on IN, "BOOL TIME"); true = EN/ENO enabled call (EN <- rung, IN <- TRUE)
    private const bool TimerWithEn = false;

    private void RailOnlyFeedsContacts(int startCount)
    {
        XElement? trueContact = null; int trueId = 0;
        foreach (var e in _ld.Elements().Skip(startCount).ToList())
        {
            var kind = e.Name.LocalName;
            if (kind is "contact" or "vendorElement" or "comment" or "inVariable" or "leftPowerRail") continue;
            var rails = e.Descendants(N.Tc6 + "connection").Where(c => (string?)c.Attribute("refLocalId") == "0").ToList();
            if (rails.Count == 0) continue;
            if (trueContact == null)
            {
                trueId = _ctx.NewId();
                trueContact = new XElement(N.Tc6 + "contact",
                    new XAttribute("localId", trueId),
                    new XAttribute("negated", "false"), new XAttribute("storage", "none"), new XAttribute("edge", "none"),
                    Pos(0, 0),
                    new XElement(N.Tc6 + "connectionPointIn", new XElement(N.Tc6 + "connection", new XAttribute("refLocalId", 0))),
                    new XElement(N.Tc6 + "connectionPointOut"),
                    new XElement(N.Tc6 + "variable", "TRUE"));
                // in front of the consumer and of the inVariables written just before it
                var refs = e.Descendants(N.Tc6 + "connection").Select(c => (string?)c.Attribute("refLocalId")).ToHashSet();
                XElement start = e;
                while (start.PreviousNode is XElement prev && prev.Name.LocalName == "inVariable" &&
                       refs.Contains((string?)prev.Attribute("localId")))
                    start = prev;
                start.AddBeforeSelf(trueContact);
            }
            foreach (var c in rails) c.SetAttributeValue("refLocalId", trueId);
        }
    }
    // Siemens N_TRIG / P_TRIG (call style: CLK, Q and an instance) is handled like the NBox / PBox edge boxes:
    // N_TRIG -> F_TRIG, P_TRIG -> R_TRIG, the instance becomes the edge bit.  T_CONV is the CONVERT box.
    private readonly HashSet<string> _callEdge = new();
    /// <summary>Siemens InRange(MIN, VAL, MAX) = GE(VAL, MIN) followed by LE(VAL, MAX) (power flow in series).</summary>
    private void ExpandInRange()
    {
        foreach (var r in _net.Parts.Where(x => x.Name == "InRange").ToList())
        {
            var ge = new Part { UId = r.UId + "_ge", Name = "Ge" };
            var le = new Part { UId = r.UId + "_le", Name = "Le" };
            foreach (var kv in r.Properties) { ge.Properties[kv.Key] = kv.Value; le.Properties[kv.Key] = kv.Value; }
            int at = _net.Parts.IndexOf(r);
            _net.Parts.RemoveAt(at); _net.Parts.Insert(at, le); _net.Parts.Insert(at, ge);
            foreach (var w in _net.Wires)
            {
                var names = new List<(string Uid, string Pin)>();
                foreach (var (uid, pin) in w.Names)
                {
                    if (uid != r.UId) { names.Add((uid, pin)); continue; }
                    switch (pin.ToLowerInvariant())
                    {
                        case "pre": case "en": names.Add((ge.UId, "pre")); break;
                        case "min": names.Add((ge.UId, "in2")); break;
                        case "val": names.Add((ge.UId, "in1")); names.Add((le.UId, "in1")); break;
                        case "max": names.Add((le.UId, "in2")); break;
                        case "out": names.Add((le.UId, "out")); break;
                        case "eno": names.Add((le.UId, "eno")); break;
                        default: names.Add((uid, pin)); break;
                    }
                }
                w.Names.Clear(); w.Names.AddRange(names);
            }
            var link = new Wire(); link.Names.Add((ge.UId, "out")); link.Names.Add((le.UId, "pre"));
            _net.Wires.Add(link);
        }
    }

    /// <summary>TwinCAT has no INC / DEC box: Siemens Inc(operand) = ADD(operand, 1) -> operand, Dec = SUB(operand, 1).</summary>
    private void ExpandIncDec()
    {
        foreach (var p in _net.Parts.Where(x => x.Name is "Inc" or "Dec").ToList())
        {
            var oneKey = p.UId + "_one";
            _net.Access[oneKey] = "1";
            var dest = new Wire(); dest.Idents.Add(""); dest.Names.Add((p.UId, "out"));
            string? operandIdent = null;
            foreach (var w in _net.Wires)
            {
                for (int i = 0; i < w.Names.Count; i++)
                    if (w.Names[i].Uid == p.UId && w.Names[i].Pin.Equals("operand", StringComparison.OrdinalIgnoreCase))
                    {
                        w.Names[i] = (p.UId, "in1");
                        operandIdent ??= w.Idents.FirstOrDefault();
                    }
            }
            dest.Idents.Clear();
            if (operandIdent != null) dest.Idents.Add(operandIdent);
            var one = new Wire(); one.Idents.Add(oneKey); one.Names.Add((p.UId, "in2"));
            _net.Wires.Add(dest); _net.Wires.Add(one);
            p.Name = p.Name == "Inc" ? "Add" : "Sub";
        }
    }

    private void NormalizeParts()
    {
        ExpandInRange();
        ExpandIncDec();
        foreach (var p in _net.Parts)
        {
            if (p.Name is "N_TRIG" or "P_TRIG")
            {
                _callEdge.Add(p.UId);
                p.Name = p.Name == "N_TRIG" ? "NBox" : "PBox";
            }
            else if (p.Name == "T_CONV") p.Name = "Convert";
        }

        if (_callEdge.Count == 0) return;
        foreach (var w in _net.Wires)
            for (int i = 0; i < w.Names.Count; i++)
                if (_callEdge.Contains(w.Names[i].Uid) && w.Names[i].Pin.Equals("clk", StringComparison.OrdinalIgnoreCase))
                    w.Names[i] = (w.Names[i].Uid, "in");
    }

    private void AddInstanceBits()
    {
        foreach (var uid in _callEdge)
            if (_parts[uid].InstanceName.Length > 0)
                AddSrc((uid, "bit"), new Src { Kind = 'V', Expr = _parts[uid].InstanceName });
    }

    // ── Edge blocks: fold the contact chain in front of P_TRIG/N_TRIG into one expression ──
    private void PrepareEdgeConditions()
    {
        foreach (var p in _net.Parts.Where(x => Edge.ContainsKey(x.Name)))
        {
            // Preferred form: the LAST contact in front of the edge block becomes the plain CLK variable,
            // the contacts before it stay as contact elements and feed EN (no AND / NOT expression).
            if (_sources.TryGetValue((p.UId, "in"), out var inl) && inl.Count == 1)
            {
                var s0 = inl[0];
                if (s0.Kind == 'V' && s0.Expr.Length > 0)
                { _edgeClk[p.UId] = (s0.Expr, null); continue; }

                if (s0.Kind == 'P' && _parts.TryGetValue(s0.Uid, out var lc) && lc.Name == "Contact" &&
                    ConsumerCount(lc.UId) == 1 &&
                    _sources.TryGetValue((lc.UId, "operand"), out var opl) &&
                    opl.FirstOrDefault(x => x.Kind == 'V' && x.Expr.Length > 0) is { } ov)
                {
                    var term = lc.NegatedPins.Contains("operand") ? $"NOT {ov.Expr}" : ov.Expr;
                    _edgeClk[p.UId] = (term, lc.UId);
                    _absorbed.Add(lc.UId);
                    continue;
                }
            }

            var absorbed = new List<string>();
            var cond = BuildCond(p.UId, "in", absorbed, 0);
            if (cond == null) continue;                       // not pure contacts: keep old behaviour

            _edgeCond[p.UId] = cond;
            // contacts stay as real contact elements (they feed EN, like TwinCAT draws it)
        }
    }

    // ── NOT with merged (OR) input that only feeds coils ─────────────────────────
    // TwinCAT accepts several <connection> entries on contacts, coils and EN pins, but NOT on a data input such as
    // NOT.In1 (import fails with "Object reference is not set to an instance of an object").
    // NOT(OR(...)) -> coil is therefore written as a negated coil that takes the merged connections itself.
    private readonly HashSet<string> _notAsCoil = new();
    private readonly Dictionary<string, string> _notFold = new();      // NOT uid -> compare uid it inverts
    private readonly HashSet<string> _invCmp = new();                  // compare uids written with the opposite relation
    private static string InvertCompare(string siemens) => siemens switch
    { "Eq" => "Ne", "Ne" => "Eq", "Lt" => "Ge", "Ge" => "Lt", "Gt" => "Le", "Le" => "Gt", _ => siemens };

    /// <summary>NOT(compare) that only feeds power flow = the opposite compare block. TwinCAT cannot take a
    /// compare's data output (Out2) into a NOT and then use the NOT as rung power flow.</summary>
    private void PrepareNotCompare()
    {
        foreach (var n in _net.Parts.Where(x => x.Name == "Not" && !_absorbed.Contains(x.UId)))
        {
            if (!_sources.TryGetValue((n.UId, "in"), out var ins) || ins.Count != 1 || ins[0].Kind != 'P') continue;
            if (!_parts.TryGetValue(ins[0].Uid, out var cp) || !Compare.ContainsKey(cp.Name)) continue;
            if (!ins[0].Pin.Equals("out", StringComparison.OrdinalIgnoreCase)) continue;
            if (_invCmp.Contains(cp.UId)) continue;
            var cmpConsumers = _sources.Where(kv => kv.Value.Any(v => v.Kind == 'P' && v.Uid == cp.UId && v.Pin.Equals("out", StringComparison.OrdinalIgnoreCase))).Select(kv => kv.Key).ToList();
            if (cmpConsumers.Count == 0 || !cmpConsumers.Where(c => c.Item1 != n.UId).All(c => IsPowerFlowInput(c.Item1, c.Item2))) continue;
            var consumers = _sources.Where(kv => kv.Value.Any(v => v.Kind == 'P' && v.Uid == n.UId)).Select(kv => kv.Key).ToList();
            if (consumers.Count == 0 || !consumers.All(c => IsPowerFlowInput(c.Item1, c.Item2))) continue;

            var target = cp.UId;
            if (cmpConsumers.Count > 1)
            {
                // the compare also drives other power flow: keep it as it is and draw a second, opposite compare for the NOT
                var cloneId = cp.UId + "_not";
                var clone = new Part { UId = cloneId, Name = cp.Name };
                foreach (var kv in cp.Properties) clone.Properties[kv.Key] = kv.Value;
                foreach (var np in cp.NegatedPins) clone.NegatedPins.Add(np);
                _parts[cloneId] = clone;
                if (_pinNames.TryGetValue(cp.UId, out var pm)) _pinNames[cloneId] = new Dictionary<string, string>(pm);
                foreach (var kv in _sources.Where(k => k.Key.Item1 == cp.UId).ToList())
                    _sources[(cloneId, kv.Key.Item2)] = new List<Src>(kv.Value);
                target = cloneId;
            }
            _notFold[n.UId] = target; _invCmp.Add(target); _absorbed.Add(n.UId);
        }
    }

    private void PrepareNotCoils()
    {
        foreach (var n in _net.Parts.Where(x => x.Name == "Not"))
        {
            if (!_sources.TryGetValue((n.UId, "in"), out var ins)) continue;
            bool merged = ins.Count > 1 || (ins.Count == 1 && ins[0].Kind == 'P' &&
                          _parts.TryGetValue(ins[0].Uid, out var sp) && sp.Name == "O");
            if (!merged) continue;

            // consumers of the NOT output
            var consumers = _sources.Where(kv => kv.Value.Any(v => v.Kind == 'P' && v.Uid == n.UId)).Select(kv => kv.Key).ToList();
            if (consumers.Count == 0 ||
                !consumers.All(c => _parts.TryGetValue(c.Item1, out var cp) && cp.Name is "Coil" or "SCoil" or "RCoil" && c.Item2 == "in"))
            {
                _ctx.Warnings.Add($"Network {_net.Index}: NOT (UId {n.UId}) has a merged input and does not only feed coils - TwinCAT may reject it; verify.");
                continue;
            }
            _notAsCoil.Add(n.UId);
            _absorbed.Add(n.UId);
        }
    }

    private int ConsumerCount(string uid) =>
        _sources.Values.Sum(l => l.Count(s => s.Kind == 'P' && s.Uid == uid));

    private string? BuildCond(string uid, string pin, List<string> absorbed, int depth)
    {
        if (depth > 50) return null;
        if (!_sources.TryGetValue((uid, pin.ToLowerInvariant()), out var list) || list.Count != 1)
            return null;

        var s = list[0];
        if (s.Kind == 'R') return "TRUE";
        if (s.Kind == 'V') return s.Expr.Length > 0 ? s.Expr : null;
        if (!_parts.TryGetValue(s.Uid, out var src)) return null;

        if (src.Name == "Contact")
        {
            if (!_sources.TryGetValue((src.UId, "operand"), out var ol)) return null;
            var v = ol.FirstOrDefault(x => x.Kind == 'V' && x.Expr.Length > 0);
            if (v == null) return null;

            var up = BuildCond(src.UId, "in", absorbed, depth + 1);
            if (up == null) return null;

            var term = src.NegatedPins.Contains("operand") ? $"(NOT {v.Expr})" : v.Expr;
            absorbed.Add(src.UId);
            return up == "TRUE" ? term : $"{up} AND {term}";
        }

        if (src.Name == "O")
        {
            var parts = new List<string>();
            if (_pinNames.TryGetValue(src.UId, out var names))
                foreach (var k in names.Keys.Where(k => k.StartsWith("in", StringComparison.Ordinal))
                             .OrderBy(k => int.TryParse(Regex.Replace(k, @"\D", ""), out var n) ? n : 0))
                {
                    var c = BuildCond(src.UId, k, absorbed, depth + 1);
                    if (c == null) return null;
                    parts.Add(c);
                }
            if (parts.Count == 0) return null;
            if (parts.Contains("TRUE")) return "TRUE";
            return parts.Count == 1 ? parts[0] : "(" + string.Join(" OR ", parts.Select(x => $"({x})")) + ")";
        }

        return null;   // compare / block / anything else: not collapsible
    }
    // ── Layout ────────────────────────────────────────────────────────────────
    // TwinCAT-authored LD1 files carry x=0,y=0 everywhere and lay the network out from the connections.
    private const bool UseCoordinateLayout = false;

    private const bool EmitOutputParamTypes = false;   // TwinCAT does not write it

    // ParallelBranch metadata, written the way TwinCAT exports it (verified against the corrected Cycle Start export):
    //   * only for the OUTERMOST merge (an inner OR that feeds another OR gets none - TwinCAT: "localId already added"),
    //   * only when a branch contains a compare / edge block (contact-only ORs import fine without it),
    //   * only when the branches share a real upstream element (the power rail does not count),
    //   * one <Tree> per merge connection.
    private const bool EmitParallelBranch = true;

    // true: unused BOOL result of a compare block (Out2) and unused TON/TOF ET are assigned to 'tDummy',
    // as in the TwinCAT-authored file. 'tDummy : BOOL / TIME' must then exist (global or local).
    private const bool UseDummyForUnusedOutputs = false;

    private void ApplyLayout()
    {
        if (!UseCoordinateLayout) return;
        int topY = _ctx.CurrentNetworkTopY;

        // Titles sit top-left; the ladder tree (Series / Parallel / Element, as in the Rockwell converter)
        // decides where everything else goes.
        var posMap = new Dictionary<int, (int X, int Y)>();
        foreach (var tid in _titleIds) posMap[tid] = (Grid.RailX, topY);

        var tree = new LadderTree(_net, IsOut).Build();
        var (_, treeH) = TreeLayout.Run(tree, Grid.StartX, topY + Grid.TitleH,
            SizeOfElement,
            (uid, x, y) => PlaceElement(uid, x, y, posMap),
            BranchRank);

        foreach (var child in _ld.Elements())
        {
            var lidStr = child.Attribute("localId")?.Value;
            if (lidStr == null || !int.TryParse(lidStr, out var lid)) continue;
            if (lid < _firstLocalId) continue;                 // earlier networks keep their own positions
            var posEl = child.Element(N.Tc6 + "position");
            if (posEl == null) continue;

            if (posMap.TryGetValue(lid, out var xy))
            { posEl.SetAttributeValue("x", xy.X); posEl.SetAttributeValue("y", xy.Y); }
            else
            { posEl.SetAttributeValue("x", 0); posEl.SetAttributeValue("y", topY); }
        }

        // Advance global Y below the tallest thing in this network
        int bottom = Math.Max(topY + Grid.TitleH + treeH, posMap.Count > 0 ? posMap.Values.Max(v => v.Y) + Grid.RowH : topY);
        _ctx.CurrentNetworkTopY = bottom + Grid.NetGap;
    }

    // ── tree layout helpers ───────────────────────────────────────────────────
    private int _firstLocalId;
    private readonly Dictionary<string, List<int>> _blockInVars = new();   // block UId -> its inVariable localIds

    private bool IsDrawn(string uid) => _done.ContainsKey(uid) && !_absorbed.Contains(uid);

    private (int W, int H) SizeOfElement(string uid)
    {
        if (!IsDrawn(uid) || !_parts.TryGetValue(uid, out var p)) return (0, 0);
        if (p.Name == "O") return (0, 0);

        if (_blockInVars.TryGetValue(uid, out var iv) && IsBlockKind(p.Name))
            return (iv.Count > 0 ? 2 * Grid.ContactW : Grid.ContactW, (iv.Count >= 2 ? 2 : 1) * Grid.RowH);

        return (Grid.ContactW, Grid.RowH);
    }

    private void PlaceElement(string uid, int x, int y, Dictionary<int, (int X, int Y)> pos)
    {
        var id = _done[uid];
        if (pos.ContainsKey(id)) return;                      // already placed through another branch

        if (_blockInVars.TryGetValue(uid, out var iv) && iv.Count > 0)
        {
            for (int i = 0; i < iv.Count; i++)
                pos[iv[i]] = (x, y + i * (Grid.RowH / 2));
            pos[id] = (x + Grid.ContactW, y);
        }
        else
            pos[id] = (x, y);
    }

    // coil branches above block branches (MOVE, ...): coil first, then MOVE
    private int BranchRank(LadderNode n)
    {
        while (n.Kind != LadderNode.NodeKind.Element)
            n = n.Kind == LadderNode.NodeKind.Series ? n.Items[^1] : n.Items[0];
        return _parts.TryGetValue(n.Uid, out var p) && p.Name is "Coil" or "SCoil" or "RCoil" ? 0 : 1;
    }

    // ── Rung tracking ─────────────────────────────────────────────────────────

    private Rung CurrentRung()
    {
        if (_currRung == null) { _currRung = new Rung(); }
        return _currRung;
    }

    /// <summary>
    /// When we see a coil, it ends the current rung's contact chain and starts the outputs.
    /// When we see a block (MOVE etc.) after a coil, it's a parallel output on the same rung.
    /// When we see a contact after outputs have started, we need a new rung.
    /// </summary>
    private bool _rungInOutputPhase = false;

    private void OnContactEmitted(int localId, string variable, bool negated, string partUId)
    {
        AddToRung(new RungContact { LocalId = localId, Variable = variable, Negated = negated }, partUId, false);
    }

    // Contacts and series/parallel blocks share one placement rule: members of an OR go in one column.
    private void AddToRung(RungContact item, string partUId, bool _)
    {
        int localId = item.LocalId;
        // A contact after outputs → new rung
        if (_rungInOutputPhase)
        {
            if (_currRung != null) _rungs.Add(_currRung);
            _currRung = new Rung();
            _rungInOutputPhase = false;
        }

        // Check if this contact is part of an OR group.
        // An OR group = multiple contacts all feeding into an O (OR) part.
        // We detect this by checking if this part's output goes to an O part.
        string? orUid = FindOrPartForContact(partUId);

        if (orUid != null)
        {
            // Find or create the OR group for this OR part uid
            var rung = CurrentRung();
            // Check if an existing group already represents this OR
            var existingGroup = rung.ContactGroups.FirstOrDefault(g => g.Any(c => FindOrPartForContact(_done.FirstOrDefault(kv => kv.Value == c.LocalId).Key ?? "") == orUid));

            if (existingGroup == null)
            {
                existingGroup = new List<RungContact>();
                rung.ContactGroups.Add(existingGroup);
            }
            existingGroup.Add(item);
        }
        else
        {
            // Regular series contact — its own single-element group
            CurrentRung().ContactGroups.Add(new List<RungContact> { item });
        }
    }

    private void OnCoilEmitted(int localId, string variable)
    {
        var rung = CurrentRung();
        rung.Outputs.Insert(0, new RungOutput { LocalId = localId, Kind = ElementKind.Coil });
        _rungInOutputPhase = true;
    }

    private void OnBlockEmitted(int localId, List<int> inVarIds, string partUId)
    {
        if (!_rungInOutputPhase)
        {
            // Block in the chain (compare, NOT as gate): placed like a contact, so a compare that is one
            // branch of an OR ends up in the OR column instead of in series.
            AddToRung(new RungContact { LocalId = localId, Variable = "", Negated = false, ExtraIds = inVarIds, IsBlock = true },
                      partUId, true);
        }
        else
        {
            // Parallel output — add to current rung's outputs
            CurrentRung().Outputs.Add(new RungOutput
            { LocalId = localId, Kind = ElementKind.Block, ExtraIds = inVarIds });
        }
        // blocks don't flip _rungInOutputPhase — next contact still = new rung
    }

    /// <summary>
    /// Returns the UId of the O (OR) part that this contact feeds into, or null if it feeds
    /// directly into another contact/coil/block (series).
    /// </summary>
    private string? FindOrPartForContact(string contactUId)
    {
        if (string.IsNullOrEmpty(contactUId)) return null;
        // Find a wire where this contact's "out" pin feeds into an O part's "in*" pin
        foreach (var wire in _net.Wires)
        {
            bool hasContactOut = wire.Names.Any(n => n.Uid == contactUId && n.Pin == "out");
            if (!hasContactOut) continue;
            foreach (var (uid, pin) in wire.Names)
            {
                if (uid == contactUId) continue;
                if (_parts.TryGetValue(uid, out var p) && p.Name == "O")
                    return uid;
            }
        }
        return null;
    }

    // ── Graph building ────────────────────────────────────────────────────────
    private void BuildGraph()
    {
        foreach (var w in _net.Wires)
        {
            var outs = w.Names.Where(n => IsOut(n.Pin)).ToList();
            var ins = w.Names.Where(n => !IsOut(n.Pin)).ToList();

            foreach (var (uid, pin) in w.Names)
            {
                if (!_pinNames.TryGetValue(uid, out var map))
                    _pinNames[uid] = map = new Dictionary<string, string>();
                map[pin.ToLowerInvariant()] = pin;
            }

            if (outs.Count > 0)
            {
                foreach (var o in outs)
                {
                    var key = (o.Uid, o.Pin.ToLowerInvariant());
                    foreach (var iu in w.Idents)
                    {
                        if (_net.Access.TryGetValue(iu, out var expr) &&
                            !string.IsNullOrWhiteSpace(expr))
                        {
                            _destVar[key] = expr;
                        }
                    }
                    if (w.OpenCon) _openOut.Add(key);
                }
                foreach (var i in ins)
                    foreach (var o in outs)
                        AddSrc(i, new Src { Kind = 'P', Uid = o.Uid, Pin = o.Pin });
            }
            else
            {
                var srcs = new List<Src>();
                if (w.Powerrail) srcs.Add(new Src { Kind = 'R' });
                foreach (var iu in w.Idents)
                    srcs.Add(new Src { Kind = 'V', Expr = _net.Access.TryGetValue(iu, out var e) ? e : "" });
                foreach (var i in ins)
                    foreach (var s in srcs)
                        AddSrc(i, s);
            }
        }
    }

    private void AddSrc((string Uid, string Pin) t, Src s)
    {
        var key = (t.Uid, t.Pin.ToLowerInvariant());
        if (!_sources.TryGetValue(key, out var list)) _sources[key] = list = new();
        list.Add(s);
    }

    // ── Network title ─────────────────────────────────────────────────────────
    private void AddNetworkTitle()
    {
        bool hasLogic = _net.Parts.Any(p =>
            IsBlockKind(p.Name) || p.Name is "Contact" or "Coil" or "SCoil" or "RCoil");

        var lines = new List<string>();
        if (hasLogic || _net.Comment.Length == 0) lines.Add(_net.Title);
        if (_net.Comment.Length > 0 &&
            !_net.Comment.Trim().Equals(_net.Title.Trim(), StringComparison.OrdinalIgnoreCase))
            lines.Add(_net.Comment);

        if (lines.Count > 0)
        {
            var cid = _ctx.NewId();
            _ld.Add(new XElement(N.Tc6 + "comment",
                new XAttribute("localId", cid),
                new XAttribute("height", 0), new XAttribute("width", 0),
                Pos(0, 0),
                new XElement(N.Tc6 + "content",
                    new XElement(N.Xhtml + "xhtml", string.Join("\n", lines)))));
            _titleIds.Add(cid);
        }

        var vid = _ctx.NewId();
        _ld.Add(new XElement(N.Tc6 + "vendorElement",
            new XAttribute("localId", vid),
            Pos(0, 0),
            new XElement(N.Tc6 + "alternativeText", new XElement(N.Xhtml + "xhtml")),
            new XElement(N.Tc6 + "addData",
                new XElement(N.Tc6 + "data",
                    new XAttribute("name", "http://www.3s-software.com/plcopenxml/fbdelementtype"),
                    new XAttribute("handleUnknown", "implementation"),
                    new XElement("ElementType", "networktitle")))));
        _titleIds.Add(vid);
    }

    private static XElement Pos(int x, int y) => new(N.Tc6 + "position", new XAttribute("x", x), new XAttribute("y", y));

    // ── Connections ───────────────────────────────────────────────────────────
    private List<Conn> Conns(string uid, string pin)
    {
        var res = new List<Conn>();
        if (!_sources.TryGetValue((uid, pin.ToLowerInvariant()), out var list)) return res;
        foreach (var s in list)
        {
            if (s.Kind == 'R') res.Add(new Conn(0, null));
            else if (s.Kind == 'V') res.Add(new Conn(InVariable(s.Expr), null));
            else if (_parts.TryGetValue(s.Uid, out var p))
            {
                if (p.Name == "O") res.AddRange(OrConns(p));
                else if (p.Name is "Coil" or "SCoil" or "RCoil")
                {
                    // Siemens lets a coil's output feed the next coil (coil chain). Power flow passes through a coil
                    // unchanged, and TwinCAT has no coil-after-coil: the next coil gets the same input (parallel output).
                    if (_active.Add("coilthru:" + p.UId))
                    {
                        res.AddRange(Conns(p.UId, "in"));
                        _active.Remove("coilthru:" + p.UId);
                    }
                }
                else if (_notAsCoil.Contains(p.UId))
                {
                    res.AddRange(Conns(p.UId, "in"));      // the consuming coil is written negated
                }
                else if (_notFold.TryGetValue(p.UId, out var foldCmp) && _parts.TryGetValue(foldCmp, out var foldPart))
                {
                    Emit(foldCmp);                          // NOT(compare) -> the opposite compare block, power flow on ENO
                    if (_done.TryGetValue(foldCmp, out var fid)) res.Add(new Conn(fid, "ENO"));
                }
                else
                {
                    Emit(p.UId);
                    if (_done.TryGetValue(p.UId, out var id))
                    {
                        // Preserve the actual output pin for data connections (Compare.out -> Out2,
                        // PBox/NBox.out -> Q). ENO is used only when an output feeds a ladder
                        // power-flow input. The old code forced every Compare/Edge output to ENO,
                        // including data operands such as in1/in2, which changes the logic.
                        string? formalParameter = IsBlockKind(p.Name)
                            ? OutPinMap(p, s.Pin)
                            : null;

                        if ((Compare.ContainsKey(p.Name) || Edge.ContainsKey(p.Name)) &&
                            IsPowerFlowInput(uid, pin))
                        {
                            formalParameter = "ENO";
                        }

                        res.Add(new Conn(id, formalParameter));
                    }
                }
            }
        }
        return Dedupe(res);
    }

    /// <summary>
    /// True when the target pin represents LD power-flow/enable rather than a data operand.
    /// Compare and edge-box outputs can feed either rung logic (ENO) or a data operand (Out2/Q).
    /// </summary>
    private bool IsPowerFlowInput(string targetUid, string targetPin)
    {
        if (!_parts.TryGetValue(targetUid, out var target))
            return false;

        var pin = targetPin.Trim().ToLowerInvariant();

        if (target.Name is "Contact" or "Coil" or "SCoil" or "RCoil")
            return pin == "in";

        if (target.Name == "O")
            return pin.StartsWith("in", StringComparison.Ordinal);

        if (Compare.ContainsKey(target.Name))
            return pin is "pre" or "en";

        if (IsMoveLike(target.Name) || Arith.ContainsKey(target.Name) || FuncBlocks.ContainsKey(target.Name))
            return pin == "en";

        // FIX: the CLK pin of R_TRIG / F_TRIG (Siemens PBox / NBox / P_TRIG / N_TRIG "in") is driven by the
        // compare block's ENO in TwinCAT-authored files, not by its Out2 data pin.
        if (Edge.ContainsKey(target.Name))
            return pin is "in" or "clk";

        return false;
    }

    private static List<Conn> Dedupe(IEnumerable<Conn> src)
    {
        var seen = new HashSet<Conn>(); var res = new List<Conn>();
        foreach (var c in src) if (seen.Add(c)) res.Add(c);
        return res;
    }

    // Track inVariables created for the current block emission
    private readonly List<int> _currentBlockInVars = new();

    private int InVariable(string expr)
    {
        var id = _ctx.NewId();
        _ld.Add(new XElement(N.Tc6 + "inVariable",
            new XAttribute("localId", id),
            Pos(0, 0),
            new XElement(N.Tc6 + "connectionPointOut"),
            new XElement(N.Tc6 + "expression", expr)));
        _currentBlockInVars.Add(id);
        return id;
    }

    private static XElement CPI(IEnumerable<Conn> conns)
    {
        var cpi = new XElement(N.Tc6 + "connectionPointIn");
        foreach (var c in conns)
        {
            var e = new XElement(N.Tc6 + "connection", new XAttribute("refLocalId", c.Ref));
            if (!string.IsNullOrEmpty(c.Fp)) e.Add(new XAttribute("formalParameter", c.Fp));
            cpi.Add(e);
        }
        return cpi;
    }

    // ── OR / parallel branches ────────────────────────────────────────────────
    private static string PrimaryPin(Part p) => p.Name switch
    {
        "Contact" or "Coil" or "SCoil" or "RCoil" or "Not" => "in",
        "Move" or "Convert" => "en",
        var n when Compare.ContainsKey(n) => "pre",
        var n when Edge.ContainsKey(n) => "in",
        _ => "IN"
    };

    private List<string> Chain(Src? start)
    {
        var chain = new List<string>(); var cur = start;
        while (cur != null && chain.Count < 500)
        {
            if (cur.Kind == 'R') { chain.Add("RAIL"); break; }
            if (cur.Kind != 'P') break;
            chain.Add(cur.Uid);
            if (!_parts.TryGetValue(cur.Uid, out var p) || p.Name == "O") break;
            cur = _sources.TryGetValue((cur.Uid, PrimaryPin(p).ToLowerInvariant()), out var nx)
                && nx.Count > 0 ? nx[0] : null;
        }
        return chain;
    }

    //private List<Conn> OrConns(Part o)
    //{
    //    //if (_orCache.TryGetValue(o.UId, out var cached)) return cached;

    //    //var inPins = new List<string>();
    //    //if (_pinNames.TryGetValue(o.UId, out var names))
    //    //    inPins = names.Keys
    //    //        .Where(k => k.StartsWith("in", StringComparison.Ordinal))
    //    //        .OrderBy(k => int.TryParse(Regex.Replace(k, @"\D", ""), out var n) ? n : 0)
    //    //        .ToList();

    //    //var branches = new List<List<Conn>>(); var all = new List<Conn>();
    //    //foreach (var k in inPins) { var bc = Conns(o.UId, k); branches.Add(bc); all.AddRange(bc); }
    //    //var res = Dedupe(all); _orCache[o.UId] = res;
    //    //if (res.Any(c => c.Fp != null)) AddParallelBranch(o, inPins, branches);
    //    //return res;
    //    if (_orCache.TryGetValue(o.UId, out var cached)) return cached;

    //    var inPins = new List<string>();
    //    if (_pinNames.TryGetValue(o.UId, out var names))
    //        inPins = names.Keys
    //            .Where(k => k.StartsWith("in", StringComparison.Ordinal))
    //            .OrderBy(k => int.TryParse(Regex.Replace(k, @"\D", ""), out var n) ? n : 0)
    //            .ToList();

    //    var branches = new List<List<Conn>>(); var all = new List<Conn>();
    //    foreach (var k in inPins) { var bc = Conns(o.UId, k); branches.Add(bc); all.AddRange(bc); }
    //    var res = Dedupe(all);

    //    // If a ParallelBranch element is created, everything after the merge must
    //    // reference the ParallelBranch itself, not the individual branch ends.
    //    if (res.Any(c => c.Fp != null))
    //    {
    //        int pbId = AddParallelBranch(o, inPins, branches);
    //        res = new List<Conn> { new Conn(pbId, null) };
    //    }

    //    _orCache[o.UId] = res;
    //    return res;
    //}

    private List<Conn> OrConns(Part o)
    {
        if (_orCache.TryGetValue(o.UId, out var cached))
            return cached;

        var inPins = new List<string>();

        if (_pinNames.TryGetValue(o.UId, out var names))
        {
            inPins = names.Keys
                .Where(k => k.StartsWith(
                    "in", StringComparison.Ordinal))
                .OrderBy(k =>
                    int.TryParse(
                        Regex.Replace(k, @"\D", ""),
                        out var n) ? n : 0)
                .ToList();
        }

        // Resolve each input pin separately so the branch structure
        // is retained for ParallelBranch metadata.
        var branches = new List<List<Conn>>();

        foreach (var pin in inPins)
            branches.Add(Conns(o.UId, pin));

        // Preserve the real ladder connections. These connections
        // must remain on the downstream consumer.
        var result = Dedupe(
            branches.SelectMany(branch => branch)).ToList();

        _orCache[o.UId] = result;

        // Add Beckhoff's branch-layout metadata without changing
        // the actual connections returned to the consumer.
        // FIX: ParallelBranch vendorElements are NOT needed for import. TwinCAT rebuilds the OR from the
        // multiple <connection> entries on the consumer, and writes its own ParallelBranch on export.
        // Emitting them here (esp. with nested ORs) made TwinCAT add the same element twice:
        // "The instance with the localId '90' was already added".
        if (EmitParallelBranch &&
            inPins.Count > 1 &&
            branches.Any(branch => branch.Count > 0) &&
            !IsNestedMerge(o) &&
            MergeContainsBlock(o))
        {
            // Branches that share a deeper contact than the merge's overall root (e.g. "contact OR compare" after the same
            // series contacts, in parallel with a third branch from the rail) are a nested parallel in TwinCAT:
            // an inner ParallelBranch (mode seq) for that group, whose connections form ONE tree of the outer branch.
            List<int>? innerGroup = null;
            if (NestedBranches && CommonRoot(o) != null)
            {
                innerGroup = FindInnerGroup(o, inPins, branches, out var innerRoot);
                if (innerGroup != null && innerRoot != null && _done.TryGetValue(innerRoot, out var innerRootRef))
                    AddInnerBranch(o, innerRootRef, innerGroup, branches);
                else innerGroup = null;
            }

            if (AddParallelBranch(o, inPins, branches, innerGroup) >= 0) _pbAdded.Add(o.UId);

            // A merge that needs a ParallelBranch and feeds several elements: TwinCAT adds the branch elements once
            // per consumer ("localId already added"). Draw the merge once into a junction contact (TRUE) and let
            // every consumer connect to that single contact; the logic does not change.
            if (MergeFanOutJunction && _pbAdded.Contains(o.UId) && (MergeConsumerCount(o) > 1 || MergeFeedsBlock(o)))
            {
                _ctx.Warnings.Add($"Network {_net.Index}: TRUE contact added after the merge (UId {o.UId}) because it feeds {(MergeFeedsBlock(o) ? "a block" : MergeConsumerCount(o) + " elements")} (MergeFanOutJunction).");
                var jid = _ctx.NewId();
                _ld.Add(new XElement(N.Tc6 + "contact",
                    new XAttribute("localId", jid),
                    new XAttribute("negated", "false"), new XAttribute("storage", "none"), new XAttribute("edge", "none"),
                    Pos(0, 0), CPI(result),
                    new XElement(N.Tc6 + "connectionPointOut"),
                    new XElement(N.Tc6 + "variable", "TRUE")));
                _ctx.Contacts++;
                result = new List<Conn> { new Conn(jid, null) };
                _orCache[o.UId] = result;
            }
        }

        return result;
    }

    private readonly HashSet<string> _pbAdded = new();

    /// <summary>True when the merge output drives the input of a block (ADD, MOVE, compare ...): TwinCAT adds the
    /// branch blocks' inputs twice there ("localId already added"), so the merge ends in a contact first.</summary>
    private bool MergeFeedsBlock(Part o) =>
        _sources.Where(kv => kv.Value.Any(v => v.Kind == 'P' && v.Uid == o.UId))
                .Any(kv => _parts.TryGetValue(kv.Key.Item1, out var cp) && IsBlockKind(cp.Name) && cp.Name != "Not");

    /// <summary>Number of distinct input pins (of other parts) that read this OR's output.</summary>
    private int MergeConsumerCount(Part o) =>
        _sources.Where(kv => kv.Value.Any(v => v.Kind == 'P' && v.Uid == o.UId)).Select(kv => kv.Key).Distinct().Count();

    // ── ParallelBranch helpers ─────────────────────────────────────────────────
    /// <summary>True when the output of this OR (through series contacts) feeds another OR.</summary>
    private bool IsNestedMerge(Part o)
    {
        var seen = new HashSet<string>();
        bool Walk(string uid)
        {
            if (!seen.Add(uid)) return false;
            foreach (var w in _net.Wires.Where(x => x.Names.Any(n => n.Uid == uid && IsOut(n.Pin))))
                foreach (var (cu, cp) in w.Names)
                {
                    if (cu == uid || IsOut(cp) || !_parts.TryGetValue(cu, out var c)) continue;
                    if (c.Name == "O") return true;
                    if (c.Name == "Contact" && Walk(cu)) return true;      // contact in series, keep following
                }
            return false;
        }
        return Walk(o.UId);
    }

    /// <summary>All parts upstream of an input pin (through every pin of an OR, the primary pin of anything else).</summary>
    private List<string> Upstream(string uid, string pin)
    {
        var res = new List<string>(); var seen = new HashSet<string>();
        var q = new Queue<(string, string)>(); q.Enqueue((uid, pin));
        while (q.Count > 0)
        {
            var (u, pn) = q.Dequeue();
            if (!_sources.TryGetValue((u, pn.ToLowerInvariant()), out var list)) continue;
            foreach (var s in list)
            {
                if (s.Kind != 'P' || !seen.Add(s.Uid)) continue;
                res.Add(s.Uid);
                if (!_parts.TryGetValue(s.Uid, out var sp)) continue;
                if (sp.Name == "O")
                {
                    if (_pinNames.TryGetValue(sp.UId, out var nm))
                        foreach (var k in nm.Keys.Where(k => k.StartsWith("in", StringComparison.Ordinal))) q.Enqueue((sp.UId, k));
                }
                else q.Enqueue((sp.UId, PrimaryPin(sp)));
            }
        }
        return res;
    }

    private List<string> MergeInPins(Part o) =>
        _pinNames.TryGetValue(o.UId, out var nm)
            ? nm.Keys.Where(k => k.StartsWith("in", StringComparison.Ordinal))
                .OrderBy(k => int.TryParse(Regex.Replace(k, @"\D", ""), out var n) ? n : 0).ToList()
            : new List<string>();

    private bool MergeContainsBlock(Part o) =>
        MergeInPins(o).Any(k => Upstream(o.UId, k).Any(u => _parts.TryGetValue(u, out var p) && IsBlockKind(p.Name) && p.Name != "Not"));

    /// <summary>Closest emitted element that every branch of the merge passes through (never the power rail).</summary>
    private string? CommonRoot(Part o)
    {
        var pins = MergeInPins(o);
        if (pins.Count < 2) return null;
        var sets = pins.Select(k => Upstream(o.UId, k)).ToList();
        foreach (var cand in sets[0])                                   // nearest to the first branch first
        {
            if (_parts.TryGetValue(cand, out var cp) && cp.Name == "O") continue;
            if (!_done.ContainsKey(cand)) continue;
            if (sets.Skip(1).All(st => st.Contains(cand))) return cand;
        }
        return null;
    }

    /// <summary>Switch: write a nested (inner) ParallelBranch for branches that share a deeper contact than the merge root.</summary>
    private const bool NestedBranches = true;

    /// <summary>
    /// Finds branches of the merge that start at the same emitted contact (nearer than the root of the whole merge)
    /// and contain a block: they are drawn as an inner parallel. Returns the pin indexes or null.
    /// </summary>
    private List<int>? FindInnerGroup(Part o, List<string> inPins, List<List<Conn>> branches, out string? root)
    {
        root = null;
        if (inPins.Count < 3) return null;
        var sets = inPins.Select(k => Upstream(o.UId, k)).ToList();
        var overall = CommonRoot(o);
        var ancestors = new HashSet<string>();
        if (overall != null && _parts.TryGetValue(overall, out var op))
        {
            ancestors.Add(overall);
            foreach (var u in Upstream(overall, PrimaryPin(op))) ancestors.Add(u);
        }
        for (int i = 0; i < inPins.Count; i++)
            foreach (var cand in sets[i])
            {
                if (ancestors.Contains(cand) || !_done.ContainsKey(cand)) continue;
                if (!_parts.TryGetValue(cand, out var cp) || cp.Name == "O" || IsBlockKind(cp.Name)) continue;
                var grp = Enumerable.Range(0, inPins.Count).Where(j => sets[j].Contains(cand)).ToList();
                if (grp.Count < 2 || grp.Count >= inPins.Count) continue;
                if (!grp.Any(j => sets[j].Any(u => _parts.TryGetValue(u, out var gp) && IsBlockKind(gp.Name) && gp.Name != "Not")))
                    continue;
                if (grp.Any(j => branches[j].Count == 0)) continue;
                root = cand;
                return grp;
            }
        return null;
    }

    /// <summary>Writes the inner ParallelBranch (mode seq) of a nested group right after the last element of the group.</summary>
    private void AddInnerBranch(Part o, int rootRef, List<int> group, List<List<Conn>> branches)
    {
        var trees = new XElement("BranchTrees");
        var conns = group.SelectMany(g => branches[g]).Distinct().ToList();
        foreach (var c in conns)
        {
            var ce = new XElement("connection", new XAttribute("refLocalId", c.Ref));
            if (!string.IsNullOrWhiteSpace(c.Fp)) ce.Add(new XAttribute("formalParameter", c.Fp));
            trees.Add(new XElement("Tree", new XElement("connectionPointIn", ce)));
        }
        var id = _ctx.NewId();
        var pb = new XElement(N.Tc6 + "vendorElement",
            new XAttribute("localId", id), Pos(0, 0),
            new XElement(N.Tc6 + "alternativeText", new XElement(N.Xhtml + "xhtml", "ParallelBranch")),
            new XElement(N.Tc6 + "addData",
                new XElement(N.Tc6 + "data",
                    new XAttribute("name", "http://www.3s-software.com/plcopenxml/ldparallelbranch"),
                    new XAttribute("handleUnknown", "implementation"),
                    new XElement("ParallelBranch", new XAttribute("mode", "seq"), new XAttribute("xmlns", ""),
                        new XElement("BranchInput", new XElement("connectionPointIn",
                            new XElement("connection", new XAttribute("refLocalId", rootRef)))),
                        trees))));
        // after the last emitted element of the group (TwinCAT writes the inner branch right after the block)
        XElement? last = null; var lastIdx = -1; var all = _ld.Elements().ToList();
        foreach (var c in conns)
        {
            var el = all.FirstOrDefault(e => (string?)e.Attribute("localId") == c.Ref.ToString());
            var idx = el == null ? -1 : all.IndexOf(el);
            if (idx > lastIdx) { lastIdx = idx; last = el; }
        }
        if (last != null) last.AddAfterSelf(pb); else _ld.Add(pb);
        _titleIds.Add(id);
    }

    //private void AddParallelBranch(Part o, List<string> inPins, List<List<Conn>> branches)
    //{
    private int AddParallelBranch(
    Part o,
    List<string> inPins,
    List<List<Conn>> branches,
    List<int>? innerGroup = null)
    {
        // Closest upstream element shared by all branches (the power rail alone does not count).
        string? common = CommonRoot(o);

        if (common == null)
        {
            _ctx.Warnings.Add(
                $"Network {_net.Index}: ParallelBranch for '{o.UId}' " +
                "skipped because the branches have no shared upstream LD element.");

            return -1;
        }

        // The shared element must already have a generated localId.
        if (!_done.TryGetValue(common, out var branchInputRef))
        {
            _ctx.Warnings.Add(
                $"Network {_net.Index}: ParallelBranch for '{o.UId}' " +
                $"skipped because common element '{common}' has not been emitted.");

            return -1;
        }

        // TwinCAT-authored ParallelBranch elements always start at a contact. When the common root is a block
        // (compare / arithmetic / edge ...), draw a TRUE contact after the block's ENO and start the branches there.
        if (BlockRootJunction && _parts.TryGetValue(common, out var rootPart) && IsBlockKind(rootPart.Name))
        {
            var rootEl = _ld.Elements().FirstOrDefault(e => (string?)e.Attribute("localId") == branchInputRef.ToString());
            if (rootEl != null)
            {
                _ctx.Warnings.Add($"Network {_net.Index}: TRUE contact added after block (UId {common}) so the ParallelBranch starts at a contact (BlockRootJunction).");
                var jid = _ctx.NewId();
                var rootFp = rootPart.Name == "Not" ? "Out1" : "ENO";
                var junction = new XElement(N.Tc6 + "contact",
                    new XAttribute("localId", jid),
                    new XAttribute("negated", "false"), new XAttribute("storage", "none"), new XAttribute("edge", "none"),
                    Pos(0, 0), CPI(new[] { new Conn(branchInputRef, rootFp) }),
                    new XElement(N.Tc6 + "connectionPointOut"),
                    new XElement(N.Tc6 + "variable", "TRUE"));
                rootEl.AddAfterSelf(junction);
                _ctx.Contacts++;

                // elements of the branches that were connected to the block now connect to the junction contact
                var ancestors = new HashSet<string>(Upstream(common, PrimaryPin(rootPart))) { common };
                var members = new HashSet<string>();
                foreach (var k in inPins) foreach (var u in Upstream(o.UId, k)) members.Add(u);
                foreach (var u in members.Where(u => !ancestors.Contains(u) && _done.ContainsKey(u)))
                {
                    var el = _ld.Elements().FirstOrDefault(e => (string?)e.Attribute("localId") == _done[u].ToString());
                    if (el == null) continue;
                    foreach (var c in el.Descendants(N.Tc6 + "connection").Where(c => (string?)c.Attribute("refLocalId") == branchInputRef.ToString()))
                    {
                        c.SetAttributeValue("refLocalId", jid);
                        c.SetAttributeValue("formalParameter", null);
                    }
                }
                branchInputRef = jid;
            }
        }

        // One Tree per input branch, preserving every connection
        // and any formalParameter attribute.
        var branchTrees = new XElement("BranchTrees");

        // TwinCAT writes one <Tree> per merge connection; the connections of a nested group share one tree.
        if (innerGroup != null)
        {
            var emittedGroup = false;
            for (int bi = 0; bi < branches.Count; bi++)
            {
                IEnumerable<Conn> conns;
                if (innerGroup.Contains(bi))
                {
                    if (emittedGroup) continue;
                    emittedGroup = true;
                    conns = innerGroup.SelectMany(g => branches[g]).Distinct();
                }
                else conns = branches[bi].Distinct();
                var cpi = new XElement("connectionPointIn");
                foreach (var c in conns)
                {
                    var ce = new XElement("connection", new XAttribute("refLocalId", c.Ref));
                    if (!string.IsNullOrWhiteSpace(c.Fp)) ce.Add(new XAttribute("formalParameter", c.Fp));
                    cpi.Add(ce);
                }
                if (cpi.HasElements) branchTrees.Add(new XElement("Tree", cpi));
            }
        }
        else
            foreach (var connection in branches.SelectMany(b => b).Distinct())
            {
                var connectionPointIn = new XElement("connectionPointIn");

                var connectionElement = new XElement(
                    "connection",
                    new XAttribute("refLocalId", connection.Ref));

                if (!string.IsNullOrWhiteSpace(connection.Fp))
                    connectionElement.Add(new XAttribute("formalParameter", connection.Fp));

                connectionPointIn.Add(connectionElement);
                branchTrees.Add(new XElement("Tree", connectionPointIn));
            }

        // Match the mode used in the reference examples:
        // "sce" when a branch contains a formal-parameter connection,
        // otherwise "seq".
        var mode = branches
            .SelectMany(branch => branch)
            .Any(connection =>
                !string.IsNullOrWhiteSpace(connection.Fp))
            ? "sce"
            : "seq";

        var branchId = _ctx.NewId();

        var parallelBranch = new XElement(
            N.Tc6 + "vendorElement",
            new XAttribute("localId", branchId),
            Pos(0, 0),
            new XElement(
                N.Tc6 + "alternativeText",
                new XElement(
                    N.Xhtml + "xhtml",
                    "ParallelBranch")),
            new XElement(
                N.Tc6 + "addData",
                new XElement(
                    N.Tc6 + "data",
                    new XAttribute(
                        "name",
                        "http://www.3s-software.com/plcopenxml/ldparallelbranch"),
                    new XAttribute(
                        "handleUnknown",
                        "implementation"),
                    new XElement(
                        "ParallelBranch",
                        new XAttribute("mode", mode),
                        new XAttribute("xmlns", ""),
                        new XElement(
                            "BranchInput",
                            new XElement(
                                "connectionPointIn",
                                new XElement(
                                    "connection",
                                    new XAttribute(
                                        "refLocalId",
                                        branchInputRef)))),
                        branchTrees))));

        _ld.Add(parallelBranch);

        _titleIds.Add(branchId);

        return branchId;
    }


    // ── Execution order ───────────────────────────────────────────────────────
    // Key fix: do NOT reverse parallel outputs.
    // The correct order is: contacts → coil → blocks (MOVE etc.)
    // Reversing caused coil to appear AFTER MOVE in the emission list which
    // confused the LayoutEngine about which is the "main" output.
    private List<string> ExecutionOrder()
    {
        var order = new List<string>();
        var seen = new HashSet<string>();

        // Walk from power rail forward through wires
        void VisitWire(Wire w)
        {
            // Collect consumers of this wire in document order (contacts first, then outputs)
            var contacts = new List<string>();
            var outputs = new List<string>();

            foreach (var (uid, pin) in w.Names)
            {
                if (IsOut(pin) || !_parts.TryGetValue(uid, out var part)) continue;
                if (!pin.Equals(PrimaryPin(part), StringComparison.OrdinalIgnoreCase)) continue;

                if (part.Name is "Contact")
                    contacts.Add(uid);
                else
                    outputs.Add(uid);
            }

            // Contacts first, then non-contact outputs (coil before block)
            // Coils before blocks within outputs
            var coils = outputs.Where(u => _parts[u].Name is "Coil" or "SCoil" or "RCoil").ToList();
            var blocks = outputs.Where(u => !(_parts[u].Name is "Coil" or "SCoil" or "RCoil")).ToList();

            // Process consumers in the intended order:
            // contacts -> coils -> other output blocks.
            // FIX: keep the order in which the Siemens wire lists its consumers (= top-to-bottom order in the
            // Siemens editor). Sorting contacts -> coils -> blocks moved MOVE/compare branches in front of
            // coils and produced a different rung order than TIA / the TwinCAT-authored file.
            var inWireOrder = contacts.Concat(outputs).ToHashSet();
            var orderedConsumers = w.Names
                .Select(n => n.Uid)
                .Where(inWireOrder.Contains)
                .Distinct()
                .ToList();

            foreach (var uid in orderedConsumers)
            {
                if (!seen.Add(uid))
                    continue;

                order.Add(uid);

                // Follow each consumer's output wires downstream.
                foreach (var outputWire in _net.Wires.Where(
                    wire => wire.Names.Any(
                        name => name.Uid == uid && IsOut(name.Pin))))
                {
                    VisitWire(outputWire);
                }
            }
        }

        // Start from wires that have a powerrail
        foreach (var w in _net.Wires.Where(w => w.Powerrail)) VisitWire(w);

        // Remaining parts not yet visited (disconnected or driven by variables only)
        // Visit their primary-input wires
        foreach (var p in _net.Parts)
        {
            if (seen.Contains(p.UId)) continue;
            var primWires = _net.Wires.Where(w => w.Names.Any(n => n.Uid == p.UId && n.Pin.Equals(PrimaryPin(p), StringComparison.OrdinalIgnoreCase)));
            foreach (var w in primWires) VisitWire(w);
            if (!seen.Contains(p.UId)) { seen.Add(p.UId); order.Add(p.UId); }
        }

        return order;
    }

    // ── Emission ──────────────────────────────────────────────────────────────
    private string Operand(string uid, string pin, string what)
    {
        if (_sources.TryGetValue((uid, pin.ToLowerInvariant()), out var list))
        {
            var v = list.FirstOrDefault(s => s.Kind == 'V' && s.Expr.Length > 0);
            if (v != null) return v.Expr;
        }
        _ctx.Warnings.Add($"Network {_net.Index}: {what} (UId {uid}) has no resolvable '{pin}' operand.");
        return "TRUE";
    }

    private void Emit(string uid)
    {
        if (_done.ContainsKey(uid) || !_parts.TryGetValue(uid, out var p)) return;

        if (_absorbed.Contains(uid)) return;

        if (!_active.Add(uid)) return;

        switch (p.Name)
        {
            case "O":
                break;

            case "Contact":
                {
                    var cl = Conns(uid, "in");
                    var varN = Operand(uid, "operand", "Contact");
                    var id = _ctx.NewId(); _done[uid] = id;
                    _ld.Add(new XElement(N.Tc6 + "contact",
                        new XAttribute("localId", id),
                        new XAttribute("negated", p.NegatedPins.Contains("operand") ? "true" : "false"),
                        new XAttribute("storage", "none"),
                        new XAttribute("edge", "none"),
                        Pos(0, 0), CPI(cl),
                        new XElement(N.Tc6 + "connectionPointOut"),
                        new XElement(N.Tc6 + "variable", varN)));
                    OnContactEmitted(id, varN, p.NegatedPins.Contains("operand"), uid);
                    _ctx.Contacts++;
                    break;
                }

            case "Coil":
            case "SCoil":
            case "RCoil":
                {
                    var cl = Conns(uid, "in");
                    var varN = Operand(uid, "operand", "Coil");
                    var id = _ctx.NewId(); _done[uid] = id;
                    var st = p.Name == "SCoil" ? "set" : p.Name == "RCoil" ? "reset" : "none";
                    bool viaNot = _sources.TryGetValue((uid, "in"), out var csrc) && csrc.Any(v => v.Kind == 'P' && _notAsCoil.Contains(v.Uid));
                    bool coilNeg = p.NegatedPins.Contains("operand") ^ viaNot;
                    _ld.Add(new XElement(N.Tc6 + "coil",
                        new XAttribute("localId", id),
                        new XAttribute("negated", coilNeg ? "true" : "false"),
                        new XAttribute("storage", st),
                        Pos(0, 0), CPI(cl),
                        new XElement(N.Tc6 + "connectionPointOut"),
                        new XElement(N.Tc6 + "variable", varN)));
                    OnCoilEmitted(id, varN);
                    _ctx.Coils++;
                    break;
                }

            default:
                if (IsBlockKind(p.Name)) EmitBlock(p);
                else
                {
                    var id = _ctx.NewId(); _done[uid] = id;
                    var msg = $"Network {_net.Index}: unsupported instruction '{p.Name}' (UId {uid}).";
                    _ctx.Warnings.Add(msg);
                    _ld.Add(new XElement(N.Tc6 + "comment",
                        new XAttribute("localId", id),
                        new XAttribute("height", 0), new XAttribute("width", 0),
                        Pos(0, 0),
                        new XElement(N.Tc6 + "content",
                            new XElement(N.Xhtml + "xhtml", "UNSUPPORTED: " + msg))));
                    _titleIds.Add(id);
                }
                break;
        }
        _active.Remove(uid);
    }

    // ── Pin mapping ───────────────────────────────────────────────────────────
    private static int? NumSuffix(string l, string prefix)
    {
        var m = Regex.Match(l, "^" + prefix + @"(\d+)$");
        return m.Success ? int.Parse(m.Groups[1].Value) : (int?)null;
    }

    private static string? InPinMap(Part p, string pin)
    {
        var l = pin.ToLowerInvariant();
        if (Compare.ContainsKey(p.Name) || Arith.ContainsKey(p.Name) || IsMoveLike(p.Name))
        {
            if (l is "pre" or "en") return "EN";
            var m = NumSuffix(l, "in"); if (m != null) return "In" + (m.Value + 1);
            if (l == "in") return "In2";
            if (l == "n") return "In3";          // SHL / SHR / ROL / ROR shift count
        }
        if (p.Name == "Not") return "In1";
        if (Edge.ContainsKey(p.Name)) return l == "in" ? "CLK" : null;
        if (Timers.Contains(p.Name)) return pin.ToUpperInvariant();
        if (Counters.Contains(p.Name)) return l == "r" ? "RESET" : pin.ToUpperInvariant();
        return pin;
    }

    private static string OutPinMap(Part p, string pin)
    {
        var l = pin.ToLowerInvariant();
        if (FuncBlocks.TryGetValue(p.Name, out var fnRet)) return l == "eno" ? "ENO" : fnRet;   // function result pin is named like the function
        if (Compare.ContainsKey(p.Name)) return l == "eno" ? "ENO" : "Out2";   // TwinCAT: power flow on ENO, BOOL result on Out2
        if (p.Name == "Not") return "Out1";
        if (IsMoveLike(p.Name) || Arith.ContainsKey(p.Name))
        {
            if (l == "eno") return "ENO";
            if (l == "out") return "Out2";
            var m = NumSuffix(l, "out"); return m != null ? "Out" + (m.Value + 1) : pin;
        }
        if (Edge.ContainsKey(p.Name)) return l == "eno" ? "ENO" : "Q";
        return pin.ToUpperInvariant();
    }

    // ── Library-function blocks (FillBlockI, MoveBlockI, Chars_TO_Strg, LEN) ──────
    private static bool IsNumLit(string e) => Regex.IsMatch(e.Trim(), @"^(16#[0-9A-Fa-f_]+|\d+)$");

    private void EmitFuncBlock(Part p)
    {
        var n = p.Name; var uid = p.UId; var typeName = FuncBlocks[n];
        _currentBlockInVars.Clear();
        var id = _ctx.NewId();   // block localId stays lower than its inVariables

        string? V(string pin) =>
            _sources.TryGetValue((uid, pin), out var l) && l.Count == 1 && l[0].Kind == 'V' && l[0].Expr.Length > 0 ? l[0].Expr : null;
        string Dest(string pin) => _destVar.TryGetValue((uid, pin), out var d) && !string.IsNullOrWhiteSpace(d) ? d : "";
        // data input: a plain variable / constant is wrapped, anything else (another block output) is connected as is
        List<Conn> Data(string pin, Func<string, string> wrap)
        { var e = V(pin); return e != null ? new List<Conn> { new Conn(InVariable(wrap(e)), null) } : Conns(uid, pin); }
        List<Conn> Lit(string expr) => new() { new Conn(InVariable(expr), null) };

        var ins = new List<(string Fp, List<Conn> C)>();
        var en = Conns(uid, "en");
        ins.Add(("EN", en.Count > 0 ? en : new List<Conn> { new Conn(0, null) }));

        string ret = "";
        switch (n)
        {
            case "FillBlockI":
            case "FillBlock":
            case "UFillBlock":
                {
                    var dest = Dest("out");
                    if (dest.Length == 0) _ctx.Warnings.Add($"Network {_net.Index}: {n} (UId {uid}) has no destination array – MEMSET target left empty.");
                    ins.Add(("destAddr", Lit(UseAdrSizeof ? $"ADR({dest})" : dest)));
                    ins.Add(("fillByte", Data("in", e => e)));
                    ins.Add(("n", Data("count", e => UseAdrSizeof ? $"TO_UDINT({e}) * SIZEOF({dest})" : e)));
                    _ctx.Warnings.Add($"Network {_net.Index}: {n}→MEMSET fills bytes – correct for BYTE/CHAR/USINT arrays (or fill value 0); check wider element types.");
                    break;
                }
            case "MoveBlockI":
            case "MoveBlock":
            case "UMoveBlock":
                {
                    var dest = Dest("out"); var src = V("in") ?? "";
                    if (dest.Length == 0) _ctx.Warnings.Add($"Network {_net.Index}: {n} (UId {uid}) has no destination array – MEMCPY target left empty.");
                    ins.Add(("destAddr", Lit(UseAdrSizeof ? $"ADR({dest})" : dest)));
                    ins.Add(("srcAddr", Lit(UseAdrSizeof ? $"ADR({src})" : src)));
                    ins.Add(("n", Data("count", e => UseAdrSizeof ? $"TO_UDINT({e}) * SIZEOF({src})" : e)));
                    break;
                }
            case "Chars_TO_Strg":
                {
                    var dest = Dest("strg"); var chars = V("chars") ?? "";
                    var pc = V("pchars") ?? "0";
                    var src = chars.EndsWith("]") ? chars : $"{chars}[{pc}]";
                    ins.Add(("destAddr", Lit(UseAdrSizeof ? $"ADR({dest})" : dest)));
                    ins.Add(("srcAddr", Lit(UseAdrSizeof ? $"ADR({src})" : src)));
                    ins.Add(("n", Data("cnt", e => UseAdrSizeof ? $"TO_UDINT({e})" : e)));
                    _ctx.Warnings.Add($"Network {_net.Index}: Chars_TO_Strg→MEMCPY – the target STRING is not null-terminated by the copy; clear it first or keep Cnt < string size.");
                    break;
                }
            case "LEN":
                {
                    ins.Add(("STR", Data("in", e => e)));
                    ret = Dest("out");
                    break;
                }
        }

        _done[uid] = id; _ctx.Blocks++;

        var block = new XElement(N.Tc6 + "block",
            new XAttribute("localId", id), new XAttribute("typeName", typeName), Pos(0, 0));
        var inputVars = new XElement(N.Tc6 + "inputVariables");
        foreach (var (fp, cl) in ins)
            inputVars.Add(new XElement(N.Tc6 + "variable", new XAttribute("formalParameter", fp), CPI(cl)));
        block.Add(inputVars);
        block.Add(new XElement(N.Tc6 + "inOutVariables"));

        // TwinCAT function-call shape: ENO, then the return pin whose formalParameter is three blanks;
        // an unconnected output is written as the placeholder expression ???
        var outputVars = new XElement(N.Tc6 + "outputVariables");
        var ecp = new XElement(N.Tc6 + "connectionPointOut");
        ecp.Add(new XElement(N.Tc6 + "expression", "???"));
        outputVars.Add(new XElement(N.Tc6 + "variable", new XAttribute("formalParameter", "ENO"), ecp));
        var rcp = new XElement(N.Tc6 + "connectionPointOut");
        rcp.Add(new XElement(N.Tc6 + "expression", ret.Length > 0 ? ret : "???"));
        outputVars.Add(new XElement(N.Tc6 + "variable", new XAttribute("formalParameter", "   "), rcp));
        block.Add(outputVars);
        block.Add(new XElement(N.Tc6 + "addData",
            new XElement(N.Tc6 + "data",
                new XAttribute("name", "http://www.3s-software.com/plcopenxml/fbdcalltype"),
                new XAttribute("handleUnknown", "implementation"),
                new XElement("CallType", "function")),
            new XElement(N.Tc6 + "data",
                new XAttribute("name", "http://www.3s-software.com/plcopenxml/inputparamtypes"),
                new XAttribute("handleUnknown", "implementation"),
                new XElement("InputParamTypes", n switch
                {
                    "LEN" => "BOOL STRING(255)",
                    "MEMCPY" => "BOOL PVOID PVOID UDINT",
                    _ => typeName == "MEMCPY" ? "BOOL PVOID PVOID UDINT" : "BOOL PVOID USINT UDINT"
                }))));
        _ld.Add(block);

        var captured = _currentBlockInVars.ToList();
        _blockInVars[uid] = captured;
        OnBlockEmitted(id, captured, uid);
    }

    // ── Block emission ────────────────────────────────────────────────────────
    private void EmitBlock(Part p)
    {
        var n = p.Name;
        if (FuncBlocks.ContainsKey(n)) { EmitFuncBlock(p); return; }

        //// Clear inVar tracking before resolving inputs
        //_currentBlockInVars.Clear();

        //var inConns = new List<(string Fp, List<Conn> Conns)>();
        //if (_pinNames.TryGetValue(p.UId, out var pins))
        //    foreach (var kv in pins.OrderBy(k => k.Key, StringComparer.Ordinal))
        //    {
        //        if (IsOut(kv.Value)) continue;
        //        var fp = InPinMap(p, kv.Value); if (fp == null) continue;
        //        inConns.Add((fp, Conns(p.UId, kv.Value)));
        //    }
        //inConns = inConns
        //    .OrderBy(x => x.Fp is "EN" or "IN" or "CLK" or "CU" or "CD" ? 0 : 1)
        //    .ThenBy(x => x.Fp, StringComparer.Ordinal).ToList();

        _currentBlockInVars.Clear();
        var id = _ctx.NewId();   // TwinCAT: the block's localId is lower than the ids of its inVariables

        string? edgeCond = null;
        bool edgeClk = Edge.ContainsKey(n) && _edgeClk.ContainsKey(p.UId);
        bool edgeCollapsed = edgeClk || (Edge.ContainsKey(n) && _edgeCond.TryGetValue(p.UId, out edgeCond));

        _pinNames.TryGetValue(p.UId, out var pins);
        var inConns = new List<(string Fp, List<Conn> Conns)>();

        if (edgeClk)
        {
            // TwinCAT form: EN <- contacts in front of the last one, CLK <- the last contact's plain variable
            var (clk, lastContact) = _edgeClk[p.UId];
            var enConns = lastContact == null ? new List<Conn>() : Conns(lastContact, "in");
            inConns.Add(("EN", enConns.Count > 0 ? enConns : new List<Conn> { new Conn(0, null) }));
            inConns.Add(("CLK", new List<Conn> { new Conn(InVariable(clk), null) }));
        }
        else if (edgeCollapsed)
        {
            // fallback: EN <- the contact chain (real contacts), CLK <- one expression holding the condition
            var enConns = Conns(p.UId, "in");
            inConns.Add(("EN", enConns.Count > 0 ? enConns : new List<Conn> { new Conn(0, null) }));
            // FIX: TwinCAT keeps the CLK input empty here; the contact network (OR / AND) feeds EN.
            // Writing the whole condition as text into CLK made the file differ from the TwinCAT-authored form.
            inConns.Add(("CLK", new List<Conn> { new Conn(InVariable(""), null) }));
        }
        else if (pins != null)
        {
            foreach (var kv in pins.OrderBy(k => k.Key, StringComparer.Ordinal))
            {
                if (IsOut(kv.Value)) continue;
                var fp = InPinMap(p, kv.Value); if (fp == null) continue;
                inConns.Add((fp, Conns(p.UId, kv.Value)));
            }
        }
        inConns = inConns
            .OrderBy(x => x.Fp == "EN" ? 0 : (x.Fp is "IN" or "CLK" or "CU" or "CD" ? 1 : 2))
            .ThenBy(x => x.Fp, StringComparer.Ordinal).ToList();

        if (Timers.Contains(n) && TimerWithEn)
        {
            // TwinCAT form: the rung drives EN, IN is the constant TRUE (not the rung itself)
            var rung = inConns.FirstOrDefault(x => x.Fp == "IN").Conns ?? new List<Conn>();
            inConns.RemoveAll(x => x.Fp == "IN");
            inConns.Insert(0, ("EN", rung.Count > 0 ? rung : new List<Conn> { new Conn(0, null) }));
            inConns.Insert(1, ("IN", new List<Conn> { new Conn(InVariable("TRUE"), null) }));
        }

        _done[p.UId] = id; _ctx.Blocks++;

        string typeName, callType; string? paramTypes;
        if (Compare.TryGetValue(n, out var ct)) { typeName = _invCmp.Contains(p.UId) ? Compare[InvertCompare(n)] : ct; callType = "operator"; paramTypes = "BOOL"; }
        else if (Arith.TryGetValue(n, out var at)) { typeName = at; callType = "operator"; paramTypes = "BOOL"; }
        else if (n == "Move") { typeName = "MOVE"; callType = "operator"; paramTypes = "BOOL"; }
        else if (n == "Convert")
        {
            // Siemens CONVERT (SrcType -> DestType) is the TwinCAT conversion operator <SRC>_TO_<DEST>
            string? src = null, dst = null;
            foreach (var kv in p.Properties)
            {
                var k = kv.Key.ToLowerInvariant();
                if (src == null && (k.StartsWith("src") || k.StartsWith("source") || k.StartsWith("from") || k == "in" || k.StartsWith("intype"))) src = kv.Value;
                else if (dst == null && (k.StartsWith("dest") || k.StartsWith("dst") || k.StartsWith("to") || k == "out" || k.StartsWith("outtype"))) dst = kv.Value;
            }
            if ((src == null || dst == null) && p.Properties.Count == 2)       // unknown names: first = source, second = destination
            { var v = p.Properties.Values.ToList(); src ??= v[0]; dst ??= v[1]; }
            if (string.IsNullOrWhiteSpace(src) || string.IsNullOrWhiteSpace(dst))
            {
                typeName = "MOVE";
                _ctx.Warnings.Add($"Network {_net.Index}: Convert/T_CONV (UId {p.UId}) has no source/destination type (template values found: " +
                                  (p.Properties.Count == 0 ? "none" : string.Join(", ", p.Properties.Select(x => $"{x.Key}={x.Value}"))) + "); MOVE used - verify.");
            }
            else typeName = $"{TypeMapper.Map(src).Text}_TO_{TypeMapper.Map(dst).Text}";
            callType = "operator"; paramTypes = "BOOL";
        }
        else if (n == "Not") { typeName = "NOT"; callType = "operator"; paramTypes = ""; }   // TwinCAT: <InputParamTypes/> empty
        //else if (Edge.TryGetValue(n, out var et)) { typeName = et; callType = "functionblock"; paramTypes = "BOOL"; }
        else if (Edge.TryGetValue(n, out var et)) { typeName = et; callType = "functionblock"; paramTypes = edgeCollapsed ? "BOOL BOOL" : "BOOL"; }
        else if (Timers.Contains(n)) { typeName = n; callType = "functionblock"; paramTypes = TimerWithEn ? "BOOL BOOL TIME" : "BOOL TIME"; }
        else { typeName = n; callType = "functionblock"; paramTypes = null; }

        var block = new XElement(N.Tc6 + "block",
            new XAttribute("localId", id),
            new XAttribute("typeName", typeName));

        if (Edge.ContainsKey(n))
        {
            var bit = Operand(p.UId, "bit", n);
            block.Add(new XAttribute("instanceName", bit));
            _ctx.EdgeBits[bit] = Edge[n];
        }
        else if (Timers.Contains(n) || Counters.Contains(n))
        {
            if (p.InstanceName.Length == 0 ||
                (p.InstanceScope.Length > 0 && p.InstanceScope != "LocalVariable"))
                _ctx.Warnings.Add($"Network {_net.Index}: {n} (UId {p.UId}) – check instance.");
            block.Add(new XAttribute("instanceName",
                p.InstanceName.Length > 0 ? p.InstanceName : $"inst_{n}_{id}"));
        }

        if (Arith.ContainsKey(n))
            _ctx.Warnings.Add($"Network {_net.Index}: {n}→{typeName} MOVE pin convention – verify.");
        if (n == "Not")
            _ctx.Warnings.Add($"Network {_net.Index}: 'Not'→NOT block (In1/Out1) – verify.");

        block.Add(Pos(0, 0));  // patched by ApplyLayout

        var inputVars = new XElement(N.Tc6 + "inputVariables");
        foreach (var (fp, cl) in inConns)
            inputVars.Add(new XElement(N.Tc6 + "variable",
                new XAttribute("formalParameter", fp), CPI(cl)));
        block.Add(inputVars);
        block.Add(new XElement(N.Tc6 + "inOutVariables"));

        var want = new List<string>();
        if (IsMoveLike(n) || Arith.ContainsKey(n) || Compare.ContainsKey(n)) { want.Add("eno"); if (Compare.ContainsKey(n)) want.Add("out"); }
        else if (Timers.Contains(n)) { if (TimerWithEn) want.Add("eno"); want.Add("Q"); want.Add("ET"); }
        //else if (Edge.ContainsKey(n)) want.Add("out");

        else if (Edge.ContainsKey(n)) { want.Add("eno"); want.Add("out"); }
        else if (n is "CTU" or "CTD") { want.Add("Q"); want.Add("CV"); }
        else if (n == "CTUD") { want.Add("QU"); want.Add("QD"); want.Add("CV"); }

        var outs = pins != null
            ? pins.Values.Where(IsOut).OrderBy(x => x.ToLowerInvariant(), StringComparer.Ordinal).ToList()
            : new List<string>();
        var allOut = new List<string>();
        foreach (var w in want.Concat(outs))
            if (!allOut.Any(x => OutPinMap(p, x).Equals(OutPinMap(p, w), StringComparison.OrdinalIgnoreCase))) allOut.Add(w);

        var outputVars = new XElement(N.Tc6 + "outputVariables");
        foreach (var orig in allOut)
        {
            var cp = new XElement(N.Tc6 + "connectionPointOut");
            var key = (p.UId, orig.ToLowerInvariant());

            if (_destVar.TryGetValue(key, out var dest) &&
                !string.IsNullOrWhiteSpace(dest))
            {
                // Preserve the actual destination resolved from the source XML.
                cp.Add(new XElement(N.Tc6 + "expression", dest));
            }
            else if (_openOut.Contains(key) ||
                     (Edge.ContainsKey(n) && OutPinMap(p, orig) == "Q") ||      // TwinCAT: <Q><expression/></Q>
                     (Compare.ContainsKey(n) &&
                      orig.Equals("out", StringComparison.OrdinalIgnoreCase)))
            {
                // Keep an unconnected output empty.
                if (UseDummyForUnusedOutputs && Compare.ContainsKey(n))
                { cp.Add(new XElement(N.Tc6 + "expression", "tDummy")); _ctx.NeedsCompareSink = true; }
                else cp.Add(new XElement(N.Tc6 + "expression"));
            }
            else if (Timers.Contains(n) && TimerWithEn && OutPinMap(p, orig) != "ENO")
                cp.Add(new XElement(N.Tc6 + "expression", "???"));      // only with EN/ENO enabled timer calls

            outputVars.Add(new XElement(
                N.Tc6 + "variable",
                new XAttribute("formalParameter", OutPinMap(p, orig)),
                cp));
        }
        block.Add(outputVars);

        var addData = new XElement(N.Tc6 + "addData",
            new XElement(N.Tc6 + "data",
                new XAttribute("name", "http://www.3s-software.com/plcopenxml/fbdcalltype"),
                new XAttribute("handleUnknown", "implementation"),
                new XElement("CallType", callType)));
        if (paramTypes != null)
            addData.Add(new XElement(N.Tc6 + "data",
                new XAttribute("name", "http://www.3s-software.com/plcopenxml/inputparamtypes"),
                new XAttribute("handleUnknown", "implementation"),
                new XElement("InputParamTypes", paramTypes)));
        // FIX: the TwinCAT-authored file has NO 'outputparamtypes' data element on compare / edge blocks.
        // An empty <OutputParamTypes/> is not produced by TwinCAT and is a candidate for the import NRE.
        if (EmitOutputParamTypes && (edgeCollapsed || Compare.ContainsKey(n)))
            addData.Add(new XElement(N.Tc6 + "data",
                new XAttribute("name", "http://www.3s-software.com/plcopenxml/outputparamtypes"),
                new XAttribute("handleUnknown", "implementation"),
                new XElement("OutputParamTypes")));
        block.Add(addData);
        _ld.Add(block);

        // Capture inVar ids created during input resolution (from _currentBlockInVars)
        var capturedInVars = _currentBlockInVars.ToList();
        _blockInVars[p.UId] = capturedInVars;
        OnBlockEmitted(id, capturedInVars, p.UId);
    }
}