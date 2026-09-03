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



namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AveWrapperReport : IReport
    {
        public bool ReportSuccessfulObject { get; set; }
        public bool DisableWrapperReport { get; set; }
        private readonly List<AveWrapperReportDto> internalReportDtos;
        private AveReportObjectType defaultObjectType = AveReportObjectType.Undefined;
        public AveWrapperReport() : this(0) { }
        public AveWrapperReport(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            this.internalReportDtos = new List<AveWrapperReportDto>(capacity);
            this.DisableWrapperReport = WrapperRuntime.CurrentContext.DisableWrapperReport;
            this.ReportSuccessfulObject = true;
        }
        public AveWrapperReport(AveReportObjectType objectType) : this(0) 
        {
            this.defaultObjectType = objectType;
        }

        public ICollection<AveWrapperReportDto> GetDetails()
        {
            return GetDetails(false);
        }

        public ICollection<AveWrapperReportDto> GetDetails(bool excludeSuccessful)
        {
            if (!excludeSuccessful)
            {
                return this.internalReportDtos;
            }
            return this.internalReportDtos.Where(dto => dto.Status != AveStatus.Successful).ToList();
        }

        #region Add Method
        public bool AddDetail(AveWrapperReportDto dto)
        {
            if (dto == null || !NeedAddToInternalReport(dto))
            {
                return false;
            }
            this.internalReportDtos.Add(dto);
            return true;
        }

        public void AddDetails(ICollection<AveWrapperReportDto> details)
        {
            this.internalReportDtos.AddRange(details.Where(NeedAddToInternalReport));
        }

        public bool AddDetail(string name, string objTitle, AveStatus status, string errorMessage, AveReportObjectType type = AveReportObjectType.Undefined)
        {
            return AddDetail(new AveWrapperReportDto(name, objTitle,
                type == AveReportObjectType.Undefined ? this.defaultObjectType : type, status, errorMessage));
        }
        #endregion

        public void Dispose()
        {
            if (this.internalReportDtos != null && this.internalReportDtos.Count > 0)
            {
                this.internalReportDtos.Clear();
            }
        }

        private bool NeedAddToInternalReport(AveWrapperReportDto dto)
        {
            if (this.DisableWrapperReport)
            {
                return false;
            }
            return this.ReportSuccessfulObject || dto.Status != AveStatus.Successful;
        }
        /// <summary>
        /// Only for udpate webpart common info.
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <param name="UniqueId"></param>
        /// <param name="mappingManager"></param>
        public void UpdateWebpartInfo(string fileUrl, Guid UniqueId, AveSiteMappingManager mappingManager)
        {
            if (internalReportDtos == null)
            {
                return;
            }
            internalReportDtos.ForEach(dto =>
            {
                var webpartReport = dto as AveWrapperWebpartReportDto;
                if (webpartReport != null)
                {
                    webpartReport.DesPageUrl = fileUrl;
                    webpartReport.PageId = UniqueId;
                    if (mappingManager == null)
                    {
                        AveWebPartType value;
                        if(WebPartUpdaterMappings.TryGetValue(webpartReport.WebPartTypeId,out value))
                        {
                            webpartReport.IsCustomizeWebPart = false;
                        }
                    }
                    else
                    {
                        string value;
                        if (mappingManager.TryGetValueFromWebPartTypeIDMapping(webpartReport.WebPartTypeId, out value))
                        {
                            webpartReport.IsCustomizeWebPart = false;
                        }
                    }
                }
            });
        }
    }
}
