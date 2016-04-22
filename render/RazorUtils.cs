﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightCore.render;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace NBrightDNN.render
{

    public static class RazorUtils
    {

        public static String RazorRender(Object info, String razorTempl, String templateKey, Boolean debugMode = false)
        {
            // do razor test
            var config = new TemplateServiceConfiguration();
            config.Debug = debugMode;
            config.BaseTemplateType = typeof (RazorEngineTokens<>);
            var service = RazorEngineService.Create(config);
            Engine.Razor = service;

            var result = Engine.Razor.RunCompile(razorTempl, templateKey, null, info);
            return result;
        }

        public static String RazorRender(List<Object> infoList, String razorTempl, String templateKey, Boolean debugMode = false)
        {
            // do razor test
            if (debugMode)
            {
                var config = new TemplateServiceConfiguration();
                config.Debug = true;
                config.BaseTemplateType = typeof(RazorEngineTokens<>);
                var service = RazorEngineService.Create(config);
                Engine.Razor = service;
            }

            var result = Engine.Razor.RunCompile(razorTempl, templateKey, null, infoList);
            return result;
        }

        public static String RenderPaging(int totalRecords, int currentPage = 1, int pageSize = 20, String wrapperClass = "NBrightPagingDiv", String normalclass = "NBrightNormalPg", String selectclass = "NBrightSelectPg", String nextclass = "NBrightNextPg", String previousclass = "NBrightPrevPg", String linkclass = "cmdPg",String cssNextSection = "NBrightNextSection",String cssNextPage = "NBrightNextPg",String cssLastPage = "NBrightLastPg",String cssPrevSection = "NBrightPrevSection",String cssPrevPage = "NBrightPrevPg",String cssFirstPage = "NBrightFirstPg")
        {
            var strOut = "";

            var lastPage = Convert.ToInt32(totalRecords / pageSize);
            if (totalRecords != (lastPage * pageSize)) lastPage = lastPage + 1;
            if (lastPage == 1) return ""; //if only one page, don;t process

            if (currentPage <= 0) currentPage = 1;
            const int pageLinksPerPage = 10;
            var rangebase = Convert.ToInt32((currentPage - 1) / pageLinksPerPage);
            var lowNum = (rangebase * pageLinksPerPage) + 1;
            var highNum = lowNum + (pageLinksPerPage - 1);
            if (highNum > Convert.ToInt32(lastPage)) highNum = Convert.ToInt32(lastPage);
            if (lowNum < 1) lowNum = 1;

            strOut += "<div class='" + wrapperClass + "'><ul>";


            #region "first section"

            if ((lowNum != 1) && (currentPage > 1))
            {
                strOut += "<li class='" + cssFirstPage + "'><a pagenumber='1' class='" + linkclass + "'>&lt;&lt;</a></li>";
            }

            if (currentPage > 1)
            {
                strOut += "<li class='" + cssPrevPage + "'><a pagenumber='" + (currentPage - 1) + "' class='" + linkclass + "'>&lt;</a></li>";
            }

            if (lowNum > 1)
            {
                strOut += "<li class='" + cssPrevSection + "'><a pagenumber='" + (lowNum - 1) + "' class='" + linkclass + "'>...</a></li>";
            }

            #endregion

            // body section
            for (int i = lowNum; i <= highNum; i++)
            {
                if (currentPage == i)
                {
                    strOut += "<li class='" + selectclass + "'><a pagenumber='" + i + "' class='" + linkclass + "'>" + i + "</a></li>";
                }
                else
                {
                    strOut += "<li class='" + normalclass + "'><a pagenumber='" + i + "' class='" + linkclass + "'>" + i + "</a></li>";
                }
            }

            #region "last section"

            if ((lastPage > highNum))
            {
                strOut += "<li class='" + cssNextSection + "'><a pagenumber='" + (highNum + 1) + "' class='" + linkclass + "'>...</a></li>";
            }
            if ((lastPage > currentPage))
            {
                strOut += "<li class='" + cssNextPage + "'><a pagenumber='" + (currentPage + 1) + "' class='" + linkclass + "'>&gt;</a></li>";
            }

            if ((lastPage != highNum) && (lastPage > currentPage))
            {
                strOut += "<li class='" + cssLastPage + "'><a pagenumber='" + lastPage + "' class='" + linkclass + "'>&gt;&gt;</a></li>";
            }

            #endregion



            strOut += "</ul></div>";

            return strOut;
        }


    }

}
