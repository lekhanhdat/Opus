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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public interface IAveFieldCollection : ICollection, IEnumerable<IAveField>, IEnumerable
    {
        IAveField Add(IAveField aveField);
        IAveField AddFieldAsXml(String fieldXml);
        IAveField AddFieldAsXml(String fieldXml, bool addToDefaultView, AveAddFieldOptions op);
        bool Contains(Guid fieldId);
        bool ContainsField(string fieldName);
        bool ContainsFieldWithInternalName(string fieldInternalName);
        void Delete(string strName);
        IAveField GetById(Guid id);
        IAveField GetFieldByInternalName(string internalName);
        IAveField GetByInfo(String name, String type);
        string Add(string strDisplayName, AveFieldType fieldType, bool bRequired);
        IAveField AddLookup(string displayName, Guid lookupListId, Guid lookupWebId, bool bRequired);
        IAveField GetField(string strName);
        IAveField TryGetFieldByStaticName(string staticName);

        String SchemaXml { get; }
        IAveField this[int index] { get; }
        IAveField this[Guid id] { get; }
        IAveField this[string name] { get; }
        bool IsDirty { get; set; }
        IAveList List { get; }
        string GetWeb(IAveSite site, Guid webId);
        string GetList(IAveSite site, Guid webId, Guid listId);
        List<string> GetFieldsFromSchema(string fieldSchema);
        List<string> GetFields();
        AveFieldCollectionInfo GetFieldInfoObj();
        AveFieldCollectionInfo GetFieldInfoObj(AveBackupOption backupOption, IAveList list = null, String fieldSchema = "");
        string TransListIdToTitle(IAveWeb aveWeb, IAveList aveList, string xml);
        IAveField CreateNewField(string typeName, string displayName);

        bool ContainsFieldWithStaticName(string p);

        Dictionary<string, object> GetDisplayFields(IAveViewFieldCollection viewFields);

        Dictionary<string, object> GetDisplayFields(string viewFieldsSchema);

        List<string> GetInternalNamesBySchema();

        string GetViewFields(Guid siteID, Guid listID);

        string GetFields(Guid webId, Guid listId);

        List<string> GetFields(Guid siteId, string scope);

        IAveField GetFieldById(Guid fieldId, bool bThrowException);


        IAveField GetFieldByInternalName(string strName, bool bThrowException);

        bool GetFieldInSiteChildren(string scope, Guid siteId, Guid fieldId);

        Dictionary<string, string> GetFieldMap(IAveFieldCollection fields);
    }

    public enum AveAddFieldOptions
    {
        AddFieldCheckDisplayName = 0x20,
        AddFieldInternalNameHint = 8,
        AddFieldToDefaultView = 0x10,
        AddToAllContentTypes = 4,
        AddToDefaultContentType = 1,
        AddToNoContentType = 2,
        DefaultValue = 0
    }

    public enum AveFieldType
    {
        Invalid = 0,
        Integer = 1,
        Text = 2,
        Note = 3,
        DateTime = 4,
        Counter = 5,
        Choice = 6,
        Lookup = 7,
        Boolean = 8,
        Number = 9,
        Currency = 10,
        URL = 11,
        Computed = 12,
        Threading = 13,
        Guid = 14,
        MultiChoice = 15,
        GridChoice = 16,
        Calculated = 17,
        File = 18,
        Attachments = 19,
        User = 20,
        Recurrence = 21,
        CrossProjectLink = 22,
        ModStat = 23,
        Error = 24,
        ContentTypeId = 25,
        PageSeparator = 26,
        ThreadIndex = 27,
        WorkflowStatus = 28,
        AllDayEvent = 29,
        WorkflowEventType = 30,
        Geolocation = 31,
        OutcomeChoice = 32,
        Location = 33,
        Thumbnail = 34,
        MaxItems = 35
    }
}
