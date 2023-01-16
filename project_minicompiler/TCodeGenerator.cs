using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace project_minicompiler
{
    class TCodeGenerator
    {

        int t_index = 1;
        int label_index = 1;
        int currRepeatLabel = 1;

        string prevLine = "";

        public Stack<string> blockStack = new Stack<string>();

        Regex varRegex1 = new Regex(@"^\s*(int|String|float|double)\s*([A-Za-z|_][A-Za-z|0-9]{0,20})\s*(=)\s*([a-zA-Z]+|\d+(\.\d+)?)\s*([+\-*/]\s*(\(?[a-zA-Z]+|\d+(\.\d+)?|\(.*\))\s*)*(\)?|);\s*$");
        // line of type String a = 'hello';
        Regex varRegex2 = new Regex(@"^\s*(String)\s*([A-Za-z|_][A-Za-z|0-9]{0,10})\s*(=)\s*[']\s*([A-Za-z|_][A-Za-z|0-9]{0,30})\s*[']\s*(;)\s*$");
        //line of type a = 20;
        Regex varRegex3 = new Regex(@"^\s*([A-Za-z|_][A-Za-z|0-9]{0,20})\s*(=)\s*([a-zA-Z]+|\d+(\.\d+)?)\s*([+\-*/]\s*(\(?[a-zA-Z]+|\d+(\.\d+)?|\(.*\))\s*)*(\)?|);\s*");
        
        Regex expression_Reg = new Regex(@"([a-zA-Z]+|\d+(\.\d+)?)\s*([+\-*/]\s*(\(?[a-zA-Z]+|\d+(\.\d+)?|\(.*\))\s*)*(\)?|)");
        //if regex
        Regex ifexp_Reg = new Regex(@"(\s*if\s*\(\s*([a-zA-Z]+|\d+)\s*(>|<|>=|<=|==|!=)\s*([a-zA-Z]+|\d+)\s*\)){*");
        Regex while_Reg = new Regex(@"(\s*while\s*\(\s*([a-zA-Z]+|\d+)\s*(>|<|>=|<=|==|!=)\s*([a-zA-Z]+|\d+)\s*\)){*");


        Regex emptyArr_regex = new Regex(@"^(\s*)$");

        List<string> result = new List<string>();
        int currentIndex = 0;

        RichTextBox tcodeBox;


        public TCodeGenerator(RichTextBox tcodeBox)
        {
            t_index = 1;
            label_index = 1;
            currRepeatLabel = 1;
            currentIndex = 0;
            result = new List<string>();
            this.tcodeBox = tcodeBox;
        }

        public void generate (string line)
        {

            String tCodeLine;

            if (ifexp_Reg.Match(line).Success)
            {

                blockStack.Push("if");

                String[] splitted = splitString(line, "if");
                tCodeLine = "t" + t_index + " = " + splitted[0];
                result.Insert(currentIndex, tCodeLine);
                currentIndex++;
                tCodeLine = "if_false t" + t_index + " then goto L" + label_index;
                result.Insert(currentIndex, tCodeLine);
                currentIndex++;
                result.Insert(currentIndex, "label L" + label_index);
                label_index++;
                t_index++;
            }
            
            else if (varRegex1.Match(line).Success || varRegex2.Match(line).Success || varRegex3.Match(line).Success)
            {
                String[] splitted = splitString(line, "=");
                tCodeLine = "t" + t_index + " = " + splitted[1];
                result.Insert(currentIndex, tCodeLine);
                currentIndex++;
                tCodeLine = splitted[0] + " = " + "t" + t_index;
                result.Insert(currentIndex, tCodeLine);
                currentIndex++;
                t_index++;
            }

            else if (while_Reg.Match(line).Success)
            {
                blockStack.Push("while");

                result.Insert(currentIndex, "label L" + label_index);
                currentIndex++;
                label_index++;
                String[] splitted = splitString(line, "while");
                tCodeLine = "t" + t_index + " = " + splitted[0];
                result.Insert(currentIndex, tCodeLine);
                currentIndex++;
                tCodeLine = "if_false t" + t_index + " then goto L" + (label_index);
                result.Insert(currentIndex, tCodeLine);
                currentIndex++;
                t_index++;


                tCodeLine = "label L" + label_index;
                currRepeatLabel = label_index;
                result.Insert(currentIndex, tCodeLine);

                tCodeLine = "goto L" + (label_index - 1);
                result.Insert(currentIndex, tCodeLine);

                label_index++;
            }

            else if (line.Contains("}"))
            {
                string top = blockStack.Pop();
                if(top == "while")
                {
                    currentIndex+=2;
                }
                else if(top == "if")
                {
                    currentIndex++;
                }
                else
                {
                    MessageBox.Show("Unexpected }");
                }
                
            }

            else if (line.Contains("{"))
            {
                if(!(ifexp_Reg.Match(prevLine).Success || while_Reg.Match(prevLine).Success)){
                    MessageBox.Show("Unexpected {");
                }
            }
            /*else if (matchUntil.Success)
            {
                String[] splitted = splitString(line, "until");
                tCodeLine = "t" + t_index + " = " + splitted[0];
                result.Insert(currentIndex, tCodeLine);
                currentIndex++;
                tCodeLine = "if_false t" + t_index + " goto L" + currRepeatLabel;
                result.Insert(currentIndex, tCodeLine);
                t_index++;
                currentIndex++;
            }

            else if (line == "end")
            {
                result.Add("halt");
            }

            else if (simpleAssign_regex.Match(line).Success)
            {
                result.Insert(currentIndex, line.Replace(":=", "="));
                currentIndex++;
            }
*/
            else
            {
                tCodeLine = line;
                result.Insert(currentIndex, tCodeLine);
                currentIndex++;

            }

            this.prevLine = line;

        }

        public void output()
        {
            foreach(string line in result)
            {
                tcodeBox.Text += line + "\n";
            }

        }

        private string[] splitString(string str, string deliminator)
        {
            List<string> returnarr = new List<string>();
            string[] splitted = Regex.Split(str, deliminator);
            foreach (string s in splitted)
            {
                if (!emptyArr_regex.Match(s).Success)
                {
                    returnarr.Add(s);
                }
            }
            return returnarr.ToArray();
        }

    }
}
