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
using AvePoint.GCommon.Contract.Server.ControlPanel.ManagedAccount;
using AvePoint.GCommon.Contract.Server.ControlPanel.ManagedAccount.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.GCommon.Utility.AccountProfile
{
    public class AccountProfileHelper
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(AccountProfileHelper));

        private static IMManagedAccountService _managedAccountService;

        private static Dictionary<string, AccountProfileDto> _cache = new Dictionary<string, AccountProfileDto>();

        public static string AllAccountProfilePwdCrc { get; set; }

        public static void InitManagedAccountService(IMManagedAccountService service)
        {
            _managedAccountService = service;
        }

        /// <summary>
        /// 通过id从缓存中获取AccountProfileDto的方法
        /// 如果不能通过id在缓存中获取AccountProfileDto，则调用IMManagedAccountService取出并放入缓存。
        /// 注：get前请调用InitManagedAccountService方法将IMManagedAccountService具体实现赋值，否则会导致无法取出DB中保存的ProfileDto
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static AccountProfileDto GetAccountProfileById(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    logger.Error("id as parameter is null or empty.");
                    return null;
                }
                lock (_cache)
                {
                    AccountProfileDto dto = null;
                    if (!_cache.TryGetValue(id, out dto))
                    {
                        logger.Debug("the id {0} have no value in account profile cache data, need to get it from db and add to cache.", id);
                        if (_managedAccountService == null)
                        {
                            logger.Error("ManagedAccountService is null.");
                            return null;
                        }
                        dto = _managedAccountService.GetAccountProfileById(id);
                        if (dto != null)
                        {
                            logger.Debug("get account profile from db by id {0} finished.", id);
                            _cache.Add(id, dto);
                        }
                        else
                        {
                            logger.Debug("cannot get account profile from db by id {0}.");
                        }
                    }
                    return dto;
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("get account profile by id error: " + e.Message, e));
                return null;
            }
        }

        /// <summary>
        /// 通过id集合从缓存中获取AccountProfileDto集合的方法
        /// 如果不能通过id在缓存中获取AccountProfileDto，则调用IMManagedAccountService取出并放入缓存
        /// 注：get前请调用InitManagedAccountService方法将IMManagedAccountService具体实现赋值，否则会导致无法取出DB中保存的ProfileDto
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<AccountProfileDto> GetAccountProfilesByIdList(List<string> ids)
        {
            try
            {
                if (ids == null || ids.Count <= 0)
                {
                    logger.Error("ids as parameter is null or count is 0.");
                    return null;
                }
                lock (_cache)
                {
                    List<AccountProfileDto> dtos = new List<AccountProfileDto>();
                    logger.Debug("there have {0} ids to get.", ids.Count());
                    foreach (var id in ids)
                    {
                        AccountProfileDto dto = null;
                        if (!_cache.TryGetValue(id, out dto))
                        {
                            logger.Debug("the id {0} have no value in account profile cache data, need to get it from db and add to cache.", id);
                            if (_managedAccountService == null)
                            {
                                logger.Error("ManagedAccountService is null.");
                            }
                            else
                            {
                                dto = _managedAccountService.GetAccountProfileById(id);
                                if (dto != null)
                                {
                                    logger.Debug("get account profile from db by id {0} finished.", id);
                                    _cache.Add(id, dto);
                                }
                                else
                                {
                                    logger.Debug("cannot get account profile from db by id {0}.");
                                }
                            }
                        }
                        if (dto != null)
                        {
                            dtos.Add(dto);
                        }
                    }
                    logger.Debug("get account profile count is: {0}.", dtos.Count());
                    return dtos;
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("get account profile by id error: " + e.Message, e));
                return null;
            }
        }

        /// <summary>
        /// 向缓存中添加或更新一个AccountProfileDto(主要是管理Account Profile时调用)
        /// </summary>
        /// <param name="profileDto"></param>
        /// <returns>-1 因为过程中出现异常导致失败；0 参数原因失败； 1成功</returns>
        public static int AddOrUpdateAccountProfileToCache(AccountProfileDto profileDto)
        {
            try
            {
                if (profileDto == null || string.IsNullOrEmpty(profileDto.Id))
                {
                    //throw new Exception("account profile as parameter is null or it's Id is null or empty.");
                    logger.Error("id as parameter is null or empty.");
                    return 0;
                }
                lock (_cache)
                {
                    if (_cache.Keys.Contains(profileDto.Id))
                    {
                        logger.Debug("the account profile id {0} is in cache, update it", profileDto.Id);
                        _cache[profileDto.Id] = profileDto;
                    }
                    else
                    {
                        logger.Debug("the account profile id {0} is not in cache, add it", profileDto.Id);
                        _cache.Add(profileDto.Id, profileDto);
                    }
                    logger.Debug("account profile cache count is: {0}", _cache.Count);
                    return 1;
                }
            }
            catch (Exception e)
            {
                logger.Error("add or update account profile to cache error:" + e.Message, e);
                return -1;
            }
        }

        /// <summary>
        /// 根据id移除缓存中的一个AccountProfileDto(主要是管理Account Profile时调用)
        /// </summary>
        /// <param name="id"></param>
        /// <returns>-2 执行失败；-1 因为过程中出现异常导致失败；0 参数原因失败； 1成功</returns>
        public static int DeleteAccountProfileFromCacheById(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    logger.Error("id as parameter is null or empty.");
                    return 0;
                }
                lock (_cache)
                {
                    
                    if (_cache.Keys.Contains(id))
                    {
                        logger.Debug("the account profile id {0} is in cache, delete it", id);
                        return _cache.Remove(id) ? 1 : -2;
                    }
                    else
                    {
                        logger.Debug("the account profile id {0} is not in cache, don't need to delete it", id);
                        return 1;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("delete account profile from ca che by id error:" + e.Message, e);
                return -1;
            }
        }

        /// <summary>
        /// 清空缓存中的所有Account Profile信息
        /// </summary>
        public static void ClearnCacheAccountProfile()
        {
            _cache.Clear();
        }
    }
}
