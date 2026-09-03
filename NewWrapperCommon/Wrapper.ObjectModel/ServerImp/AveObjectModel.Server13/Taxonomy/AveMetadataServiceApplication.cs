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
using System.IO;
using System.Reflection;
using System.Web;
using System.Xml;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.QueryService;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Taxonomy;

namespace AvePoint.ObjectModel.Server13
{
    class AveMetadataServiceApplication : AveIisWebServiceApplication, IAveMetadataServiceApplication
    {
        private readonly Guid DefaultPartitionId = new Guid("0C37852B-34D0-418e-91C6-2AC25AF4BE5B");

        internal const string XmlAttributeCacheCheckInterval = "CacheSec";
        internal const string XmlAttributeContentTypeSyndicationHub = "HubUri";
        internal const string XmlAttributeIsSyndicationErrorReportEnabled = "RptOn";
        internal const string XmlAttributeMaxChannelCache = "MaxChan";
        internal const string XmlAttributeTermStoreId = "TSId";
        internal const string XmlElementMetadataSettings = "MetadataSettings";

        private IAveCommonQueryService mQueryService;
        //Type applicationType;
        private AveDatabase databaseInstance;
        private SPServiceApplicationProxy servicApplicationProxy;
        private AveTermStoreInfo termStoreInfo;

        internal SPServiceApplicationProxy ApplicationProxy
        {
            get
            {
                if (servicApplicationProxy == null)
                {
                    servicApplicationProxy = GetApplicationProxy();
                }
                return servicApplicationProxy;
            }
        }

        public int DefaultLanguage { get; set; }
        //对于Multi-Tenant类型的MMS，PartitionId并不准确，仅仅是从数据库查询数据的第一条数据的PartitionId值
        //对于普通的MMS，只有一个PartitionId值，他是准确的
        public Guid PartitionId { get; private set; }
        public AveMetadataServiceApplication(Guid applicationId)
            : base(GetApplication(applicationId))
        {
            mQueryService = AveQueryServiceProvider.Instance<IAveCommonQueryService>(this.Database);
            GetLanguage();
        }
        public AveMetadataServiceApplication(Guid applicationId, Guid defaultPartitionId)
            : base(GetApplication(applicationId))
        {
            mQueryService = AveQueryServiceProvider.Instance<IAveCommonQueryService>(this.Database);
            GetLanguage(defaultPartitionId);
        }

        public AveMetadataServiceApplication(string name)
            : base(GetApplication(name))
        {
            mQueryService = AveQueryServiceProvider.Instance<IAveCommonQueryService>(this.Database);
            GetLanguage();
        }
        public AveMetadataServiceApplication(string name, Guid defaultPartitionId)
            : base(GetApplication(name))
        {
            mQueryService = AveQueryServiceProvider.Instance<IAveCommonQueryService>(this.Database);
            GetLanguage(defaultPartitionId);
        }

        public void GetLanguage()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetLanguage"))
            {

                this.DefaultLanguage = mQueryService.GetLanguage(ref termStoreInfo, DefaultPartitionId);

            }

        }

        public void GetLanguage(Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetLanguage"))
            {

                this.DefaultLanguage = mQueryService.GetLanguage(defaultPartitionId);

            }

        }

        private static SPIisWebServiceApplication GetApplication(Guid applicationId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetApplication"))
            {

                Assembly a = typeof(TaxonomySession).Assembly;

                Type applicationType = a.GetType("Microsoft.SharePoint.Taxonomy.MetadataWebService");
                SPServiceCollection services = SPFarm.Local.Services;

                SPIisWebService service = AveAssemblyUtility.InvokeGenericMethod(services, "GetValue", new object[0], new Type[] { applicationType }) as SPIisWebService;

                applicationType = a.GetType("Microsoft.SharePoint.Taxonomy.MetadataWebServiceApplication");

               if (service != null)
               {
                   return (SPIisWebServiceApplication)AveAssemblyUtility.InvokeGenericMethod(service.Applications, "GetValue", new object[] { applicationId }, applicationType);
               }
               return null;

            }

        }

        private static SPIisWebServiceApplication GetApplication(string name)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetApplication"))
            {

            Assembly a = typeof(TaxonomySession).Assembly;

            Type applicationType = a.GetType("Microsoft.SharePoint.Taxonomy.MetadataWebService");
            SPServiceCollection services = SPFarm.Local.Services;

            SPIisWebService service = AveAssemblyUtility.InvokeGenericMethod(services, "GetValue", new object[0], new Type[] { applicationType }) as SPIisWebService;

            applicationType = a.GetType("Microsoft.SharePoint.Taxonomy.MetadataWebServiceApplication");

            if (service != null)
            {
                return (SPIisWebServiceApplication)AveAssemblyUtility.InvokeGenericMethod(service.Applications, "GetValue", new object[] { name }, applicationType);
            }
            return null;

            }

        }


        private SPServiceApplicationProxy GetApplicationProxy()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetApplicationProxy"))
            {

                Assembly a = typeof(TaxonomySession).Assembly;

                Type proxyType = a.GetType("Microsoft.SharePoint.Taxonomy.MetadataWebServiceProxy");
                SPServiceProxyCollection services = SPFarm.Local.ServiceProxies;

                SPServiceProxy service = (SPServiceProxy)AveAssemblyUtility.InvokeGenericMethod(services, "GetValue", new object[0], new Type[] { proxyType });
                foreach (SPServiceApplicationProxy appProxy in service.ApplicationProxies)
                {
                    if (this.mServiceApplication.IsConnected(appProxy))
                    {
                        return appProxy;
                    }
                }
                return null;

            }

        }

        public IAveDatabase Database
        {
            get
            {
                if (databaseInstance == null)
                {
                    databaseInstance = new AveDatabase(AveAssemblyUtility.GetPropertyValue(mServiceApplication, "Database") as SPDatabase);
                }
                return databaseInstance;
            }
        }

        public List<AveMetadataGroupInfo> GetGlobalGroups()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetGlobalGroups"))
            {

                return mQueryService.GetGlobalGroups(this.DefaultPartitionId);

            }

        }
        public List<AveMetadataGroupInfo> GetGlobalGroups(Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetGlobalGroups"))
            {

                return mQueryService.GetGlobalGroups(defaultPartitionId);

            }

        }

        public AveMetadataGroupInfo GetGroup(Guid groupId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetGroup"))
            {

                return mQueryService.GetGroup(groupId, this.DefaultPartitionId);

            }

        }
        public AveMetadataGroupInfo GetGroup(Guid groupId, Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetGroup"))
            {

                return mQueryService.GetGroup(groupId, defaultPartitionId);

            }

        }

        public AveMetadataGroupInfo GetGroup(string groupName)
        {
            return mQueryService.GetGroup(groupName, this.DefaultPartitionId);
        }
        public AveMetadataGroupInfo GetGroup(string groupName, Guid defaultPartitionId)
        {
            return mQueryService.GetGroup(groupName, defaultPartitionId);
        }

        public AveMetadataGroupInfo GetGroup(int groupId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetGroup_1"))
            {

                return mQueryService.GetGroup(groupId, this.DefaultPartitionId);

            }

        }
        public AveMetadataGroupInfo GetGroup(int groupId, Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetGroup_1"))
            {

                return mQueryService.GetGroup(groupId, defaultPartitionId);

            }

        }

        public List<AveMetadataGroupInfo> GetLocalGroups()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetLocalGroups"))
            {

                return mQueryService.GetLocalGroups(this.DefaultPartitionId);

            }

        }
        public List<AveMetadataGroupInfo> GetLocalGroups(Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetLocalGroups"))
            {

                return mQueryService.GetLocalGroups(defaultPartitionId);

            }

        }

        public AveTermSetInfo GetTermSet(Guid setId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTermSet"))
            {

                return mQueryService.GetTermSet(setId, this.DefaultPartitionId, this.DefaultLanguage);

            }

        }
        public AveTermSetInfo GetTermSet(Guid setId, Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTermSet"))
            {

                return mQueryService.GetTermSet(setId, defaultPartitionId, this.DefaultLanguage);

            }

        }

        public AveTermSetInfo GetTermSet(int setId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTermSet_1"))
            {

                return mQueryService.GetTermSet(setId, this.DefaultPartitionId, this.DefaultLanguage);

            }

        }
        public AveTermSetInfo GetTermSet(int setId, Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTermSet_1"))
            {

                return mQueryService.GetTermSet(setId, defaultPartitionId, this.DefaultLanguage);

            }

        }

        public List<AveTermSetInfo> GetTermSets(Guid groupId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTermSets"))
            {

                return mQueryService.GetTermSets(groupId, this.DefaultPartitionId, this.DefaultLanguage);

            }

        }
        public List<AveTermSetInfo> GetTermSets(Guid groupId, Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTermSets"))
            {

                return mQueryService.GetTermSets(groupId, defaultPartitionId, this.DefaultLanguage);

            }

        }


        public int GetTermId(Guid termId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTermId"))
            {

                return mQueryService.GetTermId(termId, this.DefaultPartitionId);

            }

        }
        public int GetTermId(Guid termId, Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTermId"))
            {

                return mQueryService.GetTermId(termId, defaultPartitionId);

            }

        }

        public AveTermInfo GetTerm(Guid termSetId, Guid termId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTerm"))
            {

                return GetTerm(termSetId, GetTermId(termId));

            }

        }
        public AveTermInfo GetTerm(Guid termSetId, Guid termId, Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTerm"))
            {

                return GetTerm(termSetId, GetTermId(termId, defaultPartitionId), defaultPartitionId);

            }

        }

        public AveTermInfo GetTerm(Guid termSetId, int termId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTerm_1"))
            {

                return mQueryService.GetTerm(termSetId, termId, this.DefaultPartitionId, this.DefaultLanguage);

            }

        }
        public AveTermInfo GetTerm(Guid termSetId, int termId, Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTerm_1"))
            {

                return mQueryService.GetTerm(termSetId, termId, defaultPartitionId, this.DefaultLanguage);

            }

        }

        public List<AveTermInfo> GetTermsInTerm(Guid termSetId, Guid termId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTermsInTerm"))
            {

                return mQueryService.GetTermsInTerm(termSetId, termId, this.DefaultPartitionId, this.DefaultLanguage);

            }

        }
        public List<AveTermInfo> GetTermsInTerm(Guid termSetId, Guid termId, Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTermsInTerm"))
            {

                return mQueryService.GetTermsInTerm(termSetId, termId, defaultPartitionId, this.DefaultLanguage);

            }

        }

        public List<AveTermInfo> GetTermsInTermSet(Guid termSetId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTermsInTermSet_1"))
            {

                return mQueryService.GetTermsInTermSet(termSetId, this.DefaultPartitionId, this.DefaultLanguage);

            }

        }
        public List<AveTermInfo> GetTermsInTermSet(Guid termSetId, Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTermsInTermSet_1"))
            {

                return mQueryService.GetTermsInTermSet(termSetId, defaultPartitionId, this.DefaultLanguage);

            }

        }

        public List<Guid> GetTermIds(Guid termSetId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTermIds"))
            {

                return mQueryService.GetTermIds(termSetId, this.DefaultPartitionId);

            }

        }
        public List<Guid> GetTermIds(Guid termSetId, Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTermIds"))
            {

                return mQueryService.GetTermIds(termSetId, defaultPartitionId);

            }

        }

        public new void Dispose()
        {

            base.Dispose();
            if (mQueryService != null)
            {
                mQueryService.Dispose();
            }
        }

        public Uri GetContentTypeSyndicationHubLocal()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetContentTypeSyndicationHubLocal"))
            {

                return (Uri)AveAssemblyUtility.InvokeMethod(this.mServiceApplication, "GetContentTypeSyndicationHubLocal", new object[] { this.DefaultPartitionId });

            }

        }
        public Uri GetContentTypeSyndicationHubLocal(Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetContentTypeSyndicationHubLocal"))
            {

                return (Uri)AveAssemblyUtility.InvokeMethod(this.mServiceApplication, "GetContentTypeSyndicationHubLocal", new object[] { defaultPartitionId });

            }

        }

        public bool IsSiteCollectionGroup(Guid groupId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.IsSiteCollectionGroup"))
            {

                return mQueryService.IsSiteCollectionGroup(groupId, this.DefaultPartitionId);

            }

        }
        public bool IsSiteCollectionGroup(Guid groupId, Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.IsSiteCollectionGroup"))
            {

                return mQueryService.IsSiteCollectionGroup(groupId, defaultPartitionId);

            }

        }

        public List<Guid> GetSiteCollectionId(Guid groupId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetSiteCollectionId"))
            {

                return mQueryService.GetSiteCollectionIdList(groupId, this.DefaultPartitionId);

            }

        }
        public List<Guid> GetSiteCollectionId(Guid groupId, Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetSiteCollectionId"))
            {

                return mQueryService.GetSiteCollectionIdList(groupId, defaultPartitionId);

            }

        }

        public List<AveTermChangeItem> GetChangesInTermSet(Guid termSetId, Nullable<DateTime> sinceTime)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetChangesInTermSet"))
            {

                return mQueryService.GetChangesInTermSet(termSetId, sinceTime, this.DefaultPartitionId, this.DefaultLanguage);

            }

        }
        public List<AveTermChangeItem> GetChangesInTermSet(Guid termSetId, Nullable<DateTime> sinceTime, Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetChangesInTermSet"))
            {

                return mQueryService.GetChangesInTermSet(termSetId, sinceTime, defaultPartitionId, this.DefaultLanguage);

            }

        }
        
        public List<AveTermChangeItem> GetChangesInGroup(Guid groupId, Nullable<DateTime> sinceTime)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetChangesInGroup"))
            {

                return mQueryService.GetChangesInGroup(groupId, sinceTime, this.DefaultPartitionId, this.DefaultLanguage);

            }

        }
        public List<AveTermChangeItem> GetChangesInGroup(Guid groupId, Nullable<DateTime> sinceTime, Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetChangesInGroup"))
            {

                return mQueryService.GetChangesInGroup(groupId, sinceTime, defaultPartitionId, this.DefaultLanguage);

            }

        }

        public List<AveTermChangeItem> GetChangesInTerm(Guid termSetId, Guid termId, Nullable<DateTime> sinceTime)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetChangesInTerm"))
            {

                return mQueryService.GetChangesInTerm(termSetId, termId, sinceTime, this.DefaultPartitionId, this.DefaultLanguage);

            }

        }
        public List<AveTermChangeItem> GetChangesInTerm(Guid termSetId, Guid termId, Nullable<DateTime> sinceTime, Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetChangesInTerm"))
            {

                return mQueryService.GetChangesInTerm(termSetId, termId, sinceTime, defaultPartitionId, this.DefaultLanguage);

            }

        }

        public AveTermChangeItem GetTermParent(Guid termSetId, Guid termId, Guid parentTermId, Guid partitionId, bool isRoot, bool isSourceTerm)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTermParent"))
            {
                return mQueryService.GetTermParent(termSetId, termId, parentTermId, partitionId, isRoot, isSourceTerm, this.DefaultLanguage);
            }
        }

        public List<AveTermChangeItem> GetTermSetChildren(Guid termSetId, Guid partitionId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTermSetChildren"))
            {
                return mQueryService.GetTermSetChildren(termSetId, partitionId, this.DefaultLanguage);
            }
        }

        public AveTermChangeItem GetTermSetParent(Guid termSetId, Guid partitionId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTermSetParent"))
            {
                return mQueryService.GetTermSetParent(termSetId, partitionId, this.DefaultLanguage);
            }
        }

        public List<AveTermChangeItem> GetAllChanges(Nullable<DateTime> sinceTime)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetAllChanges"))
            {

                return mQueryService.GetChanges(null, null, sinceTime.Value, null, this.DefaultPartitionId);

            }

        }
        public List<AveTermChangeItem> GetAllChanges(Nullable<DateTime> sinceTime, Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetAllChanges"))
            {

                return mQueryService.GetChanges(null, null, sinceTime.Value, null, defaultPartitionId);

            }

        }

        public List<AveTermChangeItem> GetChangesInStore(DateTime? sinceTime, bool isGlobal)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetChangesInStore"))
            {

                return mQueryService.GetChangesInStore(sinceTime, isGlobal, this.DefaultPartitionId);

            }

        }
        public List<AveTermChangeItem> GetChangesInStore(DateTime? sinceTime, bool isGlobal, Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetChangesInStore"))
            {

                return mQueryService.GetChangesInStoreForTenant(sinceTime, isGlobal, defaultPartitionId);

            }

        }

        public List<AveTermChangeItem> GetChangesInGroup(Guid groupId, DateTime? sinceTime, DateTime? toTime)
        {
            return mQueryService.GetChangesInGroup(groupId, sinceTime, toTime, this.DefaultPartitionId);
        }
        public List<AveTermChangeItem> GetChangesInGroup(Guid groupId, DateTime? sinceTime, DateTime? toTime, Guid defaultPartitionId)
        {
            return mQueryService.GetChangesInGroup(groupId, sinceTime, toTime, defaultPartitionId);
        }

        public List<AveTermChangeItem> GetChangesInTermSet(Guid termSetId, DateTime? sinceTime, DateTime? toTime)
        {
            return mQueryService.GetChangesInTermSet(termSetId, sinceTime, toTime, this.DefaultPartitionId);
        }
        public List<AveTermChangeItem> GetChangesInTermSet(Guid termSetId, DateTime? sinceTime, DateTime? toTime, Guid defaultPartitionId)
        {
            return mQueryService.GetChangesInTermSet(termSetId, sinceTime, toTime, defaultPartitionId);
        }

        public bool IsPublished(string contentTypeId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.IsPublished"))
            {

                return mQueryService.IsPublished(contentTypeId, this.DefaultPartitionId);

            }

        }
        public bool IsPublished(string contentTypeId, Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.IsPublished"))
            {

                return mQueryService.IsPublished(contentTypeId, defaultPartitionId);

            }

        }

        public bool IsUnPublished(string contentTypeId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.IsUnPublished"))
            {

                return mQueryService.IsUnPublished(contentTypeId, this.DefaultPartitionId);

            }

        }
        public bool IsUnPublished(string contentTypeId, Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.IsUnPublished"))
            {

                return mQueryService.IsUnPublished(contentTypeId, defaultPartitionId);

            }

        }

        public AveTermStoreInfo GetTermStore()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTermStore"))
            {

                string settingsXml = mQueryService.GetTermStore(this.DefaultPartitionId);
                if (!string.IsNullOrEmpty(settingsXml))
                {
                    PartitionSettings settings = DeserializeServiceSettings(settingsXml);
                    if (termStoreInfo == null)
                    {
                        GetLanguage(DefaultPartitionId);
                        this.termStoreInfo = mQueryService.GetTermStoreInfo(DefaultPartitionId);
                    }
                    termStoreInfo.Id = settings.TermStoreId;
                    termStoreInfo.Name = this.ApplicationProxy.DisplayName;
                    termStoreInfo.WorkingLanguage = this.DefaultLanguage; ;
                    termStoreInfo.DefaultLanguage = this.DefaultLanguage;
                }

                return termStoreInfo;

            }

        }
        public AveTermStoreInfo GetTermStore(Guid defaultPartitionId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.GetTermStore"))
            {

                string settingsXml = mQueryService.GetTermStore(defaultPartitionId);
                if (!string.IsNullOrEmpty(settingsXml))
                {
                    PartitionSettings settings = DeserializeServiceSettings(settingsXml);
                    if (termStoreInfo == null)
                    {
                        GetLanguage(defaultPartitionId);
                        this.termStoreInfo = mQueryService.GetTermStoreInfo(defaultPartitionId);
                    }
                    termStoreInfo.Id = settings.TermStoreId;
                    termStoreInfo.PartitionId = defaultPartitionId;
                    termStoreInfo.Name = this.ApplicationProxy.DisplayName;
                    termStoreInfo.WorkingLanguage = this.DefaultLanguage;
                    termStoreInfo.DefaultLanguage = this.DefaultLanguage;
                }

                return termStoreInfo;

            }

        }

        internal static PartitionSettings DeserializeServiceSettings(string settingsXml)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.DeserializeServiceSettings"))
            {

                settingsXml = HttpUtility.HtmlDecode(settingsXml);
                PartitionSettings settings = null;
                StringReader input = new StringReader(settingsXml);
                XmlTextReader reader = new XmlTextReader(input);
                XmlReaderSettings settings2 = new XmlReaderSettings();
                XmlReader reader3 = XmlReader.Create(reader, settings2);
                if (reader3.Read() && reader3.IsStartElement() && reader3.Name == XmlElementMetadataSettings)
                {
                    string str = HttpUtility.HtmlDecode(reader3.GetAttribute(XmlAttributeContentTypeSyndicationHub));
                    Uri hubUri = null;
                    if (!string.IsNullOrEmpty(str)) hubUri = new Uri(str);
                    settings = new PartitionSettings()
                    {
                        TermStoreId = XmlConvert.ToGuid(reader3.GetAttribute(XmlAttributeTermStoreId)),
                        HubUri = hubUri,
                        IsSyndicationErrorReportEnabled = XmlConvert.ToBoolean(reader3.GetAttribute(XmlAttributeIsSyndicationErrorReportEnabled))
                        //CacheCheckIntervalInSeconds = reader3.GetAttribute(XmlAttributeCacheCheckInterval),
                        //MaxChannelCacheSize = reader3.GetAttribute(XmlAttributeMaxChannelCache)
                    };
                }
                reader3.Close();
                reader.Close();
                input.Close();
                return settings;

            }

        }

        public string GetTermDefaultLabel(int termId)
        {
            return mQueryService.GetTermDefaultLabel(termId, this.DefaultPartitionId, this.DefaultLanguage);
        }
        public string GetTermDefaultLabel(int termId, Guid defaultPartitionId)
        {
            return mQueryService.GetTermDefaultLabel(termId, defaultPartitionId, this.DefaultLanguage);
        }

        public string GetTermDefaultLabel(int termId, Guid defaultPartitionId, int defaultLanguage)
        {
            return mQueryService.GetTermDefaultLabel(termId, defaultPartitionId, defaultLanguage);
        }

        /// <summary>
        /// ????PartitionId??settingsXml
        /// </summary>
        /// <returns></returns>
        public List<ServiceSetting> GetPartitionServiceSettings()
        {
            return mQueryService.GetPartitionServiceSettings();
        }

        /// <summary>
        /// ??Metadata???Partition?MMS
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public bool IsMetadataPartition(AveServiceApplication app)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetadataServiceApplication.IsMetadataPartition"))
            {

                Type type = Assembly.Load("Microsoft.Office.Server, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c")
                              .GetType("Microsoft.Office.Server.Utilities.SPServiceApplicationUtilities");
                AvePartitionOptions PartitionOptions = (AvePartitionOptions)AveAssemblyUtility.InvokeStaticMethod(type, "GetPartitionOptions", new object[] { app.ServiceApplication });
                return PartitionOptions == AvePartitionOptions.UniquePartitionPerSubscription;

            }

        }
        /// <summary>
        /// ??Metadata???Partition?MMS
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public bool IsMetadataPartition(Guid ApplicationId)
        {
            SPServiceApplication app = GetApplication(ApplicationId);
           return IsMetadataPartition(new AveServiceApplication(app));
        }

        public List<AveSiteMapVisible> GetTenancyAdminSiteId(Guid defaultPartitionId)
        {
            using (var queryService = AveQueryServiceProvider.Instance<IAveMetadataServiceQueryService>(ConfigurationDatabase.DatabaseConnectionString))
            {
                return queryService.GetTenancyAdminSiteId(defaultPartitionId);
            }
        }
    }

    internal sealed class PartitionSettings
    {
        // Fields
        internal int CacheCheckIntervalInSeconds { get; set; }
        internal Uri HubUri { get; set; }
        internal bool IsSyndicationErrorReportEnabled { get; set; }
        internal int MaxChannelCacheSize { get; set; }
        internal Guid TermStoreId { get; set; }
    }
}
