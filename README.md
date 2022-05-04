AdvertisingPolicyValidationTool

About:
This automation tool takes a list of Domains in a CSV file and validates whether it contains key words.
The input file should be in CSV format containing a list of domain IDs and Names.
Keywords are hard coded at the top Program.cs
Results will open upon completion, or can be found in the bin folder in a file named Output.csv 

The result list will include these fields:
Domain ID, CPU Thead, Domain Name, whether keywords where found, List and amounts of keywords found, Any errors while fetching domain. 


Using Program:
To run, simply run solution in Visual Studio debugger, or run exe file in bin folder.
When the command line opens and paste in the input file path. Then select how many CPU threads you would like to use.
NOTE: When running more than 3000 domains, it is best to use half of your CPU to prevent over working your computer.
NOTE: Output file will be over written, copy results to another location.