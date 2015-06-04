﻿using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN.SqlDataProvider;


namespace NBrightDNN
{

    public class NBrightDataController : NBrightDataCtrlInterface
	{

        #region "NBrightBuy override DB Public Methods"

        /// <summary>
        /// override for Database Function
        /// </summary>
        /// <param name="itemId"></param>
        public override void Delete(int itemId)
        {
            DataProvider.Instance().Delete(itemId);
        }

        /// <summary>
        /// override for Database Function
        /// </summary>
        public override void CleanData()
        {
            DataProvider.Instance().CleanData();
        }

        /// <summary>
        /// override for Database Function.  Gets record, if lang is specified then lang xml in injected into the  base genxml node.
        /// </summary>
        /// <param name="itemId">itmeid of base genxml</param>
        /// <param name="lang">Culturecode of data to be injected into base genxml</param>
        /// <returns></returns>
        public override NBrightInfo Get(int itemId, string lang = "")
        {
            return CBO.FillObject<NBrightInfo>(DataProvider.Instance().Get(itemId, lang));
        }

        public override NBrightInfo GetData(int itemId)
        {
            return CBO.FillObject<NBrightInfo>(DataProvider.Instance().GetData(itemId));
        }


        /// <summary>
        /// override for Database Function
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="moduleId"></param>
        /// <param name="typeCode"></param>
        /// <param name="sqlSearchFilter"></param>
        /// <param name="sqlOrderBy"></param>
        /// <param name="returnLimit"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="recordCount"></param>
        /// <param name="typeCodeLang"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public override List<NBrightInfo> GetList(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string sqlOrderBy = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0, string lang = "")
        {
            return CBO.FillCollection<NBrightInfo>(DataProvider.Instance().GetList(portalId, moduleId, typeCode, sqlSearchFilter, sqlOrderBy, returnLimit, pageNumber, pageSize, recordCount, lang));
        }

	    /// <summary>
	    /// override for Database Function
	    /// </summary>
	    /// <param name="portalId"></param>
	    /// <param name="moduleId"></param>
	    /// <param name="typeCode"></param>
	    /// <param name="sqlSearchFilter"></param>
	    /// <param name="typeCodeLang"></param>
	    /// <param name="lang"></param>
	    /// <returns></returns>
	    public override int GetListCount(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string lang = "")
        {
            return DataProvider.Instance().GetListCount(portalId, moduleId, typeCode, sqlSearchFilter, lang);
        }

        /// <summary>
        /// override for Database Function
        /// </summary>
        /// <param name="objInfo"></param>
        /// <returns></returns>
        public override int Update(NBrightInfo objInfo)
        {
            objInfo.ModifiedDate = DateTime.Now;
            return DataProvider.Instance().Update(objInfo.ItemID, objInfo.PortalId, objInfo.ModuleId, objInfo.TypeCode, objInfo.XMLData, objInfo.GUIDKey, objInfo.ModifiedDate, objInfo.TextData, objInfo.XrefItemId, objInfo.ParentItemId, objInfo.UserId, objInfo.Lang);
        }

        /// <summary>
        /// Gte a single record from the Database using the EntityTypeCode.  This is usually used to fetch settings data "SETTINGS", where only 1 record will exist for the module.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="moduleId"></param>
        /// <param name="entityTypeCode"></param>
        /// <param name="selUserId"></param>
        /// <param name="entityTypeCodeLang"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public NBrightInfo GetByType(int portalId, int moduleId, string entityTypeCode, string selUserId = "", string lang = "")
        {
            var strFilter = "";
            if (selUserId != "")
            {
                strFilter += " and NB1.UserId = " + selUserId + " ";
            }

            var l = CBO.FillCollection<NBrightInfo>(DataProvider.Instance().GetList(portalId, moduleId, entityTypeCode, strFilter, "", 1, 1, 1, 1, lang));
            if (l.Count >= 1)
            {
                return l[0];
            }
            return null;
        }

        /// <summary>
        /// Get a single record back from the database, using the guyidkey (The seluserid is used to confirm the correct user.)
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="moduleId"></param>
        /// <param name="entityTypeCode"></param>
        /// <param name="guidKey"></param>
        /// <param name="selUserId"></param>
        /// <returns></returns>
        public NBrightInfo GetByGuidKey(int portalId, int moduleId, string entityTypeCode, string guidKey, string selUserId = "")
        {
            var strFilter = " and NB1.GUIDKey = '" + guidKey + "' ";
            if (selUserId != "")
            {
                strFilter += " and NB1.UserId = " + selUserId + " ";
            }

            var l = GetList(portalId, moduleId, entityTypeCode, strFilter, "", 1);
            if (l.Count == 0) return null;
            if (l.Count > 1)
            {
                for (int i = 1; i < l.Count; i++)
                {
                    // remove invalid DB entries
                    Delete(l[i].ItemID);
                }
            }
            return l[0];
        }


        public NBrightInfo GetData(int itemId, string typeCodeLang, string lang = "",bool debugMode = false)
        {
            if (lang == "") lang = Utils.GetCurrentCulture();
            // get cache data
            var strCacheKey = itemId.ToString("") + "*" + typeCodeLang + "*" + "*" + lang;
            NBrightInfo rtnInfo = null;
            if (debugMode == false)
            {
                var obj = Utils.GetCache(strCacheKey);
                if (obj != null) rtnInfo = (NBrightInfo)obj;
            }

            if (rtnInfo == null)
            {
                rtnInfo = CBO.FillObject<NBrightInfo>(DataProvider.Instance().Get(itemId, lang)); 
                if (debugMode == false) Utils.SetCache(strCacheKey, rtnInfo);
            }
            return rtnInfo;
        }

        public NBrightInfo GetDataLang(int parentitemId, string lang = "", bool debugMode = false)
        {
            if (lang == "") lang = Utils.GetCurrentCulture();
            // get cache data
            var strCacheKey = "datalang*" + parentitemId.ToString("") + "*" + lang;
            NBrightInfo rtnInfo = null;
            if (debugMode == false)
            {
                var obj = Utils.GetCache(strCacheKey);
                if (obj != null) rtnInfo = (NBrightInfo)obj;
            }

            if (rtnInfo == null)
            {
                rtnInfo = CBO.FillObject<NBrightInfo>(DataProvider.Instance().GetDataLang(parentitemId, lang));
                if (debugMode == false) Utils.SetCache(strCacheKey, rtnInfo);
            }
            return rtnInfo;
        }


        /* *********************  list Data Gets ********************** */

	    /// <summary>
	    /// Get data list count with caching
	    /// </summary>
	    /// <param name="portalId"></param>
	    /// <param name="moduleId"></param>
	    /// <param name="typeCode"></param>
	    /// <param name="sqlSearchFilter"></param>
	    /// <param name="typeCodeLang"></param>
	    /// <param name="lang"></param>
	    /// <param name="debugMode"></param>
	    /// <param name="visibleOnly"> </param>
	    /// <returns></returns>
	    public int GetDataListCount(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string lang = "", Boolean debugMode = false)
        {
            // get cache data
            var strCacheKey = portalId.ToString("") + "*" + moduleId.ToString("") + "*" + typeCode + "*" + "*filter:" + sqlSearchFilter.Replace(" ", "") + "*" + lang;
            var rtncount = -1;
            if (debugMode == false)
            {
                var obj = Utils.GetCache(strCacheKey);
                if (obj != null) rtncount = (int)obj;
            }

            if (rtncount == -1)
            {
                rtncount = DataProvider.Instance().GetListCount(portalId, moduleId, typeCode, sqlSearchFilter, lang);
                if (debugMode == false) Utils.SetCache(strCacheKey, rtncount);
            }
            return rtncount;
        }

        /// <summary>
        /// Data Get, used to call the Database provider and applies caching. Plus the option of taking filter and order information from the meta fields of the repeater template 
        /// </summary>
        /// <param name="rp1"></param>
        /// <param name="portalId"></param>
        /// <param name="moduleId"></param>
        /// <param name="entityTypeCode"></param>
        /// <param name="entityTypeCodeLang"></param>
        /// <param name="cultureCode"></param>
        /// <param name="debugMode"></param>
        /// <param name="selUserId"></param>
        /// <param name="returnLimit"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public List<NBrightInfo> GetDataList(Repeater rp1, int portalId, int moduleId, string entityTypeCode,  string cultureCode, bool debugMode = false, string selUserId = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0)
        {
            var strFilters = GenXmlFunctions.GetSqlSearchFilters(rp1);
            var strOrderBy = GenXmlFunctions.GetSqlOrderBy(rp1);
            //Default orderby if not set
            if (String.IsNullOrEmpty(strOrderBy)) strOrderBy = " Order by NB1.ModifiedDate DESC ";
            return GetDataList(portalId, moduleId, entityTypeCode, Utils.GetCurrentCulture(), strFilters, strOrderBy, debugMode, selUserId, returnLimit, pageNumber, pageSize, recordCount);
        }


	    /// <summary>
	    /// Data Get, used to call the Database provider and applies caching. Plus the option of adding user to the filter.
	    /// </summary>
	    /// <param name="portalId"></param>
	    /// <param name="moduleId"></param>
	    /// <param name="entityTypeCode"></param>
	    /// <param name="entityTypeCodeLang"></param>
	    /// <param name="cultureCode"></param>
	    /// <param name="strFilters"></param>
	    /// <param name="strOrderBy"></param>
	    /// <param name="debugMode"></param>
	    /// <param name="selUserId"></param>
	    /// <param name="returnLimit"></param>
	    /// <param name="pageNumber"></param>
	    /// <param name="pageSize"></param>
	    /// <param name="recordCount"></param>
	    /// <returns></returns>
	    public List<NBrightInfo> GetDataList(int portalId, int moduleId, string entityTypeCode, string cultureCode, string strFilters, string strOrderBy, bool debugMode = false, string selUserId = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0)
        {
            if (selUserId != "")
            {
                strFilters += " and UserId = " + selUserId + " ";
            }

            List<NBrightInfo> l = null;

            // get cache template 
            var strCacheKey = portalId.ToString("") + "*" + moduleId.ToString("") + "*" + entityTypeCode + "*" + "*filter:" + strFilters.Replace(" ", "") + "*orderby:" + strOrderBy.Replace(" ", "") + "*" + returnLimit.ToString("") + "*" + pageNumber.ToString("") + "*" + pageSize.ToString("") + "*" + recordCount.ToString("") + "*" + Utils.GetCurrentCulture();
            if (debugMode == false)
            {
                l = (List<NBrightInfo>)Utils.GetCache(strCacheKey);
            }

            if (l == null)
            {
                l = GetList(portalId, moduleId, entityTypeCode, strFilters, strOrderBy, returnLimit, pageNumber, pageSize, recordCount, cultureCode);
                //add rowcount, so we can use databind RowCount in the templates
                foreach (var i in l)
                {
                    i.RowCount = l.IndexOf(i) + 1;
                }
                if (debugMode == false) Utils.SetCache(strCacheKey, l);
            }
            return l;
        }

        #endregion


	}

}
