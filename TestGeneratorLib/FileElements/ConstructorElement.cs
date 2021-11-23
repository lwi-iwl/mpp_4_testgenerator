using System.Collections.Generic;

namespace TestGeneratorLib.FileElements
{
    public class ConstructorElement
    {
        public string ConstructorName { get; private set; }
        public Dictionary<string, string> Parameters { get; private set; }

        public ConstructorElement(string name, Dictionary<string, string> parameters)
        {
            ConstructorName = name;
            Parameters = parameters;
        }
    }
}