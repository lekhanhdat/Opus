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
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Reflection;

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
            if (ItemEventReceiver == null)
            {
                ItemEventReceiver = objectModelFactory.CreateItemEventReceiver();
            }
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

    public abstract class AveSPEventReceiver : AvePoint.Wrapper.Restore.IAveSPEventReceiver, IDisposable
    {
        private static readonly AveLogger Logger = AveLogger.GetInstance(typeof(AveSPEventReceiver));

        protected IReport report = new AveWrapperReport();

        protected abstract string SPVersion { get; }
        protected abstract bool IsOnlineSite { get; }

        public static AveSPEventReceiver CreateInstance(object obj)
        {
            AveSPEventReceiver instance;
            if (obj is AveSPWeb)
            {
                instance = new AveSPWebEventReceiver((AveSPWeb) obj);
            }
            else if (obj is AveSPList)
            {
                instance = new AveSPListEventReceiver((AveSPList) obj);
            }
            else
            {
                throw new Exception("Cannot construct an instance for this object type: " + obj.GetType());
            }
            return instance;
        }

        public abstract void RestoreEventReceivers(List<AveEventReceiverInfo> aveEventReceivers);

        public bool FindAndNeedUpdate(List<AveEventReceiverInfo> aveEventReceivers, IAveEventReceiverDefinition reciever, ref string name)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPEventReceiver.FindAndNeedUpdate"))
            {

            foreach (AveEventReceiverInfo receiverInfo in aveEventReceivers)
            {
                if (receiverInfo.AssemblyInfo.Equals(reciever.Assembly) && receiverInfo.ClassName.Equals(reciever.Class) && receiverInfo.Type == (int)(reciever.Type))
                {
                    if (reciever.Name != receiverInfo.Name || receiverInfo.Synchronization != reciever.Synchronization || reciever.SequenceNumber != receiverInfo.SequenceNumber)
                    {
                        name = receiverInfo.Name;
                        return true;
                    }

                }
            }
            return false;

            }

        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SPWorkflowAutostartEventReceiver is a class")]
        protected bool AddEventReceivers(IAveEventReceiverDefinitionCollection spEventReceivers, List<AveEventReceiverInfo> aveEventReceivers)
        {
            bool updated = false;

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebEventReceiver.AddEventReceivers"))
            {

            foreach (AveEventReceiverInfo receiver in aveEventReceivers)
            {
                try
                {
                    bool isExist = false;
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
                    if (!isExist)
                    {
                        if (!string.Equals(receiver.ClassName, "Microsoft.SharePoint.Workflow.SPWorkflowAutostartEventReceiver", StringComparison.OrdinalIgnoreCase))
                        {
                            if (NeedSkipByAssembly(receiver.AssemblyInfo))
                            {
                                report.AddDetail(new AveWrapperReportDto(receiver.Name, receiver.Name, AveReportObjectType.EventReceiver, AveStatus.Skipped, AveReportResource.Wrapper_Report_Office365EnvironmentIssue));
                                continue;
                            }
                            spEventReceivers.Add((AveEventReceiverType)receiver.Type, receiver.AssemblyInfo, receiver.ClassName, receiver.Name);
                            updated = true;
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    Logger.Log(AveLogLevel.WARN, string.Format("An error occurred while add event receiver. \n error message:{0}", ex));
                    report.AddDetail(new AveWrapperReportDto(receiver.Name, receiver.Name, AveReportObjectType.EventReceiver, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToRestoreEventReceiver , ex.Message));
                }
                catch (Exception e)
                {
                    Logger.Log(AveLogLevel.WARN, string.Format("An error occurred while add event receiver. \n error message:{0}", e));
                    report.AddDetail(new AveWrapperReportDto(receiver.Name, receiver.Name, AveReportObjectType.EventReceiver, AveStatus.Failed, AveReportResource.Wrapper_Report_AddEventReceiverError, e.Message));
                    //mLog.Error("An error occurred while add event receiver.", e);
                }
            }


            }

            return updated;
        }

        private bool NeedSkipByAssembly(string assemblyInfo)
        {
            bool needSkip = false;
            Version assemblyVersion = new Version(this.SPVersion);
            AssemblyName assemblyName = new AssemblyName(assemblyInfo);
            if (!this.IsOnlineSite && assemblyName.Version.Major > assemblyVersion.Major)
            {
                needSkip = true;
            }
            return needSkip;
        }

        public IReport GetReport()
        {
            return report;
        }

        public void Dispose()
        {
            report.Dispose();
        }
    }

    public class AveSPWebEventReceiver : AveSPEventReceiver
    {
        private readonly AveSPWeb mAveSPWeb;

        protected override string SPVersion
        {
            get
            {
                return mAveSPWeb.ParentSite.SPSite.SPVersion;
            }
        }

        protected override bool IsOnlineSite
        {
            get
            {
                return mAveSPWeb.ParentSite.SPSite.IsOnlineSite;
            }
        }

        public AveSPWebEventReceiver(AveSPWeb web)
        {
            mAveSPWeb = web;
        }

        public override void RestoreEventReceivers(List<AveEventReceiverInfo> aveEventReceivers)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebEventReceiver.WebEventReceivers"))
            {

            IAveEventReceiverDefinitionCollection eventRecivers = mAveSPWeb.SPWeb.EventReceivers;
            if (eventRecivers != null)  //eventreceiver is not supported by BPOS-S
            {
                AddEventReceivers(eventRecivers, aveEventReceivers);
                UpdateEventReceiversName(aveEventReceivers);
            }

            }

        }

        public void UpdateEventReceiversName(List<AveEventReceiverInfo> aveEventReceivers)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebEventReceiver.UpdateEventReceiversName"))
            {


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
                receiver.Name = needRepliceName[receiverID];
                AveEventReceiverInfo sourceReciver = aveEventReceivers.Find(r => { return r.Name.Equals(receiver.Name, StringComparison.OrdinalIgnoreCase); });
                if (sourceReciver != null)
                {
                    receiver.SequenceNumber = sourceReciver.SequenceNumber;
                    receiver.Synchronization = sourceReciver.Synchronization;
                }
                try
                {
                    receiver.Update();
                }
                catch (Exception e)
                {
                    report.AddDetail(new AveWrapperReportDto(receiver.Name, receiver.Name, AveReportObjectType.EventReceiver, AveStatus.Failed, AveReportResource.Wrapper_Report_UpdateEventReceiverToWebError, receiver.Name, e.Message));
                }
            }


            }

        }
    }

    public class AveSPListEventReceiver : AveSPEventReceiver
    {
        private readonly AveSPList mAveSPList;

        protected override string SPVersion
        {
            get
            {
                return mAveSPList.ParentSite.SPSite.SPVersion;
            }
        }

        protected override bool IsOnlineSite
        {
            get
            {
                return mAveSPList.ParentSite.SPSite.IsOnlineSite;
            }
        }

        public AveSPListEventReceiver(AveSPList list)
        {
            mAveSPList = list;
        }

        public override void RestoreEventReceivers(List<AveEventReceiverInfo> aveEventReceivers)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebEventReceiver.ListEventReceivers"))
            {

            IAveEventReceiverDefinitionCollection eventRecivers = mAveSPList.SPList.EventReceivers;
            if (eventRecivers != null) //eventreceiver is not supported by BPOS-S
            {
                bool updated = AddEventReceivers(eventRecivers, aveEventReceivers);
                updated = updated | UpdateEventReceiversName(aveEventReceivers);

                if (updated)
                {
                    //由于SharePoint添加Event Receiver会导致SPWeb.Lists为dirtry，所以当前的list已经标记为过期的list，
                    //不能够继续update，如果继续update，会导致对象不一致，所以需要重新reload list就好使了。
                    this.mAveSPList.ReloadList();
                }
            }

            }

        }

        public bool UpdateEventReceiversName(List<AveEventReceiverInfo> aveEventReceivers)
        {
            bool updated = false;

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebEventReceiver.UpdateEventReceiversName"))
            {

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
                receiver.Name = needRepliceName[receiverID];
                AveEventReceiverInfo sourceReciver = aveEventReceivers.Find(r => { return r.Name.Equals(receiver.Name, StringComparison.OrdinalIgnoreCase); });
                if (sourceReciver != null)
                {
                    receiver.SequenceNumber = sourceReciver.SequenceNumber;
                    receiver.Synchronization = sourceReciver.Synchronization;
                }
                try
                {
                    receiver.Update();
                    updated = true;
                }
                catch (Exception e)
                {
                    //mAveSPList.RecordRestoreInformation(AveMetadataType.ListEventReceiver, WrapperRestoreStatus.RestoreException, string.Format("Error occurred while add event receive to web, event receiver name:{0}, Error message:{1}", receiver.Name, e.Message));
                    report.AddDetail(new AveWrapperReportDto(receiver.Name, receiver.Name, AveReportObjectType.EventReceiver, AveStatus.Failed, AveReportResource.Wrapper_Report_UpdateEventReceiverToListError, receiver.Name, e.Message));
                }
            }

            }

            return updated;
        }

    }
}
