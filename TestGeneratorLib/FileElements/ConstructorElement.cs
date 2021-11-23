using System.Collections.Generic;

namespace TestGeneratorLib.FileElements
{
    public class ConstructorElement
    {
        public string ConstructorName { get; private set; }
        public List<string> ParameterTypes { get; private set; }
        public List<string> Parameters { get; private set; }

        public ConstructorElement(string name, List<string> parameterTypes, List<string> parameters)
        {
            ConstructorName = name;
            ParameterTypes = parameterTypes;
            Parameters = parameters;
        }
    }
}