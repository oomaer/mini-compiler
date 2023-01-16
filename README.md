# mini-compiler
Semester Project for Compiler Construction Course

Things that work:
Assignment Expressions
If statements
While Loops

Converts Simple Code into Three Address Code and Shows the working of every stage of compiler
Stages in output:
1. Scanning
2. Parsing
3. Sementic Analysis
4. Three Address Code Generation

Sample Source Code:

int x = 20;<br/>
if  ( 0 < x )<br/>
{<br/>
  int fact = 1;<br/>
  while ( x != 0 )<br/>
  {<br/>
    fact = fact * x;<br/>
    x = x - 1;<br/>
  }<br/>
  print fact<br/>
}<br/>
