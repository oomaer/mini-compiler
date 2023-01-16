using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace project_minicompiler
{
    class Rule
    {
        public string nonTerminal;
        public List<string> production = new List<string>();
        public bool isComplete = false;

        public Rule(string nt, List<string> p)
        {
            this.nonTerminal = nt;
            this.production = p;
        }
        public Rule copy()
        {
            List<string> newp = new List<string>();
            newp = this.production.ToList();
            Rule r = new Rule(nonTerminal, newp);
            return r;
        }

        public string convertString()
        {
            string str = "";
            str += this.nonTerminal + "->";
            foreach (string p in this.production)
            {
                if (p != ".")
                {
                    str += p;
                }
            }
            return str;
        }



        public bool matches(Rule r)
        {
            bool matches = true;
            if (r.nonTerminal != this.nonTerminal)
            {
                matches = false;
            }
            else
            {
                if (r.production.Count != this.production.Count)
                {
                    matches = false;
                }
                else
                {
                    for (int i = 0; i < this.production.Count; i++)
                    {
                        if (r.production[i] != this.production[i])
                        {
                            matches = false;
                            break;
                        }
                    }
                }
            }
            return matches;
        }
    }

    class State
    {
        public string name = "";
        public string input = "";
        public Dictionary<string, State> outputs = new Dictionary<string, State>();
        public bool extended = false;
        public List<Rule> rules = new List<Rule>();
        public bool completeState = false;
        public Dictionary<string, Rule> reductionRules = new Dictionary<string, Rule>();


        public State(string name)
        {
            this.name = name;
        }

        public void addRule(Rule r)
        {
            if (r.production.Last() == ".")
            {
                r.isComplete = true;
            }
            rules.Add(r);
        }

        public void getReductionRules(Hashtable followSets)
        {
            foreach (Rule rule in this.rules)
            {
                if (rule.isComplete)
                {
                    List<string> fsarray = refineFollowSet(followSets[rule.nonTerminal].ToString()).Split(',').ToList();

                    foreach (string item in fsarray)
                    {
                        if (item != "" && item != " " && item != ",")
                        {
                            if (!this.reductionRules.ContainsKey(item))
                            {
                                this.reductionRules.Add(item, rule);
                            }
                            else
                            {

                            }
                        }
                    }
                }
            }
            //displayReductionRules();
        }


        public bool matches(State s)
        {
            bool matches = true;
            if (this.input != s.input)
            {
                matches = false;
            }
            else
            {
                foreach (Rule r in s.rules)
                {
                    bool rulematch = false;
                    foreach (Rule rule in this.rules)
                    {
                        if (rule.matches(r))
                        {
                            rulematch = true;
                            break;
                        }
                    }
                    if (!rulematch)
                    {
                        matches = false;
                        break;
                    }
                }
            }
            return matches;
        }

        public void displayRules()
        {
            foreach (Rule r in rules)
            {
                Console.Write(r.nonTerminal + "-> ");
                foreach (string t in r.production)
                {
                    Console.Write(t + " ");
                }
                Console.WriteLine();
            }
        }

        public void displayRulesInTextBox(RichTextBox textBox)
        {
            foreach (Rule r in rules)
            {
                textBox.AppendText(r.nonTerminal + "-> ");
                foreach (string t in r.production)
                {
                    textBox.AppendText(t + " ");
                }
                textBox.AppendText("\n");
            }
        }

        public void displayOutputsInTextBox(RichTextBox textBox)
        {
            foreach (KeyValuePair<string, State> entry in this.outputs)
            {
                textBox.AppendText(entry.Key + " ----> ");
                textBox.AppendText(entry.Value.name);
                textBox.AppendText("\n");
            }
        }
        public void displayOutputs()
        {
            foreach (KeyValuePair<string, State> entry in this.outputs)
            {
                Console.Write(entry.Key + " ----> ");
                Console.Write(entry.Value.name);
                Console.WriteLine();
            }
        }

        public void display()
        {
            Console.WriteLine("State: " + this.name);
            Console.WriteLine("Input: " + this.input);
            displayRules();
            Console.WriteLine("---------------------------");
        }
        public void displayToRichTextBox(RichTextBox textbox)
        {
            textbox.Text += "State: " + this.name + "\n";
            textbox.Text += "Input: " + this.input + "\n";

            textbox.AppendText("\n");
            displayRulesInTextBox(textbox);
            textbox.AppendText("\n");
            displayOutputsInTextBox(textbox);
            textbox.AppendText("\n\n---------------------------\n\n");
        }


        public void displayReductionRules()
        {
            Console.WriteLine(this.name);
            foreach (KeyValuePair<string, Rule> entry in this.reductionRules)
            {
                Console.Write(entry.Key + ": ");
                foreach (string p in entry.Value.production)
                {
                    Console.Write(p + " ");
                }
                Console.WriteLine();

            }
            Console.WriteLine("----------------");
        }


        private string refineFollowSet(string followset)
        {
            string finalFollowSet = "";
            String[] temparr = followset.Split(',');

            for (int i = 0; i < temparr.Length; i++)
            {
                string item = temparr[i];
                if (item != " " && item != "" && !finalFollowSet.Contains(item))
                {
                    if (i != 0)
                    {
                        finalFollowSet += ",";
                    }
                    finalFollowSet += item;
                }
            }
            return finalFollowSet;
        }


    }
}
