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
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AvePoint.GCommon;

namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// Write project data to a xml file saved as content not metadata
    /// An example xml file referer to ProjectSerializerDataSample.xml
    /// If you want to read data stored in this xml, please use AveProjectReader.
    /// </summary>
    public class AveProjectWriter : IDisposable
    {


        private XmlWriter mInternalWriter;

        public AveProjectWriter(Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            this.mInternalWriter = XmlWriter.Create(stream, settings);
            StartWrite();
        }

        private void WriteDataCollection<T>(ICollection<T> items, string collectionNodeName, string nodeName) where T : class
        {
            this.mInternalWriter.WriteStartElement(collectionNodeName);
            try
            {
                if (items.Count == 0) return;

                foreach (var item in items)
                {
                    WriteData(item, nodeName);
                }
            }
            finally
            {
                this.mInternalWriter.WriteEndElement();
            }
        }

        private void WriteData<T>(T data, string nodeName) where T :class
        {
            this.mInternalWriter.WriteStartElement(nodeName);
            try
            {
                if (data != null)
                {
                    AveXmlSerializer.Serialize(this.mInternalWriter, null, data);
                }
            }
            finally
            {
                this.mInternalWriter.WriteEndElement();
            }
        }

        private void StartWrite()
        {
            this.mInternalWriter.WriteStartDocument(false);
            this.mInternalWriter.WriteStartElement(ProjectSerializerTag.PUBLISHEDPROJECT);
        }

        private void FinishWrite()
        {
            this.mInternalWriter.WriteEndElement();
            this.mInternalWriter.WriteEndDocument();
            this.mInternalWriter.Flush();
        }

        public void WritePublishedTasks(ICollection<AveProjectTaskInfo> tasks)
        {
            WriteDataCollection(tasks, ProjectSerializerTag.PUBLISHEDTASKS, ProjectSerializerTag.TASK);
        }

        public void WriteDraftProject(AveProjectInfo project)
        {
            WriteData(project, ProjectSerializerTag.DRAFTPROJECT);
        }

        public void WriteDraftTasks(ICollection<AveProjectTaskInfo> tasks)
        {
            WriteDataCollection(tasks, ProjectSerializerTag.DRAFTTASKS, ProjectSerializerTag.TASK);
        }

        public void WriteProjectCalendars(ICollection<AveProjectCalendarInfo> calendars)
        {
            WriteDataCollection(calendars, ProjectSerializerTag.CALENDARS, ProjectSerializerTag.CALENDAR);
        }

        public void WriteProjectLookupTables(ICollection<AveProjectLookupTableInfo> tables)
        {
            WriteDataCollection(tables, ProjectSerializerTag.LOOKUPTABLES, ProjectSerializerTag.LOOKUPTABLE);
        }

        public void WriteProjectCustomFields(ICollection<AveProjectCustomFieldInfo> fields)
        {
            WriteDataCollection(fields, ProjectSerializerTag.CUSTOMFIELDS, ProjectSerializerTag.CUSTOMFIELD);
        }

        public void WriteProjectEnterpriseResources(ICollection<AveProjectEnterpriseResourceInfo> resources)
        {
            WriteDataCollection(resources, ProjectSerializerTag.ENTERPRISERESOURCES, ProjectSerializerTag.ENTERPRISERESOURCE);
        }

        public void WriteProjectPhases(ICollection<AveProjectPhaseInfo> phases)
        {
            WriteDataCollection(phases, ProjectSerializerTag.PHASES, ProjectSerializerTag.PHASE);
        }

        public void WriteProjectStages(ICollection<AveProjectStageInfo> stages)
        {
            WriteDataCollection(stages, ProjectSerializerTag.STAGES, ProjectSerializerTag.STAGE);
        }

        public void WriteProjectEnterpriseTypes(ICollection<AveProjectEnterpriseProjectTypeInfo> types)
        {
            WriteDataCollection(types, ProjectSerializerTag.ENTERPRISEPROJECTTYPES, ProjectSerializerTag.ENTERPRISEPROJECTTYPE);
        }

        public void Dispose()
        {
            FinishWrite();
            this.mInternalWriter.Dispose();
        }
    }
}
