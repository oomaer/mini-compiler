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

int x = 20;
if  ( 0 < x ) 
{
	int fact = 1;
	while ( x != 0 ) 
	{
		fact = fact * x;
		x = x - 1;
	}
	print fact
}
