using System.Collections.Generic;

namespace TestGeneratorLib.FileElements
{
    public class FileElement
    {
        public List<ClassElement> Classes{ get; private set; }

        public FileElement(List<ClassElement> classes)
        {
            this.Classes = classes;
        }
    }
}