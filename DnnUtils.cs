using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Xml;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.FileSystem;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using NBrightCore.common;

namespace NBrightDNN
{
    public class DnnUtils
    {

        public static void UnZip(string zipFileMapPath, string outputFolder)
        {
            var zipStream = new FileStream(zipFileMapPath, FileMode.Open, FileAccess.Read);
            var zStream = new ZipInputStream(zipStream);
            DotNetNuke.Common.Utilities.FileSystemUtils.UnzipResources(zStream, outputFolder);
            zipStream.Close();
            zStream.Close();
        }


        public static List<System.IO.FileInfo> GetFiles(string FolderMapPath)
        {
            DirectoryInfo di = new DirectoryInfo(FolderMapPath);
            List<System.IO.FileInfo> files = new List<System.IO.FileInfo>();
            foreach (System.IO.FileInfo file in di.GetFiles())
            {
                    files.Add(file);
            }
            return files;
        }

        public static List<string> GetCultureCodeList()
        {
			var rtnList = new List<string>();
			if (DotNetNuke.Entities.Portals.PortalSettings.Current != null)
			{
				var objPortalSettings = DotNetNuke.Entities.Portals.PortalSettings.Current;
				var enabledLanguages = LocaleController.Instance.GetLocales(objPortalSettings.PortalId);
				foreach (KeyValuePair<string, Locale> kvp in enabledLanguages)
				{
					rtnList.Add(kvp.Value.Code);
				}				
			}
            return rtnList;            
        }

        public static string GetCurrentValidCultureCode(List<string> validCultureCodes = null)
        {
            var validCurrentCulture = Utils.GetCurrentCulture();

            if (validCultureCodes != null)
            {
                if (validCultureCodes.Count > 0 && !validCultureCodes.Contains(validCurrentCulture))
                {
                    //Cannot find the current culture so return the first in the valid list
                    return validCultureCodes[0];
                }
            }

            return validCurrentCulture;
        }

        public static string GetDataResponseAsString(string dataurl,string headerFieldId = "",string headerFieldData = "")
        {
            string strOut = "";
            if (!string.IsNullOrEmpty(dataurl))
            {
                try
                {

                    // solution for exception
                    // The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel.
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                    System.Net.HttpWebRequest req = DotNetNuke.Common.Globals.GetExternalRequest(dataurl);
                    if (headerFieldId!= "")
                    {
                        //allow a get request to pass data via the header.
                        //  This is limited to 32K by default in IIS, but can be limited to less. (So use with care!)
                        req.Headers.Add(headerFieldId,headerFieldData);
                    }
                    System.Net.WebResponse resp = req.GetResponse();
                    var s = resp.GetResponseStream();
                    if (s != null)
                    {
                        var reader = new StreamReader(s);
                        strOut = reader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    strOut = "ERROR - dataurl=" + dataurl + "  ex=" + ex.ToString();
                }
            }
            else
            {
                strOut = "ERROR - No Data Url given";
            }
            return strOut;
        }


        public static void PurgeDataBaseInfo(int portalId, int moduleId, DataCtrlInterface objCtrl, string entityTypeCode, int purgeDays = -7)
        {
            var l = objCtrl.GetList(portalId, moduleId, entityTypeCode);
            foreach (NBrightInfo obj in l)
            {
                if (obj.ModifiedDate < (DateTime.Now.AddDays(purgeDays)))
                {
                    objCtrl.Delete(obj.ItemID);                    
                }
            }

        }

        public static DotNetNuke.Entities.Tabs.TabCollection GetPortalTabs(int portalId)
        {
            var portalTabs = (DotNetNuke.Entities.Tabs.TabCollection)NBrightCore.common.Utils.GetCache("NBright_portalTabs" + portalId.ToString(""));
            if (portalTabs == null)
            {
                var objTabCtrl = new DotNetNuke.Entities.Tabs.TabController();
                portalTabs = objTabCtrl.GetTabsByPortal(portalId);
                NBrightCore.common.Utils.SetCache("NBright_portalTabs" + portalId.ToString(""), portalTabs);
            }
            return portalTabs;
        }

        public static DotNetNuke.Entities.Users.UserInfo GetValidUser(int PortalId, string username, string password)
        {
            var userLoginStatus = new DotNetNuke.Security.Membership.UserLoginStatus();
            return DotNetNuke.Entities.Users.UserController.ValidateUser(PortalId, username, password, "", "", "", ref userLoginStatus);
        }

        public static bool IsValidUser(int PortalId, string username, string password)
        {
            var u = GetValidUser(PortalId,username,password); 
            if (u != null)
            {
                return true;
            }
            return false;
        }

        public static string GetLocalizedString(string Key, string resourceFileRoot,string lang)
        {
            return Localization.GetString(Key, resourceFileRoot, lang);
        }

        public static int GetPortalByModuleID(int moduleId)
        {
            var objMCtrl = new DotNetNuke.Entities.Modules.ModuleController();
            var objMInfo = objMCtrl.GetModule(moduleId);
            if (objMInfo == null) return -1;
            return objMInfo.PortalID;
        }

        //Get portal list to overcome problem with DNN6 GetPortals when ran from scheduler.
        public static List<PortalInfo> GetAllPortals()
        {
            var pList = new List<PortalInfo>();
            var objPC = new DotNetNuke.Entities.Portals.PortalController();
            PortalInfo objPInfo;

            for (var lp = 0; lp <= 500; lp++)
            {
                objPInfo = objPC.GetPortal(lp);
                if ((objPInfo != null))
                {
                    pList.Add(objPInfo);
                }
                else
                {
                    break;
                }
            }

            return pList;
        }

		public static string GetModuleVersion(int moduleId)
		{
			var strVersion = "";
			var objMCtrl = new DotNetNuke.Entities.Modules.ModuleController();
			var objMInfo = objMCtrl.GetModule(moduleId);
			if (objMInfo != null)
			{
				strVersion = objMInfo.DesktopModule.Version;
			}
			return strVersion;
		}

        public static ModuleInfo GetModuleinfo(int moduleId)
        {
            var objMCtrl = new DotNetNuke.Entities.Modules.ModuleController();
            var objMInfo = objMCtrl.GetModule(moduleId);
            return objMInfo;
        }

        public static void CreateFolder(string fullfolderPath)
        {
            // This function is to get around medium trust not allowing createfolder in .Net 2.0. 
            // DNN seems to have some work around (Not followed up on what exactly, probably security allowed in shared hosting environments for DNN except???).
            // But this leads me to have this rather nasty call to DNN FolderManager.
            // Prefered method is to us the Utils.CreateFolder function in NBrightCore.

            var blnCreated = false;
            //try normal test (doesn;t work on medium trust, but stops us forcing a "AddFolder" and suppressing the error.)
            try
            {
                blnCreated = System.IO.Directory.Exists(fullfolderPath);
            }
            catch (Exception ex)
            {
                blnCreated = false;
            }

            if (!blnCreated)
            {
                try
                {
                    var f = FolderManager.Instance.AddFolder(FolderMappingController.Instance.GetFolderMapping(8), fullfolderPath);
                }
                catch (Exception ex)
                {
                    // Suppress error, becuase the folder may already exist!..NASTY!!..try and find a better way to deal with folders out of portal range!!
                }
            }
        }

        public static void CreatePortalFolder(DotNetNuke.Entities.Portals.PortalSettings PortalSettings, string FolderName)
        {
            bool blnCreated = false;

            //try normal test (doesn;t work on medium trust, but avoids waiting for GetFolder.)
            try
            {
                blnCreated = System.IO.Directory.Exists(PortalSettings.HomeDirectoryMapPath + FolderName);
            }
            catch (Exception ex)
            {
                blnCreated = false;
            }

            if (!blnCreated)
            {
                FolderManager.Instance.Synchronize(PortalSettings.PortalId, PortalSettings.HomeDirectory, true,true);
                var folderInfo =  FolderManager.Instance.GetFolder(PortalSettings.PortalId, FolderName);
                if (folderInfo == null & !string.IsNullOrEmpty(FolderName))
                {
                    //add folder and permissions
                    try
                    {
                        FolderManager.Instance.AddFolder(PortalSettings.PortalId, FolderName);
                    }
                    catch (Exception ex)
                    {
                    }
                    folderInfo = FolderManager.Instance.GetFolder(PortalSettings.PortalId, FolderName);
                    if ((folderInfo != null))
                    {
                        int folderid = folderInfo.FolderID;
                        DotNetNuke.Security.Permissions.PermissionController objPermissionController = new DotNetNuke.Security.Permissions.PermissionController();
                        var arr = objPermissionController.GetPermissionByCodeAndKey("SYSTEM_FOLDER", "");
                        foreach (DotNetNuke.Security.Permissions.PermissionInfo objpermission in arr)
                        {
                            if (objpermission.PermissionKey == "WRITE")
                            {
                                // add READ permissions to the All Users Role
                                FolderManager.Instance.SetFolderPermission(folderInfo, objpermission.PermissionID, int.Parse(DotNetNuke.Common.Globals.glbRoleAllUsers));
                            }
                        }
                    }
                }
            }
        }

        public static DotNetNuke.Entities.Portals.PortalSettings GetCurrentPortalSettings()
        {
            return (DotNetNuke.Entities.Portals.PortalSettings) System.Web.HttpContext.Current.Items["PortalSettings"];
        }


    }
}
