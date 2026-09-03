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
using AvePoint.RA.APIContract;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.PhysicalBrowserService;
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Utils;
using AvePoint.RA.Web.Extentions.Util;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AvePoint.RA.Web.Controllers.API
{
    public class MobileAPIController : MobilePortalApiController
    {
        private RALogger mLogger = RALogger.GetInstance(typeof(MobileAPIController));
        private IExplorerService _ExplorerService;
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService(ref _ExplorerService);
        private IPhysicalReqeustService _PhysicalRequestService;
        private IPhysicalReqeustService PhysicalRequestService => PlatformWindsorManager.GetService(ref _PhysicalRequestService);
        private IPhysicalBrowserService _PhysicalBrowserService;
        private IPhysicalBrowserService PhysicalBrowserService => PlatformWindsorManager.GetService(ref _PhysicalBrowserService);


        private IUserService _UserSerive;
        private IUserService UserSerive => PlatformWindsorManager.GetService(ref _UserSerive);
        private IMobileHistoryDao _MobileHistoryDao;
        private IMobileHistoryDao MobileHistoryDao => PlatformWindsorManager.GetService(ref _MobileHistoryDao);
        private ISecurityService _SecurityService = null;
        private ISecurityService SecurityService
        {
            get
            {
                if (_SecurityService == null)
                {
                    _SecurityService = new SecurityService();
                }
                return _SecurityService;
            }
        }

        [HttpGet]
        public string CheckSession()
        {
            mLogger.Info($"Refresh token for id : {TenantLocalValue.LogonGroupId}.");
            var token = Request.Headers.GetFirstHeaderValue("X_Records_Access_Token");
#if Debug
            //token = HttpUtility.UrlDecode(token);
#endif
            var accessTokenModel = JsonConvert.DeserializeObject<AccessTokenModel>(token);
            var accessToken = ModeConvertUtil.ToAccessToken(accessTokenModel);
            var sessionTimeOutMinute = int.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.MOBILE_SESSION_TIMEOUT_MINUTES]);
            var refreshToken = SecurityService.RefreshToken(accessToken, sessionTimeOutMinute);
            mLogger.Info($"Finish refreshing token for id : {TenantLocalValue.LogonGroupId}.");
            return HttpUtility.UrlEncode(JsonConvert.SerializeObject(refreshToken));
        }

        [HttpPost]
        public async Task<PhysicalObjectDto> Scan([FromBody] string barcode)
        {
            mLogger.Info($"Mobile : Scan barcode : {barcode}.");
            string tenantId = string.Empty, userEmail = string.Empty;
            try
            {
                var claims = HttpContext?.User;
                tenantId = claims?.FindFirst("tenantid")?.Value;
                userEmail = claims?.FindFirst("userEmail")?.Value;
                mLogger.Info($"scan user claims:{tenantId}, {userEmail}");
            }
            catch (Exception ex)
            {
                mLogger.Error($"scan get user claim error:{tenantId}, {userEmail}, error:{ex.ToString()}");
            }
            PhysicalObjectDto record = null;
            try
            {
                if (string.IsNullOrEmpty(barcode))
                {
                    return null;
                }

                //record = await ExplorerService.FindPhysicalObjectByRecordsIdAsync(recordId);
                record = await ExplorerService.FindPhysicalObjectByBarcodeAsync(barcode);
                record ??= await ExplorerService.FindPhysicalObjectByRecordsIdAsync(barcode);

                if(record == null)
                {
                    mLogger.Info($"Mobile : cannot find record by id : {barcode}.");
                    return null;
                }

                record.HomeLocationFullPath = ExplorerService.GetPhysicalObjectFullPath(record.Id);
                var request = new PhysicalRequestDto();
                try
                {
                    request = await PhysicalRequestService.GetRequestByRecordIdAsync(record.UniqueId);
                }
                catch
                {
                    request = null;
                }
                if (request != null)
                {
                    record.HasRequest = true;
                    record.PhysicalRequestDto = request;
                }
                mLogger.Info($"Mobile : finish scan barcode : {barcode}.");
            }
            catch (Exception ex)
            {
                record = null;
                mLogger.Error($"Error in scan barcode, reason : {ex.ToString()}.");
            }
            return record;
        }

        [HttpPost]
        public async Task<string> ApproveLoanRequest([FromBody] MobileApprovalLoanDto requestDto)
        {
            mLogger.Info($"Mobile : Approve loan request : {string.Join(",", requestDto.RequestDtos.Select(r => r.Name))}");
            var logonGroupId = TenantLocalValue.LogonGroupId;
            var logonUserEmail = TenantLocalValue.LogonUserEmail;
            var logonPartnerUser = TenantLocalValue.PartnerUser;
            await Task.Run(async () =>
            {
                TenantLocalValue.LogonGroupId = logonGroupId;
                TenantLocalValue.LogonUserEmail = logonUserEmail;
                TenantLocalValue.PartnerUser = logonPartnerUser;
                try
                {
                    var result = await PhysicalRequestService.ApproveLoanForMobileAsync(requestDto);
                    //if (MobileHistoryDao == null)
                    //{
                    //    mLogger.Info("Init Mobile history dao");
                    //    MobileHistoryDao = new MobileHistoryDao();
                    //}
                    if (!result.HasError)
                    {
                        var histories = requestDto.RequestDtos.Select(r => ConvertToRMMobileHistory(r.Id, r.RecordId, r.Name, (int)AuditStatus.Successful, (int)AuditAction.MobileApprovalLoanRequest)).ToList();
                        MobileHistoryDao.AddHistory(histories);
                    }
                    else
                    {
                        var histories = new List<RMMobileHistory>();
                        foreach (var request in requestDto.RequestDtos)
                        {
                            if (result.FailedIdList != null && result.FailedIdList.Contains(request.RequestId))
                            {
                                var cloneRequestDto = JsonConvert.DeserializeObject<MobileApprovalLoanDto>(JsonConvert.SerializeObject(requestDto));
                                cloneRequestDto.RequestDtos = new List<PhysicalLoanRequestDto4Mobile>() { request };
                                histories.Add(ConvertToRMMobileHistory(request.Id, request.RecordId, request.Name, (int)AuditStatus.Failed, (int)AuditAction.MobileApprovalLoanRequest, JsonConvert.SerializeObject(cloneRequestDto)));
                            }
                            else
                            {
                                histories.Add(ConvertToRMMobileHistory(request.Id, request.RecordId, request.Name, (int)AuditStatus.Successful, (int)AuditAction.MobileApprovalLoanRequest));
                            }
                        }
                        MobileHistoryDao.AddHistory(histories);
                    }
                    mLogger.Info($"Mobile : finish approve loan request.");
                }
                catch (Exception ex)
                {
                    mLogger.Error($"Mobile : Error in  approve loan request, reason : {ex}.");
                }
            });
            return string.Empty;
        }

        [HttpPost]
        public async Task<string> Return([FromBody] List<MobilePhysicalObjectDto> returnDtos)
        {
            var uniqueIds = returnDtos.Select(r => r.Id).ToList();
            mLogger.Info($"Mobile : Return object : {string.Join(",", uniqueIds)}");
            var logonGroupId = TenantLocalValue.LogonGroupId;
            var logonUserEmail = TenantLocalValue.LogonUserEmail;
            var logonPartnerUser = TenantLocalValue.PartnerUser;
            await Task.Run(async () =>
            {
                TenantLocalValue.LogonGroupId = logonGroupId;
                TenantLocalValue.LogonUserEmail = logonUserEmail;
                TenantLocalValue.PartnerUser = logonPartnerUser;
                try
                {
                    var returnMessage = await ExplorerService.RemovePersonalHoldForMobileAsync(uniqueIds);
                    var histories = new List<RMMobileHistory>();
                    if (returnMessage.MessageType == Contract.Object.RAMessageType.Successful)
                    {
                        histories = returnDtos.Select(u => ConvertToRMMobileHistory(u, (int)AuditStatus.Successful, (int)AuditAction.MobileReturn)).ToList();
                    }
                    else
                    {
                        histories = returnDtos.Select(u => ConvertToRMMobileHistory(u, (int)AuditStatus.Failed, (int)AuditAction.MobileReturn, JsonConvert.SerializeObject(u))).ToList();
                    }
                    MobileHistoryDao.AddHistory(histories);
                    mLogger.Info($"Mobile : finish return object.");
                }
                catch (Exception ex)
                {
                    mLogger.Error($"Mobile : Error in return action, reason : {ex.ToString()}.");
                }
            });
            return string.Empty;
        }
        [HttpPost]
        public string ChangeStatus([FromBody] MobileChangeStatusDto requestDto)
        {
            mLogger.Info($"Mobile : Change status for {string.Join(",", requestDto.RecordIds.Select(r => r.Name + "-" + r.Id))}, to {requestDto?.PhysicalRecordStatus}");
            var logonGroupId = TenantLocalValue.LogonGroupId;
            var logonUserEmail = TenantLocalValue.LogonUserEmail;
            var logonPartnerUser = TenantLocalValue.PartnerUser;
            Task.Run(() =>
            {
                TenantLocalValue.LogonGroupId = logonGroupId;
                TenantLocalValue.LogonUserEmail = logonUserEmail;
                TenantLocalValue.PartnerUser = logonPartnerUser;
                try
                {
                    var result = ExplorerService.UpdatePhysicalRecordStatusForMobile(requestDto);
                    var histories = new List<RMMobileHistory>();
                    if (result.MessageType == Contract.Object.RAMessageType.Successful)
                    {
                        histories = requestDto.RecordIds.Select(u => ConvertToRMMobileHistory(u, (int)AuditStatus.Successful, (int)AuditAction.MobileChangeStatus)).ToList();
                    }
                    else
                    {
                        histories = requestDto.RecordIds.Select(u => ConvertToRMMobileHistory(u, (int)AuditStatus.Failed, (int)AuditAction.MobileChangeStatus, JsonConvert.SerializeObject(requestDto))).ToList();
                    }
                    MobileHistoryDao.AddHistory(histories);
                    mLogger.Info("Mobile : finish change status.");
                }
                catch (Exception ex)
                {
                    mLogger.Error($"Mobile : Error in Change status action, reason : {ex.ToString()}."); ;
                }
            });
            return string.Empty;
        }

        [HttpPost]
        public async Task<string> Move([FromBody] MobileMoveDto mobileMoveDto)
        {
            string jobId = "PM" + Guid.NewGuid().ToString();
            mLogger.Info("Mobile : start to move object");
            var logonGroupId = TenantLocalValue.LogonGroupId;
            var logonUserEmail = TenantLocalValue.LogonUserEmail;
            var logonPartnerUser = TenantLocalValue.PartnerUser;
            await Task.Run(async () =>
            {
                TenantLocalValue.LogonGroupId = logonGroupId;
                TenantLocalValue.LogonUserEmail = logonUserEmail;
                TenantLocalValue.PartnerUser = logonPartnerUser;
                try
                {
                    var moveOption = this.ConvertToMoveOption(mobileMoveDto);
                    foreach (var id in mobileMoveDto.SourcePhyRecordIds)
                    {
                        moveOption.SourcePhyRecordIds = new List<Guid>() { id.Id };
                        var returnMessage = await ExplorerService.PhysicalMoveForMobileAsync(moveOption, jobId);
                        var histories = new List<RMMobileHistory>();
                        if (returnMessage.ResultType == ResultType.Success)
                        {
                            histories = new List<RMMobileHistory>() { ConvertToRMMobileHistory(id, (int)AuditStatus.Successful, (int)AuditAction.MobileMove) };
                        }
                        else
                        {
                            var cloneMoveDto = JsonConvert.DeserializeObject<MobileMoveDto>(JsonConvert.SerializeObject(mobileMoveDto));
                            cloneMoveDto.SourcePhyRecordIds = new List<MobilePhysicalObjectDto>() { id };
                            histories = new List<RMMobileHistory>() { ConvertToRMMobileHistory(id, (int)AuditStatus.Failed, (int)AuditAction.MobileMove, JsonConvert.SerializeObject(cloneMoveDto)) };
                        }
                        MobileHistoryDao.AddHistory(histories);
                        mLogger.Info("Mobile : finish moving object");
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Error($"Mobile: Error in move action : reason : {ex.ToString()}.");
                }
            });
            return jobId;
        }

        [HttpPost]
        //返回值先预留string，为了方便以后扩展
        public string Rerun([FromBody] List<int> ids)
        {
            var logonGroupId = TenantLocalValue.LogonGroupId;
            var logonUserEmail = TenantLocalValue.LogonUserEmail;
            var logonPartnerUser = TenantLocalValue.PartnerUser;
            Task.Run(async () =>
            {
                TenantLocalValue.LogonGroupId = logonGroupId;
                TenantLocalValue.LogonUserEmail = logonUserEmail;
                TenantLocalValue.PartnerUser = logonPartnerUser;
                try
                {
                    mLogger.Info($"Rerun action for ids: {string.Join(",", ids)}");
                    var histories = MobileHistoryDao.GetHistoryByIds(ids);
                    foreach (var history in histories)
                    {
                        if (history.Status == (int)AuditStatus.Failed)
                        {
                            await MobileHistoryDao.UpdateHistoryStatusAsync(history.Id, 3);
                            await RerunAsync(history);
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Error($"Error in rerun the action. reason : {ex.ToString()}.");
                }
            });
            return string.Empty;
        }

        [HttpPost]
        public async Task<RMPhysicalExplorerNode> BrowserTree([FromBody] RMPhysicalExplorerNode tree)
        {
            if (tree == null)
            {
                return null;
            }
            RMPhysicalExplorerNode browserResult = new RMPhysicalExplorerNode();
            if (tree.Id.Equals("Root", StringComparison.OrdinalIgnoreCase))
            {
                var t = await PhysicalBrowserService.InitTreeAsync(tree.PagerSize);
                browserResult = t[0];
            }
            else
            {
                browserResult = await PhysicalBrowserService.BrowserAsync(tree);
            }
            RemoveContainerChildren(browserResult);
            return browserResult;
        }

        private void RemoveContainerChildren([FromBody] RMPhysicalExplorerNode browserResult)
        {
            try
            {
                if (browserResult.Children != null && browserResult.Children.Count > 0)
                {
                    var children = browserResult.Children.Where(c => c.NodeType != (int)Contract.RMWeb.Tree.Base.RMNodeLevel.PhysicalCustom).ToList();
                    browserResult.Children = children;
                    browserResult.ChildrenCount = children.Count();
                    browserResult.HasChildren = children.Count() > 0;
                }
            }
            catch (Exception ex)
            {
                mLogger.Error($"Error occurred while removing container children. reason : {ex.ToString()}.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchUser()
        {
            var searchResult = await UserSerive.SearchUsersAsync(TenantLocalValue.LogonGroupId, string.Empty);
            return new JsonResult(searchResult);
        }

        [HttpGet]
        public List<MobileHistoryDto> GetHistory([FromQuery] int pageSize, [FromQuery] int pageIndex)
        {
            var result = new List<MobileHistoryDto>();
            var historys = MobileHistoryDao.GetHistoryByUserId(TenantLocalValue.LogonUserEmail, pageSize, pageIndex);
            result = historys.Select(h => ConvertToPhysicalMobileHistoryDto(h)).ToList();
            return result;
        }

        [HttpGet]
        public List<BarCodeType> GetBarcodeStandards()
        {
            return [BarCodeType.code128, BarCodeType.code39];
        }

        private async Task RerunAsync(RMMobileHistory history)
        {
            switch (history.Action)
            {
                case (int)AuditAction.MobileApprovalLoanRequest:
                    {
                        var requestDto = JsonConvert.DeserializeObject<MobileApprovalLoanDto>(history.Content);
                        var loanRequestDto = requestDto.RequestDtos.FirstOrDefault(r => r.Id == history.PhysicalObjUniqueId);
                        requestDto.RequestDtos = new List<PhysicalLoanRequestDto4Mobile>();
                        requestDto.RequestDtos.Add(loanRequestDto);
                        await this.ApproveLoanRequest(requestDto);
                        break;
                    }
                case (int)AuditAction.MobileReturn:
                    {
                        var requestDto = JsonConvert.DeserializeObject<MobilePhysicalObjectDto>(history.Content);
                        await this.Return(new List<MobilePhysicalObjectDto>() { requestDto });
                        break;
                    }
                case (int)AuditAction.MobileChangeStatus:
                    {
                        var requestDto = JsonConvert.DeserializeObject<MobileChangeStatusDto>(history.Content);
                        var failedObjectInfo = requestDto.RecordIds.FirstOrDefault(r => r.Id == history.PhysicalObjUniqueId);
                        requestDto.RecordIds = new List<MobilePhysicalObjectDto>() { failedObjectInfo };
                        this.ChangeStatus(requestDto);
                        break;
                    }
                case (int)AuditAction.MobileMove:
                    {
                        var requestDto = JsonConvert.DeserializeObject<MobileMoveDto>(history.Content);
                        var faileObjectInfo = requestDto.SourcePhyRecordIds.FirstOrDefault(r => r.Id == history.PhysicalObjUniqueId);
                        requestDto.SourcePhyRecordIds = new List<MobilePhysicalObjectDto>() { faileObjectInfo };
                        await this.Move(requestDto);
                        break;
                    }
                default:
                    break;
            }
        }

        private PhysicalMoveOption ConvertToMoveOption(MobileMoveDto mobileMoveDto)
        {
            if (mobileMoveDto == null) return null;
            var moveOption = new PhysicalMoveOption();
            moveOption.BoxId = mobileMoveDto.BoxId;
            moveOption.LocationId = mobileMoveDto.LocationId;
            moveOption.HoldConflictOption = mobileMoveDto.HoldConflictOption;
            moveOption.NameConflictOption = mobileMoveDto.NameConflictOption;
            moveOption.SourcePhyRecordIds = mobileMoveDto.SourcePhyRecordIds.Select(s => s.Id).ToList();
            return moveOption;
        }

        private RMMobileHistory ConvertToRMMobileHistory(MobilePhysicalObjectDto mobileDto, int status, int action, string content = "")
        {
            return this.ConvertToRMMobileHistory(mobileDto.Id, mobileDto.RecordId, mobileDto.Name, status, action, content);
        }

        private RMMobileHistory ConvertToRMMobileHistory(Guid phyObjectId, string recordId, string name, int status, int action, string content = "")
        {
            RMMobileHistory result = new RMMobileHistory();
            result.PhysicalObjUniqueId = phyObjectId;
            result.Status = status;
            result.RecordId = recordId;
            result.Name = name;
            result.UserEmail = TenantLocalValue.LogonUserEmail;
            result.UserName = TenantLocalValue.LogonUserEmail;
            result.Role = ""; //TODO 
            result.ExecuteOn = DateTime.UtcNow.Ticks;
            result.Action = action;
            result.Content = content;
            return result;
        }

        private MobileHistoryDto ConvertToPhysicalMobileHistoryDto(RMMobileHistory history)
        {
            if (history == null) return null;
            return new MobileHistoryDto()
            {
                Name = history.Name,
                PhysicalObjUniqueId = history.PhysicalObjUniqueId,
                RecordId = history.RecordId,
                Action = history.Action,
                ExecuteOn = history.ExecuteOn,
                Status = history.Status,
                Content = history.Content,
                Id = history.Id
            };
        }

        /*private List<RMPhysicalExplorerNode> ConvertToRMPhysicalExplorerNode(List<RMLocation> mLocations, string parentNodeId)
        {
            List<RMPhysicalExplorerNode> physicalTreeNodes = new List<RMPhysicalExplorerNode>();
            if (mLocations != null && mLocations.Count > 0)
            {
                physicalTreeNodes = mLocations.Select(l => ConvertToRMPhysicalExplorerNode(l, parentNodeId)).ToList();
            }
            return physicalTreeNodes;
        }
*/

    }

    public enum BarCodeType {
      aztec,
      code128,
      code39,
      code39mod43,
      code93,
      ean13,
      ean8,
      pdf417,
      qr,
      upc_e,
      interleaved2of5,
      itf14,
      datamatrix,
    };
}
