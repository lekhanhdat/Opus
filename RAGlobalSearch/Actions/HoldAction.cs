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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using RACloudFS.FSFolderJob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGlobalSearch.Actions
{
    public class HoldAction : IGlobalSearchAction
    {
        public IExplorerService ExplorerService { get; set; }
        private IExplorerDao _explorerDao;
        public IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new ExplorerDao();
                }
                return _explorerDao;
            }
        }
        private GlobalSearchAction mAction;
        public HoldAction(GlobalSearchAction action)
        {
            mAction = action;
        }
        public void DoAction(List<BaseRecordDto> data, SourceFlag flag, object actionExtension)
        {
            if (!IsFSFolder(data[0].Id, data[0].ScopeId))
            {
                switch (mAction)
                {
                    case GlobalSearchAction.PlaceHoldCreate:
                        UpdateHoldDto createDto = SerializerHelper.DeserializeByDataContractSerializer<UpdateHoldDto>(actionExtension.ToString());
                        ExplorerService.CreateHoldWithRecordsForGlobalSearch(data.Select(r => r.Id).ToList(), createDto, null);
                        break;
                    case GlobalSearchAction.PlaceHoldReuse:
                        UpdateHoldDto reuseDto = SerializerHelper.DeserializeByDataContractSerializer<UpdateHoldDto>(actionExtension.ToString());
                        ExplorerService.ReuseHoldWithRecordsForGlobalSearch(data.Select(r => r.Id).ToList(), reuseDto, null);
                        break;
                    case GlobalSearchAction.ChangeHoldCreate:
                        UpdateHoldDto changeDto = SerializerHelper.DeserializeByDataContractSerializer<UpdateHoldDto>(actionExtension.ToString());
                        ExplorerService.ChangeHoldCreateForGlobalSearch(data.Select(r => r.Id).ToList(), changeDto, null);
                        break;
                    case GlobalSearchAction.ChangeHoldReuse:
                        UpdateHoldDto changeDto1 = SerializerHelper.DeserializeByDataContractSerializer<UpdateHoldDto>(actionExtension.ToString());
                        ExplorerService.ChangeHoldReuseForGlobalSearch(data.Select(r => r.Id).ToList(), changeDto1, null);
                        break;
                    case GlobalSearchAction.ExtendHold:
                        UpdateHoldDto extendDto = SerializerHelper.DeserializeByDataContractSerializer<UpdateHoldDto>(actionExtension.ToString());
                        ExplorerService.SusPendWithRecordsForGlobalSearch(data.Select(r => r.Id).ToList(), extendDto);
                        break;
                    case GlobalSearchAction.CancelHold:
                        ExplorerService.CancelHoldWithRecordsForGlobalSearch(data.Select(r => r.Id).ToList(), flag == SourceFlag.Physical);
                        break;
                }
            }
            else
            {
                FSFolderHold fsHold = new FSFolderHold();
                //fsHold.RunForGlobalSearchAction();
            }

            //TODO FSFolder
        }


        private bool IsFSFolder(Guid id, Guid scopeId)
        {
            var record = ExplorerDao.GetFSRecord(scopeId, id);
            if (record != null && record.NodeType == (int)NodeLevel.FSFolder)
            {
                return true;
            }
            return false;
        }

        private HoldOption ConvertHoldSettingToOption(UpdateHoldDto dto, int action)
        {
            DateTime utcReleaseTime = CalculateHoldReleaseTime(dto.HoldSetting);
            HoldOption option = new HoldOption()
            {
                HoldBy = WebUtil.LogOnUserName,
                HoldId = dto.HoldSetting.Id,
                Action = action,
                IsOverWrite = dto.IsOverRide,
                Number = dto.HoldSetting.Number,
                Unit = dto.HoldSetting.Unit,
                ReleaseTime = utcReleaseTime.Ticks,

            };
            return option;
        }

        private int GetHoldAction(GlobalSearchAction globalSearchAction)
        {
            int action = -1;
            switch (globalSearchAction)
            {
                case GlobalSearchAction.PlaceHoldCreate:
                    action = (int)AuditAction.CreateHoldTypeWithRecord;
                    break;
                case GlobalSearchAction.PlaceHoldReuse:
                    action = (int)AuditAction.ReuseHoldTypeWithRecord;
                    break;
                case GlobalSearchAction.ChangeHoldCreate:
                    action = (int)AuditAction.ChangeHoldCreate;
                    break;
                case GlobalSearchAction.ChangeHoldReuse:
                    action = (int)AuditAction.ChangeHoldReuse;
                    break;
                case GlobalSearchAction.ExtendHold:
                    action = (int)AuditAction.SusPendRecords;
                    break;
                case GlobalSearchAction.CancelHold:
                    action = (int)AuditAction.CancelHoldByRecords;
                    break;
            }
            return action;
        }

        private DateTime CalculateHoldReleaseTime(HoldSetting hold)
        {
            if (hold.Type == HoldDateType.Custom)
            {
                DateTime tempNow = new DateTime();
                if (hold.Unit == HoldDateUnit.Day)
                {
                    tempNow = DateTime.UtcNow.AddDays(hold.Number);
                }
                else if (hold.Unit == HoldDateUnit.Week)
                {
                    tempNow = DateTime.UtcNow.AddDays(7 * hold.Number);
                }
                else if (hold.Unit == HoldDateUnit.Month)
                {
                    tempNow = DateTime.UtcNow.AddMonths(hold.Number);
                }
                else if (hold.Unit == HoldDateUnit.Years)
                {
                    tempNow = DateTime.UtcNow.AddYears(hold.Number);
                }
                return tempNow;
            }
            else
            {
                DateTime calenderTime = DateTime.Parse(hold.CalenderTime);
                calenderTime = DateTime.SpecifyKind(calenderTime, DateTimeKind.Unspecified);
                DateTime utcTime = DateTimeUtil.ConvertTimeToUtcDate(calenderTime, TimeZoneInfo.FindSystemTimeZoneById(hold.TimeZoneId), !hold.IsDayLightSaving);
                return utcTime;
            }
        }
    }
}
