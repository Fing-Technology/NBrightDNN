using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using NBrightCore;
using NBrightCore.common;
using NBrightCore.providers;
using NBrightCore.render;
using DotNetNuke.Entities.Users;

namespace NBrightDNN.render
{
    public class GenXmlTemplateExt : GenXProvider
    {

        private string _rootname = "genxml";
        private string _databindColumn = "XMLData";


        #region "Override methods"
        // This section overrides the interface methods for the GenX provider.
        // It allows providers to create controls/Literals in the NBright template system.

        public override bool CreateGenControl(string ctrltype, Control container, XmlNode xmlNod, string rootname = "genxml", string databindColum = "XMLData", string cultureCode = "", Dictionary<string, string> settings = null)
        {
            _rootname = rootname;
            _databindColumn = databindColum;

            switch (ctrltype)
            {
                case "dnntexteditor":
                    CreateTextEditor(container, xmlNod);
                    return true;
                case "dnnlabelcontrol":
                    CreateLabelControl(container, xmlNod);
                    return true;
                case "dnndatecontrol":
                    CreateDateControl(container, xmlNod);
                    return true;
                case "dateeditcontrol": // Legacy name for "dnndatecontrol"
                    CreateDateControl(container, xmlNod);
                    return true;
                case "dnnhomedirectory":
                    CreatePortalHomeDirectory(container);
                    return true;
                default:
                    return false;

            }

        }

        #region "GenXmlFunction Extension"

        public override string GetField(Control ctrl)
        {
            if (ctrl is GenTextEditor)
            {
                return ((GenTextEditor)ctrl).Text;
            }
            if (ctrl is GenDateControl)
            {
                return ((GenDateControl)ctrl).Text;
            }
            return "";
        }

        public override void SetField(Control ctrl, string newValue)
        {
            if (ctrl is GenTextEditor)
            {
                ((GenTextEditor)ctrl).Text = newValue;
            }
            if (ctrl is GenDateControl)
            {
                ((GenDateControl)ctrl).Text = newValue;
            }
        }

        public override string GetGenXml(List<Control> genCtrls, XmlDataDocument xmlDoc, string originalXml, string folderMapPath, string xmlRootName = "genxml")
        {

            //check row exists (0 based)
            if (genCtrls.Count == 0)
            {
                return "";
            }

            var teCtrls = new List<Control>();

            //build list of controls
            foreach (var ctrl in genCtrls)
            {
                if (ctrl is GenTextEditor)
                {
                    teCtrls.Add(ctrl);
                }
            }

            //Create XML
            var strXml = "";

            //Text Editor
            strXml += "<edt>";
            foreach (GenTextEditor gteCtrl in teCtrls)
            {
                var gteCtrlText = System.Web.HttpUtility.HtmlDecode(gteCtrl.Text);
                if (!string.IsNullOrEmpty(originalXml))
                {
                    GenXmlFunctions.ReplaceXmlNode(xmlDoc, xmlRootName + "/edt/" + gteCtrl.ID.ToLower(), gteCtrlText);
                }
                else
                {
                    strXml += "<" + gteCtrl.ID.ToLower() + "><![CDATA[";
                    try
                    {
                        strXml += gteCtrlText;
                        // ReSharper disable EmptyGeneralCatchClause
                    }
                    catch (Exception)
                    {
                        // ReSharper restore EmptyGeneralCatchClause
                        //do nothing, fails if updating from datalist created in-line code.
                    }
                    strXml += "]]></" + gteCtrl.ID.ToLower() + ">";
                }
            }
            strXml += "</edt>";

            return strXml;
        }

        public override string GetGenXmlTextBox(List<Control> genCtrls, XmlDataDocument xmlDoc, string originalXml, string folderMapPath, string xmlRootName = "genxml")
        {

            //check row exists (0 based)
            if (genCtrls.Count == 0)
            {
                return "";
            }

            var deCtrls = new List<Control>();

            //build list of controls
            foreach (var ctrl in genCtrls)
            {
                if (ctrl is GenDateControl)
                {
                    deCtrls.Add(ctrl);
                }
            }

            //Create XML
            var strXml = "";

            foreach (GenDateControl txtCtrl in deCtrls)
            {
                    if (!string.IsNullOrEmpty(originalXml))
                    {
                        GenXmlFunctions.ReplaceXmlNode(xmlDoc, xmlRootName + "/textbox/" + txtCtrl.ID.ToLower(), Utils.FormatToSave(txtCtrl.Text));
                    }
                    else
                    {
                        strXml += "<" + txtCtrl.ID.ToLower() + " datatype=\"date\"><![CDATA[";
                        strXml += Utils.FormatToSave(txtCtrl.Text, TypeCode.DateTime);
                        strXml += "]]></" + txtCtrl.ID.ToLower() + ">";
                    }
            }

            return strXml;
        }

        public override object PopulateGenObject(List<Control> genCtrls, object obj)
        {
            //check row exists (0 based)
            if (genCtrls.Count == 0)
            {
                return "";
            }

            var teCtrls = new List<Control>();
            var deCtrls = new List<Control>();

            //build list of controls
            foreach (var ctrl in genCtrls)
            {
                if (ctrl is GenTextEditor)
                {
                    teCtrls.Add(ctrl);
                }
                if (ctrl is GenDateControl)
                {
                    deCtrls.Add(ctrl);
                }
            }

            foreach (GenTextEditor gteCtrl in teCtrls)
            {
                if ((gteCtrl.Attributes["databind"] != null))
                {
                    obj = GenXmlFunctions.AssignByReflection(obj, gteCtrl.Attributes["databind"], gteCtrl.Text);
                }
            }

            foreach (GenDateControl dteCtrl in deCtrls)
            {
                if ((dteCtrl.Attributes["databind"] != null))
                {
                    obj = GenXmlFunctions.AssignByReflection(obj, dteCtrl.Attributes["databind"], dteCtrl.Value.ToString());
                }
            }

            return obj;
        }

        #endregion

        #endregion

        #region "Private support Methods"
        //These methods create the actual controls and databinding for the controls specified in the "CreateGenControl" method.

        #region "Text Editor"

        private void CreateTextEditor(Control container, XmlNode xmlNod)
        {
            var te = new GenTextEditor(xmlNod);
            if (xmlNod.Attributes != null && (xmlNod.Attributes["id"] != null))
            {
                te.ID = xmlNod.Attributes["id"].InnerXml;

                if (xmlNod.Attributes != null && (xmlNod.Attributes["databind"] != null))
                {
                    te.Attributes.Add("databind", xmlNod.Attributes["databind"].InnerXml);
                }

                te.Visible = GetRoleVisible(xmlNod.OuterXml);

                te.DataBinding += TextEditorDataBinding;
                container.Controls.Add(te);
            }
        }

        private void TextEditorDataBinding(object sender, EventArgs e)
        {
            var gte = (GenTextEditor)sender;
            var container = (IDataItemContainer)gte.NamingContainer;
            try
            {
                gte.Visible = NBrightGlobal.IsVisible;

                if ((gte.Attributes["databind"] != null))
                {
                    gte.Text = (string)DataBinder.Eval(container.DataItem, gte.Attributes["databind"]);
                }
                else
                {
                    gte.Text = GenXmlFunctions.GetGenXmlValue(gte.ID, "edt", (string)DataBinder.Eval(container.DataItem, _databindColumn));
                }

            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
            // ReSharper restore EmptyGeneralCatchClause
            {
                //do nothing
            }
        }
        
        #endregion

        #region "Date Control"

        private void CreateDateControl(Control container, XmlNode xmlNod)
        {
            if (xmlNod.Attributes != null && (xmlNod.Attributes["id"] != null))
            {
                var dte = new GenDateControl();

                dte.ID = xmlNod.Attributes["id"].InnerXml;

                dte.Visible = GetRoleVisible(xmlNod.OuterXml);
                dte.Enabled = GetRoleEnabled(xmlNod.OuterXml);

                dte.DataBinding += DateControlDataBinding;
                container.Controls.Add(dte);
            }
        }

        private void DateControlDataBinding(object sender, EventArgs e)
        {
            var dte = (GenDateControl)sender;
            var container = (IDataItemContainer)dte.NamingContainer;
            try
            {
                dte.Visible = NBrightGlobal.IsVisible;

                if ((dte.Attributes["databind"] != null))
                {
                    dte.Text = Convert.ToString(DataBinder.Eval(container.DataItem, dte.Attributes["databind"]));
                }
                else
                {
                    dte.Text = GenXmlFunctions.GetGenXmlValue(dte.ID, "textbox", (string)DataBinder.Eval(container.DataItem, _databindColumn));
                }

            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
            // ReSharper restore EmptyGeneralCatchClause
            {
                //do nothing
            }
        }

        #endregion

        #region "label control"

        /// <summary title="DNN label Control" >
        /// <para class="text">
        /// Using the dnn Label Control
        /// </para>
        /// <para class="example">
        /// [<tag id="lbllabelkey" type="dnnlabelcontrol" Text="label : " HelpText="Help text to be displayed"  />]
        /// </para>
        /// </summary>
        private void CreateLabelControl(Control container, XmlNode xmlNod)
        {
            var lblc = new GenLabelControl(xmlNod);
            if (xmlNod.Attributes != null && (xmlNod.Attributes["id"] != null))
            {
                lblc.ID = "glc" + xmlNod.Attributes["id"].InnerXml;

                if  (xmlNod.Attributes != null && xmlNod.Attributes["databind"] != null)
                {
                    lblc.Attributes.Add("databind", xmlNod.Attributes["databind"].InnerXml);
                }

                lblc.DataBinding += LabelControlBinding;

                // no event triggered for dnn label control, so we can;t remove it with visible = false, so dnnlabels need to be hidden by css.
                container.Controls.Add(lblc);
            }
        }

        private void LabelControlBinding(object sender, EventArgs e)
        {
            var lbl = (GenLabelControl)sender;
            try
            {
                lbl.Visible = NBrightGlobal.IsVisible;
            }
            catch (Exception)
            {
                //do nothing
            }
        }


        #endregion

        #region "Portal Settings value"

        /// <summary title="DNN Portal Home Diirectory" >
        /// <para class="text">
        /// Output current DNN Portal Home Directory
        /// </para>
        /// <para class="example">
        /// [<tag type="dnnhomedirectory" />]
        /// </para>
        /// </summary>
        private void CreatePortalHomeDirectory(Control container)
        {
            var lc = new Literal();
            var PS = DnnUtils.GetCurrentPortalSettings();
            lc.Text = PS.HomeDirectory;
            lc.DataBinding += LiteralDataBinding;
            container.Controls.Add(lc);
        }

        #endregion

        private void LiteralDataBinding(object sender, EventArgs e)
        {
            try
            {
                var lc = (Literal)sender;
                lc.Visible = NBrightGlobal.IsVisible;
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        #endregion

        #region "Required Support Methods"
        // Methods to check if control/literal is displayable to the user
        // These methods needs to be the provider so the template system test will work on CMS roles.

        private static bool GetRoleEnabled(string xmLproperties)
        {
            var genprop = GenXmlFunctions.GetGenControlPropety(xmLproperties, "editinrole");
            if (genprop == "")
            {
                return true;
            }
            var objUser = UserController.GetCurrentUserInfo();
            return objUser.IsInRole(genprop);
        }

        private static bool GetRoleVisible(string xmLproperties)
        {
            var genprop = GenXmlFunctions.GetGenControlPropety(xmLproperties, "viewinrole");
            if (genprop == "")
            {
                return true;
            }
            var objUser = UserController.GetCurrentUserInfo();
            return objUser.IsInRole(genprop);
        }

        #endregion
    }
}
