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
    public class TestsGenerator
    {
        public Dictionary<string, string> GenerateTests(FileElement fileElement)
        {
            var fileCode = new Dictionary<string, string>();
            foreach (var classInfo in fileElement.Classes)
            {
                var classDeclaration = GenerateClass(classInfo);
                var compilationUnit = SyntaxFactory.CompilationUnit()
                    .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                    .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("NUnit.Framework")))
                    .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Moq")))
                    .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")))
                    .AddMembers(classDeclaration);
                fileCode.Add(classInfo.ClassName + "Test",
                    compilationUnit.NormalizeWhitespace().ToFullString());
            }

            return fileCode;
        }
        
        private ClassDeclarationSyntax GenerateClass(ClassElement classElement)
        {
            var mockFields = new List<FieldDeclarationSyntax>();
            var interfaces = new Dictionary<string, string>();
            ConstructorElement constructor = null;
            var methods = new List<MethodDeclarationSyntax>();
            if (classElement.Constructors.Count > 0)
            {
                constructor = GetConstructor(classElement);
                interfaces = GetInterfaces(constructor.Parameters);
                mockFields = GetConstructorFields(interfaces);
            }

            List<FieldDeclarationSyntax> fields = mockFields.Concat(GetFields(classElement)).ToList();
            methods = GetMethods(constructor, classElement);

            return SyntaxFactory.ClassDeclaration(classElement.ClassName + "Test")
                .AddMembers(fields.ToArray())
                .AddMembers(methods.ToArray())
                .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.AttributeList()
                    .Attributes.Add(SyntaxFactory.Attribute(SyntaxFactory.ParseName("TestFixture")))));
        }

        public ConstructorElement GetConstructor(ClassElement classElement)
        {
            ConstructorElement  constructor = classElement.Constructors[0];
            foreach (var temp in classElement.Constructors)
            {
                if (constructor.Parameters.Count < temp.Parameters.Count)
                {
                    constructor = temp;
                }
            }

            return constructor;
        }

        public Dictionary<string, string> GetInterfaces(Dictionary<string, string> parameters)
        {
            Dictionary<string, string> interfaces = new Dictionary<string, string>();
            foreach (var parameter in parameters)
            {
                if (parameter.Value[0] == 'I')
                {
                    interfaces.Add(parameter.Key, parameter.Value);
                }
            }

            return interfaces;
        }
        
        private Dictionary<string, string> GetVariables(Dictionary<string, string> parameters)
        {
            Dictionary<string, string> interfaces = new Dictionary<string, string>();
            foreach (var parameter in parameters)
            {
                if (parameter.Value[0] != 'I')
                {
                    interfaces.Add(parameter.Key, parameter.Value);
                }
            }

            return interfaces;
        }

        public List<FieldDeclarationSyntax> GetConstructorFields(Dictionary<string, string> interfaces)
        {
            List<FieldDeclarationSyntax> fields = new List<FieldDeclarationSyntax>();
            foreach (var custom in interfaces)
            {
                VariableDeclarationSyntax variable = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName($"Mock<{custom.Value}>"))
                    .AddVariables(SyntaxFactory.VariableDeclarator("_" + custom.Key));
                fields.Add(SyntaxFactory.FieldDeclaration(variable)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));
            }

            return fields;
        }

        private List<FieldDeclarationSyntax> GetFields(ClassElement classElement)
        {            
            string classVariable = "_" + classElement.ClassName[0].ToString().ToLower() + classElement.ClassName.Remove(0, 1);
            var variable = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(classElement.ClassName))
                .AddVariables(SyntaxFactory.VariableDeclarator(classVariable));

            List<FieldDeclarationSyntax> fields = new List<FieldDeclarationSyntax>();
            fields.Add(SyntaxFactory.FieldDeclaration(variable)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));
            return fields;
        }

        public List<MethodDeclarationSyntax> GetMethods(ConstructorElement constructor, ClassElement classElement)
        {
            List<MethodDeclarationSyntax> methods = new List<MethodDeclarationSyntax>();
            methods.Add(GetSetUpMethod(constructor, classElement.ClassName));
            foreach (var methodInfo in classElement.Methods)
            {
                methods.Add(GenerateMethod(methodInfo, classElement.ClassName));
            }

            return methods;
        }

        private string ParametersToString(Dictionary<string, string> parameters)
        {
            var s = "";
            foreach (var pair in parameters)
            {
                if (pair.Value[0] == 'I')
                    s += $"_{pair.Key}.Object";
                else
                    s += $"{pair.Key}";
                s += ", ";
            }

            if (s.Length > 0)
                s = s.Remove(s.Length - 2, 2);
            return s;
        }
        private MethodDeclarationSyntax GetSetUpMethod(ConstructorElement constructorElement, string className)
        {
            List<StatementSyntax> body = new List<StatementSyntax>();
            if (constructorElement != null)
            {
                var variables = GetVariables(constructorElement.Parameters);
                foreach (var var in variables)
                {
                    body.Add(SyntaxFactory.ParseStatement(string.Format
                    (
                        "var {0} = default({1});",
                        var.Key,
                        var.Value
                    )));
                }

                var customVars = GetInterfaces(constructorElement.Parameters);
                foreach (var var in customVars)
                {
                    SyntaxFactory.ParseStatement(string.Format
                    (
                        "{0} = new {1}{2};",
                        "_" + var.Key,
                        $"Mock<{var.Value}>",
                        "()"
                    ));
                }

                body.Add(SyntaxFactory.ParseStatement(string.Format
                    (
                        "{0} = new {1}{2};",
                        "_" + className[0].ToString().ToLower() + className.Remove(0, 1),
                        className,
                        $"({ParametersToString(constructorElement.Parameters)})"
                    )));
            }
            else 
                body.Add(SyntaxFactory.ParseStatement(string.Format
                    (
                        "{0} = new {1}{2};",
                        "_" + className[0].ToString().ToLower() + className.Remove(0, 1),
                        className,
                        "()"
                    )));
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "SetUp")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.AttributeList().Attributes.Add(SyntaxFactory.Attribute(SyntaxFactory.ParseName("SetUp")))))
                .WithBody(SyntaxFactory.Block(body)); ;
        }
        
        private MethodDeclarationSyntax GenerateMethod(MethodElement methodElement, string checkedClassVar)
        {
            List<StatementSyntax> body = new List<StatementSyntax>();
            body = VariablesToString(methodElement);
            body = body.Concat(GetActPart(methodElement, checkedClassVar)).ToList();
            if (methodElement.ReturnType != "void")
            {
                body = body.Concat(GetAssertPart(methodElement.ReturnType)).ToList();
            }

            body.Add(SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("Assert"),
                            SyntaxFactory.IdentifierName("Fail")))
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal("autogenerated"))))))));
            
            return SyntaxFactory.MethodDeclaration( SyntaxFactory.ParseTypeName("void"), methodElement.MethodName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.AttributeList().Attributes.Add(SyntaxFactory.Attribute(SyntaxFactory.ParseName("Test")))))
                .WithBody(SyntaxFactory.Block(body)); ;
        }

        private List<StatementSyntax> GetAssertPart(string returnType)
        {
            List<StatementSyntax> body = new List<StatementSyntax>();
            body.Add(SyntaxFactory.ParseStatement(string.Format
            (
                "var {0} = default({1});",
                "expected",
                returnType
            )));
            var invocationExpression = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("Assert"),
                    SyntaxFactory.IdentifierName("That")));
            var secondPart = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("Is"),
                    SyntaxFactory.IdentifierName("EqualTo"))).WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(
                new SyntaxNodeOrToken[] {
                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("expected"))})));
            var argList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(
                new SyntaxNodeOrToken[] {
                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("actual")),
                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName(secondPart.ToString()))}));

            var s = SyntaxFactory.ExpressionStatement(invocationExpression.WithArgumentList(argList));
            body.Add(s);
            return body;
        }
        private List<StatementSyntax> VariablesToString(MethodElement methodElement)
        {
            List<StatementSyntax> body = new List<StatementSyntax>();
            var baseTypeVars = GetVariables(methodElement.Parameters);
            foreach (var var in baseTypeVars)
            {
                body.Add(SyntaxFactory.ParseStatement(string.Format
                (
                    "var {0} = default({1});",
                    var.Key,
                    var.Value
                )));
            }

            return body;
        }
        
        private List<StatementSyntax> GetActPart(MethodElement methodElement, string checkedClassVariable)
        {
            List<StatementSyntax> body = new List<StatementSyntax>();
            if (methodElement.ReturnType != "void")
            {
                body.Add(SyntaxFactory.ParseStatement(string.Format
                (
                    "var {0} = {1}{2};",
                    "actual",
                    "_" + checkedClassVariable[0].ToString().ToLower() + checkedClassVariable.Remove(0, 1)+ "." + methodElement.MethodName,
                    $"({ParametersToString(methodElement.Parameters)})"
                )));
            }
            else
            {
                body.Add(SyntaxFactory.ParseStatement(string.Format
                (
                    "{0}{1};",
                    "_" + checkedClassVariable[0].ToString().ToLower() + checkedClassVariable.Remove(0, 1) + "." + methodElement.MethodName,
                    $"({ParametersToString(methodElement.Parameters)})"
                )));
            }

            return body;
        }
        
    }
}