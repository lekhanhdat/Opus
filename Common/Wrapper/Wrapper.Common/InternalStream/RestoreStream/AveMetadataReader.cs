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


//namespace AvePoint.Wrapper.Common
//{
//    using AvePoint.GCommon;
//    using System;
//    using System.Collections.Generic;
//    using System.IO;
//    using System.Linq;
//    using System.Reflection;
//    using System.Text;
//    using System.Xml;
//    [Obsolete("will be removed later")]
//    class AveMetadataReader: IAveMetadataReader
//    {
//        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
//        protected bool IsAllMetadataLoaded = false;
//        protected List<XmlElement> MetadataElementList { get; set; }
//        XmlReader internalReader { get; set; }
//        public AveMetadataReader(Stream stream)
//        {
//            var streamReader = new StreamReader(stream,Encoding.UTF8);
//            XmlReaderSettings setting = new XmlReaderSettings();
//            setting.IgnoreComments = true;
//            setting.IgnoreWhitespace = true;
//            setting.CloseInput = false;
//            internalReader = XmlReader.Create(streamReader, setting);
//        }

//        public AveMetadataReader(StreamReader streamReader)
//        {
//            XmlReaderSettings setting = new XmlReaderSettings();
//            setting.IgnoreComments = true;
//            setting.IgnoreWhitespace = true;
//            setting.CloseInput = false;
//            internalReader = XmlReader.Create(streamReader, setting);
//        }

//        private bool ReadNextMetadataElement()
//        {
//            bool isRead = false;
//            while (true)
//            {
//                if (internalReader.ReadState == ReadState.Initial)
//                {
//                    //Read data Node
//                    internalReader.Read();
//                    //Read first child element metadata
//                    internalReader.Read();
//                }
//                if (internalReader.NodeType == XmlNodeType.Element)
//                {
//                    string name = internalReader.GetAttribute(AveWrapperConstants.COLUMN_NAME);
//                    if (!string.IsNullOrEmpty(name) && Enum.IsDefined(typeof(AveMetadataType), name))
//                    {
//                        log.Info("Reader metadata by type :{0}", name);
//                        isRead = true;
//                        break;
//                    }
//                    if (!ShouldContinue())
//                    {
//                        break;
//                    }
//                    //this code should not go into
//                    //read next
//                    internalReader.Read();

//                    log.Warn("[NodeName:{0}][AttributeName:{1}][NodeType:{2}][AttributeCount:{3}]",
//                        internalReader.Name, name, internalReader.NodeType, internalReader.AttributeCount);

//                }
//                else
//                {
//                    if (!ShouldContinue())
//                    {
//                        break;
//                    }
//                    //read next
//                    internalReader.Read();
//                }


//            }
//            log.Info("IsMetadataReaded:{0}", isRead);
//            return isRead;
//        }

//        private bool ShouldContinue()
//        {
//            bool shouldContinue = true;
//            if (internalReader.ReadState == ReadState.EndOfFile || internalReader.ReadState == ReadState.Closed)
//            {
//                log.Info("Read end of xml reader V2.");
//                //read end of reader
//                shouldContinue = false;
//            }
//            if (internalReader.ReadState == ReadState.Error)
//            {
//                throw new XmlException("Internal reader state is Error.");
//            }
//            return shouldContinue;
//        }

//        protected void ReadAllMetadataElement(bool cacheMetadata=true)
//        {
//            if (!IsAllMetadataLoaded)
//            {
//                DateTime start = DateTime.Now;
//                log.Info("Begin to load all metadata");
//                StringBuilder elementBuilder = new StringBuilder();
//                elementBuilder.AppendLine("MetadataList");
//                if (MetadataElementList != null)
//                {
//                    MetadataElementList.Clear();
//                    MetadataElementList = null;
//                }
//                MetadataElementList = new List<XmlElement>();
//                while (ReadNextMetadataElement())
//                {
//                    var element = ReadCurrentMetadataElement();
//                    string name = element.GetAttribute(AveWrapperConstants.COLUMN_NAME);
//                    if (cacheMetadata)
//                    {
//                        elementBuilder.AppendLine(string.Format("[{0}][Cached]", name));
//                        MetadataElementList.Add(element);
//                    }
//                    else
//                    {
//                        elementBuilder.AppendLine(string.Format("[{0}][NoCache]", name));
//                    }
//                }
//                IsAllMetadataLoaded = true;
//                log.Info("Load all metadata complete.TimeCost:[{0}],Details:{1}", DateTime.Now - start,elementBuilder);
//            }
//            else
//            {
//                log.Info("Metadata already loaded previously.");
//            }
//        }

//        private XmlElement ReadCurrentMetadataElement()
//        { 
//            XmlDocument doc = new XmlDocument();
//            doc.LoadXml(internalReader.ReadOuterXml());
//            log.Info("Read current metadata element content,MetadataType:{0},Length:{1}.",doc.DocumentElement.GetAttribute(AveWrapperConstants.COLUMN_NAME),doc.DocumentElement.OuterXml.Length);
//            return doc.DocumentElement;
//        }


//        public AveMetadata ReadMetadata()
//        {
//            log.Info("Begin to read metadata");
//            if (IsAllMetadataLoaded)
//            {
//                if (MetadataElementList==null)
//                {
//                    log.Warn("IsAllMetadataLoaded is true, but MetadataElementList is null.");
//                    return null;
//                }
//                if (MetadataElementList.Count == 0)
//                {
//                    log.Warn("IsAllMetadataLoaded is true, but no metadata element exist in MetadataElementList.");
//                    return null;
//                }
//                var metadataElement = MetadataElementList.FirstOrDefault();               
//                MetadataElementList.RemoveAt(0);
//                if (metadataElement == null)
//                {
//                    log.Warn("IsAllMetadataLoaded is true, but metadata at first element in MetadataElementList is null.");
//                    return null;
//                }
//                log.Info("IsAllMetadataLoaded is true, Get Metadata successfully.MetadataType:{0}", metadataElement.GetAttribute(AveWrapperConstants.COLUMN_NAME));
//                return new AveMetadata(metadataElement);
//            }
//            else
//            {
//                if (ReadNextMetadataElement())
//                {
//                    XmlElement metadataElement = ReadCurrentMetadataElement();
//                    if (metadataElement != null)
//                    {
//                        log.Info("IsAllMetadataLoaded is false, Get Metadata successfully.MetadataType:{0}", metadataElement.GetAttribute(AveWrapperConstants.COLUMN_NAME));
//                        return new AveMetadata(metadataElement);
//                    }
//                    else
//                    {
//                        log.Warn("Metadata element is null.");
//                        return null;
//                    }
//                }
//                else
//                {
//                    log.Warn("Read next metadata failed,no metadata exist in reader any more.");
//                    return null;
//                }
//            }
//        }

//        public AveMetadata TryReadMetadata(AveMetadataType type)
//        {
//            ReadAllMetadataElement();
//            var element = MetadataElementList.Where(t => IsMetadataTypeMatch(type.ToString(), t)).FirstOrDefault();
//            if (element == null)
//            {
//                log.Warn("TryRead Metadata from MetadataElementList,no metadata with type {0} exist in MetadataElementList.", type);
//                return null;
//            }
//            log.Info("TryRead Metadata from MetadataElementList with type {0} successfully.", type);
//            return new AveMetadata(element);
//        }

//        private bool IsMetadataTypeMatch(string type, XmlElement element)
//        {
//            var attributeValue = element.GetAttribute(AveWrapperConstants.COLUMN_NAME);
//            if (!string.IsNullOrEmpty(attributeValue) && element!=null&& string.Equals(attributeValue, type, StringComparison.OrdinalIgnoreCase))
//            {
//                return true;
//            }
//            return false;
//        }

//        public List<AveMetadata> TryReadMetadataList(AveMetadataType type)
//        {
//            ReadAllMetadataElement();
//            var elements = MetadataElementList.Where(t => IsMetadataTypeMatch(type.ToString(), t));
//            if (elements == null)
//            {
//                log.Warn("TryReadMetadataList failed,no metadata with type {0} exist in MetadataElementList.", type);
//                return null;
//            }
//            var metadataList = new List<AveMetadata>();
//            foreach (var element in elements)
//            {
//                if (element != null)
//                {
//                    metadataList.Add(new AveMetadata(element));
//                }
//            }
//            log.Warn("TryReadMetadataList successfully,Type:{0},Count:{1}.", type,metadataList.Count);
//            return metadataList;
//        }

//        public void Dispose()
//        {
//            if(!IsAllMetadataLoaded)
//            {
//                ReadAllMetadataElement(false);
//            }
//            if (internalReader != null)
//            {
//                internalReader = null;
//            }
//            if (MetadataElementList != null)
//            {
//                MetadataElementList.Clear();
//                MetadataElementList = null;
//            }
//        }
//    }
//}
