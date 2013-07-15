using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;
using DotNetNuke.Common;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN.controls;

namespace NBrightDNN
{
    public abstract class DataCtrlInterface
    {
        public abstract List<NBrightInfo> GetList(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string sqlOrderBy = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0, string typeCodeLang = "", string lang = "");
        public abstract int GetListCount(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string typeCodeLang = "", string lang = "");
        public abstract NBrightInfo Get(int itemId, string typeCodeLang = "", string lang = "");
        public abstract int Update(NBrightInfo objInfo);
        public abstract void Delete(int itemId);
        public abstract void CleanData();

    }

    public class NBrightInfo
    {
        public int ItemID { get; set; }
        public int PortalId { get; set; }
        public int ModuleId { get; set; }
        public string TypeCode { get; set; }
        public string GUIDKey { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string TextData { get; set; }
        public int XrefItemId { get; set; }
        public int ParentItemId { get; set; }
        public XmlDataDocument XMLDoc { get; set; }
        public string Lang { get; set; }
        public int UserId { get; set; }
        public int RowCount { get; set; }

        private string _xmlData;

        public string XMLData
        {
            get { return _xmlData; } 
            set 
            {
                XMLDoc = null;
                _xmlData = value;
                try
                {
                    if (!String.IsNullOrEmpty(_xmlData))
                    {
                        XMLDoc = new XmlDataDocument();
                        XMLDoc.LoadXml(_xmlData);                        
                    }
                }
                catch (Exception)
                {
                    //trap erorr and don't report. (The XML might be invalid, but we don;t want to stop processing here.)
                    XMLDoc = null;
                }
            } 
        }

        public string GetXmlNode(string xpath)
        {
            if (!string.IsNullOrEmpty(_xmlData) & XMLDoc != null)
            {
                try
                {
                    var selectSingleNode = XMLDoc.SelectSingleNode(xpath);
                    if (selectSingleNode != null) return selectSingleNode.InnerXml;
                }
                catch (Exception ex)
                {
                    return "XML READ ERROR";
                }
            }
            return "";
        }

        public void RemoveXmlNode(string xPath)
        {
            var xmlNod = XMLDoc.SelectSingleNode(xPath);
            if (xmlNod != null)
            {
                if (xmlNod.ParentNode != null) xmlNod.ParentNode.RemoveChild(xmlNod);
            }
            XMLData = XMLDoc.OuterXml;
        }

        public void AddXmlNode(string strXml, string xPathSource, string xPathRootDestination)
        {
            var xmlDocNew = new XmlDataDocument();
            xmlDocNew.LoadXml(strXml);

            var xmlTarget = XMLDoc.SelectSingleNode(xPathRootDestination);
            if (xmlTarget != null)
            {
                var xmlNod2 = xmlDocNew.SelectSingleNode(xPathSource);
                if (xmlNod2 != null)
                {
                    var newNod = XMLDoc.ImportNode(xmlNod2, true);
                    xmlTarget.AppendChild(newNod);
                    XMLData = XMLDoc.OuterXml;
                }
            }
        }

        public void ReplaceXmlNode(string strXml, string xPathSource, string xPathRootDestination, bool addNode = true)
        {
            var xmlDocNew = new XmlDataDocument();
            xmlDocNew.LoadXml(strXml);

            var xmlNod = XMLDoc.SelectSingleNode(xPathSource);
            if (xmlNod != null)
            {
                var xmlNod2 = xmlDocNew.SelectSingleNode(xPathSource);
                if (xmlNod2 != null)
                {
                    var newNod = XMLDoc.ImportNode(xmlNod2, true);
                    var selectSingleNode = XMLDoc.SelectSingleNode(xPathRootDestination);
                    if (selectSingleNode != null)
                    {
                        selectSingleNode.ReplaceChild(newNod, xmlNod);
                        XMLData = XMLDoc.OuterXml;
                    }
                }
            }
            else
            {
                AddXmlNode(strXml,xPathSource,xPathRootDestination);
            }
        }

        public string GetXmlProperty(string xpath)
        {
            var xmlDoc = new XmlDataDocument();
            if (!string.IsNullOrEmpty(XMLData))
            {
                try
                {
                    return GenXmlFunctions.GetGenXmlValue(XMLData, xpath);
                }
                catch (Exception ex)
                {
                    return "XML READ ERROR";
                }
            }
            return "";
        }


        public void AppendToXmlProperty(string xpath, string Value, System.TypeCode DataTyp = System.TypeCode.String, bool cdata = true)
        {
            if (!string.IsNullOrEmpty(XMLData))
            {
                var strData = GenXmlFunctions.GetGenXmlValue(XMLData, xpath) + Value;
                XMLData = GenXmlFunctions.SetGenXmLvalue(XMLData, xpath, strData, cdata);
            }
        }

        public void SetXmlProperty(string xpath, string Value, System.TypeCode DataTyp = System.TypeCode.String, bool cdata = true)
        {
            if (!string.IsNullOrEmpty(XMLData))
            {
                XMLData = GenXmlFunctions.SetGenXmLvalue(XMLData, xpath, Value, cdata);
            }
        }

        public string ToXmlItem(bool withTextData = true)
        {
            // don't use serlization, becuase depending what is in the TextData field could make it fail.
            var xmlOut = "<item>";
            xmlOut += "<itemid>";
            xmlOut += ItemID.ToString("");
            xmlOut += "</itemid>";
            xmlOut += "<portalid>";
            xmlOut += PortalId.ToString("");
            xmlOut += "</portalid>";
            xmlOut += "<moduleid>";
            xmlOut += ModuleId.ToString("");
            xmlOut += "</moduleid>";
            xmlOut += "<xrefitemid>";
            xmlOut += XrefItemId.ToString("");
            xmlOut += "</xrefitemid>";
            xmlOut += "<parentitemid>";
            xmlOut += ParentItemId.ToString("");
            xmlOut += "</parentitemid>";
            xmlOut += "<typecode>";
            xmlOut += TypeCode;
            xmlOut += "</typecode>";
            xmlOut += "<guidkey>";
            xmlOut += GUIDKey;
            xmlOut += "</guidkey>";
            xmlOut += "<lang>";
            xmlOut += Lang;
            xmlOut += "</lang>";
            xmlOut += "<userid>";
            xmlOut += UserId.ToString("");
            xmlOut += "</userid>";
            xmlOut += XMLData;
            if (withTextData)
            {
                xmlOut += "<data><![CDATA[";
                xmlOut += TextData.Replace("<![CDATA[", "***CDATASTART***").Replace("]]>", "***CDATAEND***");
                xmlOut += "]]></data>";                
            }
            xmlOut += "</item>";

            return xmlOut;
        }

        public void FromXmlItem(string xmlItem)
        {
            var xmlDoc = new XmlDataDocument();
            xmlDoc.LoadXml(xmlItem);

            //itemid
            var selectSingleNode = xmlDoc.SelectSingleNode("item/itemid");
            if (selectSingleNode != null) ItemID = Convert.ToInt32(selectSingleNode.InnerText);

            //portalid
            selectSingleNode = xmlDoc.SelectSingleNode("item/portalid");
            if (selectSingleNode != null) PortalId = Convert.ToInt32(selectSingleNode.InnerText);

            // moduleid
            selectSingleNode = xmlDoc.SelectSingleNode("item/moduleid");
            if (selectSingleNode != null) ModuleId = Convert.ToInt32(selectSingleNode.InnerText);

            //xrefitemid
            selectSingleNode = xmlDoc.SelectSingleNode("item/xrefitemid");
            if (selectSingleNode != null) XrefItemId = Convert.ToInt32(selectSingleNode.InnerText);

            //parentitemid
            selectSingleNode = xmlDoc.SelectSingleNode("item/parentitemid");
            if (selectSingleNode != null) ParentItemId = Convert.ToInt32(selectSingleNode.InnerText);

            //typecode
            selectSingleNode = xmlDoc.SelectSingleNode("item/typecode");
            if (selectSingleNode != null) TypeCode = selectSingleNode.InnerText;

            //guidkey
            selectSingleNode = xmlDoc.SelectSingleNode("item/guidkey");
            if (selectSingleNode != null) GUIDKey = selectSingleNode.InnerText;

            //XmlData
            selectSingleNode = xmlDoc.SelectSingleNode("item/genxml");
            if (selectSingleNode != null) XMLData = selectSingleNode.OuterXml;

            //TextData
            selectSingleNode = xmlDoc.SelectSingleNode("item/data");
            if (selectSingleNode != null) TextData = selectSingleNode.InnerText.Replace("***CDATASTART***", "<![CDATA[").Replace("***CDATAEND***", "]]>");

            //lang
            selectSingleNode = xmlDoc.SelectSingleNode("item/lang");
            if (selectSingleNode != null) Lang = selectSingleNode.InnerText;

            //userid
            selectSingleNode = xmlDoc.SelectSingleNode("item/userid");
            if ((selectSingleNode != null) && (Utils.IsNumeric(selectSingleNode.InnerText))) UserId = Convert.ToInt32(selectSingleNode.InnerText);

        }



        #region "Xref"

        public void AddXref(string nodeName, string value)
        {
            //create node if not there.
            if (XMLDoc.SelectSingleNode("genxml/" + nodeName) == null)
                AddXmlNode("<genxml><" + nodeName + "></" + nodeName + "></genxml>", "genxml/" + nodeName, "genxml");
            //Add new xref node, if not there.
            if (XMLDoc.SelectSingleNode("genxml/" + nodeName + "/id[.='" + value + "']") == null)
                AddXmlNode("<genxml><" + nodeName + "><id>" + value + "</id></" + nodeName + "></genxml>", "genxml/" + nodeName + "/id", "genxml/" + nodeName);
        }

        public void RemoveXref(string nodeName, string value)
        {
            //Removexref node, if there.
            if (XMLDoc.SelectSingleNode("genxml/" + nodeName + "/id[.='" + value + "']") != null)
                RemoveXmlNode("genxml/" + nodeName + "/id[.='" + value + "']");
        }


        public List<String> GetXrefList(string nodeName)
        {
            var strList = new List<String>();
            var nodList = XMLDoc.SelectNodes("genxml/" + nodeName + "/id");
            foreach (XmlNode nod in nodList)
            {
                strList.Add(nod.InnerText);
            }
            return strList;
        }


        #endregion

    }


}
