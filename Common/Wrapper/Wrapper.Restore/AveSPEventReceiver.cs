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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.Wrapper.Restore
{
    public class AveSPEventReceiverConfig
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(AveSPEventReceiver));

        private static IAveItemEventReceiver ItemEventReceiver;

        //private static readonly AveObjectModelFactory ObjectModelFactory = AveObjectModelFactory.CreateObjectModelFactory(string.Empty, null, AveContextKind.Auto);

        //static AveSPEventReceiverConfig()
        //{
        //    if (ObjectModelFactory != null)
        //    {
        //        ItemEventReceiver = ObjectModelFactory.CreateItemEventReceiver();
        //    }
        //}

        public static void InitEventReceiver(AveObjectModelFactory objectModelFactory)
        {
            ItemEventReceiver = objectModelFactory.CreateItemEventReceiver();
        }

        private AveSPEventReceiverConfig() { }

        public static void InitItemEventReceiver(AveContextKind contextKind)
        {
            //mItemEventReceiver = AveObjectModelFactory.CreateItemEventReceiver(contextKind);
        }

        public static void DisableEventReceiver()
        {
            try
            {
                if (ItemEventReceiver != null)
                {
                    ItemEventReceiver.EventFiringEnabled = false;
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, string.Format("Cannot disable event receiver in current thread. \n error message:{0}", ex));
                //mLog.Warn(string.Format("cannot disable event receiver in current thread.\r\nException:{0}", ex.ToString()));
            }
        }

        public static void EnableEventReceiver()
        {
            try
            {
                if (ItemEventReceiver != null)
                {
                    ItemEventReceiver.EventFiringEnabled = true;
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, string.Format("Cannot enable event receiver in current thread. \n error message:{0}", ex));
                //mLog.Warn(string.Format("cannot enable event receiver in current thread.\r\nException:{0}", ex.ToString()));
            }
        }

        public static Nullable<bool> EventReceiverEnabled
        {
            get
            {
                try
                {
                    return ItemEventReceiver.EventFiringEnabled;
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.WARN, string.Format("Cannot get event receiver enabled in current thread. \n error message:{0}", ex));
                    return null;
                }
            }
        }
    }

    public abstract class AveSPEventReceiver: IDisposable
    {
        protected static readonly AveLogger Logger = AveLogger.GetInstance(typeof(AveSPEventReceiver));

        protected IReport report = new AveWrapperReport();

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
                    throw new Exception("Cannot construct a instance for this object type: " + obj.GetType());
            }
            return instance;
        }

        public abstract void RestoreEventReceivers(List<AveEventReceiverInfo> aveEventReceivers);

        public bool FindAndNeedUpdate(List<AveEventReceiverInfo> aveEventReceivers, IAveEventReceiverDefinition reciever, ref string name)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPEventReceiver.FindAndNeedUpdate"))
            {
#endif
                foreach (AveEventReceiverInfo receiverInfo in aveEventReceivers)
                {
                    if (receiverInfo.AssemblyInfo.Equals(reciever.Assembly) && receiverInfo.ClassName.Equals(reciever.Class) && receiverInfo.Type == (int)(reciever.Type))
                    {
                        if (reciever.Name != receiverInfo.Name)
                        {
                            name = receiverInfo.Name;
                            return true;
                        }

                    }
                }
                return false;
#if PerformanceLog
            }
#endif
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SPWorkflowAutostartEventReceiver is a class")]
        protected void AddEventReceivers(IAveEventReceiverDefinitionCollection spEventReceivers, List<AveEventReceiverInfo> aveEventReceivers)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebEventReceiver.AddEventReceivers"))
            {
#endif
            foreach (AveEventReceiverInfo receiver in aveEventReceivers)
            {
                try
                {
                    bool isExist = false;
                    if (spEventReceivers != null)
                    {
                        foreach (IAveEventReceiverDefinition spReceiver in spEventReceivers)
                        {
                            if (receiver.AssemblyInfo.Equals(spReceiver.Assembly)
                             && receiver.ClassName.Equals(spReceiver.Class)
                             && receiver.Name.Equals(spReceiver.Name)
                             && receiver.Type == (int)spReceiver.Type)
                            {
                                isExist = true;
                                break;
                            }
                        }
                    }
                    if (!isExist)
                    {
                        if (!string.Equals(receiver.ClassName, "Microsoft.SharePoint.Workflow.SPWorkflowAutostartEventReceiver", StringComparison.OrdinalIgnoreCase))
                        {
                            spEventReceivers?.Add((AveEventReceiverType)receiver.Type, receiver.AssemblyInfo, receiver.ClassName, receiver.Name);
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    Logger.Log(AveLogLevel.WARN, string.Format("An error occurred while add event receiver. \n error message:{0}", ex));
                    report.AddDetail(new AveWrapperReportDto(receiver.Name, receiver.Name, AveReportObjectType.EventReceiver, AveStatus.Skipped, "you don't have permission to restore EventReceiver" + ex.Message));
                }
                catch (Exception e)
                {
                    Logger.Log(AveLogLevel.WARN, string.Format("An error occurred while add event receiver. \n error message:{0}", e));
                    report.AddDetail(new AveWrapperReportDto(receiver.Name, receiver.Name, AveReportObjectType.EventReceiver, AveStatus.Failed, string.Format("An error occurred while add event receiver. \n error message:{0}", e.Message)));
                    //mLog.Error("An error occurred while add event receiver.", e);
                }
            }

#if PerformanceLog
            }
#endif
        }

        public void Dispose()
        {
            report?.Dispose();
        }
    }

    public class AveSPWebEventReceiver : AveSPEventReceiver
    {
        private readonly AveSPWeb mAveSPWeb;

        public AveSPWebEventReceiver(AveSPWeb web)
        {
            mAveSPWeb = web;
        }

        public override void RestoreEventReceivers(List<AveEventReceiverInfo> aveEventReceivers)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebEventReceiver.WebEventReceivers"))
            {
#endif
                IAveEventReceiverDefinitionCollection eventRecivers = mAveSPWeb.SPWeb.EventReceivers;
                if (eventRecivers != null)  //eventreceiver is not supported by BPOS-S
                {
                    AddEventReceivers(eventRecivers, aveEventReceivers);
                    UpdateEventReceiversName(aveEventReceivers);
                }
#if PerformanceLog
            }
#endif
        }

        public void UpdateEventReceiversName(List<AveEventReceiverInfo> aveEventReceivers)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebEventReceiver.UpdateEventReceiversName"))
            {
#endif

                Dictionary<Guid, string> needRepliceName = new Dictionary<Guid, string>();
                foreach (IAveEventReceiverDefinition reciever in mAveSPWeb.SPWeb.EventReceivers)
                {
                    string name = string.Empty;
                    if (FindAndNeedUpdate(aveEventReceivers, reciever, ref name))
                    {
                        needRepliceName.Add(reciever.ID, name);
                    }
                }
                foreach (Guid receiverID in needRepliceName.Keys)
                {
                    IAveEventReceiverDefinition receiver = mAveSPWeb.SPWeb.EventReceivers[receiverID];
                    var oldName = receiver.Name;
                    receiver.Name = needRepliceName[receiverID];
                    try
                    {
                        receiver.Update();
                    }
                    catch (Exception e)
                    {
                        Logger.Error("update event receiver name from {0} to {1} with id:{2}, type:{3}, class:{4} and assembly:{5} failed:{6}",
                        oldName, receiver.Name, receiver.ID, receiver.Type, receiver.Class, receiver.Assembly, e);
                        report.AddDetail(new AveWrapperReportDto(receiver.Name, receiver.Name, AveReportObjectType.EventReceiver, AveStatus.Failed, string.Format("Error occurred while update event receive to web, event receiver name:{0}, Error message:{1}", receiver.Name, e.Message)));
                    }
                }

#if PerformanceLog
            }
#endif
        }
    }

    public class AveSPListEventReceiver : AveSPEventReceiver
    {
        private readonly AveSPList mAveSPList;

        public AveSPListEventReceiver(AveSPList list)
        {
            mAveSPList = list;
        }

        public override void RestoreEventReceivers(List<AveEventReceiverInfo> aveEventReceivers)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebEventReceiver.ListEventReceivers"))
            {
#endif
                IAveEventReceiverDefinitionCollection eventRecivers = mAveSPList.SPList.EventReceivers;
                if (eventRecivers != null) //eventreceiver is not supported by BPOS-S
                {
                    AddEventReceivers(eventRecivers, aveEventReceivers);
                    UpdateEventReceiversName(aveEventReceivers);
                }
#if PerformanceLog
            }
#endif
        }

        public void UpdateEventReceiversName(List<AveEventReceiverInfo> aveEventReceivers)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebEventReceiver.UpdateEventReceiversName"))
            {
#endif
                Dictionary<Guid, string> needRepliceName = new Dictionary<Guid, string>();
                foreach (IAveEventReceiverDefinition reciever in mAveSPList.SPList.EventReceivers)
                {
                    string name = string.Empty;
                    if (FindAndNeedUpdate(aveEventReceivers, reciever, ref name))
                    {
                        needRepliceName.Add(reciever.ID, name);
                    }
                }
                foreach (Guid receiverID in needRepliceName.Keys)
                {
                    IAveEventReceiverDefinition receiver = mAveSPList.SPList.EventReceivers[receiverID];
                    var oldName = receiver.Name;
                    receiver.Name = needRepliceName[receiverID];
                    try
                    {
                        receiver.Update();
                    }
                    catch (Exception e)
                    {
                        Logger.Error("update event receiver name from {0} to {1} with id:{2}, type:{3}, class:{4} and assembly:{5} failed:{6}",
                        oldName, receiver.Name, receiver.ID, receiver.Type, receiver.Class, receiver.Assembly, e);
                        //mAveSPList.RecordRestoreInformation(AveMetadataType.ListEventReceiver, WrapperRestoreStatus.RestoreException, string.Format("Error occurred while add event receive to web, event receiver name:{0}, Error message:{1}", receiver.Name, e.Message));
                        report.AddDetail(new AveWrapperReportDto(receiver.Name, receiver.Name, AveReportObjectType.EventReceiver, AveStatus.Failed, string.Format("Error occurred while update event receive to list, event receiver name:{0}, Error message:{1}", receiver.Name, e.Message)));
                    }
                }
#if PerformanceLog
            }
#endif
        }

    }
}
