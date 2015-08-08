using System;
using System.ComponentModel;
using System.Linq;
using System.Web;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace NBrightDNN.render
{
    public class RazorEngineTokens<T> : TemplateBase<T>
    {

        #region general html control tokens

        public IEncodedString TextBox(NBrightInfo info, String xpath, String attributes = "", String defaultValue = "")
        {
            var upd = getUpdateAttr(xpath, attributes);
            var id = xpath.Split('/').Last();
            var value = info.GetXmlProperty(xpath);
            if (value == "") value = defaultValue;
            var strOut = "<input value='" + value + "' id='" + id + "' " + attributes + " " + upd + " type='text' />";

            return new RawString(strOut);
        }

        public IEncodedString RichTextBox(NBrightInfo info, String xpath, String attributes = "")
        {
            var upd = getUpdateAttr(xpath, attributes);
            var id = xpath.Split('/').Last();
            var strOut = " <textarea id='" + id + "' type='text' name='editor" + id + "' " + attributes + " " + upd + " >" + info.GetXmlProperty(xpath) + "</textarea>";
            strOut += "<script> var editorvar" + id + " = CKEDITOR.replace('editor" + id + "'); $('#savedata').click(function () { var value = editorvar" + id + ".getData(); $('#" + id + "').val(value);});</script>";
            return new RawString(strOut);
        }

        public IEncodedString CheckBox(NBrightInfo info, String xpath,String text, String attributes = "", Boolean defaultValue = false)
        {
            var upd = getUpdateAttr(xpath, attributes);
            var id = xpath.Split('/').Last();
            var strOut = "    <input id='" + id + "' type='checkbox' " + getChecked(info, xpath, defaultValue) + "' " + attributes + " " + upd + " /><label>" + text + "</label>";
            return new RawString(strOut);
        }

        public IEncodedString CheckBoxList(NBrightInfo info, String xpath, String datavalue, String datatext, String attributes = "", Boolean defaultValue = false)
        {
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

        public IEncodedString HeadingOf(String text,String headerstyle)
        {
            var headingstylestart = "<" + headerstyle + ">";
            var headingstyleend = "</" + headerstyle + ">";
            var strOut = headingstylestart + text + headingstyleend;
            return new RawString(strOut);
        }


        #endregion


        #region functions

        private String getUpdateAttr(String xpath,String attributes)
        {
            var upd = "update='save'";
            if (xpath.StartsWith("genxml/lang/")) upd = "update='lang'";
            if (attributes.Contains("update=")) upd = "";
            return upd;
        }

        private String getChecked(NBrightInfo info, String xpath, Boolean defaultValue)
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
