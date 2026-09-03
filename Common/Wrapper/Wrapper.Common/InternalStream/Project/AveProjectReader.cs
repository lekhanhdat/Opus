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
using System.Reflection;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AvePoint.GCommon;
using System.Configuration;
using System.Xml.Schema;

namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// Read data from a xml file which written by AveProjectWriter
    /// An example xml file referer to ProjectSerializerDataSample.xml
    /// If you want to use this class read data, please make sure that xml file is written by AveProjectWriter
    /// Please note that because stream is not seekable, data will be read by sequence
    /// if you skip one node, there will be an exception thrown by FindNode method.
    /// </summary>
    public class AveProjectReader : IDisposable
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private XmlReader mInternalReader;

        public delegate int FindMember(int oringinalId);

        public event FindMember FindEvent;

        public AveProjectReader(IAveRestoreStream stream)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
            var fileStream = new AveSPFileStream(stream);
            mInternalReader = XmlReader.Create(fileStream, settings);
        }

        private static void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            // 处理验证错误  
            throw new XmlSchemaValidationException($"Validation error: {e.Message}", e.Exception);
        }

        private Stream LoadSingleNode(string endNodeName)
        {
            MemoryStream stream = new MemoryStream();
            XmlWriter writer = XmlWriter.Create(stream);
            writer.WriteStartDocument(false);
            while (this.mInternalReader.Read())
            {
                switch (this.mInternalReader.NodeType)
                {
                    case XmlNodeType.Element:
                        WriteStartElement(writer);
                        break;
                    case XmlNodeType.Text:
                        writer.WriteString(this.mInternalReader.Value);
                        break;
                    case XmlNodeType.EndElement:
                        if (string.Equals(this.mInternalReader.Name, endNodeName, StringComparison.OrdinalIgnoreCase))
                        {
                            writer.WriteEndDocument();
                            writer.Flush();
                            stream.Seek(0, SeekOrigin.Begin);
                            return stream;
                        }
                        writer.WriteEndElement();
                        break;
                    default:
                        break;
                }
            }

            return stream;
        }

        private void WriteStartElement(XmlWriter writer)
        {
            writer.WriteStartElement(this.mInternalReader.Name);
            if (this.mInternalReader.HasAttributes)
            {
                for (int i = 0; i < this.mInternalReader.AttributeCount; i++)
                {
                    this.mInternalReader.MoveToAttribute(i);
                    writer.WriteAttributeString(this.mInternalReader.Name, this.mInternalReader.Value);
                }
                this.mInternalReader.MoveToElement();
            }
            if (this.mInternalReader.IsEmptyElement)
            {
                writer.WriteEndElement();
            }
        }

        private T CreateNodeInstance<T>(Stream content) where T : class
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(content);
                return (T)AveXmlSerializer.Deserialize(doc.DocumentElement);
            }
            catch (Exception ex)
            {
                mLog.Error("Create instance of Type:[{0}] failed. Error:{1}", typeof(T).ToString(), ex);
            }
            finally
            {
                content.Dispose();
            }

            return default(T);
        }

        private bool FindNode(string nodeName)
        {
            while (this.mInternalReader.Read())
            {
                switch (this.mInternalReader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (string.Equals(this.mInternalReader.Name, nodeName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                        break;
                    case XmlNodeType.XmlDeclaration:
                    case XmlNodeType.EndElement:
                        break;
                    default:
                        throw new Exception("Missing Project Data!");
                }
            }

            return false;
        }

        public List<T> CreateNodeInstances<T>(string collectionNodeName, string nodeName) where T : class
        {
            List<T> collection = new List<T>();
            bool endOfNode = false;
            if (FindNode(collectionNodeName))
            {
                while (this.mInternalReader.Read())
                {
                    switch (this.mInternalReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (string.Equals(this.mInternalReader.Name, nodeName, StringComparison.OrdinalIgnoreCase))
                            {
                                var stream = LoadSingleNode(nodeName);
                                var instance = CreateNodeInstance<T>(stream);
                                if (instance != default(T))
                                {
                                    collection.Add(instance);
                                }
                            }
                            break;
                        case XmlNodeType.EndElement:
                            if (string.Equals(this.mInternalReader.Name, collectionNodeName, StringComparison.OrdinalIgnoreCase))
                            {
                                endOfNode = true;
                            }
                            break;
                        default:
                            break;
                    }
                    if (endOfNode)
                    {
                        break;
                    }
                }
            }

            return collection;
        }

        public List<AveProjectEnterpriseProjectTypeInfo> GetProjectEnterpriseProjectTypes()
        {
            try
            {
                return CreateNodeInstances<AveProjectEnterpriseProjectTypeInfo>(ProjectSerializerTag.ENTERPRISEPROJECTTYPES, ProjectSerializerTag.ENTERPRISEPROJECTTYPE);
            }
            catch (Exception ex)
            {
                mLog.Warn("Cannot get enterprise project type data. Error:{0}", ex);
            }

            return new List<AveProjectEnterpriseProjectTypeInfo>();
        }

        public List<AveProjectCalendarInfo> GetProjectCalendars()
        {
            try
            {
                return CreateNodeInstances<AveProjectCalendarInfo>(ProjectSerializerTag.CALENDARS, ProjectSerializerTag.CALENDAR);
            }
            catch (Exception ex)
            {
                mLog.Warn("Cannot get project calendar data. Error:{0}", ex);
            }

            return new List<AveProjectCalendarInfo>();
        }

        public List<AveProjectLookupTableInfo> GetProjectLookupTables()
        {
            try
            {
                return CreateNodeInstances<AveProjectLookupTableInfo>(ProjectSerializerTag.LOOKUPTABLES, ProjectSerializerTag.LOOKUPTABLE);
            }
            catch (Exception ex)
            {
                mLog.Warn("Cannot get project lookup table data. Error:{0}", ex);
            }

            return new List<AveProjectLookupTableInfo>();
        }

        public List<AveProjectCustomFieldInfo> GetProjectCustomFields()
        {
            try
            {
                return CreateNodeInstances<AveProjectCustomFieldInfo>(ProjectSerializerTag.CUSTOMFIELDS, ProjectSerializerTag.CUSTOMFIELD);
            }
            catch (Exception ex)
            {
                mLog.Warn("Cannot get project custom field data. Error:{0}", ex);
            }

            return new List<AveProjectCustomFieldInfo>();
        }

        public List<AveProjectEnterpriseResourceInfo> GetProjectEnterpriseResources()
        {
            try
            {
                return CreateNodeInstances<AveProjectEnterpriseResourceInfo>(ProjectSerializerTag.ENTERPRISERESOURCES, ProjectSerializerTag.ENTERPRISERESOURCE);
            }
            catch (Exception ex)
            {
                mLog.Warn("Cannot get project custom field data. Error:{0}", ex);
            }

            return new List<AveProjectEnterpriseResourceInfo>();
        }

        public List<AveProjectPhaseInfo> GetProjectPhases()
        {
            try
            {
                return CreateNodeInstances<AveProjectPhaseInfo>(ProjectSerializerTag.PHASES, ProjectSerializerTag.PHASE);
            }
            catch (Exception ex)
            {
                mLog.Warn("Cannot get project phase data. Error:{0}", ex);
            }

            return new List<AveProjectPhaseInfo>();
        }

        public List<AveProjectStageInfo> GetProjectStages()
        {
            try
            {
                return CreateNodeInstances<AveProjectStageInfo>(ProjectSerializerTag.STAGES, ProjectSerializerTag.STAGE);
            }
            catch (Exception ex)
            {
                mLog.Warn("Cannot get project stage data. Error:{0}", ex);
            }

            return new List<AveProjectStageInfo>();
        }

        public List<AveProjectTaskInfo> GetPublishedTasks()
        {
            try
            {
                return CreateNodeInstances<AveProjectTaskInfo>(ProjectSerializerTag.PUBLISHEDTASKS, ProjectSerializerTag.TASK);
            }
            catch (Exception ex)
            {
                mLog.Warn("Cannot get published tasks data. Error:{0}", ex);
            }

            return new List<AveProjectTaskInfo>();
        }

        public AveProjectInfo GetDraftProject()
        {
            try
            {
                if (FindNode(ProjectSerializerTag.DRAFTPROJECT))
                {
                    var stream = LoadSingleNode(ProjectSerializerTag.DRAFTPROJECT);
                    return CreateNodeInstance<AveProjectInfo>(stream);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Cannot get draft project data. Error:{0}", ex);
            }
            return null;
        }

        public List<AveProjectTaskInfo> GetDraftTasks()
        {
            try
            {
                return CreateNodeInstances<AveProjectTaskInfo>(ProjectSerializerTag.DRAFTTASKS, ProjectSerializerTag.TASK);
            }
            catch (Exception ex)
            {
                mLog.Warn("Cannot get draft tasks data. Error:{0}", ex);
            }

            return new List<AveProjectTaskInfo>();
        }

        public int FindUser(int originalId)
        {
            if (FindEvent != null)
            {
                return FindEvent(originalId);
            }
            return 0;
        }

        public void Dispose()
        {
            this.mInternalReader.Dispose();
        }
    }
}
