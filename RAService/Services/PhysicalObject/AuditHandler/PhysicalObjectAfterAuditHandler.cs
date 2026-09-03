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
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common;

namespace AvePoint.RA.Service.Services.PhysicalObject.AuditHandler
{
    class PhysicalObjectAfterAuditHandler : IAfterAuditHandler
    {
        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            RMAuditInfo auditInfo = info != null ? info : new RMAuditInfo();
            auditInfo.Module = (AuditModule)model;
            auditInfo.Category = (AuditCategory)category;
            ArgumentCheck.NotNull(info, nameof(info));
            auditInfo.Action = info.Action;     //Action属性值在BeforeHandler里判断,因为Service中方法将Add和Update写在一起

            if ((AuditAction)action == AuditAction.AddOrUpdatePhysicalObject)
            {
                PhysicalObjectDto param = args[0] as PhysicalObjectDto;

                switch (auditInfo.Action)
                {
                    case AuditAction.SavePhysicalBox:
                    case AuditAction.SavePhysicalFile:
                    case AuditAction.SavePhysicalRecord:
                    case AuditAction.SavePhysicalContainer:
                        {
                            CollectSave(auditInfo, args, target, returnValue);
                            break;
                        }
                    case AuditAction.UpdatePhysicalBox:
                    case AuditAction.UpdatePhysicalFile:
                    case AuditAction.UpdatePhysicalRecord:
                    case AuditAction.UpdatePhysicalContainer:
                        {
                            CollectUpdate(auditInfo, args, target, returnValue);
                            break;
                        }
                    default:
                        break;
                }
            }
            else if ((AuditAction)action == AuditAction.DeletePhysicalObject)
            {
                CollectDelete(auditInfo, args, target, returnValue);
            }
            else if ((AuditAction)action == AuditAction.RunPhysicalExplorerTimer)
            {
                string reValue = Convert.ToString(returnValue);
                auditInfo.Action = (AuditAction)action;
                auditInfo.UserName = "RM_TS_RunSchedule";
                if (string.IsNullOrEmpty(reValue))
                {
                    info.Status = 1;
                }
                auditInfo.Object = reValue;
            }
            else if ((AuditAction)action == AuditAction.RunConnectorExplorerTimer)
            {
                string reValue = Convert.ToString(returnValue);
                auditInfo.Action = (AuditAction)action;
                auditInfo.UserName = "RM_TS_RunSchedule";
                if (string.IsNullOrEmpty(reValue))
                {
                    info.Status = 1;
                }
                auditInfo.Object = reValue;
            }
            else if ((AuditAction)action == AuditAction.SaveBarcodeStandard)
            {
                auditInfo.Action = (AuditAction)action;
                info.Status = (bool)returnValue ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
            }
            return auditInfo;
        }


        private void CollectDelete(RMAuditInfo auditInfo, object[] args, object target, object returnValue)
        {
            if (args == null || args.Length == 0) return;
            List<PhysicalObjectDto> listDts = args[0] as List<PhysicalObjectDto>;

            StringBuilder sb = new StringBuilder();
            List<string> objectName = new List<string>();
            RMNodeLevel nodeType = RMNodeLevel.PhysicalBox;
            listDts.ForEach(dto =>
            {
                //var physicalObjectDto = ExplorerService.GetPhysicalObjectById(id);
                objectName.Add(dto.Name);
                nodeType = (RMNodeLevel)dto.NodeType;
            });
            string deleteNames = String.Join(";", objectName.ToArray());

            DeleteResultInfo result = returnValue as DeleteResultInfo;
            auditInfo.Object = deleteNames;
            auditInfo.Status = (result == null || result.HasError) ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
            switch (nodeType)
            {
                case RMNodeLevel.PhysicalBox:
                    {
                        auditInfo.Action = AuditAction.DeletePhysicalBox;
                        break;
                    }
                case RMNodeLevel.PhysicalFile:
                    {
                        auditInfo.Action = AuditAction.DeletePhysicalFile;
                        break;
                    }
                case RMNodeLevel.PhysicalRecord:
                    {
                        auditInfo.Action = AuditAction.DeletePhysicalRecord;
                        break;
                    }
                case RMNodeLevel.PhysicalCustom:
                    {
                        auditInfo.Action = AuditAction.DeletePhysicalContainer;
                        break;
                    }
                default:
                    break;
            }
        }


        private void CollectSave(RMAuditInfo auditInfo, object[] args, object target, object returnValue)
        {
            PhysicalObjectDto param = args[0] as PhysicalObjectDto;
            RAReturnMessage result = returnValue as RAReturnMessage;
            auditInfo.Object = param.Name;
            auditInfo.Status = result == null || result.MessageType != RAMessageType.Successful ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
        }

        private void CollectUpdate(RMAuditInfo auditInfo, object[] args, object target, object returnValue)
        {
            PhysicalObjectDto param = args[0] as PhysicalObjectDto;
            RAReturnMessage result = returnValue as RAReturnMessage;
            auditInfo.Object = param.Name;
        }
    }

    internal class AuditCommon
    {
        public static AuditAction GetAuditAction(AuditAction paramAction, PhysicalObjectDto param)
        {

            if (paramAction == AuditAction.AddOrUpdatePhysicalObject)
            {
                AuditAction action = AuditAction.AddOrUpdatePhysicalObject;
                switch (param.NodeType)
                {
                    case RMNodeType.PhyBox:
                        {
                            if (param.Id == new Guid("00000000-0000-0000-0000-000000000000"))
                            {
                                action = AuditAction.SavePhysicalBox;
                            }
                            else if (param.Id != new Guid("00000000-0000-0000-0000-000000000000"))
                            {
                                action = AuditAction.UpdatePhysicalBox;
                            }
                            break;
                        }
                    case RMNodeType.PhyFile:
                        {
                            if (param.Id == new Guid("00000000-0000-0000-0000-000000000000"))
                            {
                                action = AuditAction.SavePhysicalFile;
                            }
                            else if (param.Id != new Guid("00000000-0000-0000-0000-000000000000"))
                            {
                                action = AuditAction.UpdatePhysicalFile;
                            }
                            break;
                        }
                    case RMNodeType.PhyRecord:
                        {
                            if (param.Id == new Guid("00000000-0000-0000-0000-000000000000"))
                            {
                                action = AuditAction.SavePhysicalRecord;
                            }
                            else if (param.Id != new Guid("00000000-0000-0000-0000-000000000000"))
                            {
                                action = AuditAction.UpdatePhysicalRecord;
                            }
                            break;
                        }
                    case RMNodeType.PhyCustom:
                        {
                            if (param.Id == new Guid("00000000-0000-0000-0000-000000000000"))
                            {
                                action = AuditAction.SavePhysicalContainer;
                            }
                            else if (param.Id != new Guid("00000000-0000-0000-0000-000000000000"))
                            {
                                action = AuditAction.UpdatePhysicalContainer;
                            }
                            break;
                        }
                    default:
                        break;
                }
                paramAction = action;
            }
            return paramAction;
        }

    }

}
