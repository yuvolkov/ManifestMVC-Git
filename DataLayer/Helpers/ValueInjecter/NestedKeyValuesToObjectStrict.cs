using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.ComponentModel;

using Omu.ValueInjecter;


namespace DataLayer.Helpers.ValueInjecter
{
    public class NestedKeyValuesToObjectStrict : KnownSourceValueInjection<IEnumerable<KeyValuePair<string, string>>>
    {
        protected override void Inject(IEnumerable<KeyValuePair<string, string>> source, object target)
        {
            PropertyDescriptorCollection targetProps = target.GetProps();

            foreach (PropertyDescriptor t in targetProps)
            {
                KeyValuePair<string, string> elem = source.SingleOrDefault(kvp => kvp.Key == t.Name);
                IEnumerable<KeyValuePair<string, string>> subelems = source.Where(kvp => kvp.Key.StartsWith(t.Name + "."));

                if (elem.Key != null)    // Key is null if not exists
                {
                    object value = null;

                    if (t.PropertyType == typeof(string))
                        value = elem.Value;
                    else if (t.PropertyType == typeof(bool))
                        value = bool.Parse(elem.Value);
                    else if (t.PropertyType == typeof(int))
                        value = int.Parse(elem.Value, System.Globalization.NumberStyles.Integer);
                    else if (t.PropertyType == typeof(double))
                        value = double.Parse(elem.Value, System.Globalization.CultureInfo.InvariantCulture);
                    else
                        throw new NotImplementedException(
                            String.Format("Type {0} for {1} is not implemented!", t.PropertyType, t.Name));

                    // instead: t.SetValue(target, value); - this sets only public values, not internal or protected
                    // and this one sets all values:
                    target.GetType().GetProperty(t.Name)
                        .SetValue(target, value, null);
                }
                else if (subelems.Any())
                {
                    // should be a complex type

                    // removing prefix
                    subelems =
                        subelems.Select(kvp => new KeyValuePair<string,string>(
                            kvp.Key.Substring(t.Name.Length+1),
                            kvp.Value));

                    Inject(subelems, t.GetValue(target));
                }
                else
                    throw new InvalidOperationException(
                        String.Format("Property {1} not found in data source!", t.Name));
            }
        }
    }
}
