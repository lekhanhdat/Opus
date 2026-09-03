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
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;

    public class GetExport : GetRequest<ExportResult>
    {
        public GetExport(string baseUrl, Func<string> refreshToken, IYammerRetryable retryable, string directoryPath, string since, string until, List<string> models, ExportInclude include) : base(baseUrl, refreshToken, retryable)
        {
            this.DirectoryPath = directoryPath;
            this.Since = since;
            this.Until = until;
            this.Models = models;
            this.Include = include;
        }

        public string DirectoryPath { get; private set; }

        public string Since { get; private set; }

        public string Until { get; private set; }

        public List<string> Models { get; private set; }

        public ExportInclude Include { get; private set; }

        protected override string RequestUrl => GenerateRequestUrl();

        private string GenerateRequestUrl()
        {
            StringBuilder tempUrl = new StringBuilder();
            tempUrl.Append($"{this.apiUrlV1}/export?");
            tempUrl.Append($"since={Since}");
            if (!string.IsNullOrEmpty(Until)) tempUrl.Append($"&until={Until}");
            Models?.ForEach(m => tempUrl.Append($"&model={m}"));
            tempUrl.Append($"&include={Include}");
            return tempUrl.ToString();
        }

        public override ExportResult GetApiResult()
        {
            this.httpMethod = HttpMethod.Get;
            //return this.ExecuteV1(null, this.RequestUrl, DirectoryPath);
            return this.ExecuteV2(null, this.RequestUrl, DirectoryPath);
        }
    }

    public enum ExportInclude
    {
        csv = 0,
        all = 1,
    }

    public enum ExportModel
    {
        User = 0,
        Group = 1,
        Message = 2,
        MessageVersion = 3,
        Topic = 4,
        //Tags = 5, not work in the test
        UploadedFileVersion = 6,
    }
}