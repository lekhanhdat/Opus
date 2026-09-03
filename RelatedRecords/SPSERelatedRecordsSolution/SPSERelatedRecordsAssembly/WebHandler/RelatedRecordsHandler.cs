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
using AvePoint.Opus.RelatedRecords.Contract;
using AvePoint.Opus.RelatedRecords.Utilities;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;

namespace AvePoint.Opus.RelatedRecords.WebHandler
{
    public class RelatedRecordsHandler : IHttpHandler
    {
        /// <summary>
        /// You will need to configure this handler in the Web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: https://go.microsoft.com/?linkid=8101007
        /// </summary>

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            try
            {
                if (!SPUtility.ValidateFormDigest())
                {
                    Logger.LogError($"Validate form disgest failed");
                    context.Response.StatusCode = 500;
                    context.Response.Write(JsonConvert.SerializeObject(new { success = false, error = "Invalidate form disgest" }));
                    return;
                }

                RelatedRecordRequestType reqType = int.TryParse(context.Request.QueryString["RequestType"]?.ToString(), out var tempVal) ? (RelatedRecordRequestType)tempVal : RelatedRecordRequestType.None;
                switch (reqType)
                {
                    case RelatedRecordRequestType.SaveOpusApiInfo:
                        var saveOpusApiInfoResult = RelatedAppRequestProcessor.SaveOpusApiInfo(ReadBody<OpusAPIInfo>(context));
                        context.Response.StatusCode = 200;
                        context.Response.Write(JsonConvert.SerializeObject(saveOpusApiInfoResult));
                        return;
                    case RelatedRecordRequestType.SubmitRelateRecords:
                        var result = new RelatedRecordsUtility(ReadBody<RelatedItemSubmit>(context)).SubmitRelatedRecords();
                        context.Response.StatusCode = 200;
                        context.Response.Write(JsonConvert.SerializeObject(result));
                        return;
                    case RelatedRecordRequestType.SearchAllSites:
                        var queryRes = RelatedAppRequestProcessor.QueryRecords(ReadBody<SearchCondition>(context));
                        context.Response.StatusCode = 200;
                        context.Response.Write(JsonConvert.SerializeObject(queryRes));
                        return;
                    case RelatedRecordRequestType.ItemHasEditPermission:
                        var hasPermission = RelatedAppRequestProcessor.CheckItemHasEditPermission(ReadBody<ListItemInfo>(context));
                        context.Response.StatusCode = 200;
                        context.Response.Write(JsonConvert.SerializeObject(hasPermission));
                        return;
                    case RelatedRecordRequestType.TryAddRecord:
                        var tryAddResult = RelatedAppRequestProcessor.TryAddRecord(ReadBody<ListItemInfo>(context));
                        context.Response.StatusCode = 200;
                        context.Response.Write(JsonConvert.SerializeObject(tryAddResult));
                        return;
                    default:
                        Logger.LogError($"Invalidate RequestType: {tempVal}");

                        context.Response.StatusCode = 500;
                        context.Response.Write(JsonConvert.SerializeObject(new { success = false, error = "Invalidate request type" }));
                        return;
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write(JsonConvert.SerializeObject(new { success = false, error = ex.Message }));
                Logger.LogError($"Related Record API error: {ex}");
            }
        }

        private static string ReadBodyAsString(HttpRequest request)
        {
            using (var stream = GetDecodedStream(request))
            using (var reader = new StreamReader(stream, request.ContentEncoding ?? Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            {
                return reader.ReadToEnd();
            }
        }

        // 读取原始字节（自动处理 gzip/deflate）
        //private static byte[] ReadBodyAsBytes(HttpRequest request)
        //{
        //    using (var stream = GetDecodedStream(request))
        //    using (var ms = new MemoryStream())
        //    {
        //        stream.CopyTo(ms);
        //        return ms.ToArray();
        //    }
        //}

        // 处理 Content-Encoding: gzip/deflate
        private static Stream GetDecodedStream(HttpRequest request)
        {
            var input = request.InputStream;
            // 如果上游读取过，可重置位置
            if (input.CanSeek) input.Position = 0;

            var enc = request.Headers["Content-Encoding"] ?? string.Empty;
            if (enc.IndexOf("gzip", StringComparison.OrdinalIgnoreCase) >= 0)
                return new GZipStream(input, CompressionMode.Decompress, leaveOpen: false);
            if (enc.IndexOf("deflate", StringComparison.OrdinalIgnoreCase) >= 0)
                return new DeflateStream(input, CompressionMode.Decompress, leaveOpen: false);

            return input; // 原始流
        }

        // 解析 x-www-form-urlencoded
        //private static object ReadForm(HttpRequest request)
        //{
        //    var dict = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        //    foreach (var key in request.Form.AllKeys)
        //    {
        //        if (key == null) continue;
        //        dict[key] = request.Form[key];
        //    }
        //    return dict;
        //}

        // 解析 multipart 简要信息（文件名、大小、表单域）
        //private static object ReadMultipartSummary(HttpRequest request)
        //{
        //    var files = new System.Collections.Generic.List<object>();
        //    for (int i = 0; i < request.Files.Count; i++)
        //    {
        //        var f = request.Files[i];
        //        files.Add(new
        //        {
        //            name = f.FileName,
        //            key = request.Files.GetKey(i),
        //            contentType = f.ContentType,
        //            contentLength = f.ContentLength
        //        });
        //        // 大文件可直接保存：
        //        // using (var fs = File.Create(Path.Combine(@"D:\Uploads", Path.GetFileName(f.FileName))))
        //        //     f.InputStream.CopyTo(fs);
        //    }

        //    var form = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        //    foreach (var k in request.Form.AllKeys)
        //    {
        //        if (k == null) continue;
        //        form[k] = request.Form[k];
        //    }

        //    return new { form, filesCount = files.Count, files };
        //}

        private static T TryDeserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return default(T);
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new EmptyGuidConverter());
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        //private static string GetTextPreview(byte[] bytes, Encoding enc, int max = 512)
        //{
        //    if (bytes == null || bytes.Length == 0) return string.Empty;
        //    var count = Math.Min(bytes.Length, max);
        //    return enc.GetString(bytes, 0, count);
        //}

        private T ReadBody<T>(HttpContext context)
        {
            var request = context.Request;
            if (request.ContentType?.Contains("application/json") ?? true)
            {
                var json = ReadBodyAsString(request);
                return TryDeserialize<T>(json);
            }
            //else if (ct.IndexOf("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) >= 0)
            //{
            //    data = ReadForm(req);
            //}
            //else if (ct.IndexOf("multipart/form-data", StringComparison.OrdinalIgnoreCase) >= 0)
            //{
            //    data = ReadMultipartSummary(req);
            //}
            else
            {
                Logger.LogError(($"Unsupported Content-Type: {request.ContentType}"));
                //var bytes = ReadBodyAsBytes(req);
                //data = new
                //{
                //    contentType = ct,
                //    length = bytes?.Length ?? 0,
                //    textPreview = GetTextPreview(bytes, req.ContentEncoding ?? Encoding.UTF8)
                //};
                return default(T);
            }
        }
    }





}

