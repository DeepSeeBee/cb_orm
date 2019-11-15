using CbOrm.Gen;
using System;
using System.IO;

namespace CbOrm
{
    class Program
    {
        static void Main(string[] args)
        {
            var aDirectoryInfo = new DirectoryInfo(@"..\..");
            var aModelInputFileInfo = new FileInfo(Path.Combine(aDirectoryInfo.FullName, @"App\GeneratorInput\orm_model.txt"));
            var aIdsInputFileInfo = new FileInfo(Path.Combine(aDirectoryInfo.FullName, @"App\GeneratorInput\orm_model_ids.txt"));
            var aModelOutputFileInfo = new FileInfo(Path.Combine(aDirectoryInfo.FullName, @"App\GeneratorOutput", "CbOrm.cs"));
            var aIdsOutputFileInfo = aIdsInputFileInfo;
            var aCodeGenerator = new CCodeGenerator(aModelInputFileInfo, aIdsInputFileInfo, aModelOutputFileInfo, aIdsOutputFileInfo);
            aCodeGenerator.GenerateCode();
        }
    }
}
