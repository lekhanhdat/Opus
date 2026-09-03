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
using System.IO;
using System.Web.UI.WebControls.WebParts;

namespace AvePoint.Wrapper.Common
{
    public interface IAveFile
    {
        void Approve(string comment);
        void CheckIn(string comment);
        void CheckIn(string comment, AveCheckinType checkinType);
        void CheckOut();
        void CheckOut(bool checkOutToLocal, string lastModifiedDate);
        void CopyTo(string strNewUrl, bool bOverWrite);
        IAveLimitedWebPartManager GetLimitedWebPartManager(PersonalizationScope scope);
        void MoveTo(string newUrl, AveMoveOperations flags);
        byte[] OpenBinary(AveOpenBinaryOptions openOptions);
        void SaveBinary(Stream file);
        void SaveBinary(byte[] file);
        void SaveBinary(Stream file, bool checkRequiredFields, bool createVersion, string etagMatch, string lockIdMatch, Stream fileFormatMetaInfo, out string etagNew);
        void UndoCheckOut();
        void UnPublish(string comment);
        void Update();
        void Publish(string comment);
        void Delete();
        byte[] OpenBinary();
        Stream OpenBinaryStream();
        Stream OpenBinaryStream(AveOpenBinaryOptions option);
        Stream OpenVersionBinaryStream(int versionId);
        void RevertContentStream();
        Guid Recycle();
        void Deny(string comment);
        IAveUser Author { get; }
        IAveUser CheckedOutByUser { get; }
        AveCheckOutStatus CheckOutStatus { get; }
        string CheckInComment { get; }
        AveCheckOutType CheckOutType { get; }
        AveCustomizedPageStatus CustomizedPageStatus { get; }
        string ETag { get; }
        bool Exists { get; }
        bool InDocumentLibrary { get; }
        IAveListItem Item { get; }
        long Length { get; }
        AveFileLevel Level { get; }
        IAveUser LockedByUser { get; }
        int MajorVersion { get; }
        int MinorVersion { get; }
        IAveUser ModifiedBy { get; }
        string Name { get; }
        Hashtable Properties { get; }
        string ServerRelativeUrl { get; }
        DateTime TimeCreated { get; }
        DateTime TimeLastModified { get; }
        string Title { get; }
        int UIVersion { get; }
        string UIVersionLabel { get; }
        Guid UniqueId { get; }
        IAveFileVersionCollection Versions { get; }
        IAveFolder ParentFolder { get; }
        IAveWeb Web { get; }
        string Url { get; }
        string CharSetName { get; }
        DateTime CheckedOutDate { get; }
        IAveLinkCollection BackwardLinks { get; }
        IAveLinkCollection ForwardLinks { get; }
        void ReplaceLink(string oldUrl, string newUrl);
        #region Add in DocAve 6.7
        string LinkingUrl { get; }
        #endregion
        bool ChangeXSNContent(AveDocumentInfo info, Guid listId, out string publishContentTypeId);
    }
}
