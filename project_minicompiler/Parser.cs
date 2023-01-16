using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace project_minicompiler
{
    class Parser
    {

     

        Hashtable productionRulez = new Hashtable();
        Hashtable firstSets = new Hashtable();
        public Hashtable followSets = new Hashtable();
        string startsymbol = "";
        Regex re = new Regex(@"(follow)\([A-Z]+[`]*([-][A-Z]+[`]*)*\)");
        List<Hashtable> followIterations = new List<Hashtable>();

        RichTextBox statesBox;
        DataGridView parsingTable;
        DataGridView parsingStack;

        //GRAMMER RULES TO BE USED
        string declGrammer =
            "S`>S\n" +
            "S>D id = E ;\n" +
            "D>int | float\n" +
            "E>E + T | E - T | T\n" +
            "T>T* F | F\n" +
            "F>(E ) | id | number";

        string reAssignGrammer =
            "S`>S\n" +
            "S>id = E ;\n" +
            "E>E + T | E - T | T\n" +
            "T>T* F | F\n" +
            "F>(E ) | id | number";


        public Parser(RichTextBox statesBox, DataGridView parsingTable, DataGridView parsingStack)
        {
            this.statesBox = statesBox;
            this.parsingTable = parsingTable;
            this.parsingStack = parsingStack;
        }

        public void parse(String line, String grammer)
        {
            productionRulez.Clear();
            firstSets.Clear();
            followSets.Clear();
            followIterations.Clear();
            startsymbol = "";

            //change the line to match the grammer
            line += " $";
            line = Regex.Replace(line, @";+", " ;");

           
           



            //flag used to check for small letters non-terminals
            bool flag = true;


            //array of all the production rules
            String[] productionRules = grammer.Split('\n');


            //loop through all the production rules
            for (int i = 0; i < productionRules.Length; i++)
            {
                string productionRule = productionRules[i];
                //seperate the left and right part of the rule
                String[] splittedRule = productionRule.Split('>');

                //add $ to starting follow set
                if (i == 0)
                {
                    startsymbol = splittedRule[0];
                    addToFollowSet(startsymbol, "$");
                }
                //if rule of a non terminal is not in hashtable, then put it in the hashtable
                if (!productionRulez.Contains(splittedRule[0]))
                {
                    productionRulez.Add(splittedRule[0], splittedRule[1]);
                    var te = splittedRule[0].ToCharArray()[0];
                    if (!(new Regex(@"^(([A-Z]+)|([A-Z]+[-]*[A-Z]+))[`]?$")).Match(te + "").Success)
                    {
                        flag = false;
                       // MessageBox.Show("Non terminals cant be small letters");
                    }
                }
                else
                {
                    productionRulez[splittedRule[0]] += "|" + splittedRule[1];
                }
            }


            if (flag)
            {
                foreach (DictionaryEntry rule in productionRulez)
                {
                    getFirstSet(rule.Key.ToString());
                }

                getFollowSet();
                runIterationsForFollow();

                if (firstSets.Count == 0)
                {
                    /*MessageBox.Show("Left Recursive Grammer");
                    Application.Exit();*/
                }

                genDFA();
                generateParsingTable();
            }

            parseInput(line);
        }


        Dictionary<string, State> DFAStatesDict = new Dictionary<string, State>();
        List<Rule> initalDots = new List<Rule>();
        List<State> dfastates = new List<State>();
        List<State> tempStates = new List<State>();


        //generate DFA
        public void genDFA()
        {
            statesBox.Text = "";
            parsingTable.Rows.Clear();
            initalDots = new List<Rule>();
            dfastates = new List<State>();
            DFAStatesDict = new Dictionary<string, State>();

            //adding the first rule
            foreach (String rul in productionRulez[startsymbol].ToString().Split('|'))
            {
                List<string> prod = removeEmptyStringsFromArr(rul.Split(' ')).ToList();
                prod.Insert(0, ".");
                initalDots.Add(new Rule(startsymbol, prod));
            }


            //adding remaining rules
            foreach (DictionaryEntry r in productionRulez)
            {
                if (r.Key.ToString() != startsymbol)
                {
                    foreach (String rul in r.Value.ToString().Split('|'))
                    {
                        List<string> prod = removeEmptyStringsFromArr(rul.Split(' ')).ToList();
                        prod.Insert(0, ".");
                        initalDots.Add(new Rule(r.Key.ToString(), prod));
                    }
                }
            }


            State initialState = new State("0");
            initialState.addRule(initalDots[0]);
            addGenRules(initalDots[0], initialState);
            initialState.getReductionRules(followIterations.Last());
            dfastates.Add(initialState);

            extendState(initialState);


            foreach (State s in dfastates)
            {
                //s.display();
                s.displayToRichTextBox(statesBox);
                DFAStatesDict.Add(s.name, s);
            }
        }

        //generate non terminal rules after the dot
        private void addGenRules(Rule rule, State state)
        {
            for (int i = 0; i < rule.production.Count; i++)
            {
                //if we find a . in the rule
                if (rule.production[i] == ".")
                {
                    //if . not at last
                    if (i < rule.production.Count - 1)
                    {
                        string next = rule.production[i + 1];
                        //if rule goes to ~
                        if (next == "~")
                        {
                            Rule emptyRule = new Rule(rule.nonTerminal, new List<string>() { "~", "." });
                            emptyRule.isComplete = true;
                            if (!ruleAlreadyThereInState(emptyRule, state))
                            {
                                state.addRule(emptyRule);
                            }

                        }
                        else
                        {
                            foreach (Rule r in initalDots)
                            {
                                //if there is a non Terminal in next (next is the symbol after dot)
                                if (r.nonTerminal == next)
                                {
                                    if (!ruleAlreadyThereInState(r, state))
                                    {

                                        //dont add empty rule, we already added above
                                        if (!r.production.Contains("~"))
                                        {
                                            state.addRule(r);
                                        }
                                        addGenRules(r, state);
                                    }
                                }
                            }
                        }

                    }
                }
            }

        }


        //check if a certian dot rule is present in the state
        private bool ruleAlreadyThereInState(Rule r, State s)
        {
            bool alreadyThere = false;
            foreach (Rule rule in s.rules)
            {
                if (rule.matches(r))
                {
                    alreadyThere = true;
                }
            }
            return alreadyThere;
        }

        //creates a new state based on a previous state
        private void extendState(State state)
        {
            tempStates = new List<State>();
            foreach (Rule rule in state.rules)
            {
                for (int i = 0; i < rule.production.Count; i++)
                {
                    if (rule.production[i] == ".")
                    {
                        if (i < rule.production.Count - 1)
                        {
                            String next = rule.production[i + 1];
                            bool alreadyadded = false;

                            foreach (State tempstate in tempStates)
                            {
                                if (tempstate.input == next)
                                {
                                    genNewRule(tempstate, rule);
                                    alreadyadded = true;
                                    break;
                                }
                            }
                            if (!alreadyadded)
                            {
                                State newState = new State("");

                                newState.input = next;
                                genNewRule(newState, rule);
                                tempStates.Add(newState);
                            }

                        }
                        break;
                    }
                }
            }

            foreach (State newstate in tempStates)
            {
                Boolean statealreadythere = false;
                foreach (State dfastate in dfastates)
                {
                    if (dfastate.matches(newstate))
                    {
                        state.outputs.Add(dfastate.input, dfastate);
                        statealreadythere = true;
                        break;
                    }
                }
                if (!statealreadythere)
                {
                    newstate.getReductionRules(followIterations.Last());
                    newstate.name = "" + dfastates.Count;
                    state.outputs.Add(newstate.input, newstate);
                    dfastates.Add(newstate);
                    //if that state not extended
                    if (!newstate.extended)
                    {
                        extendState(newstate);
                        newstate.extended = true;
                    }

                }
            }
        }

        //move dot to right side of the rule and add it to the newly created state
        private void genNewRule(State state, Rule rule)
        {
            Rule newr = rule.copy();
            int index = rule.production.LastIndexOf(".");
            newr.production.Remove(".");
            newr.production.Insert(index + 1, ".");
            state.addRule(newr);
            addGenRules(newr, state);
        }




        //method generates the parsing table in data grid view
        private void generateParsingTable()
        {
            List<string> nonTerminals = new List<string>();
            List<string> Terminals = new List<string>();

            foreach (Rule rule in initalDots)
            {
                foreach (string symbol in rule.production)
                {
                    if (symbol != ".")
                    {
                        //nonterminal
                        if (productionRulez.Contains(symbol))
                        {
                            if (!nonTerminals.Contains(symbol))
                            {
                                nonTerminals.Add(symbol);
                            }
                        }
                        else
                        {
                            if (!Terminals.Contains(symbol))
                            {
                                Terminals.Add(symbol);
                            }
                        }
                    }
                }
            }




            int colcount = nonTerminals.Count + Terminals.Count + 2;
            parsingTable.ColumnCount = colcount;

            parsingTable.Columns[0].Name = "State";

            for (int i = 0; i < Terminals.Count; i++)
            {
                if (Terminals[i] != "~")
                {
                    parsingTable.Columns[i + 1].Name = Terminals[i];
                }
            }
            parsingTable.Columns[Terminals.Count + 1].Name = "$";
            for (int i = 0; i < nonTerminals.Count; i++)
            {
                parsingTable.Columns[i + Terminals.Count + 2].Name = nonTerminals[i];
            }

            foreach (State s in dfastates)
            {
                List<string> row = new List<string>();
                row.Add(s.name);

                //fill terminals
                foreach (string t in Terminals)
                {
                    if (t != "~")
                    {
                        if (s.outputs.ContainsKey(t))
                        {
                            row.Add(s.outputs[t].name);
                        }
                        else if (s.reductionRules.ContainsKey(t))
                        {
                            row.Add("red: " + s.reductionRules[t].convertString());
                        }
                        else
                        {
                            row.Add("");
                        }
                    }

                }


                //fill $;
                if (s.reductionRules.ContainsKey("$"))
                {
                    if (s.reductionRules["$"].nonTerminal == startsymbol)
                    {
                        row.Add("accept");
                    }
                    else
                    {
                        row.Add("red: " + s.reductionRules["$"].convertString());
                    }

                }
                else
                {
                    row.Add("");
                }

                //fill nonTerminals
                foreach (string nt in nonTerminals)
                {
                    if (s.outputs.ContainsKey(nt))
                    {
                        row.Add(s.outputs[nt].name);
                    }
                    else
                    {
                        row.Add("");
                    }
                }
                parsingTable.Rows.Add(row.ToArray());

            }



        }


        private void parseInput(string inp)
        {
            parsingStack.Rows.Add("", "", "", "");
            List<string> stack = new List<string>();
            List<string> input = new List<string>();
            foreach (string item in inp.Split(' '))
            {
                if (item != " " && item != "" && item != "")
                {
                    input.Add(item);
                }
            }

            stack.Add("$");
            stack.Add("0");


            bool accepted = false;
            int iterationNo = 1;
            while (!accepted)
            {
                //row to be added to gui
                List<string> parsingStackRow = new List<string>();

                State lastState = DFAStatesDict[stack.Last()];

                /*Console.WriteLine();
                Console.Write(iterationNo);
                printListInLine(stack);
                Console.Write("|");

                printListInLine(input);
                Console.Write("|");*/
                parsingStackRow.Add(iterationNo + "");
                parsingStackRow.Add(getListInString(stack));
                parsingStackRow.Add(getListInString(input));

                //shift
                if (lastState.outputs.ContainsKey(input.First()))
                {
                    /*Console.Write("shift " + lastState.outputs[input.First()].name);
                    Console.WriteLine();*/
                    parsingStackRow.Add("shift " + lastState.outputs[input.First()].name);
                    stack.Add(input.First());
                    stack.Add(lastState.outputs[input.First()].name);
                    input = shiftInput(input);
                }

                //reduce
                else if (lastState.reductionRules.ContainsKey(input.First()))
                {
                    Rule reductionRule = lastState.reductionRules[input.First()];
                    if (reductionRule.production[0] == "~")
                    {
                        stack.Add(reductionRule.nonTerminal);
                        stack.Add(DFAStatesDict[lastState.name].outputs[reductionRule.nonTerminal].name);
                        /*Console.Write("r (" + reductionRule.convertString() + ")");
                        Console.WriteLine();*/
                        parsingStackRow.Add("r (" + reductionRule.convertString() + ")");

                    }
                    else
                    {
                        int stackindex = stack.LastIndexOf(reductionRule.production[0]);
                        bool matchingStack = true;
                        for (int i = 0; i < reductionRule.production.Count - 1; i++)
                        {
                            //Console.Write(reductionRule.production[i] + "---" + stack[stackindex]);
                            if (reductionRule.production[i] != stack[stackindex])
                            {
                                matchingStack = false;
                                break;
                            }
                            stackindex += 2;
                        }
                        //if reduction can be done
                        if (matchingStack)
                        {
                            if (reductionRule.nonTerminal == startsymbol)
                            {
                                /*Console.Write("accept");
                                Console.WriteLine();*/
                                parsingStackRow.Add("Accept");
                                parsingStack.Rows.Add(parsingStackRow.ToArray());
                                accepted = true;
                                break;
                            }
                            /* Console.Write("R (" + reductionRule.convertString() + ")");
                             Console.WriteLine();*/
                            parsingStackRow.Add("r (" + reductionRule.convertString() + ")");

                            List<string> newstack = new List<string>();
                            for (int i = 0; i < stack.LastIndexOf(reductionRule.production[0]); i++)
                            {
                                newstack.Add(stack[i]);
                            }
                            string laststateofnew = newstack.Last();
                            newstack.Add(reductionRule.nonTerminal);
                            newstack.Add(DFAStatesDict[laststateofnew].outputs[reductionRule.nonTerminal].name);
                            stack = newstack;
                        }
                        else
                        {
                            parsingStack.Rows.Add("ERROR");
                            Console.Write("error in matching");
                            MessageBox.Show("PARSING ERROR");
                            Application.Exit();
                            break;
                        }
                    }
                }
                else
                {
                    parsingStack.Rows.Add("ERROR");
                    Console.WriteLine("error");
                    MessageBox.Show("PARSING ERROR");
                    Application.Exit();
                    break;

                }
                parsingStack.Rows.Add(parsingStackRow.ToArray());
                iterationNo++;
            }
        }



        private List<string> shiftInput(List<string> input)
        {
            List<string> newlist = new List<string>();
            for (int i = 1; i < input.Count; i++)
            {
                newlist.Add(input[i]);
            }
            return newlist;
        }

        private string getListInString(List<string> list)
        {
            string str = "";
            foreach (string item in list)
            {
                str += item + " ";
            }
            return str;
        }




        //first and follow sets
        private void getFirstSet(string nonTerminal)
        {
            List<String[]> rules = new List<String[]>();

            foreach (String rul in productionRulez[nonTerminal].ToString().Split('|'))
            {
                rules.Add(removeEmptyStringsFromArr(rul.Split(' ')));
            }
            foreach (String[] rul in rules)
            {
                if (rul[0] != nonTerminal)
                {
                    if (!firstSets.Contains(nonTerminal))
                    {
                        firstSets.Add(nonTerminal, refineFirstSet(calculateFirst(nonTerminal, rul, 0)));
                    }
                    else
                    {
                        firstSets[nonTerminal] += "," + refineFirstSet(calculateFirst(nonTerminal, rul, 0));
                    }
                }
                else
                {
                    //left recursion check
                    if (nonTerminal == rul[0])
                    {
                        /*MessageBox.Show("Left Recurstion Problem");
                        Application.Exit();*/
                    }
                }
            }
        }


        private string calculateFirst(string nonterminal, String[] rule, int index)
        {
            //if it is a terminal
            if (!productionRulez.Contains(rule[0]) && rule[0] != "~")
            {
                return rule[0];
            }
            //case of non-terminal
            else if (rule[0] != "~" && rule.Length >= 1 && index < rule.Length)
            {

                string fsOfNt = nonTerminalCase(rule[index]);

                if (fsOfNt.Contains("~"))
                {
                    return fsOfNt + calculateFirst(nonterminal, rule, index + 1);
                }

                else
                {
                    return fsOfNt;
                }
            }

            return "~";
        }



        private string nonTerminalCase(string nonTerminal)
        {

            List<String[]> rules = new List<String[]>();

            foreach (String rul in productionRulez[nonTerminal].ToString().Split('|'))
            {
                rules.Add(removeEmptyStringsFromArr(rul.Split(' ')));
            }

            string firstSet = "";

            foreach (String[] rul in rules)
            {
                if (rul[0] != nonTerminal)
                {
                    string fs = calculateFirst(nonTerminal, rul, 0);
                    firstSet += fs + ",";
                }
                else
                {
                    /*MessageBox.Show("Left Recursive Grammer");
                    Application.Exit();*/
                }
            }

            return firstSet;

        }


        private String[] removeEmptyStringsFromArr(string[] array)
        {
            var temp = new List<string>();
            foreach (string s in array)
            {
                if (!string.IsNullOrEmpty(s))
                    temp.Add(s);
            }
            return temp.ToArray();
        }


        //removing extra commas and repeated terminals from first set
        private string refineFirstSet(string firstset)
        {
            string finalFirstSet = "";
            String[] temparr = firstset.Split(',');

            for (int i = 0; i < temparr.Length; i++)
            {
                string item = temparr[i];
                if (item != " " && item != "" && !finalFirstSet.Contains(item))
                {
                    if (i != 0)
                    {
                        finalFirstSet += ",";
                    }
                    finalFirstSet += item;
                }
            }
            return finalFirstSet;
        }


        private void printArr(String[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                Console.WriteLine(i + ": " + arr[i]);
            }
        }


        private void getFollowSet()
        {
            foreach (DictionaryEntry r in productionRulez)
            {
                List<String[]> rules = new List<String[]>();

                foreach (String rul in r.Value.ToString().Split('|'))
                {
                    rules.Add(removeEmptyStringsFromArr(rul.Split(' ')));
                }

                foreach (string[] rule in rules)
                {

                    for (int i = 0; i < rule.Length; i++)
                    {
                        string entry = rule[i];
                        if (productionRulez.Contains(entry))
                        {
                            //non terminal detected in a production rule

                            //case 3
                            if (i == rule.Length - 1 && entry != r.Key.ToString())
                            {
                                addToFollowSet(entry, "follow(" + r.Key.ToString() + ")");
                            }
                            else
                            {
                                //case 1
                                //terminal on right side of non-terminal
                                if (i + 1 < rule.Length)
                                {

                                    if (!productionRulez.Contains(rule[i + 1]))
                                    {
                                        addToFollowSet(entry, rule[i + 1]);
                                    }

                                    //non-terminal on right side of non-terminal
                                    else
                                    {
                                        string firstSet = firstSets[rule[i + 1]].ToString();
                                        //case 3
                                        //first empty, follow of nonterminal is follow of that
                                        if (firstSet == "~" && entry != r.Key.ToString())
                                        {
                                            addToFollowSet(entry, "follow(" + r + ")");
                                        }
                                        //case 2 
                                        //first not empty, add first of non terminal into follow
                                        else
                                        {
                                            string[] firstSetValues = firstSet.Split(',');
                                            foreach (string value in firstSetValues)
                                            {
                                                if (value != "~")
                                                {
                                                    addToFollowSet(entry, value);
                                                }
                                            }
                                        }

                                    }
                                }
                            }

                        }
                    }

                }

            }
        }


        private void runIterationsForFollow()
        {

            followIterations.Add(followSets);

            for (int i = 1; i <= followSets.Count; i++)
            {
                followIterations.Add(new Hashtable());
                for (int j = 0; j < 10; j++)
                {
                    replaceFollowWithUpdated(followIterations[i - 1], followIterations[i]);
                }
            }


        }

        private void replaceFollowWithUpdated(Hashtable followSets, Hashtable newfollow)
        {
            foreach (DictionaryEntry x in followSets)
            {

                string fs = x.Value.ToString();

                newfollow[x.Key] = fs;

                var match = re.Match(fs);

                if (match.Success)
                {
                    int totalmatches = match.Groups.Count;
                    for (int j = 0; j < totalmatches; j++)
                    {
                        string nonterminal = (fs.Split('(')[1].Split(')')[0]);
                        if (newfollow[nonterminal] == null)
                        {
                            string newfollowset = followSets[nonterminal].ToString();
                            string updatedFollow = x.Value.ToString().Replace("follow(" + nonterminal + ")", newfollowset);

                            if (updatedFollow.Contains(x.Key.ToString()))
                            {
                                updatedFollow = updatedFollow.Replace("follow(" + nonterminal + ")", "");
                            }
                            newfollow[x.Key] = updatedFollow;
                        }
                        else
                        {
                            string newfollowset = newfollow[nonterminal].ToString();
                            string updatedFollow = x.Value.ToString().Replace("follow(" + nonterminal + ")", newfollowset);

                            if (updatedFollow.Contains(x.Key.ToString()))
                            {
                                updatedFollow = updatedFollow.Replace("follow(" + nonterminal + ")", "");
                            }

                            newfollow[x.Key] = updatedFollow;
                        }


                    }

                }


            }
        }

        private void addToFollowSet(string nonTerminal, string value)
        {
            if (followSets.Contains(nonTerminal))
            {
                followSets[nonTerminal] += "," + value;
            }
            else
            {
                followSets.Add(nonTerminal, value);
            }
        }

   
    }
}
