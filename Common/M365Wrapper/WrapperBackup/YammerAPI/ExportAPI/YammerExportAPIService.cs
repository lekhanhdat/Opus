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
namespace ExchangeUtility.Graph
{
    #region namespace
    using ICSharpCode.SharpZipLib.Zip;

    using AvePoint.RA.CommonUtil;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    #endregion
    public class YammerExportAPIService
    {
        protected static RALogger logger = RALogger.GetInstance(typeof(YammerExportAPIService));
        private readonly string apiBaseUrl = "https://export.yammer.com/api/";
        private Func<string> refreshAccessToken;

        public string ExportLocation { get; set; }
        public IYammerRetryable RetryController { get; set; }

        public YammerExportAPIService(Func<string> refreshToken, string exportLocation)
        {
            this.refreshAccessToken = refreshToken;
            this.ExportLocation = exportLocation;
        }

        public ExportResult GetNetworkInfo(string since)
        {
            return GetGroup(since, string.Empty);
        }

        public ExportResult GetGroup(string since, string until)
        {
            return GetExportBase(since, until, new List<string>() { ExportModel.Group.ToString() });
        }

        public ExportResult GetMessage(string since, string until)
        {
            return GetExportBase(since, until, new List<string>() { ExportModel.Message.ToString() });
        }

        public ExportResult GetMessageAndGroup(string since, string until)
        {
            return GetExportBase(since, until, new List<string>() { ExportModel.Group.ToString(), ExportModel.Message.ToString() });
        }

        public ExportResult GetExportBase(string since, string until, List<string> models, ExportInclude include = ExportInclude.csv)
        {
            return new GetExport(apiBaseUrl, refreshAccessToken, RetryController, ExportLocation, since, until, models, include).GetApiResult();
        }

    }

    public class ExportResult
    {
        private long unZipSize = 0L;

        public string Url { get; set; }
        public string ExportFilePath { get; set; }

        public string ErrorMessage { get; set; }

        public HttpStatusCode ErrorCode { get; set; }

        public bool Error
        {
            get { return string.IsNullOrEmpty(this.ExportFilePath); }
        }

        public long ZipSize
        {
            get
            {
                if (this.Error) return 0L;

                return new FileInfo(this.ExportFilePath).Length;
            }
        }

        public long UnZipSize
        {
            get
            {
                if (this.Error) return 0L;
                if (unZipSize == 0L) unZipSize = GetZipFileRealSize();
                return unZipSize;
            }
        }

        private long GetZipFileRealSize()
        {
            long totalSize = 0L;
            using (ZipFile zf = new ZipFile(ExportFilePath))
            {
                var testZip = zf.GetEnumerator();
                while (testZip.MoveNext())
                {
                    totalSize += ((ZipEntry)testZip.Current).Size;
                }
            }
            return totalSize;
        }

        private ExportResult()
        {
        }

        public static ExportResult CreateSuccessfulResult(string url, string exportFilePath)
        {
            return new ExportResult()
            {
                Url = url,
                ExportFilePath = exportFilePath
            };
        }

        public static ExportResult CreateFailedResult(string url, string error)
        {
            return new ExportResult()
            {
                Url = url,
                ErrorMessage = error
            };
        }

        public static ExportResult CreateFailedResult(string url, string error, HttpStatusCode errorCode)
        {
            return new ExportResult()
            {
                Url = url,
                ErrorMessage = $"Error code: {errorCode}.{Environment.NewLine}{error}",
                ErrorCode = errorCode
            };
        }

    }
}