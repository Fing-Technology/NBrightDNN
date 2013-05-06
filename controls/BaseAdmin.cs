using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Common;
using NBrightCore.common;
using NBrightCore.render;

namespace NBrightDNN.controls
{
    public class BaseAdmin : NBrightDNN.controls.BasePage
    {
        protected Repeater CtrlBanner;

        public ControlTabMenu CtrlTabMenu;

        protected override void OnInit(EventArgs e)
        {
            if (Utils.RequestQueryStringParam(Context, "SkinSrc") == "")
            {
                base.OnInit(e);                
            }
            else
            {

            this.Controls.AddAt(0, new LiteralControl("<div id='NBright_Content'>")); // create wrapper for content

            base.OnInit(e);

			if (String.IsNullOrEmpty(ControlAdminIncludePath)) ControlAdminIncludePath = ControlAdminPath;

            char[] charsToTrim = { '/', '\\', '.', ' ' };
			if (System.IO.File.Exists(MapPath(ControlAdminIncludePath + "ui/admin.css")))
            {
				NBrightCore.common.PageIncludes.IncludeCssFile(Page, "GenCSSadmin.css", ControlAdminIncludePath.TrimEnd(charsToTrim) + "/ui/admin.css");
            }
			if (System.IO.File.Exists(MapPath(ControlAdminIncludePath + "ui/admin.js")))
            {
				NBrightCore.common.PageIncludes.IncludeJsFile(Page, "GenCSSadmin.js", ControlAdminIncludePath.TrimEnd(charsToTrim) + "/ui/admin.js");
            }


            // *** Menu ***
            CtrlTabMenu = new ControlTabMenu();
            CtrlTabMenu.ControlAdminPath = ControlAdminPath;
            //CtrlTabMenu.DebugMode = true;
            this.Controls.AddAt(0, CtrlTabMenu);


            //*** Banner ***
            this.Controls.AddAt(0, new LiteralControl("</div>")); // end banner
            var strItemTemplate = TemplCtrl.GetTemplateData("Banner.txt", Utils.GetCurrentCulture());
            if (!strItemTemplate.StartsWith("NO TEMPLATE FOUND"))
            {
                CtrlBanner = new Repeater();
                this.Controls.AddAt(0, CtrlBanner);
                CtrlBanner.ItemTemplate = new GenXmlTemplate(strItemTemplate);
                var obj = new object();
                var l = new List<object> { obj };
                CtrlBanner.DataSource = l;
                CtrlBanner.DataBind();
            }
            this.Controls.AddAt(0, new LiteralControl("<div id='NBright_Banner'>")); // create banner.


            this.Controls.AddAt(0, new LiteralControl("<div id='NBright_Container'>")); // create full wrapper for content.
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            this.Controls.Add(new LiteralControl("</div>")); // end wrapper for content
            this.Controls.Add(new LiteralControl("</div>")); // end wrapper

            base.OnLoad(e);

        }


        public void HideBanner()
        {
            CtrlBanner.Visible = false;
        }

        public void HideMenu()
        {
            CtrlTabMenu.Visible = false;
        }

        public new string EditUrl()
        {
            return this.EditUrl("", "", "Edit");
        }

        public new string EditUrl(string ControlKey)
        {
            return this.EditUrl("", "", ControlKey);
        }

        public new string EditUrl(string KeyName, string KeyValue)
        {
            return this.EditUrl(KeyName, KeyValue, "Edit");
        }

        public new string EditUrl(string KeyName, string KeyValue, string ControlKey)
        {
			string[] paramlist = new string[0];
			return EditUrl(KeyName,  KeyValue, ControlKey, paramlist);
        }

		public new string EditUrl ( string KeyName, string KeyValue, string ControlKey, string[] AdditionalParameters )
		{
			var objBase = new DotNetNuke.Entities.Modules.PortalModuleBase();
			var skinSrc = Globals.QueryStringEncode(Utils.RequestQueryStringParam(Context, "SkinSrc"));
			var key = ControlKey;

			if ((string.IsNullOrEmpty(key)))
			{
				key = "Edit";
			}

			var tempList = new Dictionary<String, String>();

			tempList.Add("mid", Convert.ToString(ModuleId));

			if (!string.IsNullOrEmpty(KeyName) & !string.IsNullOrEmpty(KeyValue))
			{
				tempList.Add(KeyName.ToLower(), KeyValue);
			}

			if (!string.IsNullOrEmpty(skinSrc))
			{
				tempList.Add("SkinSrc", skinSrc);
			}

			for (int i = 0; i <= AdditionalParameters.Length - 1; i++)
			{
				tempList.Add(AdditionalParameters[i].Split('=')[0].ToLower(), AdditionalParameters[i].Split('=')[1]);
			}

			// build array to pass as param
			var paramlist = new string[tempList.Keys.Count];
			var lp = 0;
			foreach (var t in tempList)
			{
				paramlist[lp] = t.Key + "=" + t.Value;
				lp += 1;
			}

			return Globals.NavigateURL(PortalSettings.ActiveTab.TabID, key, paramlist);
		}
    }
}
