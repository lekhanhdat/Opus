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

using AvePoint.GCommon;
using AvePoint.Wrapper.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Core.SPRestore
{
    abstract class BaseLanguageMapping : ILanguageMapping
    {
        protected static AveLogger logger = AveLogger.GetInstance(typeof(BaseLanguageMapping), false);

        protected bool isDefaultLoaded = false;

        protected Dictionary<string, string> listTitleMapping = null;
        protected Dictionary<string, string> fieldDisplayNameMapping = null;
        protected Dictionary<string, string> contentTypeNameMapping = null;
        protected Dictionary<string, string> navigationTitleMapping = null;
        protected Dictionary<string, string> permissionLevelMapping = null;
        protected Dictionary<string, string> groupNameMapping = null;

        public uint SourceLCID { get; set; }

        public uint DestLCID { get; set; }

        protected string GetMappingItem(ref Dictionary<string, string> collection, string item)
        {
            string mappingItem = null;

            if (collection != null && collection.Count > 0)
            {
                lock (collection)
                {
                    collection.TryGetValue(item, out mappingItem);
                }
            }

            if (mappingItem == null)
            {
                mappingItem = item;
            }
            else
            {
                LogMapping(item, mappingItem);
            }

            return mappingItem;
        }

        protected void AddMappingItem(ref Dictionary<string, string> collection, string key, string value)
        {
            if(collection == null)
            {
                lock(this)
                {
                    if(collection == null)
                    {
                        collection = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }
                }
            }

            lock (collection)
            {
                collection[key] = value;
            }
        }

        public virtual string GetMappingListTitle(string listTitle)
        {
            return GetMappingItem(ref listTitleMapping, listTitle);
        }

        public virtual string GetMappingFieldDisplayName(string fieldDisplayName)
        {
            return GetMappingItem(ref fieldDisplayNameMapping, fieldDisplayName);
        }

        public virtual string GetMappingContentTypeName(string contentTypeName)
        {
            return GetMappingItem(ref contentTypeNameMapping, contentTypeName);
        }

        public virtual string GetMappingNavigationTitle(string navigationTitle)
        {
            return GetMappingItem(ref navigationTitleMapping, navigationTitle);
        }

        public virtual string GetMappingPermissionLevelName(string permissionLevelName)
        {
            return GetMappingItem(ref permissionLevelMapping, permissionLevelName);
        }

        public virtual string GetMappingGroupName(string title)
        {
            return GetMappingItem(ref groupNameMapping, title);
        }

        internal void AddListTitleMapping(string sourceListTitle, string destListTitle)
        {
            AddMappingItem(ref listTitleMapping, sourceListTitle, destListTitle);
        }

        internal void AddColumnDisplayNameMapping(string sourceName, string destName)
        {
            AddMappingItem(ref fieldDisplayNameMapping, sourceName, destName);
        }

        public string ExportMapping()
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml("<LanguageMapping />");
            var sourceLanguage = document.CreateElement("Language");
            sourceLanguage.SetAttribute("id", SourceLCID.ToString());
            var destLanguage = document.CreateElement("Language");
            destLanguage.SetAttribute("id", DestLCID.ToString());

            var changed = AddMapping(ref listTitleMapping, "Lists", document, sourceLanguage, destLanguage, 0);
            changed |= AddMapping(ref fieldDisplayNameMapping, "Columns", document, sourceLanguage, destLanguage, 0);
            changed |= AddMapping(ref contentTypeNameMapping, "ContentType", document, sourceLanguage, destLanguage, 0);
            changed |= AddMapping(ref navigationTitleMapping, "Navigation", document, sourceLanguage, destLanguage, 0);
            changed |= AddMapping(ref permissionLevelMapping, "Permissions", document, sourceLanguage, destLanguage, 0);
            changed |= AddMapping(ref groupNameMapping, "Permissions", document, sourceLanguage, destLanguage, permissionLevelMapping.Count);

            if (changed)
            {
                document.AppendChild(sourceLanguage);
                document.AppendChild(destLanguage);

                return document.OuterXml;
            }

            return string.Empty;
        }

        protected static bool AddMapping(ref Dictionary<string, string> collection, string name, XmlDocument document, XmlElement sourceRoot, XmlElement destRoot, int index)
        {
            bool modified = false;
            if (collection.Count > 0)
            {
                var sourceNode = document.CreateElement(name);
                sourceRoot.AppendChild(sourceNode);
                var destNode = document.CreateElement(name);
                destRoot.AppendChild(destNode);

                lock (collection)
                {
                    modified = true;
                    foreach (var item in collection)
                    {
                        var sourceItem = document.CreateElement("Nodes");
                        sourceItem.SetAttribute("key", index.ToString());
                        sourceItem.SetAttribute("value", item.Key);
                        sourceNode.AppendChild(sourceItem);

                        var destItem = document.CreateElement("Nodes");
                        destItem.SetAttribute("key", index.ToString());
                        destItem.SetAttribute("value", item.Value);
                        destNode.AppendChild(destItem);

                        index++;
                    }
                }
            }

            return modified;
        }

        public bool IsDefaultLoaded { get { return isDefaultLoaded; } }

        public virtual void LoadWrapperDefaultLanguageMapping(string languageMappingXmlFile)
        {
            throw new NotImplementedException();
        }

        protected abstract void LogMapping(string sourceName, string mappingName);
    }

    class BuiltInLanguageMapping : BaseLanguageMapping
    {
        /// <summary>
        /// load from xml
        /// </summary>
        private XmlLanguageMapping xmlLanguageMapping;

        protected string GetMappingItem(ref Dictionary<string, string> collection, Func<string, string> getMappingItem, string item)
        {
            string mappingItem = null;

            if (collection != null && collection.Count > 0)
            {
                lock (collection)
                {
                    collection.TryGetValue(item, out mappingItem);
                }
            }

            if (mappingItem == null)
            {
                if (getMappingItem != null)
                {
                    mappingItem = getMappingItem(item);
                }
                else
                {
                    mappingItem = item;
                }                
            }
            else
            {
                LogMapping(item, mappingItem);
            }

            return mappingItem;
        }

        protected override void LogMapping(string sourceName, string mappingName)
        {
            logger.Debug(WrapperResource.GetString(WrapperResourceKey.Wrapper_FindMappingName, sourceName, mappingName));
        }

        public override void LoadWrapperDefaultLanguageMapping(string languageMappingXmlFile)
        {
            if (!isDefaultLoaded)
            {
                lock (this)
                {
                    if (!isDefaultLoaded)
                    {
                        xmlLanguageMapping = new XmlLanguageMapping(languageMappingXmlFile, SourceLCID, DestLCID);                        
                        isDefaultLoaded = true;
                    }
                }
            }
        }

        public override string GetMappingContentTypeName(string contentTypeName)
        {
            if(xmlLanguageMapping != null)
            {
                return GetMappingItem(ref this.contentTypeNameMapping, xmlLanguageMapping.GetMappingContentTypeName, contentTypeName);
            }
            return base.GetMappingContentTypeName(contentTypeName);
        }

        public override string GetMappingFieldDisplayName(string fieldDisplayName)
        {
            if (xmlLanguageMapping != null)
            {
                return GetMappingItem(ref this.fieldDisplayNameMapping, xmlLanguageMapping.GetMappingFieldDisplayName, fieldDisplayName);
            }
            return base.GetMappingFieldDisplayName(fieldDisplayName);
        }

        public override string GetMappingGroupName(string title)
        {
            if (xmlLanguageMapping != null)
            {
                return GetMappingItem(ref this.groupNameMapping, xmlLanguageMapping.GetMappingGroupName, title);
            }
            return base.GetMappingGroupName(title);
        }

        public override string GetMappingListTitle(string listTitle)
        {
            if (xmlLanguageMapping != null)
            {
                return GetMappingItem(ref this.listTitleMapping, xmlLanguageMapping.GetMappingListTitle, listTitle);
            }
            return base.GetMappingListTitle(listTitle);
        }

        public override string GetMappingNavigationTitle(string navigationTitle)
        {
            if (xmlLanguageMapping != null)
            {
                return GetMappingItem(ref this.navigationTitleMapping, xmlLanguageMapping.GetMappingNavigationTitle, navigationTitle);
            }
            return base.GetMappingNavigationTitle(navigationTitle);
        }

        public override string GetMappingPermissionLevelName(string permissionLevelName)
        {
            if (xmlLanguageMapping != null)
            {
                return GetMappingItem(ref this.permissionLevelMapping, xmlLanguageMapping.GetMappingPermissionLevelName, permissionLevelName);
            }
            return base.GetMappingPermissionLevelName(permissionLevelName);
        }
    }

    class XmlLanguageMapping : BaseLanguageMapping
    {
        protected override void LogMapping(string sourceName, string mappingName)
        {
            logger.Debug(WrapperResource.GetString(WrapperResourceKey.Wrapper_FindMappingNameInXmlMapping, sourceName, mappingName));
        }

        internal XmlLanguageMapping(uint sourceLCID, uint destLCID)
        {
            this.SourceLCID = sourceLCID;
            this.DestLCID = destLCID;
        }

        public XmlLanguageMapping(string languageMappingXmlFile, uint sourceLCID, uint destLCID)
        {
            this.SourceLCID = sourceLCID;
            this.DestLCID = destLCID;

            Initialized(languageMappingXmlFile);
        }

        public override void LoadWrapperDefaultLanguageMapping(string languageMappingXmlFile)
        {
            Initialized(languageMappingXmlFile);
        }

        internal void Initialized(string file)
        {
            if(string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException("file");
            }
            else if(!File.Exists(file))
            {
                throw new FileNotFoundException(string.Empty, file);
            }

            var document = new XmlDocument();
            document.Load(file);

            XmlElement sourceNode = null;
            XmlElement destNode = null;

            foreach (XmlElement item in document.DocumentElement.ChildElements())
            {
                if (item.Name.Equals("Language", StringComparison.OrdinalIgnoreCase))
                {
                    var id = item.GetAttribute("id");

                    if (!string.IsNullOrEmpty(id))
                    {
                        if (id.Equals(SourceLCID.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            sourceNode = item;
                        }
                        else if (id.Equals(DestLCID.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            destNode = item;
                        }
                    }
                }

                if (sourceNode != null && destNode != null)
                {
                    break;
                }
            }

            if(sourceNode != null && destNode != null)
            {
                foreach(XmlElement item in sourceNode.ChildNodes)
                {
                    foreach(XmlElement dItem in destNode.ChildNodes)
                    {
                        if(item.Name.Equals(dItem.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            AddMapping(item, dItem);
                            break;
                        }
                    }
                }
            }
        }

        private void AddMapping(XmlElement sourceItem, XmlElement destItem)
        {
            if(sourceItem.Name.Equals("Lists", StringComparison.OrdinalIgnoreCase))
            {
                AddMapping(sourceItem, destItem, ref listTitleMapping);
            }
            else if (sourceItem.Name.Equals("Permissions", StringComparison.OrdinalIgnoreCase))
            {
                AddMapping(sourceItem, destItem, ref permissionLevelMapping);
            }
            else if (sourceItem.Name.Equals("Columns", StringComparison.OrdinalIgnoreCase))
            {
                AddMapping(sourceItem, destItem, ref fieldDisplayNameMapping);
            }
            else if (sourceItem.Name.Equals("ContentType", StringComparison.OrdinalIgnoreCase))
            {
                AddMapping(sourceItem, destItem, ref contentTypeNameMapping);
            }
            else if (sourceItem.Name.Equals("Navigation", StringComparison.OrdinalIgnoreCase))
            {
                AddMapping(sourceItem, destItem, ref navigationTitleMapping);
            }
        }

        private void AddMapping(XmlElement sourceItem, XmlElement destItem, ref Dictionary<string, string> collection)
        {
            var sourceItems = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach(XmlElement item in sourceItem.ChildNodes)
            {
                sourceItems[item.GetAttribute("key")] = item.GetAttribute("value");
            }

            var destItems = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (XmlElement item in destItem.ChildElements())
            {
                destItems[item.GetAttribute("key")] = item.GetAttribute("value");
            }

            if(collection == null)
            {
                lock(this)
                {
                    if(collection == null)
                    {
                        collection = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }
                }
            }

            lock (collection)
            {
                foreach (var item in sourceItems)
                {
                    string destValue = null;
                    if (destItems.TryGetValue(item.Key, out destValue))
                    {
                        collection[item.Value] = destValue;
                    }
                }
            }
        }
    }


    /// <summary>
    /// 可能存在的问题:
    /// 1. 源端是低版本，目的端是高版本的，这样language mapping匹配不上
    /// </summary>
    class BuiltInLanguageMappingController : ILanguageMappingController
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(BuiltInLanguageMappingController), false);

        private List<ILanguageMapping> languageMappings = new List<ILanguageMapping>(1);
        private List<string> resourceFiles = new List<string>();
        private WrapperSPMode spMode;

        public BuiltInLanguageMappingController()
        {
            IsDefaultLanguageMappingEnabled = false;
        }

        public BuiltInLanguageMappingController(WrapperSPMode spMode)
        {
            this.spMode = spMode;
        }

        public uint GetMappingLCID(uint originalLCID)
        {
            if (languageMappings != null && languageMappings.Count > 0)
            {
                lock (languageMappings)
                {
                    foreach (var mapping in languageMappings)
                    {
                        if (mapping.SourceLCID == originalLCID)
                        {
                            return mapping.DestLCID;
                        }
                    }
                }

                logger.Debug(WrapperResource.GetString(WrapperResourceKey.Wrapper_UsingOriginalLCID, originalLCID));
            }

            return originalLCID;
        }

        public ILanguageMapping GetLanguageMapping(uint originalLCID, uint currentLCID)
        {
            ILanguageMapping mapping = null;

            bool needLoadXml = false;

            lock (languageMappings)
            {
                foreach (var item in languageMappings)
                {
                    if (item.SourceLCID == originalLCID && item.DestLCID == currentLCID)
                    {
                        mapping = item;
                        break;
                    }
                }

                if (mapping == null && IsDefaultLanguageMappingEnabled)
                {
                    mapping = WrapperUtil.CreateEmptyLanguageMapping(originalLCID, currentLCID);
                    languageMappings.Add(mapping);
                }
            }
            
            if(mapping == null)
            {
                logger.Debug(WrapperResource.GetString(WrapperResourceKey.Wrapper_LanguageMappingNotFound, originalLCID, currentLCID));
            }
            else if (needLoadXml)
            {
                if (spMode == WrapperSPMode.O365)
                {
                     mapping.LoadWrapperDefaultLanguageMapping(Path.Combine(WrapperEnv.ProductRootFolder, Constants.Office365LanguageMappingFile));
                }
                else if (WrapperSPEnv.SPVersion >= WrapperSPEnv.SPVersionInternal.SharePoint2013)
                {
                    mapping.LoadWrapperDefaultLanguageMapping(Path.Combine(WrapperEnv.ProductRootFolder, Constants.SP2013LanguageMappingFile));
                } 
                else
                {
                    mapping.LoadWrapperDefaultLanguageMapping(Path.Combine(WrapperEnv.ProductRootFolder, Constants.SP2010LanguageMappingFile));
                }
            }

            return mapping;
        }

        public virtual void AddLanguageMapping(ILanguageMapping languageMapping)
        {
            lock(languageMappings)
            {
                foreach (var mapping in languageMappings)
                {
                    if (mapping.SourceLCID == languageMapping.SourceLCID)
                    {
                        throw new WrapperException(WrapperResource.GetString(WrapperResourceKey.Wrapper_DuplicatedLanguageMapping, languageMapping.SourceLCID, languageMapping.DestLCID), WrapperErrorCode.DuplicatedLanguageMapping);
                    }
                }

                languageMappings.Add(languageMapping);
            }

            if(languageMapping != null && (!languageMapping.IsDefaultLoaded))
            {
                if (spMode == WrapperSPMode.O365)
                {
                    languageMapping.LoadWrapperDefaultLanguageMapping(Path.Combine(WrapperEnv.ProductRootFolder, Constants.Office365LanguageMappingFile));
                }
                else if (WrapperSPEnv.SPVersion >= WrapperSPEnv.SPVersionInternal.SharePoint2013)
                {
                    languageMapping.LoadWrapperDefaultLanguageMapping(Path.Combine(WrapperEnv.ProductRootFolder, Constants.SP2013LanguageMappingFile));
                }
                else
                {
                    languageMapping.LoadWrapperDefaultLanguageMapping(Path.Combine(WrapperEnv.ProductRootFolder, Constants.SP2010LanguageMappingFile));
                }
            }
        }

        public bool IsDefaultLanguageMappingEnabled { get; set; }

        public string TemporaryDirectoryForSPResourceFile { get; set; }

        public void RestoreLanguageFile(Wrapper.Common.AveLanguageInfo languageInfo)
        {
            if ((!string.IsNullOrEmpty(TemporaryDirectoryForSPResourceFile)) && Directory.Exists(TemporaryDirectoryForSPResourceFile))
            {
                string fileName = string.Empty;
                try
                {
                    fileName = Path.Combine(TemporaryDirectoryForSPResourceFile, string.Concat(languageInfo.LanguageLCD, "src.resx"));

                    File.WriteAllText(fileName, languageInfo.LanguageContent, Encoding.UTF8);

                    lock(resourceFiles)
                    {
                        resourceFiles.Add(fileName);
                    }
                }
                catch(Exception ex)
                {
                    logger.Warn(WrapperResource.GetString(WrapperResourceKey.Wrapper_WriteResourceFileFailed, fileName, ex));
                }
            }
        }

        public void CleanLanguageFile()
        {
            if (resourceFiles.Count > 0)
            {
                lock (resourceFiles)
                {
                    foreach (var item in resourceFiles)
                    {
                        try
                        {
                            File.Delete(item);
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(WrapperResource.GetString(WrapperResourceKey.Wrapper_DeleteFileFailed, item, ex));
                        }
                    }
                }
            }
        }
    }
}
