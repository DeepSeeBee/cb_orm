using CbOrm.Test;
using CbOrm.Xdl;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CbOrm.Util;
using CbOrm.Loader;
using System.Dynamic;

namespace CbOrm.Gen.Test
{
    using CTestCase = CRflTyp;
    using CTestFileCombo = Tuple<FileInfo, FileInfo>; // In, Out
    using CTestRun = Tuple<CGenUnitTest, CRflTyp>; // ..., TestCase
    using CReport = CRflRowList;
    using CTestResult = ExpandoObject;
    using CTestSequence = CRflModel;
    using static CbOrm.Test.CUnitTest;

    public class CInterceptor
    {

        public event EventHandler Intercepting;
        public bool Accepted;
        public virtual void OnIntercept(EventArgs aArgs)
        {
            //System.Diagnostics.Debugger.Break();
            if(!this.Intercepting.IsNullRef())
            {
                this.Intercepting(this, aArgs);
            }
        }
    }

    public sealed class CTestResultEventArgs : EventArgs
    {
        public CTestResultEventArgs(CTestCase aTestCase, 
                                      IEnumerable<CTestResult> aTestResults)
        {
            this.TestCase = aTestCase;
            this.TestResults = aTestResults;
        }
        public readonly CTestCase TestCase;
        public readonly IEnumerable<CTestResult> TestResults;
        public CTestResult TestResult { get => this.TestResults.Last(); }
    }

    public sealed class CTestModelInterpreter : CRflModelInterpreter
    {
        public FileInfo GetInModelFileInfo(CTestRun aTestRun)
        {
            var aUnitTest = aTestRun.Item1;
            var aTestCase = aTestRun.Item2;
            var aDir = aUnitTest.TestCasesDirectoryInfo;
            var aInModelFileInfo = new FileInfo(Path.Combine(aDir.FullName, aTestCase.Name + "-in.xdl"));
            return aInModelFileInfo;
        }

        public IEnumerable<CTestFileCombo> GetTestFiles(CTestRun aTestRun)
        {
            var aUnitTest = aTestRun.Item1;
            var aTestCase = aTestRun.Item2;
            var aDir = aUnitTest.TestCasesDirectoryInfo;            
            var aOutTestFileInfo = new FileInfo(Path.Combine(aDir.FullName, aTestCase.Name + "-out-test.cs"));
            var aOutOkFileInfo = new FileInfo(Path.Combine(aDir.FullName, aTestCase.Name + "-out-ok.cs"));
            var aTestFileCombo = new CTestFileCombo( aOutTestFileInfo, aOutOkFileInfo);
            yield return aTestFileCombo;
        }
        public void AddTestResult(CReport aReport, CTestResult aTestResult) => this.Add(aReport, aTestResult, "TestResult");
    }

    public sealed class CGenUnitTest : CUnitTest
    {
        public CTestModelInterpreter TestModelInterperter = new CTestModelInterpreter();
        public CGenUnitTest(FileInfo aSequenceFile) : this(aSequenceFile, new DirectoryInfo(Path.Combine(aSequenceFile.Directory.FullName, "TestCases"))) 
        {
        }
        public CGenUnitTest(FileInfo aSequenceFileInfo, DirectoryInfo aTestCasesDirectoryInfo)
        {
            this.SequenceFileInfo = aSequenceFileInfo;
            this.TestCasesDirectoryInfo = aTestCasesDirectoryInfo;
            this.TestSequence = CRflModel.NewFromTextFile(this.TestModelInterperter, this.SequenceFileInfo); 
            this.OutReportFileInfo = new FileInfo(this.SequenceFileInfo.FullName + "-report.xdl");
        }

        public readonly FileInfo SequenceFileInfo;
        public readonly DirectoryInfo TestCasesDirectoryInfo;
        public readonly CInterceptor TestInterceptor = new CInterceptor();
        public readonly CReport Report = new CReport();
        public readonly CTestSequence TestSequence;
        public readonly FileInfo OutReportFileInfo;
        public bool? Ok = true;
        public CGenTokens Tok = new CGenTokens();

        public void SaveReport()
        {
            this.Report.SaveAsText(this.OutReportFileInfo); 
        }

        public void Run(CTestCase aTestCase)
        {
            var aReport = this.Report;
            var aTestSequence = this.TestSequence;
            
            var aDir = this.TestCasesDirectoryInfo;
            var aTestInterceptor = this.TestInterceptor;
            var aInterceptTemplate = new Action<CTestResultEventArgs>(delegate(CTestResultEventArgs aArgs)
            {
                aTestInterceptor.Accepted = (bool)aArgs.TestResult.Dyn().Ok;
                aTestInterceptor.OnIntercept(aArgs);
                if (!aTestInterceptor.Accepted)
                    throw new Exception("TestCase interceptur did not accept.");
            });


            var aInModelFileInfo = new FileInfo(Path.Combine(aDir.FullName, aTestCase.Name + "-in.xdl"));
            var aOutTestFileInfo = new FileInfo(Path.Combine(aDir.FullName, aTestCase.Name + "-out-test.cs"));
            var aOutOkFileInfo = new FileInfo(Path.Combine(aDir.FullName, aTestCase.Name + "-out-ok.cs"));

            System.Diagnostics.Debug.Print("Running Test: " + aTestCase.Name);

            if (aInModelFileInfo.Exists)
            {
                var aGenerator = new CCodeGenerator(new CGenModelInterpreter(),
                                                    new CGenTokens(),
                                                    new CCodeDomBuilder(),
                                                    aInModelFileInfo,
                                                    null,
                                                    aOutTestFileInfo,
                                                    null);
                var aTestCaseId = aTestCase.Name;
                var aNamespace = "Test" + aTestCaseId.Replace("-", "_");
                var aNamespaceRow = CRflRow.New(string.Empty, string.Empty, this.Tok.Mdl_G_A_Nsp_Nme, aNamespace);
                var aSchemaRow = CRflRow.New(string.Empty, string.Empty, this.Tok.Mdl_G_A_Schema, "Test");
                aGenerator.Exp.ChainedExpanders.Add(new CRowsExpander(aNamespaceRow, aSchemaRow));
                aGenerator.GenerateCode();

                var aOutTestLines = File.ReadAllLines(aOutTestFileInfo.FullName);
                if (!aOutTestFileInfo.Exists)
                {
                    System.Diagnostics.Debugger.Break(); // First Test detected. Continue to accept.
                    File.WriteAllLines(aOutTestFileInfo.FullName, aOutTestLines);
                }
                else if (!aOutOkFileInfo.Exists)
                {
                    System.Diagnostics.Debugger.Break(); // First Test-Validation detected. Continue to accept.
                    File.WriteAllLines(aOutOkFileInfo.FullName, aOutTestLines);
                }
                else
                {
                    try
                    {
                        Action aAcceptAction;
                        var aTestResults  = new List<CTestResult>();
                        var aOkLines = File.ReadAllLines(aOutOkFileInfo.FullName);
                        if (aOutTestLines.Length != aOkLines.Length)
                        {
                            aTestResults.Add( CTestResultBuilder.NewTestResult(aTestCase, false, default(int?), "Number of generated rows missmatch."));
                            aAcceptAction = new Action(delegate () { File.WriteAllLines(aOutOkFileInfo.FullName, aOutTestLines); });                           
                        }
                        else
                        {
                            var aLinePairs = from aIdx in Enumerable.Range(0, aOutTestLines.Length) select new Tuple<int, string, string>(aIdx, aOkLines[aIdx], aOutTestLines[aIdx]);
                            var aFirstDiff = (from aTest in aLinePairs
                                              where aTest.Item2 != aTest.Item3
                                              select aTest).FirstOrDefault();
                            if (aFirstDiff.IsNullRef())
                            {
                                var aTestMethods = new CGenTestMethods();
                                var aExcNullable = aTestMethods.RunTests(aTestCase);
                                aTestResults.AddRange(aTestMethods.TestResults);
                                aTestResults.Add(CTestResultBuilder.NewTestResult(aTestCase, aExcNullable.IsNullRef(), default, aExcNullable.IsNullRef() ? string.Empty : aExcNullable.Message));
                                aAcceptAction = new Action(delegate () { });
                            }
                            else
                            {
                                aTestResults.Add(CTestResultBuilder.NewTestResult(aTestCase, false, aFirstDiff.Item1 + 1, "Difference found."));
                                aAcceptAction = new Action(delegate () {File.WriteAllLines(aOutOkFileInfo.FullName, aOutTestLines); });                               
                            }
                        }
                        foreach (var aTestResult in aTestResults)
                        {
                            this.TestModelInterperter.AddTestResult(aReport, aTestResult);
                        }
                        aInterceptTemplate(new CTestResultEventArgs(aTestCase, aTestResults));
                        aAcceptAction();
                    }
                    finally
                    {
                        this.SaveReport();
                    }
                }
            }
            else
            {
                System.Diagnostics.Debugger.Break(); // New TestCase detected. Continue to create in-file.
                File.WriteAllText(aInModelFileInfo.FullName, string.Empty);

            }
        }

        public override void Run()
        {
            var aTestSequence = this.TestSequence;
            var aTestCases = aTestSequence.Typs;
            foreach (var aTestCase in aTestCases)
            {
                this.Run(aTestCase);

            }
            this.SaveReport();
            this.Ok = true;
        }
    }
    public static class CTestResultBuilder
    {
        public static CTestResult NewTestResult(CTestCase aTestCase,
                                                         bool aOk,
                                                         int? aFirstDiffRowNr,
                                                         string aText,
                                                         string aTestMethod = null
                                                         )
        {
            var aExpandoObject = new ExpandoObject();
            aExpandoObject.Dyn().TestCase = aTestCase.Name;
            aExpandoObject.Dyn().TestMethod = aTestMethod.AvoidNullString();
            aExpandoObject.Dyn().Ok = aOk;
            aExpandoObject.Dyn().FirstDiffRowNr = aFirstDiffRowNr;
            aExpandoObject.Dyn().Text = aText;
            return aExpandoObject;
        }

    }

}
