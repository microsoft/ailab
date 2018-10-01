# Using the code

## Requirements

* Windows 10, Linux or Mac
* .Net Framework
* WPF libraries
* GTK#

## Installation process

You can fork the project in the github repository of [SnipInsights](https://github.com/Microsoft/Snip-Insights).

Then navigate to the code location on your machine and double click the solution file depending on the platform:

- Windows: SnipInsights.WPF.sln
- Linux: SnipInsights.GTK.sln
- Mac OS: SnipInsights.GTK.MacOS.sln


For Linux and Mac OS be sure to set SnipInsight.Forms.GTK as the start up project. 

You can change the default API Keys of Cognitive services used by modifying the files:

- Windows:
	- SnipInsight/APIKeys.cs
- Linux and Mac OS:
	- SnipInsight.Forms/APIKeys.cs