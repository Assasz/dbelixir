using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XMLParser.Services
{
    public class TypeValidator
    {
        public string Error { get; set; }

        public TypeValidator()
        {
            this.Error = "";
        }

        public bool Validate(string input, string type, string propertyName)
        {
            switch (type)
            {
                case "int":
                    int resultInt;
                    if (!Int32.TryParse(input, out resultInt))
                    {
                        this.Error = propertyName + " value needs to be int.";
                        return false;
                    }
                    break;
                case "long":
                    long resultLong;
                    if (!long.TryParse(input, out resultLong))
                    {
                        this.Error = propertyName + " value needs to be long.";
                        return false;
                    }
                    break;
                case "double":
                    double resultDouble;
                    if (!double.TryParse(input, out resultDouble))
                    {
                        this.Error = propertyName + " value needs to be double.";
                        return false;
                    }
                    break;
                case "bool":
                    bool resultBool;
                    if (!bool.TryParse(input, out resultBool))
                    {
                        this.Error = propertyName + " value needs to be bool.";
                        return false;
                    }
                    break;
                case "decimal":
                    decimal resultDecimal;
                    if (!decimal.TryParse(input, out resultDecimal))
                    {
                        this.Error = propertyName + " value needs to be decimal.";
                        return false;
                    }
                    break;
                case "datetime":
                    DateTime resultDatetime;
                    if (!DateTime.TryParse(input, out resultDatetime))
                    {
                        this.Error = propertyName + " value needs to be datetime.";
                        return false;
                    }
                    break;
            }

            return true;
        }
    }
}