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
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public abstract class AveSPEventReceiver
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected AveMetadataType mType;
        protected IAveBackupStream mStream;

        public static AveSPEventReceiver CreateInstance(object obj)
        {
            string type = obj.GetType().Name;
            AveSPEventReceiver instance;
            switch (type)
            {
                case "AveSPWeb":
                    instance = new AveSPWebEventReceiver((AveSPWeb)obj);
                    break;
                case "AveSPList":
                    instance = new AveSPListEventReceiver((AveSPList)obj);
                    break;
                default:
                    throw new Exception("Cannot construct a instance for this object type: " + obj.GetType().ToString());
            }
            return instance;
        }

        public string Export()
        {
            return AveConvert.ConvertAveObjToAveXml(mType.ToString(), GetReceivers());
        }

        public void Export(IAveBackupStream stream)
        {
            stream.WriteMetadata(mType, GetReceivers());
        }

        protected List<AveEventReceiverInfo> ConvertToList(IAveEventReceiverDefinitionCollection receiverDefinitions)
        {
            List<AveEventReceiverInfo> list = new List<AveEventReceiverInfo>();
            foreach (IAveEventReceiverDefinition receiverDefinition in receiverDefinitions)
            {
                try
                {
                    //不备份DocAve的event receiver
                    if (receiverDefinition.Assembly.StartsWith("DocAve", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    AveEventReceiverInfo receiverInfo = new AveEventReceiverInfo()
                    {
                        ClassName = receiverDefinition.Class,
                        Type = (int)receiverDefinition.Type,
                        AssemblyInfo = receiverDefinition.Assembly,
                        Name = receiverDefinition.Name
                    };
                    list.Add(receiverInfo);
                }
                catch (Exception e)
                {
                    mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while convert eventReceiverDefinition to list. receiverDefinition:{0} \n error message:{1}", receiverDefinition.Name, e));
                }
            }
            return list;
        }

        public abstract List<AveEventReceiverInfo> GetReceivers();
    }

    public class AveSPWebEventReceiver : AveSPEventReceiver
    {
        private AveSPWeb mAveSPWeb;

        public AveSPWebEventReceiver(AveSPWeb _web)
        {
            mAveSPWeb = _web;
            mType = AveMetadataType.WebEventReceiver;
            mStream = _web.Sender;
        }

        public override List<AveEventReceiverInfo> GetReceivers()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.GetReceivers"))
            {
                IAveEventReceiverDefinitionCollection eventReceivers = mAveSPWeb.SPWeb.EventReceivers;
                if (eventReceivers != null)
                {
                    return ConvertToList(eventReceivers);
                }
                return null;
                //return ConvertToList(mAveSPWeb.SPWeb.EventReceivers);
            }
        }
    }

    public class AveSPListEventReceiver : AveSPEventReceiver
    {
        private AveSPList mAveSPList;

        public AveSPListEventReceiver(AveSPList _list)
        {
            mAveSPList = _list;
            mType = AveMetadataType.ListEventReceiver;
            mStream = _list.Sender;
        }

        public override List<AveEventReceiverInfo> GetReceivers()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.GetReceivers"))
            {
                if (mAveSPList.SPList.EventReceivers != null)
                {
                    return ConvertToList(mAveSPList.SPList.EventReceivers);
                }
                return null;
            }
        }
    }
}