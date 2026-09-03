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
using System.Text;
using AvePoint.GCommon;
using AvePoint.Common;

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
            wc.Opimized = this.Opimized;
            return wc;
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
                if (m_modelFactory == null || m_modelFactory.ContextKind == AveContextKind.ServerObjectModel
                    || m_modelFactory.ContextKind == AveContextKind.Server07ObjectModel)
                {
                    //Wrappe AveEnv in Wrapper later
                    return AveEnv.IsMoss;
                }
                else
                {
                    //need to change for BPOS
                    return true;
                }
            }
        }

        public void Dispose()
        {
            m_mappingManager.Dispose();
        }
    }
}
