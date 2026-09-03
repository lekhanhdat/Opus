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
namespace AvePoint.Wrapper.Restore
{
    public interface IAveSPContentTypeCollection
    {
        global::System.Collections.Generic.Dictionary<string, global::AvePoint.Wrapper.Common.AveContentTypeInfo> ContentTypeCache { get; }
        global::AvePoint.Wrapper.Common.IAveContentTypeMapping ContentTypeMapping { get; }
        global::System.Collections.Generic.Dictionary<string, global::AvePoint.Wrapper.Restore.ContentTypeRestoreReport> ContentTypeResult { get; set; }
        global::AvePoint.Wrapper.Common.IAveContentType CreateContentType(global::AvePoint.Wrapper.Common.AveContentTypeInfo ctInfo, global::AvePoint.Wrapper.Restore.AveContentTypeRestoreOption restoreOption);
        void Dispose();
        global::AvePoint.Wrapper.Common.IAveContentType FindContentTypeByIdMapping(global::AvePoint.Wrapper.Common.IAveContentTypeCollection contentTypes, global::AvePoint.Wrapper.Common.IAveContentTypeId ctId);
        global::AvePoint.Wrapper.Common.IAveContentType FindContentTypeByOptions(global::AvePoint.Wrapper.Common.AveContentTypeInfo ctInfo, global::AvePoint.Wrapper.Common.IAveContentTypeCollection collection, global::AvePoint.Wrapper.Restore.ContentTypeFindOption[] findOption);
        global::AvePoint.Wrapper.Common.IAveContentType FindWebContentType(global::AvePoint.Wrapper.Common.AveContentTypeInfo ctInfo, global::AvePoint.Wrapper.Restore.ContentTypeFindOption[] findOptions, global::AvePoint.Wrapper.Restore.ContentTypeFindScope[] findScopes, ref global::AvePoint.Wrapper.Restore.ContentTypeExistStatus status);
        global::AvePoint.Wrapper.Common.IAveContentType GetParentContentType(global::AvePoint.Wrapper.Common.AveContentTypeInfo ctInfo, global::AvePoint.Wrapper.Restore.AveContentTypeRestoreOption restoreOption, bool needCompare);
        global::AvePoint.Wrapper.Common.IReport GetReport();
        void HandleConflict(global::AvePoint.Wrapper.Common.AveContentTypeInfo ctInfo, ref global::AvePoint.Wrapper.Common.IAveContentType contentType, global::AvePoint.Wrapper.Restore.AveContentTypeRestoreOption restoreOption);
        void HandleConflict(global::AvePoint.Wrapper.Common.AveContentTypeInfo ctInfo, ref global::AvePoint.Wrapper.Common.IAveContentType contentType, global::AvePoint.Wrapper.Restore.AveContentTypeRestoreOption restoreOption, bool isHighVersionToLowVersion);
        void LoadContentTypes(global::AvePoint.Wrapper.Common.AveContentTypeCollectionInfo contentTypeInfos);
        void RestoreContentTypes(global::AvePoint.Wrapper.Common.AveContentTypeCollectionInfo contentTypeInfo, global::System.Collections.Generic.Dictionary<string, string> customerRenameTable);
        void RestoreContentTypes(global::AvePoint.Wrapper.Common.AveContentTypeCollectionInfo contentTypeInfo, global::System.Collections.Generic.Dictionary<string, string> customerRenameTable, global::AvePoint.Wrapper.Restore.AveContentTypeRestoreOption restoreOption);
        void RestoreContentTypes(global::AvePoint.Wrapper.Common.AveContentTypeCollectionInfo contentTypeInfos);
        void RestoreContentTypes(global::AvePoint.Wrapper.Common.AveContentTypeCollectionInfo contentTypeInfos, global::AvePoint.Wrapper.Restore.AveContentTypeRestoreOption restoreOption);
    }
}
