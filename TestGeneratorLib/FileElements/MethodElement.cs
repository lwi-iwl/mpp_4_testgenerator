using System.Collections.Generic;
using System.Reflection;

namespace TestGeneratorLib.FileElements
{
    public class MethodElement
    {
        public string MethodName { get; private set; }
        public Dictionary<string, string> Parameters { get; private set;}
        public string ReturnType { get; private set;}

        public MethodElement(string name, string returnType, Dictionary<string, string> parameters)
        {
            MethodName = name;
            Parameters = parameters;
            ReturnType = returnType;
        }
    }
}