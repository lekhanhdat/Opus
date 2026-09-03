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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Compress;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Activities.DurableInstancing;
using System.Collections.Generic;
using System.IO;
using System.Runtime.DurableInstancing;
using System.Runtime.Serialization;
using System.Workflow.Runtime.Hosting;
using System.Xml;

namespace AvePoint.RA.Workflow.Common
{
    public class CustomSQLWorkflowInstanceStore : InstanceStore
    {
        private IWorkflowDataDao WorkflowDataDao {
            get 
            {
                return (IWorkflowDataDao)PlatformWindsorManager.GetService(typeof(IWorkflowDataDao));
            }
        } 

        private Guid ownerInstanceID;

        //private ThreadSetting threadSetting;

        public CustomSQLWorkflowInstanceStore(Guid id)
        {
            ownerInstanceID = id;
            //threadSetting = ThreadSetting.GetSetting();
        }

        //Synchronous version of the Begin/EndTryCommand functions
        protected override bool TryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout)
        {
            return EndTryCommand(BeginTryCommand(context, command, timeout, null, null));
        }

        //The persistence engine will send a variety of commands to the configured InstanceStore,
        //such as CreateWorkflowOwnerCommand, SaveWorkflowCommand, and LoadWorkflowCommand.
        //This method is where we will handle those commands
        protected override IAsyncResult BeginTryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
        {
            //ThreadSetting.SetSetting(threadSetting);

            IDictionary<System.Xml.Linq.XName, InstanceValue> data = null;

            //The CreateWorkflowOwner command instructs the instance store to create a new instance owner bound to the instanace handle
            if (command is CreateWorkflowOwnerCommand)
            {
                context.BindInstanceOwner(ownerInstanceID, Guid.NewGuid());
            }
            else if (command is DeleteWorkflowOwnerCommand)
            {
            }
            //The SaveWorkflow command instructs the instance store to modify the instance bound to the instance handle or an instance key
            else if (command is SaveWorkflowCommand)
            {
                SaveWorkflowCommand saveCommand = (SaveWorkflowCommand)command;
                data = saveCommand.InstanceData;

                Save(data);
            }
            //The LoadWorkflow command instructs the instance store to lock and load the instance bound to the identifier in the instance handle
            else if (command is LoadWorkflowCommand)
            {
                try
                {
                    var xml = LoadFromDB();

                    data = LoadInstanceData(xml);
                    //load the data into the persistence Context
                    context.LoadedInstance(InstanceState.Initialized, data, null, null, null);
                }
                catch (Exception exception)
                {
                    throw new PersistenceException(exception.Message);
                }
            }
            

            return new CompletedAsyncResult<bool>(true, callback, state);
        }

        protected override bool EndTryCommand(IAsyncResult result)
        {
            return CompletedAsyncResult<bool>.End(result);
        }

        //Reads data from xml file and creates a dictionary based off of that.
        IDictionary<System.Xml.Linq.XName, InstanceValue> LoadInstanceData(string xmlStr)
        {
            IDictionary<System.Xml.Linq.XName, InstanceValue> data = new Dictionary<System.Xml.Linq.XName, InstanceValue>();

            NetDataContractSerializer s = new NetDataContractSerializer();
            
            //XmlReader rdr = XmlReader.Create(inputStream);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlStr);


            XmlNodeList instances = doc.GetElementsByTagName("InstanceValue");
            foreach (XmlElement instanceElement in instances)
            {
                XmlElement keyElement = (XmlElement)instanceElement.SelectSingleNode("descendant::key");
                System.Xml.Linq.XName key = (System.Xml.Linq.XName)DeserializeObject(s, keyElement);

                XmlElement valueElement = (XmlElement)instanceElement.SelectSingleNode("descendant::value");
                object value = DeserializeObject(s, valueElement);
                InstanceValue instVal = new InstanceValue(value);

                data.Add(key, instVal);
            }

            return data;
        }

        object DeserializeObject(NetDataContractSerializer serializer, XmlElement element)
        {
            object deserializedObject = null;

            MemoryStream stm = new MemoryStream();
            XmlDictionaryWriter wtr = XmlDictionaryWriter.CreateTextWriter(stm);
            element.WriteContentTo(wtr);
            wtr.Flush();
            stm.Position = 0;

            deserializedObject = serializer.Deserialize(stm);

            return deserializedObject;
        }

        //Saves the persistance data to an xml file.
        void Save(IDictionary<System.Xml.Linq.XName, InstanceValue> instanceData)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<InstanceValues/>");

            foreach (KeyValuePair<System.Xml.Linq.XName, InstanceValue> valPair in instanceData)
            {
                XmlElement newInstance = doc.CreateElement("InstanceValue");

                XmlElement newKey = SerializeObject("key", valPair.Key, doc);
                newInstance.AppendChild(newKey);

                XmlElement newValue = SerializeObject("value", valPair.Value.Value, doc);
                newInstance.AppendChild(newValue);

                doc.DocumentElement.AppendChild(newInstance);
            }

            Save2DB(doc.OuterXml);
        }

        
        private void Save2DB(string xmlStr)
        {
            var entity = new RMWorkflowData()
            {
                Id = ownerInstanceID,
                BinaryContent = DeflateStreamUtil.Compress(xmlStr)
            };
            WorkflowDataDao.Save(entity);
        }

        private string LoadFromDB()
        {
            var entity = WorkflowDataDao.GetById(ownerInstanceID);
            return DeflateStreamUtil.Decompress(entity.BinaryContent);
        }

        XmlElement SerializeObject(string elementName, object o, XmlDocument doc)
        {
            NetDataContractSerializer s = new NetDataContractSerializer();

            XmlElement newElement = doc.CreateElement(elementName);
            MemoryStream stm = new MemoryStream();

            s.Serialize(stm, o);
            stm.Position = 0;
            StreamReader rdr = new StreamReader(stm);
            newElement.InnerXml = rdr.ReadToEnd();

            return newElement;
        }
    }
}
