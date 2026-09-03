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
using AvePoint.GCommon;
using AvePoint.Common;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace AvePoint.Wrapper.Common
{
    public class WrapperContext : ICloneable, IDisposable
    {
        private Type m_LoggerType;
        private AveObjectModelFactory m_modelFactory;
        private AveMappingManager m_mappingManager = new AveMappingManager();

        public WrapperContext()
        {
            Opimized = true;
        }
        public Type LoggerType
        {
            get
            {
                return m_LoggerType;
            }
            set
            {
                //if (value.IsAssignableFrom(typeof(IAveLogger)))
                //{
                m_LoggerType = value;
                //}
                //else
                //{
                //  throw new ArgumentException("logger type must inherit IAveLogger");
                //}
            }
        }
        public string UserLoginName
        {
            get;
            set;
        }

        public bool Opimized
        {
            get;
            set;
        }

        public AveWrapperRunningAccountInfo SecurityTrimmingAccount
        {
            get;
            set;
        }

        public IAveLogger CreateLogger(Type instanceType)
        {
            if (m_LoggerType == null)
            {
                //default logger
                m_LoggerType = typeof(AveLogger);
            }
            return AveAssemblyUtility.CreateInstance(m_LoggerType, new Type[] { typeof(Type) }, new object[] { instanceType }) as IAveLogger;
        }

        public object Clone()
        {
            WrapperContext wc = new WrapperContext();
            wc.LoggerType = m_LoggerType;
            wc.Opimized = Opimized;

            wc.ModelFactory = AveObjectModelFactory.CloneAveObjectModelFactory();
            //wc.MappingManager = new AveMappingManager();
            return wc;
        }
        private static object DeepClone(object sourceObj)
        {
            object destinatonObj;
            Type objectType = sourceObj.GetType();
            if (objectType.IsValueType == true)
            {
                destinatonObj = sourceObj;
            }
            else
            {
                destinatonObj = Activator.CreateInstance(objectType);   //创建引用对象   
                MemberInfo[] memberCollection = sourceObj.GetType().GetMembers();

                foreach (MemberInfo member in memberCollection)
                {
                    if (member.MemberType == MemberTypes.Field)
                    {
                        FieldInfo field = (FieldInfo)member;
                        object fieldValue = field.GetValue(sourceObj);
                        if (fieldValue != null)
                        {
                            field.SetValue(destinatonObj, DeepClone(fieldValue));
                        }

                    }
                    else if (member.MemberType == MemberTypes.Property)
                    {
                        PropertyInfo myProperty = (PropertyInfo)member;
                        MethodInfo info = myProperty.GetSetMethod(false);
                        if (info != null)
                        {
                            object propertyValue = myProperty.GetValue(sourceObj, null);
                            myProperty.SetValue(destinatonObj, DeepClone(propertyValue), null);
                        }

                    }
                }
            }
            return destinatonObj;
        }

        public AveObjectModelFactory ModelFactory
        {
            get
            {
                return m_modelFactory;
            }
            set
            {
                m_modelFactory = value;
            }
        }

        public AveMappingManager MappingManager
        {
            get
            {
                return m_mappingManager;
            }
            set
            {
                m_mappingManager = value;
            }
        }

        public bool IsMoss
        {
            get
            {

                if (WrapperConfiguration.ForceFoundationModel)
                {
                    return false;

                }
                else
                {

                    if (m_modelFactory == null || m_modelFactory.ContextKind.IsServerMode())
                    {
                        //Wrapper AveEnv in Wrapper later
                        return AveEnv.IsMoss;
                    }
                    else
                    {
                        //need to change for BPOS
                        return true;
                    }

                }
            }
        }

        public bool BackupContentTypeDocumentTemplateFile = true;
        public bool RestoreManagedMetadataNavigation = false;
        public bool BackupWebpartPropertiesForOffice365 = true;

        public void Dispose()
        {
            m_mappingManager.Dispose();
        }

        public bool DisableWrapperReport { get; set; }
    }
}
