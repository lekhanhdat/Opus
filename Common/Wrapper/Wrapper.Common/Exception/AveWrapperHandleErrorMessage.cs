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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Utility;

namespace AvePoint.Wrapper.Common
{
    public class AveWrapperHandleErrorMessage
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(typeof(AveWrapperHandleErrorMessage));
        #region 处理CM  DPM模块的错误信息
        public static string GetFormateErrorMessage(string defaultKey, string defaultErrorMessage, params object[] defaultArgs)
        {
            try
            {
                string defaultValue = string.Format(defaultErrorMessage, defaultArgs);
                List<PropertyItem> propertyItems = new List<PropertyItem>() { new PropertyItem() { PropertyType = ParamKey.Message, Key = defaultKey, Args = defaultArgs, DefaultValue = defaultValue } };
                return SerializerHelper.SerializeToXmlString<List<PropertyItem>>(propertyItems);
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while converting error message to xml. Error{0}", e.ToString());
                return defaultErrorMessage;
            }
        }

        /// <summary>
        /// 建议直接使用AveWrapperI18NException.GetFormatedMessage不要使用该方法
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="defaultKey"></param>
        /// <param name="defaultErrorMessage"></param>
        /// <param name="defaultArgs"></param>
        /// <returns></returns>
        public static string GetFormateErrorMessage(Exception exception, string defaultKey, string defaultErrorMessage, params object[] defaultArgs)
        {
            try
            {
                AveWrapperI18NException i18nException = exception as AveWrapperI18NException;
                string key = defaultKey;
                string defaultValue = string.Format(defaultErrorMessage, defaultArgs);
                List<object> args = new List<object>(defaultArgs);
                if (i18nException != null && !string.IsNullOrEmpty(i18nException.Key))
                {
                    key = i18nException.Key;
                    args = i18nException.Args;
                    defaultValue = i18nException.Message;
                }
                List<PropertyItem> propertyItems = new List<PropertyItem>() { new PropertyItem() { PropertyType = ParamKey.Message, Key = key, Args = args.ToArray(), DefaultValue = defaultValue } };
                return SerializerHelper.SerializeToXmlString<List<PropertyItem>>(propertyItems);
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while converting error message to xml. Error{0}", e.ToString());
                return exception.Message;
            }
        }
        #endregion


        #region 处理Granular 模块的错误信息
        public static string ConvertErrorMessageToXML(string defaultKey, string defaultErrorMessage, params object[] defaultArgs)
        {
            try
            {
                XmlDocument xd = new XmlDocument();
                xd.LoadXml("<ErrorMessage></ErrorMessage>");
                XmlElement rootNode = xd.DocumentElement;
                XmlElement keyNode = xd.CreateElement("Key");
                keyNode.SetAttribute("key", defaultKey);
                if (!string.IsNullOrEmpty(string.Format(defaultErrorMessage, defaultArgs)))
                {
                    keyNode.SetAttribute("DefaultValue", string.Format(defaultErrorMessage, defaultArgs));
                }
                if (defaultArgs != null)
                {
                    foreach (object para in defaultArgs)
                    {
                        XmlElement paraNode = xd.CreateElement("Para");
                        paraNode.SetAttribute("Value", para.ToString());
                        keyNode.AppendChild(paraNode);
                    }
                }
                rootNode.AppendChild(keyNode);
                return rootNode.OuterXml;
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while converting error message to xml. Error{0}", e.ToString());
                return defaultErrorMessage;
            }
        }

        public static string ConvertErrorMessageToXML(Exception exception, string defaultKey, string defaultErrorMessage, params object[] defaultArgs)
        {
            try
            {
                    AveWrapperI18NException i18nException = exception as AveWrapperI18NException;
                    string key = defaultKey;
                    string defaultValue = string.Format(defaultErrorMessage, defaultArgs);
                    List<object> args = new List<object>(defaultArgs);
                    if (i18nException != null && !string.IsNullOrEmpty(i18nException.Key))
                    {
                        key = i18nException.Key;
                        args = i18nException.Args;
                        defaultValue = i18nException.Message;
                    }
                    XmlDocument xd = new XmlDocument();
                    xd.LoadXml("<ErrorMessage></ErrorMessage>");
                    XmlElement rootNode = xd.DocumentElement;
                    XmlElement keyNode = xd.CreateElement("Key");
                    keyNode.SetAttribute("key", key);
                    if (!string.IsNullOrEmpty(defaultValue))
                    {
                        keyNode.SetAttribute("DefaultValue", defaultValue);
                    }
                    if (args != null)
                    {
                        foreach (object para in args)
                        {
                            XmlElement paraNode = xd.CreateElement("Para");
                            paraNode.SetAttribute("Value", para.ToString());
                            keyNode.AppendChild(paraNode);
                        }
                    }
                    rootNode.AppendChild(keyNode);
                    return rootNode.OuterXml;
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while converting error message to xml. Error{0}", e.ToString());
                return exception.Message;
            }
        }
        #endregion

    }
}
