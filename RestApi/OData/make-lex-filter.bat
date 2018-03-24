CLS
REM Building ANTLR Parser using Alternate C# Target
REM https://github.com/antlr/antlr4/tree/master/runtime/CSharp
java -jar "%USERPROFILE%\.nuget\packages\Antlr4\4.5.3\tools\antlr4-csharp-4.5.3-complete.jar" -Dlanguage=CSharp_v4_5 FilterTranslator.g4
