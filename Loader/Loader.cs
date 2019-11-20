using CbOrm.Gen;
using CbOrm.Xdl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbOrm.Loader
{

    public abstract class CModelExpander
    {
        public static CModelExpander NewDefault()
        {
            return new CNullModelExpander();
        }

        public abstract CRflModel Expand(CRflModel Model);
    }

    public sealed class CNullModelExpander : CModelExpander
    {
        public override CRflModel Expand(CRflModel Model) => Model;
    }
    public sealed class NewIdsModelExpander : CModelExpander
    {
        public override CRflModel Expand(CRflModel aModel)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class CNewCrossRefModelExpander : CModelExpander
    {
        public override CRflModel Expand(CRflModel Model)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class CIncludeModelExpander : CModelExpander
    {
        public interface IModelInterpreter
        {
            IEnumerable<Tuple<CRflRow, FileInfo>> GetIncludes(CRflModel aModel);
            CRflModel NewIncludedModel(FileInfo aFileInfo);
        }
        public CIncludeModelExpander(IModelInterpreter aModelInterpreter)
        {
            this.ModelInterpreter = aModelInterpreter;
        }
        private readonly IModelInterpreter ModelInterpreter;
        public override CRflModel Expand(CRflModel aModel)
        {
            var aTmpModel = aModel;
            bool aInludeFound;
            do
            {
                var aIncludes = this.ModelInterpreter.GetIncludes(aTmpModel).ToArray();
                if (aIncludes.Length > 0)
                {
                    var aInclude = aIncludes.First();
                    var aIncludeRow = aInclude.Item1;
                    var aIncludeFileInfo = aInclude.Item2;
                    var aIncludedModelUnexpanded = this.ModelInterpreter.NewIncludedModel(aIncludeFileInfo);
                    var aInclduedModelExpanded = this.Expand(aIncludedModelUnexpanded);
                    var aRows1 = aTmpModel.Rows.TakeWhile(aRow => !object.ReferenceEquals(aIncludeRow, aRow));
                    var aRows2 = aInclduedModelExpanded.Rows;
                    var aRows3 = aTmpModel.Rows.Reverse().TakeWhile(aRow => !object.ReferenceEquals(aIncludeRow, aRow)).Reverse();
                    var aRows4 = aRows1.Concat(aRows2).Concat(aRows3);
                    var aRows = aRows4.ToArray();
                    aTmpModel = new CRflModel(aTmpModel.ModelInterpreter, aModel.FileInfo, aRows);
                    aInludeFound = true;
                }
                else
                {
                    aInludeFound = false;
                }
            }
            while (aInludeFound);
            return aTmpModel;
        }
    }

    public sealed class CDefaultOrderModelExpander : CModelExpander
    {
        public override CRflModel Expand(CRflModel Model)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class CChainedModelExpander : CModelExpander
    {

        public readonly List<CModelExpander> ChainedExpanders = new List<CModelExpander>();

        public override CRflModel Expand(CRflModel aModel)
        {
            var aTmpModel = aModel;
            foreach(var aExpander in this.ChainedExpanders)
            {
                aTmpModel = aExpander.Expand(aTmpModel);
            }
            return aTmpModel;
        }
    }



    public sealed class CRowsExpander : CModelExpander
    {
        public CRowsExpander(params CRflRow[] aRows)
        {
            this.Rows.AddRange(aRows);
        }
        public readonly List<CRflRow> Rows = new List<CRflRow>();
        public override CRflModel Expand(CRflModel aModel)
        {
            return new CRflModel(aModel.ModelInterpreter, aModel.FileInfo, aModel.Rows.Concat(this.Rows));
        }

    }

    internal sealed class CCrossReferenceExpander : CModelExpander
    {
        internal CCrossReferenceExpander(CGenModelInterpreter aGenModelInterpreter)
        {
            this.ModelInterpreter = aGenModelInterpreter;
        }
        private readonly CGenModelInterpreter ModelInterpreter;
        public override CRflModel Expand(CRflModel aModel)
        {
            var aModelInterpreter = this.ModelInterpreter;

            var aTyps = aModel.Typs;
            var aEntityObjectProperties = from aTyp in aTyps
                                          from aProperty in aTyp.NamedProperties
                                          where aModelInterpreter.GetIsObject(this.ModelInterpreter.GetReturnTyp(aProperty))
                                          select aProperty;
            var aR1NCProperties = from aProperty in aEntityObjectProperties
                                  where aModelInterpreter.GetCardinality(aProperty) == CCardinalityEnum.R1NC
                                  select aProperty;
            var aR1NCFkRows = from aProperty in aR1NCProperties
                              from aRow in aModelInterpreter.NewR1NCForeignKeyRows(aProperty)
                              select aRow
                               ;
            var aR1NCRevereRows = from aProperty in aR1NCProperties
                                  from aRow in aModelInterpreter.NewR1NCReverseNavigationRows(aProperty)
                                  select aRow;

            var aR11CProperties = from aProperty in aEntityObjectProperties
                                  where aModelInterpreter.GetCardinality(aProperty) == CCardinalityEnum.R11C
                                  select aProperty;
            var aR11CFkRows = (from aProperty in aR11CProperties
                               from aRow in aModelInterpreter.NewR11CForeignKeyRows(aProperty)
                               select aRow);
            var aR11CReverseRows = from aProperty in aR11CProperties
                                   from aRow in aModelInterpreter.NewR11CReverseNavigationRows(aProperty)
                                   select aRow;
            var aOrgRows = aModel.Rows;
            var aNewRows = aR1NCFkRows.Concat(aR1NCRevereRows).Concat(aR11CFkRows).Concat(aR11CReverseRows).Concat(aOrgRows);
            var aOutModel = new CRflModel(aModelInterpreter, aModel.FileInfo, aNewRows);
            return aOutModel;
        }
    }

}
