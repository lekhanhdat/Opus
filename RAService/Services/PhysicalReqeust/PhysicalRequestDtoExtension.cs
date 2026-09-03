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
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Dao;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace AvePoint.RA.Service.Services.PhysicalReqeust
{
    public static class PhysicalRequestDtoExtension
    {
        /// <summary>
        /// Check if Name is valid, it should be an non empty string.
        /// If invalid, will throw ArgumentException.
        /// </summary>
        /// <param name="dto"></param>
        public static void ValidateName(this PhysicalRequestDto dto)
        {
            var isCheckName = true;
            if(dto.PhysicalFileInfos != null)
            {
                foreach (var physicalFileInfo in dto.PhysicalFileInfos)
                {
                    var value = GetColumnValueInMetaInfo(DefaultColumnIDs.NameOrTitle, physicalFileInfo);
                    if (!string.IsNullOrEmpty(value))
                    {
                        var name = value.Trim();
                        if (!string.IsNullOrEmpty(name))
                        {
                            physicalFileInfo.MetaInfo[DefaultColumnIDs.NameOrTitle] = name;
                            physicalFileInfo.Name = name;
                            continue;
                        }
                    }
                    isCheckName = false;
                    break;
                }
            }
            if(dto.PhysicalFileInfo != null)
            {
                var value = GetColumnValueInMetaInfo(DefaultColumnIDs.NameOrTitle, dto.PhysicalFileInfo);
                if (!string.IsNullOrEmpty(value))
                {
                    var name = value.Trim();
                    if (!string.IsNullOrEmpty(name))
                    {
                        dto.PhysicalFileInfo.MetaInfo[DefaultColumnIDs.NameOrTitle] = name;
                        dto.PhysicalFileInfo.Name = name;
                        return;
                    }
                }
                isCheckName = false;
            }
            if (!isCheckName) throw new ArgumentException("Name is invalid.");
        }

        /// <summary>
        /// Check if Size is valid, it should be either empty or a value of double type equals or greater than TemplateConstants.MinBoxSize.
        /// If invalid, will throw ArgumentException.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="mustHave">indicate if size should have a value</param>
        public static void ValidateSize(this PhysicalRequestDto dto, bool mustHave = true)
        {
            var isCheck = true;
            if(dto.PhysicalFileInfos != null)
            {
                foreach (var physicalFileInfo in dto.PhysicalFileInfos)
                {
                    var value = GetColumnValueInMetaInfo(DefaultColumnIDs.Capability, physicalFileInfo);
                    if (!string.IsNullOrEmpty(value))
                    {
                        value = value.Trim();
                        physicalFileInfo.MetaInfo[DefaultColumnIDs.Capability] = value;
                    }

                    if (string.IsNullOrEmpty(value) && !mustHave) continue;

                    if (double.TryParse(value, out double outvalue) && outvalue >= TemplateConstants.MinBoxSize)
                    {
                        continue;
                    }
                    isCheck = false;
                    break;
                }
            }

            if(dto.PhysicalFileInfo != null)
            {
                var value = GetColumnValueInMetaInfo(DefaultColumnIDs.Capability, dto.PhysicalFileInfo);
                if (!string.IsNullOrEmpty(value))
                {
                    value = value.Trim();
                    dto.PhysicalFileInfo.MetaInfo[DefaultColumnIDs.Capability] = value;
                }

                if (string.IsNullOrEmpty(value) && !mustHave) return;

                if (double.TryParse(value, out double outvalue) && outvalue >= TemplateConstants.MinBoxSize)
                {
                    return;
                }
                isCheck = false;
            }

            if(!isCheck) throw new ArgumentException("Size is invalid.");
        }

        /// <summary>
        /// Check if Status is valid, it should be one of the following values, Active, Closed, Destroyed, and Missing.
        /// If invalid, will throw ArgumentException.
        /// </summary>
        /// <param name="dto"></param>
        public static void ValidateStatus(this PhysicalRequestDto dto)
        {
            var isCheck = true;
            if(dto.PhysicalFileInfos != null)
            {
                foreach (var physicalFileInfo in dto.PhysicalFileInfos)
                {
                    var column = GetColumnInMetaInfo(DefaultColumnIDs.Status, physicalFileInfo);

                    if (column != null && Enum.TryParse(column.Value, out RMRecordStatus status) && RecordStatusHelper.GetDefaultPhysicalStatus().Contains(status))
                    {
                        continue;
                    }
                    isCheck = false;
                    break;
                }
            }

            if(dto.PhysicalFileInfo != null)
            {
                var column = GetColumnInMetaInfo(DefaultColumnIDs.Status, dto.PhysicalFileInfo);

                if (column != null && Enum.TryParse(column.Value, out RMRecordStatus status) && RecordStatusHelper.GetDefaultPhysicalStatus().Contains(status))
                {
                    return;
                }
                isCheck = false;
            }

            if(!isCheck) throw new ArgumentException("Status is invalid.");
        }

        /// <summary>
        /// Check if location is a valid bottom location id.
        /// If invalid, will throw ArgumentException.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="LocationDao"></param>
        public static void ValidateHomeLocation(this PhysicalRequestDto dto, IRMLocationDao LocationDao)
        {
            var isCheck = true;
            if(dto.PhysicalFileInfos != null)
            {
                foreach (var physicalFileInfo in dto.PhysicalFileInfos)
                {
                    var column = GetColumnInMetaInfo(DefaultColumnIDs.HomeLocation, physicalFileInfo);
                    if (column != null)
                    {
                        if (Guid.TryParse(column.Id, out Guid locationId))
                        {
                            var location = LocationDao.GetLocationInfo(locationId);
                            if (location?.NodeType == (int)RMNodeLevel.PhysicalBottomLocation) continue;
                        }
                    }
                    isCheck = false;
                    break;
                }
            }

            if(dto.PhysicalFileInfo != null)
            {
                var column = GetColumnInMetaInfo(DefaultColumnIDs.HomeLocation, dto.PhysicalFileInfo);
                if (column != null)
                {
                    if (Guid.TryParse(column.Id, out Guid locationId))
                    {
                        var location = LocationDao.GetLocationInfo(locationId);
                        if (location?.NodeType == (int)RMNodeLevel.PhysicalBottomLocation) return;
                    }
                }
                isCheck = false;
            }

            if(!isCheck) throw new ArgumentException("Location is invalid.");

        }

        /// <summary>
        /// Check if term is a valid and not be removed term.
        /// If invalid, will throw ArgumentException
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="TermDao"></param>
        public static void ValidateTerm(this PhysicalRequestDto dto, ITermDao TermDao)
        {
            var isCheck = true;
            if (dto.PhysicalFileInfos != null)
            {
                foreach (var physicalFileInfo in dto.PhysicalFileInfos)
                {
                    var column = GetColumnInMetaInfo(DefaultColumnIDs.Classification, physicalFileInfo);
                    if (column != null)
                    {
                        if (Guid.TryParse(column.Id, out Guid termId))
                        {
                            var term = TermDao.GetRMTermByGuId(termId);
                            if (term != null && !term.IsRemoved) continue;
                        }
                    }
                    isCheck = false;
                    break;
                }
            }

            if(dto.PhysicalFileInfo != null)
            {
                var column = GetColumnInMetaInfo(DefaultColumnIDs.Classification, dto.PhysicalFileInfo);
                if (column != null)
                {
                    if (Guid.TryParse(column.Id, out Guid termId))
                    {
                        var term = TermDao.GetRMTermByGuId(termId);
                        if (term != null && !term.IsRemoved) return;
                    }
                }
            }

            if(!isCheck) throw new ClassificationInvalidException("RM_PRM_PRE_InvalidClassificationSetting");
        }

        public static bool HasPhysicalFileInfo(PhysicalObjectDto physicalFileInfo)
        {
            return physicalFileInfo != null;
        }

        public static bool HasMetaInfo(PhysicalObjectDto physicalFileInfo)
        {
            return HasPhysicalFileInfo(physicalFileInfo) && physicalFileInfo.MetaInfo != null;
        }

        public static string GetColumnValueInMetaInfo(string columnId, PhysicalObjectDto physicalFileInfo)
        {
            return HasMetaInfo(physicalFileInfo) && physicalFileInfo.MetaInfo.ContainsKey(columnId) ? physicalFileInfo.MetaInfo[columnId] : null;
        }

        public static CustomColumn GetColumnInMetaInfo(string columnId, PhysicalObjectDto physicalFileInfo)
        {
            var content = GetColumnValueInMetaInfo(columnId, physicalFileInfo);
            return !string.IsNullOrEmpty(content)? JsonConvert.DeserializeObject<CustomColumn>(content): null;
        }

    }
}
