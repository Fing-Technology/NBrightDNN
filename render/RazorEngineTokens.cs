﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;
using NBrightCore.common;
using NBrightCore.providers;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace NBrightDNN.render
{
    public class RazorEngineTokens<T> : TemplateBase<T>
    {
        private Dictionary<String,List<String>> _metadata;

        public RazorEngineTokens()
        {
            _metadata = new Dictionary<String, List<String>>();
        }

        #region "token to add meta data for tokens"

        public IEncodedString AddMetaData(String metaType, String metaValue)
        {
            var l = new List<String>();
            if (_metadata.ContainsKey(metaType)) l = _metadata[metaType];                
            l.Add(metaValue);

            if (_metadata.ContainsKey(metaType))
                _metadata[metaType] = l;
            else
                _metadata.Add(metaType,l);

            return new RawString(""); //return nothing
        }

        public IEncodedString AddPreProcessMetaData(String metaKey, String metaValue,String templateFullName)
        {
            var cachedlist = (Dictionary<String, String>)Utils.GetCache("preprocessmetadata" + templateFullName);
            if (cachedlist == null)
            {
                cachedlist = new Dictionary<String, String>();
            }

            if (cachedlist.ContainsKey(metaKey))
                cachedlist[metaKey] = metaValue;
            else
                cachedlist.Add(metaKey, metaValue);

            Utils.SetCache("preprocessmetadata" + templateFullName, cachedlist);

            return new RawString(""); //return nothing
        }

        #endregion

        #region "general html control tokens"

        public IEncodedString @HiddenField(NBrightInfo info, String xpath, String attributes = "", String defaultValue = "")
        {
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();
            if (defaultValue.StartsWith("ResourceKey:")) defaultValue = ResourceKey(defaultValue.Replace("ResourceKey:", "")).ToString();

            var upd = getUpdateAttr(xpath, attributes);
            var id = xpath.Split('/').Last();
            var value = info.GetXmlProperty(xpath);
            if (value == "") value = defaultValue;
            var strOut = "<input value='" + value + "' id='" + id + "' " + attributes + " " + upd + " type='hidden' />";

            return new RawString(strOut);
        }

        public IEncodedString TextBox(NBrightInfo info, String xpath, String attributes = "", String defaultValue = "")
        {
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();
            if (defaultValue.StartsWith("ResourceKey:")) defaultValue = ResourceKey(defaultValue.Replace("ResourceKey:", "")).ToString();

            var upd = getUpdateAttr(xpath, attributes);
            var id = xpath.Split('/').Last();
            var value = info.GetXmlProperty(xpath);
            if (value == "") value = defaultValue;
            var strOut = "<input value='" + value + "' id='" + id + "' " + attributes + " " + upd + " type='text' />";

            return new RawString(strOut);
        }

        public IEncodedString TextArea(NBrightInfo info, String xpath, String attributes = "", String defaultValue = "")
        {
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();
            if (defaultValue.StartsWith("ResourceKey:")) defaultValue = ResourceKey(defaultValue.Replace("ResourceKey:", "")).ToString();

            var upd = getUpdateAttr(xpath, attributes);
            var id = xpath.Split('/').Last();
            var value = info.GetXmlProperty(xpath);
            if (value == "") value = defaultValue;
            var strOut = "<textarea id='" + id + "' " + attributes + " " + upd + " type='text'>" + value + "</textarea>";

            return new RawString(strOut);
        }

        public IEncodedString RichTextBox(NBrightInfo info, String xpath, String attributes = "")
        {
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var upd = getUpdateAttr(xpath, attributes);
            var id = xpath.Split('/').Last();
            var strOut = " <textarea id='" + id + "' type='text' name='editor" + id + "' " + attributes + " " + upd + " >" + info.GetXmlProperty(xpath) + "</textarea>";
            strOut += "<script> var editorvar" + id + " = CKEDITOR.replace('editor" + id + "'); $('#savedata').click(function () { var value = editorvar" + id + ".getData(); $('#" + id + "').val(value);});  $('.selecteditlanguage').click(function () { var value = editorvar" + id + ".getData(); $('#" + id + "').val(value);});</script>";
            return new RawString(strOut);
        }

        public IEncodedString CheckBox(NBrightInfo info, String xpath,String text, String attributes = "", Boolean defaultValue = false)
        {
            if (text.StartsWith("ResourceKey:")) text = ResourceKey(text.Replace("ResourceKey:", "")).ToString();
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var upd = getUpdateAttr(xpath, attributes);
            var id = xpath.Split('/').Last();
            var strOut = "    <input id='" + id + "' type='checkbox' " + getChecked(info, xpath, defaultValue) + "' " + attributes + " " + upd + " /><label>" + text + "</label>";
            return new RawString(strOut);
        }

        public IEncodedString CheckBoxList(NBrightInfo info, String xpath, String datavalue, String datatext, String attributes = "", Boolean defaultValue = false)
        {
            if (datavalue.StartsWith("ResourceKey:")) datavalue = ResourceKey(datavalue.Replace("ResourceKey:", "")).ToString();
            if (datatext.StartsWith("ResourceKey:")) datatext = ResourceKey(datatext.Replace("ResourceKey:", "")).ToString();
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var strOut = "";
            var datav = datavalue.Split(',');
            var datat = datatext.Split(',');
            if (datav.Count() == datat.Count())
            {
                var upd = getUpdateAttr(xpath, attributes);
                var id = xpath.Split('/').Last();
                strOut = "<div id='" + id + "' " + upd + " " + attributes + ">";
                var c = 0;
                foreach (var v in datav)
                {
                    strOut += "    <input id='" + id + "_" + c.ToString("") + "' name='" + id + "$" + c.ToString("") + "' type='checkbox' value='" + v + "' " + getChecked(info, xpath + "/chk[@data='" + v + "']/@value", defaultValue) + "' /><label for='" + id + "_" + c.ToString("") + "'>" + datat[c] + "</label>";
                    c += 1;
                }
                strOut += "</div>";
            }
            return new RawString(strOut);
        }

        public IEncodedString RadioButtonList(NBrightInfo info, String xpath, String datavalue, String datatext, String attributes = "", String defaultValue = "")
        {
            if (datavalue.StartsWith("ResourceKey:")) datavalue = ResourceKey(datavalue.Replace("ResourceKey:", "")).ToString();
            if (datatext.StartsWith("ResourceKey:")) datatext = ResourceKey(datatext.Replace("ResourceKey:", "")).ToString();
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();
            if (defaultValue.StartsWith("ResourceKey:")) defaultValue = ResourceKey(defaultValue.Replace("ResourceKey:", "")).ToString();

            var strOut = "";
            var datav = datavalue.Split(',');
            var datat = datatext.Split(',');
            if (datav.Count() == datat.Count())
            {
                var upd = getUpdateAttr(xpath, attributes);
                var id = xpath.Split('/').Last();
                strOut = "<div " + attributes + ">";
                var c = 0;
                var s = "";
                var value = info.GetXmlProperty(xpath);
                if (value == "") value = defaultValue; 
                foreach (var v in datav)
                {
                    if (value == v)
                        s = "checked";
                    else
                        s = "";
                    strOut += "    <input id='" + id + "_" + c.ToString("") + "' " + upd + " name='" + id + "radio' type='radio' value='" + v + "'  " + s + "/><label>" + datat[c] + "</label>";
                    c += 1;
                }
                strOut += "</div>";
            }
            return new RawString(strOut);
        }

        public IEncodedString DropDownList(NBrightInfo info, String xpath, String datavalue, String datatext, String attributes = "", String defaultValue = "")
        {
            if (datavalue.StartsWith("ResourceKey:")) datavalue = ResourceKey(datavalue.Replace("ResourceKey:", "")).ToString();
            if (datatext.StartsWith("ResourceKey:")) datatext = ResourceKey(datatext.Replace("ResourceKey:", "")).ToString();
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();
            if (defaultValue.StartsWith("ResourceKey:")) defaultValue = ResourceKey(defaultValue.Replace("ResourceKey:", "")).ToString();

            var strOut = "";
            var datav = datavalue.Split(',');
            var datat = datatext.Split(',');
            if (datav.Count() == datat.Count())
            {
                var upd = getUpdateAttr(xpath,attributes);
                var id = xpath.Split('/').Last();
                strOut = "<select id='" + id + "' " + upd + " " + attributes + ">";
                var c = 0;
                var s = "";
                var value = info.GetXmlProperty(xpath);
                if (value == "") value = defaultValue;
                foreach (var v in datav)
                {
                    if (value == v)
                        s = "selected";
                    else
                        s = "";

                    strOut += "    <option value='" + v + "' " + s + ">" + datat[c] + "</option>";
                    c += 1;
                }
                strOut += "</select>";
            }
            return new RawString(strOut);
        }

        #endregion

        #region DNN specific tokens

        public IEncodedString TabSelectList(NBrightInfo info, String xpath, String attributes = "",Boolean allowEmpty = true)
        {
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var tList = DnnUtils.GetTreeTabListOnUniqueId();
            var strOut = "";

            var upd = getUpdateAttr(xpath, attributes);
            var id = xpath.Split('/').Last();
            strOut = "<select id='" + id + "' " + upd + " guidkey='tab' " + attributes + ">";
            var c = 0;
            var s = "";
            if (allowEmpty) strOut += "    <option value=''></option>";                
            foreach (var tItem in tList)
            {
                if (info.GetXmlProperty(xpath) == tItem.Key.ToString())
                    s = "selected";
                else
                    s = "";
                strOut += "    <option value='" + tItem.Key.ToString() + "' " + s + ">" + tItem.Value + "</option>";
            }
            strOut += "</select>";

            return new RawString(strOut);
        }

        public IEncodedString DnnLabel(String id, String resourceFileKey, String lang = "")
        {
            var strOut = new StringBuilder("<div class='dnnLabel'>");
            strOut.Append("<label><span>" + ResourceKey(resourceFileKey, lang) + "</span> </label>");
            var msg = ResourceKey(resourceFileKey, lang, "Help").ToString();
            if (msg == "") msg = ResourceKey(resourceFileKey, lang, "HelpText").ToString();
            if (msg != "")
            {
                strOut.Append("<a id='" + id + "_cmdHelp' tabindex='-1' class='dnnFormHelp' href='javascript:void();'></a>");
                strOut.Append("<div id='" + id + "_pnlHelp' class='dnnTooltip'><div class='dnnFormHelpContent dnnClear'>");
                strOut.Append("<span id='" + id + "_lblHelp' class='dnnHelpText'>" + msg + "</span>");
                strOut.Append("<a href='#' class='pinHelp'></a>");
                strOut.Append("</div></div>");                
            }
            strOut.Append("</div>");

            return new RawString(strOut.ToString());
        }

        public IEncodedString GetTabUrlByGuid(NBrightInfo info, String xpath)
        {
            var strOut = "";

            var t = (from kvp in TabController.GetTabsBySortOrder(PortalSettings.Current.PortalId) where kvp.UniqueId.ToString() == info.GetXmlProperty(xpath) select kvp.TabID);
            if (t.Any())
            {
                var tabid = t.First();
                strOut = Globals.NavigateURL(tabid);
            }

            return new RawString(strOut);
        }

        public IEncodedString ResourceKey(String resourceFileKey, String lang = "",String resourceExtension = "Text")
        {
            var strOut = "";
            if (_metadata.ContainsKey("resourcepath"))
            {
                var l = _metadata["resourcepath"];
                foreach (var r in l)
                {
                    strOut = DnnUtils.GetResourceString(r, resourceFileKey, resourceExtension, lang);
                    if (strOut != "") break;
                }
            }
            return new RawString(strOut);
        }


        #endregion

        #region "extra tokens"

        public IEncodedString EditCultureSelect(String cssclass, String cssclassli)
        {
            var enabledlanguages = LocaleController.Instance.GetLocales(PortalSettings.Current.PortalId);
            var strOut = new StringBuilder("<ul class='" + cssclass + "'>");
            foreach (var l in enabledlanguages)
            {
                strOut.Append("<li>");
                strOut.Append("<a href='javascript:void(0)' lang='" + l.Value.Code + "' class='selecteditlanguage " + cssclassli + "'><img src='/Images/Flags/" + l.Value.Code + ".gif' alt='" + l.Value.NativeName + "' /></a>");
                strOut.Append("</li>");
            }
            strOut.Append("</ul>");
            return new RawString(strOut.ToString());
        }


        public IEncodedString WebsiteUrl(String parameters = "")
        {
            var strOut = "";
            var ps = DnnUtils.GetCurrentPortalSettings();
            var strAry = ps.DefaultPortalAlias.Split('/');
            if (strAry.Any())
                strOut = strAry[0]; // Only display base domain, without lanaguge
            else
                strOut = ps.DefaultPortalAlias;
            if (strOut == "")
            {
                strAry = ps.PortalAlias.HTTPAlias.Split('/');
                if (strAry.Any())
                    strOut = strAry[0]; // Only display base domain, without lanaguge
                else
                    strOut = ps.DefaultPortalAlias;
            }
            if (parameters != "") strOut += "?" + parameters;
            return new RawString(strOut);
        }


        public IEncodedString HtmlOf(NBrightInfo info, String xpath)
        {
            var strOut = info.GetXmlProperty(xpath);
            strOut = System.Web.HttpUtility.HtmlDecode(strOut);
            return new RawString(strOut);
        }

        public IEncodedString BreakOf(NBrightInfo info, String xpath)
        {
            var strOut = info.GetXmlProperty(xpath);
            strOut = System.Web.HttpUtility.HtmlEncode(strOut);
            strOut = strOut.Replace(Environment.NewLine, "<br/>");
            strOut = strOut.Replace("\t", "&nbsp;&nbsp;&nbsp;");
            strOut = strOut.Replace("'", "&apos;");
            return new RawString(strOut);
        }

        public IEncodedString HeadingOf(String text, String headerstyle)
        {
            var headingstylestart = "<" + headerstyle + ">";
            var headingstyleend = "</" + headerstyle + ">";
            var strOut = headingstylestart + text + headingstyleend;
            return new RawString(strOut);
        }

        public IEncodedString CheckBoxListOf(NBrightInfo info, String xpath, String datavalue, String datatext, String attributes = "")
        {
            if (datavalue.StartsWith("ResourceKey:")) datavalue = ResourceKey(datavalue.Replace("ResourceKey:", "")).ToString();
            if (datatext.StartsWith("ResourceKey:")) datatext = ResourceKey(datatext.Replace("ResourceKey:", "")).ToString();
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var strOut = "";
            var datav = datavalue.Split(',');
            var datat = datatext.Split(',');
            if (datav.Count() == datat.Count())
            {
                strOut = "<ul " + attributes + ">";
                var c = 0;
                foreach (var v in datav)
                {
                    if (info.GetXmlProperty(xpath + "/chk[@data='" + v + "']/@value") == "True") strOut += "    <li>" + datat[c] + "</li>";
                    c += 1;
                }
                strOut += "</ul>";
            }
            return new RawString(strOut.ToString());
        }

        public IEncodedString FolderSelectList(NBrightInfo info, String xpath, String relitiveRootFolder, String attributes = "", Boolean allowEmpty = true)
        {
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var mappathRootFolder = System.Web.Hosting.HostingEnvironment.MapPath(relitiveRootFolder);
            var dirlist = System.IO.Directory.GetDirectories(mappathRootFolder);
            var tList = new List<String>();
            foreach (var d in dirlist)
            {
                var dr = new System.IO.DirectoryInfo(d);
                tList.Add(dr.Name);
            }
            var strOut = "";

            var upd = getUpdateAttr(xpath, attributes);
            var id = xpath.Split('/').Last();
            strOut = "<select id='" + id + "' " + upd + " " + attributes + ">";
            var c = 0;
            var s = "";
            if (allowEmpty) strOut += "    <option value=''></option>";
            foreach (var tItem in tList)
            {
                if (info.GetXmlProperty(xpath) == tItem)
                    s = "selected";
                else
                    s = "";
                strOut += "    <option value='" + tItem + "' " + s + ">" + tItem + "</option>";
            }
            strOut += "</select>";

            return new RawString(strOut);
        }


        #endregion


        #region functions

        public String getUpdateAttr(String xpath,String attributes)
        {
            var upd = "update='save'";
            if (xpath.StartsWith("genxml/lang/")) upd = "update='lang'";
            if (attributes.Contains("update=")) upd = "";
            return upd;
        }

        public String getChecked(NBrightInfo info, String xpath, Boolean defaultValue)
        {
            if (info.GetXmlProperty(xpath) == "True") return "checked='True'";
            if (info.GetXmlProperty(xpath) == "")
            {
                if (defaultValue) return "checked='True'";                
            }            
            return "";
        }

        #endregion


    }


}
