using System;
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

    }

}
