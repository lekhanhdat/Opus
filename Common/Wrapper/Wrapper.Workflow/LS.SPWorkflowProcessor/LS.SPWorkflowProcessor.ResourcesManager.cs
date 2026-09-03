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
using System.Xml;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource;

namespace LS.SPWorkflowProcessor.Resources
{
    internal class LogObject
    {
        internal LogLevel mLogLevel;
        internal string mMessage;
        internal string mCategory;
    }
    internal class LogResoucesManager
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static Dictionary<string, LogObject> mLogObjects;
        internal static void LoadResources(string fileName)
        {
            mLogObjects = new Dictionary<string, LogObject>();
            XmlDocument doc = null;
            try
            {
                doc = new XmlDocument();
                doc.Load(fileName);
                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    if (!(node is XmlElement))
                        continue;
                    XmlElement xe = (XmlElement)node;
                    LogObject logObj = new LogObject();
                    logObj.mLogLevel = (LogLevel)int.Parse(xe.GetAttribute("Level"));
                    logObj.mMessage = xe.GetAttribute("Message");
                    logObj.mCategory = xe.GetAttribute("Category");
                    mLogObjects.Add(xe.GetAttribute("Name").ToLower(), logObj);
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.LoadXmlContentError, ex);
            }
            finally
            {
                if (doc != null)
                    doc.RemoveAll();
            }
        }

        internal static bool GetResource(string key, out LogLevel level, out string value, out string category)
        {
            level = LogLevel.Monitorable;
            value = string.Empty;
            category = string.Empty;
            key=key.ToLower();
            if(mLogObjects.ContainsKey(key))
            {
                level=mLogObjects[key].mLogLevel;
                value=mLogObjects[key].mMessage;
                category = mLogObjects[key].mCategory;
                return true;
            }
            return false;
        }

        internal static string GetLevelString(string key)
        { 
            key=key.ToLower();
            if (mLogObjects.ContainsKey(key))
            {
                return mLogObjects[key].mMessage;
            }
            else
                return "Unknown";
        }
    }
}
