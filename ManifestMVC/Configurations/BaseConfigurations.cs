using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ManifestMVC.Configurations
{
    public class StringCollection : ConfigurationElementCollection
    {
        public StringCollection()
        {
        }

        public override
            ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return

                    ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        protected override
            ConfigurationElement CreateNewElement()
        {
            return new StringConfigElement();
        }


        protected override
            ConfigurationElement CreateNewElement(
            string elementName)
        {
            return new StringConfigElement(elementName);
        }


        protected override Object
            GetElementKey(ConfigurationElement element)
        {
            return ((StringConfigElement)element).Name;
        }


        public new string AddElementName
        {
            get
            { return base.AddElementName; }

            set
            { base.AddElementName = value; }

        }

        public new string ClearElementName
        {
            get
            { return base.ClearElementName; }

            set
            { base.ClearElementName = value; }

        }

        public new string RemoveElementName
        {
            get
            { return base.RemoveElementName; }
        }

        public new int Count
        {
            get { return base.Count; }
        }


        public StringConfigElement this[int index]
        {
            get
            {
                return (StringConfigElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        new public StringConfigElement this[string Name]
        {
            get
            {
                return (StringConfigElement)BaseGet(Name);
            }
        }

        public int IndexOf(StringConfigElement sce)
        {
            return BaseIndexOf(sce);
        }

        public void Add(StringConfigElement sce)
        {
            BaseAdd(sce);
            // Add custom code here.
        }

        protected override void
            BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
            // Add custom code here.
        }

        public void Remove(StringConfigElement sce)
        {
            if (BaseIndexOf(sce) >= 0)
                BaseRemove(sce.Name);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        public void Clear()
        {
            BaseClear();
            // Add custom code here.
        }
    }


    public class StringConfigElement : ConfigurationElement
    {
        // Constructor allowing name, url, and port to be specified. 
        public StringConfigElement(String value)
        {
            Value = value;
        }

        // Default constructor, will use default values as defined 
        // below. 
        public StringConfigElement()
        {
        }

        // Constructor allowing name to be specified, will take the 
        // default values for url and port. 
        public StringConfigElement(string elementName)
        {
            Name = elementName;
        }

        [ConfigurationProperty("value",
            DefaultValue = "",
            IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("url",
            DefaultValue = "http://www.microsoft.com",
            IsRequired = true)]
        [RegexStringValidator(@"\w+:\/\/[\w.]+\S*")]
        public string Url
        {
            get
            {
                return (string)this["url"];
            }
            set
            {
                this["url"] = value;
            }
        }

        [ConfigurationProperty("port",
            DefaultValue = (int)0,
            IsRequired = false)]
        [IntegerValidator(MinValue = 0,
            MaxValue = 8080, ExcludeRange = false)]
        public int Port
        {
            get
            {
                return (int)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }

        protected override void DeserializeElement(
           System.Xml.XmlReader reader,
            bool serializeCollectionKey)
        {
            base.DeserializeElement(reader,
                serializeCollectionKey);
            // You can your custom processing code here.
        }


        protected override bool SerializeElement(
            System.Xml.XmlWriter writer,
            bool serializeCollectionKey)
        {
            bool ret = base.SerializeElement(writer,
                serializeCollectionKey);
            // You can enter your custom processing code here. 
            return ret;

        }


        protected override bool IsModified()
        {
            bool ret = base.IsModified();
            // You can enter your custom processing code here. 
            return ret;
        }
    }
}