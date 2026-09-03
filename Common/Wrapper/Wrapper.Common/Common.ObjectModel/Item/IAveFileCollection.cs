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

namespace AvePoint.Wrapper.Common
{
    public interface IAveFileCollection : ICollection, IEnumerable<IAveFile>, IEnumerable
    {
        IAveFile Add(AveFileCreationInformation parameters);
        IAveFile Add(string urlOfFile, AveTemplateFileType templateFileType);
        IAveFile Add(string url, byte[] file, bool overwrite);
        IAveFile Add(string urlOrFile, System.IO.Stream file, bool overwrite);
        IAveFile Add(string urlOfFile, byte[] file, bool overwrite, string checkInComment, bool checkRequiredFields);
        IAveFile Add(string urlOfFile, System.IO.Stream file, bool overwrite, string checkInComment, bool checkRequiredFields);
        IAveFile Add(string urlOfFile, byte[] file);
        IAveFile AddGhosted(string sourceFilePath, string targetFilePath, bool bIsPublishing);
        IAveFile AddStreamInternal(string urlOfFile, Stream stream, bool bIsMigrate, bool bIsPublish, bool bcheckRequiredProps, bool bAutoCheckoutOnInvalidData, bool bForceCreateVersion, string lockIdMatch, IAveUser createdBy, IAveUser modifiedBy, DateTime timeCreated, DateTime timeLastModified, object varProperties, string checkinComment, bool bOverwrite, Stream formatMetadata, string etagToMatch, bool bSyncUpdate, out AveVirusCheckStatus virusCheckStatus, out string virusCheckMessage, out string etagNew);

        IAveFile this[string urlOrFile] { get; }
        IAveFile this[int index] { get; }//
        IAveFolder Folder { get; }
        IAveWeb Web { get; }
        IAveDocumentSerializer DocumentSerializer { get; }

        bool ChangeContent(IAveSite site, IAveFile file, AveDocumentInfo info);
    }

    public interface IAveFileVersionCollection : ICollection, IEnumerable<IAveFileVersion>, IEnumerable
    {
        void DeleteAll();
        void DeleteByID(int vid);
        List<int> DeleteByIDs(List<int> vid);
        void RecycleByID(int vid);
        void DeleteByLabel(string versionlabel);
        IAveFileVersion GetVersionFromID(int versionid);
        IAveFileVersion GetVersionFromLabel(string versionlabel);
        void RestoreByLabel(string versionlabel);

        IAveFileVersion this[int index] { get; }
        IAveWeb Web { get; }
    }

    public interface IAveFileVersion
    {
        string CheckInComment { get; }
        DateTime Created { get; }
        IAveUser CreatedBy { get; }
        int ID { get; }
        bool IsCurrentVersion { get; }
        long Size { get; }
        string Url { get; }
        string VersionLabel { get; }
        void Delete();
        void Recycle();
        Stream OpenBinaryStream();
        byte[] OpenBinary();
        AveFileLevel Level { get; }
        Hashtable Properties { get; }
    }

    public class AveFileCreationInformation
    {
        private byte[] mcontent;
        private bool moverwrite;
        private string murl;

        public byte[] Content
        {
            get
            {
                return this.mcontent;
            }
            set
            {
                this.mcontent = value;
            }
        }

        public bool Overwrite
        {
            get
            {
                return this.moverwrite;
            }
            set
            {
                this.moverwrite = value;
            }
        }

        public string Url
        {
            get
            {
                return this.murl;
            }
            set
            {
                this.murl = value;
            }

        }
    }

    public sealed class AveFileInformation : IDisposable
    {
        private string metag;
        private Stream mstream;

        internal AveFileInformation(Stream stream, string etag)
        {
            this.mstream = stream;
            this.metag = etag;
        }

        public void Dispose()
        {
            if (this.mstream != null)
            {
                this.mstream.Dispose();
                this.mstream = null;
            }
        }

        public string ETag
        {
            get
            {
                return this.metag;
            }
        }

        public Stream Stream
        {
            get
            {
                return this.mstream;
            }
        }
    }

    public enum AveTemplateFileType
    {
        StandardPage,
        WikiPage,
        FormPage
    }

    public enum AveCheckinType
    {
        MinorCheckIn,
        MajorCheckIn,
        OverwriteCheckIn
    }

    public enum AveMoveOperations
    {
        AllowBrokenThickets = 8,
        BypassApprovePermission = 0x40,
        None = 0,
        Overwrite = 1
    }

    public enum AveCheckOutType
    {
        Online,
        Offline,
        None
    }

    public enum AveCustomizedPageStatus
    {
        None,
        Uncustomized,
        Customized
    }

    public enum AveFileLevel : byte
    {
        Checkout = 0xff,
        Draft = 2,
        Published = 1
    }
}
