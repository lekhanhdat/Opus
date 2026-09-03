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

//namespace AvePoint.GCommon.GraphAPI
//{
//    using Microsoft.Graph;
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Net;
//    using System.Threading.Tasks;

//    public partial class MicrosoftGraphAPIService
//    {
//        public Microsoft.Graph.User GetMe()
//        {
//            return graphServiceClient.Me.Request().GetAsync().GetAwaiter().GetResult();
//        }
//        /// <summary>
//        /// 返回第一个匹配项 ID ，或 null
//        /// </summary>
//        /// <param name="o365GroupMailBox"></param>
//        /// <param name="useSDK"></param>
//        /// <returns></returns>
//        public string GetGroupIdByAddress(String o365GroupMailBox, Boolean? useSDK)
//        {
//            var request = graphServiceClient.Groups.Request().Top(1).Filter($"mail eq '{ODataSpecialCharactersConverter.ConvertToS(o365GroupMailBox)}'").Select("id");
//            return request.GetAsync().GetAwaiter().GetResult().FirstOrDefault()?.Id;
//        }
//        /// <summary>
//        /// 返回第一个匹配项，或 null
//        /// </summary>
//        /// <param name="o365GroupMailBox"></param>
//        /// <param name="useSDK"></param>
//        /// <returns></returns>
//        public Microsoft.Graph.Group GetGroupInfoByAddress(String o365GroupMailBox, Boolean? useSDK)
//        {
//            var request = graphServiceClient.Groups.Request().Top(1).Filter($"mail eq '{ODataSpecialCharactersConverter.ConvertToS(o365GroupMailBox)}'");
//            return request.GetAsync().GetAwaiter().GetResult().FirstOrDefault();
//        }
//        /// <summary>
//        /// Some group information is hidden by default : allowExternalSenders,autoSubscribeNewMembers,isSubscribedByMail
//        /// </summary>
//        /// <param name="groupId"></param>
//        /// <returns></returns>
//        public Microsoft.Graph.Group GetGroupHiddenInfo(String groupId)
//        {
//            var request = graphServiceClient.Groups[groupId].Request().Select("allowExternalSenders,autoSubscribeNewMembers,isSubscribedByMail");
//            return request.GetAsync().GetAwaiter().GetResult();
//        }

//        public Microsoft.Graph.Group GetGroupInfoById(String groupId)
//        {
//            var request = graphServiceClient.Groups[groupId].Request();
//            return request.GetAsync().GetAwaiter().GetResult();
//        }

//        public Microsoft.Graph.Group UpdateGroup(String groupId, Microsoft.Graph.Group groupToUpdate)
//        {
//            var request = graphServiceClient.Groups[groupId].Request();
//            return request.UpdateAsync(groupToUpdate).GetAwaiter().GetResult();
//        }

//        public Microsoft.Graph.Group CreateGroup(Microsoft.Graph.Group groupToCreate)
//        {
//            var request = graphServiceClient.Groups.Request();
//            return request.AddAsync(groupToCreate).GetAwaiter().GetResult();
//        }

//        public Microsoft.Graph.Site GetGroupRootSite(String groupId)
//        {
//            var request = graphServiceClient.Groups[groupId].Sites["root"].Request();
//            return request.GetAsync().GetAwaiter().GetResult();
//        }

//        public void AddGroupMember(String groupId, Microsoft.Graph.DirectoryObject user, Boolean? useSDK)
//        {
//            var request = graphServiceClient.Groups[groupId].Members.References.Request();
//            request.AddAsync(user).GetAwaiter().GetResult();
//        }
//        public void AddGroupOwner(String groupId, Microsoft.Graph.DirectoryObject user, Boolean? useSDK)
//        {
//            var request = graphServiceClient.Groups[groupId].Owners.References.Request();
//            request.AddAsync(user).GetAwaiter().GetResult();
//        }

//        public Microsoft.Graph.User GetGroupFirstOwner(String groupId)
//        {
//            var request = graphServiceClient.Groups[groupId].Owners.Request().Select("id,displayName,mail,userPrincipalName,userType").Top(1);
//            return (Microsoft.Graph.User)request.GetAsync().GetAwaiter().GetResult().CurrentPage.FirstOrDefault();
//        }

//        public Microsoft.Graph.User GetGroupFirstMember(String groupId)
//        {
//            var request = graphServiceClient.Groups[groupId].Members.Request().Select("id,displayName,mail,userPrincipalName,userType").Top(1);
//            return (Microsoft.Graph.User)request.GetAsync().GetAwaiter().GetResult().CurrentPage.FirstOrDefault();
//        }

//        public List<Microsoft.Graph.DirectoryObject> GetGroupOwners(String groupId)
//        {
//            var owners = new List<Microsoft.Graph.DirectoryObject>();
//            var request = graphServiceClient.Groups[groupId].Owners.Request().Select("id,displayName,mail,userPrincipalName,userType");
//            var ownerColl = request.GetAsync().GetAwaiter().GetResult();
//            owners.AddRange(ownerColl.CurrentPage);
//            while (null != ownerColl.NextPageRequest)
//            {
//                var nextGraphTask = ownerColl.NextPageRequest.GetAsync();
//                ownerColl = nextGraphTask.GetAwaiter().GetResult();
//                owners.AddRange(ownerColl.CurrentPage);
//            }
//            return owners;
//        }
//        public List<Microsoft.Graph.DirectoryObject> GetGroupMembers(String groupId)
//        {
//            var members = new List<Microsoft.Graph.DirectoryObject>();
//            var request = graphServiceClient.Groups[groupId].Members.Request().Select("id,displayName,mail,userPrincipalName,userType");
//            var memberColl = request.GetAsync().GetAwaiter().GetResult();
//            members.AddRange(memberColl.CurrentPage);
//            while (null != memberColl.NextPageRequest)
//            {
//                var nextGraphTask = memberColl.NextPageRequest.GetAsync();
//                memberColl = nextGraphTask.GetAwaiter().GetResult();
//                members.AddRange(memberColl.CurrentPage);
//            }
//            return members;
//        }



      
//        public static TResult ExcuteSDKRequest<TResult>(Task<TResult> task1 = null, Task task2 = null)
//        {
//            try
//            {
//                if (null != task1)
//                {
//                    using (task1)
//                    {
//                        return task1.GetAwaiter().GetResult();
//                    }
//                }
//                if (null != task2)
//                {
//                    using (task2)
//                    {
//                        task2.GetAwaiter().GetResult();
//                    }
//                }
//                return default(TResult);
//            }
//            catch (WebException ex)
//            {
//                throw new WebException(ex.ToString());
//            }
//            catch (Microsoft.Graph.ServiceException ex)
//            {
//                HandleError(ex);
//                throw;
//            }
//            catch (Exception ex)
//            {
//                //Console.WriteLine(ex);
//                throw new Exception(ex.ToString());
//            }
//        }

//        private static void HandleError(Microsoft.Graph.ServiceException ex)
//        {
//            GraphApiErrorRoot result = null;
//            var errorString = ex.Error.ToString();
//            //LogError(errorString, response);
//            if (null != ex.Error)
//            {
//                try
//                {
//                    result = new GraphApiErrorRoot()
//                    {
//                        Error = ToGraphApiError(ex.Error),
//                    };
//                }
//                catch(Exception e)
//                {
//                    Logger.Info($"To Graph API {e.ToString()}");
//                }
//            }
//            result = result ?? new GraphApiErrorRoot { Error = new GraphApiError() { Code = "Unknown", Message = errorString } };
//            throw new GraphAPIException(ex.StatusCode, result);
//        }
//        private static GraphApiError ToGraphApiError(Microsoft.Graph.Error error)
//        {
//            if (null == error) return null;
//            var apiRoot = new GraphApiError()
//            {
//                AdditionalData = error.AdditionalData,
//                Code = error.Code,
//                Message = error.Message,
//                InnerError = ToGraphApiError(error.InnerError),
//            };
//            return apiRoot;
//        }
//    }
//}