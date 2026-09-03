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

namespace AvePoint.RA.SharePoint.RMCustomization4JPMC.Excel
{
    public class SiteStatusSheetDto
    {
        public string SiteID { get; set; }
        public string SiteName { get; set; }
        public string SiteURL { get; set; }
        public string SiteStatus { get; set; }
        public string InformationOwner { get; set; }
        public string AlternateOwner { get; set; }
        public string DataClassification { get; set; }
        public string SiteTemplate { get; set; }
        public string OBR { get; set; }
        public string LOB1 { get; set; }
        public string LOB2 { get; set; }
        public string LOB3 { get; set; }
        public string LOB4 { get; set; }
        public string TotalLibraries { get; set; }
        public string TotalActiveRecords { get; set; }
        public string TotalArchivedRecords { get; set; }
        public string TotalDestroyedRecords { get; set; }
        public string TotalManagedRecords { get; set; }
    }

    public class LibrariesSheetDto
    {
        public string SiteID { get; set; }
        public string LibraryName { get; set; }
        public string LibraryURL { get; set; }
        public string LibraryType { get; set; }
        public string TotalActiveRecords { get; set; }
        public string TotalArchivedRecords { get; set; }
        public string TotalDestroyedRecords { get; set; }
        public string TotalManagedRecords { get; set; }
    }

    public class DERSheetDto
    {
        public string SiteID { get; set; }
        public string SiteURL { get; set; }
        public string InformationOwner { get; set; }
        public string AlternateOwner { get; set; }
        public string RccCountry { get; set; }
        public string RCC { get; set; }
        public string RecordStatus { get; set; }
        public string LOB1 { get; set; }
        public string LOB2 { get; set; }
        public string LOB3 { get; set; }
        public string LOB4 { get; set; }
        public string TotalActiveRecords { get; set; }
        public string TotalRecordVolume { get; set; }
        /// <summary>
        /// 今天Approve数据的Number
        /// </summary>
        public string TotalRecordsEligibleDestructionToday { get; set; }
        /// <summary>
        /// 今天Approve数据的Size
        /// </summary>
        public string TotalRecordsEligibleDestructionTodayVolume { get; set; }
        /// <summary>
        /// 所有待审批的数据的Number
        /// </summary>
        public string TotalRecordsEligibleDisposedTillDate { get; set; }
        /// <summary>
        /// 所有待审批的数据的Size
        /// </summary>
        public string TotalRecordsEligibleDisposedTillDateVolume { get; set; }
        /// <summary>
        /// approve了但是还没有destory数据的Number
        /// </summary>
        public string RecordPendingDestruction0To60Days { get; set; }
        /// <summary>
        /// approve了但是还没有destory数据的Number
        /// </summary>
        public string RecordPendingDestruction60To90Days { get; set; }
        /// <summary>
        /// approve了但是还没有destory数据的Number
        /// </summary>
        public string RecordPendingDestruction90To180Days { get; set; }
        /// <summary>
        /// approve了但是还没有destory数据的Number
        /// </summary>
        public string RecordPendingDestruction180To365Days { get; set; }
        /// <summary>
        /// approve了但是还没有destory数据的Number
        /// </summary>
        public string RecordPendingDestructionThan365Days { get; set; }
    }

    public class RCCSheetDto
    {
        public string SiteID { get; set; }
        public string SiteURL { get; set; }
        public string InformationOwner { get; set; }
        public string AlternateOwner { get; set; }
        public string RccCountry { get; set; }
        public string RecordClassCode { get; set; }
        public string RccStatus { get; set; }
        public string LOB1 { get; set; }
        public string LOB2 { get; set; }
        public string LOB3 { get; set; }
        public string LOB4 { get; set; }
        public string RecordCount { get; set; }
    }

    public class AllSitesSheetDto
    {
        public string SiteID { get; set; }
        public string SiteURL { get; set; }
        public string SiteStatus { get; set; }
        public string SiteName { get; set; }
        public string SiteDescription { get; set; }
        public string SiteOwnerSID { get; set; }
        public string SiteOwnerName { get; set; }
        public string AlternateOwnerSID { get; set; }
        public string AlternateOwnerName { get; set; }
        public string DeveloperSID { get; set; }
        public string JADEClassification { get; set; }
        public string Template { get; set; }
        public string Quota { get; set; }
        public string Size_MB { get; set; }
        public string Version { get; set; }
        public string SiteCreationDate { get; set; }
        public string CostCenter { get; set; }
        public string SortCode { get; set; }
        public string BillingCostCenter { get; set; }
        public string BillingSortCode { get; set; }
        public string AllowCrossLOBCostCenter { get; set; }
        public string RegulatoryCompliance { get; set; }
        public string OBR { get; set; }
        public string LastAttested { get; set; }
        public string LOB1 { get; set; }
        public string LOB2 { get; set; }
        public string LOB3 { get; set; }
        public string LOB4 { get; set; }
        public string EEANexus { get; set; }
        public string ProcessPI { get; set; }
        public string PISource { get; set; }
        public string SEALID { get; set; }
        public string ControllerProcessor { get; set; }
        public string Countries { get; set; }
        public string BusinessPurpose { get; set; }
        public string LegalBase { get; set; }
        public string CategoriesofPIProcessed { get; set; }
        public string CategoryofWorkforce { get; set; }
        public string CategoryofIndividuals { get; set; }
        public string CategoryofCorporateClient_ServiceProvider { get; set; }
        public string CategoriesofInternalRecipientsofData { get; set; }
        public string CategoriesofExternalRecipientofData { get; set; }
        public string Librarians { get; set; }
        public string SiteAdmins { get; set; }
        public string ValidAttestation { get; set; }
        public string ExtendedProperty01 { get; set; }
        public string ExtendedProperty02 { get; set; }
        public string ExtendedProperty03 { get; set; }
        public string ExtendedProperty04 { get; set; }
        public string ExtendedProperty05 { get; set; }
    }
}
