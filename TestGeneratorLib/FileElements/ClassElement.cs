using System.Collections.Generic;

namespace TestGeneratorLib.FileElements
{
    public class ClassElement
    {
        public List<MethodElement> Methods { get; private set; }
        public string ClassName { get; private set; }
        public List<ConstructorElement> Constructors { get; private set; }

        public ClassElement(string className, List<MethodElement> methods, List<ConstructorElement> constructors)
        {
            ClassName = className;
            Methods = methods;
            Constructors = constructors;
        }
    }
}