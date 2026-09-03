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
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Contract.AIExtractionModel;
using Util.AI.Text.Extractor;

public class Program
{
    private static readonly int ExtractFileContentTimeout = 5;
    public static async Task Main(string[] args)
    {
        var msgPath = args[0];

        AIExtractMessage? msg = null;
        try
        {
            var json = await File.ReadAllTextAsync(msgPath);
            msg = Json.Deserialize<AIExtractMessage>(json);
        }
        catch (Exception ex)
        {
        }

        var sw = Stopwatch.StartNew();
        try
        {
            if (string.IsNullOrWhiteSpace(msg.FilePath) || !File.Exists(msg.FilePath))
                throw new FileNotFoundException($"Source file not found: {msg.FilePath}");

            // Open as async FileStream (read-only)
            await using var fs = new FileStream(
                msg.FilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 1024 * 128,
                useAsync: true);

            fs.Position = 0;

            var ext = (msg.Extension ?? string.Empty).Trim();
            IExtract extractor = new Extractor();
            string content = string.Empty;

            //content = AveTenantTasks.ExecuteActionHaveTimeOut(() =>
            //{
            //    var tempContent = extractor.ExtractAsync(fs, ext, new ExtractOption() { MaxCharsCountPerFile = msg.MaxCharsCount }).GetAwaiter().GetResult();
            //    return tempContent;
            //}, ExtractFileContentTimeout);


            var extractTask = Task.Run(async () =>
            {
                return await extractor.ExtractAsync(fs, ext, new ExtractOption
                {
                    MaxCharsCountPerFile = msg.MaxCharsCount
                });
            });

            using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(ExtractFileContentTimeout)))
            {
                try
                {
                    content = await extractTask.WaitAsync(cts.Token);
                }
                catch (TimeoutException)
                {
                    throw new TimeoutException("Extract file timeout");
                }
            }

            var result = new AIExtractResult
            {
                ItemId = msg.ItemId ?? "",
                Succeed = true,
                Content = content,
                ErrorMessage = null,
                ExtractDuration = sw.ElapsedMilliseconds
            };

            await WriteResultJsonSafeAsync(msg.ResultPath, result);
        }
        catch (Exception ex)
        {
            var result = new AIExtractResult
            {
                ItemId = msg?.ItemId ?? "",
                Succeed = false,
                Content = string.Empty,
                ErrorMessage = ex.ToString(),
                FileSize = 0,
                ExtractDuration = sw.ElapsedMilliseconds
            };
            try 
            { 
                await WriteResultJsonSafeAsync(msg?.ResultPath ?? "", result);
            } 
            catch 
            {
                /* ignore */ 
            }
        }
        finally
        {
            sw.Stop();
        }
    }

    private static async Task WriteResultJsonSafeAsync(string path, AIExtractResult res)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("ResultPath is empty.");

        var dir = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(dir))
            throw new ArgumentException("Invalid ResultPath directory.");

        Directory.CreateDirectory(dir);

        var tmp = path + ".tmp";
        await File.WriteAllTextAsync(tmp, Json.Serialize(res));
        if (File.Exists(path))
        {
            File.Delete(path);
        } 
        File.Move(tmp, path);
    }

    private static class Json
    {
        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            WriteIndented = false
        };

        public static string Serialize<T>(T obj) => JsonSerializer.Serialize(obj, _opts);
        public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, _opts);
    }

}
