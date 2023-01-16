using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace project_minicompiler
{
    class SemanticAnalyzer
    {

        Regex expression_Reg = new Regex(@"([a-zA-Z]+|\d+(\.\d+)?)\s*([+\-*/]\s*(\(?[a-zA-Z]+|\d+(\.\d+)?|\(.*\))\s*)*(\)?|)");
        Regex digit_Reg = new Regex(@"\d+");
        Regex varlist_reg = new Regex(@"((int)|(float))\s*\w+");
        //Regex digit_Reg = new Regex(@"\d+");
        RichTextBox semanticRulesBox;
        RichTextBox saOutputBox;

        public SemanticAnalyzer(RichTextBox semanticRulesBox, RichTextBox saOutputBox)
        {
            this.semanticRulesBox = semanticRulesBox;
            this.saOutputBox = saOutputBox;
        }

        public void execute(string line)
        {
            if (digit_Reg.Match(line).Success)
            {
                semanticRulesBox.Text += "" +
                        "1) number1.val = (number2.val*10)+digit.val\n" +
                        "2) number.val = digit.val\n" +
                        "3) digit.val = 0\n" +
                        "4) digit.val = 1\n" +
                        "5) digit.val = 2\n" +
                        "6) digit.val = 3\n" +
                        "7) digit.val = 4\n" +
                        "8) digit.val = 5\n" +
                        "9) digit.val = 6\n" +
                        "10) digit.val = 7\n" +
                        "11) digit.val = 8\n" +
                        "12) digit.val = 9\n\n";
                foreach (Match match in digit_Reg.Matches(line))
                {
                    analyzeCase0(match.ToString());
                }
            }
            if (expression_Reg.Match(line).Success)
            {
                semanticRulesBox.Text += "" +
                        "1) exp1.val = exp2.val + term.val\n" +
                        "2) exp1.val = exp2.val - term.val\n" +
                        "3) exp.val = term.val\n" +
                        "4) term1.val = term2.val*factor.val\n" +
                        "5) term.val = factor.val\n" +
                        "6) factor.val = exp.val\n" +
                        "7) factor.val = number.val\n\n";

                foreach (Match match in expression_Reg.Matches(line))
                {
                    try
                    {
                        evaluateExpression(match.ToString());
                        Console.WriteLine(match.ToString());
                        string updtedexp = Regex.Replace(match.ToString(), @"\s+", "");
                        Console.WriteLine(updtedexp);
                        analyzeCase1(updtedexp);
                    }
                    catch
                    {
                        
                    }
 
                }
                
            }

            if (varlist_reg.Match(line).Success)
            {
                semanticRulesBox.Text += "" +
                        "1) var-list.dtype = type.dtype\n" +
                        "2) type.dtype = int\n" +
                        "3) type.dtype = float\n" +
                        "4) id.dtype = var-list1.dtype\n" +
                        "var-list2.dtype = var-list1.dtype\n" +
                        "5) id.dtype = var-list.dtype\n\n";

                foreach (Match match in varlist_reg.Matches(line))
                {
                    analyzeCase2(match.ToString());
                }
            }
            if (false)
            {
                semanticRulesBox.Text = "" +
                        "1) based-num.val = num.val\n" +
                        "num.basee = basechar.basee \n" +
                        "2) basechar.basee = 8\n" +
                        "3) basechar.basee = 10\n" +
                        "4) if(digit.val == null || num2.val == null)\n" +
                        "num1.val = null\n" +
                        "else\n" +
                        "num1.val = (num2.val*num1.basee)+digit.val\n" +
                        "num2.basee = num1.basee\n" +
                        "digit.basee = num1.basee\n" +
                        "5) num.val = digit.val\n" +
                        "digit.basee = num.basee\n" +
                        "6) digit.val = 0\n" +
                        "7) digit.val = 1\n" +
                        "8) digit.val = 2\n" +
                        "9) digit.val = 3\n" +
                        "10) digit.val = 4\n" +
                        "11) digit.val = 5\n" +
                        "12) digit.val = 6\n" +
                        "13) digit.val = 7\n" +
                        "14) if(digit.basee == 8)\n" +
                        "digit.val = null\n" +
                        "else\n" +
                        "digit.val = 8\n" +
                        "15) if(digit.basee == 8)\n" +
                        "digit.val = null\n" +
                        "else\n" +
                        "digit.val = 9\n\n";
            }
            if (line.Contains("(") || line.Contains(")"))
            {
                string parenthesis = "";
                foreach (char c in line.ToCharArray())
                {
                    if (c == '(' || c == ')')
                    {
                        parenthesis += c + "";
                    }
                }
                analyzeCase4(parenthesis);
                semanticRulesBox.Text += "" +
                        "S.valid = S.count = 0 then true else false\n" +
                        "S.count = if '(' then count=count+1 else if ')' then count=count-1 else count=count\n" +
                        "S.valid = true\n" +
                        "S.count = 0\n\n";
            }
        }


        private void analyzeCase0 (string input)
        {
            saOutputBox.Text += "\n";
            char[] chars = input.ToCharArray();
            List<NonTerminal> list = new List<NonTerminal>();
            //Console.WriteLine("________________________________________________________");
            list.Clear();
            try
            {
                foreach (char ch in chars)
                {
                    NonTerminal d = new NonTerminal(0);
                    string num_str = "" + ch;

                    d.val = int.Parse(num_str);
                    list.Add(d);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Error in Sementic Anaylzer Phase Case 0", "ERROR",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            list.Reverse();
            foreach (NonTerminal n in list)
            {
                //Console.WriteLine("digit.val = " + n.val);
                saOutputBox.AppendText("digit.val = " + n.val + "\n");
            }
            //Console.WriteLine("number.val = " + list[list.Count - 1].val);
            saOutputBox.AppendText("number.val = " + list[list.Count - 1].val + "\n");
            NonTerminal root = new NonTerminal(0);
            root.val = list[list.Count - 1].val;
            list.Reverse();
            for (int i = 1; i < list.Count; i++)
            {
                root.val = (root.val * 10) + list[i].val;
                //Console.WriteLine("number.val = " + root.val);
                saOutputBox.AppendText("number.val = " + root.val + "\n");
            }
        }

        private void analyzeCase1(string input)
        {
            saOutputBox.Text += "\n";
            char[] chars = input.ToCharArray();
            List<NonTerminal> list = new List<NonTerminal>();

            //Console.WriteLine("________________________________________________________");
            string state = "exp";
            NonTerminal ro = ParseCase1(state, chars, 0, chars.Length - 1);
            if (ro.val != int.MinValue)
            {
                //Console.WriteLine("exp.val = " + ro.val);
                saOutputBox.AppendText("exp.val = " + ro.val + "\n");
            }
            else
            {
                //Console.WriteLine("ERROR");
                saOutputBox.AppendText("ERROR" + "\n");
            }
        }

        private void analyzeCase2(string input)
        {
            saOutputBox.Text += "\n";
            char[] chars = input.ToCharArray();
            List<NonTerminal> list = new List<NonTerminal>();
            //Console.WriteLine("________________________________________________________");
            string state2 = "decl";
            ParseCase2(state2, new string(chars), null);
        }

        private void analyzeCase3(string input)
        {
            saOutputBox.Text += "\n";
            char[] chars = input.ToCharArray();
            List<NonTerminal> list = new List<NonTerminal>();
        }

        private void analyzeCase4(string input)
        {
            saOutputBox.Text += "\n";
            char[] chars = input.ToCharArray();
            List<NonTerminal> list = new List<NonTerminal>();
            ParseCase4(input);
        }
    

        public void ParseCase4(string input)
        {
            NonTerminal root = new NonTerminal(0);


            if (input.Length == 0)
            {
                root.val = int.MinValue;
                saOutputBox.Text += "Valid";
            }
            else if (input.Length == 1)
            {
                if (input[0] == 'e')
                {
                    root.val = 0;
                    saOutputBox.Text = "S.count=" + 0 + "\n";
                    saOutputBox.Text += "Valid";
                }
                else
                {
                    saOutputBox.Text += "Invalid";
                    root.val = int.MinValue;
                }
            }
            else
            {
                int count = 0;
                foreach (char ch in input)
                {
                    if (ch == '(')
                    {
                        count++;
                        saOutputBox.Text += "S.count=" + count + "\n";
                        saOutputBox.Text += "(" + "\n";
                    }
                    else if (ch == ')')
                    {
                        count--;
                        saOutputBox.Text += "S.count=" + count + "\n";
                        saOutputBox.Text += ")" + "\n";
                    }
                    else
                    {
                        saOutputBox.Text += ch + "\n";
                    }

                    if (count < 0)
                    {
                        saOutputBox.Text += "S.count=" + count + "\n";
                        saOutputBox.Text += "S.valid=" + "false" + "\n";
                        return;
                    }


                }
                if (count != 0)
                {
                    saOutputBox.Text += "S.count=" + count + "\n";
                    saOutputBox.Text += "S.valid=" + "false" + "\n";
                }
                else
                {
                    saOutputBox.Text += "S.count=" + count + "\n";
                    saOutputBox.Text += "S.valid=" + "true" + "\n";

                }




            }
        }
        public NonTerminal ParseCase3(string state, List<char> chars, NonTerminal parent)
        {
            NonTerminal root = new NonTerminal(0);
            if (state == "based-num")
            {
                /*
                 * numbers go on the left
                 * last digit go to the right
                 * call on each
                 */
                List<char> right_in = new List<char>();
                char last = chars[chars.Count - 1];
                right_in.Add(last);
                chars.Remove(last);

                NonTerminal basechar = ParseCase3("basechar", right_in, parent);
                NonTerminal num = ParseCase3("num", chars, basechar);
                saOutputBox.Text += "num.base = " + num.basee + "\n";
                root.val = num.val;
                saOutputBox.Text += "based-num.val = " + root.val + "\n";

            }
            else if (state == "basechar")
            {
                if (chars[0] == 'o')
                {
                    root.basee = 8;
                    saOutputBox.Text += "base-char.base = 8" + "\n";
                }
                else if (chars[0] == 'd')
                {
                    root.basee = 10;
                    saOutputBox.Text += "base-char.base = 10" + "\n";
                }
                else
                {
                    MessageBox.Show("error during semantic analysis case3");
                    saOutputBox.Text += "INPUT ERROR" + "\n";
                }
            }
            else if (state == "num")
            {
                root.basee = parent.basee;
                if (chars.Count > 1)
                {
                    List<char> right_in = new List<char>();
                    char last = chars[chars.Count - 1];
                    right_in.Add(last);
                    chars.Remove(last);


                    NonTerminal num = ParseCase3("num", chars, parent);
                    saOutputBox.Text += "num.base = " + num.basee + "\n";
                    NonTerminal digit = ParseCase3("digit", right_in, parent);
                    saOutputBox.Text += "digit.base = " + digit.basee + "\n";

                    if (digit.val == int.MinValue || num.val == int.MinValue)
                    {
                        root.val = int.MinValue;
                        MessageBox.Show("error during semantic analysis case3");
                        saOutputBox.Text += "INPUT ERROR" + "\n";
                    }
                    else
                    {
                        root.val = (num.val * num.basee) + digit.val;
                        saOutputBox.Text += "num.val = " + root.val + "\n";
                    }
                }
                else
                {
                    NonTerminal digit = ParseCase3("digit", chars, parent);
                    root.val = digit.val;
                    saOutputBox.Text += "digit.base = " + digit.basee + "\n";
                    saOutputBox.Text += "num.val = " + root.val + "\n";
                }
            }
            else if (state == "digit")
            {
                root.basee = parent.basee;
                try
                {
                    string str = "" + chars[0];
                    int value = int.Parse(str);
                    if (value >= 0 && value <= 7)
                    {
                        root.val = value;
                        saOutputBox.Text += "digit.inval = " + root.val + "\n";
                    }
                    else if (value >= 7 && value <= 9)
                    {
                        if (root.basee == 10)
                        {
                            root.val = value;
                            saOutputBox.Text += "digit.inval = " + root.val + "\n";
                        }
                        else
                        {
                            MessageBox.Show("error during semantic analysis case3");
                            saOutputBox.Text += "INPUT ERROR" + "\n";
                        }
                    }
                    else
                    {
                        saOutputBox.Text += "INPUT ERROR" + "\n";
                    }
                }
                catch (Exception)
                {
                    saOutputBox.Text += "INPUT ERROR" + "\n";

                }
            }
            return root;
        }

        public NonTerminal ParseCase2(string state, string input, string inherit)
        {
            NonTerminal root = new NonTerminal(0);
            if (state == "decl")
            {
                int breaker = -1;
                if (input.Length <= 4)
                {
                    MessageBox.Show("Error during sementic analysis case2", "ERROR",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return root;
                }
                else if (input.Substring(0, 4) == "int ")
                {
                    breaker = 4;
                }
                else if (input.Substring(0, 6) == "float ")
                {
                    breaker = 6;
                }
                else
                {
                    MessageBox.Show("error during semantic analysis case2", "ERROR",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return root;
                }
                NonTerminal type = ParseCase2("type", input.Substring(0, breaker - 1), null);

                NonTerminal vlist = ParseCase2("vlist", input.Substring(breaker), type.dtype);

            }
            else if (state == "type")
            {
                if (input[0] == 'i')
                {
                    root.dtype = "int";
                }
                else if (input[0] == 'f')
                {
                    root.dtype = "float";
                }
                saOutputBox.Text += "type.dtype = " + root.dtype + "\n";
            }
            else if (state == "vlist")
            {
                root.dtype = inherit;
                saOutputBox.Text += "var-list.dtype = " + root.dtype + "\n";
                bool found = false;
                string lhs = "";
                string rhs = "";
                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i] == ',')
                    {
                        lhs = (input.Substring(0, i)).Trim();
                        rhs = (input.Substring(i + 1)).Trim();
                        found = true;
                        break;
                    }

                }
                if (found)
                {
                    NonTerminal id = ParseCase2("id", lhs, inherit);
                    NonTerminal vlist = ParseCase2("vlist", rhs, inherit);
                }
                else
                {
                    NonTerminal id = ParseCase2("id", input, inherit);
                }

            }
            else if (state == "id")
            {
                Regex idreg = new Regex("^[a-zA-Z_$][a-zA-Z_$0-9]*$");
                if (idreg.IsMatch(input) && (input != "int" || input != "float"))
                {
                    root.dtype = inherit;
                    saOutputBox.Text += "id.dtype = " + root.dtype + "\n";
                }
                else
                {
                    MessageBox.Show("Error during semantic analysis case2");
                    saOutputBox.Text += "PARSING ERROR" + "\n";
                    return root;
                }

            }
            return root;
        }


        public NonTerminal ParseCase1(string state, char[] chars, int start, int end)
        {
            NonTerminal root = new NonTerminal(0);
            if (state == "exp")
            {
                bool found = false;
                int started = 0;
                for (int i = end; i >= start; i--)
                {
                    if (chars[i] == '(')
                    {
                        started--;
                    }
                    else if (chars[i] == ')')
                    {
                        started++;
                    }
                    if ((chars[i] == '+' || chars[i] == '-') && (started == 0))
                    {

                        found = true;
                        NonTerminal left = ParseCase1("exp", chars, start, i - 1);
                        saOutputBox.Text += "exp.val = " + left.val + "\n";
                        NonTerminal right = ParseCase1("term", chars, i + 1, end);
                        saOutputBox.Text += "term.val = " + right.val + "\n";
                        if (chars[i] == '+')
                        {
                            root.val = (left.val + right.val);
                        }
                        else
                        {
                            root.val = (left.val - right.val);
                        }
                        break;
                    }

                }
                if (!found)
                {
                    NonTerminal term = ParseCase1("term", chars, start, end);
                    saOutputBox.Text += "term.val = " + term.val + "\n";
                    root.val = term.val;
                }
            }
            else if (state == "term")
            {
                bool found = false;
                int started = 0;
                for (int i = end; i >= start; i--)
                {
                    if (chars[i] == '(')
                    {
                        started--;
                    }
                    else if (chars[i] == ')')
                    {
                        started++;
                    }
                    if ((chars[i] == '*') && (started == 0))
                    {
                        found = true;
                        NonTerminal left = ParseCase1("term", chars, start, i - 1);
                        saOutputBox.Text += "term.val = " + left.val + "\n";
                        NonTerminal right = ParseCase1("factor", chars, i + 1, end);
                        saOutputBox.Text += "factor.val = " + right.val + "\n";

                        root.val = (left.val * right.val);
                        break;
                    }

                }
                if (!found)
                {
                    NonTerminal factor = ParseCase1("factor", chars, start, end);
                    saOutputBox.Text += "factor.val = " + factor.val + "\n";
                    root.val = factor.val;
                }
            }
            else if (state == "factor")
            {
                Regex num_reg = new Regex("^[0-9]+$");
                int stop = (end + 1) - start;
                try
                {
                    String str = new string(chars).Substring(start, stop);
                    if (num_reg.IsMatch(str))
                    {
                        NonTerminal number = ParseCase1("number", chars, start, end);
                        saOutputBox.Text += "number.inval = " + number.val + "\n";
                        root.val = number.val;
                    }
                    else
                    {
                        NonTerminal exp = ParseCase1("exp", chars, start + 1, end - 1);
                        saOutputBox.Text += "exp.val = " + exp.val + "\n";
                        root.val = exp.val;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    MessageBox.Show("Error during semantic analysis case 1", "ERROR",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return root;
                }

            }
            else if (state == "number")
            {
                string str = new string(chars);
                int stop = (end + 1) - start;
                int val = int.Parse(str.Substring(start, stop));
                root.val = val;
            }
            return root;
        }

        public static double evaluateExpression(string expression)
        {
            DataTable table = new DataTable();
            table.Columns.Add("expression", typeof(string), expression);
            DataRow row = table.NewRow();
            table.Rows.Add(row);
            return double.Parse((string)row["expression"]);
        }
        public struct NonTerminal
        {
            public int val;
            public int basee;
            public string dtype;
            public NonTerminal(int v)
            {
                val = int.MinValue;
                basee = int.MinValue;
                dtype = null;
            }
        }
    }
}
