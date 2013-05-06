using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using NBrightCore.common;
using NBrightCore.render;

namespace NBrightDNN.controls
{
    public class BaseAdminPage : NBrightDNN.controls.BaseAdmin
    {



        #region "events"

        protected override void PagingClick(object source, RepeaterCommandEventArgs e)
        {
            base.PagingClick(source,e);
            Response.Redirect(EditUrl(CtrlTypeCode));
        }

        protected override void CtrlListItemCommand(object source, RepeaterCommandEventArgs e)
        {
            EventListItemCommand(source, e);

            var cArg = e.CommandArgument.ToString();
            switch (e.CommandName.ToLower())
            {
                case "selectitemid":
                    UInfo.SelItemId = cArg;
                    UInfo.Save();
                    Response.Redirect(UInfo.RtnSelUrl);
                    break;
                case "edit":
                    Response.Redirect(EditUrl("itemid", cArg, CtrlTypeCode));
                    break;
                case "delete":
                    if (Utils.IsNumeric(cArg))
                    {
						var obj = ((DataCtrlInterface)ObjCtrl).GetInfo(Convert.ToInt32(cArg));
						if (obj.ModuleId == ModuleId | obj.ModuleId == -1) //only delete items linked with this module or portalwide (-1). 
						{
							((DataCtrlInterface)ObjCtrl).DeleteInfo(Convert.ToInt32(cArg));
						}
                    }
                    Response.Redirect(EditUrl(CtrlTypeCode));
                    break;
                case "search":
                    SetSearchUserDataInfoVar();
                    Response.Redirect(EditUrl(CtrlTypeCode));
                    break;
                case "sort":
                    UInfo.SortItemId = cArg;
                    UInfo.Save();
                    Response.Redirect(EditUrl(CtrlTypeCode));
                    break;
                case "sortselect":
                    SortEntityRecords(EntityTypeCode, UInfo.SortItemId, cArg);
                    UInfo.SortItemId = "";
                    UInfo.Save();
                    Response.Redirect(EditUrl(CtrlTypeCode));
                    break;
                case "copy":
                    CopyEntry(cArg);
                    Response.Redirect(EditUrl(CtrlTypeCode));
                    break;
                case "exit":
                    Response.Redirect(Globals.NavigateURL(PortalSettings.ActiveTab.TabID));
                    break;
            }

        }

        protected override void CtrlSearchItemCommand(object source, RepeaterCommandEventArgs e)
        {
            EventSearchItemCommand(source, e);

            var cArg = e.CommandArgument.ToString();
            switch (e.CommandName.ToLower())
            {
                case "search":
                    SetSearchUserDataInfoVar();
                    Response.Redirect(EditUrl(CtrlTypeCode));
                    break;
                case "resetsearch":
                    UInfo.ClearSearchData();
                    Response.Redirect(EditUrl(CtrlTypeCode));
                    break;
                case "new":
                    Response.Redirect(EditUrl("itemid", "0", CtrlTypeCode), true);
                    break;
                case "return":
                    UInfo.SelItemId = ""; // clear any proviously selected items
                    UInfo.SortItemId = "";
                    UInfo.Save();
                    Response.Redirect(UInfo.RtnUrl);
                    break;
                case "exit":
                    Response.Redirect(Globals.NavigateURL(PortalSettings.ActiveTab.TabID));
                    break;
            }

        }

        #endregion


        #region "update functions"


        private void SortEntityRecords(string EntityType, string FromId, string ToId)
        {
            if (Utils.IsNumeric(FromId) && Utils.IsNumeric(ToId))
            {
                var objInfo = base.GetData(Convert.ToInt32(FromId));
                var objInfoTo = base.GetData(Convert.ToInt32(ToId));
				if ((objInfo != null && objInfoTo != null) && (objInfo.ItemID != objInfoTo.ItemID))
                {
                    var strOrderBy = " Order by [xmlData].value('(genxml/hidden/recordsortorder)[1]', 'nvarchar(7)') ";
                    var strFilter = "";
                    var lp = 1;

                    var l = new List<NBrightInfo>();
                    if (base.OverRideInfoList == null)
                    {
                        l = base.GetList(PortalId, ModuleId, EntityType, "", "", strOrderBy);
                    }
                    else
                    {
                        // Make sure we only change the sort order without language xml attached.
                        l = base.GetList(base.OverRideInfoList[0].PortalId, -1, base.OverRideInfoList[0].TypeCode, "", "", strOrderBy);
                    }

                    // move items in list
                    var i1 = l.FindIndex(f => f.ItemID == objInfo.ItemID);
                    l.RemoveAt(i1);
                    var i2 = l.FindIndex(f => f.ItemID == objInfoTo.ItemID);
                    if (i1 > i2)
                    {
                        l.Insert(i2, objInfo);
                    }
                    else
                    {
                        l.Insert(i2 + 1, objInfo);                        
                    }

                    // resequence all records, could jump in at move point, but this covers all bases. (And with manual sort I don't imagine a lot of records.)
                    foreach (var obj in l)
                    {
                        obj.SetXmlProperty("genxml/hidden/recordsortorder", lp.ToString("0000000"));
                        base.UpdateData(obj);
                        lp = lp + 1;
                    }
                }

            }
        }

        private void CopyEntry(string CopyItemId)
        {
            if (Utils.IsNumeric(CopyItemId))
            {
                var objInfo = base.GetData(Convert.ToInt32(CopyItemId));
                if (objInfo!= null)
                {
                    objInfo.ItemID = -1;
                    base.UpdateData(objInfo);

                    var strFilters = " and parentitemid = '" + CopyItemId + "' ";
                    var l = base.GetList(PortalId,ModuleId,"%","", strFilters);
                    foreach(var o in l)
                    {
                        o.ItemID = -1;
                        base.UpdateData(o);
                    }

                }
            }
        }


        #endregion





    }
}
