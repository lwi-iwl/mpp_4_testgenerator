using System.Collections.Generic;
using System.Reflection;

namespace TestGeneratorLib.FileElements
{
    public class MethodElement
    {
        public string MethodName { get; private set; }
        public List<string> ParameterTypes { get; private set;}
        public List<string> Parameters { get; private set;}
        public string ReturnType { get; private set;}

        public MethodElement(string name, string returnType, List<string> parametersTypes, List<string> parameters)
        {
            MethodName = name;
            ParameterTypes = parametersTypes;
            Parameters = parameters;
            ReturnType = returnType;
        }
    }
}