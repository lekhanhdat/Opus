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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using Microsoft.Exchange.WebServices.Data;
using System.Net;
using System.Reflection;
using static RAExportCommon.RecordVEOClassV3;

namespace RAExportCommon
{
    public class EXORecordVEODataV3
    {
        private IRALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private EXOVEOContentXML EXOVEOContentXML = null;
        private EXOVEOHistoryXML EXOVEOHistoryXML = null;
        private Item EXOItem = null;
        private string jobID = string.Empty;
        private string contentExportPath = string.Empty;
        private string mailFullPath = string.Empty;
        private string msgFilePath = string.Empty;
        private string disposalClass = string.Empty;
        EXOVEOV3CustomizeProperty EXOVEOV3CustomizeProperty;

        #region VEO CONTENT
        public EXORecordVEODataV3(EXOVEOContentXML xml)
        {
            EXOVEOV3CustomizeProperty = new EXOVEOV3CustomizeProperty();
            EXOVEOContentXML = xml;
        }

        public void BuildVEOContentData(ref VEOContent vers, Item EXOItem, string jobID, string contentExportPath, string mailFullPath, string msgFilePath, string disposalClass)
        {
            try
            {
                logger.Info($"Start build VEO content data, Item path: {mailFullPath}, Msg file: {Path.GetFileName(contentExportPath)}.");
                this.EXOItem = EXOItem;
                this.jobID = jobID;
                this.contentExportPath = contentExportPath;
                this.disposalClass = disposalClass;
                this.mailFullPath = mailFullPath;
                this.msgFilePath = msgFilePath;
                AddInformationObjectElement(ref vers);
            }
            catch (Exception ex)
            {
                logger.Error($"Build VEO content data failed. Item path: {mailFullPath}, Msg file: {Path.GetFileName(contentExportPath)}. Error: {ex}");
                throw;
            }
        }

        public void AddInformationObjectElement(ref VEOContent vers)
        {
            if (vers.InformationObject == null) { vers.InformationObject = new List<InformationObject>(); };
            InformationObject mInformationObject = new InformationObject();
            foreach(var M4 in EXOVEOContentXML.M1.M4)
            {
                if (M4 != null)
                {
                    mInformationObject.InformationObjectDepth = VEOV3CommonString.NO_DEPTH_STRUCTURE;
                    mInformationObject.InformationObjectType = VEOV3CommonString.OBJECTTYPE_RECORD;
                    AddMetadataPackageElement(ref mInformationObject, M4);
                    AddInformationPieceElement(ref mInformationObject, M4);
                    vers.InformationObject.Add(mInformationObject);
                    logger.Info($"Added a IO in VEOContent file, Path: {contentExportPath}");
                }
            }
        }

        public void AddMetadataPackageElement(ref InformationObject vers, EXORecordVEO_M4_InfomationObject M4)
        {
            MetadataPackage metadataPackage = new MetadataPackage();
            metadataPackage.MetadataSchemaIdentifier = "http://prov.vic.gov.au/vers/schema/ANZS5478";
            metadataPackage.MetadataSyntaxIdentifier = "http://www.w3.org/1999/02/22-rdf-syntax-ns";
            foreach (var item in M4.M7)
            {
                AddRDFElement(ref metadataPackage, item);
            }
            (vers.MetadataPackage ??= []).Add(metadataPackage);
        }

        public void AddInformationPieceElement(ref InformationObject vers, EXORecordVEO_M4_InfomationObject M4)
        {
            foreach(var M68 in M4.M68)
            {
                if (M68 != null)
                {
                    var mInformationPiece = new InformationPiece();
                    var mContentFile = new ContentFile();
                    mContentFile.PathName = this.contentExportPath;
                    mContentFile.HashValue = VEOV3CommonMethod.ComputeHashAsBase64(msgFilePath, VEOV3CommonString.ALGORITHM_SHA512);
                    if(M68.M69 != null)
                    {
                        mInformationPiece.Label = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M68.M69.ExchangeMetadataAsSource, M68.M69.DefaultValue, M68.M69.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                    }
                    mContentFile = VEOV3CommonMethod.FilteredValidValue(mContentFile);
                    if (mContentFile != null) (mInformationPiece.ContentFile ??= []).Add(mContentFile);

                    mInformationPiece = VEOV3CommonMethod.FilteredValidValue(mInformationPiece);
                    if (mInformationPiece != null) (vers.InformationPiece ??= []).Add(mInformationPiece);
                }
            }
        }

        public void AddRDFElement(ref MetadataPackage vers, EXORecordVEO_M7_MetadataPackage M7)
        {
            foreach (var item in M7.M10)
            {
                RDF rdf = new RDF();
                AddRDFDescriptionElement(ref rdf, item);
                (vers.RDF ??= []).Add(rdf);
            }
        }

        public void AddRDFDescriptionElement(ref RDF vers, EXORecordVEO_M10_RDF M10)
        {
            foreach (var M90 in M10.M90)
            {
                AddRDFDescriptionRecordElement(ref vers, M90);
            }
        }

        public void AddRDFDescriptionRecordElement(ref RDF vers, EXORecordVEO_M90_RDFDescription M90)
        {
            if (M90.M11 != null)
            {
                var M11 = M90.M11;
                var recordVers = new Record();
                recordVers.ParseType = VEOV3CommonString.RESOURCE_PARSETYPE;
                recordVers.EntityType = VEOV3CommonString.OBJECTTYPE_RECORD;
                
                AddRDFRecordCategoryElement(ref recordVers, M11);

                AddRDFRecordIdentifierElement(ref recordVers, M11);

                AddRDFRecordNameElement(ref recordVers, M11);

                AddRDFRecordDateRangeElement(ref recordVers, M11);

                AddRDFRecordDescriptionElement(ref recordVers, M11);

                AddRDFRecordJurisdictionElement(ref recordVers, M11);

                AddRDFRecordSecurityClassificationElement(ref recordVers, M11);

                AddRDFRecordSecurityCaveatElement(ref recordVers, M11);

                AddRDFRecordRightsElement(ref recordVers, M11);

                AddRDFRecordLanguageElement(ref recordVers, M11);

                AddRDFRecordCoverageElement(ref recordVers, M11);

                AddRDFRecordKeywordElement(ref recordVers, M11);

                AddRDFRecordDisposalElement(ref recordVers, M11);

                AddRDFRecordFormatElement(ref recordVers, M11);

                AddRDFRecordExtentElement(ref recordVers, M11);

                AddRDFRecordMediumElement(ref recordVers, M11);

                AddRDFRecordIntegrityCheckElement(ref recordVers, M11);

                AddRDFRecordLocationElement(ref recordVers, M11);

                AddRDFRecordDocumentFormElement(ref recordVers, M11);

                AddRDFRecordPrecedenceElement(ref recordVers, M11);

                AddRDFRecordRelationshipElement(ref recordVers, M11);

                var about = $"mailto:{Uri.EscapeDataString(mailFullPath.Replace("\\", "/"))}";

                var anzsDesc = new AnzsDescription
                {
                    about = about,
                    Template = recordVers
                };
                vers.Xmlns = VEOV3CommonMethod.AddXMLNS(RDFTemplate.Record);
                (vers.AnzsDescription ??= []).Add(anzsDesc);
            }
        }

        public void AddRDFRecordCategoryElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            if(M11.M13 != null)
            {
                vers.Category = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M11.M13.ExchangeMetadataAsSource, M11.M13.DefaultValue, M11.M13.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
            }
        }

        public void AddRDFRecordIdentifierElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            foreach (var M14 in M11.M14)
            {
                if (M14 != null)
                {
                    var identifier = new RecordIdentifier();
                    if (M14.M15 != null)
                    {
                        identifier.IdentifierString = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M14.M15.ExchangeMetadataAsSource, M14.M15.DefaultValue, M14.M15.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                    }
                    if (M14.M16 != null)
                    {
                        identifier.IdentifierScheme = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M14.M16.ExchangeMetadataAsSource, M14.M16.DefaultValue, M14.M16.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                    }
                    identifier = VEOV3CommonMethod.FilteredValidValue(identifier);
                    if (identifier != null) (vers.Identifier ??= []).Add(identifier);
                }
            }
        }

        public void AddRDFRecordNameElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            foreach (var M17 in M11.M17)
            {
                if (M17 != null)
                {
                    var recordName = new RecordName();
                    if (M17.M18 != null)
                    {
                        recordName.NameWords = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M17.M18.ExchangeMetadataAsSource, M17.M18.DefaultValue, M17.M18.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                    }
                    if (M17.M19 != null)
                    {
                        recordName.NameScheme = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M17.M19.ExchangeMetadataAsSource, M17.M19.DefaultValue, M17.M19.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                    }
                    recordName = VEOV3CommonMethod.FilteredValidValue(recordName);
                    if (recordName != null) (vers.Name ??= []).Add(recordName);
                }
            }

        }

        public void AddRDFRecordDateRangeElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            if (M11.M20 != null)
            {
                var mDateRange = new RecordDateRange();
                if (M11.M20.M21 != null)
                {
                    mDateRange.StartDate = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M11.M20.M21.ExchangeMetadataAsSource, M11.M20.M21.DefaultValue, M11.M20.M21.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                if (M11.M20.M22 != null)
                {
                    mDateRange.EndDate = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M11.M20.M22.ExchangeMetadataAsSource, M11.M20.M22.DefaultValue, M11.M20.M22.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                vers.DateRange = VEOV3CommonMethod.FilteredValidValue(mDateRange);
            }
        }

        public void AddRDFRecordDescriptionElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            vers.Description = new List<string>();
            foreach (var M23 in M11.M23)
            {
                vers.Description.Add(EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M23.ExchangeMetadataAsSource, M23.DefaultValue, M23.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass));
            }
            vers.Description = VEOV3CommonMethod.FilteredValidValue(vers.Description);
        }

        public void AddRDFRecordJurisdictionElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            vers.Jurisdiction = new List<string>();
            foreach (var M24 in M11.M24)
            {
                vers.Jurisdiction.Add(EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M24.ExchangeMetadataAsSource, M24.DefaultValue, M24.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass));
            }
            vers.Jurisdiction = VEOV3CommonMethod.FilteredValidValue(vers.Jurisdiction);
        }

        public void AddRDFRecordSecurityClassificationElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            vers.SecurityClassification = new List<string>();
            foreach (var M25 in M11.M25)
            {
                vers.SecurityClassification.Add(EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M25.ExchangeMetadataAsSource, M25.DefaultValue, M25.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass));
            }
            vers.SecurityClassification = VEOV3CommonMethod.FilteredValidValue(vers.SecurityClassification);
        }

        public void AddRDFRecordSecurityCaveatElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            foreach (var M26 in M11.M26)
            {
                var mSecurityCaveat = new RecordSecurityCaveat();
                if (M26.M27 != null)
                {
                    mSecurityCaveat.CaveatText = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M26.M27.ExchangeMetadataAsSource, M26.M27.DefaultValue, M26.M27.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                if (M26.M28 != null)
                {
                    mSecurityCaveat.CaveatCategory = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M26.M28.ExchangeMetadataAsSource, M26.M28.DefaultValue, M26.M28.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                mSecurityCaveat = VEOV3CommonMethod.FilteredValidValue(mSecurityCaveat);
                if (mSecurityCaveat != null) (vers.SecurityCaveat ??= []).Add(mSecurityCaveat);
            }
        }

        public void AddRDFRecordRightsElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            foreach (var M29 in M11.M29)
            {
                var mRights = new RecordRights();
                if (M29.M30 != null)
                {
                    AddRDFRecordRightsRightsStatementElement(ref mRights, M29);
                }
                if (M29.M31 != null)
                {
                    mRights.RightsType = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M29.M31.ExchangeMetadataAsSource, M29.M31.DefaultValue, M29.M31.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                if (M29.M32 != null)
                {
                    mRights.RightsStatus = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M29.M32.ExchangeMetadataAsSource, M29.M32.DefaultValue, M29.M32.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                mRights = VEOV3CommonMethod.FilteredValidValue(mRights);
                if (mRights != null) (vers.Rights ??= []).Add(mRights);
            }
        }

        public void AddRDFRecordRightsRightsStatementElement(ref RecordRights mRights, EXORecordVEO_M29_RDFRecord_Rights M29)
        {
            mRights.RightsStatement = new List<string>();
            foreach (var M30 in M29.M30)
            {
                mRights.RightsStatement.Add(EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M30.ExchangeMetadataAsSource, M30.DefaultValue, M30.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass));
            }
            mRights.RightsStatement = VEOV3CommonMethod.FilteredValidValue(mRights.RightsStatement);
        }

        public void AddRDFRecordLanguageElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            vers.Language = new List<string>();
            foreach (var M33 in M11.M33)
            {
                vers.Language.Add(EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M33.ExchangeMetadataAsSource, M33.DefaultValue, M33.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass));
            }
            vers.Language = VEOV3CommonMethod.FilteredValidValue(vers.Language);
        }

        public void AddRDFRecordCoverageElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            foreach (var M34 in M11.M34)
            {
                var mCoverage = new RecordCoverage();

                if (M34.M35 != null)
                {
                    foreach (var M35 in M34.M35)
                    {
                        var value = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M35.ExchangeMetadataAsSource, M35.DefaultValue, M35.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                        if (string.IsNullOrWhiteSpace(value)) continue;
                        (mCoverage.JurisdictionalCoverage ??= []).Add(value);
                    }
                }
                if (M34.M36 != null)
                {
                    foreach (var M36 in M34.M36)
                    {
                        var value = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M36.ExchangeMetadataAsSource, M36.DefaultValue, M36.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                        if (string.IsNullOrWhiteSpace(value)) continue;
                        (mCoverage.TemporalCoverage ??= []).Add(value);

                    }
                }
                if (M34.M37 != null)
                {
                    foreach (var M37 in M34.M37)
                    {
                        var value = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M37.ExchangeMetadataAsSource, M37.DefaultValue, M37.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                        if (string.IsNullOrWhiteSpace(value)) continue;
                        (mCoverage.SpatialCoverage ??= []).Add(value);
                    }
                }
                mCoverage = VEOV3CommonMethod.FilteredValidValue(mCoverage);
                if (mCoverage != null) (vers.Coverage ??= []).Add(mCoverage);
            }
        }

        public void AddRDFRecordKeywordElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            foreach (var M38 in M11.M38)
            {
                var mKeyword = new RecordKeyword();
                if (M38.M39 != null)
                {
                    mKeyword.KeywordTerm = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M38.M39.ExchangeMetadataAsSource, M38.M39.DefaultValue, M38.M39.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                if (M38.M40 != null)
                {
                    mKeyword.KeywordID = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M38.M40.ExchangeMetadataAsSource, M38.M40.DefaultValue, M38.M40.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                if (M38.M41 != null)
                {
                    mKeyword.KeywordScheme = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M38.M41.ExchangeMetadataAsSource, M38.M41.DefaultValue, M38.M41.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }

                if (M38.M42 != null)
                {
                    mKeyword.KeywordSchemeType = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M38.M42.ExchangeMetadataAsSource, M38.M42.DefaultValue, M38.M42.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                mKeyword = VEOV3CommonMethod.FilteredValidValue(mKeyword);
                if (mKeyword != null) (vers.Keyword ??= []).Add(mKeyword);
            }
        }

        public void AddRDFRecordDisposalElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            foreach (var M43 in M11.M43)
            {
                var mDisposal = new RecordDisposal();

                if (M43.M44 != null)
                {
                    mDisposal.RetentionAndDisposalAuthority = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M43.M44.ExchangeMetadataAsSource, M43.M44.DefaultValue, M43.M44.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                    if (!string.IsNullOrEmpty(mDisposal.RetentionAndDisposalAuthority) && mDisposal.RetentionAndDisposalAuthority.Equals(VEOV3CommonString.NO_DISPOSAL_COVERAGE, StringComparison.OrdinalIgnoreCase))
                    {
                        (vers.Disposal ??= []).Add(mDisposal);
                        continue;
                    }
                }
                if (M43.M45 != null)
                {
                    mDisposal.DisposalClassID = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M43.M45.ExchangeMetadataAsSource, M43.M45.DefaultValue, M43.M45.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                if (M43.M46 != null)
                {
                    mDisposal.DisposalAction = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M43.M46.ExchangeMetadataAsSource, M43.M46.DefaultValue, M43.M46.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                if (M43.M47 != null)
                {
                    AddRDFRecordRecordDisposalDisposalTriggerDateElement(ref mDisposal, M43);
                }
                if (M43.M48 != null)
                {
                    AddRDFRecordRecordDisposalDisposalActionDueElement(ref mDisposal, M43);
                }
                mDisposal = VEOV3CommonMethod.FilteredValidValue(mDisposal);
                if (mDisposal != null) (vers.Disposal ??= []).Add(mDisposal);
            }
        }

        public void AddRDFRecordRecordDisposalDisposalTriggerDateElement(ref RecordDisposal mDisposal, EXORecordVEO_M43_RDFRecord_Disposal M43)
        {
            mDisposal.DisposalTriggerDate = new List<string>();
            foreach (var M47 in M43.M47)
            {
                mDisposal.DisposalTriggerDate.Add(EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M47.ExchangeMetadataAsSource, M47.DefaultValue, M47.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass));
            }
            mDisposal.DisposalTriggerDate = VEOV3CommonMethod.FilteredValidValue(mDisposal.DisposalTriggerDate);
        }

        public void AddRDFRecordRecordDisposalDisposalActionDueElement(ref RecordDisposal mDisposal, EXORecordVEO_M43_RDFRecord_Disposal M43)
        {
            mDisposal.DisposalActionDue = new List<string>();
            foreach (var M48 in M43.M48)
            {
                mDisposal.DisposalActionDue.Add(EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M48.ExchangeMetadataAsSource, M48.DefaultValue, M48.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass));
            }
            mDisposal.DisposalActionDue = VEOV3CommonMethod.FilteredValidValue(mDisposal.DisposalActionDue);
        }


        public void AddRDFRecordFormatElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            if (M11.M49 != null)
            {
                var mFormat = new RecordFormat();
                if (M11.M49.M50 != null)
                {
                    mFormat.FormatName = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M11.M49.M50.ExchangeMetadataAsSource, M11.M49.M50.DefaultValue, M11.M49.M50.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                if (M11.M49.M51 != null)
                {
                    mFormat.FormatVersion = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M11.M49.M51.ExchangeMetadataAsSource, M11.M49.M51.DefaultValue, M11.M49.M51.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                if (M11.M49.M52 != null)
                {
                    mFormat.CreatingApplicationName = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M11.M49.M52.ExchangeMetadataAsSource, M11.M49.M52.DefaultValue, M11.M49.M52.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                if (M11.M49.M53 != null)
                {
                    mFormat.CreatingApplicationVersion = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M11.M49.M53.ExchangeMetadataAsSource, M11.M49.M53.DefaultValue, M11.M49.M53.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                if (M11.M49.M54 != null)
                {
                    mFormat.FormatRegistry = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M11.M49.M54.ExchangeMetadataAsSource, M11.M49.M54.DefaultValue, M11.M49.M54.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                if (M11.M49.M55 != null)
                {
                    mFormat.FormatRegistryID = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M11.M49.M55.ExchangeMetadataAsSource, M11.M49.M55.DefaultValue, M11.M49.M55.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                vers.Format = VEOV3CommonMethod.FilteredValidValue(mFormat);
            }
        }

        public void AddRDFRecordExtentElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            foreach (var M56 in M11.M56)
            {
                var mExtent = new RecordExtent();
                if (M56.M57 != null)
                {
                    mExtent.PhysicalDimensions = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M56.M57.ExchangeMetadataAsSource, M56.M57.DefaultValue, M56.M57.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                if (M56.M58 != null)
                {
                    mExtent.LogicalSize = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M56.M58.ExchangeMetadataAsSource, M56.M58.DefaultValue, M56.M58.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                if (M56.M59 != null)
                {
                    mExtent.Quantity = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M56.M59.ExchangeMetadataAsSource, M56.M59.DefaultValue, M56.M59.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                if (M56.M60 != null)
                {
                    mExtent.Units = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M56.M60.ExchangeMetadataAsSource, M56.M60.DefaultValue, M56.M60.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                if (mExtent.LogicalSize != null && mExtent.Units != null)
                {
                    if (long.TryParse(mExtent.LogicalSize, out long size))
                    {
                        mExtent.LogicalSize = Math.Round(VEOV3CommonMethod.AutoFitSizeUnit(size, mExtent.Units), 2).ToString();
                    }
                }
                mExtent = VEOV3CommonMethod.FilteredValidValue(mExtent);
                if (mExtent != null) (vers.Extent ??= []).Add(mExtent);
            }
        }

        public void AddRDFRecordMediumElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            if (M11.M61 != null)
            {
                vers.Medium = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M11.M61.ExchangeMetadataAsSource, M11.M61.DefaultValue, M11.M61.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
            }

        }

        public void AddRDFRecordIntegrityCheckElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            if (M11.M62 != null)
            {
                if (M11.M62.M63 != null)
                {
                    var mIntegrityCheck = new RecordIntegrityCheck();
                    try
                    {
                        mIntegrityCheck.HashFunctionName = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M11.M62.M63.ExchangeMetadataAsSource, M11.M62.M63.DefaultValue, M11.M62.M63.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                        if (string.IsNullOrWhiteSpace(mIntegrityCheck.HashFunctionName))
                        {
                            return;
                        }
                        mIntegrityCheck.MessageDigest = VEOV3CommonMethod.ComputeHashAsBase64(this.msgFilePath, mIntegrityCheck.HashFunctionName);
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                    vers.IntegrityCheck = VEOV3CommonMethod.FilteredValidValue(mIntegrityCheck);
                }
            }
        }

        public void AddRDFRecordLocationElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            if (M11.M65 != null)
            {
                var locations = new List<string>();
                foreach (var M65 in M11.M65)
                {
                    locations.Add(EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M65.ExchangeMetadataAsSource, M65.DefaultValue, M65.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass));
                }
                vers.Location = VEOV3CommonMethod.FilteredValidValue(locations);
            }
        }

        public void AddRDFRecordDocumentFormElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            if (M11.M66 != null)
            {
                vers.DocumentForm = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M11.M66.ExchangeMetadataAsSource, M11.M66.DefaultValue, M11.M66.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
            }
        }

        public void AddRDFRecordPrecedenceElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            if (M11.M67 != null)
            {
                vers.Precedence = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M11.M67.ExchangeMetadataAsSource, M11.M67.DefaultValue, M11.M67.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
            }
        }

        public void AddRDFRecordRelationshipElement(ref Record vers, EXORecordVEO_M11_RDFDescriptionRecord M11)
        {
            if (M11.M73 != null)
            {
                vers.Relationship = new Relationship();
                vers.Relationship.EntityType = typeof(Relationship).Name;
                AddRDFRecordRelationshipCategoryElement(ref vers, M11.M73);
                AddRDFRecordRelationshipIdentifierElement(ref vers, M11.M73);
                AddRDFRecordRelationshipNameElement(ref vers, M11.M73);
                AddRDFRecordRelationshipDateRangeElement(ref vers, M11.M73);
                AddRDFRecordRelationshipDescriptionElement(ref vers, M11.M73);
                AddRDFRecordRelationshipRelatedEntityElement(ref vers, M11.M73);
                AddRDFRecordRelationshipChangeHistoryElement(ref vers, M11.M73);
            }
        }

        public void AddRDFRecordRelationshipCategoryElement(ref Record vers, EXORecordVEO_M73_RDFRecord_Relationship M73)
        {
            if (M73.M13 != null)
            {
                vers.Relationship.Category = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M73.M13.ExchangeMetadataAsSource, M73.M13.DefaultValue, M73.M13.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
            }
        }

        public void AddRDFRecordRelationshipIdentifierElement(ref Record vers, EXORecordVEO_M73_RDFRecord_Relationship M73)
        {
            if (M73.M14 != null)
            {
                foreach (var M14 in M73.M14)
                {
                    var recordIdentifier = new RecordIdentifier();
                    if (M14.M15 != null)
                    {
                        recordIdentifier.IdentifierString = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M14.M15.ExchangeMetadataAsSource, M14.M15.DefaultValue, M14.M15.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                    }
                    if (M14.M16 != null)
                    {
                        recordIdentifier.IdentifierScheme = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M14.M16.ExchangeMetadataAsSource, M14.M16.DefaultValue, M14.M16.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                    }
                    recordIdentifier = VEOV3CommonMethod.FilteredValidValue(recordIdentifier);
                    if (recordIdentifier != null) (vers.Relationship.Identifier ??= []).Add(recordIdentifier);
                }
            }
        }

        public void AddRDFRecordRelationshipNameElement(ref Record vers, EXORecordVEO_M73_RDFRecord_Relationship M73)
        {
            foreach (var M17 in M73.M17)
            {
                if (M17 != null)
                {
                    var recordName = new RecordName();
                    if (M17.M18 != null)
                    {
                        recordName.NameWords = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M17.M18.ExchangeMetadataAsSource, M17.M18.DefaultValue, M17.M18.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                    }
                    if (M17.M19 != null)
                    {
                        recordName.NameScheme = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M17.M19.ExchangeMetadataAsSource, M17.M19.DefaultValue, M17.M19.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                    }
                    recordName = VEOV3CommonMethod.FilteredValidValue(recordName);
                    if (recordName != null) (vers.Relationship.Name ??= []).Add(recordName);
                }
            }
        }

        public void AddRDFRecordRelationshipDateRangeElement(ref Record vers, EXORecordVEO_M73_RDFRecord_Relationship M73)
        {
            if (M73.M20 != null)
            {
                var mDateRange = new RecordDateRange();
                if (M73.M20.M21 != null)
                {
                    mDateRange.StartDate = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M73.M20.M21.ExchangeMetadataAsSource, M73.M20.M21.DefaultValue, M73.M20.M21.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                if (M73.M20.M22 != null)
                {
                    mDateRange.EndDate = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M73.M20.M22.ExchangeMetadataAsSource, M73.M20.M22.DefaultValue, M73.M20.M22.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                vers.Relationship.DateRange = VEOV3CommonMethod.FilteredValidValue(mDateRange);
            }
        }

        public void AddRDFRecordRelationshipDescriptionElement(ref Record vers, EXORecordVEO_M73_RDFRecord_Relationship M73)
        {
            vers.Relationship.Description = new List<string>();
            foreach (var M23 in M73.M23)
            {
                vers.Relationship.Description.Add(EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M23.ExchangeMetadataAsSource, M23.DefaultValue, M23.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass));
            }
            vers.Relationship.Description = VEOV3CommonMethod.FilteredValidValue(vers.Relationship.Description);
        }

        public void AddRDFRecordRelationshipRelatedEntityElement(ref Record vers, EXORecordVEO_M73_RDFRecord_Relationship M73)
        {
            if (M73.M74 != null)
            {
                foreach (var M74 in M73.M74)
                {
                    var mRecordRelationshipRelatedEntity = new RelationshipRelatedEntity();
                    if (M74.M75 != null)
                    {
                        mRecordRelationshipRelatedEntity.AssignedEntityID = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M74.M75.ExchangeMetadataAsSource, M74.M75.DefaultValue, M74.M75.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                    }
                    if (M74.M76 != null)
                    {
                        mRecordRelationshipRelatedEntity.AssignedEntityIDScheme = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M74.M76.ExchangeMetadataAsSource, M74.M76.DefaultValue, M74.M76.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                    }
                    if (M74.M77 != null)
                    {
                        mRecordRelationshipRelatedEntity.RelationshipRole = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M74.M77.ExchangeMetadataAsSource, M74.M77.DefaultValue, M74.M77.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                    }
                    if (M74.M78 != null)
                    {
                        AddRDFRecordRelationshipRelatedEntityAgentElement(ref mRecordRelationshipRelatedEntity, M74.M78);
                    }
                    mRecordRelationshipRelatedEntity = VEOV3CommonMethod.FilteredValidValue(mRecordRelationshipRelatedEntity);
                    if (mRecordRelationshipRelatedEntity != null) (vers.Relationship.RelatedEntity ??= []).Add(mRecordRelationshipRelatedEntity);
                }
            }
        }

        public void AddRDFRecordRelationshipChangeHistoryElement(ref Record vers, EXORecordVEO_M73_RDFRecord_Relationship M73)
        {
            if (M73.M86 != null)
            {
                foreach (var M86 in M73.M86)
                {
                    var mChangeHistory = new RelationshipChangeHistory();
                    if (M86.M87 != null)
                    {
                        mChangeHistory.PropertyName = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M86.M87.ExchangeMetadataAsSource, M86.M87.DefaultValue, M86.M87.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                    }
                    if (M86.M88 != null)
                    {
                        mChangeHistory.PriorValue = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M86.M88.ExchangeMetadataAsSource, M86.M88.DefaultValue, M86.M88.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                    }
                    if (M86.M89 != null)
                    {
                        mChangeHistory.RelationshipID = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M86.M89.ExchangeMetadataAsSource, M86.M89.DefaultValue, M86.M89.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                    }
                    mChangeHistory = VEOV3CommonMethod.FilteredValidValue(mChangeHistory);
                    if (mChangeHistory != null) (vers.Relationship.ChangeHistory ??= []).Add(mChangeHistory);
                }
            }
        }

        #region Relationship_RelatedEntity_Agent

        public void AddRDFRecordRelationshipRelatedEntityAgentElement(ref RelationshipRelatedEntity vers, EXORecordVEO_M78_RDFAgent M78)
        {
            var agent = new Agent();

            agent.EntityType = typeof(Agent).Name;

            AddRDFRecordRelationshipRelatedEntityAgentCategoryElement(ref agent, M78);

            AddRDFRecordRelationshipRelatedEntityAgentIdentifierElement(ref agent, M78);

            AddRDFRecordRelationshipRelatedEntityAgentNameElement(ref agent, M78);

            AddRDFRecordRelationshipRelatedEntityAgentDateRangeElement(ref agent, M78);

            AddRDFRecordRelationshipRelatedEntityAgentDescriptionElement(ref agent, M78);

            AddRDFRecordRelationshipRelatedEntityAgentJurisdictionElement(ref agent, M78);

            AddRDFRecordRelationshipRelatedEntityAgentPermissionsElement(ref agent, M78);

            AddRDFRecordRelationshipRelatedEntityAgentContactElement(ref agent, M78);

            AddRDFRecordRelationshipRelatedEntityAgentPositionElement(ref agent, M78);

            AddRDFRecordRelationshipRelatedEntityAgentLanguageElement(ref agent, M78);

            vers.Template = agent;
        }

        public void AddRDFRecordRelationshipRelatedEntityAgentCategoryElement(ref Agent vers, EXORecordVEO_M78_RDFAgent M78)
        {
            if (M78.M13 != null)
            {
                vers.Category = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M78.M13.ExchangeMetadataAsSource, M78.M13.DefaultValue, M78.M13.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
            }
        }

        public void AddRDFRecordRelationshipRelatedEntityAgentIdentifierElement(ref Agent vers, EXORecordVEO_M78_RDFAgent M78)
        {
            foreach (var M14 in M78.M14)
            {
                if (M14 != null)
                {
                    var identifier = new RecordIdentifier();
                    if (M14.M15 != null)
                    {
                        identifier.IdentifierString = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M14.M15.ExchangeMetadataAsSource, M14.M15.DefaultValue, M14.M15.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                    }
                    if (M14.M16 != null)
                    {
                        identifier.IdentifierScheme = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M14.M16.ExchangeMetadataAsSource, M14.M16.DefaultValue, M14.M16.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                    }
                    identifier = VEOV3CommonMethod.FilteredValidValue(identifier);
                    if (identifier != null) (vers.Identifier ??= []).Add(identifier);
                }
            }
        }

        public void AddRDFRecordRelationshipRelatedEntityAgentNameElement(ref Agent vers, EXORecordVEO_M78_RDFAgent M78)
        {
            foreach (var M17 in M78.M17)
            {
                if (M17 != null)
                {
                    var recordName = new RecordName();
                    if (M17.M18 != null)
                    {
                        recordName.NameWords = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M17.M18.ExchangeMetadataAsSource, M17.M18.DefaultValue, M17.M18.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                    }
                    if (M17.M19 != null)
                    {
                        recordName.NameScheme = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M17.M19.ExchangeMetadataAsSource, M17.M19.DefaultValue, M17.M19.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                    }
                    recordName = VEOV3CommonMethod.FilteredValidValue(recordName);
                    if (recordName != null) (vers.Name ??= []).Add(recordName);
                }
            }
        }

        public void AddRDFRecordRelationshipRelatedEntityAgentDateRangeElement(ref Agent vers, EXORecordVEO_M78_RDFAgent M78)
        {
            if (M78.M20 != null)
            {
                var mDateRange = new RecordDateRange();
                if (M78.M20.M21 != null)
                {
                    mDateRange.StartDate = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M78.M20.M21.ExchangeMetadataAsSource, M78.M20.M21.DefaultValue, M78.M20.M21.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                if (M78.M20.M22 != null)
                {
                    mDateRange.EndDate = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M78.M20.M22.ExchangeMetadataAsSource, M78.M20.M22.DefaultValue, M78.M20.M22.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                vers.DateRange = VEOV3CommonMethod.FilteredValidValue(mDateRange);
            }
        }

        public void AddRDFRecordRelationshipRelatedEntityAgentDescriptionElement(ref Agent vers, EXORecordVEO_M78_RDFAgent M78)
        {
            vers.Description = new List<string>();
            foreach (var M23 in M78.M23)
            {
                vers.Description.Add(EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M23.ExchangeMetadataAsSource, M23.DefaultValue, M23.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass));
            }
            vers.Description = VEOV3CommonMethod.FilteredValidValue(vers.Description);
        }

        public void AddRDFRecordRelationshipRelatedEntityAgentJurisdictionElement(ref Agent vers, EXORecordVEO_M78_RDFAgent M78)
        {
            vers.Jurisdiction = new List<string>();
            foreach (var M24 in M78.M24)
            {
                vers.Jurisdiction.Add(EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M24.ExchangeMetadataAsSource, M24.DefaultValue, M24.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass));
            }
            vers.Jurisdiction = VEOV3CommonMethod.FilteredValidValue(vers.Jurisdiction);
        }

        public void AddRDFRecordRelationshipRelatedEntityAgentPermissionsElement(ref Agent vers, EXORecordVEO_M78_RDFAgent M78)
        {
            vers.Permissions = new List<AgentPermissions>();
            foreach (var M80 in M78.M80)
            {
                var mPermission = new AgentPermissions();
                if (M80.M81 != null)
                {
                    mPermission.PermissionText = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M80.M81.ExchangeMetadataAsSource, M80.M81.DefaultValue, M80.M81.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }

                if (M80.M81 != null)
                {
                    mPermission.PermissionType = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M80.M82.ExchangeMetadataAsSource, M80.M82.DefaultValue, M80.M82.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                mPermission = VEOV3CommonMethod.FilteredValidValue(mPermission);
                if (mPermission != null) (vers.Permissions ??= []).Add(mPermission);
            }
        }

        public void AddRDFRecordRelationshipRelatedEntityAgentContactElement(ref Agent vers, EXORecordVEO_M78_RDFAgent M78)
        {
            foreach (var M83 in M78.M83)
            {
                var mContact = new AgentContact();
                if (M83.M84 != null)
                {
                    mContact.ContactDetails = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M83.M84.ExchangeMetadataAsSource, M83.M84.DefaultValue, M83.M84.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                if (M83.M85 != null)
                {
                    mContact.ContactType = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M83.M85.ExchangeMetadataAsSource, M83.M85.DefaultValue, M83.M85.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
                }
                mContact = VEOV3CommonMethod.FilteredValidValue(mContact);
                if (mContact != null) (vers.Contact ??= []).Add(mContact);
            }
        }

        public void AddRDFRecordRelationshipRelatedEntityAgentPositionElement(ref Agent vers, EXORecordVEO_M78_RDFAgent M78)
        {
            if (M78.M79 != null)
            {
                vers.Position = EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M78.M79.ExchangeMetadataAsSource, M78.M79.DefaultValue, M78.M79.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass);
            }
        }

        public void AddRDFRecordRelationshipRelatedEntityAgentLanguageElement(ref Agent vers, EXORecordVEO_M78_RDFAgent M78)
        {
            vers.Language = new List<string>();
            foreach (var M33 in M78.M33)
            {
                vers.Language.Add(EXOVEOV3CustomizeProperty.GetEXOItemPropertyValue(M33.ExchangeMetadataAsSource, M33.DefaultValue, M33.ExchangeMetadata, EXOItem, jobID, contentExportPath, mailFullPath, disposalClass));
            }
            vers.Language = VEOV3CommonMethod.FilteredValidValue(vers.Language);
        }

        #endregion

        #endregion

        #region VEO HISTORY

        public EXORecordVEODataV3(EXOVEOHistoryXML xml)
        {
            EXOVEOHistoryXML = xml;
        }

        public void BuildVEOHistoryData(ref VEOHistory vers)
        {
            foreach (var M3 in EXOVEOHistoryXML.M1.M3)
            {
                Event mEvent = new Event();

                if (M3.M4 != null)
                {
                    if (M3.M4.DefaultValue.Equals("@TimeNow@", StringComparison.OrdinalIgnoreCase))
                    {
                        mEvent.EventDateTime = DateTime.UtcNow.ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                    }
                    else
                    {
                        mEvent.EventDateTime = M3.M4.DefaultValue;
                        if (string.IsNullOrEmpty(mEvent.EventDateTime)) mEvent.EventDateTime = null;
                    }
                }
                if (M3.M5 != null)
                {
                    mEvent.EventType = M3.M5.DefaultValue;
                    if (string.IsNullOrEmpty(mEvent.EventType)) mEvent.EventType = null;
                }
                if (M3.M6 != null)
                {
                    mEvent.Initiator = M3.M6.DefaultValue;
                    if (string.IsNullOrEmpty(mEvent.Initiator)) mEvent.Initiator = null;
                }
                if (M3.M23 != null)
                {
                    mEvent.Description = new List<string>();
                    foreach (var M23 in M3.M23)
                    {
                        mEvent.Description.Add(M23.DefaultValue);
                    }
                    mEvent.Description = VEOV3CommonMethod.FilteredValidValue(mEvent.Description);
                }
                if (M3.M7 != null)
                {
                    mEvent.Error = new List<string>();
                    foreach (var M7 in M3.M7)
                    {
                        mEvent.Error.Add(M7.DefaultValue);
                    }
                    mEvent.Error = VEOV3CommonMethod.FilteredValidValue(mEvent.Error);
                }
                mEvent = VEOV3CommonMethod.FilteredValidValue(mEvent);
                if (mEvent != null) (vers.Event ??= []).Add(mEvent);
            }
        }

        #endregion
    }

    internal class EXOVEOV3CustomizeProperty
    {
        public string? GetEXOItemPropertyValue(bool ExchangeMetadataAsSource, string defaultValue, string columnName, Item EXOItem, string jobID, string exportPath, string filePath, string disposalClass)
        {
            var result = EXOCustomizeProperty.GetEXOItemPropertyValue(ExchangeMetadataAsSource, defaultValue, columnName?.Replace("@", ""), EXOItem, jobID, exportPath, filePath, disposalClass);
            return string.IsNullOrWhiteSpace(result) ? null : result;
        }
    }
}

