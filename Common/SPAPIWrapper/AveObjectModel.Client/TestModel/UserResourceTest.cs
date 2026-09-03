/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestModel
{
    public static class TestExtension
    {
        public static StringBuilder FormatOutput<T1, T2>(this Dictionary<T1, T2> obj)
        {
            string formatString = "{0}-{1}";
            StringBuilder result = new StringBuilder();
            result.AppendLine("UserResourceInfo");
            if (obj == null) return result;
            foreach (var key in obj.Keys)
            {
                result.AppendLine(string.Format(formatString,key,obj[key]));
            }
            return result;
        }
    }
    public class UserResourceTest
    {
        private IAveSite mSite;
        private IAveWeb mWeb;
        private IAveList mList;

        public UserResourceTest()
        {
            Init();
        }
        
        private void Init()
        {
            string user = "wbhu@M365x113665.onmicrosoft.com";
            string pwd = "demo12!@";
            WrapperConfiguration.BPOS_S.EnableMultiLanguage = true;
            string url = "https://m365x113665.sharepoint.com/sites/Multiply_Language_English";
            var factory = AveObjectModelFactory.CreateObjectModelFactory(url, new AveBPOSAccountInfo { UserName = user, Password = pwd });
            mSite = factory.CreateSite(url);
            mWeb = mSite.RootWeb;
            mList = mWeb.Lists["Documents"];
        }

        public void RunTest()
        {
            Console.WriteLine("Web:"+mWeb.Url);
            var webData = AveAssemblyUtility.GetFieldValue(mWeb.TitleResource, "mKeyValues") as Dictionary<string,string>;
            Console.WriteLine(webData.FormatOutput());
            webData = AveAssemblyUtility.GetFieldValue(mWeb.DescriptionResource, "mKeyValues") as Dictionary<string, string>;
            Console.WriteLine(webData.FormatOutput());
           var ctinfos= mWeb.AvailableContentTypes.GetContentTypeInfos(false);
            foreach (var ct in mWeb.AvailableContentTypes)
            {
                if (ct.Name == "CustomCT")
                {
                    Console.WriteLine("CT:" + ct.Name);
                    webData = AveAssemblyUtility.GetFieldValue(ct.NameResource, "mKeyValues") as Dictionary<string, string>;
                    Console.WriteLine(webData.FormatOutput());
                    webData = AveAssemblyUtility.GetFieldValue(ct.DescriptionResource, "mKeyValues") as Dictionary<string, string>;
                    Console.WriteLine(webData.FormatOutput());
                }
            }

            foreach (var list in mWeb.Lists)
            {
                webData = AveAssemblyUtility.GetFieldValue(mList.TitleResource, "mKeyValues") as Dictionary<string, string>;
                Console.WriteLine(webData.FormatOutput());
                webData = AveAssemblyUtility.GetFieldValue(mList.DescriptionResource, "mKeyValues") as Dictionary<string, string>;
                Console.WriteLine(webData.FormatOutput());
                //webData = AveAssemblyUtility.GetFieldValue(mList.DescriptionResource, "mKeyValues") as Dictionary<string, string>;
                //if (webData != null && webData.Count > 0)
                //{
                //    Console.WriteLine("List:" + list.Title);
                //    Console.WriteLine(webData.FormatOutput());
                //}
            }
        }


        public void RunUpdateTest()
        {
            Console.WriteLine("Web:" + mWeb.Url);
            mWeb.TitleResource.SetUserResource(mWeb,new Dictionary<string,string> { { "de-DE","test title"}},true);
            mWeb.DescriptionResource.SetUserResource(mWeb, new Dictionary<string, string> { { "de-DE", "test desr" } }, true);
            mWeb.TitleResource.Update();
            mWeb.DescriptionResource.Update();
            var webData = AveAssemblyUtility.GetFieldValue(mWeb.TitleResource, "mKeyValues") as Dictionary<string, string>;
            Console.WriteLine(webData.FormatOutput());
            webData = AveAssemblyUtility.GetFieldValue(mWeb.DescriptionResource, "mKeyValues") as Dictionary<string, string>;
            Console.WriteLine(webData.FormatOutput());

            foreach (var ct in mWeb.AvailableContentTypes)
            {
                if (ct.Name == "CustomCT")
                {
                    Console.WriteLine("CT:" + ct.Name);
                    webData = AveAssemblyUtility.GetFieldValue(ct.NameResource, "mKeyValues") as Dictionary<string, string>;
                    Console.WriteLine(webData.FormatOutput());
                    webData = AveAssemblyUtility.GetFieldValue(ct.DescriptionResource, "mKeyValues") as Dictionary<string, string>;
                    Console.WriteLine(webData.FormatOutput());
                }
            }

            foreach (var list in mWeb.Lists)
            {
                webData = AveAssemblyUtility.GetFieldValue(mList.TitleResource, "mKeyValues") as Dictionary<string, string>;
                Console.WriteLine(webData.FormatOutput());
                webData = AveAssemblyUtility.GetFieldValue(mList.DescriptionResource, "mKeyValues") as Dictionary<string, string>;
                Console.WriteLine(webData.FormatOutput());
                //webData = AveAssemblyUtility.GetFieldValue(mList.DescriptionResource, "mKeyValues") as Dictionary<string, string>;
                //if (webData != null && webData.Count > 0)
                //{
                //    Console.WriteLine("List:" + list.Title);
                //    Console.WriteLine(webData.FormatOutput());
                //}
            }
        }
    }
}
