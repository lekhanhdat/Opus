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
using System.Linq;
using System.Collections.Generic;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    public class AveClientCacheHandler
    {
        private static AveLogger Logger = new AveLogger(typeof(AveClientCacheHandler));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaXml"></param>
        /// <param name="HandlerId">Identify SchemaXml Cache uniquely for multi threads</param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="objId"></param>
        /// <param name="schemaType"></param>
        public static void WriteSchemaXml(string schemaXml, Guid HandlerId, string webId, string listId, string objId, SchemaType schemaType)
        {
            string webPath = AveWrapperConstants.WrapperTempFolder + "\\" + HandlerId.ToString() + webId;
            string dirPath = webPath;
            if (!string.IsNullOrEmpty(listId))
            {
                dirPath = webPath + "\\" + listId;
            }
            string filePath = dirPath + "\\" + schemaType.ToString() + ".xml";
            string fileLock = string.Intern(filePath);
            XmlDocument schemaFile = new XmlDocument();
            //CreateCacheFile(schemaFile, webId, listId, schemaType);
            lock (fileLock)
            {
                try
                {
                    if (!CreateTempFile(schemaFile, dirPath, webPath, filePath))
                    {
                        return;
                    }
                    schemaFile.Load(filePath);
                    AppendNode(schemaFile, schemaFile.DocumentElement, schemaType, objId, schemaXml);

                    schemaFile.Save(filePath);
                }
                catch (Exception ex)
                {
                    Logger.Debug("Error occurred while write SchemaXml to temp file.ErrorMessage:{0}.", ex);
                }
            }
        }

        public static void WriteSchemaXml(Dictionary<string, string> idSchemaXmlMappings, Guid HandlerId, string webId, string listId, SchemaType schemaType)
        {
            string webPath = AveWrapperConstants.WrapperTempFolder + "\\" + HandlerId.ToString() + webId;
            string dirPath = webPath;
            if (!string.IsNullOrEmpty(listId))
            {
                dirPath = webPath + "\\" + listId;
            }
            string filePath = dirPath + "\\" + schemaType.ToString() + ".xml";
            string fileLock = string.Intern(filePath);
            XmlDocument schemaFile = new XmlDocument();
            lock (fileLock)
            {
                try
                {
                    if (!CreateTempFile(schemaFile, dirPath, webPath, filePath))
                    {
                        return;
                    }
                    schemaFile.Load(filePath);
                    XmlElement root = schemaFile.DocumentElement;
                    foreach (var ct in idSchemaXmlMappings)
                    {
                        AppendNode(schemaFile, root, schemaType, ct.Key, ct.Value);
                    }
                    schemaFile.Save(filePath);
                }
                catch (Exception ex)
                {
                    Logger.Debug("Error occurred while write SchemaXml to temp file.ErrorMessage:{0}.", ex);
                }
            }
        }

        private static void AppendNode(XmlDocument schemaFile, XmlElement root, SchemaType schemaType, string objId, string schemaXml)
        {
            XmlElement tempNode = root.SelectSingleNode(".//*[@ID='" + objId + "']") as XmlElement;
            if (tempNode == null)
            {
                tempNode = schemaFile.CreateElement(schemaType.ToString());
                XmlAttribute id = schemaFile.CreateAttribute("ID");
                id.Value = objId;
                tempNode.Attributes.Append(id);
                root.AppendChild(tempNode);
            }
            tempNode.InnerXml = schemaXml;
        }

        private static bool CreateTempFile(XmlDocument schemaFile, string dirPath, string webPath, string filePath)
        {
            if (!Directory.Exists(webPath))
            {
                DirectoryInfo webDirectory = Directory.CreateDirectory(webPath);
                if (!webDirectory.Exists)
                {
                    return false;
                }
            }
            if (!dirPath.Equals(webPath, StringComparison.OrdinalIgnoreCase))
            {
                if (!Directory.Exists(dirPath))
                {
                    DirectoryInfo directory = Directory.CreateDirectory(dirPath);
                    if (!directory.Exists)
                    {
                        Logger.Warn("Create this folder failed. Path : {0}", dirPath);
                        return false;
                    }
                }
            }
            if (!File.Exists(filePath))
            {
                using (FileStream file = File.Create(filePath))
                {
                    XmlElement schemasNode = schemaFile.CreateElement("Schemas");
                    schemaFile.AppendChild(schemasNode);
                    schemaFile.Save(file);
                }
            }
            return true;
        }

        public static string GetSchemaXml(Guid HandlerId, string webId, string listId, string objId, SchemaType schemaType)
        {
            string schemaXml = string.Empty;
            string webDirectory = AveWrapperConstants.WrapperTempFolder + "\\" + HandlerId.ToString() + webId;
            string dirPath = webDirectory;
            if (!string.IsNullOrEmpty(listId))
            {
                dirPath = webDirectory + "\\" + listId;
            }
            string filePath = dirPath + "\\" + schemaType.ToString() + ".xml";
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

        public static bool CleanSchemaXml(Guid HandlerId, string webId, string listId)
        {
            bool successful = true;
            string errorInfo = string.Empty;
            string dirPath = AveWrapperConstants.WrapperTempFolder + "\\" + HandlerId.ToString() + webId;
            if(!string.IsNullOrEmpty(listId))
            {
                dirPath = Path.Combine(dirPath, listId);
            }
            DeleteCacheFile(dirPath);
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
                            Logger.Debug("Delete cache file failed.ErrorMessage:{0}", ex.ToString());
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
                        directory.Delete();
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Delete cache folder failed.ErrorMessage:{0}", ex.ToString());
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