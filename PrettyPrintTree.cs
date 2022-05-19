using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace PrettyPrintTree{

    public enum Orientation {
        VERTICAL, HORIZONTAL
    }

    public enum Color {
        RED, GREEN, YELLOW, BLUE, PINK, LIGHT_BLUE, WHITE, NONE
    }


    public class PrettyPrintTree<Node> {
        private static Regex slashNRegex = new Regex("(\\\\n|\n)", RegexOptions.Compiled);
        private static Dictionary<Color, string> colorToNum = new() {
            { Color.RED, "41" },
            { Color.GREEN, "42" },
            { Color.YELLOW, "43" },
            { Color.BLUE, "44" },
            { Color.PINK, "45" },
            { Color.LIGHT_BLUE, "46" },
            { Color.WHITE, "47" },
        };
        private Func<Node, IEnumerable<Node?>> getChildren;
        private Func<Node, string> getNodeVal;
        private int maxDepth, trim;
        private Color color;
        private bool border, escapeNewline;
        public PrettyPrintTree(
            Func<Node, IEnumerable<Node?>> getChildren,
            Func<Node, string> getVal,
            Color color = Color.BLUE,
            bool border = false,
            bool escapeNewline = false,
            int trim = -1,
            int maxDepth = -1
        ) {
            this.getChildren = getChildren;
            this.getNodeVal = getVal;
            this.color = color;
            this.border = border;
            this.maxDepth = maxDepth;
            this.trim = trim;
            this.escapeNewline = escapeNewline;
        }
        public void Display(Node node, int depth = 0) {
            Console.WriteLine(ToStr(node, depth: depth));
        }

        public string ToStr(Node node, int depth = 0) {
            string[][] res = TreeToStr(node, depth: depth);
            var str = new StringBuilder();
            foreach (var line in res) {
                foreach (var x in line) {
                    str.Append(IsNode(x) ? ColorTxt(x) : x);
                }
                str.Append("\n");
            }
            str.Length -= 1;
            return str.ToString();
        }

        private string[][] GetVal(Node node) {
            var stVal = getNodeVal(node);
            if (this.trim != -1 && this.trim < stVal.Length) {
                stVal = stVal.Substring(0, this.trim) + "...";
            }
            if (this.escapeNewline) {
                stVal = slashNRegex.Replace(stVal, (x) => x.Value.Equals("\n") ? "\\n" : "\\\\n");
            }
            if (!stVal.Contains("\n")) {
                return new string[][]{
                new string[]{ stVal }
            };
            }
            var lstVal = stVal.Split("\n");
            var longest = 0;
            foreach (var item in lstVal) {
                longest = item.Length > longest ? item.Length : longest;
            }
            var res = new string[lstVal.Length][];
            for (int i = 0; i < lstVal.Length; i++) {
                res[i] = new string[] { lstVal[i] + new string(' ', longest - lstVal[i].Length) };
            }
            return res;
        }
        private string[][] TreeToStr(Node node, int depth = 0) {
            var val = GetVal(node);
            var children = this.getChildren(node).Where(x => x != null).Cast<Node>();
            if (children.Count() == 0) {
                if (val.Length == 1) {
                    var res = new[] { new[] { $"[{ val[0][0] }]" } };
                    return res;
                } else {
                    var res = FormatBox("", val);
                    return res;
                }
            }
            var toPrint = new List<List<string>>() { new() };
            var spacing_count = 0;
            var spacing = "";
            if (depth + 1 != this.maxDepth) {
                foreach (var child in children) {
                    var childPrint = TreeToStr(child, depth + 1);
                    for (int l = 0; l < childPrint.Length; l++) {
                        var line = childPrint[l];
                        if (l + 1 >= toPrint.Count) {
                            toPrint.Add(new List<string>());
                        }
                        if (l == 0) {
                            var lineLen = LenJoin(line);
                            var middleOfChild = lineLen - (int)Math.Ceiling(line[^1].Length / 2d);
                            var toPrint0Len = LenJoin(toPrint[0]);
                            toPrint[0].Add(new string(' ', spacing_count - toPrint0Len + middleOfChild) + "┬");
                        }
                        var toPrintNxtLen = LenJoin(toPrint[l + 1]);
                        toPrint[l + 1].Add(new string(' ', spacing_count - toPrintNxtLen));
                        toPrint[l + 1].AddRange(line);
                    }
                    spacing_count = 0;
                    foreach (var item in toPrint) {
                        var itemLen = LenJoin(item);
                        spacing_count = itemLen > spacing_count ? itemLen : spacing_count;
                    }
                    spacing_count++;
                }
                int pipePos;
                if (toPrint[0].Count != 1) {
                    var newLines = string.Join("", toPrint[0]);
                    var spaceBefore = newLines.Length - (newLines = newLines.Trim()).Length;
                    int lenOfTrimmed = newLines.Length;
                    newLines = new string(' ', spaceBefore) +
                        "┌" + newLines.Substring(1, newLines.Length - 2).Replace(' ', '─') + "┐";
                    var middle = newLines.Length - (int)Math.Ceiling(lenOfTrimmed / 2d);
                    pipePos = middle;
                    var newCh = new Dictionary<char, char> { { '─', '┴' }, { '┬', '┼' }, { '┌', '├' }, { '┐', '┤' } }[newLines[middle]];
                    newLines = newLines.Substring(0, middle) + newCh + newLines.Substring(middle + 1);
                    toPrint[0] = new List<string> { newLines };
                } else {
                    toPrint[0][0] = toPrint[0][0].Substring(0, toPrint[0][0].Length - 1) + '│';
                    pipePos = toPrint[0][0].Length - 1;
                }
                if (val[0][0].Length < pipePos * 2) {
                    spacing = new string(' ', pipePos - (int)Math.Ceiling(val[0][0].Length / 2d));
                }
            }
            if (val.Length == 1) {
                val = new string[][] { new string[] { spacing, $"[{val[0][0]}]" } };
            } else {
                val = FormatBox(spacing, val);
            }
            // int maxLen = 0;
            // foreach(var item in val)        {  maxLen = item.Length > maxLen ? item.Length: maxLen; }
            // foreach(var item in toPrint)    {  maxLen = item.Count > maxLen ? item.Count: maxLen;   }

            var asArr = new string[val.Length + toPrint.Count][];
            int row = 0;
            foreach (var item in val) {
                asArr[row] = new string[item.Length];
                for (int i = 0; i < item.Length; i++) {
                    asArr[row][i] = item[i];
                }
                row++;
            }
            foreach (var item in toPrint) {
                asArr[row] = new string[item.Count];
                int i = 0;
                foreach (var x in item) {
                    asArr[row][i] = x;
                    i++;
                }
                row++;
            }

            return asArr;
        }
        private static bool IsNode(string x) {
            if (x is null || x.Equals("")) { return false; }
            if (x[0] == '[' || x[0] == '|' || (x[0] == '│' && x.TrimEnd().Length > 1)) {
                return true;
            }
            if (x.Length < 2) { return false; }
            var middle = new string('─', x.Length - 2);
            return x.Equals($"┌{ middle }┐") || x.Equals($"└{ middle }┘");
        }
        private string ColorTxt(string txt) {
            var spaces = new string(' ', txt.Length - (txt = txt.TrimStart()).Length);
            bool is_label = txt.StartsWith('|');
            if (is_label) {
                throw new NotImplementedException();
            }
            txt = this.border ? txt : $" { txt.Substring(1, txt.Length - 2)} ";
            txt = this.color == Color.NONE ? txt : $"\u001b[{ colorToNum[this.color] }m{ txt }\u001b[0m";
            return spaces + txt;
        }

        private int LenJoin(IEnumerable<string> lst) => string.Join("", lst).Length;

        private string[][] FormatBox(string spacing, string[][] val) {
            string[][] res;
            int start = 0;
            if (this.border) {
                res = new string[val.Length + 2][];
                start = 1;
                var middle = new string('─', val[0][0].Length);
                res[0] = new string[] { spacing, '┌' + middle + '┐' };
                res[^1] = new string[] { spacing, '└' + middle + '┘' };
            } else {
                res = new string[val.Length][];
            }
            for (int r = 0; r < val.Length; r++) {
                res[r + start] = new string[] { spacing, $"│{val[r][0]}│" };
            }
            return res;
        }
    }
}