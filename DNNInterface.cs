using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Xml;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Localization;
using NBrightCore.common;
using NBrightCore.providers;

namespace NBrightDNN
{
    public class DnnInterface : CmsProvider
    {

        public override int GetCurrentUserId()
        {
            var objUser = UserController.GetCurrentUserInfo();
            return objUser.UserID;
        }

        public override String GetCurrentUserName()
        {
            var objUser = UserController.GetCurrentUserInfo();
            if (objUser.Username == null) return "";
            return objUser.Username;
        }

        public override bool IsInRole(string testRole)
        {
            var objUser = UserController.GetCurrentUserInfo();
            return objUser.IsInRole(testRole);
        }

        public override string HomeMapPath()
        {
            return DotNetNuke.Entities.Portals.PortalSettings.Current.HomeDirectoryMapPath;
        }

        public override void SetCache(string cacheKey, object objObject, DateTime absoluteExpiration)
        {
            DataCache.SetCache(cacheKey, objObject, absoluteExpiration);
        }

        public override object GetCache(string cacheKey)
        {
            return DataCache.GetCache(cacheKey);
        }

        public override void RemoveCache(string cacheKey)
        {
            DataCache.RemoveCache(cacheKey);
        }

        public override Dictionary<int, string> GetTabList(string cultureCode)
        {
            return DnnUtils.GetTreeTabList();
        }

        public override List<string> GetCultureCodeList()
        {
            return DnnUtils.GetCultureCodeList();
        }

		public override Dictionary<String, String> GetResourceData(String resourcePath, String resourceKey,String lang = "")
		{
		    if (lang == "") lang = DnnUtils.GetCurrentValidCultureCode();
            var ckey = resourcePath + resourceKey + lang;
			var obj  = GetCache(ckey);
			if (obj != null) return (Dictionary<String, String>)obj;

			var rtnList = new Dictionary<String, String>();
			var s = resourceKey.Split('.');
            if (s.Length == 2 && resourcePath != "")
            {
                var fName = s[0];
                var rKey = s[1];
                var relativefilename = resourcePath.TrimEnd('/') + "/" + fName + ".ascx.resx";
                var fullFileName = System.Web.Hosting.HostingEnvironment.MapPath(relativefilename);
                if (!String.IsNullOrEmpty(fullFileName) && System.IO.File.Exists(fullFileName))
                {
                    var xmlDoc = new XmlDataDocument();
                    xmlDoc.Load(fullFileName);
                    var xmlNodList = xmlDoc.SelectNodes("root/data[starts-with(./@name,'" + rKey + ".')]");
                    if (xmlNodList != null)
                    {
                        foreach (XmlNode nod in xmlNodList)
                        {
                            var n = nod.Attributes["name"].Value;
                            var rtnValue = Localization.GetString(n, relativefilename, PortalSettings.Current, Utils.GetCurrentCulture(), true);
                            rtnList.Add(n.Replace(rKey + ".", ""), rtnValue);
                        }
                    }
                }

				SetCache(ckey, rtnList, DateTime.Now.AddMinutes(20));
            }
		    return rtnList;
		}





    }
}
