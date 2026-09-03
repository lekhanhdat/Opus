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

namespace AvePoint.GCommon.GraphAPI
{
    using Microsoft.Graph;
    using Microsoft.Kiota.Abstractions.Serialization;
    using Microsoft.Kiota.Abstractions;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Graph.Models;
    using Microsoft.Graph.Models.ODataErrors;

    public partial class MicrosoftGraphAPIService
    {
        public Microsoft.Graph.Models.User GetMe()
        {
            return graphServiceClient.Me.GetAsync().GetAwaiter().GetResult();
        }
        /// <summary>
        /// 返回第一个匹配项 ID ，或 null
        /// </summary>
        /// <param name="o365GroupMailBox"></param>
        /// <param name="useSDK"></param>
        /// <returns></returns>
        public string GetGroupIdByAddress(String o365GroupMailBox, Boolean? useSDK)
        {
            return graphServiceClient.Groups.GetAsync((request) =>
            {
                request.QueryParameters.Filter = $"mail eq '{ODataSpecialCharactersConverter.ConvertMailForSDK(o365GroupMailBox)}'";
                request.QueryParameters.Select = new string[] { "id" };
                request.QueryParameters.Top = 1;
            }).ConfigureAwait(false).GetAwaiter().GetResult()?.Value.FirstOrDefault()?.Id;
        }
        /// <summary>
        /// 返回第一个匹配项，或 null
        /// </summary>
        /// <param name="o365GroupMailBox"></param>
        /// <param name="useSDK"></param>
        /// <returns></returns>
        public Microsoft.Graph.Models.Group GetGroupInfoByAddress(String o365GroupMailBox, Boolean? useSDK)
        {
            return graphServiceClient.Groups.GetAsync((request) =>
            {
                request.QueryParameters.Filter = $"mail eq '{ODataSpecialCharactersConverter.ConvertMailForSDK(o365GroupMailBox)}'";
                request.QueryParameters.Top = 1;
            }).ConfigureAwait(false).GetAwaiter().GetResult()?.Value.FirstOrDefault();
        }
        /// <summary>
        /// Some group information is hidden by default : allowExternalSenders,autoSubscribeNewMembers,isSubscribedByMail
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public Microsoft.Graph.Models.Group GetGroupHiddenInfo(String groupId)
        {
            return graphServiceClient.Groups[groupId].GetAsync(request => 
            {
                request.QueryParameters.Select = new string[] { "allowExternalSenders","autoSubscribeNewMembers","isSubscribedByMail" };
            }).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public Microsoft.Graph.Models.Group GetGroupInfoById(String groupId)
        {
            return graphServiceClient.Groups[groupId].GetAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public Microsoft.Graph.Models.Group UpdateGroup(String groupId, Microsoft.Graph.Models.Group groupToUpdate)
        {
            return graphServiceClient.Groups[groupId].PatchAsync(groupToUpdate).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public Microsoft.Graph.Models.Group CreateGroup(Microsoft.Graph.Models.Group groupToCreate)
        {
            return graphServiceClient.Groups.PostAsync(groupToCreate).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public Microsoft.Graph.Models.Site GetGroupRootSite(String groupId)
        {
            return graphServiceClient.Groups[groupId].Sites["root"].GetAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public List<Microsoft.Graph.Models.DirectoryObject> GetGroupOwners(String groupId)
        {
            return ListAllEntities<Microsoft.Graph.Models.DirectoryObject,DirectoryObjectCollectionResponse>(
                graphServiceClient,
                () => graphServiceClient.Groups[groupId].Owners.GetAsync(request =>
                    {
                        request.QueryParameters.Select = new string[] { "id", "displayName", "mail", "userPrincipalName", "userType" };
                    }).ConfigureAwait(false).GetAwaiter().GetResult(),
                null
                ).ToList();
        }

        static IEnumerable<TEntity> ListAllEntities<TEntity, TCollectionPage>(
        GraphServiceClient client,
        Func<TCollectionPage> initialCollection,
        Func<RequestInformation, RequestInformation>? requestConfigurator) where TCollectionPage : IParsable, IAdditionalDataHolder, new()
        {
            ArgumentNullException.ThrowIfNull(initialCollection);
            const int CacheCount = 10;
            var cache = new List<TEntity>(CacheCount);
            string? nextlink = default;
            PageIterator<TEntity, TCollectionPage>? iterator =
                PageIterator<TEntity, TCollectionPage>
                .CreatePageIterator(
                    client,
                     initialCollection.Invoke(),
                    (TEntity item) =>
                    {
                        cache.Add(item);
                        return cache.Count < CacheCount;
                    }, requestConfigurator);

            do
            {
                iterator.IterateAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                foreach (var item in cache)
                {
                    yield return item;
                }
                cache.Clear();
                nextlink = iterator.Nextlink;
            }
            while (iterator.State != PagingState.Complete && iterator.State != PagingState.Delta);
        }

        public List<Microsoft.Graph.Models.DirectoryObject> GetGroupMembers(String groupId)
        {
            return ListAllEntities<Microsoft.Graph.Models.DirectoryObject, DirectoryObjectCollectionResponse>(
               graphServiceClient,
               () => graphServiceClient.Groups[groupId].Members.GetAsync(request =>
               {
                   request.QueryParameters.Select = new string[] { "id", "displayName", "mail", "userPrincipalName", "userType" };
               }).ConfigureAwait(false).GetAwaiter().GetResult(),
               null
               ).ToList();
        }


        #region 4 Graph Client Api Request 
        private TResult HandleMGRequest<TIn, TResult>(Func<TIn, Task<TResult>> doTask, TIn requestBody = default(TIn))
        {
            if (null != RetryController)
            {
                return RetryController.Retry(ExcuteSDKRequest, doTask1: doTask, requestBody: requestBody);
            }
            else
            {
                return ExcuteSDKRequest(doTask(requestBody));
            }
        }
        private TResult HandleMGRequest<TResult>(Func<Task<TResult>> doTask)
        {
            if (null != RetryController)
            {
                return RetryController.Retry<object, TResult>(ExcuteSDKRequest, doTask2: doTask);
            }
            else
            {
                return ExcuteSDKRequest(doTask());
            }
        }
        private void HandleMGRequest<TIn>(Func<TIn, Task> doTask, TIn requestBody = default(TIn))
        {
            if (null != RetryController)
            {
                RetryController.Retry<TIn, object>(ExcuteSDKRequest, doTask3: doTask, requestBody: requestBody);
            }
            else
            {
                ExcuteSDKRequest<object>(task2: doTask(requestBody));
            }
        }
        private void HandleMGRequest(Func<Task> doTask)
        {
            if (null != RetryController)
            {
                RetryController.Retry<object, object>(ExcuteSDKRequest, doTask4: doTask);
            }
            else
            {
                ExcuteSDKRequest<object>(task2: doTask());
            }
        }
        #endregion

        /// <summary>
        /// Get Task Result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task1">Task with result</param>
        /// <param name="task2">Task no result</param>
        /// <returns></returns>
        public static TResult ExcuteSDKRequest<TResult>(Task<TResult> task1 = null, Task task2 = null)
        {
            try
            {
                if (null != task1)
                {
                    using (task1)
                    {
                        return task1.GetAwaiter().GetResult();
                    }
                }
                if (null != task2)
                {
                    using (task2)
                    {
                        task2.GetAwaiter().GetResult();
                    }
                }               
            }
            catch (ApiException ex)
            {
                HandleError(ex);
            }
            return default(TResult);
        }

        private static void HandleError(ApiException ex)
        {
            GraphApiErrorRoot result = null;
            var errorString = ex.ToString();
            if (ex is ServiceException se)
            {
                try
                {
                    errorString = se.RawResponseBody;
                    result = new GraphApiErrorRoot()
                    {
                        Error = ToGraphApiError(se),
                    };
                }
                catch (Exception e)
                {
                    Logger.Error($"Error occured while invoke graph api. Error message : {e.Message}");
                }
               
            }
            else if (ex is ODataError ode)
            {
                try
                {
                    result = new GraphApiErrorRoot()
                    {
                        Error = ToGraphApiError(ode),
                    };
                }
                catch (Exception e)
                {
                    Logger.Error($"Error occured while invoke graph api. Error message : {e.Message}");
                }

            }
            result = result ?? new GraphApiErrorRoot { Error = new GraphApiError() { Code = "Unknown", Message = errorString } };
            throw new GraphAPIException((HttpStatusCode)ex.ResponseStatusCode, result);
        }
        /// <summary>
        /// TODO:The exception should be reconsidered
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        private static GraphApiError ToGraphApiError(Microsoft.Graph.ServiceException error)
        {
            if (null == error) return null;
            var apiRoot = new GraphApiError()
            {
                AdditionalData = error.AdditionalData,
                Code = error.Message,
                Message = error.Message,
                InnerError = ToGraphApiError(error.InnerException as Microsoft.Graph.ServiceException),
            };
            return apiRoot;
        }

        /// <summary>
        /// TODO:The exception should be reconsidered
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        private static GraphApiError ToGraphApiError(ODataError error)
        {
            if (null == error) return null;
            var apiRoot = new GraphApiError()
            {
                AdditionalData = error.AdditionalData,
                Code = error.Error.Code,
                Message = error.Message,
                InnerError = ToGraphApiError(error.InnerException as Microsoft.Graph.ServiceException),
            };
            return apiRoot;
        }
    }
}