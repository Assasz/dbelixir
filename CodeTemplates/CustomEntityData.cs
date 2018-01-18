using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XMLParser.CodeTemplates
{
    public partial class CustomEntity
    {
        public string entityName { get; set; }
        public Dictionary<string, Dictionary<string, string>> propertyMap { get; set; }

        public CustomEntity(string entityName, Dictionary<string, Dictionary<string, string>> propertyMap)
        {
            this.entityName = entityName;
            this.propertyMap = propertyMap;
        }
    }
}