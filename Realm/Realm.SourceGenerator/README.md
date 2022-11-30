# Source Generator notes

## Debugging source generator on project

In order to be able to debug source generator on a project in the solution you need to install `.NET Compiler Platform SDK` from the Visual Studio Installer (https://github.com/dotnet/roslyn-sdk/issues/850);
This allows to create a `Roslyn Component` debug launch profile, where it's possible to choose the project the source generator needs to take as input. 

After this is done set the source generator project as the startup project, and select the launch profile in order to be able to run the debugger. 

