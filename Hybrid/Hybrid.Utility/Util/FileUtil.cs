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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace  AvePoint.Hybrid.Utility
{
    public class FileUtil
    {

        public static void CheckAndCreateDirectory(string path)
        {

            FileInfo reportFile = new FileInfo(path);
            if (!reportFile.Directory.Exists)
            {
                reportFile.Directory.Create();
            }

        }

        public static void WriteFile(string path, string text)
        {

            File.WriteAllText(path, text, Encoding.UTF8);

        }

        public static void WriteFile(string userName, string password, string path, string text)
        {

            File.WriteAllText(path, text, Encoding.UTF8);
        }

        public static string ReadFile(string path)
        {

           return File.ReadAllText(path, Encoding.UTF8);

        }

        public static string ReadFile(string userName, string password, string path)
        {

          return  File.ReadAllText(path, Encoding.UTF8);

        }

        public static void ReplaceContent(string path, string oldValue, string newValue)
        {
            String strFile = File.ReadAllText(path);
            if(strFile.IndexOf(oldValue) > 0)
            {
                strFile = strFile.Replace(oldValue, newValue);
                File.WriteAllText(path, strFile);
            }
        }

        public static string GetAgentLog4netFilePath()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Config\\RecordsAgentLog4net.config";
            if(File.Exists(path))
            {
                return path;
            }
            throw new Exception("Log4net config file not found.");
        }

        public static string GetTimerLog4netFilePath()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Config\\TimerLog4net.config";
            if (File.Exists(path))
            {
                return path;
            }

            throw new Exception("Log4net config file not found.");
        }

        public static string GetDALog4netFilePath(string logFilePath)
        {
            string logPath = "";
            if (File.Exists(logFilePath))
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(logFilePath);
                XmlNode root = xd.SelectSingleNode("//log4net//appender");
                XmlNodeList nodelist = root.ChildNodes;
                if (nodelist.Count > 0)
                {
                    foreach (XmlElement el in nodelist) 
                    {
                        if(el.Name.Equals("param", StringComparison.OrdinalIgnoreCase))
                        {
                            var nameAttr = el.Attributes["name"];
                            var valueAttr = el.Attributes["value"];
                            if (nameAttr != null && nameAttr.Value.Equals("File", StringComparison.OrdinalIgnoreCase) && valueAttr != null)
                            {
                                string temp = valueAttr.Value.Replace('/', '\\');
                                int lastSlash = temp.LastIndexOf('\\');
                                logPath = temp.Substring(0, lastSlash);

                                if(logPath.IndexOf(':') == -1 && !logPath.StartsWith("\\"))
                                {
                                    logPath = AppDomain.CurrentDomain.BaseDirectory + "..\\" + logPath;
                                }
                                break;
                            }
                        }
                    }
                }
                
                return logPath;
            }

            throw new Exception("Log4net config file not found.");
        }

        public static string GetDAAgentLog4netFilePath(string logFilePath)
        {
            string logPath = "";
            if (File.Exists(logFilePath))
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(logFilePath);
                XmlNode root = xd.SelectSingleNode("//log4net//appender");
                XmlNodeList nodelist = root.ChildNodes;
                if (nodelist.Count > 0)
                {
                    foreach (XmlElement el in nodelist)
                    {
                        if (el.Name.Equals("file", StringComparison.OrdinalIgnoreCase))
                        {
                            var typeAttr = el.Attributes["type"];
                            var valueAttr = el.Attributes["value"];
                            if (typeAttr != null && typeAttr.Value.Equals("log4net.Util.PatternString", StringComparison.OrdinalIgnoreCase) && valueAttr != null)
                            {
                                string temp = valueAttr.Value.Replace('/', '\\');
                                int lastSlash = temp.LastIndexOf('\\');
                                logPath = temp.Substring(0, lastSlash).Replace("%property{RelatedPath}", "");

                                if (logPath.IndexOf(':') == -1 && !logPath.StartsWith("\\") && !logPath.StartsWith("..\\"))
                                {
                                    logPath = AppDomain.CurrentDomain.BaseDirectory + "..\\bin\\" + logPath;
                                }
                                break;
                            }
                        }
                    }
                }
                return logPath;
            }
            throw new Exception("Log4net config file not found.");
        }
    }
}
