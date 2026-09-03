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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Metadata;
using DataExportCore.Export;
using DataExportCore.Utils;
using ExchangeCommonWrapper;
using System.Runtime.Serialization;

namespace DataExportCore
{
    //public class ExchangeDataBlock : IDisposable
    //{
    //    public ExchangeFileHeader FileHeader { get; set; }

    //    public ExchangeRestoreData RestoreData { get; set; }

    //    public RestoreFileTail FileTail { get; set; }

    //    public Boolean IsTimeOut { get; set; }

    //    public Boolean IsException { get; set; }

    //    public String ExceptionMessage { get; set; }

    //    // public List<ExchangeDataBlock> Items { get; set; }

    //    private MetadataEntity HandleMetaData(AveMetadata metadata)
    //    {
    //        if (metadata.MetadataType != AveMetadataType.ExchangeMailBox && metadata.MetadataType != AveMetadataType.ExchangeMicrosoftTeams)
    //        {
    //            var entityString = metadata.GetMetadata<string>();
    //            return ConvertUtil.ConvertToBaseEntity(entityString);
    //        }
    //        else
    //        {
    //            return new MetadataEntity() { DisplayPath = string.Empty };
    //        }
    //    }



    //    public void Dispose()
    //    {
    //        if (this.RestoreData != null)
    //        {
    //            this.RestoreData.Dispose();
    //            this.RestoreData = null;
    //        }
    //    }

    //    public MetadataEntity Metadata
    //    {
    //        get
    //        {
    //            return HandleMetaData(this.RestoreData.MetadataLists.First());
    //        }
    //    }

    //    public T TryGetMetadata<T>(AveMetadataType type) where T : class
    //    {
    //        try
    //        {
    //            var md = RestoreData.MetadataLists.FirstOrDefault(m => m.MetadataType == type);
    //            if (md != null)
    //            {
    //                return SerializerHelper.DeserializeByDataContractSerializer<T>(md.GetMetadata<string>());
    //            }
    //            else
    //            {
    //                return null;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            return null;
    //        }
    //    }
    //}

    //public class ExchangeRestoreData : IDisposable
    //{
    //    public AveMetadata Metadata { get { return this.MetadataLists.First(); } }

    //    public IEnumerable<AveMetadata> MetadataLists { get; set; }

    //    public IRestoreStream RestoreStream { get; set; }

    //    public System.IO.Stream ContentStream { get; set; }

    //    public void Dispose()
    //    {
    //        if (this.RestoreStream != null)
    //        {
    //            this.RestoreStream.Dispose();
    //            this.RestoreStream = null;
    //        }
    //    }
    //}

    //public static class BaseEntityConvertor
    //{
    //    public static MetadataEntity ToBaseEntity(this BaseEntityV2 v2)
    //    {
    //        return new MetadataEntity()
    //        {
    //            ChangeType = v2.ChangeType,
    //            DisplayPath = v2.DisplayPath,
    //            ExchangeId = v2.ExchangeId,
    //            FolderState = v2.FolderState,
    //            Id = v2.Id,
    //            InternalPath = v2.InternalPath,
    //            ItemState = v2.ItemState,
    //            ParentFolderId = v2.ParentFolderId,
    //            RootFolderId = v2.RootFolderId,
    //            SendToMeida = v2.SendToMeida,
    //            Size = v2.Size,
    //            Status = v2.Status,
    //            Title = v2.Title,
    //            Type = v2.Type,
    //            Url = v2.Url,
    //        };
    //    }
    //}
}
