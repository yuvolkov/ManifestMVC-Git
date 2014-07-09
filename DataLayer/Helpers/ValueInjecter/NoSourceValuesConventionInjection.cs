using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Omu.ValueInjecter;


namespace DataLayer.Helpers.ValueInjecter
{
    public class DoNotReadTargetValuesConventionInjection : ValueInjection
    {
        //protected abstract bool Match(ConventionInfo c);
        protected virtual bool Match(ConventionInfo c)
        {
            return
                c.SourceProp.Name == c.TargetProp.Name &&
                c.SourceProp.Type == c.TargetProp.Type;
                // ( || c.SourceProp.Type == Nullable.GetUnderlyingType(c.TargetProp.Type));
        }

        protected virtual object SetValue(ConventionInfo c)
        {
            return c.SourceProp.Value;
        }

        protected override void Inject(object source, object target)
        {
            var sourceProps = source.GetProps();
            var targetProps = target.GetProps();

            var ci = new ConventionInfo
            {
                Source =
                {
                    Type = source.GetType(),
                    Value = source
                },
                Target =
                {
                    Type = target.GetType(),
                    Value = target
                }
            };

            for (var i = 0; i < sourceProps.Count; i++)
            {
                var s = sourceProps[i];
                ci.SourceProp.Name = s.Name;
                ci.SourceProp.Value = s.GetValue(source);
                ci.SourceProp.Type = s.PropertyType;

                for (var j = 0; j < targetProps.Count; j++)
                {
                    var t = targetProps[j];
                    ci.TargetProp.Name = t.Name;
                    //ci.TargetProp.Value = t.GetValue(target);
                    ci.TargetProp.Type = t.PropertyType;
                    if (Match(ci))
                        t.SetValue(target, SetValue(ci));
                }
            }
        }
    }
}
