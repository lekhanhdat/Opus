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
namespace AvePoint.Media.Service.DomainModel
{
    [Table("ArchiverSiteMasterIndexes")]
    public class ArchiverSiteMasterIndexExportDto : IIndexable
    {
        [Column("Id")]
        public string Id { get; set; }
        [Column("ArchiverTime")]
        public long ArchiverTime { get; set; }
        [Column("JobId")]
        public string JobId { get; set; }
        [Column("SiteURL")]
        public string SiteURL { get; set; }
        [Column("SiteId")]
        public string SiteId { get; set; }
        [Column("SourceFlag")]
        public int SourceFlag { get; set; }
        [Column("GroupMailboxAddress")]
        public string GroupMailboxAddress { get; set; }
        [Column("O365TenantId")]
        public string? O365TenantId { get; set; }

        public Dictionary<string, object> GenerateInsertDatabaseParameters()
        {
            Dictionary<string, Object> dic = new Dictionary<string, Object>()
            {
                {  "Id", this.Id},
                {  "ArchiverTime", this.ArchiverTime},
                {  "JobId", this.JobId},
                {  "SiteURL", this.SiteURL},
                {  "SiteId", this.SiteId},
                {  "SourceFlag", this.SourceFlag},
                {  "GroupMailboxAddress", this.GroupMailboxAddress},
                {  "O365TenantId", this.O365TenantId ?? string.Empty },
            };
            return dic;
        }
    }
}
