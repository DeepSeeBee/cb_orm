// This is the runtime for my 2nd generation ORM-Wrapper. (Schema subsystem)
using CbOrm.Attributes;
using CbOrm.Blop;
using CbOrm.Converters;
using CbOrm.Entity;
using CbOrm.Meta;
using CbOrm.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbOrm.Schema
{
    public abstract class CSchema
    {
        protected CSchema()
        {
            this.AddTyp(CEntityObject._CEntityObject_Typ);
            this.AddTyp(CBlop._CBlop_Typ);
        }

        internal const string AddTyp_Name = nameof(AddTyp);

        private CTyps TypsM;
        internal CTyps Typs
        {
            get
            {
                if (this.TypsM.IsNullRef())
                    throw new InvalidOperationException();
                return this.TypsM;
            }
        }
        private List<CTyp> TypList = new List<CTyp>();
        protected void AddTyp(CTyp aTyp)
        {
            if (!this.TypsM.IsNullRef())
                throw new InvalidOperationException();
            this.TypList.Add(aTyp);
        }

        internal const string NameOf_Init = nameof(Init);
        protected void Init()
        {
            if (!this.TypsM.IsNullRef())
                throw new InvalidOperationException();
            var aTyps = new CTyps(this.TypList);
            aTyps.Init();
            this.TypsM = aTyps;
            this.PersistentProperties = this.CalcPersistentProperties(aTyps);
            this.TypList = null;
            CXmlConverters.Register(this.SaveXmlValueConverters.Register);
        }



        private Dictionary<CTyp, IEnumerable<CRefMetaInfo>> PersistentProperties;
        private CTyp CalcBaseTypNullable(CTyp aTyp)
        {
            var aSystemType = aTyp.SystemType;
            var aBaseSystemType = aSystemType.BaseType;
            var aSchema = this;
            var aBaseType = aSchema.Typs.ContainsKey(aBaseSystemType)
                          ? aSchema.Typs.GetBySystemType(aBaseSystemType)
                          : default(CTyp)
                          ;
            return aBaseType;
        }

        private IEnumerable<CTyp> CalcHirarchy(CTyp aTyp)
        {
            var aBaseTyp = this.CalcBaseTypNullable(aTyp);
            if (!aBaseTyp.IsNullRef())
                foreach (var aSubTyp in this.CalcHirarchy(aBaseTyp))
                    yield return aSubTyp;
            yield return aTyp;            
        }
        private IEnumerable<CRefMetaInfo> CalcPersistentProperties(CTyp aObjectTyp)
        {
            var aHirarchy = this.CalcHirarchy(aObjectTyp);
            var aProperties = from aApsect in aHirarchy
                                from aProperty in this.CalcPersistentProperties(aObjectTyp, aApsect)
                                select aProperty;
            return aProperties;
        }

        private IEnumerable<CRefMetaInfo> CalcPersistentProperties(CTyp aObjectTyp, CTyp aAspect)
        {
            var aProperties1 = aAspect.Properties;
            var aProperties2 = from aProperty in aProperties1
                               where this.CalcIsPersistent(aProperty, aObjectTyp)
                               select aProperty
                               ;
            return aProperties2;
        }
        private CTyp GetDeclaringTyp(CRefMetaInfo aProperty) => this.Typs.GetBySystemType(aProperty.PropertyInfo.DeclaringType);
        private bool CalcSpreadAcrossTables(CRefMetaInfo aProperty) => aProperty.IsDefined<CSpreadAcrossTablesAttribute>()
                                                                    ? aProperty.GetAttribute<CSpreadAcrossTablesAttribute>().Value
                                                                    : false
                                                                    ;

        private bool CalcIsPersistent(CRefMetaInfo aProperty, CTyp aObjectTyp)
        {
            var aIsPersistent = this.GetDeclaringTyp(aProperty) == aObjectTyp
                             || this.CalcSpreadAcrossTables(aProperty)
                            ;
            return aIsPersistent;
        }
        private Dictionary<CTyp, IEnumerable<CRefMetaInfo>> CalcPersistentProperties(IEnumerable<CTyp> aTyps)
        {
            var aDic = new Dictionary<CTyp, IEnumerable<CRefMetaInfo>>(aTyps.Count());
            foreach(var aTyp in aTyps)
            {
                aDic.Add(aTyp, this.CalcPersistentProperties(aTyp).ToArray());
            }
            return aDic;
        }
        internal IEnumerable<CRefMetaInfo> GetPersistentProperties(CTyp aTyp)=>this.PersistentProperties[aTyp];
        internal IEnumerable<CTyp> GetHirarchy(CTyp aTyp) => this.CalcHirarchy(aTyp); //// TODO-Opt

        private readonly CConverters SaveXmlValueConverters = new CConverters();
        internal CConverter GetSaveXmlConverter(Type aValueType)
        {
            return this.SaveXmlValueConverters[aValueType];
        }

    }
}
