using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Omu.ValueInjecter;


namespace DataLayer.Helpers.ValueInjecter
{
    //public class EnumToInt : ConventionInjection
    //{
    //    protected override bool Match(ConventionInfo c)
    //    {
    //        return c.SourceProp.Name == c.TargetProp.Name &&
    //            c.SourceProp.Type.IsSubclassOf(typeof(Enum)) && c.TargetProp.Type == typeof(int);
    //    }
    //}

    //public class IntToEnum : ConventionInjection
    //{
    //    protected override bool Match(ConventionInfo c)
    //    {
    //        return c.SourceProp.Name == c.TargetProp.Name &&
    //            c.SourceProp.Type == typeof(int) && c.TargetProp.Type.IsSubclassOf(typeof(Enum));
    //    }
    //}

    ////e.g. int? -> int
    //public class NullablesToNormal : ConventionInjection
    //{
    //    protected override bool Match(ConventionInfo c)
    //    {
    //        return c.SourceProp.Name == c.TargetProp.Name &&
    //               Nullable.GetUnderlyingType(c.SourceProp.Type) == c.TargetProp.Type;
    //    }
    //}

    ////e.g. int -> int?
    //public class NormalToNullables : ConventionInjection
    //{
    //    protected override bool Match(ConventionInfo c)
    //    {
    //        return c.SourceProp.Name == c.TargetProp.Name &&
    //               c.SourceProp.Type == Nullable.GetUnderlyingType(c.TargetProp.Type);
    //    }
    //}

    ////e.g. int -> int?
    //public class WithNullables : DoNotReadTargetValuesConventionInjection
    //{
    //    protected override bool Match(ConventionInfo c)
    //    {
    //        return
    //            c.SourceProp.Name == c.TargetProp.Name &&
    //            (c.SourceProp.Type == c.TargetProp.Type ||
    //             c.SourceProp.Type == Nullable.GetUnderlyingType(c.TargetProp.Type));
    //    }
    //}

}
