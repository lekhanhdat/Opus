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
#nullable enable
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace ExchangeBackupUtility.Graph;

public interface IExchangeItemExporter
{
    Dictionary<string, ExportItemResult> ExportItems(List<string> ids);
}


public class ExportItemResult
{
    public required string Id { get; set; }

    public string? TempFilePath { get; private set; }

    public Stream? Stream { get; private set; }

    public string? ErrorMessage { get; private set; }

    internal ServiceError ErrorCode { get; private set; }

    internal string? GraphErrorCode { get; private set; }

    public bool Error
    {
        get { return string.IsNullOrEmpty(this.TempFilePath) && Stream == null; }
    }

    public bool SkippedError
    {
        get
        {
            return this.Error && ItemNotFound;
        }
    }

    internal bool ItemNotFound => this.ErrorCode == ServiceError.ErrorItemNotFound || string.Equals(this.GraphErrorCode, "itemnotfound", StringComparison.OrdinalIgnoreCase);

    public long Size
    {
        get
        {
            if (this.Error) return 0L;
            return Stream?.Length ?? new FileInfo(this.TempFilePath!).Length;
        }
    }

    private ExportItemResult()
    {
    }

    public static ExportItemResult CreateSuccessfulResult(string id, string tempFilePath)
    {
        return new ExportItemResult() { Id = id, TempFilePath = tempFilePath };
    }

    public static ExportItemResult CreateSuccessfulResult(string id, Stream stream)
    {
        return new ExportItemResult() { Id = id, Stream = stream, TempFilePath = (stream as FileStream)?.Name };
    }

    public static ExportItemResult CreateFailedResult(string id, string error, ServiceError errorCode)
    {
        return new ExportItemResult()
        {
            Id = id,
            ErrorMessage = string.Format("Error code: {0}.{1}{2}", errorCode, Environment.NewLine, error),
            ErrorCode = errorCode
        };
    }
    public static ExportItemResult CreateFailedResult(string id, string error, string? errorCode)
    {
        return new ExportItemResult()
        {
            Id = id,
            ErrorMessage = string.Format("Error code: {0}.{1}{2}", errorCode, Environment.NewLine, error),
            GraphErrorCode = errorCode
        };
    }

    public static ExportItemResult CreateFailedResult(string id, string error)
    {
        return new ExportItemResult()
        {
            Id = id,
            ErrorMessage = error,
        };
    }
}