using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

//-----------------------------------------------------------------------------------------------------------------------------------------------------------------
namespace MGL.Data.DataUtilities {

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------
    public class MglWebConfigurationInfo : ConfigurationSection {

        //------------------------------------------------------------------------------------------------------------------------------------------------------------
        [ConfigurationProperty("mglConfigParams", IsRequired=true)]
        public MglWebConfigurationInfoParamsCollection ConfigInfoList {
            get {
                return this["mglConfigParams"] as MglWebConfigurationInfoParamsCollection;
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static MglWebConfigurationInfo GetConfig() {
            return ConfigurationManager.GetSection("mglConfigurationInfo") as MglWebConfigurationInfo;
        }

    }

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------
    public class MglWebConfigurationInfoParam : ConfigurationElement {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name {
            get {
                return this["name"] as string;
            }
        }

        [ConfigurationProperty("value", IsRequired = true)]
        public string Value {
            get {
                return this["value"] as string;
            }
        }
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------
    public class MglWebConfigurationInfoParamsCollection : ConfigurationElementCollection {
        public MglWebConfigurationInfoParam this[int index] {
            get {
                return base.BaseGet(index) as MglWebConfigurationInfoParam;
            }
            set {
                if (base.BaseGet(index) != null) {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement() {
            return new MglWebConfigurationInfoParam();
        }

        protected override object GetElementKey(ConfigurationElement element) {
            return ((MglWebConfigurationInfoParam)element).Name;
        }
    }
}
