using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace AvaloniaCodeRunner
{
    public static class CodeRunner
    {
        public static CodeExecutionResult RunCode(string code, Type runnableType, object[] parameters)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);

            // define other necessary objects for compilation
            string assemblyName = Path.GetRandomFileName();
            MetadataReference[] references =
            {
                MetadataReference.CreateFromFile(runnableType.Assembly.Location),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            };
            // analyse and generate IL code from syntax tree
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] {syntaxTree},
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using var ms = new MemoryStream();
            // write IL code into memory
            EmitResult result = compilation.Emit(ms);

            if (!result.Success)
            {
                // handle exceptions
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                string errorText = failures.Aggregate("", (current, diagnostic)
                    => current + (($"{diagnostic.Id}: {diagnostic.GetMessage()}") + Environment.NewLine));

                return new CodeExecutionResult() {Errors = errorText};
            }

            // load this 'virtual' DLL so that we can use
            ms.Seek(0, SeekOrigin.Begin);
            Assembly assembly = Assembly.Load(ms.ToArray());

            // create instance of the desired class and call the desired function

            var typeToExecute = assembly
                .GetTypes().FirstOrDefault(runnableType.IsAssignableFrom);
            var obj = Activator.CreateInstance(typeToExecute);

            const BindingFlags flags = BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance;
            var methodToExecute = typeToExecute.GetMethods(flags).FirstOrDefault();


            var executionResult = runnableType.InvokeMember(methodToExecute.Name,
                BindingFlags.Default | BindingFlags.InvokeMethod,
                null,
                obj,
                parameters);
            return new CodeExecutionResult() {Result = executionResult};
        }
    }

    public class CodeExecutionResult
    {
        public string? Errors { get; set; }
        public object? Result { get; set; }
    }
}