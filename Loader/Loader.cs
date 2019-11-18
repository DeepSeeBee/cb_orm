using CbOrm.Xdl;
using System;
using System.Collections.Generic;
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

    public sealed class CInlcudeModelExpander : CModelExpander
    {
        public override CRflModel Expand(CRflModel Model)
        {
            throw new NotImplementedException();
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
            return new CRflModel(aModel.ModelInterpreter, aModel.Rows.Concat(this.Rows));
        }

    }
}
