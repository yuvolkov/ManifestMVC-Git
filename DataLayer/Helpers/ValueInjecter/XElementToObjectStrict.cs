using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.ComponentModel;

using Omu.ValueInjecter;
using System.Reflection;
using System.Diagnostics;


namespace DataLayer.Helpers.ValueInjecter
{
    public class XElementToObjectStrict : KnownSourceValueInjection<XElement>
    {
        protected override void Inject(XElement source, object target)
        {
            PropertyDescriptorCollection targetProps = target.GetProps();

            foreach (PropertyDescriptor t in targetProps)
            {
                XElement elem = null;

                elem = source.Descendants(t.Name).SingleOrDefault();

                if (elem != null && !elem.HasElements)
                {
                    object value = null;

                    if (t.PropertyType == typeof(string))
                        value = elem.Value;
                    else if (t.PropertyType == typeof(bool))
                        value = bool.Parse(elem.Value);
                    else if (t.PropertyType == typeof(int))
                        value = int.Parse(elem.Value, System.Globalization.NumberStyles.Integer);
                    else if (t.PropertyType == typeof(float))
                        value = float.Parse(elem.Value, System.Globalization.CultureInfo.InvariantCulture);
                    else
                        throw new NotImplementedException(
                            String.Format("Type {0} for {1} is not implemented!", t.PropertyType, t.Name));

                    // instead: t.SetValue(target, value); - this sets only public values, not internal or protected
                    // and this one sets all values:
                    target.GetType().GetProperty(t.Name)
                        .SetValue(target, value, null);
                }
                else if (elem.HasElements)
                {
                    // should be a complex type
                    Inject(elem, t.GetValue(target));
                }
                else
                    throw new InvalidOperationException(
                        String.Format("Property {1} not found in data source!", t.Name));
            }
        }
    }
}
