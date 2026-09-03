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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using System.Xml;
using System.IO;

namespace AvePoint.ObjectModel.Server13
{
    abstract class AvePersistedObjectCollection<T> : IAvePersistedObjectCollection<T>, IEnumerable<T>, IEnumerable where T : IAvePersistedObject
    {
        protected IEnumerable mPersistedObjectCollection;

        public AvePersistedObjectCollection(IEnumerable persistedObjectCollection)
        {
            mPersistedObjectCollection = persistedObjectCollection;
        }

        public AvePersistedObjectCollection()
        { }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator<T>(this);
        }

        public abstract int Count
        {
            get;
        }

        public U GetValue<U>() where U : IAvePersistedObject
        {
            Type genericType = GetGenericType(typeof(U));
            if (genericType == null)
            {
                return default(U);
            }
            object obj = AveAssemblyUtility.InvokeGenericMethod(mPersistedObjectCollection, "GetValue", new Type[] { }, new object[] { }, new Type[] { genericType });
            if (obj == null)
            {
                return default(U);
            }
            return (U)CreateElementInstance(typeof(U), obj);
        }

        public U GetValue<U>(Guid id) where U : IAvePersistedObject
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException("id");
            }
            Type genericType = GetGenericType(typeof(U));
            if (genericType == null)
            {
                return default(U);
            }
            object obj = AveAssemblyUtility.InvokeGenericMethod(mPersistedObjectCollection, "GetValue", new Type[] { typeof(Guid) }, new object[] { id }, new Type[] { genericType });
            if (obj == null)
            {
                return default(U);
            }
            return (U)CreateElementInstance(typeof(U), obj);
        }

        public U GetValue<U>(string name) where U : IAvePersistedObject
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            Type genericType = GetGenericType(typeof(U));
            if (genericType == null)
            {
                return default(U);
            }
            object obj = AveAssemblyUtility.InvokeGenericMethod(mPersistedObjectCollection, "GetValue", new Type[] { typeof(string) }, new object[] { name }, new Type[] { genericType });
            if (obj == null)
            {
                return default(U);
            }
            return (U)CreateElementInstance(typeof(U), obj);
        }

        public virtual T this[Guid id]
        {
            get
            {
                return GetValue<T>(id);
            }
        }

        public virtual T this[string name]
        {
            get
            {
                return GetValue<T>(name);
            }
        }

        internal virtual object CreateElementInstance(Type genericType, object persistedObject)
        {
            return AveServerAssemblyInit.CreateElement(genericType, persistedObject);
        }

        internal Type GetGenericType(Type aveType)
        {
            string typeMapping = string.Empty;
            typeMapping = XmlConfiguration.GetTypeMapping(aveType.Name);
            return AveAssemblyUtility.GetGenerticType(aveType, typeMapping);
        }

        private class Enumerator<C> : IEnumerator<C>, IDisposable, IEnumerable where C : IAvePersistedObject
        {
            private IEnumerator mEnumerator;
            private AvePersistedObjectCollection<C> mPersistedObjects;

            public Enumerator(AvePersistedObjectCollection<C> persistedObjects)
            {
                mPersistedObjects = persistedObjects;
                mEnumerator = persistedObjects.mPersistedObjectCollection.GetEnumerator();
            }

            public C Current
            {
                get
                {
                    object obj = mPersistedObjects.CreateElementInstance(typeof(C), mEnumerator.Current);
                    return (C)obj;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            public bool MoveNext()
            {
                return mEnumerator.MoveNext();
            }

            public void Reset()
            {
                mEnumerator.Reset();
            }

            public void Dispose()
            { }

            public IEnumerator GetEnumerator()
            {
                return this;
            }
        }
    }

    class XmlConfiguration
    {
        private const string CONFIG_FILE_NAME = "AvePoint.ObjectModel.Server13.ServerConfig.xml";
        private static XmlDocument mXmlDocument = new XmlDocument();
        private static XmlNode mRootNode;
        private static Dictionary<string, string> mConfigTypeMapping = new Dictionary<string, string>();

        static XmlConfiguration()
        {
            InitXmlConifgMapping();
        }

        public static void InitXmlConifgMapping()
        {
            mRootNode = LoadConfigFile();
            lock (mConfigTypeMapping)
            {
                mConfigTypeMapping = LoadConfigTypeMapping();
            }
        }

        private static XmlNode LoadConfigFile()
        {
            using (StreamReader sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(CONFIG_FILE_NAME)))
            {
                mXmlDocument.LoadXml(sr.ReadToEnd());
            }
            XmlNode rootNode = mXmlDocument.DocumentElement.FirstChild;
            if (ConfigurationConstants.ROOT_NODE.Equals(rootNode.Name))
            {
                return rootNode;
            }
            else
            {
                throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Server_ConfigurationFileIllegal);
            }
        }

        private static Dictionary<string, string> LoadConfigTypeMapping()
        {
            Dictionary<string, string> configTypeMapping = new Dictionary<string, string>();
            foreach (XmlNode node in mRootNode.ChildNodes)
            {
                if (ConfigurationConstants.NODE_SERVER_GENERIC_MAPPINGS.Equals(node.Name))
                {
                    foreach (XmlNode mappingNode in node.ChildNodes)
                    {
                        if (ConfigurationConstants.NODE_SERVER_GENERIC_MAPPING.Equals(mappingNode.Name))
                        {
                            string targetType = mappingNode.Attributes[ConfigurationConstants.ATTRIBUTE_TARGETTYPE].Value;
                            string proxyType = mappingNode.Attributes[ConfigurationConstants.ATTRIBUTE_PROXYTYPE].Value;
                            configTypeMapping.Add(proxyType, targetType);
                        }
                    }
                }
            }

            return configTypeMapping;
        }

        public static string GetTypeMapping(string proxyType)
        {
            if (mConfigTypeMapping.ContainsKey(proxyType))
            {
                return mConfigTypeMapping[proxyType];
            }
            return string.Empty;
        }

        class ConfigurationConstants
        {
            internal const string ROOT_NODE = "ServerConfigTypeMapping";
            internal const string NODE_SERVER_GENERIC_MAPPINGS = "AveServerGenericTypeMappings";
            internal const string NODE_SERVER_GENERIC_MAPPING = "GenericTypeMapping";

            internal const string ATTRIBUTE_TARGETTYPE = "targetType";
            internal const string ATTRIBUTE_PROXYTYPE = "proxyType";

            internal const string NAMESPACE_SEPARETOR = ".";
            internal const string ASSEMBLY_FILE_EXTENSION = ".dll";
        }
    }
}
