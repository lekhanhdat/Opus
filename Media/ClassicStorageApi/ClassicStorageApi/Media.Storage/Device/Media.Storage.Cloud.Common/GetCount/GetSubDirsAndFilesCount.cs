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



using System;
using System.Collections.Generic;
using System.Text;
using AvePoint.Media.ClassicStorage.Cloud.Common;
using AvePoint.Media.ClassicStorage.Cloud.Common.Client;
using System.Xml.XPath;
using AvePoint.Media.ClassicStorage.Util;
using System.Globalization;


namespace AvePoint.Media.ClassicStorage.Cloud.Azure.GetCount
{
    public class GetSubDirsAndFilesCount
    {
        public static SubDirsAndFilesBean GetAzureSubDirsAndFilesCount(string responseXmlString, StorageInfo dirInfo, AbstractRESTOprationExecutor client)
        {
            SubDirsAndFilesBean sf = new SubDirsAndFilesBean();
            List<XPathNavigator> navs = client.FirstStepAnalyzeXML(responseXmlString, "EnumerationResults/Blobs/Blob");
            sf.filesCount = navs.Count;
            //XPathNavigator singleNav;
            //string name;
            //long size;

            //foreach (XPathNavigator nav in navs)
            //{
            //    name = null;
            //    size = 0;
            //    singleNav = nav.SelectSingleNode("Name");
            //    if (singleNav != null)
            //    {
            //        name = singleNav.Value;

            //        singleNav = nav.SelectSingleNode("Properties/Content-Length");
            //        if (singleNav != null)
            //        {
            //            size = singleNav.ValueAsLong;

            //        }
            //        name = name.RemoveFirst(dirInfo.LowName);
            //        if (name.Contains("/"))
            //        {
            //            if (!string.IsNullOrEmpty(name) && name.EndsWith("/"))
            //            {
            //                sf.dirsCount++;
            //            }
            //        }
            //        else
            //        {
            //            if (!string.IsNullOrEmpty(name))
            //            {
            //                sf.filesCount++;
            //            }
            //        }
            //    }
            //}

            navs = client.FirstStepAnalyzeXML(responseXmlString, "EnumerationResults/Blobs/BlobPrefix");
            sf.dirsCount = navs.Count;
            //foreach (XPathNavigator nav in navs)
            //{
            //    singleNav = nav.SelectSingleNode("Name");
            //    if (singleNav != null)
            //    {
            //        name = singleNav.Value;
            //        name = name.RemoveFirst(dirInfo.LowName);
            //        if (name.Contains("/"))
            //        {
            //            int index = name.IndexOf('/');
            //            if (index > 0)
            //            {
            //                name = name.Substring(0, index);
            //            }
            //            if (!string.IsNullOrEmpty(name) && !name.Contains("/"))
            //            {
            //                sf.dirsCount++;
            //            }
            //        }
            //    }
            //}

            return sf;
        }


        public static SubDirsAndFilesBean GetAmazonSubDirsAndFilesCount(string responseXmlString, StorageInfo dirInfo, AbstractRESTOprationExecutor client)
        {
            SubDirsAndFilesBean sf = new SubDirsAndFilesBean();

            responseXmlString = responseXmlString.Replace(" XMLNS=\"HTTP://S3.AMAZONAWS.COM/DOC/2006-03-01/\"", "");
            List<XPathNavigator> navs = client.FirstStepAnalyzeXML(responseXmlString, "ListBucketResult/Contents");

            sf.filesCount = navs.Count;
            //XPathNavigator singleNav;
            //string name;
            //long size;
            //foreach (XPathNavigator nav in navs)
            //{
            //    name = null;
            //    size = 0;
            //    singleNav = nav.SelectSingleNode("Key");
            //    if (singleNav != null)
            //    {
            //        name = singleNav.Value;

            //        singleNav = nav.SelectSingleNode("Size");
            //        if (singleNav != null)
            //        {
            //            size = singleNav.ValueAsLong;

            //        }
            //        if (name.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            //        {
            //            name = name.RemoveFirst(dirInfo.LowName).TrimEnd('/');
            //            if (string.IsNullOrEmpty(name))
            //            {
            //                continue;
            //            }
            //            sf.dirsCount++;
            //        }
            //        else
            //        {
            //            sf.filesCount++;
            //        }
            //    }

            //}

            navs = client.FirstStepAnalyzeXML(responseXmlString, "ListBucketResult/CommonPrefixes");
            sf.dirsCount = navs.Count;

            //foreach (XPathNavigator nav in navs)
            //{
            //    name = null;
            //    size = 0;
            //    singleNav = nav.SelectSingleNode("Prefix");
            //    if (singleNav != null)
            //    {
            //        name = singleNav.Value;
            //        name = name.RemoveFirst(dirInfo.LowName).TrimEnd('/');
            //        if (string.IsNullOrEmpty(name))
            //        {
            //            continue;
            //        }
            //        sf.dirsCount++;
            //    }
            //}
            return sf;
        }

        public static SubDirsAndFilesBean GetAtmosSubDirsAndFilesCount(string responseXmlString, StorageInfo dirInfo, AbstractRESTOprationExecutor client)
        {
            SubDirsAndFilesBean sf = new SubDirsAndFilesBean();

            responseXmlString = responseXmlString.Replace("XMLNS='HTTP://WWW.EMC.COM/COS/'".ToLower(CultureInfo.InvariantCulture), "");
            List<XPathNavigator> navs = client.FirstStepAnalyzeXML(responseXmlString, "ListDirectoryResponse/DirectoryList/DirectoryEntry");
            XPathNavigator singleNav;
            string name;
            string fileType;
            foreach (XPathNavigator nav in navs)
            {
                name = null;
                singleNav = nav.SelectSingleNode("FileType");
                if (singleNav != null)
                {
                    fileType = singleNav.Value;
                    if ("directory".Equals(fileType, StringComparison.CurrentCultureIgnoreCase))
                    {
                        singleNav = nav.SelectSingleNode("Filename");
                        if (singleNav != null)
                        {
                            name = singleNav.Value;
                            if (!string.IsNullOrEmpty(name))
                            {
                                sf.dirsCount++;
                            }
                        }
                    }
                    else
                    {
                        singleNav = nav.SelectSingleNode("Filename");
                        if (singleNav != null)
                        {
                            name = singleNav.Value;
                            if (!string.IsNullOrEmpty(name))
                            {
                                sf.filesCount++;
                            }
                        }
                    }
                }
            }
            return sf;
        }


        public static SubDirsAndFilesBean GetRackspaceSubDirsAndFilesCount(string responseXmlString, StorageInfo dirInfo, AbstractRESTOprationExecutor client)
        {

            SubDirsAndFilesBean sf = new SubDirsAndFilesBean();
            //List<XPathNavigator> fileNavs;
            //List<XPathNavigator> dirNavs;
            List<XPathNavigator> navs = client.FirstStepAnalyzeXML(responseXmlString, "container/object");
            sf.filesCount = navs.Count;


            //fileNavs = navs;
            //XPathNavigator singleNav;
            //string name;
            //long size;
            //foreach (XPathNavigator nav in navs)
            //{
            //    name = null;
            //    size = 0;
            //    singleNav = nav.SelectSingleNode("name");
            //    if (singleNav != null)
            //    {
            //        name = singleNav.Value;

            //        singleNav = nav.SelectSingleNode("bytes");
            //        if (singleNav != null)
            //        {
            //            size = singleNav.ValueAsLong;
            //        }

            //        if (name.EndsWith("/", StringComparison.OrdinalIgnoreCase) && size == 0)
            //        {
            //            name = name.RemoveFirst(dirInfo.LowName).TrimEnd('/');
            //            if (!string.IsNullOrEmpty(name))
            //            {
            //                sf.dirsCount++;
            //            }
            //        }
            //        else
            //        {
            //            name = name.RemoveFirst(dirInfo.LowName);
            //            if (!string.IsNullOrEmpty(name))
            //            {
            //                sf.filesCount++;
            //            }
            //        }
            //    }
            //}


            navs = client.FirstStepAnalyzeXML(responseXmlString, "container/" + "SUBDIR".ToLower(CultureInfo.InvariantCulture));
            sf.dirsCount = navs.Count;

            //dirNavs = navs;
            //foreach (XPathNavigator nav in navs)
            //{
            //    singleNav = nav.SelectSingleNode("name");
            //    if (singleNav != null)
            //    {
            //        name = singleNav.Value;
            //        name = name.RemoveFirst(dirInfo.LowName).TrimEnd('/');
            //        if (!string.IsNullOrEmpty(name))
            //        {
            //            sf.dirsCount++;
            //        }
            //    }
            //}
            return sf;
        }
    }




    public class SubDirsAndFilesBean
    {
        public int dirsCount { set; get; }
        public int filesCount { set; get; }

    }


}
