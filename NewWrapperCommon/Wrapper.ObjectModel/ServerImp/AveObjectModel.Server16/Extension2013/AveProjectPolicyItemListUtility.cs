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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint;
using System.Reflection;
using Microsoft.Office.RecordsManagement.InformationPolicy;
using Microsoft.SharePoint.Publishing.Internal;
using System.Diagnostics.CodeAnalysis;
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.Server16
{
    [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
    public class AveProjectPolicyItemListUtility : IAveProjectPolicyItemListUtility
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private static readonly string[] s_rgstrUrls = new string[] { 
        "$Resources:dlccore,DocumentsFolder;", "$Resources:dlccore,DocumentsFolder;/Forms/AllItems.aspx", "$Resources:dlccore,DropOffZone_ListFolder;", "$Resources:dlccore,RoutingRuleList_ListFolder;", "$Resources:core,lists_Folder;/$Resources:dlccore,EmailRouting_EmailListFolder;", "$Resources:dlccore,HoldReportsListFolder;", "$Resources:core,lists_Folder;/$Resources:dlccore,HoldsListFolder;", "$Resources:core,lists_Folder;/$Resources:core,tasks_Folder;", "$Resources:core,lists_Folder;/$Resources:core,tasks_Folder;/AllItems.aspx", "UploadEx.aspx", "$Resources:dlccore,RecordsLib_ListFolder;", "$Resources:dlccore,Reporting_TemplateListFolder;", "$Resources:core,lists_Folder;/$Resources:dlccore,Reporting_MetadataListFolder;", "$Resources:core,lists_Folder;/$Resources:dlccore,SourcesListUrl;", "$Resources:core,lists_Folder;/$Resources:dlccore,SourceGroupsListUrl;", "$Resources:core,lists_Folder;/$Resources:dlccore,SourceInstancesListUrl;", 
        "$Resources:core,lists_Folder;/$Resources:dlccore,CustodiansListUrl;", "$Resources:core,lists_Folder;/$Resources:dlccore,SavedSearchesListUrl;", "$Resources:core,lists_Folder;/$Resources:dlccore,ExportsListUrl;", "$Resources:dlccore,RootWebHoldListUrl;", "$Resources:dlccore,PreservationListUrl;", "$Resources:dlccore,ProjectPolicyItemListUrl;"
     };


     //   private static readonly string[] s_rgstrUrls = new string[] { 
     //   "$Resources:dlccore,DocumentsFolder;", "$Resources:dlccore,DocumentsFolder;/Forms/AllItems.aspx", "$Resources:dlccore,DropOffZone_ListFolder;", "$Resources:dlccore,RoutingRuleList_ListFolder;", "$Resources:core,lists_Folder;/$Resources:dlccore,EmailRouting_EmailListFolder;", "$Resources:dlccore,HoldReportsListFolder;", "$Resources:core,lists_Folder;/$Resources:dlccore,HoldsListFolder;", "$Resources:core,lists_Folder;/$Resources:core,tasks_Folder;", "$Resources:core,lists_Folder;/$Resources:core,tasks_Folder;/AllItems.aspx", "UploadEx.aspx", "$Resources:dlccore,RecordsLib_ListFolder;", "$Resources:dlccore,Reporting_TemplateListFolder;", "$Resources:core,lists_Folder;/$Resources:dlccore,Reporting_MetadataListFolder;", "$Resources:core,lists_Folder;/$Resources:dlccore,SourcesListUrl;", "$Resources:core,lists_Folder;/$Resources:dlccore,SourceGroupsListUrl;", "$Resources:core,lists_Folder;/$Resources:dlccore,SourceInstancesListUrl;", 
     //   "$Resources:core,lists_Folder;/$Resources:dlccore,CustodiansListUrl;", "$Resources:core,lists_Folder;/$Resources:dlccore,SavedSearchesListUrl;", "$Resources:core,lists_Folder;/$Resources:dlccore,ExportsListUrl;", "$Resources:dlccore,RootWebHoldListUrl;", "$Resources:dlccore,PreservationListUrl;", "$Resources:dlccore,ProjectPolicyItemListUrl;"
     //};



     //   public DateTime? GetDateTimeFieldValue(IAveListItem item, string fieldName)
     //   {
     //       DateTime? nullable = null;
     //       object obj2 = item[fieldName];
     //       if (obj2 != null)
     //       {
     //           nullable = new DateTime?((DateTime)obj2);
     //       }
     //       return nullable;

     //   }
        internal int GetCloseDeleteOption(SPContentType contentType)
        {
            Type type = Assembly.Load("Microsoft.Office.Policy, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c").GetType("Microsoft.Office.RecordsManagement.InformationPolicy.ProjectPolicy");
            MethodInfo method = type.GetMethod("GetProjectPolicy", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
            var obj = method.Invoke(type, new object[] { contentType });
            ProjectPolicy policy = obj as ProjectPolicy;
            int closeDeleteOption = 0;
            if (policy != null)
            {
                PropertyInfo property = type.GetProperty("CloseDeleteOption", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
                closeDeleteOption = (int)property.GetValue(policy, null);
            }
            return closeDeleteOption;
        }
        
        internal SPList GetProjectPolicyItemList(SPSite site)
         {
             
             Type type = Assembly.Load("Microsoft.Office.Policy, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c").GetType("Microsoft.Office.RecordsManagement.InformationPolicy.ProjectPolicyItemListUtility");
             MethodInfo method = type.GetMethod("GetProjectPolicyItemList", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);


             var obj = method.Invoke(type, new object[] { site });
             SPList currentProjectPolicyItemList = obj as SPList;
             return currentProjectPolicyItemList;
         }


        internal void DeleteProjectPolicy(SPSite site, SPList projectPolicyItemList, ProjectPolicy projectPolicy)
        {
            string typeId = string.Empty;
            if (projectPolicyItemList != null)
            {
                SPContentTypeCollection contentTypes = projectPolicyItemList.ContentTypes;
                SPContentType type = contentTypes[projectPolicy.Name];
                typeId =type.Id.ToString();
                if (type != null)
                {
                    contentTypes.Delete(type.Id);
                }
            }
            if(!string.IsNullOrEmpty(typeId))
                site.RootWeb.ContentTypes.Delete(new SPContentTypeId(typeId));

        }
        internal ProjectPolicy GetProjectPolicy(SPContentType ct)
        {
            Type type = Assembly.Load("Microsoft.Office.Policy, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c").GetType("Microsoft.Office.RecordsManagement.InformationPolicy.ProjectPolicy");
            ConstructorInfo constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance, null, new Type[] { typeof(SPContentType) }, null);
            return constructor.Invoke(new object[] {ct }) as ProjectPolicy;
        }



        public AveProjectPolicyInfo GetObjectData(Guid siteId,Guid webId)
        {
            AveProjectPolicyInfo policyInfo = null;
            using(SPSite site = new SPSite(siteId))
            {
                using(SPWeb web=site.OpenWeb(webId))
                {
                    SPList list = null;
                    list=GetProjectPolicyItemList(site);
                    ProjectPolicy policy= ProjectPolicy.GetCurrentlyAppliedProjectPolicyOnWeb(web);
                    if (list != null)
                    {
                        policyInfo = new AveProjectPolicyInfo();
                        policyInfo.SiteClosedTime = DateTime.MinValue;
                        policyInfo.IsSiteClosed = ProjectPolicy.IsProjectClosed(web);
                        SPListItem item=GetProjectPolicyItemWithProjectWebGuid(list,web.ID);
                       
                        if(item!=null)
                        {
                            policyInfo.ProjectPolicyContentType=item.ContentTypeId.ToString();                           
                            policyInfo.SiteClosedTime =  GetProjectCloseDateProperty(item);
                            policyInfo.ProjectPolicyName = policy.Name;
                            
                            if (GetCloseDeleteOption(item.ContentType) != 0)
                            {
                                policyInfo.projectExpirationDate = GetProjectExpirationDateProperty(item);
                            }                           
                        }
                        
                    }
                }
            }
            return policyInfo;
        }


        public void SetObjectData(Guid siteId, Guid webId, AveProjectPolicyInfo policyInfo)
        {

            if (policyInfo != null)
            {
                string contentTypeId=policyInfo.ProjectPolicyContentType;
                if (!string.IsNullOrEmpty(contentTypeId))
                {
                    using (SPSite site = new SPSite(siteId))
                    {
                        using (SPWeb web = site.OpenWeb(webId))
                        {
                            SPList currentProjectPolicyItemlist=GetProjectPolicyItemList(site);
                            if (currentProjectPolicyItemlist == null)
                            { 
                                currentProjectPolicyItemlist= CreateProjectPolicyItemList(site);
                                
                            } 
                            //if (ProjectPolicy.DoesProjectHavePolicy(web))
                            //{ 
                            //    SPListItem item= GetProjectPolicyItemWithProjectWebGuid(currentProjectPolicyItemlist,webId);
                            //    ProjectPolicy policy=GetProjectPolicy(item.ContentType);
                            //    DeleteProjectPolicy(site, currentProjectPolicyItemlist, policy);
                            //}
                            if (!string.IsNullOrEmpty(policyInfo.ProjectPolicyContentType))
                            {

                                ProjectPolicy policy = GetProjectPolicy(web, policyInfo.ProjectPolicyContentType, policyInfo.ProjectPolicyName);
                                if (policy != null)
                                {
                                    //添加异常处理，处理由ProjectPolicy API内错误引起的整个Web备份失败的异常。由于微软的问题不能正确的在13 RTM环境中备份web policy，13 SP1没有这个问题。
                                    try
                                    {
                                        ProjectPolicy.ApplyProjectPolicy(web, policy);
                                    }
                                    catch(Microsoft.SharePoint.SPInvalidPropertyException e)
                                    {
                                        mLog.Error("Restore web policy failed. Url: {0}, Exception message :{1}", web.Url, e);
                                    }
                                }                               
                            }
                            if (!ProjectPolicy.IsProjectClosed(web) && policyInfo.IsSiteClosed)
                            {
                                ProjectPolicy.CloseProject(web);
                            }
                            if (ProjectPolicy.IsProjectClosed(web) && !policyInfo.IsSiteClosed)
                            {
                                ProjectPolicy.OpenProject(web);
                            }
                        }
                    }
                }
            
            }
        }
        internal ProjectPolicy GetProjectPolicy(SPWeb web,string contentTypeId, string name)
        { 
             SPContentTypeId contentId = new SPContentTypeId(contentTypeId);
             SPContentType contentType= web.ContentTypes[contentId];
             if (contentType != null)
             {
                  ProjectPolicy policy = GetProjectPolicy(contentType);
                  return policy;
             }
            var polices= ProjectPolicy.GetProjectPolicies(web);
            foreach (var policy in polices)
            {
                if (policy.Name == name)
                    return policy;

            }
            return null;

        }     


        internal bool TryGetTeamMailBoxId(SPWeb web, out string teamMailBoxId)
        {
            bool flag = false;
            teamMailBoxId = string.Empty;
            if (web.AllProperties.ContainsKey("ExchangeTeamMailboxEmailAddress"))
            {
                teamMailBoxId = web.AllProperties["ExchangeTeamMailboxEmailAddress"] as string;
                flag = !string.IsNullOrEmpty(teamMailBoxId);
            }
            return flag;
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "dlccore")]
        internal SPList CreateProjectPolicyItemList(SPSite site)
        {
            SPList list = null;
            
            SPWeb rootWeb = site.RootWeb;
            Guid guid = rootWeb.Lists.Add("$Resources:dlccore,ProjectPolicyItemListName;", "$Resources:dlccore,ProjectPolicyItemListDescription;",  SPUtility.GetLocalizedString(s_rgstrUrls[21], null, rootWeb.Language), WssFeatureIds.CustomList.ToString("D"), 100, null);
            list = site.RootWeb.Lists[guid];
            list.Hidden = true;
            list.ForceCheckout = false;
            list.EnableVersioning = false;
            list.EnableFolderCreation = false;
            list.NoCrawl = true;
            SPFieldCollection fields = list.Fields;
            fields.Add(AveProjectPolicyFieldNames.ProjectWebGuid, SPFieldType.Guid, true);
            SPField field = fields[AveProjectPolicyFieldNames.ProjectWebGuid];
            field.Indexed = true;
            field.Update();
            fields.Add(AveProjectPolicyFieldNames.ProjectWebUrl, SPFieldType.URL, true);
            fields.Add(AveProjectPolicyFieldNames.ProjectParentWebGuid, SPFieldType.Guid, true);
            fields.Add(AveProjectPolicyFieldNames.ProjectCreateDate, SPFieldType.DateTime, true);
            fields.Add(AveProjectPolicyFieldNames.ProjectCloseDate, SPFieldType.DateTime, false);
            fields.Add(AveProjectPolicyFieldNames.ProjectExpirationDate, SPFieldType.DateTime, false);
            fields.Add(AveProjectPolicyFieldNames.ProjectIsClosed, SPFieldType.Boolean, true);
            fields.Add(AveProjectPolicyFieldNames.ProjectNumberOfPostpone, SPFieldType.Integer, false);
            fields.Add(AveProjectPolicyFieldNames.ProjectTeamMailBoxId, SPFieldType.Text, false);
            fields.Add(AveProjectPolicyFieldNames.ProjectTeamMailBoxWorkItemId, SPFieldType.Guid, false);
            list.Update();
            return list;

        
        }
        internal SPListItem GetProjectPolicyItemWithProjectWebGuid(SPList policyItem, Guid webId)
        {
            Type type = Assembly.Load("Microsoft.Office.Policy, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c").GetType("Microsoft.Office.RecordsManagement.InformationPolicy.ProjectPolicyItemListUtility");
            MethodInfo method2 = type.GetMethod("GetProjectPolicyItemWithProjectWebGuid", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
            var obj2 = method2.Invoke(type, new object[] { policyItem, webId });

            SPListItem item = obj2 as SPListItem;
            return item;
        }
        internal DateTime GetProjectCloseDateProperty(SPListItem item)
        {
            Type type = Assembly.Load("Microsoft.Office.Policy, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c").GetType("Microsoft.Office.RecordsManagement.InformationPolicy.ProjectPolicyItemListUtility");

            MethodInfo method4 = type.GetMethod("GetProjectCloseDateProperty", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
            var obj4 = method4.Invoke(type, new object[] { item });
            DateTime? closeDate = (DateTime?)obj4;
            return closeDate.HasValue ? closeDate.Value : DateTime.MinValue;
        }

        internal DateTime GetProjectExpirationDateProperty(SPListItem item)
        {
            Type type = Assembly.Load("Microsoft.Office.Policy, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c").GetType("Microsoft.Office.RecordsManagement.InformationPolicy.ProjectPolicyItemListUtility");

            MethodInfo method5 = type.GetMethod("GetProjectExpirationDateProperty", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
            var obj5 = method5.Invoke(type, new object[] { item });
            DateTime? projectExpirationDateProperty = (DateTime?)obj5;
            DateTime expirationDateProperty = projectExpirationDateProperty.HasValue ? projectExpirationDateProperty.Value : DateTime.MinValue;

            return expirationDateProperty;
        }



        public AveListPolicyInfo GetObjectData(Guid siteId, Guid webId, Guid listId)
        {
            AveListPolicyInfo policyInfo = null;
            
            using (SPSite site = new SPSite(siteId))
            {
                using (SPWeb web = site.OpenWeb(webId))
                {
                    SPList list = null;
                    list = web.Lists[listId];
                    ListPolicySettings policySettings = new ListPolicySettings(list);
                    if (policySettings.ListHasPolicy == true && policySettings.UseListPolicy == true)
                    {
                        string retentionSchedule = policySettings.GetRetentionSchedule(list.RootFolder.ServerRelativeUrl);
                        var description = AveAssemblyUtility.InvokeGenericMethod(policySettings, "GetScheduleDescription", new object[] { list.RootFolder.ServerRelativeUrl }, new Type[] { typeof(string) });

                        policyInfo = new AveListPolicyInfo();
                        policyInfo.RetentionSchedule = retentionSchedule;
                        policyInfo.Description = description == null ? string.Empty : description.ToString();
                    }
                }
            }
            return policyInfo;
        }

        public void SetObjectData(Guid siteId, Guid webId, Guid listId, AveListPolicyInfo policyInfo)
        {
            if (policyInfo != null)
            {
                try
                {
                    using (SPSite site = new SPSite(siteId))
                    {
                        using (SPWeb web = site.OpenWeb(webId))
                        {
                            SPList list = null;
                            list = web.Lists[listId];
                                                        
                            list = web.Lists[listId];
                            ListPolicySettings policySettings = new ListPolicySettings(list);

                            policySettings.UseListPolicy = true;
                            policySettings.SetRetentionSchedule(policyInfo.RetentionSchedule, policyInfo.Description);
                            policySettings.Update();
                        }
                    }
                }
                catch(Exception ex)
                {
                    mLog.Warn("Update list policy failed. {0} ", ex.ToString());   
                }
            }
        }
    }
}
