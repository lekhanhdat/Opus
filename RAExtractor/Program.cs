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
using AvePoint.RA.Common.RAProcess.Extractor;
using AvePoint.RA.Contract.ArchivedFullTextIndex;
using Newtonsoft.Json;
using RAExtractor;
using System.Diagnostics;
using System.IO;
using System.Text;
using Util.AI.Text.Extractor;

var extractor = new Extractor();
var messagePath = args[0];

try
{
    await ExtractAsync(messagePath);
}
catch
{

}

async Task ExtractAsync(string filePath)
{
    var messageJson = await File.ReadAllTextAsync(filePath);
    var message = JsonConvert.DeserializeObject<RMArchivedFullTextIndexExtractMessage>(messageJson);
    try
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        string content;
        using (var fileStream = File.OpenRead(message.FilePath))
        {
            var extractTask = Task.Run(async () =>
            {
                return await extractor.ExtractAsync(fileStream, message.FileType, new ExtractOption
                {
                    MaxCharsCountPerFile = message.LetterCountLimit
                });
            });

            using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3)))
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
        }

        stopwatch.Stop();
        var extractedFilePath = message.FilePath + ".extracted";
        if (File.Exists(extractedFilePath))
        {
            File.Delete(extractedFilePath);
        }

        using (var fileStream = File.Create(extractedFilePath))
        {
            await fileStream.WriteAsync(Encoding.UTF8.GetBytes(content));
        }
        await WriteResultAsync(message, extractedFilePath, stopwatch.ElapsedMilliseconds, true);
    }
    catch(Exception e)
    {
        await WriteResultAsync(message, "", -1, false, e.ToString());
    }
}

async Task WriteResultAsync(RMArchivedFullTextIndexExtractMessage message, string extractedFilePath, long extractDuration, bool succeed, string errorMessage = "")
{
    try
    {
        var result = new RMArchivedFullTextIndexExtractResult
        {
            ItemId = message.ItemId,
            FilePath = extractedFilePath,
            FileSize = new FileInfo(message.FilePath).Length,
            ExtractDuration = extractDuration,
            Succeed = succeed,
            ErrorMessage = errorMessage
        };
        var resultJson = JsonConvert.SerializeObject(result);
        await File.WriteAllTextAsync(message.ResultPath, resultJson);
    }
    catch
    {

    }
}