using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace project_minicompiler
{
    public partial class Form1 : Form
    {

        List<String> keywordList = new List<String>();
        //row is an index counter for symbol table
        int symbolTableRow = 1;
        //count is a variable to incremenet variable id in tokens 
        int varId = 1;
        //line_num is a counter for lines in user input 
        int currLineNo = 0;

        //Hashtable to implement Symbol Table
        Hashtable SymbolTable = new Hashtable();

        List<String> tempArr = new List<String>();
        List<String> linesArray = new List<String>();
        List<String> wordsArray = new List<String>();
        //regex
        //Regular Expression for Variables
        Regex variable_Reg = new Regex(@"^[A-Za-z|_][A-Za-z|0-9]*$");
        //Regular Expression for Constants
        Regex constants_Reg = new Regex(@"^[0-9]+([.][0-9]+)?([e]([+|-])?[0-9]+)?$");
        //Regular Expression for Operators
        Regex operators_Reg = new Regex(@"^[-*+/><&&||=|!=]$");
        //Regular Expression for Special_Characters
        Regex Special_Reg = new Regex(@"^[.,'\[\]{}();:?]$");
        // line of type int alpha = 2;
        Regex varRegex1 = new Regex(@"^\s*(int|String|float|double)\s*([A-Za-z|_][A-Za-z|0-9]{0,20})\s*(=)\s*([a-zA-Z]+|\d+(\.\d+)?)\s*([+\-*/]\s*(\(?[a-zA-Z]+|\d+(\.\d+)?|\(.*\))\s*)*(\)?|);\s*$");
        // line of type String a = 'hello';
        Regex varRegex2 = new Regex(@"^\s*(String)\s*([A-Za-z|_][A-Za-z|0-9]{0,10})\s*(=)\s*[']\s*([A-Za-z|_][A-Za-z|0-9]{0,30})\s*[']\s*(;)\s*$");
        //line of type a = 20;
        Regex varRegex3 = new Regex(@"^\s*([A-Za-z|_][A-Za-z|0-9]{0,20})\s*(=)\s*([a-zA-Z]+|\d+(\.\d+)?)\s*([+\-*/]\s*(\(?[a-zA-Z]+|\d+(\.\d+)?|\(.*\))\s*)*(\)?|);\s*");


        //
        Regex expression_Reg = new Regex(@"([a-zA-Z]+|\d+(\.\d+)?)\s*([+\-*/]\s*(\(?[a-zA-Z]+|\d+(\.\d+)?|\(.*\))\s*)*(\)?|)");

        //if regex
        Regex ifexp_Reg = new Regex(@"(\s*if\s*\(\s*([a-zA-Z]+|\d+)\s*(>|<|>=|<=|==|!=)\s*([a-zA-Z]+|\d+)\s*\)){*");
        Regex while_Reg = new Regex(@"(\s*while\s*\(\s*([a-zA-Z]+|\d+)\s*(>|<|>=|<=|==|!=)\s*([a-zA-Z]+|\d+)\s*\)){*");
        // Regex Special_Reg = new Regex(@"^[.,'\[\]{}();:?]$");

        //GRAMMER RULES TO BE USED
        string declGrammer = 
            "S`>S\n" +
            "S>D id = E ;\n" +
            "D>int | float\n" +
            "E>E + T | E - T | T\n" +
            "T>T * F | F\n" +
            "F>( E ) | id | number";

        string reAssignGrammer =
            "S`>S\n" +
            "S>id = E ;\n" +
            "E>E + T | E - T | T\n" +
            "T>T * F | F\n" +
            "F>( E ) | id | number";

        string ifGrammer =
            "S`>S\n" +
            "S>if ( exp )";

        string whileGrammer =
            "S`>S\n" +
            "S>while ( exp )";




        Parser parser;
        SemanticAnalyzer semanticAnalyzer;
        TCodeGenerator codeGenerator;


        public Form1()
        {
            InitializeComponent();
            keywordList.Add("int");
            keywordList.Add("String");
            keywordList.Add("float");
            keywordList.Add("while");
            keywordList.Add("main");
            keywordList.Add("if");
            keywordList.Add("else");
            keywordList.Add("new");
            keywordList.Add("print");
            keywordList.Add("input");
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            symbolBox.ColumnCount = 5;
            symbolBox.Columns[0].Name = "Index";
            symbolBox.Columns[1].Name = "Variable";
            symbolBox.Columns[2].Name = "Type";
            symbolBox.Columns[3].Name = "Value";
            symbolBox.Columns[4].Name = "Line No.";

            parsingStack.ColumnCount = 4;
            parsingStack.Columns[0].Name = "No.";
            parsingStack.Columns[1].Name = "Stack";
            parsingStack.Columns[2].Name = "Input";
            parsingStack.Columns[3].Name = "Action";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            parser = new Parser(statesBox, parsingTable, parsingStack);
            semanticAnalyzer = new SemanticAnalyzer(sementicRulesBox, sementicAnalyzerBox);
            semanticAnalyzer = new SemanticAnalyzer(sementicRulesBox, sementicAnalyzerBox);
            codeGenerator = new TCodeGenerator(tcodeBox);

            symbolTableRow = 1;
            varId = 1;
            currLineNo = 0;
            SymbolTable = new Hashtable();
            tempArr = new List<string>();
            linesArray = new List<string>();
            wordsArray = new List<string>();
            tokensBox.Clear();
            symbolBox.Rows.Clear();
            parsingStack.Rows.Clear();
            sementicAnalyzerBox.Clear();
            sementicRulesBox.Clear();

            String textBoxInput = inputBox.Text;
            char[] inputArr = textBoxInput.ToCharArray();
            startCompiler(inputArr);
            codeGenerator.output();

            if(codeGenerator.blockStack.Count != 0)
            {
                MessageBox.Show("Error, Expected }");
            }

        }


        private void startCompiler(char[] charinput)
        {
          
            for (int itr = 0; itr < charinput.Length; itr++)
            {


                Match Match_Variable = variable_Reg.Match(charinput[itr] + "");
                Match Match_Constant = constants_Reg.Match(charinput[itr] + "");
                Match Match_Operator = operators_Reg.Match(charinput[itr] + "");
                Match Match_Special = Special_Reg.Match(charinput[itr] + "");

                if (Match_Variable.Success || Match_Constant.Success || Match_Operator.Success || Match_Special.Success || charinput[itr] == ' ')
                {
                    tempArr.Add(charinput[itr] + "");
                }
                if (charinput[itr] == '\n')
                {
                    emptyBuffer(tempArr, linesArray);
                }
            }
            emptyBuffer(tempArr, linesArray);
            //loop through all lines and detemine the type of line
            foreach (string line in linesArray)
            {
     
                startLexer(line);
                if (varRegex1.Match(line).Success)
                {
                    string updtline = Regex.Replace(line, @"\b(?!int|float)\w+\b", "id");
                    updtline = Regex.Replace(updtline, @"(\d[.\d]*){1,99999999}", "number");
                    parser.parse(updtline, declGrammer);
                }
                else if (varRegex2.Match(line).Success)
                { 

                }
                else if (varRegex3.Match(line).Success)
                {
                    string updtline = Regex.Replace(line, @"\b(?!int|float)\w+\b", "id");
                    updtline = Regex.Replace(updtline, @"[0-9](.[0-9])*", "number");
                    parser.parse(updtline, reAssignGrammer);
                }
                else if (ifexp_Reg.Match(line).Success)
                {
                    String updtline = Regex.Replace(line, @"([a-zA-Z]+|\d+)\s*(>|<|>=|<=|==|\!=)\s*([a-zA-Z]+|\d+)", "exp");
                    updtline = removeChars(updtline, new[] { '{' });
                    parser.parse(updtline, ifGrammer);
                }
                else if (while_Reg.Match(line).Success)
                {
                    String updtline = Regex.Replace(line, @"([a-zA-Z]+|\d+)\s*(>|<|>=|<=|==|\!=)\s*([a-zA-Z]+|\d+)", "exp");
                    updtline = removeChars(updtline, new[] { '{' });
                    parser.parse(updtline, whileGrammer);
                }
                else
                {
                    
                }

                wordsArray.Clear();
                tokensBox.AppendText("\n");
                currLineNo += 1;

                semanticAnalyzer.execute(line);
                codeGenerator.generate(line);

            }

        }

        
        















        private void startLexer(string line)
        {
            
            char[] lineChars = line.ToCharArray();
            foreach (char character in lineChars)
            {
                Match Match_Variable = variable_Reg.Match(character + "");
                Match Match_Constant = constants_Reg.Match(character + "");
                Match Match_Operator = operators_Reg.Match(character + "");
                Match Match_Special = Special_Reg.Match(character + "");

                if (Match_Constant.Success || Match_Variable.Success)
                {
                    tempArr.Add(character + "");
                }
                else if (character == ' ')
                {
                    emptyBuffer(tempArr, wordsArray);
                }
                else if (Match_Special.Success || Match_Operator.Success)
                {
                    emptyBuffer(tempArr, wordsArray);
                    wordsArray.Add(character + "");
                }
            }
            emptyBuffer(tempArr, wordsArray);


            for (int i = 0; i < wordsArray.Count; i++)
            {
                bool varfound = false;
                string word = wordsArray[i];
                Match Match_Variable = variable_Reg.Match(word);
                Match Match_Constant = constants_Reg.Match(word);
                Match Match_Operator = operators_Reg.Match(word);
                Match Match_Special = Special_Reg.Match(word);

                if (Match_Constant.Success)
                {
                    tokensBox.AppendText("<number, " + word + "> ");
                }
                else if (Match_Operator.Success)
                {
                    tokensBox.AppendText("<operator, " + word + "> ");
                }
                else if (Match_Special.Success)
                {
                    tokensBox.AppendText("<punc, " + word + "> ");
                }
                else if (Match_Variable.Success && !varfound)
                {
                    if (keywordList.Contains(word))
                    {
                        tokensBox.AppendText("<keyword, " + word + "> ");
                    }
                    else
                    {
                        varfound = true;
                        if (varRegex1.Match(line).Success)
                        {
                            SymbolTable.Add(symbolTableRow*5 + 1, symbolTableRow.ToString()); //index
                            SymbolTable.Add(symbolTableRow*5 + 2, wordsArray[i].ToString()); //variable name
                            SymbolTable.Add(symbolTableRow*5 + 3, wordsArray[i - 1].ToString()); //type
                            //SymbolTable.Add(symbolTableRow*5 + 4, wordsArray[i + 2].ToString()); //value
                            try
                            {
                                SymbolTable.Add(symbolTableRow * 5 + 4, evaluateExpression(expression_Reg.Matches(line)[2].ToString()));
                            }
                            catch
                            {
                                SymbolTable.Add(symbolTableRow * 5 + 4, expression_Reg.Matches(line)[2].ToString());

                            }
                            
                            SymbolTable.Add(symbolTableRow*5 + 5, currLineNo.ToString()); // line number

                            tokensBox.AppendText("<var" + varId + ", " + symbolTableRow + "> ");

                            string[] row = new string[]
                            {
                                SymbolTable[symbolTableRow*5 + 1].ToString(),
                                SymbolTable[symbolTableRow*5 + 2].ToString(),
                                SymbolTable[symbolTableRow*5 + 3].ToString(),
                                SymbolTable[symbolTableRow*5 + 4].ToString(),
                                SymbolTable[symbolTableRow*5 + 5].ToString(),
                            };
                                
                            symbolBox.Rows.Add(row);
                            symbolTableRow+=1;
                            varId++;
            

                        }
                        else if (varRegex2.Match(line).Success)
                        {
                            //prevent null
                            if (!(i <= 0 || i >= wordsArray.Count))
                            {
                                if (!(wordsArray[i - 1].ToString().Equals("'") && wordsArray[i + 1].ToString().Equals("'")))
                                {
                                    SymbolTable.Add(symbolTableRow*5 + 1, symbolTableRow.ToString()); //index
                                    SymbolTable.Add(symbolTableRow*5 + 2, wordsArray[i].ToString()); //variable name
                                    SymbolTable.Add(symbolTableRow*5 + 3, wordsArray[i - 1].ToString()); //type
                                    SymbolTable.Add(symbolTableRow*5 + 4, wordsArray[i + 2].ToString()); //value
                                    SymbolTable.Add(symbolTableRow*5 + 5, currLineNo.ToString()); // line number
                                    tokensBox.AppendText("<var" + varId + ", " + symbolTableRow + "> ");

                                    string[] row = new string[]
                                    {
                                        SymbolTable[symbolTableRow*5 + 1].ToString(),
                                        SymbolTable[symbolTableRow*5 + 2].ToString(),
                                        SymbolTable[symbolTableRow*5 + 3].ToString(),
                                        SymbolTable[symbolTableRow*5 + 4].ToString(),
                                        SymbolTable[symbolTableRow*5 + 5].ToString(),
                                    };

                                    symbolBox.Rows.Add(row);
                                    symbolTableRow+=1;
                                    varId++;
                                }
                            }
 

                        }
                        else if (varRegex3.Match(line).Success)
                        {
                            String ind = "Default";
                            String ty = "Default";
                            String val = "Default";
                            String lin = "Default";
                            bool found = false;
                            foreach (int k in SymbolTable.Keys)
                            {
                                if (SymbolTable[k].Equals(word.ToString()))
                                {
                                    found = true;
                                    ind = SymbolTable[k - 1].ToString();
                                    ty = SymbolTable[k + 1].ToString();
                                    val = SymbolTable[k + 2].ToString();
                                    lin = SymbolTable[k + 3].ToString();
                                    tokensBox.AppendText("<var" + ind + ", " + ind + "> ");
                                    break;
                                }
                            }
                            if (!found)
                            {
                                MessageBox.Show("Error, Variable " + word + "not declared!");
                                Application.Exit();
                            }
                          
                        }
                        else
                        {
                            // if any other category line comes in we check if we have initializes that varaible before,
                            // if we have initiazed it before then we put the index of that variable in symbol table, in its token
                            String ind = "Default";
                            String ty = "Default";
                            String val = "Default";
                            String lin = "Default";

                            foreach (int k in SymbolTable.Keys)
                            {
                                if (SymbolTable[k].Equals(word.ToString()))
                                {
                                    ind = SymbolTable[k-1].ToString();
                                    ty = SymbolTable[k+1].ToString();
                                    val = SymbolTable[k+2].ToString();
                                    lin = SymbolTable[k+3].ToString();
                                    tokensBox.AppendText("<var" + ind + ", " + ind + "> ");
                                    break;
                                }
                            }
                        }

                    }
                }
            }

        }

        private void emptyBuffer(List<string> source, List<string> destination)
        {
            if (source.Count != 0)
            {
                string arraystring = "";
                for (int i = 0; i < source.Count; i++)
                {
                    arraystring += source[i];
                }
                destination.Add(arraystring);
                tempArr.Clear();
            }
        }

        public static double evaluateExpression(string expression)
        {
            DataTable table = new DataTable();
            table.Columns.Add("expression", typeof(string), expression);
            DataRow row = table.NewRow();
            table.Rows.Add(row);
            return double.Parse((string)row["expression"]);
        }

        private string removeChars(string input, char[] chrs)
        {
            var result = string.Concat(input.Where(c => !chrs.Contains(c)));
            return result;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
