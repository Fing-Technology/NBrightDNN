using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.FileSystem;
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
            UnzipResources(zStream, outputFolder);
            zipStream.Close();
            zStream.Close();
        }

        public static void UnzipResources(ZipInputStream zipStream, string destPath)
        {
            try
            {
                ZipEntry objZipEntry;
                string LocalFileName;
                string RelativeDir;
                string FileNamePath;
                objZipEntry = zipStream.GetNextEntry();
                while (objZipEntry != null)
                {
                    LocalFileName = objZipEntry.Name;
                    RelativeDir = Path.GetDirectoryName(objZipEntry.Name);
                    if ((RelativeDir != string.Empty) && (!Directory.Exists(Path.Combine(destPath, RelativeDir))))
                    {
                        Directory.CreateDirectory(Path.Combine(destPath, RelativeDir));
                    }
                    if ((!objZipEntry.IsDirectory) && (!String.IsNullOrEmpty(LocalFileName)))
                    {
                        FileNamePath = Path.Combine(destPath, LocalFileName).Replace("/", "\\");
                        try
                        {
                            if (File.Exists(FileNamePath))
                            {
                                File.SetAttributes(FileNamePath, FileAttributes.Normal);
                                File.Delete(FileNamePath);
                            }
                            FileStream objFileStream = null;
                            try
                            {
                                objFileStream = File.Create(FileNamePath);
                                int intSize = 2048;
                                var arrData = new byte[2048];
                                intSize = zipStream.Read(arrData, 0, arrData.Length);
                                while (intSize > 0)
                                {
                                    objFileStream.Write(arrData, 0, intSize);
                                    intSize = zipStream.Read(arrData, 0, arrData.Length);
                                }
                            }
                            finally
                            {
                                if (objFileStream != null)
                                {
                                    objFileStream.Close();
                                    objFileStream.Dispose();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                         //   DnnLog.Error(ex);
                        }
                    }
                    objZipEntry = zipStream.GetNextEntry();
                }
            }
            finally
            {
                if (zipStream != null)
                {
                    zipStream.Close();
                    zipStream.Dispose();
                }
            }
        }



        public static List<System.IO.FileInfo> GetFiles(string FolderMapPath)
        {
            DirectoryInfo di = new DirectoryInfo(FolderMapPath);
            List<System.IO.FileInfo> files = new List<System.IO.FileInfo>();
            if (di.Exists)
            {
                foreach (System.IO.FileInfo file in di.GetFiles())
                {
                    files.Add(file);
                }                
            }
            return files;
        }

        public static List<string> GetCountryCodeList(int portalId = -1)
        {
            var rtnList = new List<string>();
            if (portalId == -1 && PortalSettings.Current != null) portalId = PortalSettings.Current.PortalId;
            if (portalId != -1)
            {
                var objLCtrl = new DotNetNuke.Common.Lists.ListController();
            }
            return rtnList;
        }

        public static List<string> GetCultureCodeList(int portalId = -1)
        {
			var rtnList = new List<string>();
            if (portalId == -1 && PortalSettings.Current != null) portalId = PortalSettings.Current.PortalId;
            if (portalId != -1)
			{
				var enabledLanguages = LocaleController.Instance.GetLocales(portalId);
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

        public static Dictionary<int, string> GetTreeTabList()
        {
            var tabList = DotNetNuke.Entities.Tabs.TabController.GetTabsBySortOrder(DotNetNuke.Entities.Portals.PortalSettings.Current.PortalId, Utils.GetCurrentCulture(), true);
            var rtnList = new Dictionary<int, string>();
            return GetTreeTabList(rtnList, tabList, 0, 0);
        }

        private static Dictionary<int, string> GetTreeTabList(Dictionary<int, string> rtnList, List<TabInfo> tabList, int level, int parentid, string prefix = "")
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

        public static Dictionary<string, string> GetUserProfileProperties(UserInfo userInfo)
        {
            var prop = new Dictionary<string, string>();
            foreach (DotNetNuke.Entities.Profile.ProfilePropertyDefinition p in userInfo.Profile.ProfileProperties)
            {
                prop.Add(p.PropertyName, p.PropertyValue);
            }
            return prop;
        }

        public static Dictionary<string, string> GetUserProfileProperties(String userId)
        {
            if (!Utils.IsNumeric(userId)) return null;
            var userInfo = UserController.GetUserById(PortalSettings.Current.PortalId,Convert.ToInt32(userId));
            return GetUserProfileProperties(userInfo);
        }

        public static void SetUserProfileProperties(UserInfo userInfo,Dictionary<string, string> properties)
        {
            foreach (var p in properties)
            {
                userInfo.Profile.SetProfileProperty(p.Key,p.Value);
                UserController.UpdateUser(PortalSettings.Current.PortalId, userInfo);
            }
        }
        public static void SetUserProfileProperties(String userId,Dictionary<string, string> properties)
        {
            if (Utils.IsNumeric(userId))
            {
                var userInfo = UserController.GetUserById(PortalSettings.Current.PortalId, Convert.ToInt32(userId));
                SetUserProfileProperties(userInfo, properties);                
            }
        }


        #region "encryption"

        public static String Encrypt(String value, String passkey = "")
        {
            var objSec = new PortalSecurity();
            if (value == null) return "";
            if (passkey == "")
            {
                var ps = GetCurrentPortalSettings();
                passkey = ps.GUID.ToString();
            }
            return objSec.Encrypt(passkey, value);
        }

        public static String Decrypt(String value, String passkey = "")
        {
            var objSec = new PortalSecurity();
            if (value == null) return "";
            if (passkey == "")
            {
                var ps = GetCurrentPortalSettings();
                passkey = ps.GUID.ToString();
            }
            return objSec.Decrypt(passkey, value);
        }

        #endregion

    }
}
