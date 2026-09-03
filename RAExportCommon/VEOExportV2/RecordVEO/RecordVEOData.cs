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
using RAExportCommon.VEOExportV2;

namespace RAExportCommon
{
    internal class RecordVEOData
    {

        private const string ALGORITHMID_SHA1WITHRSA = "1.2.840.113549.1.1.5";
        private string DateTimeString = string.Empty;
        private RecordVEOParameters recordVEOParameters = null;
        private RecordVEOXML recordVEOXML = null;
        private AveSPDoc aveDoc = null;

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        internal RecordVEOData()
        {
            //YYYY[‘-‘MM[‘-‘DD[Thh’:’mm[‘:ss]Z[xx’:’yy]]]]
            DateTimeString = DateTime.Now.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
        }

        internal RecordVEOClass.VERSEncapsulatedObject GeneratorVEOData(RecordVEOXML recordVEOXML, RecordVEOParameters para, AveSPDoc aveDoc)
        {
            recordVEOParameters = para;
            this.recordVEOXML = recordVEOXML;
            this.aveDoc = aveDoc;
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
            RecordVEO_M2_VEOFormatDescription M2 = recordVEOXML.M1.M2;
            if (M2 != null)
            {
                var des = new RecordVEOClass.VERSEncapsulatedObjectVEOFormatDescription() { Text = CustomizeProperty.GetPropertyValue(M2.SharePointMetadataAsSource, M2.DefaultValue, M2.SharePointMetadata, aveDoc) };
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
            RecordVEO_M5_ObjectMetadata M5 = recordVEOXML.M1.M4.M5;
            var mObjectMetadata = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectMetadata();
            mObjectMetadata.ObjectType = "Record";
            if (M5 != null)
            {
                mObjectMetadata.ObjectTypeDescription = CustomizeProperty.GetPropertyValue(M5.M7.SharePointMetadataAsSource, M5.M7.DefaultValue, M5.M7.SharePointMetadata, aveDoc);
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
            List<RecordVEO_M37_Subject> M37 = recordVEOXML.M1.M4.M9.M10.M11.M37;
            if (M37 != null)
            {
                var subject = new List<RecordVEOClass.Subject>();
                foreach (var temp in M37)
                {
                    var mSubject = new RecordVEOClass.Subject();
                    if (temp.M38 != null)
                    {
                        mSubject.KeywordLevel = CustomizeProperty.GetPropertyValue(temp.M38.SharePointMetadataAsSource, temp.M38.DefaultValue, temp.M38.SharePointMetadata, aveDoc);
                    }
                    if (temp.M39.Count != 0)
                    {
                        List<string> keyWord = new List<string>();
                        foreach (var M39 in temp.M39)
                        {
                            keyWord.Add(CustomizeProperty.GetPropertyValue(M39.SharePointMetadataAsSource, M39.DefaultValue, M39.SharePointMetadata, aveDoc));
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
            RecordVEO_M11_RecordMetadata M11 = recordVEOXML.M1.M4.M9.M10.M11;
            if (M11 != null)
            {
                List<RecordVEOClass.Agent> mAgent = new List<RecordVEOClass.Agent>();
                foreach (var M12 in recordVEOXML.M1.M4.M9.M10.M11.M12)
                {
                    RecordVEOClass.Agent agent = new RecordVEOClass.Agent();
                    if (M12.M13.Count != 0)
                    {
                        agent.AgentType = new List<string>();
                        foreach (var M13 in M12.M13)
                        {
                            agent.AgentType.Add(CustomizeProperty.GetPropertyValue(M13.SharePointMetadataAsSource, M13.DefaultValue, M13.SharePointMetadata, aveDoc));
                        }
                    }
                    if (M12.M14.Count != 0)
                    {
                        agent.Jurisdiction = new List<string>();
                        foreach (var M14 in M12.M14)
                        {
                            agent.Jurisdiction.Add(CustomizeProperty.GetPropertyValue(M14.SharePointMetadataAsSource, M14.DefaultValue, M14.SharePointMetadata, aveDoc));
                        }
                    }
                    if (M12.M15 != null)
                    {
                        agent.CorporateId = CustomizeProperty.GetPropertyValue(M12.M15.SharePointMetadataAsSource, M12.M15.DefaultValue, M12.M15.SharePointMetadata, aveDoc);
                    }
                    if (M12.M16.Count != 0)
                    {
                        agent.CorporateName = new List<string>();
                        foreach (var M16 in M12.M16)
                        {
                            agent.CorporateName.Add(CustomizeProperty.GetPropertyValue(M16.SharePointMetadataAsSource, M16.DefaultValue, M16.SharePointMetadata, aveDoc));
                        }
                    }
                    if (M12.M17 != null)
                    {
                        agent.PersonId = CustomizeProperty.GetPropertyValue(M12.M17.SharePointMetadataAsSource, M12.M17.DefaultValue, M12.M17.SharePointMetadata, aveDoc);
                    }
                    if (M12.M18.Count != 0)
                    {
                        agent.PersonalName = new List<string>();
                        foreach (var M18 in M12.M18)
                        {
                            agent.PersonalName.Add(CustomizeProperty.GetPropertyValue(M18.SharePointMetadataAsSource, M18.DefaultValue, M18.SharePointMetadata, aveDoc));
                        }
                    }
                    if (M12.M19.Count != 0)
                    {
                        agent.SectionName = new List<string>();
                        foreach (var M19 in M12.M19)
                        {
                            agent.SectionName.Add(CustomizeProperty.GetPropertyValue(M19.SharePointMetadataAsSource, M19.DefaultValue, M19.SharePointMetadata, aveDoc));
                        }
                    }
                    if (M12.M20.Count != 0)
                    {
                        agent.PositionName = new List<string>();
                        foreach (var M20 in M12.M20)
                        {
                            agent.PositionName.Add(CustomizeProperty.GetPropertyValue(M20.SharePointMetadataAsSource, M20.DefaultValue, M20.SharePointMetadata, aveDoc));
                        }
                    }
                    if (M12.M21.Count != 0)
                    {
                        agent.ContactDetails = new List<string>();
                        foreach (var M21 in M12.M21)
                        {
                            agent.ContactDetails.Add(CustomizeProperty.GetPropertyValue(M21.SharePointMetadataAsSource, M21.DefaultValue, M21.SharePointMetadata, aveDoc));
                        }
                    }
                    if (M12.M22.Count != 0)
                    {
                        agent.Email = new List<string>();
                        foreach (var M22 in M12.M22)
                        {
                            agent.Email.Add(CustomizeProperty.GetPropertyValue(M22.SharePointMetadataAsSource, M22.DefaultValue, M22.SharePointMetadata, aveDoc));
                        }
                    }
                    if (M12.M23.Count != 0)
                    {
                        agent.DigitalSignature = new List<string>();
                        foreach (var M23 in M12.M23)
                        {
                            agent.DigitalSignature.Add(CustomizeProperty.GetPropertyValue(M23.SharePointMetadataAsSource, M23.DefaultValue, M23.SharePointMetadata, aveDoc));
                        }
                    }
                    mAgent.Add(agent);
                }
                recordMetadata.Agent = mAgent;
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataRelationElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            List<RecordVEO_M42_Relation> M42 = recordVEOXML.M1.M4.M9.M10.M11.M42;
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
                            mRelatedItemId.Add(CustomizeProperty.GetPropertyValue(M43.SharePointMetadataAsSource, M43.DefaultValue, M43.SharePointMetadata, aveDoc));
                        }
                        relation.RelatedItemId = mRelatedItemId;
                    }
                    if (item.M44.Count != 0)
                    {
                        List<string> mRelationType = new List<string>();
                        foreach (var M44 in item.M44)
                        {
                            mRelationType.Add(CustomizeProperty.GetPropertyValue(M44.SharePointMetadataAsSource, M44.DefaultValue, M44.SharePointMetadata, aveDoc));
                        }
                        relation.RelationType = mRelationType;
                    }
                    if (item.M45.Count != 0)
                    {
                        List<string> mRelationDescrition = new List<string>();
                        foreach (var M45 in item.M45)
                        {
                            mRelationDescrition.Add(CustomizeProperty.GetPropertyValue(M45.SharePointMetadataAsSource, M45.DefaultValue, M45.SharePointMetadata, aveDoc));
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
            List<RecordVEO_M46_Coverage> M46 = recordVEOXML.M1.M4.M9.M10.M11.M46;
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
                            mJurisdiction.Add(CustomizeProperty.GetPropertyValue(M47.SharePointMetadataAsSource, M47.DefaultValue, M47.SharePointMetadata, aveDoc));
                        }
                        coverage.Jurisdiction = mJurisdiction;
                    }
                    if (item.M48.Count != 0)
                    {
                        List<string> mPlaceName = new List<string>();
                        foreach (var M48 in item.M48)
                        {
                            mPlaceName.Add(CustomizeProperty.GetPropertyValue(M48.SharePointMetadataAsSource, M48.DefaultValue, M48.SharePointMetadata, aveDoc));
                        }
                        coverage.PlaceName = mPlaceName;
                    }
                    if (item.M49.Count != 0)
                    {
                        List<string> mPeriodName = new List<string>();
                        foreach (var M49 in item.M49)
                        {
                            mPeriodName.Add(CustomizeProperty.GetPropertyValue(M49.SharePointMetadataAsSource, M49.DefaultValue, M49.SharePointMetadata, aveDoc));
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
            List<RecordVEO_M50_Function> M50 = recordVEOXML.M1.M4.M9.M10.M11.M50;
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
                            mFunctionDescriptor.Add(CustomizeProperty.GetPropertyValue(M51.SharePointMetadataAsSource, M51.DefaultValue, M51.SharePointMetadata, aveDoc));
                        }
                        function.FunctionDescriptor = mFunctionDescriptor;
                    }
                    if (item.M52.Count != 0)
                    {
                        List<string> mActivityDescriptor = new List<string>();
                        foreach (var M52 in item.M52)
                        {
                            mActivityDescriptor.Add(CustomizeProperty.GetPropertyValue(M52.SharePointMetadataAsSource, M52.DefaultValue, M52.SharePointMetadata, aveDoc));
                        }
                        function.ActivityDescriptor = mActivityDescriptor;
                    }
                    if (item.M53.Count != 0)
                    {
                        List<string> mThirdLevelDescriptor = new List<string>();
                        foreach (var M53 in item.M53)
                        {
                            mThirdLevelDescriptor.Add(CustomizeProperty.GetPropertyValue(M53.SharePointMetadataAsSource, M53.DefaultValue, M53.SharePointMetadata, aveDoc));
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
            List<RecordVEO_M41_Language> M41 = recordVEOXML.M1.M4.M9.M10.M11.M41;
            if (M41 != null)
            {
                if (M41.Count != 0)
                {
                    List<string> mLanguage = new List<string>();
                    foreach (var item in M41)
                    {
                        mLanguage.Add(CustomizeProperty.GetPropertyValue(item.SharePointMetadataAsSource, item.DefaultValue, item.SharePointMetadata, aveDoc));
                    }
                    recordMetadata.Language = mLanguage;
                }
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataRightsManagementElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            //M14
            RecordVEO_M24_RightsManagement M24 = recordVEOXML.M1.M4.M9.M10.M11.M24;
            if (M24 != null)
            {
                recordMetadata.RightsManagement = new RecordVEOClass.RightsManagement();
                if (M24.M25 != null)
                {
                    recordMetadata.RightsManagement.SecurityClassification = CustomizeProperty.GetPropertyValue(M24.M25.SharePointMetadataAsSource, M24.M25.DefaultValue, M24.M25.SharePointMetadata, aveDoc);
                }
                if (M24.M26.Count != 0)
                {
                    recordMetadata.RightsManagement.Caveat = new List<string>();
                    foreach (var M26 in M24.M26)
                    {
                        recordMetadata.RightsManagement.Caveat.Add(CustomizeProperty.GetPropertyValue(M26.SharePointMetadataAsSource, M26.DefaultValue, M26.SharePointMetadata, aveDoc));
                    }
                }
                if (M24.M27.Count != 0)
                {
                    recordMetadata.RightsManagement.Codeword = new List<string>();
                    foreach (var M27 in M24.M27)
                    {
                        recordMetadata.RightsManagement.Codeword.Add(CustomizeProperty.GetPropertyValue(M27.SharePointMetadataAsSource, M27.DefaultValue, M27.SharePointMetadata, aveDoc));
                    }
                }
                if (M24.M28.Count != 0)
                {
                    recordMetadata.RightsManagement.ReleasabilityIndicator = new List<string>();
                    foreach (var M28 in M24.M28)
                    {
                        recordMetadata.RightsManagement.ReleasabilityIndicator.Add(CustomizeProperty.GetPropertyValue(M28.SharePointMetadataAsSource, M28.DefaultValue, M28.SharePointMetadata, aveDoc));
                    }
                }
                if (M24.M29 != null)
                {
                    recordMetadata.RightsManagement.AccessStatus = CustomizeProperty.GetPropertyValue(M24.M29.SharePointMetadataAsSource, M24.M29.DefaultValue, M24.M29.SharePointMetadata, aveDoc);
                }
                if (M24.M30.Count != 0)
                {
                    recordMetadata.RightsManagement.UsageCondition = new List<string>();
                    foreach (var M30 in M24.M30)
                    {
                        recordMetadata.RightsManagement.UsageCondition.Add(CustomizeProperty.GetPropertyValue(M30.SharePointMetadataAsSource, M30.DefaultValue, M30.SharePointMetadata, aveDoc));
                    }
                }
                if (M24.M31 != null)
                {
                    recordMetadata.RightsManagement.EncryptionDetails = CustomizeProperty.GetPropertyValue(M24.M31.SharePointMetadataAsSource, M24.M31.DefaultValue, M24.M31.SharePointMetadata, aveDoc);
                }
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataTitleElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            RecordVEO_M32_Title M32 = recordVEOXML.M1.M4.M9.M10.M11.M32;
            if (M32 != null)
            {
                recordMetadata.Title = new RecordVEOClass.Title();
                if (M32.M34 != null)
                {
                    recordMetadata.Title.SchemeName = CustomizeProperty.GetPropertyValue(M32.M34.SharePointMetadataAsSource, M32.M34.DefaultValue, M32.M34.SharePointMetadata, aveDoc);
                }
                if (M32.M33.Count != 0)
                {
                    List<string> schemeType = new List<string>();
                    foreach (var M33 in M32.M33)
                    {
                        schemeType.Add(CustomizeProperty.GetPropertyValue(M33.SharePointMetadataAsSource, M33.DefaultValue, M33.SharePointMetadata, aveDoc));
                    }
                    recordMetadata.Title.SchemeType = schemeType;
                }
                if (M32.M35 != null)
                {
                    recordMetadata.Title.TitleWords = CustomizeProperty.GetPropertyValue(M32.M35.SharePointMetadataAsSource, M32.M35.DefaultValue, M32.M35.SharePointMetadata, aveDoc);
                }
                if (M32.M36.Count != 0)
                {
                    List<string> alternative = new List<string>();
                    foreach (var M36 in M32.M36)
                    {
                        alternative.Add(CustomizeProperty.GetPropertyValue(M36.SharePointMetadataAsSource, M36.DefaultValue, M36.SharePointMetadata, aveDoc));
                    }
                    recordMetadata.Title.Alternative = alternative;
                }
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataDescriptionElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            List<RecordVEO_M40_Description> M40 = recordVEOXML.M1.M4.M9.M10.M11.M40;
            if (M40 != null)
            {
                if (M40.Count != 0)
                {
                    List<string> mDescription = new List<string>();
                    foreach (var item in M40)
                    {
                        mDescription.Add(CustomizeProperty.GetPropertyValue(item.SharePointMetadataAsSource, item.DefaultValue, item.SharePointMetadata, aveDoc));
                    }
                    recordMetadata.Description = mDescription;
                }
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataAuxiliaryDescriptionElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            List<RecordVEO_M153_AuxiliaryDescription> M153 = recordVEOXML.M1.M4.M9.M10.M11.M153;
            if (M153 != null)
            {
                if (M153.Count != 0)
                {
                    List<string> mAuxiliaryDescription = new List<string>();
                    foreach (var item in M153)
                    {
                        mAuxiliaryDescription.Add(CustomizeProperty.GetPropertyValue(item.SharePointMetadataAsSource, item.DefaultValue, item.SharePointMetadata, aveDoc));
                    }
                    recordMetadata.AuxiliaryDescription = mAuxiliaryDescription;
                }
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataDisposalElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            RecordVEO_M88_Disposal M88 = recordVEOXML.M1.M4.M9.M10.M11.M88;
            if (M88 != null)
            {
                recordMetadata.Disposal = new RecordVEOClass.Disposal();
                if (M88.M89.Count != 0)
                {
                    List<string> disposalAuthorisation = new List<string>();
                    foreach (var M89 in M88.M89)
                    {
                        disposalAuthorisation.Add(CustomizeProperty.GetPropertyValue(M89.SharePointMetadataAsSource, M89.DefaultValue, M89.SharePointMetadata, aveDoc));
                    }
                    recordMetadata.Disposal.DisposalAuthorisation = disposalAuthorisation;
                }
                if (M88.M90 != null)
                {
                    recordMetadata.Disposal.Sentence = CustomizeProperty.GetPropertyValue(M88.M90.SharePointMetadataAsSource, M88.M90.DefaultValue, M88.M90.SharePointMetadata, aveDoc);
                }
                if (M88.M91 != null)
                {
                    recordMetadata.Disposal.DisposalActionDue = CustomizeProperty.GetPropertyValue(M88.M91.SharePointMetadataAsSource, M88.M91.DefaultValue, M88.M91.SharePointMetadata, aveDoc);
                }
                if (M88.M92 != null)
                {
                    recordMetadata.Disposal.DisposalStatus = CustomizeProperty.GetPropertyValue(M88.M92.SharePointMetadataAsSource, M88.M92.DefaultValue, M88.M92.SharePointMetadata, aveDoc);
                }
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataMandateElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            List<RecordVEO_M93_Mandate> M93 = recordVEOXML.M1.M4.M9.M10.M11.M93;
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
                            mMandateType.Add(CustomizeProperty.GetPropertyValue(M94.SharePointMetadataAsSource, M94.DefaultValue, M94.SharePointMetadata, aveDoc));
                        }
                        mandate.MandateType = mMandateType;
                    }
                    if (item.M95.Count != 0)
                    {
                        List<string> mRefersto = new List<string>();
                        foreach (var M95 in item.M95)
                        {
                            mRefersto.Add(CustomizeProperty.GetPropertyValue(M95.SharePointMetadataAsSource, M95.DefaultValue, M95.SharePointMetadata, aveDoc));
                        }
                        mandate.RefersTo = mRefersto;
                    }
                    if (item.M96.Count != 0)
                    {
                        List<string> mMandateName = new List<string>();
                        foreach (var M96 in item.M96)
                        {
                            mMandateName.Add(CustomizeProperty.GetPropertyValue(M96.SharePointMetadataAsSource, M96.DefaultValue, M96.SharePointMetadata, aveDoc));
                        }
                        mandate.MandateName = mMandateName;
                    }
                    if (item.M97.Count != 0)
                    {
                        List<string> mMandateReference = new List<string>();
                        foreach (var M97 in item.M97)
                        {
                            mMandateReference.Add(CustomizeProperty.GetPropertyValue(M97.SharePointMetadataAsSource, M97.DefaultValue, M97.SharePointMetadata, aveDoc));
                        }
                        mandate.MandateReference = mMandateReference;
                    }
                    if (item.M98.Count != 0)
                    {
                        List<string> mRequirement = new List<string>();
                        foreach (var M98 in item.M98)
                        {
                            mRequirement.Add(CustomizeProperty.GetPropertyValue(M98.SharePointMetadataAsSource, M98.DefaultValue, M98.SharePointMetadata, aveDoc));
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
            RecordVEO_M54_Date M54 = recordVEOXML.M1.M4.M9.M10.M11.M54;
            if (M54 != null)
            {
                recordMetadata.Date = new RecordVEOClass.Date();
                if (M54.M55 != null)
                {
                    recordMetadata.Date.DateTimeCreated = CustomizeProperty.GetPropertyValue(M54.M55.SharePointMetadataAsSource, M54.M55.DefaultValue, M54.M55.SharePointMetadata, aveDoc);
                }
                if (M54.M56 != null)
                {
                    recordMetadata.Date.DateTimeRegistered = CustomizeProperty.GetPropertyValue(M54.M57.SharePointMetadataAsSource, M54.M57.DefaultValue, M54.M57.SharePointMetadata, aveDoc);
                }
                if (M54.M57 != null)
                {
                    recordMetadata.Date.DateTimeTransacted = CustomizeProperty.GetPropertyValue(M54.M56.SharePointMetadataAsSource, M54.M56.DefaultValue, M54.M56.SharePointMetadata, aveDoc);
                }
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataTypeElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            RecordVEO_M58_Type M58 = recordVEOXML.M1.M4.M9.M10.M11.M58;
            if (M58 != null)
            {
                recordMetadata.Type = CustomizeProperty.GetPropertyValue(M58.SharePointMetadataAsSource, M58.DefaultValue, M58.SharePointMetadata, aveDoc);
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataRecordIdentifierElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            RecordVEO_M65_RecordIdentifier M65 = recordVEOXML.M1.M4.M9.M10.M11.M65;
            if (M65 != null)
            {
                recordMetadata.RecordIdentifier = CustomizeProperty.GetPropertyValue(M65.SharePointMetadataAsSource, M65.DefaultValue, M65.SharePointMetadata, aveDoc);
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataFormatElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            RecordVEO_M60_Format M60 = recordVEOXML.M1.M4.M9.M10.M11.M60;
            if (M60 != null)
            {
                var mFormat = new RecordVEOClass.Format();
                if (M60.M61 != null)
                {
                    mFormat.MediaFormat = CustomizeProperty.GetPropertyValue(M60.M61.SharePointMetadataAsSource, M60.M61.DefaultValue, M60.M61.SharePointMetadata, aveDoc);
                }
                if (M60.M62 != null)
                {
                    mFormat.DataFormat = CustomizeProperty.GetPropertyValue(M60.M62.SharePointMetadataAsSource, M60.M62.DefaultValue, M60.M62.SharePointMetadata, aveDoc);
                }
                if (M60.M63 != null)
                {
                    mFormat.Medium = CustomizeProperty.GetPropertyValue(M60.M63.SharePointMetadataAsSource, M60.M63.DefaultValue, M60.M63.SharePointMetadata, aveDoc);

                }
                if (M60.M64.Count != 0)
                {
                    mFormat.Extent = new List<string>();
                    foreach (var M64 in M60.M64)
                    {
                        mFormat.Extent.Add(CustomizeProperty.GetPropertyValue(M64.SharePointMetadataAsSource, M64.DefaultValue, M64.SharePointMetadata, aveDoc));
                    }
                }
                recordMetadata.Format = mFormat;
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataVEOIdentifierElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            RecordVEO_M99_VEOIdentifier M99 = recordVEOXML.M1.M4.M9.M10.M11.M99;
            if (M99 != null)
            {
                var mVEOIdentifier = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadataVEOIdentifier();
                if (M99.M102.Count != 0)
                {
                    List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadataVEOIdentifierFileIdentifier> mFileIdentifier = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadataVEOIdentifierFileIdentifier>();
                    foreach (var M102 in M99.M102)
                    {
                        var fileIdentifier = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadataVEOIdentifierFileIdentifier();
                        //fileIdentifier.Text = recordVEOParameters.VLibraryID;
                        if (aveDoc.ParentFolder.ServerRelativeUrl != string.Empty
                            && aveDoc.ParentFolder.AveList.ServerRelativeUrl != string.Empty
                            && aveDoc.ParentFolder.ServerRelativeUrl.EqualsIgnoreCase(aveDoc.ParentFolder.AveList.ServerRelativeUrl))
                        {
                            fileIdentifier.Text = aveDoc.ParentFolder.AveList.Id.ToString();
                        }
                        else
                        {
                            fileIdentifier.Text = aveDoc.ParentFolder.Id.ToString();
                        }
                        mFileIdentifier.Add(fileIdentifier);
                    }
                    mVEOIdentifier.FileIdentifier = mFileIdentifier;
                }
                if (M99.M103 != null)
                {
                    var mVERSRecordIdentifier = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadataVEOIdentifierVERSRecordIdentifier();
                    mVERSRecordIdentifier.Text = CustomizeProperty.GetPropertyValue(M99.M103.SharePointMetadataAsSource, M99.M103.DefaultValue, M99.M103.SharePointMetadata, aveDoc);
                    mVEOIdentifier.VERSRecordIdentifier = mVERSRecordIdentifier;
                }
                if (M99.M100 != null)
                {
                    var mAgencyIdentifier = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadataVEOIdentifierAgencyIdentifier();
                    mAgencyIdentifier.Text = CustomizeProperty.GetPropertyValue(M99.M100.SharePointMetadataAsSource, M99.M100.DefaultValue, M99.M100.SharePointMetadata, aveDoc);
                    mVEOIdentifier.AgencyIdentifier = mAgencyIdentifier;

                }
                if (M99.M101 != null)
                {
                    var mSeriesIdentifier = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadataVEOIdentifierSeriesIdentifier();
                    mSeriesIdentifier.Text = CustomizeProperty.GetPropertyValue(M99.M101.SharePointMetadataAsSource, M99.M101.DefaultValue, M99.M101.SharePointMetadata, aveDoc);
                    mVEOIdentifier.SeriesIdentifier = mSeriesIdentifier;
                }

                recordMetadata.VEOIdentifier = mVEOIdentifier;
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataTransactionElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            List<RecordVEO_M104_Transaction> M104 = recordVEOXML.M1.M4.M9.M10.M11.M104;
            if (M104.Count != 0)
            {
                var mTransaction = new List<RecordVEOClass.Transaction>();
                foreach (var item in M104)
                {
                    var transaction = new RecordVEOClass.Transaction();
                    if (item.M105 != null)
                    {
                        transaction.TransactionIdentifier = CustomizeProperty.GetPropertyValue(item.M105.SharePointMetadataAsSource, item.M105.DefaultValue, item.M105.SharePointMetadata, aveDoc);
                    }
                    if (item.M106 != null)
                    {
                        transaction.Orginator = CustomizeProperty.GetPropertyValue(item.M106.SharePointMetadataAsSource, item.M106.DefaultValue, item.M106.SharePointMetadata, aveDoc);
                    }
                    if (item.M107.Count != 0)
                    {
                        List<string> mRecipient = new List<string>();
                        foreach (var M107 in item.M107)
                        {
                            mRecipient.Add(CustomizeProperty.GetPropertyValue(M107.SharePointMetadataAsSource, M107.DefaultValue, M107.SharePointMetadata, aveDoc));
                        }
                        transaction.Recipient = mRecipient;
                    }
                    if (item.M108.Count != 0)
                    {
                        List<string> mActionRequired = new List<string>();
                        foreach (var M108 in item.M108)
                        {
                            mActionRequired.Add(CustomizeProperty.GetPropertyValue(M108.SharePointMetadataAsSource, M108.DefaultValue, M108.SharePointMetadata, aveDoc));
                        }
                        transaction.ActionRequired = mActionRequired;
                    }
                    if (item.M109 != null)
                    {
                        transaction.OriginatorsCopy = CustomizeProperty.GetPropertyValue(item.M109.SharePointMetadataAsSource, item.M109.DefaultValue, item.M109.SharePointMetadata, aveDoc);
                    }
                    if (item.M110.Count != 0)
                    {
                        List<string> mTransactionType = new List<string>();
                        foreach (var M110 in item.M110)
                        {
                            mTransactionType.Add(CustomizeProperty.GetPropertyValue(M110.SharePointMetadataAsSource, M110.DefaultValue, M110.SharePointMetadata, aveDoc));
                        }
                        transaction.TransactionType = mTransactionType;
                    }
                    if (item.M111.Count != 0)
                    {
                        List<string> mBusinessProcedureReference = new List<string>();
                        foreach (var M111 in item.M111)
                        {
                            mBusinessProcedureReference.Add(CustomizeProperty.GetPropertyValue(M111.SharePointMetadataAsSource, M111.DefaultValue, M111.SharePointMetadata, aveDoc));
                        }
                        transaction.BusinessProcedureReference = mBusinessProcedureReference;
                    }
                    if (item.M112.Count != 0)
                    {
                        List<string> mTransactionReference = new List<string>();
                        foreach (var M112 in item.M112)
                        {
                            mTransactionReference.Add(CustomizeProperty.GetPropertyValue(M112.SharePointMetadataAsSource, M112.DefaultValue, M112.SharePointMetadata, aveDoc));
                        }
                        transaction.TransactionReference = mTransactionReference;
                    }
                    if (item.M113.Count != 0)
                    {
                        List<string> mTransactionLinkage = new List<string>();
                        foreach (var M113 in item.M113)
                        {
                            mTransactionLinkage.Add(CustomizeProperty.GetPropertyValue(M113.SharePointMetadataAsSource, M113.DefaultValue, M113.SharePointMetadata, aveDoc));
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
            RecordVEO_M66_ManagementHistory M66 = recordVEOXML.M1.M4.M9.M10.M11.M66;
            if (M66 != null)
            {
                recordMetadata.ManagementHistory = new RecordVEOClass.ManagementHistory();
                List<RecordVEOClass.ManagementEvent> mManagementEvent = new List<RecordVEOClass.ManagementEvent>();
                foreach (var M67 in M66.M67)
                {
                    RecordVEOClass.ManagementEvent management = new RecordVEOClass.ManagementEvent();
                    if (M67.M68 != null)
                    {
                        management.EventDateTime = CustomizeProperty.GetPropertyValue(M67.M68.SharePointMetadataAsSource, M67.M68.DefaultValue, M67.M68.SharePointMetadata, aveDoc);
                    }
                    if (M67.M69 != null)
                    {
                        management.EventType = CustomizeProperty.GetPropertyValue(M67.M69.SharePointMetadataAsSource, M67.M69.DefaultValue, M67.M69.SharePointMetadata, aveDoc);
                    }
                    if (M67.M70 != null)
                    {
                        management.EventDescription = CustomizeProperty.GetPropertyValue(M67.M70.SharePointMetadataAsSource, M67.M70.DefaultValue, M67.M70.SharePointMetadata, aveDoc);
                    }
                    mManagementEvent.Add(management);
                }
                recordMetadata.ManagementHistory.ManagementEvent = mManagementEvent;
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataUseHistoryElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            RecordVEO_M71_UseHistory M71 = recordVEOXML.M1.M4.M9.M10.M11.M71;
            if (M71 != null)
            {
                var mUseHistory = new RecordVEOClass.UseHistory();
                mUseHistory.Use = new List<RecordVEOClass.Use>();
                foreach (var M72 in M71.M72)
                {
                    var use = new RecordVEOClass.Use();
                    if (M72.M73 != null)
                    {
                        use.UseDateTime = CustomizeProperty.GetPropertyValue(M72.M73.SharePointMetadataAsSource, M72.M73.DefaultValue, M72.M73.SharePointMetadata, aveDoc);
                    }
                    if (M72.M74 != null)
                    {
                        use.UseType = CustomizeProperty.GetPropertyValue(M72.M74.SharePointMetadataAsSource, M72.M74.DefaultValue, M72.M74.SharePointMetadata, aveDoc);
                    }
                    if (M72.M75 != null)
                    {
                        use.UseDescription = CustomizeProperty.GetPropertyValue(M72.M75.SharePointMetadataAsSource, M72.M75.DefaultValue, M72.M75.SharePointMetadata, aveDoc);
                    }
                    mUseHistory.Use.Add(use);
                }
                recordMetadata.UseHistory = mUseHistory;
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataPreservationHistoryElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            RecordVEO_M76_PreservationHistory M76 = recordVEOXML.M1.M4.M9.M10.M11.M76;
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
                            action.ActionDateTime = CustomizeProperty.GetPropertyValue(M77.M78.SharePointMetadataAsSource, M77.M78.DefaultValue, M77.M78.SharePointMetadata, aveDoc);
                        }
                        if (M77.M79 != null)
                        {
                            action.ActionType = CustomizeProperty.GetPropertyValue(M77.M79.SharePointMetadataAsSource, M77.M79.DefaultValue, M77.M79.SharePointMetadata, aveDoc);
                        }
                        if (M77.M80 != null)
                        {
                            action.ActionDescription = CustomizeProperty.GetPropertyValue(M77.M80.SharePointMetadataAsSource, M77.M80.DefaultValue, M77.M80.SharePointMetadata, aveDoc);
                        }
                        mPreservationHistory.Action.Add(action);
                    }
                }
                if (M76.M81 != null)
                {
                    mPreservationHistory.NextAction = CustomizeProperty.GetPropertyValue(M76.M81.SharePointMetadataAsSource, M76.M81.DefaultValue, M76.M81.SharePointMetadata, aveDoc);
                }
                if (M76.M82 != null)
                {
                    mPreservationHistory.NextActionDue = CustomizeProperty.GetPropertyValue(M76.M82.SharePointMetadataAsSource, M76.M82.DefaultValue, M76.M82.SharePointMetadata, aveDoc);
                }
                recordMetadata.PreservationHistory = mPreservationHistory;
            }
        }

        private void AddSignedObjectObjectContentRecordMetadataLocationElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordRecordMetadata recordMetadata)
        {
            RecordVEO_M83_Location M83 = recordVEOXML.M1.M4.M9.M10.M11.M83;
            if (M83 != null)
            {
                var mLocation = new RecordVEOClass.Location();
                if (M83.M84 != null)
                {
                    mLocation.CurrentLocation = CustomizeProperty.GetPropertyValue(M83.M84.SharePointMetadataAsSource, M83.M84.DefaultValue, M83.M84.SharePointMetadata, aveDoc);
                }
                if (M83.M85 != null)
                {
                    mLocation.HomeLocationDetails = CustomizeProperty.GetPropertyValue(M83.M85.SharePointMetadataAsSource, M83.M85.DefaultValue, M83.M85.SharePointMetadata, aveDoc);
                }
                if (M83.M86 != null)
                {
                    mLocation.HomeStorageDetails = CustomizeProperty.GetPropertyValue(M83.M86.SharePointMetadataAsSource, M83.M86.DefaultValue, M83.M86.SharePointMetadata, aveDoc);
                }
                if (M83.M87 != null)
                {
                    mLocation.RKSID = CustomizeProperty.GetPropertyValue(M83.M87.SharePointMetadataAsSource, M83.M87.DefaultValue, M83.M87.SharePointMetadata, aveDoc);
                }
                recordMetadata.Location = mLocation;
            }
        }

        private void AddSignedObjectObjectContentRecordDocumentElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecord record)
        {
            List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocument> mDocument = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocument>();
            foreach (var M114 in recordVEOXML.M1.M4.M9.M114)
            {
                var document = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocument();
                document.id = "Revision-1-Document-1";
                AddSignedObjectObjectContentRecordDocumentDocumentMetadataElement(ref document, M114);

                AddSignedObjectObjectContentRecordDocumentDocumentEncodingElement(ref document, recordVEOParameters.VItemUrl, recordVEOParameters.GetFileFormatType(), recordVEOParameters.VFileName, M114);
                mDocument.Add(document);
            }
            record.Document = mDocument;
        }

        private void AddSignedObjectObjectContentRecordDocumentDocumentMetadataElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocument mRecordDocument, RecordVEO_M114_Document M114)
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
                documentTitle.Text = CustomizeProperty.GetPropertyValue(M117.SharePointMetadataAsSource, M117.DefaultValue, M117.SharePointMetadata, aveDoc);
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
        private void AddSignedObjectObjectContentRecordDocumentDocumentEncodingElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocument RecordDocument, string fileUrl, FileFormatType fileType, string filename, RecordVEO_M114_Document M114)
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
                    mDocumentData.Value = CustomizeProperty.GetPropertyValue(M126.M133.SharePointMetadataAsSource, M126.M133.DefaultValue, M126.M133.SharePointMetadata, aveDoc);
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
                    mFileEncoding.Text = CustomizeProperty.GetPropertyValue(M126.M127.M128.SharePointMetadataAsSource, M126.M127.M128.DefaultValue, M126.M127.M128.SharePointMetadata, aveDoc);
                }

                mEncodingMetadata.FileEncoding = mFileEncoding;

                if (M126.M127.M129 != null)
                {
                    mEncodingMetadata.SourceFileIdentifier = CustomizeProperty.GetPropertyValue(M126.M127.M129.SharePointMetadataAsSource, M126.M127.M129.DefaultValue, M126.M127.M129.SharePointMetadata, aveDoc);
                }

                var mFileRendering = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentEncodingEncodingMetadataFileRendering();

                List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentEncodingEncodingMetadataFileRenderingRenderingText> mRenderingText = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentEncodingEncodingMetadataFileRenderingRenderingText>();
                foreach (var M131 in M126.M127.M130.M131)
                {
                    var renderingText = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentEncodingEncodingMetadataFileRenderingRenderingText();
                    renderingText.Text = CustomizeProperty.GetPropertyValue(M131.SharePointMetadataAsSource, M131.DefaultValue, M131.SharePointMetadata, aveDoc);
                    mRenderingText.Add(renderingText);
                }
                mFileRendering.RenderingText = mRenderingText;
                mFileRendering.RenderingKeywords = "'.b64; ." + CustomizeProperty.GetPropertyValue(M126.M127.M130.M132.SharePointMetadataAsSource, M126.M127.M130.M132.DefaultValue, M126.M127.M130.M132.SharePointMetadata, aveDoc) + "'";
                mEncodingMetadata.FileRendering = mFileRendering;
                encoding.EncodingMetadata = mEncodingMetadata;
                mEncoding.Add(encoding);
            }
            RecordDocument.Encoding = mEncoding;
        }

        private void AddDocumentMetadataAgentElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, RecordVEO_M114_Document M114)
        {
            //M116
            List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent> mDocumentAgent = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent>();
            foreach (var M116 in M114.M115.M116)
            {
                var documentAgent = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent();
                documentAgent.Text = CustomizeProperty.GetPropertyValue(M116.SharePointMetadataAsSource, M116.DefaultValue, M116.SharePointMetadata, aveDoc);
                mDocumentAgent.Add(documentAgent);
            }
            mDocumentMetadata.DocumentAgent = mDocumentAgent;

            #region 废弃
            //var DocumentAgent2 = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent();
            //DocumentAgent2.Text = string.Format("Sub-Section:{0}", recordVEOParameters.VCustomerMapping.SubSection);
            //mDocumentMetadata.DocumentAgent[1] = DocumentAgent2;

            //var DocumentAgent3 = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent();
            //DocumentAgent3.Text = string.Format("Agency:{0}", recordVEOParameters.VCustomerMapping.Agency);
            //mDocumentMetadata.DocumentAgent[2] = DocumentAgent3;

            //var DocumentAgent4 = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent();
            //DocumentAgent4.Text = string.Format("Group:{0}", recordVEOParameters.VCustomerMapping.Group);
            //mDocumentMetadata.DocumentAgent[3] = DocumentAgent4;

            //var DocumentAgent5 = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent();
            //DocumentAgent5.Text = string.Format("Division:{0}", recordVEOParameters.VCustomerMapping.Division);
            //mDocumentMetadata.DocumentAgent[4] = DocumentAgent5;

            //var DocumentAgent6 = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentAgent();
            //DocumentAgent6.Text = string.Format("Branch:{0}", recordVEOParameters.VCustomerMapping.Branch);
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

        private void AddDocumentMetadataDocumentRightsManagementElement(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, RecordVEO_M114_Document M114)
        {
            //M154
            var mRightMangement = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentRightsManagement>();
            foreach (var M154 in M114.M115.M154)
            {
                var rightMangement = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentRightsManagement();
                rightMangement.Text = CustomizeProperty.GetPropertyValue(M154.SharePointMetadataAsSource, M154.DefaultValue, M154.SharePointMetadata, aveDoc);
                mRightMangement.Add(rightMangement);
            }
            mDocumentMetadata.DocumentRightsManagement = mRightMangement;
        }

        private void AddDocumentMetadataDocumentSubject(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, RecordVEO_M114_Document M114)
        {
            //M118
            var mSubject = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentSubject>();
            foreach (var M118 in M114.M115.M118)
            {
                var subject = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentSubject();
                subject.Text = CustomizeProperty.GetPropertyValue(M118.SharePointMetadataAsSource, M118.DefaultValue, M118.SharePointMetadata, aveDoc);
                mSubject.Add(subject);
            }
            mDocumentMetadata.DocumentSubject = mSubject;
        }

        private void AddDocumentMetadataDocumentDate(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, RecordVEO_M114_Document M114)
        {
            //M123
            if (M114.M115.M123 != null)
            {
                var documentDate = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentDate();
                documentDate.Text = CustomizeProperty.GetPropertyValue(M114.M115.M123.SharePointMetadataAsSource, M114.M115.M123.DefaultValue, M114.M115.M123.SharePointMetadata, aveDoc);
                mDocumentMetadata.DocumentDate = documentDate;
            }
        }

        private void AddDocumentMetadataDocumentDescription(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, RecordVEO_M114_Document M114)
        {
            ////Description M119
            var mDescription = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentDescription>();
            foreach (var M119 in M114.M115.M119)
            {
                var description = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentDescription();
                description.Text = CustomizeProperty.GetPropertyValue(M119.SharePointMetadataAsSource, M119.DefaultValue, M119.SharePointMetadata, aveDoc);
                mDescription.Add(description);
            }
            mDocumentMetadata.DocumentDescription = mDescription;
        }

        private void AddDocumentMetadataDocumentSource(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, RecordVEO_M114_Document M114)
        {
            List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentSource> mDocumentSource = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentSource>();
            foreach (var M125 in M114.M115.M125)
            {
                var documentSource = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentSource();
                documentSource.Text = CustomizeProperty.GetPropertyValue(M125.SharePointMetadataAsSource, M125.DefaultValue, M125.SharePointMetadata, aveDoc);
                mDocumentSource.Add(documentSource);
            }

            mDocumentMetadata.DocumentSource = mDocumentSource;
        }

        private void AddDocumentMetadataDocumentLanguage(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, RecordVEO_M114_Document M114)
        {
            //120
            var mLanguage = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentLanguage>();
            foreach (var M120 in M114.M115.M120)
            {
                var language = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentLanguage();
                language.Text = CustomizeProperty.GetPropertyValue(M120.SharePointMetadataAsSource, M120.DefaultValue, M120.SharePointMetadata, aveDoc);
                mLanguage.Add(language);
            }
            mDocumentMetadata.DocumentLanguage = mLanguage;
        }

        private void AddDocumentMetadataDocumentRelation(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, RecordVEO_M114_Document M114)
        {
            ////Reference No. M121
            var mRelation = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentRelation>();
            foreach (var M121 in M114.M115.M121)
            {
                var relation = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentRelation();
                relation.Text = CustomizeProperty.GetPropertyValue(M121.SharePointMetadataAsSource, M121.DefaultValue, M121.SharePointMetadata, aveDoc);
                mRelation.Add(relation);
            }
            mDocumentMetadata.DocumentRelation = mRelation;
        }

        private void AddDocumentMetadataDocumentCoverage(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, RecordVEO_M114_Document M114)
        {
            //M122
            var mCoverage = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentCoverage>();
            foreach (var M122 in M114.M115.M122)
            {
                var coverage = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentCoverage();
                coverage.Text = CustomizeProperty.GetPropertyValue(M122.SharePointMetadataAsSource, M122.DefaultValue, M122.SharePointMetadata, aveDoc);
                mCoverage.Add(coverage);
            }
            mDocumentMetadata.DocumentCoverage = mCoverage;
        }

        private void AddDocumentMetadataDocumentFunction(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, RecordVEO_M114_Document M114)
        {
            //155
            var mFunction = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentFunction>();
            foreach (var M155 in M114.M115.M155)
            {
                var function = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentFunction();
                function.Text = CustomizeProperty.GetPropertyValue(M155.SharePointMetadataAsSource, M155.DefaultValue, M155.SharePointMetadata, aveDoc);
                mFunction.Add(function);
            }
            mDocumentMetadata.DocumentFunction = mFunction;
        }

        private void AddDocumentMetadataDocumentType(ref RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadata mDocumentMetadata, RecordVEO_M114_Document M114)
        {
            var mType = new List<RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentType>();
            foreach (var M124 in M114.M115.M124)
            {
                var type = new RecordVEOClass.VERSEncapsulatedObjectSignedObjectObjectContentRecordDocumentDocumentMetadataDocumentType();
                type.Text = CustomizeProperty.GetPropertyValue(M124.SharePointMetadataAsSource, M124.DefaultValue, M124.SharePointMetadata, aveDoc);
                mType.Add(type);
            }
            mDocumentMetadata.DocumentType = mType;
        }

        private string AddValueNewLine(string res)
        {
            return string.Format("{0}{1}{0}", "\n", res);
        }

       


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

    public class ColumnData
    {
        public string ColumnKey;
        public object ColumnValue;

        public ColumnData(string key, object value)
        {
            ColumnKey = key;
            ColumnValue = value;
        }
    }

    public enum FileFormatType
    {
        None = 0,
        Office = 1,
        PDF = 2
    }
}