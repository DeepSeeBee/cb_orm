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
            }
            this.BeginTest("a278c706-c441-40b7-bd38-16f5a1308a85");
            {
                var aStorage = this.Storage;
                var aC = aStorage.LoadObject<Testf1c0d6f6_cc10_4afb_bc8f_a3a940b5ac4d.C>(aObjectid);
                this.Test(!aC.IsNullRef(), "856494a4-2641-4d93-a98d-244ca83896e8");
            }
        }

        public void Test_c91b9188_dd2b_4b6c_89b9_7df3ab8d7b9b()
        {
            // Generate SkalarField
        }

        public void Test_cfd975a9_b348_4085_9306_bbea67fc771e()
        {
            // Generate 1:NC Relation (Cardinality 1:N, Parent->Child)
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