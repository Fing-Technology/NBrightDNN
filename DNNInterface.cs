using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using NBrightCore.providers;

namespace NBrightDNN
{
    public class DnnInterface : CmsProvider
    {

        public override bool IsInRole(string testRole)
        {
            var objUser = UserController.GetCurrentUserInfo();
            return objUser.IsInRole(testRole);
        }

        public override string HomeMapPath()
        {
            return DotNetNuke.Entities.Portals.PortalSettings.Current.HomeDirectoryMapPath;
        }

        public override void SetCache(string CacheKey, object objObject, DateTime AbsoluteExpiration)
        {
            DotNetNuke.Common.Utilities.DataCache.SetCache(CacheKey, objObject, AbsoluteExpiration);
        }

        public override object GetCache(string CacheKey)
        {
            return DotNetNuke.Common.Utilities.DataCache.GetCache(CacheKey);
        }

        public override void RemoveCache(string CacheKey)
        {
            DotNetNuke.Common.Utilities.DataCache.RemoveCache(CacheKey);
        }

        public override Dictionary<int, string> GetTabList(string CultureCode)
        {
            var tabList = DotNetNuke.Entities.Tabs.TabController.GetTabsBySortOrder(DotNetNuke.Entities.Portals.PortalSettings.Current.PortalId, CultureCode, true);
            var rtnList = new Dictionary<int, string>();
            return GetTreeTabList(rtnList,tabList,0,0);
        }

        public override List<string> GetCultureCodeList()
        {
            return DnnUtils.GetCultureCodeList();
        }

		public override Dictionary<String, String> GetResourceData(String ResourcePath, String ResourceKey)
		{
			var rtnList = new Dictionary<String, String>();
			var s = ResourceKey.Split('.');
			if (s.Length == 2 && ResourcePath != "")
			{
				var fName = s[0];
				var rKey = s[1];
				var fullFileName = System.Web.Hosting.HostingEnvironment.MapPath(ResourcePath.TrimEnd('/') + "/" + fName + ".ascx.resx");
				if (DnnUtils.GetCurrentValidCultureCode().Substring(0, 2).ToLower() != "en")
				{
					var tmpfullFileName = System.Web.Hosting.HostingEnvironment.MapPath(ResourcePath.TrimEnd('/') + "/" + fName + ".ascx." + DnnUtils.GetCurrentValidCultureCode() + ".resx");
					if (System.IO.File.Exists(tmpfullFileName))
					{
						fullFileName = tmpfullFileName;
					}
				}

				if (!String.IsNullOrEmpty(fullFileName) && System.IO.File.Exists(fullFileName))
				{
					var xmlDoc = new XmlDataDocument();
					xmlDoc.Load(fullFileName);
					var xmlNodList = xmlDoc.SelectNodes("root/data");
					if (xmlNodList != null)
					{
						foreach (XmlNode nod in xmlNodList)
						{
							if (nod.Attributes != null && nod.Attributes["name"] != null)
							{
								var n = nod.Attributes["name"].Value;
								var v = nod.SelectSingleNode("value");
								if (n.StartsWith(rKey + "."))
								{
									rtnList.Add(n.Replace(rKey + ".", ""), v.InnerText);
								}
							}
						}
					}
				}
			}
			return rtnList;
		}


        private Dictionary<int, string> GetTreeTabList(Dictionary<int, string> rtnList, List<TabInfo> tabList, int level, int parentid, string prefix = "")
        {
            if (level > 20) // stop infinate loop
            {
                return rtnList;
            }
            if (parentid > 0) prefix += "..";
            foreach (TabInfo tInfo in tabList)
            {
                var parenttestid = tInfo.ParentId;
                if (parenttestid < 0) parenttestid = 0;
                if (parentid == parenttestid)
                {
                    if (!tInfo.IsDeleted && tInfo.TabPermissions.Count > 2)
                    {
                        rtnList.Add(tInfo.TabID, prefix + "" + tInfo.TabName);
                        GetTreeTabList(rtnList, tabList, level + 1, tInfo.TabID, prefix);
                    }
                }
            }

            return rtnList;
        }


    }
}
