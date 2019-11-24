// This is the runtime for my 2nd generation ORM-Wrapper. (Schema subsystem)
using CbOrm.Attributes;
using CbOrm.Blop;
using CbOrm.Converters;
using CbOrm.Di;
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

            {
                var aProperties0 = aTyp.Properties.OfType<CSkalarRefMetaInfo>();
                var aProperties1 = from aTest in aProperties0
                                   where aTest.IsDefined<CTargetTypeAttribute>()
                                   select new Tuple<CSkalarRefMetaInfo, CTargetTypeAttribute>(aTest, aTest.GetAttribute<CTargetTypeAttribute>())
                                   ;
                var aProperties2 = from aTest in aProperties1
                                   where aTest.Item2.TargetType.IsDefined(typeof(CSaveConverterAttribute), true)
                                   select new Tuple<CSkalarRefMetaInfo, CTargetTypeAttribute, CSaveConverterAttribute>
                                   (aTest.Item1, aTest.Item2, aTest.Item2.TargetType.GetCustomAttribute<CSaveConverterAttribute>(true))
                                   ;
                var aProperties = aProperties2;
                foreach(var aProperty in aProperties)
                {
                    var aTargetTypeAttribute = aProperty.Item2;
                    var aSaveConverterAtttribute = aProperty.Item3;
                    var aSaveConverter = (CConverter)Activator.CreateInstance(aSaveConverterAtttribute.ConverterType);
                    this.ModelConverterChain.Ly1WrapConverters.Register(aTargetTypeAttribute.TargetType, aSaveConverter);
                }
            }

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
            CXmlConverters.Register(this.ModelConverterChain.Ly0XmlConverters.Register);
        }



        private Dictionary<CTyp, IEnumerable<CRefMetaInfo>> PersistentProperties;
        internal const string RegisterEnumType_Name = "RegisterEnumType";

        protected void RegisterEnumType(Type aEnumType)
        {
            this.ModelConverterChain.Ly0XmlConverters.Register(aEnumType, new CSaveXmlEnumConverter(aEnumType));
        }

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

        internal readonly CModelConverterChain  ModelConverterChain = new CModelConverterChain();
        //internal CConverter GetSaveXmlConverter(Type aValueType)
        //{
        //    return this.SaveXmlValueConverters.GetConverter(aValueType);
        //}

        public readonly CDiContainer DefaultCalculators = new CDiContainer();

        internal const string NameOf_RegisterDefaultCalculator = "RegisterDefaultCalculator";
        protected void RegisterDefaultCalculator(Type aType, Func<object> aCreateDefault)
        {
            var aDefaultCalculatorType = typeof(CDefaultCalculator<object>).GetGenericTypeDefinition().MakeGenericType(aType);
            var aDefaultCaluclator = (CDefaultCalculator)Activator.CreateInstance(aDefaultCalculatorType, aCreateDefault);
            aDefaultCaluclator.Register(this.DefaultCalculators);
        }

        internal Func<T> GetDefaultCalculator<T>()
        {
            var aCalculator = this.DefaultCalculators.GetNullable<CDefaultCalculator<T>>();
            var aDefault = aCalculator.IsNullRef()
                         ? new Func<T>(() => default(T))
                         : new Func<T>(()=> aCalculator.CalcDefaultFunc())
                         ;
            return aDefault;
        }

        internal abstract class CDefaultCalculator
        {
            internal abstract void Register(CDiContainer aDiContainer);
        }
        internal sealed class CDefaultCalculator<T> : CDefaultCalculator
        {
            public CDefaultCalculator(Func<object> aCalcDefaultFunc)
            {
                this.CalcDefaultFunc = new Func<T>(() => (T)aCalcDefaultFunc());
            }
            internal readonly Func<T> CalcDefaultFunc;
            internal override void Register(CDiContainer aDiContainer)
            {
                aDiContainer.Add < CDefaultCalculator<T>>(() => this);
            }

        }

    }


}
