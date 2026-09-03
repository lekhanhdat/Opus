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
using AvePoint.Common;
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AvePoint.RA.Hybrid.Browser.SharePointBrowser.IndividualLevel
{
    public static class BrowserTreeUtility
    {
        private static readonly AvePoint.GCommon.AveLogger Logger = AvePoint.GCommon.AveLogger.GetInstance(typeof(BrowserTreeUtility));

        #region Design List
        private static List<string> list;
        public static List<string> DesignLists
        {
            get
            {
                if (list == null)
                {
                    list = new List<string>();
                    try
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(DirPath + "SP2010CMDMCommonDesignLists.xml");
                        foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                        {
                            try
                            {
                                XmlElement ele = node as XmlElement;
                                if (ele != null)
                                {
                                    list.Add(node.Attributes["url"].Value.ToUpper(CultureInfo.InvariantCulture) + "," + node.Attributes["serverTemplate"].Value);
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Warn("Getting exception while Assembling design list in AveBTreeConstants.DesignLists: " + e.Message + e.StackTrace);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn("Exception in AveBTreeConstants.DesignLists: " + ex.Message + ex.StackTrace);
                    }
                }
                return list;
            }
        }
        #endregion

        #region Config File Dir
        private static string m_DirPath;
        /// <summary>
        /// Bin Directory of DocAve Agent.
        /// </summary>
        public static string DirPath
        {
            get
            {
                if (m_DirPath == null || m_DirPath == string.Empty)
                {
                    FileInfo assemblyFile = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    m_DirPath = assemblyFile.Directory.Parent.FullName + "\\data\\SP2010\\CMDMCommon\\Browser\\";
                }
                return m_DirPath;
            }
        }
        #endregion
    }
}
