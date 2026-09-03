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
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Model;
using RAGoogle.Models;

namespace RAGoogle.Extension;

public static class EnumerableExtension
{
    public static string Join<T>(this IEnumerable<T> source, string separator)
    {
        return string.Join(separator, source.Select(item => item.ToString()));
    }

    public static bool IsNotEmptyCollection<T>(this IEnumerable<T> source)
    {
        return source != null && source.Any();
    }

    public static async Task ForEachAsync<T>(this IEnumerable<T> enumerable, Func<T, Task> action)
    {
        if (enumerable == null)
        {
            return;
        }
        foreach (var item in enumerable)
        {
            await action(item);
        }
    }

    public static async Task ParallelExecute<TSource>(this IEnumerable<TSource> source,
            Func<TSource, Task> action, int maxThread, CancellationToken cancellationToken = default)
    {
        using (var semaphore = new SemaphoreSlim(maxThread, maxThread))
        {
            foreach (var data in source)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                await semaphore.WaitAsync();
                var task = Task.Run(async () =>
                {
                    try
                    {
                        await action(data);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
            }

            while (!cancellationToken.IsCancellationRequested && semaphore.CurrentCount != maxThread)
                await Task.Delay(1000);
        }
    }

    public static IEnumerable<JMGoogleLabelJobDetails> ConvertToJobDetails(this IEnumerable<RMLabel> sources, JobDetailsStatus status, string tenantId, string action, string message)
    {
        var result = new List<JMGoogleLabelJobDetails>();
        sources = sources.OrderBy(l => l.Name).ToList();
        sources.ForEach(src =>
        {

            result.Add(new JMGoogleLabelJobDetails()
            {
                LabelName = src.Name,
                LabelId = src.Name,
                Action = action,
                TenantId = tenantId,
                Status = status,
                Comment = message,
            });

        });
        return result;
    }
    public static JMGoogleLabelJobDetails ConvertToJobDetail(this RMLabel sources, JobDetailsStatus status, string tenantId, string action, string message)
    {
        return new JMGoogleLabelJobDetails
        {
            LabelName = sources.Name,
            LabelId = sources.Name,
            Action = action,
            TenantId = tenantId,
            Status = status,
            Comment = message,
        };
    }


    public static JMGoogleDataSyncJobDetails ConvertToJobDetail(this GoogleItemData source, JobDetailsStatus status, string tenantId, string action, string message)
    {
        return new JMGoogleDataSyncJobDetails()
        {
            ObjectName = source.Name,
            FullPath = source.RelativePath,
            Status = status,
            Comment = message
        };
    }
    public static JMTermSyncJobDetails ConvertGoogleTermToJobDetail(this RMTerm? sources, JobDetailsStatus status, string tenantUrl, string action, string message)
    {
        return new JMTermSyncJobDetails
        {
            Term = sources?.Name,
            Action = action,
            MMSApplication = tenantUrl,
            Status = status,
            Comment = message,
        };
    }
}
