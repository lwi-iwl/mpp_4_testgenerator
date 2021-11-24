using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestGeneratorLib.FileElements;

namespace TestGeneratorLib
{
    public class CodeParser
    {
        public FileElement GetFileElement(string code)
        {
            CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(code).GetCompilationUnitRoot();
            var classes = new List<ClassElement>();
            foreach (ClassDeclarationSyntax classDeclaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                classes.Add(GetClassElement(classDeclaration));
            }

            return new FileElement(classes);
        }

        private MethodElement GetMethodElement(MethodDeclarationSyntax method)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            foreach (var parameter in method.ParameterList.Parameters) 
            {
                parameters.Add(parameter.Identifier.Text, parameter.Type.ToString());
            }

            return new MethodElement(method.Identifier.ValueText, method.ReturnType.ToString(), parameters);
        }

        private ConstructorElement GetConstructorElement(ConstructorDeclarationSyntax constructor)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            foreach (var parameter in constructor.ParameterList.Parameters)
            {
                parameters.Add(parameter.Identifier.Text, parameter.Type.ToString());
            }

            return new ConstructorElement(constructor.Identifier.ValueText, parameters);
        }

        private ClassElement GetClassElement(ClassDeclarationSyntax classDeclaration)
        {
            var methods = new List<MethodElement>();
            foreach (var method in classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>().Where((methodDeclaration) => methodDeclaration.Modifiers.Any((modifier) => modifier.IsKind(SyntaxKind.PublicKeyword))))
            {
                methods.Add(GetMethodElement(method));
            }

            var constructors = new List<ConstructorElement>();
            foreach (var constructor in classDeclaration.DescendantNodes().OfType<ConstructorDeclarationSyntax>().Where((constructorDeclaration) => constructorDeclaration.Modifiers.Any((modifier) => modifier.IsKind(SyntaxKind.PublicKeyword))))
            {
                Console.WriteLine(constructor.Identifier);
                constructors.Add(GetConstructorElement(constructor));
            }

            return new ClassElement(classDeclaration.Identifier.ValueText, methods, constructors);
        }
    }
}