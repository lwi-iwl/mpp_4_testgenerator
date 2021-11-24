using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TestGeneratorLib;
using TestGeneratorLib.FileElements;

namespace TestProject1
{
    public class Tests
    {
        private string _firstClassString;
        private string _secondClassString;
        private TestsGenerator _testGenerator;
        private CodeParser _codeParser;
        [SetUp]
        public void Setup()
        {
            var firstClassPath = "C:\\Users\\nikst\\RiderProjects\\TestGenerator\\Example\\1.cs";
            var secondClassPath = "C:\\Users\\nikst\\RiderProjects\\TestGenerator\\Example\\2.cs";
            string[] firstClassStringArray =  File.ReadAllLines(firstClassPath);
            string[] secondClassStringArray = File.ReadAllLines(secondClassPath);

            _firstClassString = String.Join("\n", firstClassStringArray);
            _secondClassString = String.Join("\n", secondClassStringArray);
            
            _testGenerator = new TestsGenerator();
            _codeParser = new CodeParser();
        }

        [Test]
        public void FirstFileClassCount()
        {

            FileElement fileElement = _codeParser.GetFileElement(_firstClassString);
            Console.WriteLine(_firstClassString);
            Assert.AreEqual(2, fileElement.Classes.Count);
        }
        
        [Test]
        public void FirstFileClassesName()
        {
            FileElement fileElement = _codeParser.GetFileElement(_firstClassString);
            Assert.AreEqual("NewClass2", fileElement.Classes[0].ClassName);
            Assert.AreEqual("NewClass3", fileElement.Classes[1].ClassName);
        }
        
        [Test]
        public void FirstClassMethodsCount()
        {
            FileElement fileElement = _codeParser.GetFileElement(_firstClassString);
            Assert.AreEqual(1, fileElement.Classes[0].Methods.Count);
        }
        
        [Test]
        public void FirstClassConstructorsCount()
        {
            FileElement fileElement = _codeParser.GetFileElement(_firstClassString);
            Assert.AreEqual(0, fileElement.Classes[0].Constructors.Count);
        }
        
        [Test]
        public void SecondClassMethodsCount()
        {
            FileElement fileElement = _codeParser.GetFileElement(_firstClassString);
            Assert.AreEqual(1, fileElement.Classes[1].Methods.Count);
        }
        
        [Test]
        public void SecondClassConstructorsCount()
        {
            FileElement fileElement = _codeParser.GetFileElement(_firstClassString);
            Assert.AreEqual(0, fileElement.Classes[1].Constructors.Count);
        }
        
        [Test]
        public void SecondFileClassCount()
        {

            FileElement fileElement = _codeParser.GetFileElement(_secondClassString);
            Console.WriteLine(_secondClassString);
            Assert.AreEqual(1, fileElement.Classes.Count);
        }
        
        [Test]
        public void SecondFileClassesName()
        {
            FileElement fileElement = _codeParser.GetFileElement(_secondClassString);
            Assert.AreEqual("NewClass1", fileElement.Classes[0].ClassName);
        }
        
        [Test]
        public void ClassMethodsCount()
        {
            FileElement fileElement = _codeParser.GetFileElement(_secondClassString);
            Assert.AreEqual(2, fileElement.Classes[0].Methods.Count);
        }
        
        [Test]
        public void ClassConstructorsCount()
        {
            FileElement fileElement = _codeParser.GetFileElement(_secondClassString);
            Assert.AreEqual(1, fileElement.Classes[0].Constructors.Count);
        }
        
        [Test]
        public void FirstFileTestCount()
        {
            FileElement fileElement = _codeParser.GetFileElement(_firstClassString);
            Dictionary<string, string> tests = _testGenerator.GenerateTests(fileElement);
            Assert.AreEqual(2, tests.Count);
        }
        
        [Test]
        public void SecondFileTestCount()
        {
            FileElement fileElement = _codeParser.GetFileElement(_secondClassString);
            Dictionary<string, string> tests = _testGenerator.GenerateTests(fileElement);
            Assert.AreEqual(1, tests.Count);
        }
        
        [Test]
        public void ThirdTestMockCount()
        {
            FileElement fileElement = _codeParser.GetFileElement(_secondClassString);
            var constructor = _testGenerator.GetConstructor(fileElement.Classes[0]);
            Console.WriteLine(fileElement.Classes.Count);
            var interfaces = _testGenerator.GetInterfaces(constructor.Parameters);
            var mocks = _testGenerator.GetConstructorFields(interfaces);
            Assert.AreEqual(1, mocks.Count);
        }
    }
}