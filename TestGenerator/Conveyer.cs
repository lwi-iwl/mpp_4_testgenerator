using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TestGenerator
{
    public class Conveyer
    {
        public Task startConveyer(string[] srcFiles, string dstPath, int pipelineLimit)
        {
            Directory.CreateDirectory(dstPath);
            
            var getSrcFiles = new TransformBlock<string, string>
            (
                async path =>
                {
                    using (var reader = new StreamReader(path))
                    {
                        return await reader.ReadToEndAsync();
                    }
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = pipelineLimit }
            );

            var generateTests = new TransformManyBlock<string, KeyValuePair<string, string>>
            (
                async sourceCode =>
                {
                    var fileInfo = await Task.Run(()=> new CodeParser.GetFileInfo(sourceCode));
                    return await Task.Run(()=> TestsGenerator.GenerateTests(fileInfo));
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = pipelineLimit }
            );

            var writeTests = new ActionBlock<KeyValuePair<string, string>>
            (
                async fileNameCodePair =>
                {

                    using (var writer = new StreamWriter(dstPath + '\\' + fileNameCodePair.Key + ".cs"))
                    {
                        await writer.WriteAsync(fileNameCodePair.Value);
                    }
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = pipelineLimit }
            );

            getSrcFiles.LinkTo(generateTests, new DataflowLinkOptions { PropagateCompletion = true });
            generateTests.LinkTo(writeTests, new DataflowLinkOptions { PropagateCompletion = true });
            foreach (string srcFile in srcFiles)
            {
                getSrcFiles.Post(srcFile);
            }

            getSrcFiles.Complete();
            return writeTests.Completion;
        }
    }
}