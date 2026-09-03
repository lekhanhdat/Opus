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
using System.IO;
using System.Xml;
using AvePoint.Common;
using AvePoint.GCommon;
using System.Collections.Generic;
using AvePoint.GCommon.Utility;

namespace AvePoint.ObjectModel.Common
{
    public class AveClientCacheHandler
    {
        private static AveLogger Logger = new AveLogger(typeof(AveClientCacheHandler));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaXml"></param>
        /// <param name="handlerId">Identify SchemaXml Cache uniquely for multi threads</param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="objId"></param>
        /// <param name="schemaType"></param>
        public static void WriteSchemaXml(string schemaXml, Guid handlerId, string webId, string listId, string objId, SchemaType schemaType)
        {
            string filePath = GetFilePath(handlerId, webId, listId, schemaType, true);
            string fileLock = string.Intern(filePath);
            XmlDocument schemaFile = new XmlDocument();
            lock (fileLock)
            {
                try
                {
                    #region create temp file
                    if (!File.Exists(filePath))
                    {
                        using (FileStream file = File.Create(filePath))
                        {
                            XmlElement schemasNode = schemaFile.CreateElement("Schemas");
                            schemaFile.AppendChild(schemasNode);
                            schemaFile.Save(file);
                        }
                    }
                    #endregion
                    schemaFile.Load(filePath);
                    XmlElement root = schemaFile.DocumentElement;

                    var validatedObjId = SecurityUtils.SanitizeXMLContent(objId);
                    XmlElement tempNode = root.SelectSingleNode(".//*[@ID='" + validatedObjId + "']") as XmlElement;
                    if (tempNode == null)
                    {
                        tempNode = schemaFile.CreateElement(schemaType.ToString());
                        XmlAttribute id = schemaFile.CreateAttribute("ID");
                        id.Value = objId;
                        tempNode.Attributes.Append(id);
                        root.AppendChild(tempNode);
                    }
                    tempNode.InnerXml = schemaXml;
                    schemaFile.Save(filePath);
                }
                catch (Exception ex)
                {
                    Logger.Debug("Error occurred while write SchemaXml to temp file.ErrorMessage:{0}.", ex.ToString());
                }
            }
        }

        public static void WriteSchemaXml(IEnumerable<KeyValuePair<string, string>> fieldProperties, Guid handlerId, string webId, string listId, SchemaType schemaType)
        {
            string filePath = GetFilePath(handlerId, webId, listId, schemaType, true);
            string fileLock = string.Intern(filePath);
            XmlDocument schemaFile = new XmlDocument();
            lock (fileLock)
            {
                try
                {
                    #region create temp file
                    if (!File.Exists(filePath))
                    {
                        using (FileStream file = File.Create(filePath))
                        {
                            XmlElement schemasNode = schemaFile.CreateElement("Schemas");
                            schemaFile.AppendChild(schemasNode);
                            schemaFile.Save(file);
                        }
                    }
                    #endregion
                    string schemaTypeStr = schemaType.ToString();
                    schemaFile.Load(filePath);
                    XmlElement root = schemaFile.DocumentElement;
                    var fieldMap = new List<string>();
                    if (root.ChildNodes.Count > 0)
                    {
                        foreach (XmlElement node in root.ChildNodes)
                        {
                            string id = node.GetAttribute("ID");
                            fieldMap.Add(id);
                        }
                    }

                    foreach (var property in fieldProperties)
                    {
                        string key = property.Key;
                        if (!fieldMap.Contains(key))
                        {
                            XmlElement tempNode = schemaFile.CreateElement(schemaTypeStr);
                            tempNode.SetAttribute("ID", key);
                            tempNode.InnerXml = property.Value;
                            root.AppendChild(tempNode);
                        }
                    }

                    schemaFile.Save(filePath);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error occurred while write SchemaXml to temp file:{0} .ErrorMessage:{1}.",filePath, ex.ToString());
                }
            }
        }

        public static Dictionary<string, XmlElement> GetSchemaXmlElements(Guid handlerId, string webId, string listId, SchemaType schemaType)
        {
            string filePath = GetFilePath(handlerId, webId, listId, schemaType, false);

            Dictionary<string, XmlElement> schemaXml = new Dictionary<string, XmlElement>(StringComparer.Ordinal);

            if (File.Exists(filePath))
            {
                string fileLock = string.Intern(filePath);
                XmlDocument schemaFile = new XmlDocument();
                lock (fileLock)
                {
                    try
                    {
                        schemaFile.Load(filePath);
                        XmlElement root = schemaFile.DocumentElement;
                        if (root.ChildNodes.Count > 0)
                        {
                            foreach (XmlElement node in root.ChildNodes)
                            {
                                string id = node.GetAttribute("ID");
                                schemaXml[id] = node.FirstChild as XmlElement;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Error occurred while read SchemaXml from temp file:{0} .ErrorMessage:{1}.", filePath, ex.ToString());
                    }
                }
            }

            return schemaXml;
        }

        public static Dictionary<string, string> GetSchemaXmlMapping(Guid handlerId, string webId, string listId, SchemaType schemaType)
        {
            string filePath = GetFilePath(handlerId, webId, listId, schemaType, false);

            Dictionary<string, string> schemaXml = new Dictionary<string, string>(StringComparer.Ordinal);

            if (File.Exists(filePath))
            {
                string fileLock = string.Intern(filePath);
                XmlDocument schemaFile = new XmlDocument();
                lock (fileLock)
                {
                    try
                    {
                        schemaFile.Load(filePath);
                        XmlElement root = schemaFile.DocumentElement;
                        if (root.ChildNodes.Count > 0)
                        {
                            foreach (XmlElement node in root.ChildNodes)
                            {
                                string id = node.GetAttribute("ID");
                                schemaXml[id] = node.InnerXml;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Error occurred while read SchemaXml from temp file:{0} .ErrorMessage:{1}.", filePath, ex.ToString());
                    }
                }
            }

            return schemaXml;
        }

        private static string GetFilePath(Guid handlerId, string webId, string listId, SchemaType schemaType, bool autoCreated)
        {
            string filePath = string.Empty;
            var webPath = Path.Combine(AveEnv.AgentTempFolder, handlerId.ToString() + webId);
            //var webPath = AveEnv.AgentTempFolder + "\\" + handlerId.ToString() + webId.ToString();
            var dirPath = webPath;
            if (!string.IsNullOrEmpty(listId))
            {
                dirPath = Path.Combine(webPath, listId);
            }
            filePath = Path.Combine(dirPath, schemaType.ToString() + ".xml");

            if (autoCreated)
            {
                try
                {
                    if (!Directory.Exists(webPath))
                    {
                        DirectoryInfo webDirectory = Directory.CreateDirectory(webPath);
                    }

                    if (!dirPath.Equals(webPath, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!Directory.Exists(dirPath))
                        {
                            DirectoryInfo directory = Directory.CreateDirectory(dirPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn("Ensure directory {0} and {1} failed:{2}", handlerId.ToString() + webId, schemaType.ToString() + ".xml", ex);
                }
            }

            return filePath;
        }

        //private static void CreateCacheFile(XmlDocument schemaFile, Guid HandlerId, string webId, string listId, SchemaType schemaType)
        //{
        //    string webPath = AveEnv.AgentTempFolder + "\\" + HandlerId.ToString() + webId;
        //    string dirPath = webPath;
        //    if (!string.IsNullOrEmpty(listId))
        //    {
        //        dirPath = webPath + "\\" + listId;
        //    }
        //    string filePath = dirPath + "\\" + schemaType.ToString() + ".xml";
        //    if (!Directory.Exists(webPath))
        //    {
        //        DirectoryInfo webDirectory = Directory.CreateDirectory(webPath);
        //        if (!webDirectory.Exists)
        //        {
        //            return;
        //        }
        //    }
        //    if (!dirPath.Equals(webPath, StringComparison.OrdinalIgnoreCase))
        //    {
        //        if (!Directory.Exists(dirPath))
        //        {
        //            DirectoryInfo directory = Directory.CreateDirectory(dirPath);
        //            if (!directory.Exists)
        //            {
        //                return;
        //            }
        //        }
        //    }
        //    if (!File.Exists(filePath))
        //    {
        //        using (FileStream file = File.Create(filePath))
        //        {
        //        }
        //        XmlElement schemasNode = schemaFile.CreateElement("Schemas");
        //        schemaFile.AppendChild(schemasNode);
        //        schemaFile.Save(filePath);
        //    }
        //}

        public static string GetSchemaXml(Guid handlerId, string webId, string listId, string objId, SchemaType schemaType)
        {
            string schemaXml = string.Empty;
            string filePath = GetFilePath(handlerId, webId, listId, schemaType, false);
            if (!File.Exists(filePath))
            {
                return string.Empty;
            }
            string fileLock = string.Intern(filePath);
            XmlDocument schemaFile = new XmlDocument();
            lock (fileLock)
            {
                try
                {
                    schemaFile.Load(filePath);
                    XmlNode tempNode = schemaFile.SelectSingleNode(".//*[@ID='" + objId + "']");
                    if (tempNode != null)
                    {
                        schemaXml = tempNode.InnerXml;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Debug("Error occurred while get SchemaXml.ErrorMessage:{0}.", ex.ToString());
                }
            }
            return schemaXml;
        }

        public static bool CleanSchemaXml(Guid handlerId, string containerGuid)
        {
            bool successful = true;
            string errorInfo = string.Empty;
            //string dirPath = SecurityUtils.SafeCombinePath(AveEnv.AgentTempFolder, handlerId.ToString() + containerGuid);
            //Logger.Info("start to clean schema xml under folder:{0}, {1}", dirPath, new System.Diagnostics.StackTrace());
            Logger.Info("start to clean schema xml under folder:{0}", handlerId.ToString() + containerGuid);
            DeleteCacheFile(SecurityUtils.SafeCombinePath(AveEnv.AgentTempFolder, handlerId.ToString() + containerGuid));
            if (!string.IsNullOrEmpty(errorInfo))
            {
                successful = false;
            }
            return successful;
        }

        private static void DeleteCacheFile(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                DirectoryInfo directory = new DirectoryInfo(dirPath);
#region Clean Files
                foreach (FileInfo file in directory.GetFiles())
                {
                    string fileLock = string.Intern(file.FullName);
                    lock (fileLock)
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception ex)
                        {
                            Logger.Warn("Delete cache file:{0} failed.ErrorMessage:{1}", file.Name, ex);
                        }
                    }
                }
                foreach (DirectoryInfo folder in directory.GetDirectories())
                {
                    DeleteCacheFile(folder.FullName);
                }
#endregion
                string folderLock = string.Intern(directory.FullName);
                lock (folderLock)
                {
                    try
                    {
                        Logger.Info($"Delete Directory [{directory.Name}].Location:AveClientCacheHandler.DeleteCacheFile");
                        directory.Delete();
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn("Delete cache folder:{0} failed.ErrorMessage:{1}", directory.Name, ex);
                    }
                }
            }
        }
    }

    public enum SchemaType : int
    {
        List = 0,
        Field = 1,
        FieldCollection = 2,
        ViewFieldCollection = 3,
        ContentType = 4,
        View = 5
    }
}