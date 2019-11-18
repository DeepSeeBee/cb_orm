using CbOrm.Gen;
using CbOrm.Gen.Test;
using CbOrm.Test;
using System;
using System.IO;

namespace CbOrm
{
    class Program
    {
        static void Main(string[] args)
        {            
            var aTestEnabled = false;
            var aGenerateAppModel = true;
            var aTestOk = !aTestEnabled;

            var aDirectoryInfo = new DirectoryInfo(@"..\..");            
            if(aTestEnabled)
            {
                var aSeqFileInfo = new FileInfo(Path.Combine(aDirectoryInfo.FullName, @"Gen\Test\TestSequence.xdl"));
                var aTest = new CGenUnitTest(aSeqFileInfo);
                aTest.Run();
                aTestOk = aTest.Ok.Value;
            }

            if (aGenerateAppModel
            && aTestOk)
            {                
                var aModelInputFileInfo = new FileInfo(Path.Combine(aDirectoryInfo.FullName, @"App\GeneratorInput\cb_orm_vapp_m.xdl"));
                var aIdsInputFileInfo = new FileInfo(Path.Combine(aDirectoryInfo.FullName, @"App\GeneratorInput\cb_orm_vapp_id.xdl"));
                var aModelOutputFileInfo = new FileInfo(Path.Combine(aDirectoryInfo.FullName, @"App\GeneratorOutput", "CbOrm.cs"));
                var aIdsOutputFileInfo = aIdsInputFileInfo;
                var aCodeGenerator = new CCodeGenerator(aModelInputFileInfo, aIdsInputFileInfo, aModelOutputFileInfo, aIdsOutputFileInfo);
                aCodeGenerator.GenerateCode();
            }
        }
    }
}
