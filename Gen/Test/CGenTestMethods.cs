﻿using CbOrm.FileSys;
using CbOrm.Schema;
using CbOrm.Storage;
using CbOrm.Util;
using CbOrm.Xdl;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbOrm.Gen.Test
{
    using CTestCase = CRflTyp;
    using CReport = CRflRowList;
    using CTestResult = ExpandoObject;

    public sealed class CTestFailExc : Exception
    {

        public CTestFailExc(CTestResult aTestResult)
        {
            this.TestResult = aTestResult;
        }
        private readonly CTestResult TestResult;
    }

    public class CGenTestMethods
    {
        #region Base
        public CGenTestMethods()
        {
        }
        private readonly Guid TestRunGuid = Guid.NewGuid();

        private CTestCase TestCase;
        private DirectoryInfo TestRunsTempDirectory
        {
            get => new DirectoryInfo(Path.Combine(Path.GetTempPath(), "bffa5489-516c-4244-bd83-15bd5c24f5af", this.TestCase.Name));
        }
        private DirectoryInfo TestRunTempDirectory { get => new DirectoryInfo(Path.Combine(this.TestRunsTempDirectory.FullName, this.TestRunGuid.ToString())); }
        public CSchema Schema { get; private set; }
        private CFileSystemStorage FileSystemStorageM;
        private CFileSystemStorage FileSystemStorage { get => CLazyLoad.Get(ref this.FileSystemStorageM, () => new CFileSystemStorage(this.Schema, this.TestRunTempDirectory)); }
        private CStorage Storage { get => this.FileSystemStorage; }
        public readonly List<CTestResult> TestResults = new List<CTestResult>();
        private string TestMethodId;
        private void Test(bool aOk, string aId)
        {
            var aTestResult = CTestResultBuilder.NewTestResult(this.TestCase, aOk, default, aId, this.TestMethodId);
            this.TestResults.Add(aTestResult);
            if(!aOk)
            {
                throw new CTestFailExc(aTestResult);
            }
        }
        public Exception RunTests(CTestCase aTestCase)
        {
            this.TestCase = aTestCase;
            if(this.TestRunsTempDirectory.Exists)
            {
                this.TestRunsTempDirectory.DeleteRecursive();
            }

            try
            {
                var aTestMethodId = aTestCase.Name;
                this.RunTestMethod(aTestMethodId);
                return default;
            }
            catch(Exception aExc)
            {
                return aExc;
            }
        }
        private void BeginTest(string aId)
        {
            this.FileSystemStorageM = null;
        }
        private void RunTestMethod(string aTestMethodIdArg = null)
        {
            var aTestMethods = this;
            var aTestCase = aTestMethods.TestCase;
            var aTestMethodId = aTestMethodIdArg.AvoidNullString().Length == 0 ? aTestCase.Name : aTestMethodIdArg;
            var aTestMethodName = ("Test_" + aTestCase.Name.Replace("-", "_"))
                                + (aTestMethodId == aTestCase.Name
                                ? string.Empty
                                : ("_" + aTestMethodId.Replace("-", "_"))
                                );
            var aMethodInfo = aTestMethods.GetType().GetMethod(aTestMethodName);
            if (aMethodInfo.IsNullRef())
            {
                System.Diagnostics.Debugger.Break();
                var aTestResult = CTestResultBuilder.NewTestResult(aTestCase, false, default, "Testmethod not found", aTestMethodId);
                this.TestResults.Add(aTestResult);
            }
            else
            {
                bool aOk;
                this.TestMethodId = aTestMethodId;
                try
                {
                    aMethodInfo.Invoke(aTestMethods, new object[] { });
                    aOk  = true;
                }
                catch (Exception aExc)
                {
                    var aTestResult = CTestResultBuilder.NewTestResult(aTestCase, false, default, aExc.Message, aTestMethodId);
                    this.TestResults.Add(aTestResult);
                    aOk = false;
                }
                finally
                {
                    this.TestMethodId = null;
                }
                if(aOk)
                {
                    var aTestResult = CTestResultBuilder.NewTestResult(aTestCase, true, default, string.Empty);
                    this.TestResults.Add(aTestResult);
                }
            }
        }
        #endregion
        public void Test_d9774a1c_5e45_41d9_b259_9b33f0876267()
        {
            // Generate Nothing; Comment
        }

        public void Test_b442b4a1_e615_42ea_ab87_0ea0b994e370()
        {
            // Generate Nothing; RowIgnored
        }

        public void Test_f1c0d6f6_cc10_4afb_bc8f_a3a940b5ac4d()
        {
            // Generate EntityObjectClass
            this.Schema = Testf1c0d6f6_cc10_4afb_bc8f_a3a940b5ac4d.TestSchema.Singleton;
            Guid aObjectid;
            this.BeginTest("c799dd86-80fb-4742-81cb-303c59f2aea4");
            { 
                var aStorage = this.Storage;
                var aC = aStorage.CreateObject<Testf1c0d6f6_cc10_4afb_bc8f_a3a940b5ac4d.C>();
                aObjectid = aC.GuidValue;
                var aSaved = aStorage.Save();
                this.Test(aSaved == 1, "5a762bc4-b38b-4342-baa9-3b08f078f182");
                this.Test(this.FileSystemStorage.DirectoryInfo.GetFilesRecursive().Count() == 2, "c4076989-2eeb-4491-8f27-b9ddb55eec8a");
            }
            this.BeginTest("a278c706-c441-40b7-bd38-16f5a1308a85");
            {
                var aStorage = this.Storage;
                var aC = aStorage.LoadObject<Testf1c0d6f6_cc10_4afb_bc8f_a3a940b5ac4d.C>(aObjectid);
                this.Test(!aC.IsNullRef(), "856494a4-2641-4d93-a98d-244ca83896e8");
            }
            this.BeginTest("080c44a5-39f3-4c95-afb5-370dca6098a0");
            {
                var aStorage = this.Storage;
                var aCs = aStorage.LoadObjects<Testf1c0d6f6_cc10_4afb_bc8f_a3a940b5ac4d.C>();
                this.Test(aCs.Count() == 1, "3064a4ff-9768-4577-ac50-32cfbc754178");
                var aC = aCs.Single();
                aC.Delete();
                this.Test(this.FileSystemStorage.DirectoryInfo.GetFilesRecursive().Count() == 2, "2fd57c03-51f7-4fde-a0b9-417b369b0749");
                var aSaved = aStorage.Save();
                this.Test(aSaved == 1, "cbf18b45-e9fd-40e1-8c89-4214d1e07358");
                var aCs2 = aStorage.LoadObjects<Testf1c0d6f6_cc10_4afb_bc8f_a3a940b5ac4d.C>();
                this.Test(aCs2.Count() == 0, "9c371e6f-d7c9-45fd-971f-0fe394d3ed3c");
                this.Test(this.FileSystemStorage.DirectoryInfo.GetFilesRecursive().Count() == 0, "ce09d4ef-d2df-4be0-bb7d-dbce7c960fde");
            }
        }

        public void Test_c91b9188_dd2b_4b6c_89b9_7df3ab8d7b9b()
        {
            // Generate SkalarField
            this.Schema = Testc91b9188_dd2b_4b6c_89b9_7df3ab8d7b9b.TestSchema.Singleton;
            var aTestData = "ad5533b2-2eb1-4c84-8b7b-7ba2669b8fec";

            this.BeginTest("8e6db121-0105-450e-957a-1179f956f9fc");
            {
                var aStorage = this.Storage;
                var aC = aStorage.CreateObject<Testc91b9188_dd2b_4b6c_89b9_7df3ab8d7b9b.C>();
                aStorage.Save();
                aC.P.Value = aTestData;
                var aSaved = aStorage.Save();
                this.Test(aSaved == 1, "1c43d383-da4f-48b6-9111-1be312cee6c4");
            }
            this.BeginTest("d228502f-600f-4676-9105-5df0df2605a3");
            {
                var aStorage = this.Storage;
                var aC = aStorage.LoadObjects<Testc91b9188_dd2b_4b6c_89b9_7df3ab8d7b9b.C>().Single();
                this.Test(aC.P.Value == aTestData, "ac533f4a-5837-4ea1-bc80-3837a054f1a3");
            }
        }

        public void Test_cfd975a9_b348_4085_9306_bbea67fc771e()
        {
            // Generate 1:NC Relation (Cardinality 1:N, Parent->Child)
            this.Schema = Testcfd975a9_b348_4085_9306_bbea67fc771e.TestSchema.Singleton;

            Guid aChild1Id;
            Guid aChild2Id;

            this.BeginTest("d2b2584b-4d51-4ab5-8125-f7ca1501fbb1");
            {
                var aStorage = this.Storage;
                var aP = aStorage.CreateObject<Testcfd975a9_b348_4085_9306_bbea67fc771e.P>();
                aChild1Id = aP.C.Add().Guid.Value;
                aChild2Id = aP.C.Add().Guid.Value;
                var aSaved = aStorage.Save();
                this.Test(aSaved == 3, "52ef2740-d6d9-4304-8e23-48b9b7d0dc46");
            }

            this.BeginTest("d895ce33-8445-4519-8ece-6d4b28dd1277");
            {
                var aStorage = this.Storage;
                var aP = aStorage.LoadObjects<Testcfd975a9_b348_4085_9306_bbea67fc771e.P>().Single();
                var aGuids = from aTest in aP.C select aTest.Guid.Value;
                this.Test(aGuids.Contains(aChild1Id), "0ea65cb3-6f51-4569-9f0e-c08b1a32ceee");
                this.Test(aGuids.Contains(aChild2Id), "b31e1f7e-8fd2-4ad4-8108-14c145f3cc60");
            }

            this.BeginTest("6d75f770-3d3e-4c80-8d89-497c62e95d09");
            {
                var aStorage = this.Storage;
                var aP = aStorage.LoadObjects<Testcfd975a9_b348_4085_9306_bbea67fc771e.P>().Single();
                var aC = aP.C.GetByGuid(aChild1Id);
                aP.C.Remove(aC);
                var aSaved = aStorage.Save();
                this.Test(aSaved >= 1, "09e639fd-7382-46d0-b5c7-3e2117102fc0"); /// TODO: >= 1...
            }
            this.BeginTest("ef7f6093-8fa3-4d90-a8ff-a027ff7a281f");
            {
                var aStorage = this.Storage;
                var aP = aStorage.LoadObjects<Testcfd975a9_b348_4085_9306_bbea67fc771e.P>().Single();
                var aCs = aP.C.Value;
                this.Test(aCs.Count() == 1, "5c0d5c3a-dbb8-4fbd-be82-44bc71d124f0");
                this.Test(aCs.Single().Guid.Value == aChild2Id, "db9ba7d4-940f-4d5d-9a44-d5c3893c6ac4");
            }
        }

        public void Test_2dff5efa_d964_42c5_98af_d418ede035b9()
        {
            // Generate 1:1C Relation (Cardinality 1:1, Parent->Child)
        }

        public void Test_b7141ae5_956e_4fdb_9e28_b9754b8563c3()
        {
            // Generate 1:NW Relation (Cardinality 1:N, Weak)
        }

        public void Test_cb9fb56f_38ef_439b_af0c_3df00ba1d611()
        {
            // Generate 1:1W Relation (Cardinality 1:1, Weak)
        }


    }
}
