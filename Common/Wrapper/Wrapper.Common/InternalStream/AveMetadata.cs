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
using System.Xml;
using System.Collections;
using System.Reflection;
using System.Linq;

namespace AvePoint.Wrapper.Common
{
    public enum AveMetadataType
    {
        #region Common
        Unknown,
        Security,
        Navigation,
        LanguageFile,
        // add for new wrapper

        UserCache,
        GroupCache,
        AudienceCache,

        UserProfile,
        Users,
        Groups,
        Roles,
        RoleAssignment,
        ItemTableInfo,
        #endregion

        #region WebApp
        WebAppFeature,
        WebAppProperty,
        WebAppPath,
        WebAppPolicyRole,
        WebAppPolicy,

        #endregion

        #region Site
        SiteBasicInfo,
        SiteProperty,
        SiteFeature,
        SiteSearchInfo,

        SearchScope,
        SearchKeywords,
        SiteUserCustomAction,
        #endregion

        #region Web
        WebBasicInfo,
        WebProperty,
        WebFeature,
        WebContentType,
        WebField,
        WebWorkflowAssociation,
        WebCTWorkflowAssociation,
        WebWorkflowInstance,
        WebWorkflowSchedule,
        WebEventReceiver,
        WebWorkflowTemplate,

        WebProjectPolicy,
        SocialFeed, //Added to support backing up / restoring social feeds
        WebUserCustomAction,

        #endregion

        #region List
        ListBasicInfo,
        ListProperty,
        ListField,
        ListContentType,
        ListWorkflowAssociation,
        ListCTWorkflowAssociation,
        ListEventReceiver,
        ListUserCustomAction,
        #endregion

        #region Project

        ProjectCalendar,
        ProjectLookupTable,
        ProjectCustomField,
        ProjectEnterpriseResource,
        ProjectPhase,
        ProjectStage,
        ProjectTimesheet,
        ProjectEnterpriseProjectType,
        ProjectWorkflowAssociation,
        ProjectTimeline,

        #endregion

        #region ListItem
        ListItemInfo,
        #endregion

        #region Doc
        DocProperty,
        DocData,
        DocDataJunction,
        DocWebPart,
        DocImmedSubscriptions,
        DocSchedSubscriptions,
        DocSystemInfo,
        DocRbsId,
        DocStorageInfo,
        DocVersions,
        //For replicator
        LookupFieldGuidValue,
        #endregion

        #region Attachment
        AttachmentData,
        #endregion

        DocumentTagging,
        FullSchemaXml,
        FullTextIndex,
        Report,
        MetadataEnd,

        #region MetadataService
        MetadataService,
        MetadataTermStore,
        MetadataGroup,
        MetadataTermSet,
        MetadataTerm,
        #endregion

        #region User Profile
        UserProfileMembership,
        UserProfileLink,
        UserProfileDetail,
        UserProfileProperties,
        UserProfileTag,
        UserProfileComment,
        UserProfileColleague,
        #endregion

        SocialTag,
        SocialComment,
        ContentTypeHub,
        WorkflowInstance,
        WorkflowSchedule,
        WorkflowTemplate,
        AppPackageInfo,

        #region ExchangeOnline
        ExchangeMailBox,
        ExchangeFolder,
        ExchangeItem,
        ExchangeFolderPermission,
        ExchangeUserConfiguration,
        ExchangeFolderMetadata,
        ExchangeMicrosoftTeams,
        ExchangeMicrosoftTeamsConversationItem,
        ExchangePlannerPlan,
        ExchangePlannerTask,
        ExchangePlannerTaskAttachment,
        ExchangeAttachment,
        ExchangeCalendar,
        ExchangeCalendarEvent,
        #endregion

        TeamsChannel,
        ChatDetails,
        ChatMessageDetails,

        #region DPM Test Run
        ActiveFeature,
        DeActiveFeature,
        DependentFeature,
        #endregion

        ItemMetadataDto,

        #region RP real time event
        ListFieldDelete,
        #endregion
        //Used by Nintex workflow
        ReusableWorkflowTemplate,

        ProjectBasic,

        #region Google Drive
        DriveBasicInfo,
        DriveSetting,
        DriveMembers,
        DriveFolderMetadata,
        DriveFolderPermission,
        DriveFileMetadata,
        DriveFilePermission,
        #endregion

    }

    public class AveMetadata
    {
        protected XmlElement mXmlElement;
        protected AveMetadataType mMetadataType;

        internal AveMetadata()
        { }

        public AveMetadata(XmlElement xmlElement)
        {
            mXmlElement = xmlElement;
            string sname = mXmlElement.GetAttribute(AveWrapperConstants.COLUMN_NAME);
            if (string.IsNullOrEmpty(sname))
            {
                mMetadataType = AveMetadataType.Unknown;
            }
            else
            {
                try
                {
                    mMetadataType = (AveMetadataType)Enum.Parse(typeof(AveMetadataType), sname, true);
                }
                catch(ArgumentException)
                {
                    mMetadataType = AveMetadataType.Unknown;
                }
            }
        }

        [Obsolete("will be removed later")]
        public virtual XmlElement XmlElement
        {
            get { return mXmlElement; }
        }

        public virtual AveMetadataType MetadataType
        {
            get { return mMetadataType; }
        }

        public virtual object GetMetadataObject()
        {
            return AveXmlSerializer.Deserialize(mXmlElement);
        }

        public virtual T GetMetadata<T>()
        {
            return (T)AveXmlSerializer.Deserialize(mXmlElement, typeof(T));
        }

        [Obsolete("will be removed later")]
        public virtual void GetMetadata(object value)
        {
            AveXmlSerializer.Deserialize(mXmlElement, value);
        }

        public virtual void GetMetadata(IDictionary dictionary)
        {
            var dic = GetMetadata<Dictionary<string, object>>();
            if (dic != null && dic.Count > 0)
            {
                dic.Keys.ToList().ForEach(tKey => { dictionary.Add(tKey, dic[tKey]); });
            }
        }

        //only for AveSiteInfo class test
        public static object GetMetadataFromHT(string className, Hashtable ht)
        {
            object obj = null;

            Assembly assembly = Assembly.GetAssembly(typeof(AveMetadata));
            Type type = Type.GetType("AvePoint.Wrapper.Common" + className);
            ConstructorInfo cons = type.GetConstructors()[0];
            obj = cons.Invoke(null);

            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.GetField);

            foreach (string key in ht.Keys)
            {
                string temp = key;
                if (key.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                {
                    temp = key.Substring(1);
                }
                FieldInfo field = type.GetField(temp);
                field.SetValue(obj, ht[key]);
            }

            return obj;
        }

        public static object GetMetadataFromHT(string className, Hashtable ht, Hashtable htMapping)
        {
            object obj = null;

            return obj;
        }
    }
}
