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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SnapshotList
    {
        private string mAgent = string.Empty;
        [DataMember]
        public string Agent
        {
            get
            {
                return mAgent;
            }
            set
            {
                mAgent = value;
            }
        }
        /// <summary>
        /// Server = AliasName[ServerInstance]
        /// </summary>
        private string mServer = string.Empty;
        [DataMember]
        public string Server
        {
            get { return mServer; }
            set { mServer = value; }
        }

        /// <summary>
        /// <para ServerInstance>It is not a alias name. Real ServerInstance Name.</para>
        /// </summary>
        private string mServerInstance = string.Empty;
        [DataMember]
        public string ServerInstance
        {
            get { return mServerInstance; }
            set { mServerInstance = value; }
        }
        //string:ServerInstance
        private SerializableDictionary<string, SnapshotItem> mSnapshotItems = new SerializableDictionary<string, SnapshotItem>();
        [DataMember]
        public SerializableDictionary<string, SnapshotItem> SnapshotItems
        {
            get { return mSnapshotItems; }
            set { mSnapshotItems = value; }
        }

        public void CopyTo(SnapshotList list)
        {
            list.Agent = Agent;
            list.Server = Server;
            list.ServerInstance = ServerInstance;
            foreach (string key in SnapshotItems.Keys)
            {
                list.SnapshotItems[key] = new SnapshotItem();
                SnapshotItems[key].CopyTo(list.SnapshotItems[key]);
            }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SnapshotItem
    {
        private string mServerInstance = string.Empty;
        [DataMember]
        public string ServerInstance
        {
            get { return mServerInstance; }
            set { mServerInstance = value; }
        }
        private string mUserName = string.Empty;
        [DataMember]
        public string UserName
        {
            get { return mUserName; }
            set { mUserName = value; }
        }
        private string mPassword = string.Empty;
        [DataMember]
        public string Password
        {
            get { return mPassword; }
            set { mPassword = value; }
        }
        private string mFullPath = string.Empty;
        [DataMember]
        public string FullPath
        {
            get { return mFullPath; }
            set { mFullPath = value; }
        }
        private string mKey = string.Empty;
        [DataMember]
        public string key
        {
            get { return mKey; }
            set { mKey = value; }
        }
        /// <summary>
        /// string1->DatabaseName + "|" + snapshotName 
        /// string2->Status
        /// </summary>
        /// 
        private SerializableDictionary<string, string> mSnapshotStatus = new SerializableDictionary<string, string>();
        [DataMember]
        public SerializableDictionary<string, string> SnapshotStatus
        {
            get { return mSnapshotStatus; }
            set { mSnapshotStatus = value; }
        }

        public void CopyTo(SnapshotItem item)
        {
            item.ServerInstance = ServerInstance;
            item.UserName = UserName;
            item.Password = Password;
            item.FullPath = FullPath;
            //item.ResultPath = ResultPath;
            item.key = key;
            foreach (string subKey in SnapshotStatus.Keys)
            {
                item.SnapshotStatus[subKey] = SnapshotStatus[subKey];
            }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class IndexSnapshotItem
    {
        private string mLocation = string.Empty;
        [DataMember]
        public string Location
        {
            get { return mLocation; }
            set { mLocation = value; }
        }
        private string mSnapshotName = string.Empty;
        [DataMember]
        public string SnapshotName
        {
            get { return mSnapshotName; }
            set { mSnapshotName = value; }
        }
        private string mStatus = string.Empty;
        [DataMember]
        public string Status
        {
            get { return mStatus; }
            set { mStatus = value; }
        }
        private string mFullPath = string.Empty;
        [DataMember]
        public string FullPath
        {
            get { return mFullPath; }
            set { mFullPath = value; }
        }
        private bool mCheckStatus = false;
        [DataMember]
        public bool checkStatus
        {
            get { return mCheckStatus; }
            set { mCheckStatus = value; }
        }

        public void CopyTo(IndexSnapshotItem item)
        {
            item.Location = Location;
            item.SnapshotName = SnapshotName;
            item.Status = Status;
            item.FullPath = FullPath;
            item.checkStatus = checkStatus;
            //item.ResultPath = ResultPath;
        }
    }

    [XmlRoot("dictionary"), Serializable]
    public class SerializableDictionary<TKey, TValue>
        : Dictionary<TKey, TValue>, IXmlSerializable
    {

        public SerializableDictionary()
            : base()
        {

        }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary)
            : base(dictionary)
        {

        }


        public SerializableDictionary(IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
        }


        public SerializableDictionary(int capacity)
            : base(capacity)
        {

        }

        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer)
            : base(capacity, comparer)
        {

        }

        protected SerializableDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");
                reader.ReadStartElement("key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();
                reader.ReadStartElement("value");
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();
                this.Add(key, value);
                reader.ReadEndElement();
                reader.MoveToContent();

            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));
            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item");
                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();
                writer.WriteStartElement("value");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }
    }

    public class IndexSnapshotList
    {
        private string mAgent = string.Empty;
        public string Agent
        {
            get { return mAgent; }
            set { mAgent = value; }
        }
        //string:SnapshotName
        private SerializableDictionary<string, IndexSnapshotItem> mIndexSnapshotItems = new SerializableDictionary<string, IndexSnapshotItem>();
        public SerializableDictionary<string, IndexSnapshotItem> IndexSnapshotItems
        {
            get { return mIndexSnapshotItems; }
            set { mIndexSnapshotItems = value; }
        }
        public void CopyTo(IndexSnapshotList list)
        {
            list.Agent = Agent;
            foreach (string key in IndexSnapshotItems.Keys)
            {
                list.IndexSnapshotItems[key] = new IndexSnapshotItem();
                IndexSnapshotItems[key].CopyTo(list.IndexSnapshotItems[key]);
            }
        }
    }
}
