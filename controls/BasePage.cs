using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Common;
using NBrightCore.common;
using NBrightCore.render;

namespace NBrightDNN.controls
{
    public class BasePage : DotNetNuke.Entities.Modules.PortalModuleBase
    {
        public DataCtrlInterface ObjCtrl { get; set; }

        protected const string EncrytionKey = "";
        private bool _activatePaging = false;

        protected NBrightCore.controls.PagingCtrl CtrlPaging;
        protected Repeater CtrlSearch;
        protected Repeater CtrlList;
		protected LiteralControl CtrlListMsg;

        protected UserDataInfo UInfo;

        protected NBrightCore.TemplateEngine.TemplateGetter TemplCtrl;

        private string _entityTypeCode;

        public string EntityTypeCode
        {
            get { return _entityTypeCode; }
            set
            {
                _entityTypeCode = value;
                if (String.IsNullOrEmpty(CtrlTypeCode))
                {
                    CtrlTypeCode = _entityTypeCode;
                }
            }
        }

        public string EntityLangauge { get; set; }
        public string CtrlTypeCode { get; set; }
        public string EntityTypeCodeLang { get; set; }
        public string ItemId { get; set; }
        public string ItemIdLang { get; set; }
        public String ControlAdminPath { get; set; }
		public String ControlAdminIncludePath { get; set; }
		public List<NBrightInfo> OverRideInfoList { get; set; }

		//// Debug code for cache improvement timing: REMOVE FOR BUILD
		//public String NBrightLogTrace = "";
		//public long NBrightLogStartTick;
		//public long NBrightLogEndTick;
		//public long NBrightLogElapsedTick;
		//// Debug code for cache improvement timing: REMOVE FOR BUILD

        public bool FileHasBeenUploaded = false;


        #region "Page Events"

        protected override void OnInit(EventArgs e)
        {
			//// Debug code for cache improvement timing: REMOVE FOR BUILD
			//NBrightLogTrace = NBrightCore.common.Utils.ReadFile(PortalSettings.HomeDirectoryMapPath + "\\NBrightLogTrace.txt");
			//NBrightLogStartTick = DateTime.Now.Ticks;
			//// Debug code for cache improvement timing: REMOVE FOR BUILD

            base.OnInit(e);

            // Attach events
            GenXmlFunctions.FileHasBeenUploaded += new UploadFileCompleted(OnFileUploaded);

            OverRideInfoList = null;

            if (String.IsNullOrEmpty(ControlAdminPath)) ControlAdminPath = ControlPath;
                
            UInfo = new UserDataInfo(PortalId, ModuleId, ObjCtrl,CtrlTypeCode);

            EntityLangauge = Utils.RequestQueryStringParam(Context, "lang");
            if (EntityLangauge == "") EntityLangauge = Utils.GetCurrentCulture();
            //make sure we have a valid culture code in upper and lower case. (url re-writers can make all url lowercase)
            EntityLangauge = EntityLangauge.Substring(0, 2).ToLower() + "-" + EntityLangauge.Substring(3, 2).ToUpper();

            ItemId = Utils.RequestQueryStringParam(Context, "itemid");

            ItemIdLang = Utils.RequestQueryStringParam(Context, "itemidlang");
            if (ItemIdLang == "") ItemIdLang = "0";

            CtrlSearch = new Repeater();
            this.Controls.Add(CtrlSearch);

			CtrlListMsg = new LiteralControl();
			this.Controls.Add(CtrlListMsg);
			CtrlListMsg.Visible = false;

            CtrlList = new Repeater();
            this.Controls.Add(CtrlList);

            CtrlPaging = new NBrightCore.controls.PagingCtrl();
            this.Controls.Add(CtrlPaging);

            CtrlList.ItemCommand += new RepeaterCommandEventHandler(CtrlListItemCommand);
            CtrlSearch.ItemCommand += new RepeaterCommandEventHandler(CtrlSearchItemCommand);
            CtrlPaging.PageChanged += new RepeaterCommandEventHandler(PagingClick);

            TemplCtrl = new NBrightCore.TemplateEngine.TemplateGetter(MapPath("/DesktopModules/" + base.ModuleConfiguration.DesktopModule.FolderName), MapPath(ControlAdminPath));
        }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

            //Get UserDataInfo
            if (Page.IsPostBack)
            {
                // on postback update userdatainfo with form input data.
                UpdateUserData();
            }
            else
            {
                PopulateSearchHeader(CtrlTypeCode); // Update the search header.
            }

			//// Debug code for cache improvement timing: REMOVE FOR BUILD
			//NBrightLogEndTick = DateTime.Now.Ticks;
			//NBrightLogElapsedTick = NBrightLogEndTick - NBrightLogStartTick;
			//NBrightLogTrace += NBrightLogElapsedTick.ToString() + " - Total Ticks " + base.ModuleId.ToString("") + "\r\n"; ;
			//NBrightCore.common.Utils.SaveFile(PortalSettings.HomeDirectoryMapPath + "\\NBrightLogTrace.txt", NBrightLogTrace);
			//// Debug code for cache improvement timing: REMOVE FOR BUILD

        }

        #endregion

        #region "Get data Methods"

        /* *********************  Object Gets ********************** */

        public NBrightInfo GetData(int intItemId)
        {
            return (NBrightInfo)((DataCtrlInterface)ObjCtrl).GetInfo(intItemId);
        }

        public NBrightInfo GetData()
        {
            var objInfo = new NBrightInfo();

            if (Utils.IsNumeric(ItemId))
            {
                return (NBrightInfo) ((DataCtrlInterface) ObjCtrl).GetInfo(Convert.ToInt32(ItemId));
            }
            return null;
        }

        public NBrightInfo GetDataLang()
        {
            var objInfo = new NBrightInfo();

            if (Utils.IsNumeric(ItemIdLang))
            {
                return (NBrightInfo) ((DataCtrlInterface) ObjCtrl).GetInfo(Convert.ToInt32(ItemIdLang));
            }
            return null;
        }

        public NBrightInfo GetData(string cultureCode, int moduleid = -1, string SelUserId = "")
        {
            return GetData(ItemId, cultureCode, moduleid, SelUserId);
        }

        public NBrightInfo GetData(string parentItemId, string cultureCode,int moduleid = -1,string SelUserId = "")
        {
            if (moduleid == -1) moduleid = ModuleId; // so satellite modules can pass correct moduleid.

			//test for XML lang for backward compatiblity.
			var strFilter = " and (Lang = '" + cultureCode + "' or ISNULL(Lang,'') = '' or [xmlData].value('(genxml/hidden/lang)[1]', 'nvarchar(10)') = '" + cultureCode + "' or [xmlData].value('(genxml/hidden/lang)[1]', 'nvarchar(10)') = '' ) ";
        	
			if (Utils.IsNumeric(parentItemId))
            {
                strFilter = " and parentitemid = " + parentItemId + strFilter;
            }
            if (SelUserId != "")
            {
                strFilter = " and userid = " + SelUserId + strFilter;
            }


            NBrightInfo rtnObj = null;
            var l = ((DataCtrlInterface) ObjCtrl).GetListInfo(PortalId, ModuleId, EntityTypeCodeLang, strFilter, "");
            foreach(var o in l)
            {
                if ((cultureCode == o.Lang) | (cultureCode == o.GetXmlProperty("genxml/hidden/lang"))) // test XML lang for backward compatibility
                {
                    rtnObj = o;
                    break;
                }
            }
            if (rtnObj == null)
            {                
                // create new object to return.
                rtnObj = new NBrightInfo();
                rtnObj.ItemID = -1;
                rtnObj.ModuleId = ModuleId;
                rtnObj.PortalId = PortalId;
                rtnObj.TypeCode = EntityTypeCodeLang;
            	rtnObj.Lang = cultureCode;
                if (Utils.IsNumeric(parentItemId))
                {
                    rtnObj.ParentItemId = Convert.ToInt32(parentItemId);
                }
                rtnObj.SetXmlProperty("genxml/hidden/lang", cultureCode);
            }
            return rtnObj;
        }

        /* *********************  list Gets ********************** */

        public List<NBrightInfo> GetList(Repeater rp1, int moduleRefId, string entityTypeCode, int returnLimit = 0, int pageNumber = 0, int pageSize = 0, string selUserId = "", bool debugMode = false)
        {
            return ((DataCtrlInterface)ObjCtrl).GetListWithLang(rp1, PortalId, moduleRefId, entityTypeCode, returnLimit, pageNumber, pageSize, EntityTypeCodeLang, selUserId,debugMode);
        }

        public List<NBrightInfo> GetList(Repeater rp1, string entityTypeCode, int returnLimit = 0, int pageNumber = 0, int pageSize = 0, string selUserId = "", bool debugMode = false)
        {
            return ((DataCtrlInterface)ObjCtrl).GetListWithLang(rp1, PortalId, ModuleId, entityTypeCode, returnLimit, pageNumber, pageSize, EntityTypeCodeLang, selUserId,debugMode);
        }

        public List<NBrightInfo> GetList(Repeater rp1, int returnLimit = 0, int pageNumber = 0, int pageSize = 0, string selUserId = "", bool debugMode = false)
        {
            return ((DataCtrlInterface)ObjCtrl).GetListWithLang(rp1, PortalId, ModuleId, EntityTypeCode, returnLimit, pageNumber, pageSize, EntityTypeCodeLang, selUserId,debugMode);
        }

        public List<NBrightInfo> GetList(string strFilters = "", string strOrderBy = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0, string selUserId = "", bool debugMode = false)
        {
            return GetList(PortalId, ModuleId, EntityTypeCode, EntityTypeCodeLang, strFilters, strOrderBy, returnLimit, pageNumber, pageSize, recordCount, selUserId,debugMode);
        }

        public List<NBrightInfo> GetList(string entityTypeCode, string entityTypeCodeLang, string strFilters = "", string strOrderBy = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0, string selUserId = "", bool debugMode = false)
        {
            return GetList(PortalId, ModuleId, entityTypeCode, entityTypeCodeLang, strFilters, strOrderBy, returnLimit, pageNumber, pageSize, recordCount, selUserId, debugMode);
        }

        public List<NBrightInfo> GetList(int portalId, int moduleId, string entityTypeCode, string entityTypeCodeLang, string strFilters = "", string strOrderBy = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0, string selUserId = "", bool debugMode = false)
        {
            return ((DataCtrlInterface)ObjCtrl).GetListWithLang(portalId, moduleId, entityTypeCode, strFilters, strOrderBy, returnLimit, pageNumber, pageSize, recordCount, entityTypeCodeLang, selUserId, debugMode);
        }

        #endregion

        #region "Delete Methods"

		public void DeleteAllEntityData(string entityTypeCode, string uploadFolder)
		{
			DeleteAllEntityData(base.PortalId, base.ModuleId, entityTypeCode, uploadFolder);
		}

        public void DeleteAllEntityData(int portalId, int moduleId, string entityTypeCode, string uploadFolder)
        {
            var l = GetList(portalId, moduleId, entityTypeCode,"","","",0,0,0,0,"",true);
            foreach (var obj in l)
            {
                DeleteLinkedFiles(obj.ItemID, uploadFolder);
                ((DataCtrlInterface)ObjCtrl).DeleteInfo(obj.ItemID);
            }
        }

        public void DeleteData(Repeater rp1, string uploadFolder = "")
        {
            var objInfo = new NBrightInfo();
            var itemId = GenXmlFunctions.GetHiddenField(rp1, "ItemID");

            if (itemId == "")
            { // No valid itemid on XML, take from current id.  
                itemId = ItemId;
            }

            if (Utils.IsNumeric(itemId))
            {
                DeleteData(Convert.ToInt32(itemId), uploadFolder);
            }
        }

        public void DeleteData(int itemID, string uploadFolder)
        {
            var objInfo = ((DataCtrlInterface)ObjCtrl).GetInfo(itemID);
            if (objInfo != null)
            {

                var strFilter = " and parentitemid = '" + itemID.ToString("") + "' ";

                // delete any child records linked to parent.
                var l = GetList(objInfo.PortalId,-1,"","",strFilter,"",0,0,0,0,"",true);
                foreach (var o in l)
                {
                    DeleteData(o.ItemID,uploadFolder);
                }

                DeleteLinkedFiles(itemID, uploadFolder);
                ((DataCtrlInterface)ObjCtrl).DeleteInfo(itemID);

            }

        }

        public void DeleteLinkedFiles(int itemId, string uploadFolder)
        {
            var obj = ObjCtrl.GetInfo(itemId);
            if (uploadFolder != "" & obj != null)
            {
                obj.XMLData = GenXmlFunctions.DeleteFile(obj.XMLData, PortalSettings.HomeDirectoryMapPath + uploadFolder);
                ObjCtrl.UpdateInfo(obj);
            }
        }

        #endregion

        #region "update methods"

        private object UpdateDetailData(Repeater rp1, string typeCode, string uploadFolderMapPath, string itemId = "0", string selUserId = "", string GUIDKey = "")
        {
            if (!Utils.IsNumeric(selUserId))
            {
                selUserId = UserId.ToString("");
            }

            var objInfo = new NBrightInfo();

            if (Utils.IsNumeric(GenXmlFunctions.GetHiddenField(rp1, "ItemID")))
            {
                itemId = GenXmlFunctions.GetHiddenField(rp1, "ItemID");
            }

            if (Utils.IsNumeric(itemId))
            {
                // read any existing data or create new.
                objInfo = ObjCtrl.GetInfo(Convert.ToInt32(itemId));
                if (objInfo == null)
                {
                    objInfo = new NBrightInfo();
                    // populate data
                    objInfo.PortalId = PortalId;
                    objInfo.ModuleId = ModuleId;
                    objInfo.ItemID = Convert.ToInt32(itemId);
                    objInfo.TypeCode = typeCode;
                    objInfo.UserId = Convert.ToInt32(selUserId);
                    objInfo.GUIDKey = GUIDKey;
                }

                // populate changed data
                GenXmlFunctions.SetHiddenField(rp1, "dteModifiedDate", Convert.ToString(DateTime.Now));
                objInfo.ModifiedDate = DateTime.Now;

                objInfo.UserId = Convert.ToInt32(selUserId);
                
                //rebuild xml
                objInfo.XMLData = GenXmlFunctions.GetGenXml(rp1, "", uploadFolderMapPath);

                //update GUIDKey 
                if (GUIDKey != "")
                {
                    objInfo.GUIDKey = GUIDKey;
                }

                objInfo.ItemID = ((DataCtrlInterface)ObjCtrl).UpdateInfo(objInfo);

            }
            return objInfo;
        }

        public object UpdateDetailNoValidate(Repeater rp1, string typeCode, string uploadFolderMapPath, string itemId = "0", string selUserId = "", string GUIDKey = "")
        {
            var objInfo = (NBrightInfo)UpdateDetailData(rp1, typeCode, uploadFolderMapPath, itemId, selUserId,GUIDKey);
            if (Convert.ToString(objInfo.ItemID) != ItemId)
            {
                ItemId = Convert.ToString(objInfo.ItemID); // make sure base class has correct ID                    
                GenXmlFunctions.SetHiddenField(rp1, "itemid", ItemId);
                //rebuild xml
                objInfo.XMLData = GenXmlFunctions.GetGenXml(rp1);
                objInfo.ItemID = ((DataCtrlInterface)ObjCtrl).UpdateInfo(objInfo);
            }
            return objInfo;
        }

        public object UpdateDetailLangNoValidate(Repeater rp1, string typeCode, string uploadFolderMapPath, string itemIdLang = "0", string selUserId = "",string GUIDKey = "")
        {
            var objInfo = (NBrightInfo)UpdateDetailData(rp1, typeCode, uploadFolderMapPath, itemIdLang, selUserId, GUIDKey);
			if ((Convert.ToString(objInfo.ItemID) != ItemIdLang) | (String.IsNullOrEmpty(objInfo.Lang))) //update lang field if enpty.
            {
                ItemIdLang = Convert.ToString(objInfo.ItemID); // make sure base class has correct ID                    
                GenXmlFunctions.SetHiddenField(rp1, "itemid", ItemIdLang);
                GenXmlFunctions.SetHiddenField(rp1, "lang", EntityLangauge);
                //rebuild xml
                objInfo.XMLData = GenXmlFunctions.GetGenXml(rp1);
                objInfo.ParentItemId = Convert.ToInt32(ItemId);
            	objInfo.Lang = EntityLangauge;
                objInfo.ItemID = ((DataCtrlInterface)ObjCtrl).UpdateInfo(objInfo);
            }
            return objInfo;
        }

        public int UpdateData(NBrightInfo objInfo)
        {
            return ((DataCtrlInterface)ObjCtrl).UpdateInfo(objInfo);
        }

        public int AddBlankEntity(string DatabaseTypeCode)
        {
            var objInfo = new NBrightInfo { ItemID = -1, PortalId = PortalId, ModuleId = ModuleId, TypeCode = DatabaseTypeCode, ModifiedDate = DateTime.Now };
            return ((DataCtrlInterface)ObjCtrl).UpdateInfo(objInfo);
        }

        public void CopyDataLang(string fromCultureCode, string toCultureCode, string uploadFolder = "")
        {
            if ((fromCultureCode != "" & toCultureCode != "") & (fromCultureCode != toCultureCode))
            {
                var newItemID = -1;
                var objToLang = GetData(toCultureCode);
                if (objToLang != null)
                {
                    newItemID = objToLang.ItemID;
                }

                var objFromLang = GetData(fromCultureCode);
                if (objFromLang != null)
                {
                    objFromLang.ItemID = newItemID;
                    objFromLang.SetXmlProperty("genxml/hidden/lang", toCultureCode);
                    objFromLang.SetXmlProperty("genxml/hidden/itemid", objFromLang.ItemID.ToString(""));
                	objFromLang.Lang = toCultureCode;
					if (newItemID >= 0)
                    {
                        DeleteData(newItemID, uploadFolder);
                    }
                    else
                    {
                        newItemID = UpdateData(objFromLang);
                        objFromLang.SetXmlProperty("genxml/hidden/itemid", newItemID.ToString(""));
                        objFromLang.ItemID = newItemID;
                    }
                    UpdateData(objFromLang);
                }
            }
        }

        #endregion

        #region "userData search methods"

        public void SetSearchUserDataInfoVar()
        {
            var strFilters = GenXmlFunctions.GetSqlSearchFilters(CtrlSearch);
            var strOrderBy = GenXmlFunctions.GetSqlOrderBy(CtrlSearch);

            if (GenXmlFunctions.GetHiddenField(CtrlSearch, "lang") != "")
            {
                strFilters += " and ([xmlData].value('(genxml/hidden/lang)[1]', 'nvarchar(10)') = '" + Utils.GetCurrentCulture() + "' or ISNULL([xmlData].value('(genxml/hidden/lang)[1]', 'nvarchar(10)'),'') = '') ";
            }

            UInfo.SearchFilters = strFilters;
            UInfo.SearchOrderby = strOrderBy;
            UInfo.SearchReturnLimit = GenXmlFunctions.GetHiddenField(CtrlSearch, "searchreturnlimit");
            if (UInfo.SearchPageNumber == "") UInfo.SearchPageNumber = "1";
            UInfo.SearchPageSize = GenXmlFunctions.GetHiddenField(CtrlSearch, "searchpagesize");
            var strSearchModuleId  = GenXmlFunctions.GetHiddenField(CtrlSearch, "searchmoduleid");
            if (Utils.IsNumeric(strSearchModuleId)) UInfo.SearchModuleId = Convert.ToInt32(strSearchModuleId);
            UInfo.SearchClearAfter = "0";

            UpdateUserData();

        }

        public void UpdateUserData()
        {
            UInfo.UserId =  UserInfo.UserID;
            UInfo.TabId = TabId;
            UInfo.SkinSrc = Globals.QueryStringEncode(Utils.RequestQueryStringParam(Context, "SkinSrc"));
            UInfo.EntityTypeCode = EntityTypeCode;
            UInfo.CtrlTypeCode = CtrlTypeCode;

            // set these returns independantly, to allow return to previous pages & ajax called over paging. 
            //UInfo.RtnSelUrl = EditUrl("itemid", ItemId, CtrlTypeCode);
            //UInfo.RtnUrl = EditUrl("itemid", ItemId, CtrlTypeCode);
            //UInfo.FromItemId = ItemId;
            
            if (CtrlSearch.Visible)
            {
                UInfo.SearchGenXml = GenXmlFunctions.GetGenXml(CtrlSearch);                
            }
            UInfo.Save();

        }


        public void PopulateSearchHeader(string typeCode)
        {
            if (CtrlSearch != null && CtrlSearch.Visible & !Page.IsPostBack)
            {
                var obj = new NBrightInfo();

                obj.ItemID = -1;
                obj.GUIDKey = "";
                obj.ModifiedDate = DateTime.Now;
                obj.TypeCode = typeCode;
                obj.XMLData = UInfo.SearchGenXml;   
                var l = new List<NBrightInfo> { obj };
                CtrlSearch.DataSource = l;
                CtrlSearch.DataBind();
            }
        }


        #endregion

        #region "Display Methods"

        public void DoList(int moduleRefId, Repeater rp1, int returnLimit = 0, int pageNumber = 0, int pageSize = 0, string selUserId = "", bool debugMode = false)
        {
            rp1.DataSource = GetList(rp1, moduleRefId, EntityTypeCode, returnLimit, pageNumber, pageSize, selUserId,debugMode);
            rp1.DataBind();
        }

        public void DoList(Repeater rp1, int returnLimit = 0, int pageNumber = 0, int pageSize = 0, string selUserId = "", bool debugMode = false)
        {
            rp1.DataSource = GetList(rp1, returnLimit, pageNumber, pageSize, selUserId, debugMode);
            rp1.DataBind();
        }

        public void DoList(Repeater rp1, string typeCode, int returnLimit = 0, int pageNumber = 0, int pageSize = 0, string selUserId = "", bool debugMode = false)
        {
            rp1.DataSource = GetList(rp1, typeCode, returnLimit, pageNumber, pageSize, selUserId, debugMode);
            rp1.DataBind();
        }

        public void DoDetail(Repeater rp1, NBrightInfo obj)
        {
            var l = new List<object> {obj};
            rp1.DataSource = l;
            rp1.DataBind();
        }

        /// <summary>
        /// Return NBrightInfo with Extra Langauge Data in the XML of the parent item.
        /// </summary>
        /// <param name="rp1"></param>
        /// <param name="cultureCode"></param>
        public void DoDetailWithLang(Repeater rp1, string cultureCode, string selUserId = "")
        {
            DoDetailWithLang(rp1, cultureCode, ModuleId, selUserId);
        }

        public void DoDetailWithLang(Repeater rp1, string cultureCode, int ModuleRefId, string selUserId = "")
        {
            int result;
            if (Utils.IsNumeric(ItemId))
            {
                NBrightInfo obj = null;
                obj = ((DataCtrlInterface)ObjCtrl).GetDataWithLang(PortalId, ModuleRefId, ItemId, cultureCode, EntityTypeCodeLang, selUserId);
                if (obj != null)
                {
                    EntityLangauge = cultureCode;
                }
                var l = new List<object> { obj };
                rp1.DataSource = l;
                rp1.DataBind();
            }

        }

        /// <summary>
        /// Return the NBrightInfo item of the culture.
        /// </summary>
        /// <param name="rp1"></param>
        /// <param name="cultureCode"></param>
        public void DoDetail(Repeater rp1, string cultureCode, int moduleid = -1, string selUserId = "")
        {
            int result;
            if (Utils.IsNumeric(ItemId))
            {
                NBrightInfo obj = null;
                obj = GetData(ItemId, cultureCode, moduleid, selUserId);               
                if (obj != null)
                {
                    EntityLangauge = cultureCode;
                }
                var l = new List<object> {obj};
                rp1.DataSource = l;
                rp1.DataBind();
            }

        }

        public void DoDetail(Repeater rp1)
        {
            int result;
            if (Utils.IsNumeric(ItemId))
            {
                NBrightInfo obj = null;
                obj = ((DataCtrlInterface) ObjCtrl).GetInfo(Convert.ToInt32(ItemId));
                var l = new List<object> {obj};
                rp1.DataSource = l;
                rp1.DataBind();
            }

        }

        public void DoDisplay(Repeater rp1)
        {
            var obj = new NBrightInfo();
            obj.ModuleId = ModuleId;
            obj.PortalId = PortalId;
            obj.XMLData = "<genxml></genxml>";
            var l = new List<object> { obj };
            rp1.DataSource = l;
            rp1.DataBind();            
        }

        public List<NBrightInfo> GetListByUserDataInfoVar(string typeCode,string webserviceurl = "")
        {
            try
            {
                var weblist = new List<NBrightInfo>();
                var recordCount = 0;

                if (EntityTypeCode == "" & webserviceurl != "")
                {
                    // No EntityType, therefore data must be selected from WebService.
                    var l = new List<NBrightInfo>();
                    var xmlDoc = new XmlDataDocument();
                    
                    // pass the userdatainfo into the header request (saves using or creating a post field or adding to url)
                    var objInfo = ObjCtrl.GetInfo(UInfo.ItemId);
                    var userdatainfo = "";
                    if (objInfo != null)
                    {
                        if (objInfo.TypeCode == "USERDATAINFO")
                        {
                            //userdatainfo = DotNetNuke.Common.Globals.HTTPPOSTEncode(objInfo.XMLData);
                            userdatainfo = objInfo.XMLData;
                        }
                    }

                    string strResp = DnnUtils.GetDataResponseAsString(webserviceurl, "userdatainfo", userdatainfo);
                    try
                    {
                        xmlDoc.LoadXml(strResp);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    var rc = xmlDoc.SelectSingleNode("root/recordcount");
                    if (rc != null && Utils.IsNumeric(rc.InnerText))
                    {
                        recordCount = Convert.ToInt32(rc.InnerText);
                    }

                    var xmlNodeList = xmlDoc.SelectNodes("root/item");
                    if (xmlNodeList != null)
                    {
                        foreach (XmlNode xmlNod in xmlNodeList)
                        {
                            var obj = new NBrightInfo();
                            obj.FromXmlItem(xmlNod.OuterXml);
                            l.Add(obj);
                        }
                    }
                    weblist = l;
					if (recordCount == 0) recordCount = weblist.Count;                    	
                }
                else
                {
                    if (OverRideInfoList != null)
                    {
                        recordCount = OverRideInfoList.Count;
                    }
                    else
                    {
                        recordCount = ObjCtrl.GetListInfoCount(UInfo.SearchPortalId, UInfo.SearchModuleId, EntityTypeCode, UInfo.SearchFilters, EntityTypeCodeLang);
                    }
                }


                if (!Utils.IsNumeric(UInfo.SearchPageNumber))
                {
                    UInfo.SearchPageNumber = "1";
                    UInfo.Save();
                }

                if (!Utils.IsNumeric(UInfo.SearchPageSize) | !Utils.IsNumeric(UInfo.SearchReturnLimit))
                {
                    UInfo.SearchPageSize = GenXmlFunctions.GetHiddenField(CtrlSearch, "searchpagesize");
                    UInfo.SearchReturnLimit = GenXmlFunctions.GetHiddenField(CtrlSearch, "searchreturnlimit");
                    if (!Utils.IsNumeric(UInfo.SearchPageSize)) UInfo.SearchPageSize = "25";
                    if (!Utils.IsNumeric(UInfo.SearchReturnLimit)) UInfo.SearchReturnLimit = "0";
                    UInfo.Save(); 
                }


                if (_activatePaging)
                {
                    CtrlPaging.PageSize = Convert.ToInt32(UInfo.SearchPageSize);
                    CtrlPaging.TotalRecords = recordCount;
                    CtrlPaging.CurrentPage = Convert.ToInt32(UInfo.SearchPageNumber);
                    CtrlPaging.BindPageLinks();
                }
                else
                {
                    CtrlPaging.Visible = false;
                }

                if (UInfo.SearchClearAfter == "1")
                {
                    UInfo.ClearSearchData();
                }

                if (OverRideInfoList != null)
                {
                    //overiding list passed from control, so use linq to do the paging, select  
                    var records = (from o in OverRideInfoList select o);

                    var pgNo = Convert.ToInt32(UInfo.SearchPageNumber);
                    var pgRec = Convert.ToInt32(UInfo.SearchPageSize);

                    var rtnRecords = records.Skip((pgNo - 1) * pgRec).Take(pgRec).ToList();

                    return rtnRecords;
                }

				if (EntityTypeCode == "" & webserviceurl != "")
				{
					// use website (Might be empty).
					return weblist;
				}
				else
				{
					return GetList(UInfo.SearchPortalId, UInfo.SearchModuleId, EntityTypeCode, EntityTypeCodeLang, UInfo.SearchFilters, UInfo.SearchOrderby, Convert.ToInt32(UInfo.SearchReturnLimit), Convert.ToInt32(UInfo.SearchPageNumber), Convert.ToInt32(UInfo.SearchPageSize), recordCount);
				}
            }
            catch (Exception)
            {
                //clear data incase error in userdata
                UInfo.ClearSearchData();
                throw;
            }
        }

        #endregion

        #region "methods"

        public void OnInitActivateList(string listheaderTemplate, string listbodyTemplate, string listfooterTemplate, string searchTemplate = "", bool withPaging = true)
        {

            if (searchTemplate != "")
            {
                searchTemplate = ReplaceBasicTokens(searchTemplate);
                CtrlSearch.ItemTemplate = new GenXmlTemplate(searchTemplate);
            }
            else
            {
                CtrlSearch.Visible = false;
            }


            //set default filter
            if (UInfo.SearchClearAfter == "")
            {
                UInfo.SearchFilters = "";
            }

            if (UInfo.SearchOrderby == "")

            {
                if (searchTemplate != "")
                {
                    UInfo.SearchOrderby = GenXmlFunctions.GetSqlOrderBy(searchTemplate);
                }
                if (UInfo.SearchOrderby == "")
                {
                    UInfo.SearchOrderby = GenXmlFunctions.GetSqlOrderBy(listheaderTemplate);
                }
            }

            UInfo.Save();

            _activatePaging = withPaging;
            CtrlList.HeaderTemplate = new GenXmlTemplate(listheaderTemplate);
            var templ = new GenXmlTemplate(listbodyTemplate);
            templ.SortItemId = UInfo.SortItemId;
            CtrlList.ItemTemplate = templ;
            CtrlList.FooterTemplate = new GenXmlTemplate(listfooterTemplate);

        }

        public void OnInitActivateList(bool withSearch = true, bool withPaging = true, string templatePath = "")
        {
            var strListSearch = "";
            var strListHeader = "";
            var strListBody = "";
            var strListFooter = "";

            if (withSearch)
            {
                strListSearch = TemplCtrl.GetTemplateData(CtrlTypeCode + "_Search.html", Utils.GetCurrentCulture());
                strListSearch = ReplaceBasicTokens(strListSearch);
            }

            _activatePaging = withPaging;
			strListHeader = TemplCtrl.GetTemplateData(CtrlTypeCode + "_ListH.html", Utils.GetCurrentCulture());
			strListBody = TemplCtrl.GetTemplateData(CtrlTypeCode + "_List.html", Utils.GetCurrentCulture());
			strListFooter = TemplCtrl.GetTemplateData(CtrlTypeCode + "_ListF.html", Utils.GetCurrentCulture());

            //set default filter
            if (UInfo.SearchClearAfter == "")
            {
                UInfo.SearchFilters = "";
            }

            if (UInfo.SearchOrderby == "")
            {
                if (withSearch && strListSearch != "")
                {
                    UInfo.SearchOrderby = GenXmlFunctions.GetSqlOrderBy(strListSearch);
                }
                if (UInfo.SearchOrderby == "")
                {
                    UInfo.SearchOrderby = GenXmlFunctions.GetSqlOrderBy(strListHeader);
                }
            }

            OnInitActivateList(strListHeader, strListBody, strListFooter, strListSearch, withPaging);

        }

        public string ReplaceBasicTokens(string templateText,NBrightInfo nbSettings = null)
        {
            var strOut = templateText;
            strOut = strOut.Replace("[TokenSearch:searchDate1]", UInfo.SearchDate1);
            strOut = strOut.Replace("[TokenSearch:searchDate2]", UInfo.SearchDate2);
            strOut = strOut.Replace("[TokenSearch:searchExtra1]", UInfo.SearchExtra1);
            strOut = strOut.Replace("[TokenSearch:searchExtra2]", UInfo.SearchExtra2);
            strOut = strOut.Replace("[Token:langauge]", Utils.GetCurrentCulture());

            if (nbSettings != null)
            {
                strOut = strOut.Replace("[Token:modulekey]", nbSettings.GUIDKey);                
            }

            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(UInfo))
            {
                var value = prop.GetValue(UInfo);
                if (value == null) value = ""; 
                strOut = strOut.Replace("[UInfo:" + prop.Name + "]", value.ToString());
            }

            return strOut;
        }

        public void BindListData(string webServiceUrl = "")
        {

            EventBeforeBindListData(CtrlList, webServiceUrl);

            var l = GetListByUserDataInfoVar(CtrlTypeCode, webServiceUrl);

			if (l == null || l.Count == 0)
			{
				CtrlList.Visible = false;
				CtrlListMsg.Text = DotNetNuke.Services.Localization.Localization.GetString("noresult", base.LocalResourceFile);
				CtrlListMsg.Visible = true;
			}
			else
			{
				CtrlList.Visible = true;
				CtrlList.DataSource = l;
				CtrlList.DataBind();
			}

            EventAfterBindListData(CtrlList,webServiceUrl);
        }

        public void BindData(Repeater rpData,string webServiceUrl = "")
        {
            EventBeforeBindData(rpData, webServiceUrl);

            var l = GetListByUserDataInfoVar(CtrlTypeCode, webServiceUrl);

            rpData.DataSource = l;
            rpData.DataBind();

            EventAfterBindData(rpData, webServiceUrl);
        }

        #endregion


        #region "events"

        protected virtual void PagingClick(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            if (Utils.IsNumeric(cArg))
            {
                UInfo.SearchPageNumber = cArg;
                UInfo.Save();
            }
            EventBeforePageChange(source, e);
        }

        public virtual void EventBeforeBindData(Repeater rpCtrlList, string webServiceUrl = "")
        {

        }

        public virtual void EventAfterBindData(Repeater rpCtrlList, string webServiceUrl = "")
        {

        }


        public virtual void EventBeforeBindListData(Repeater rpData, string webServiceUrl = "")
        {

        }

        public virtual void EventAfterBindListData(Repeater rpData, string webServiceUrl = "")
        {

        }


        public virtual void EventBeforePageChange(object source, RepeaterCommandEventArgs e)
        {

        }

        protected virtual void CtrlListItemCommand(object source, RepeaterCommandEventArgs e)
        {

        }

        public virtual void EventListItemCommand(object source, RepeaterCommandEventArgs e)
        {

        }

        protected virtual void CtrlSearchItemCommand(object source, RepeaterCommandEventArgs e)
        {

        }

        public virtual void EventSearchItemCommand(object source, RepeaterCommandEventArgs e)
        {

        }

        public void OnFileUploaded()
        {
            FileHasBeenUploaded = true;
        }

        #endregion

    }
}
