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
using AvePoint.Wrapper.Backup;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Exchange.WebServices.Data;
using RAExportCommon.VEOExportV2;

namespace RAExportCommon
{
    internal class EXORecordVEOData
    {
        
        private const string ALGORITHMID_SHA1WITHRSA = "1.2.840.113549.1.1.5";


        private string DateTimeString = string.Empty;
        private EXORecordVEOParameters EXORecordVEOParameters = null;
        private EXORecordVEOXML EXORecordVEOXML = null;
        private Item EXOItem = null;
        private string jobID = string.Empty;
        private string exportPath = string.Empty;
        private string filePath = string.Empty;
        private string disposalClass = string.Empty;

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        internal EXORecordVEOData()
        {
            //YYYY[‘-‘MM[‘-‘DD[Thh’:’mm[‘:ss]Z[xx’:’yy]]]]
            DateTimeString = DateTime.Now.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
        }

        internal RecordVEOClass.VERSEncapsulatedObject GeneratorVEOData(EXORecordVEOXML EXORecordVEOXML, EXORecordVEOParameters para, Item EXOItem, string jobID, string exportPath, string filePath, string disposalClass)
        {
            EXORecordVEOParameters = para;
            this.EXORecordVEOXML = EXORecordVEOXML;
            this.EXOItem = EXOItem;
            this.jobID = jobID;
            this.exportPath = exportPath;
            this.filePath = filePath;
            this.disposalClass = disposalClass;
            RecordVEOClass.VERSEncapsulatedObject vers = new RecordVEOClass.VERSEncapsulatedObject();
            AddVEOFormatDescriptionElement(ref vers);
            AddVersionElement(ref vers);

            AddSignedObjectElement(ref vers);

            AddSignatureBlockElement(ref vers);
            AddLockSignatureBlockElement(ref vers);

            return vers;
        }

        //internal VEOClass.VERSEncapsulatedObject GeneratorVEOData()

        private void AddVEOFormatDescriptionElement(ref RecordVEOClass.VERSEncapsulatedObject vers)
        {
            EXORecordVEO_M2_VEOFormatDescription M2 = EXORecordVEOXML.M1.M2;
            if (M2 != null)
            {
                var des = new RecordVEOClass.VERSEncapsulatedObjectVEOFormatDescription() { Text = EXOCustomizeProperty.GetEXOItemPropertyValue(M2.ExchangeMetadataAsSource, M2.DefaultValue, M2.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass) };
                vers.VEOFormatDescription = des;
            }
        }

        private void AddVersionElement(ref RecordVEOClass.VERSEncapsulatedObject vers)
        {
            vers.Version = "2.0";
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison")]
        private void AddSignatureBlockElement(ref RecordVEOClass.VERSEncapsulatedObject vers)
        {
            byte[] content1;
            #region SignatureBlocks
            var mSignatureBlocks = new RecordVEOClass.VERSEncapsulatedObjectSignatureBlock[1];

            var mSignatureBlock = new RecordVEOClass.VERSEncapsulatedObjectSignatureBlock();
            mSignatureBlock.id = "Revision-1-Signature-1";
            mSignatureBlock.SignatureFormatDescription = "The contents of this VEO is signed using SHA-512 hash algorithm and RSA digital signature algorithm. SHA-512 is defined in Secure Hash Standard, FIPS PUB 180-1, National Institute of Standards and Technology, US Department of Commerce, 17 April 1995, (http://csrc.nist.gov/publications/fips/fips180-1/fip180-1.pdf). The RSA algorithm (RSASSA-PKCS-v1_5) is defined in PKCS #1 v2.1: RSA Cryptography Standard, RSA Laboratories, 14 June 2002, (ftp://ftp.rsasecurity.com/pub/pkcs/pkcs-1/pkcs-1v2-1.pdf). Details of the public keys are encoded as X.509 certificates in the vers:CertificateBlock elements. X.509 certificates are define in \"Information technology - Open Systems Interconnection - The Directory: Public-key and attribute certificate frameworks\", ITU-T Recommendation X.509 (2000) The signature and certificates are encoded using Base64. Base64 is defined in Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies, Section 6.8, Base64 Content-Transfer- Encoding, IETF RFC 2045, N. Freed & N. Borenstein, November 1996, (http://www.ietf.org/rfc/rfc2045.txt?number=2045) The signature covers the contents of the vers:SignedObject element starting with the 'less than' symbol of the vers:SignedObject start tag up to and including the 'greater than' symbol of the vers:SignedObject end tag. Before verifying the signature all whitespace (Unicode characters U+0009, U+000A, U+000D, and U+0020) must be removed from the text";

            mSignatureBlock.SignatureAlgorithm = new RecordVEOClass.SignatureAlgorithm[1];
            RecordVEOClass.SignatureAlgorithm mSignatureAlgorithm = new RecordVEOClass.SignatureAlgorithm() { SignatureAlgorithmIdentifier = ALGORITHMID_SHA1WITHRSA };
            mSignatureBlock.SignatureAlgorithm[0] = mSignatureAlgorithm;

            mSignatureBlock.SignatureDate = DateTimeString;

            mSignatureBlock.Signer = VEOCommonString.SIGNER;

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("vers", "http://www.prov.vic.gov.au/gservice/standard/pros99007.htm");
            ns.Add("naa", "http://www.naa.gov.au/recordkeeping/control/rkms/contents.html");

            XmlSerializer xs = new XmlSerializer(typeof(RecordVEOClass.VERSEncapsulatedObject));
            using (Stream memStream = new MemoryStream())
            {
                xs.Serialize(memStream, vers, ns);
                memStream.Position = 0;

                XmlDocument doc = new XmlDocument();

                doc.Load(memStream);

                //substring
                int a = doc.OuterXml.IndexOf("<vers:SignedObject", 0);
                int b = doc.OuterXml.IndexOf("vers:SignedObject>", a);
                string temp3 = doc.OuterXml.Substring(a, b - a + 18);
                content1 = Encoding.UTF8.GetBytes(temp3);
                content1 = RemoveInvalidCharacter(content1);
                //byte[] content3 = RemoveInvalidCharacter(content2);

            }

            byte[] result = SHA512WithRSASignature.Signature(content1);
            mSignatureBlock.Signature = AddValueNewLine(Convert.ToBase64String(result));

            mSignatureBlock.CertificateBlock = new RecordVEOClass.CertificateBlockCertificate[1];
            var mCertificateBlock = new RecordVEOClass.CertificateBlockCertificate();
            mCertificateBlock.Value = AddValueNewLine(Convert.ToBase64String(AveCertificateOperation.ExportCertificateWithCertFormat()));
            mSignatureBlock.CertificateBlock[0] = mCertificateBlock;

            mSignatureBlocks[0] = mSignatureBlock;
            vers.SignatureBlock = mSignatureBlocks;
            #endregion
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private void AddLockSignatureBlockElement(ref RecordVEOClass.VERSEncapsulatedObject vers)
        {
            #region LockSignatureBlock
            var mLockSignatureBlock = new RecordVEOClass.VERSEncapsulatedObjectLockSignatureBlock();

            mLockSignatureBlock.SignatureFormatDescription = "The contents of this VEO is signed using SHA-512 hash algorithm and RSA digital signature algorithm. SHA-512 is defined in Secure Hash Standard, FIPS PUB 180-1, National Institute of Standards and Technology, US Department of Commerce, 17 April 1995, (http://csrc.nist.gov/publications/fips/fips180-1/fip180-1.pdf). The RSA algorithm (RSASSA-PKCS-v1_5) is defined in PKCS #1 v2.1: RSA Cryptography Standard, RSA Laboratories, 14 June 2002, (ftp://ftp.rsasecurity.com/pub/pkcs/pkcs-1/pkcs-1v2-1.pdf). Details of the public keys are encoded as X.509 certificates in the vers:CertificateBlock elements. X.509 certificates are define in \"Information technology - Open Systems Interconnection - The Directory: Public-key and attribute certificate frameworks\", ITU-T Recommendation X.509 (2000) The signature and certificates are encoded using Base64. Base64 is defined in Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies, Section 6.8, Base64 Content-Transfer- Encoding, IETF RFC 2045, N. Freed & N. Borenstein, November 1996, (http://www.ietf.org/rfc/rfc2045.txt?number=2045) The signature covers the contents of the vers:Signature element starting with the first base 64 encoded character and ending with the last character. Before verifying the signature all whitespace (Unicode characters U+0009, U+000A, U+000D, and U+0020) must be removed from the text";

            mLockSignatureBlock.signsSignatureBlock = "Revision-1-Signature-1";

            mLockSignatureBlock.SignatureAlgorithm = new RecordVEOClass.SignatureAlgorithm[1];
            var mLockSignatureBlockSignatureAlgorithm = new RecordVEOClass.SignatureAlgorithm();
            mLockSignatureBlockSignatureAlgorithm.SignatureAlgorithmIdentifier = ALGORITHMID_SHA1WITHRSA;
            mLockSignatureBlock.SignatureAlgorithm[0] = mLockSignatureBlockSignatureAlgorithm;

            mLockSignatureBlock.SignatureDate = DateTimeString;

            mLockSignatureBlock.Signer = "AvePoint";

            string signBlockStr = vers.SignatureBlock[0].Signature;
            byte[] reSignBlockByte = Encoding.UTF8.GetBytes(signBlockStr);

            byte[] result = SHA512WithRSASignature.Signature(RemoveInvalidCharacter(reSignBlockByte));
            mLockSignatureBlock.Signature = AddValueNewLine(Convert.ToBase64String(result));

            mLockSignatureBlock.CertificateBlock = new RecordVEOClass.CertificateBlockCertificate[1];
            var mCertificateBlock = new RecordVEOClass.CertificateBlockCertificate();
            mCertificateBlock.Value = AddValueNewLine(Convert.ToBase64String(AveCertificateOperation.ExportCertificateWithCertFormat()));
            mLockSignatureBlock.CertificateBlock[0] = mCertificateBlock;

            vers.LockSignatureBlock = mLockSignatureBlock;
            #endregion
        }

        private void AddSignedObjectElement(ref RecordVEOClass.VERSEncapsulatedObject vers)
        {
            var mSignedObject = new RecordVEOClass.VERSEncapsulatedObjectSignedObject();
            mSignedObject.VEOVersion = "2.0";

            #region SignedObject ObjectMetadata
            AddSignedObjectObjectMetadataElement(ref mSignedObject);
            #endregion

            #region SignedObject ObjectContent
            AddSignedObjectObjectContentElement(ref mSignedObject);
            #endregion
            vers.SignedObject = mSignedObject;
        }

        private void AddSignedObjectObjectContentElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObject signedObject)
        {
            signedObject.ObjectContent = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecord[1];
            var mObjectContent = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecord();

            //RecordMetadata
            AddSignedObjectObjectContentRecordMetadataElement(ref mObjectContent);

            //Document
            AddSignedObjectObjectContentRecordDocumentElement(ref mObjectContent);

            signedObject.ObjectContent[0] = mObjectContent;
        }

        private void AddSignedObjectObjectMetadataElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObject signedObject)
        {
            EXORecordVEO_M5_ObjectMetadata M5 = EXORecordVEOXML.M1.M4.M5;
            var mObjectMetadata = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectMetadata();
            mObjectMetadata.ObjectType = "Record";
            if (M5 != null)
            {
                mObjectMetadata.ObjectTypeDescription = EXOCustomizeProperty.GetEXOItemPropertyValue(M5.M7.ExchangeMetadataAsSource, M5.M7.DefaultValue, M5.M7.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
            }
            mObjectMetadata.ObjectCreationDate = DateTimeString;
            signedObject.ObjectMetadata = mObjectMetadata;
        }

        private void AddSignedObjectObjectContentRecordMetadataElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecord record)
        {
            var mRecordMetadata = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata();

            AddSignedObjectObjectContentRecordMetadataAgentElement(ref mRecordMetadata);

            AddSignedObjectObjectContentRecordMetadataRightsManagementElement(ref mRecordMetadata);

            AddSignedObjectObjectContentRecordMetadataTitleElement(ref mRecordMetadata);

            AddSignedObjectContentSubjectElement(ref mRecordMetadata);

            AddSignedObjectObjectContentRecordMetadataDescriptionElement(ref mRecordMetadata);

            AddSignedObjectObjectContentRecordMetadataAuxiliaryDescriptionElement(ref mRecordMetadata);

            AddSignedObjectObjectContentRecordMetadataLanguageElement(ref mRecordMetadata);

            AddSignedObjectObjectContentRecordMetadataRelationElement(ref mRecordMetadata);

            AddSignedObjectObjectContentRecordMetadataCoverageElement(ref mRecordMetadata);

            AddSignedObjectObjectContentRecordMetadataFunctionElement(ref mRecordMetadata);

            AddSignedObjectObjectContentRecordMetadataDateElement(ref mRecordMetadata);

            AddSignedObjectObjectContentRecordMetadataTypeElement(ref mRecordMetadata);

            AddSignedObjectObjectContentRecordMetadataFormatElement(ref mRecordMetadata);

            AddSignedObjectObjectContentRecordMetadataRecordIdentifierElement(ref mRecordMetadata);

            AddSignedObjectObjectContentRecordMetadataManagementHistoryElement(ref mRecordMetadata);

            AddSignedObjectObjectContentRecordMetadataUseHistoryElement(ref mRecordMetadata);

            AddSignedObjectObjectContentRecordMetadataPreservationHistoryElement(ref mRecordMetadata);

            AddSignedObjectObjectContentRecordMetadataLocationElement(ref mRecordMetadata);

            AddSignedObjectObjectContentRecordMetadataDisposalElement(ref mRecordMetadata);

            AddSignedObjectObjectContentRecordMetadataMandateElement(ref mRecordMetadata);

            AddSignedObjectObjectContentRecordMetadataVEOIdentifierElement(ref mRecordMetadata);

            AddSignedObjectObjectContentRecordMetadataTransactionElement(ref mRecordMetadata);

            record.RecordMetadata = mRecordMetadata;

            mRecordMetadata.AggregationLevel = "Item";


        }

        private void AddSignedObjectContentSubjectElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            List<EXORecordVEO_M37_Subject> M37 = EXORecordVEOXML.M1.M4.M9.M10.M11.M37;
            if (M37 != null)
            {
                var subject = new List<RecordVEOClass.Subject>();
                foreach (var temp in M37)
                {
                    var mSubject = new RecordVEOClass.Subject();
                    if (temp.M38 != null)
                    {
                        mSubject.KeywordLevel = EXOCustomizeProperty.GetEXOItemPropertyValue(temp.M38.ExchangeMetadataAsSource, temp.M38.DefaultValue, temp.M38.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                    }
                    if (temp.M39.Count != 0)
                    {
                        List<string> keyWord = new List<string>();
                        foreach (var M39 in temp.M39)
                        {
                            keyWord.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M39.ExchangeMetadataAsSource, M39.DefaultValue, M39.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        mSubject.Keyword = keyWord;
                    }
                    subject.Add(mSubject);
                }
                recordMetadata.Subject = subject;
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataAgentElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            EXORecordVEO_M11_RecordMetadata M11 = EXORecordVEOXML.M1.M4.M9.M10.M11;
            if (M11 != null)
            {
                List<RecordVEOClass.Agent> mAgent = new List<RecordVEOClass.Agent>();
                foreach (var M12 in EXORecordVEOXML.M1.M4.M9.M10.M11.M12)
                {
                    RecordVEOClass.Agent agent = new RecordVEOClass.Agent();
                    if (M12.M13.Count != 0)
                    {
                        agent.AgentType = new List<string>();
                        foreach (var M13 in M12.M13)
                        {
                            agent.AgentType.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M13.ExchangeMetadataAsSource, M13.DefaultValue, M13.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                    }
                    if (M12.M14.Count != 0)
                    {
                        agent.Jurisdiction = new List<string>();
                        foreach (var M14 in M12.M14)
                        {
                            agent.Jurisdiction.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M14.ExchangeMetadataAsSource, M14.DefaultValue, M14.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                    }
                    if (M12.M15 != null)
                    {
                        agent.CorporateId = EXOCustomizeProperty.GetEXOItemPropertyValue(M12.M15.ExchangeMetadataAsSource, M12.M15.DefaultValue, M12.M15.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                    }
                    if (M12.M16.Count != 0)
                    {
                        agent.CorporateName = new List<string>();
                        foreach (var M16 in M12.M16)
                        {
                            agent.CorporateName.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M16.ExchangeMetadataAsSource, M16.DefaultValue, M16.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                    }
                    if (M12.M17 != null)
                    {
                        agent.PersonId = EXOCustomizeProperty.GetEXOItemPropertyValue(M12.M17.ExchangeMetadataAsSource, M12.M17.DefaultValue, M12.M17.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                    }
                    if (M12.M18.Count != 0)
                    {
                        agent.PersonalName = new List<string>();
                        foreach (var M18 in M12.M18)
                        {
                            agent.PersonalName.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M18.ExchangeMetadataAsSource, M18.DefaultValue, M18.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                    }
                    if (M12.M19.Count != 0)
                    {
                        agent.SectionName = new List<string>();
                        foreach (var M19 in M12.M19)
                        {
                            agent.SectionName.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M19.ExchangeMetadataAsSource, M19.DefaultValue, M19.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                    }
                    if (M12.M20.Count != 0)
                    {
                        agent.PositionName = new List<string>();
                        foreach (var M20 in M12.M20)
                        {
                            agent.PositionName.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M20.ExchangeMetadataAsSource, M20.DefaultValue, M20.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                    }
                    if (M12.M21.Count != 0)
                    {
                        agent.ContactDetails = new List<string>();
                        foreach (var M21 in M12.M21)
                        {
                            agent.ContactDetails.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M21.ExchangeMetadataAsSource, M21.DefaultValue, M21.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                    }
                    if (M12.M22.Count != 0)
                    {
                        agent.Email = new List<string>();
                        foreach (var M22 in M12.M22)
                        {
                            agent.Email.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M22.ExchangeMetadataAsSource, M22.DefaultValue, M22.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                    }
                    if (M12.M23.Count != 0)
                    {
                        agent.DigitalSignature = new List<string>();
                        foreach (var M23 in M12.M23)
                        {
                            agent.DigitalSignature.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M23.ExchangeMetadataAsSource, M23.DefaultValue, M23.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                    }
                    mAgent.Add(agent);
                }
                recordMetadata.Agent = mAgent;
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataRelationElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            List<EXORecordVEO_M42_Relation> M42 = EXORecordVEOXML.M1.M4.M9.M10.M11.M42;
            if (M42 != null)
            {
                var mRelation = new List<RecordVEOClass.Relation>();
                foreach (var item in M42)
                {
                    var relation = new RecordVEOClass.Relation();
                    if (item.M43.Count != 0)
                    {
                        List<string> mRelatedItemId = new List<string>();
                        foreach (var M43 in item.M43)
                        {
                            mRelatedItemId.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M43.ExchangeMetadataAsSource, M43.DefaultValue, M43.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        relation.RelatedItemId = mRelatedItemId;
                    }
                    if (item.M44.Count != 0)
                    {
                        List<string> mRelationType = new List<string>();
                        foreach (var M44 in item.M44)
                        {
                            mRelationType.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M44.ExchangeMetadataAsSource, M44.DefaultValue, M44.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        relation.RelationType = mRelationType;
                    }
                    if (item.M45.Count != 0)
                    {
                        List<string> mRelationDescrition = new List<string>();
                        foreach (var M45 in item.M45)
                        {
                            mRelationDescrition.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M45.ExchangeMetadataAsSource, M45.DefaultValue, M45.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        relation.RelationDescription = mRelationDescrition;
                    }
                    mRelation.Add(relation);
                }
                recordMetadata.Relation = mRelation;
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataCoverageElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            List<EXORecordVEO_M46_Coverage> M46 = EXORecordVEOXML.M1.M4.M9.M10.M11.M46;
            if (M46 != null)
            {
                var mCoverage = new List<RecordVEOClass.Coverage>();
                foreach (var item in M46)
                {
                    var coverage = new RecordVEOClass.Coverage();
                    if (item.M47.Count != 0)
                    {
                        List<string> mJurisdiction = new List<string>();
                        foreach (var M47 in item.M47)
                        {
                            mJurisdiction.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M47.ExchangeMetadataAsSource, M47.DefaultValue, M47.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        coverage.Jurisdiction = mJurisdiction;
                    }
                    if (item.M48.Count != 0)
                    {
                        List<string> mPlaceName = new List<string>();
                        foreach (var M48 in item.M48)
                        {
                            mPlaceName.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M48.ExchangeMetadataAsSource, M48.DefaultValue, M48.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        coverage.PlaceName = mPlaceName;
                    }
                    if (item.M49.Count != 0)
                    {
                        List<string> mPeriodName = new List<string>();
                        foreach (var M49 in item.M49)
                        {
                            mPeriodName.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M49.ExchangeMetadataAsSource, M49.DefaultValue, M49.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        coverage.PeriodName = mPeriodName;
                    }
                    mCoverage.Add(coverage);
                }
                recordMetadata.Coverage = mCoverage;
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataFunctionElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            List<EXORecordVEO_M50_Function> M50 = EXORecordVEOXML.M1.M4.M9.M10.M11.M50;
            if (M50 != null)
            {
                var mFunction = new List<RecordVEOClass.Function>();
                foreach (var item in M50)
                {
                    var function = new RecordVEOClass.Function();
                    if (item.M51.Count != 0)
                    {
                        List<string> mFunctionDescriptor = new List<string>();
                        foreach (var M51 in item.M51)
                        {
                            mFunctionDescriptor.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M51.ExchangeMetadataAsSource, M51.DefaultValue, M51.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        function.FunctionDescriptor = mFunctionDescriptor;
                    }
                    if (item.M52.Count != 0)
                    {
                        List<string> mActivityDescriptor = new List<string>();
                        foreach (var M52 in item.M52)
                        {
                            mActivityDescriptor.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M52.ExchangeMetadataAsSource, M52.DefaultValue, M52.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        function.ActivityDescriptor = mActivityDescriptor;
                    }
                    if (item.M53.Count != 0)
                    {
                        List<string> mThirdLevelDescriptor = new List<string>();
                        foreach (var M53 in item.M53)
                        {
                            mThirdLevelDescriptor.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M53.ExchangeMetadataAsSource, M53.DefaultValue, M53.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        function.ThirdLevelDescriptor = mThirdLevelDescriptor;
                    }
                    mFunction.Add(function);
                }
                recordMetadata.Function = mFunction;
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataLanguageElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            List<EXORecordVEO_M41_Language> M41 = EXORecordVEOXML.M1.M4.M9.M10.M11.M41;
            if (M41 != null)
            {
                if (M41.Count != 0)
                {
                    List<string> mLanguage = new List<string>();
                    foreach (var item in M41)
                    {
                        mLanguage.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(item.ExchangeMetadataAsSource, item.DefaultValue, item.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                    }
                    recordMetadata.Language = mLanguage;
                }
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataRightsManagementElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            //M14
            EXORecordVEO_M24_RightsManagement M24 = EXORecordVEOXML.M1.M4.M9.M10.M11.M24;
            if (M24 != null)
            {
                recordMetadata.RightsManagement = new RecordVEOClass.RightsManagement();
                if (M24.M25 != null)
                {
                    recordMetadata.RightsManagement.SecurityClassification = EXOCustomizeProperty.GetEXOItemPropertyValue(M24.M25.ExchangeMetadataAsSource, M24.M25.DefaultValue, M24.M25.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }
                if (M24.M26.Count != 0)
                {
                    recordMetadata.RightsManagement.Caveat = new List<string>();
                    foreach (var M26 in M24.M26)
                    {
                        recordMetadata.RightsManagement.Caveat.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M26.ExchangeMetadataAsSource, M26.DefaultValue, M26.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                    }
                }
                if (M24.M27.Count != 0)
                {
                    recordMetadata.RightsManagement.Codeword = new List<string>();
                    foreach (var M27 in M24.M27)
                    {
                        recordMetadata.RightsManagement.Codeword.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M27.ExchangeMetadataAsSource, M27.DefaultValue, M27.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                    }
                }
                if (M24.M28.Count != 0)
                {
                    recordMetadata.RightsManagement.ReleasabilityIndicator = new List<string>();
                    foreach (var M28 in M24.M28)
                    {
                        recordMetadata.RightsManagement.ReleasabilityIndicator.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M28.ExchangeMetadataAsSource, M28.DefaultValue, M28.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                    }
                }
                if (M24.M29 != null)
                {
                    recordMetadata.RightsManagement.AccessStatus = EXOCustomizeProperty.GetEXOItemPropertyValue(M24.M29.ExchangeMetadataAsSource, M24.M29.DefaultValue, M24.M29.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }
                if (M24.M30.Count != 0)
                {
                    recordMetadata.RightsManagement.UsageCondition = new List<string>();
                    foreach (var M30 in M24.M30)
                    {
                        recordMetadata.RightsManagement.UsageCondition.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M30.ExchangeMetadataAsSource, M30.DefaultValue, M30.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                    }
                }
                if (M24.M31 != null)
                {
                    recordMetadata.RightsManagement.EncryptionDetails = EXOCustomizeProperty.GetEXOItemPropertyValue(M24.M31.ExchangeMetadataAsSource, M24.M31.DefaultValue, M24.M31.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataTitleElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            EXORecordVEO_M32_Title M32 = EXORecordVEOXML.M1.M4.M9.M10.M11.M32;
            if (M32 != null)
            {
                recordMetadata.Title = new RecordVEOClass.Title();
                if (M32.M34 != null)
                {
                    recordMetadata.Title.SchemeName = EXOCustomizeProperty.GetEXOItemPropertyValue(M32.M34.ExchangeMetadataAsSource, M32.M34.DefaultValue, M32.M34.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }
                if (M32.M33.Count != 0)
                {
                    List<string> schemeType = new List<string>();
                    foreach (var M33 in M32.M33)
                    {
                        schemeType.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M33.ExchangeMetadataAsSource, M33.DefaultValue, M33.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                    }
                    recordMetadata.Title.SchemeType = schemeType;
                }
                if (M32.M35 != null)
                {
                    recordMetadata.Title.TitleWords = EXOCustomizeProperty.GetEXOItemPropertyValue(M32.M35.ExchangeMetadataAsSource, M32.M35.DefaultValue, M32.M35.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }
                if (M32.M36.Count != 0)
                {
                    List<string> alternative = new List<string>();
                    foreach (var M36 in M32.M36)
                    {
                        alternative.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M36.ExchangeMetadataAsSource, M36.DefaultValue, M36.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                    }
                    recordMetadata.Title.Alternative = alternative;
                }
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataDescriptionElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            List<EXORecordVEO_M40_Description> M40 = EXORecordVEOXML.M1.M4.M9.M10.M11.M40;
            if (M40 != null)
            {
                if (M40.Count != 0)
                {
                    List<string> mDescription = new List<string>();
                    foreach (var item in M40)
                    {
                        mDescription.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(item.ExchangeMetadataAsSource, item.DefaultValue, item.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                    }
                    recordMetadata.Description = mDescription;
                }
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataAuxiliaryDescriptionElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            List<EXORecordVEO_M153_AuxiliaryDescription> M153 = EXORecordVEOXML.M1.M4.M9.M10.M11.M153;
            if (M153 != null)
            {
                if (M153.Count != 0)
                {
                    List<string> mAuxiliaryDescription = new List<string>();
                    foreach (var item in M153)
                    {
                        mAuxiliaryDescription.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(item.ExchangeMetadataAsSource, item.DefaultValue, item.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                    }
                    recordMetadata.AuxiliaryDescription = mAuxiliaryDescription;
                }
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataDisposalElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            EXORecordVEO_M88_Disposal M88 = EXORecordVEOXML.M1.M4.M9.M10.M11.M88;
            if (M88 != null)
            {
                recordMetadata.Disposal = new RecordVEOClass.Disposal();
                if (M88.M89.Count != 0)
                {
                    List<string> disposalAuthorisation = new List<string>();
                    foreach (var M89 in M88.M89)
                    {
                        disposalAuthorisation.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M89.ExchangeMetadataAsSource, M89.DefaultValue, M89.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                    }
                    recordMetadata.Disposal.DisposalAuthorisation = disposalAuthorisation;
                }
                if (M88.M90 != null)
                {
                    recordMetadata.Disposal.Sentence = EXOCustomizeProperty.GetEXOItemPropertyValue(M88.M90.ExchangeMetadataAsSource, M88.M90.DefaultValue, M88.M90.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }
                if (M88.M91 != null)
                {
                    recordMetadata.Disposal.DisposalActionDue = EXOCustomizeProperty.GetEXOItemPropertyValue(M88.M91.ExchangeMetadataAsSource, M88.M91.DefaultValue, M88.M91.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }
                if (M88.M92 != null)
                {
                    recordMetadata.Disposal.DisposalStatus = EXOCustomizeProperty.GetEXOItemPropertyValue(M88.M92.ExchangeMetadataAsSource, M88.M92.DefaultValue, M88.M92.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataMandateElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            List<EXORecordVEO_M93_Mandate> M93 = EXORecordVEOXML.M1.M4.M9.M10.M11.M93;
            if (M93.Count != 0)
            {
                var mMandate = new List<RecordVEOClass.Mandate>();
                foreach (var item in M93)
                {
                    var mandate = new RecordVEOClass.Mandate();
                    if (item.M94.Count != 0)
                    {
                        List<string> mMandateType = new List<string>();
                        foreach (var M94 in item.M94)
                        {
                            mMandateType.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M94.ExchangeMetadataAsSource, M94.DefaultValue, M94.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        mandate.MandateType = mMandateType;
                    }
                    if (item.M95.Count != 0)
                    {
                        List<string> mRefersto = new List<string>();
                        foreach (var M95 in item.M95)
                        {
                            mRefersto.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M95.ExchangeMetadataAsSource, M95.DefaultValue, M95.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        mandate.RefersTo = mRefersto;
                    }
                    if (item.M96.Count != 0)
                    {
                        List<string> mMandateName = new List<string>();
                        foreach (var M96 in item.M96)
                        {
                            mMandateName.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M96.ExchangeMetadataAsSource, M96.DefaultValue, M96.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        mandate.MandateName = mMandateName;
                    }
                    if (item.M97.Count != 0)
                    {
                        List<string> mMandateReference = new List<string>();
                        foreach (var M97 in item.M97)
                        {
                            mMandateReference.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M97.ExchangeMetadataAsSource, M97.DefaultValue, M97.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        mandate.MandateReference = mMandateReference;
                    }
                    if (item.M98.Count != 0)
                    {
                        List<string> mRequirement = new List<string>();
                        foreach (var M98 in item.M98)
                        {
                            mRequirement.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M98.ExchangeMetadataAsSource, M98.DefaultValue, M98.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        mandate.Requirement = mRequirement;
                    }
                    mMandate.Add(mandate);
                }
                recordMetadata.Mandate = mMandate;
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataDateElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            EXORecordVEO_M54_Date M54 = EXORecordVEOXML.M1.M4.M9.M10.M11.M54;
            if (M54 != null)
            {
                recordMetadata.Date = new RecordVEOClass.Date();
                if (M54.M55 != null)
                {
                    recordMetadata.Date.DateTimeCreated = EXOCustomizeProperty.GetEXOItemPropertyValue(M54.M55.ExchangeMetadataAsSource, M54.M55.DefaultValue, M54.M55.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }
                if (M54.M56 != null)
                {
                    recordMetadata.Date.DateTimeRegistered = EXOCustomizeProperty.GetEXOItemPropertyValue(M54.M57.ExchangeMetadataAsSource, M54.M57.DefaultValue, M54.M57.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }
                if (M54.M57 != null)
                {
                    recordMetadata.Date.DateTimeTransacted = EXOCustomizeProperty.GetEXOItemPropertyValue(M54.M56.ExchangeMetadataAsSource, M54.M56.DefaultValue, M54.M56.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataTypeElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            EXORecordVEO_M58_Type M58 = EXORecordVEOXML.M1.M4.M9.M10.M11.M58;
            if (M58 != null)
            {
                recordMetadata.Type = EXOCustomizeProperty.GetEXOItemPropertyValue(M58.ExchangeMetadataAsSource, M58.DefaultValue, M58.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataRecordIdentifierElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            EXORecordVEO_M65_RecordIdentifier M65 = EXORecordVEOXML.M1.M4.M9.M10.M11.M65;
            if (M65 != null)
            {
                recordMetadata.RecordIdentifier = EXOCustomizeProperty.GetEXOItemPropertyValue(M65.ExchangeMetadataAsSource, M65.DefaultValue, M65.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataFormatElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            EXORecordVEO_M60_Format M60 = EXORecordVEOXML.M1.M4.M9.M10.M11.M60;
            if (M60 != null)
            {
                var mFormat = new RecordVEOClass.Format();
                if (M60.M61 != null)
                {
                    mFormat.MediaFormat = EXOCustomizeProperty.GetEXOItemPropertyValue(M60.M61.ExchangeMetadataAsSource, M60.M61.DefaultValue, M60.M61.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }
                if (M60.M62 != null)
                {
                    mFormat.DataFormat = EXOCustomizeProperty.GetEXOItemPropertyValue(M60.M62.ExchangeMetadataAsSource, M60.M62.DefaultValue, M60.M62.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }
                if (M60.M63 != null)
                {
                    mFormat.Medium = EXOCustomizeProperty.GetEXOItemPropertyValue(M60.M63.ExchangeMetadataAsSource, M60.M63.DefaultValue, M60.M63.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);

                }
                if (M60.M64.Count != 0)
                {
                    mFormat.Extent = new List<string>();
                    foreach (var M64 in M60.M64)
                    {
                        mFormat.Extent.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M64.ExchangeMetadataAsSource, M64.DefaultValue, M64.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                    }
                }
                recordMetadata.Format = mFormat;
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataVEOIdentifierElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            EXORecordVEO_M99_VEOIdentifier M99 = EXORecordVEOXML.M1.M4.M9.M10.M11.M99;
            if (M99 != null)
            {
                var mVEOIdentifier = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadataVEOIdentifier();
                if (M99.M102.Count != 0)
                {
                    List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadataVEOIdentifierFileIdentifier> mFileIdentifier = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadataVEOIdentifierFileIdentifier>();
                    foreach (var M102 in M99.M102)
                    {
                        var fileIdentifier = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadataVEOIdentifierFileIdentifier();
                        fileIdentifier.Text = EXOCustomizeProperty.GetEXOItemPropertyValue(M102.ExchangeMetadataAsSource, M102.DefaultValue, M102.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);//EXORecordVEOParameters.VLibraryID;
                        mFileIdentifier.Add(fileIdentifier);
                    }
                    mVEOIdentifier.FileIdentifier = mFileIdentifier;
                }
                if (M99.M103 != null)
                {
                    var mVERSRecordIdentifier = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadataVEOIdentifierVERSRecordIdentifier();
                    mVERSRecordIdentifier.Text = EXOCustomizeProperty.GetEXOItemPropertyValue(M99.M103.ExchangeMetadataAsSource, M99.M103.DefaultValue, M99.M103.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                    mVEOIdentifier.VERSRecordIdentifier = mVERSRecordIdentifier;
                }
                if (M99.M100 != null)
                {
                    var mAgencyIdentifier = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadataVEOIdentifierAgencyIdentifier();
                    mAgencyIdentifier.Text = EXOCustomizeProperty.GetEXOItemPropertyValue(M99.M100.ExchangeMetadataAsSource, M99.M100.DefaultValue, M99.M100.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                    mVEOIdentifier.AgencyIdentifier = mAgencyIdentifier;

                }
                if (M99.M101 != null)
                {
                    var mSeriesIdentifier = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadataVEOIdentifierSeriesIdentifier();
                    mSeriesIdentifier.Text = EXOCustomizeProperty.GetEXOItemPropertyValue(M99.M101.ExchangeMetadataAsSource, M99.M101.DefaultValue, M99.M101.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                    mVEOIdentifier.SeriesIdentifier = mSeriesIdentifier;
                }

                recordMetadata.VEOIdentifier = mVEOIdentifier;
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataTransactionElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            List<EXORecordVEO_M104_Transaction> M104 = EXORecordVEOXML.M1.M4.M9.M10.M11.M104;
            if (M104.Count != 0)
            {
                var mTransaction = new List<RecordVEOClass.Transaction>();
                foreach (var item in M104)
                {
                    var transaction = new RecordVEOClass.Transaction();
                    if (item.M105 != null)
                    {
                        transaction.TransactionIdentifier = EXOCustomizeProperty.GetEXOItemPropertyValue(item.M105.ExchangeMetadataAsSource, item.M105.DefaultValue, item.M105.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                    }
                    if (item.M106 != null)
                    {
                        transaction.Orginator = EXOCustomizeProperty.GetEXOItemPropertyValue(item.M106.ExchangeMetadataAsSource, item.M106.DefaultValue, item.M106.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                    }
                    if (item.M107.Count != 0)
                    {
                        List<string> mRecipient = new List<string>();
                        foreach (var M107 in item.M107)
                        {
                            mRecipient.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M107.ExchangeMetadataAsSource, M107.DefaultValue, M107.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        transaction.Recipient = mRecipient;
                    }
                    if (item.M108.Count != 0)
                    {
                        List<string> mActionRequired = new List<string>();
                        foreach (var M108 in item.M108)
                        {
                            mActionRequired.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M108.ExchangeMetadataAsSource, M108.DefaultValue, M108.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        transaction.ActionRequired = mActionRequired;
                    }
                    if (item.M109 != null)
                    {
                        transaction.OriginatorsCopy = EXOCustomizeProperty.GetEXOItemPropertyValue(item.M109.ExchangeMetadataAsSource, item.M109.DefaultValue, item.M109.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                    }
                    if (item.M110.Count != 0)
                    {
                        List<string> mTransactionType = new List<string>();
                        foreach (var M110 in item.M110)
                        {
                            mTransactionType.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M110.ExchangeMetadataAsSource, M110.DefaultValue, M110.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        transaction.TransactionType = mTransactionType;
                    }
                    if (item.M111.Count != 0)
                    {
                        List<string> mBusinessProcedureReference = new List<string>();
                        foreach (var M111 in item.M111)
                        {
                            mBusinessProcedureReference.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M111.ExchangeMetadataAsSource, M111.DefaultValue, M111.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        transaction.BusinessProcedureReference = mBusinessProcedureReference;
                    }
                    if (item.M112.Count != 0)
                    {
                        List<string> mTransactionReference = new List<string>();
                        foreach (var M112 in item.M112)
                        {
                            mTransactionReference.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M112.ExchangeMetadataAsSource, M112.DefaultValue, M112.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        transaction.TransactionReference = mTransactionReference;
                    }
                    if (item.M113.Count != 0)
                    {
                        List<string> mTransactionLinkage = new List<string>();
                        foreach (var M113 in item.M113)
                        {
                            mTransactionLinkage.Add(EXOCustomizeProperty.GetEXOItemPropertyValue(M113.ExchangeMetadataAsSource, M113.DefaultValue, M113.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass));
                        }
                        transaction.TransactionLinkage = mTransactionLinkage;
                    }
                    mTransaction.Add(transaction);
                }
                recordMetadata.Transaction = mTransaction;
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataManagementHistoryElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            EXORecordVEO_M66_ManagementHistory M66 = EXORecordVEOXML.M1.M4.M9.M10.M11.M66;
            if (M66 != null)
            {
                recordMetadata.ManagementHistory = new RecordVEOClass.ManagementHistory();
                List<RecordVEOClass.ManagementEvent> mManagementEvent = new List<RecordVEOClass.ManagementEvent>();
                foreach (var M67 in M66.M67)
                {
                    RecordVEOClass.ManagementEvent management = new RecordVEOClass.ManagementEvent();
                    if (M67.M68 != null)
                    {
                        management.EventDateTime = EXOCustomizeProperty.GetEXOItemPropertyValue(M67.M68.ExchangeMetadataAsSource, M67.M68.DefaultValue, M67.M68.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                    }
                    if (M67.M69 != null)
                    {
                        management.EventType = EXOCustomizeProperty.GetEXOItemPropertyValue(M67.M69.ExchangeMetadataAsSource, M67.M69.DefaultValue, M67.M69.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                    }
                    if (M67.M70 != null)
                    {
                        management.EventDescription = EXOCustomizeProperty.GetEXOItemPropertyValue(M67.M70.ExchangeMetadataAsSource, M67.M70.DefaultValue, M67.M70.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                    }
                    mManagementEvent.Add(management);
                }
                recordMetadata.ManagementHistory.ManagementEvent = mManagementEvent;
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataUseHistoryElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            EXORecordVEO_M71_UseHistory M71 = EXORecordVEOXML.M1.M4.M9.M10.M11.M71;
            if (M71 != null)
            {
                var mUseHistory = new RecordVEOClass.UseHistory();
                mUseHistory.Use = new List<RecordVEOClass.Use>();
                foreach (var M72 in M71.M72)
                {
                    var use = new RecordVEOClass.Use();
                    if (M72.M73 != null)
                    {
                        use.UseDateTime = EXOCustomizeProperty.GetEXOItemPropertyValue(M72.M73.ExchangeMetadataAsSource, M72.M73.DefaultValue, M72.M73.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                    }
                    if (M72.M74 != null)
                    {
                        use.UseType = EXOCustomizeProperty.GetEXOItemPropertyValue(M72.M74.ExchangeMetadataAsSource, M72.M74.DefaultValue, M72.M74.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                    }
                    if (M72.M75 != null)
                    {
                        use.UseDescription = EXOCustomizeProperty.GetEXOItemPropertyValue(M72.M75.ExchangeMetadataAsSource, M72.M75.DefaultValue, M72.M75.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                    }
                    mUseHistory.Use.Add(use);
                }
                recordMetadata.UseHistory = mUseHistory;
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataPreservationHistoryElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            EXORecordVEO_M76_PreservationHistory M76 = EXORecordVEOXML.M1.M4.M9.M10.M11.M76;
            if (M76 != null)
            {
                var mPreservationHistory = new RecordVEOClass.PreservationHistory();
                if (M76.M77.Count != 0)
                {
                    mPreservationHistory.Action = new List<RecordVEOClass.Action>();
                    foreach (var M77 in M76.M77)
                    {
                        var action = new RecordVEOClass.Action();
                        if (M77.M78 != null)
                        {
                            action.ActionDateTime = EXOCustomizeProperty.GetEXOItemPropertyValue(M77.M78.ExchangeMetadataAsSource, M77.M78.DefaultValue, M77.M78.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                        }
                        if (M77.M79 != null)
                        {
                            action.ActionType = EXOCustomizeProperty.GetEXOItemPropertyValue(M77.M79.ExchangeMetadataAsSource, M77.M79.DefaultValue, M77.M79.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                        }
                        if (M77.M80 != null)
                        {
                            action.ActionDescription = EXOCustomizeProperty.GetEXOItemPropertyValue(M77.M80.ExchangeMetadataAsSource, M77.M80.DefaultValue, M77.M80.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                        }
                        mPreservationHistory.Action.Add(action);
                    }
                }
                if (M76.M81 != null)
                {
                    mPreservationHistory.NextAction = EXOCustomizeProperty.GetEXOItemPropertyValue(M76.M81.ExchangeMetadataAsSource, M76.M81.DefaultValue, M76.M81.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }
                if (M76.M82 != null)
                {
                    mPreservationHistory.NextActionDue = EXOCustomizeProperty.GetEXOItemPropertyValue(M76.M82.ExchangeMetadataAsSource, M76.M82.DefaultValue, M76.M82.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }
                recordMetadata.PreservationHistory = mPreservationHistory;
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataLocationElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            EXORecordVEO_M83_Location M83 = EXORecordVEOXML.M1.M4.M9.M10.M11.M83;
            if (M83 != null)
            {
                var mLocation = new RecordVEOClass.Location();
                if (M83.M84 != null)
                {
                    mLocation.CurrentLocation = EXOCustomizeProperty.GetEXOItemPropertyValue(M83.M84.ExchangeMetadataAsSource, M83.M84.DefaultValue, M83.M84.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }
                if (M83.M85 != null)
                {
                    mLocation.HomeLocationDetails = EXOCustomizeProperty.GetEXOItemPropertyValue(M83.M85.ExchangeMetadataAsSource, M83.M85.DefaultValue, M83.M85.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }
                if (M83.M86 != null)
                {
                    mLocation.HomeStorageDetails = EXOCustomizeProperty.GetEXOItemPropertyValue(M83.M86.ExchangeMetadataAsSource, M83.M86.DefaultValue, M83.M86.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }
                if (M83.M87 != null)
                {
                    mLocation.RKSID = EXOCustomizeProperty.GetEXOItemPropertyValue(M83.M87.ExchangeMetadataAsSource, M83.M87.DefaultValue, M83.M87.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }
                recordMetadata.Location = mLocation;
            }
        }

        private void AddSignedObjectObjectContentRecordDocumentElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecord record)
        {
            List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocument> mDocument = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocument>();
            foreach (var M114 in EXORecordVEOXML.M1.M4.M9.M114)
            {
                var document = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocument();
                document.id = "Revision-1-Document-1";
                AddSignedObjectObjectContentRecordDocumentDocumentMetadataElement(ref document, M114);

                AddSignedObjectObjectContentRecordDocumentDocumentEncodingElement(ref document, EXORecordVEOParameters.VItemUrl, EXORecordVEOParameters.GetFileFormatType(), EXORecordVEOParameters.VFileName, M114);
                mDocument.Add(document);
            }
            record.Document = mDocument;
        }

        private void AddSignedObjectObjectContentRecordDocumentDocumentMetadataElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocument mRecordDocument, EXORecordVEO_M114_Document M114)
        {
            var mDocumentMetadata = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata();

            //M116 need do more...
            AddDocumentMetadataAgentElement(ref mDocumentMetadata, M114);

            //M154
            AddDocumentMetadataDocumentRightsManagementElement(ref mDocumentMetadata, M114);

            //M117
            List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentTitle> mDocumentTitle = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentTitle>();
            foreach (var M117 in M114.M115.M117)
            {
                var documentTitle = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentTitle();
                documentTitle.Text = EXOCustomizeProperty.GetEXOItemPropertyValue(M117.ExchangeMetadataAsSource, M117.DefaultValue, M117.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                mDocumentTitle.Add(documentTitle);
            }
            mDocumentMetadata.DocumentTitle = mDocumentTitle;

            //M118
            AddDocumentMetadataDocumentSubject(ref mDocumentMetadata, M114);

            //Description M119
            AddDocumentMetadataDocumentDescription(ref mDocumentMetadata, M114);

            //120
            AddDocumentMetadataDocumentLanguage(ref mDocumentMetadata, M114);

            //Reference No. M121
            AddDocumentMetadataDocumentRelation(ref mDocumentMetadata, M114);

            //122
            AddDocumentMetadataDocumentCoverage(ref mDocumentMetadata, M114);

            //155
            AddDocumentMetadataDocumentFunction(ref mDocumentMetadata, M114);

            //M123
            AddDocumentMetadataDocumentDate(ref mDocumentMetadata, M114);

            //124
            AddDocumentMetadataDocumentType(ref mDocumentMetadata, M114);

            //125
            AddDocumentMetadataDocumentSource(ref mDocumentMetadata, M114);

            mRecordDocument.DocumentMetadata = mDocumentMetadata;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private void AddSignedObjectObjectContentRecordDocumentDocumentEncodingElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocument RecordDocument, string fileUrl, FileFormatType fileType, string filename, EXORecordVEO_M114_Document M114)
        {
            List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentEncoding> mEncoding = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentEncoding>();
            foreach (var M126 in M114.M126)
            {
                var encoding = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentEncoding();
                encoding.id = "Revision-1-Document-1-Encoding-1";
                if (M126.M133 != null)
                {
                    var mDocumentData = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentEncodingDocumentData();
                    mDocumentData.id = "Revision-1-Document-1-Encoding-1-DocumentData";
                    mDocumentData.Value = EXOCustomizeProperty.GetEXOItemPropertyValue(M126.M133.ExchangeMetadataAsSource, M126.M133.DefaultValue, M126.M133.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                    encoding.DocumentData = mDocumentData;
                }
                var mEncodingMetadata = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentEncodingEncodingMetadata();
                var mFileEncoding = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentEncodingEncodingMetadataFileEncoding();

                #region mFileEncoding暂时改为文件后缀名
                //if (fileType == FileFormatType.PDF)
                //{
                //    mFileEncoding.Text = "The content of the DocumentData element is a PDF file. The file conforms to 'PDF Reference', 3rd Edition, Adobe Portable Document Format, Version 1.4, Adobe Systems Incorporated, Addison Wesley, 2001, ISBN 0-201-75839- (http://partners.adobe.com/asn/developer/acrosdk/docs/filefmtspecs/PDFReference.pdf visited 7 January 2003) as modified in the 'Errata for PDF Reference, third edition' (http://partners.adobe.com/asn/developer/acrosdk/docs/PDF14errata.txt visited 7 January 2003). It may contain digital signatures defined by PDF Public-key Digital Signature and Encryption Specification, Version 3.2, Jim Pravetz, 12 September 2001, Adobe Systems Incorporated (http://partners.adobe.com/asn/developer/pdfs/tn/ppk_pdfspec.pdf visited 28 March 2003) and the appearance of the digital signature in a PDF document is defined in Digital Signature Appearances for Public-Key Interoperability, Adobe Systems Incorporated, September 2001, (http://partners.adobe.com/asn/developer/pdfs/tn/PPKAppearances.pdf visited 28 March 2003). The file has been encoded using Base64 which is defined in IETF RFC 2045 \"Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies\", Section 6.8 \"Base64 Content-Transfer-Encoding\".";
                //}
                //else
                //{
                //    mFileEncoding.Text = "The content of the DocumentData element is a Office file.";
                //}
                #endregion

                if (M126.M127.M128 != null)
                {
                    mFileEncoding.Text = EXOCustomizeProperty.GetEXOItemPropertyValue(M126.M127.M128.ExchangeMetadataAsSource, M126.M127.M128.DefaultValue, M126.M127.M128.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }

                mEncodingMetadata.FileEncoding = mFileEncoding;

                if (M126.M127.M129 != null)
                {
                    mEncodingMetadata.SourceFileIdentifier = EXOCustomizeProperty.GetEXOItemPropertyValue(M126.M127.M129.ExchangeMetadataAsSource, M126.M127.M129.DefaultValue, M126.M127.M129.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                }

                var mFileRendering = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentEncodingEncodingMetadataFileRendering();

                List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentEncodingEncodingMetadataFileRenderingRenderingText> mRenderingText = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentEncodingEncodingMetadataFileRenderingRenderingText>();
                foreach (var M131 in M126.M127.M130.M131)
                {
                    var renderingText = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentEncodingEncodingMetadataFileRenderingRenderingText();
                    renderingText.Text = EXOCustomizeProperty.GetEXOItemPropertyValue(M131.ExchangeMetadataAsSource, M131.DefaultValue, M131.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                    mRenderingText.Add(renderingText);
                }
                mFileRendering.RenderingText = mRenderingText;
                mFileRendering.RenderingKeywords = "'.b64; ." + EXOCustomizeProperty.GetEXOItemPropertyValue(M126.M127.M130.M132.ExchangeMetadataAsSource, M126.M127.M130.M132.DefaultValue, M126.M127.M130.M132.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass) + "'";
                mEncodingMetadata.FileRendering = mFileRendering;
                encoding.EncodingMetadata = mEncodingMetadata;
                mEncoding.Add(encoding);
            }
            RecordDocument.Encoding = mEncoding;
        }

        private void AddDocumentMetadataAgentElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, EXORecordVEO_M114_Document M114)
        {
            //M116
            List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent> mDocumentAgent = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent>();
            foreach (var M116 in M114.M115.M116)
            {
                var documentAgent = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent();
                documentAgent.Text = EXOCustomizeProperty.GetEXOItemPropertyValue(M116.ExchangeMetadataAsSource, M116.DefaultValue, M116.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                mDocumentAgent.Add(documentAgent);
            }
            mDocumentMetadata.DocumentAgent = mDocumentAgent;

            #region 废弃
            //var DocumentAgent2 = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent();
            //DocumentAgent2.Text = string.Format("Sub-Section:{0}", EXORecordVEOParameters.VCustomerMapping.SubSection);
            //mDocumentMetadata.DocumentAgent[1] = DocumentAgent2;

            //var DocumentAgent3 = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent();
            //DocumentAgent3.Text = string.Format("Agency:{0}", EXORecordVEOParameters.VCustomerMapping.Agency);
            //mDocumentMetadata.DocumentAgent[2] = DocumentAgent3;

            //var DocumentAgent4 = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent();
            //DocumentAgent4.Text = string.Format("Group:{0}", EXORecordVEOParameters.VCustomerMapping.Group);
            //mDocumentMetadata.DocumentAgent[3] = DocumentAgent4;

            //var DocumentAgent5 = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent();
            //DocumentAgent5.Text = string.Format("Division:{0}", EXORecordVEOParameters.VCustomerMapping.Division);
            //mDocumentMetadata.DocumentAgent[4] = DocumentAgent5;

            //var DocumentAgent6 = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent();
            //DocumentAgent6.Text = string.Format("Branch:{0}", EXORecordVEOParameters.VCustomerMapping.Branch);
            //mDocumentMetadata.DocumentAgent[5] = DocumentAgent6;

            //List<ColumnData> M116s = new List<ColumnData>();
            //if (columns.ContainsKey("Section"))
            //{
            //    M116s.Add(new ColumnData("Section", columns["Section"]));
            //}
            //if (columns.ContainsKey("Sub-Section"))
            //{
            //    M116s.Add(new ColumnData("Sub-Section", columns["Sub-Section"]));
            //}
            //if (columns.ContainsKey("Agency"))
            //{
            //    M116s.Add(new ColumnData("Agency", columns["Agency"]));
            //}
            //if (columns.ContainsKey("Group"))
            //{
            //    M116s.Add(new ColumnData("Group", columns["Group"]));
            //}
            //if (columns.ContainsKey("Division"))
            //{
            //    M116s.Add(new ColumnData("Division", columns["Division"]));
            //}
            //if (columns.ContainsKey("Branch"))
            //{
            //    M116s.Add(new ColumnData("Branch", columns["Branch"]));
            //}

            //mDocumentMetadata.DocumentAgent = new VEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent[M116s.Count];
            //for (int i = 0; i > M116s.Count; i++)
            //{
            //    var DocumentAgent = new VEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent();
            //    DocumentAgent.Text = string.Format("{0}:{1}", M116s[i].ColumnKey, M116s[i].ColumnValue);
            //    mDocumentMetadata.DocumentAgent[i] = DocumentAgent;
            //}


            //if (columns.ContainsKey("Section"))
            //{
            //    var DocumentAgent1 = new VEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent();
            //    DocumentAgent1.Text = string.Format("Section:{0}", columns["Section"]);
            //    mDocumentMetadata.DocumentAgent[0] = DocumentAgent1;
            //}
            //if (columns.ContainsKey("Sub-Section"))
            //{
            //    var DocumentAgent2 = new VEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent();
            //    DocumentAgent2.Text = string.Format("Sub-Section:{0}", "");
            //    mDocumentMetadata.DocumentAgent[1] = DocumentAgent2;
            //}

            //var DocumentAgent3 = new VEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent();
            //DocumentAgent3.Text = string.Format("Agency:{0}", "");
            //mDocumentMetadata.DocumentAgent[2] = DocumentAgent3;

            //var DocumentAgent4 = new VEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent();
            //DocumentAgent4.Text = string.Format("Group:{0}", "");
            //mDocumentMetadata.DocumentAgent[3] = DocumentAgent4;

            //var DocumentAgent5 = new VEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent();
            //DocumentAgent5.Text = string.Format("Division:{0}", "");
            //mDocumentMetadata.DocumentAgent[4] = DocumentAgent5;

            //var DocumentAgent6 = new VEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent();
            //DocumentAgent6.Text = string.Format("Branch:{0}", "");
            //mDocumentMetadata.DocumentAgent[5] = DocumentAgent6;
            #endregion
        }

        private void AddDocumentMetadataDocumentRightsManagementElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, EXORecordVEO_M114_Document M114)
        {
            //M154
            var mRightMangement = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentRightsManagement>();
            foreach (var M154 in M114.M115.M154)
            {
                var rightMangement = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentRightsManagement();
                rightMangement.Text = EXOCustomizeProperty.GetEXOItemPropertyValue(M154.ExchangeMetadataAsSource, M154.DefaultValue, M154.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                mRightMangement.Add(rightMangement);
            }
            mDocumentMetadata.DocumentRightsManagement = mRightMangement;
        }

        private void AddDocumentMetadataDocumentSubject(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, EXORecordVEO_M114_Document M114)
        {
            //M118
            var mSubject = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentSubject>();
            foreach (var M118 in M114.M115.M118)
            {
                var subject = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentSubject();
                subject.Text = EXOCustomizeProperty.GetEXOItemPropertyValue(M118.ExchangeMetadataAsSource, M118.DefaultValue, M118.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                mSubject.Add(subject);
            }
            mDocumentMetadata.DocumentSubject = mSubject;
        }

        private void AddDocumentMetadataDocumentDate(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, EXORecordVEO_M114_Document M114)
        {
            //M123
            if (M114.M115.M123 != null)
            {
                var documentDate = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentDate();
                documentDate.Text = EXOCustomizeProperty.GetEXOItemPropertyValue(M114.M115.M123.ExchangeMetadataAsSource, M114.M115.M123.DefaultValue, M114.M115.M123.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                mDocumentMetadata.DocumentDate = documentDate;
            }
        }

        private void AddDocumentMetadataDocumentDescription(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, EXORecordVEO_M114_Document M114)
        {
            ////Description M119
            var mDescription = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentDescription>();
            foreach (var M119 in M114.M115.M119)
            {
                var description = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentDescription();
                description.Text = EXOCustomizeProperty.GetEXOItemPropertyValue(M119.ExchangeMetadataAsSource, M119.DefaultValue, M119.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                mDescription.Add(description);
            }
            mDocumentMetadata.DocumentDescription = mDescription;
        }

        private void AddDocumentMetadataDocumentSource(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, EXORecordVEO_M114_Document M114)
        {
            List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentSource> mDocumentSource = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentSource>();
            foreach (var M125 in M114.M115.M125)
            {
                var documentSource = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentSource();
                documentSource.Text = EXOCustomizeProperty.GetEXOItemPropertyValue(M125.ExchangeMetadataAsSource, M125.DefaultValue, M125.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                mDocumentSource.Add(documentSource);
            }

            mDocumentMetadata.DocumentSource = mDocumentSource;
        }

        private void AddDocumentMetadataDocumentLanguage(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, EXORecordVEO_M114_Document M114)
        {
            //120
            var mLanguage = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentLanguage>();
            foreach (var M120 in M114.M115.M120)
            {
                var language = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentLanguage();
                language.Text = EXOCustomizeProperty.GetEXOItemPropertyValue(M120.ExchangeMetadataAsSource, M120.DefaultValue, M120.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                mLanguage.Add(language);
            }
            mDocumentMetadata.DocumentLanguage = mLanguage;
        }

        private void AddDocumentMetadataDocumentRelation(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, EXORecordVEO_M114_Document M114)
        {
            ////Reference No. M121
            var mRelation = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentRelation>();
            foreach (var M121 in M114.M115.M121)
            {
                var relation = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentRelation();
                relation.Text = EXOCustomizeProperty.GetEXOItemPropertyValue(M121.ExchangeMetadataAsSource, M121.DefaultValue, M121.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                mRelation.Add(relation);
            }
            mDocumentMetadata.DocumentRelation = mRelation;
        }

        private void AddDocumentMetadataDocumentCoverage(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, EXORecordVEO_M114_Document M114)
        {
            //M122
            var mCoverage = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentCoverage>();
            foreach (var M122 in M114.M115.M122)
            {
                var coverage = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentCoverage();
                coverage.Text = EXOCustomizeProperty.GetEXOItemPropertyValue(M122.ExchangeMetadataAsSource, M122.DefaultValue, M122.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                mCoverage.Add(coverage);
            }
            mDocumentMetadata.DocumentCoverage = mCoverage;
        }

        private void AddDocumentMetadataDocumentFunction(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, EXORecordVEO_M114_Document M114)
        {
            //155
            var mFunction = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentFunction>();
            foreach (var M155 in M114.M115.M155)
            {
                var function = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentFunction();
                function.Text = EXOCustomizeProperty.GetEXOItemPropertyValue(M155.ExchangeMetadataAsSource, M155.DefaultValue, M155.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                mFunction.Add(function);
            }
            mDocumentMetadata.DocumentFunction = mFunction;
        }

        private void AddDocumentMetadataDocumentType(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, EXORecordVEO_M114_Document M114)
        {
            var mType = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentType>();
            foreach (var M124 in M114.M115.M124)
            {
                var type = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentType();
                type.Text = EXOCustomizeProperty.GetEXOItemPropertyValue(M124.ExchangeMetadataAsSource, M124.DefaultValue, M124.ExchangeMetadata, EXOItem, jobID, exportPath, filePath, disposalClass);
                mType.Add(type);
            }
            mDocumentMetadata.DocumentType = mType;
        }

        private string AddValueNewLine(string res)
        {
            return string.Format("{0}{1}{0}", "\n", res);
        }

        /*private string GetSomeFullFileType(string fileExtension)
        {
            string result;
            switch (fileExtension.ToLower(System.Globalization.CultureInfo.CurrentCulture))
            {
                case "doc":
                    result = "Microsoft Word 2003 or earlier version";
                    break;
                case "xls":
                    result = "Microsoft Excel 2003 or earlier version";
                    break;
                case "ppt":
                    result = "Microsoft PowerPoint 2003 or earlier version";
                    break;
                case "docx":
                    result = "Microsoft Word 2007 or newer version";
                    break;
                case "xlsx":
                    result = "Microsoft Excel 2007 or newer version";
                    break;
                case "pptx":
                    result = "Microsoft PowerPoint 2007 or newer version";
                    break;
                case "pdf":
                    result = "Portable Document Format";
                    break;

                default:
                    result = string.Empty;
                    break;
            }
            return result;
        }*/

        private byte[] RemoveInvalidCharacter(byte[] content)
        {
            List<byte> temp = new List<byte>();
            byte tempB = new byte();
            //for (j = 0; j < content.Count; j++)
            //{
            //    tempB = content.GetEnumerator()
            //    if (tempB == 0x09 || tempB == 0x0a ||
            //        tempB == 0x0d || tempB == 0x20)
            //        continue;
            //}
            //装箱拆箱，以后想想怎么写
            IEnumerator enumerator = content.GetEnumerator();
            while (enumerator.MoveNext())
            {
                tempB = (byte)enumerator.Current;
                if (tempB == 0x09 || tempB == 0x0a ||
                    tempB == 0x0d || tempB == 0x20)
                    continue;
                temp.Add(tempB);
            }
            return temp.ToArray();
        }
    }
}