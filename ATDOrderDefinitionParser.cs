using System;
using System.Collections.Generic;
using System.Xml;

namespace ATDOrderSystem
{
    public class ATDOrderDefinitionParser
    {
        public string Title { get; private set; }
        public string Version { get; private set; }
        public string Description { get; private set; }
        public List<ATDFieldDefinition> Fields { get; private set; }

        public ATDOrderDefinitionParser()
        {
            Fields = new List<ATDFieldDefinition>();
        }

        public void LoadFromFile(string filePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            XmlNode metadataNode = doc.SelectSingleNode("/ATDOrderDefinition/Metadata");
            if (metadataNode != null)
            {
                Title = GetNodeValue(metadataNode, "Title");
                Version = GetNodeValue(metadataNode, "Version");
                Description = GetNodeValue(metadataNode, "Description");
            }

            XmlNodeList fieldNodes = doc.SelectNodes("/ATDOrderDefinition/Fields/Field");
            foreach (XmlNode fieldNode in fieldNodes)
            {
                ATDFieldDefinition field = new ATDFieldDefinition
                {
                    Name = GetNodeValue(fieldNode, "Name"),
                    Label = GetNodeValue(fieldNode, "Label"),
                    Type = GetNodeValue(fieldNode, "Type"),
                    Required = GetNodeValueAsBool(fieldNode, "Required"),
                    MinLength = GetNodeValueAsNullableInt(fieldNode, "MinLength"),
                    MaxLength = GetNodeValueAsNullableInt(fieldNode, "MaxLength"),
                    ValidationPattern = GetNodeValue(fieldNode, "ValidationPattern"),
                    ValidationMessage = GetNodeValue(fieldNode, "ValidationMessage"),
                    DefaultValue = GetNodeValue(fieldNode, "DefaultValue"),
                    MinValue = GetNodeValueAsNullableDecimal(fieldNode, "MinValue"),
                    MaxValue = GetNodeValueAsNullableDecimal(fieldNode, "MaxValue"),
                    DecimalPlaces = GetNodeValueAsNullableInt(fieldNode, "DecimalPlaces")
                };

                XmlNodeList optionNodes = fieldNode.SelectNodes("Options/Option");
                if (optionNodes != null)
                {
                    foreach (XmlNode optionNode in optionNodes)
                    {
                        field.Options.Add(optionNode.InnerText);
                    }
                }

                Fields.Add(field);
            }
        }

        private string GetNodeValue(XmlNode parentNode, string nodeName)
        {
            XmlNode node = parentNode.SelectSingleNode(nodeName);
            return node?.InnerText ?? string.Empty;
        }

        private bool GetNodeValueAsBool(XmlNode parentNode, string nodeName)
        {
            string value = GetNodeValue(parentNode, nodeName);
            bool result;
            bool.TryParse(value, out result);
            return result;
        }

        private int? GetNodeValueAsNullableInt(XmlNode parentNode, string nodeName)
        {
            string value = GetNodeValue(parentNode, nodeName);
            if (string.IsNullOrEmpty(value))
                return null;
            
            int result;
            if (int.TryParse(value, out result))
                return result;
            
            return null;
        }

        private decimal? GetNodeValueAsNullableDecimal(XmlNode parentNode, string nodeName)
        {
            string value = GetNodeValue(parentNode, nodeName);
            if (string.IsNullOrEmpty(value))
                return null;
            
            decimal result;
            if (decimal.TryParse(value, out result))
                return result;
            
            return null;
        }
    }
}
