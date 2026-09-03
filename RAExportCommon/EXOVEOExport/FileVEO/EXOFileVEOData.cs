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
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using AvePoint.Wrapper.Backup;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Exchange.WebServices.Data;
using RAExportCommon.VEOExportV2;

namespace RAExportCommon
{
    public class EXOFileVEOData
    {
        private const string ALGORITHMID_SHA1WITHRSA = "1.2.840.113549.1.1.5";

        private string DateTimeString = string.Empty;
        private EXOFileVEOXML EXOFileVEOXML = null;
        private EXOFileVEOParameters EXOFileVEOParameters = null;
        private Folder EXOfolder = null;

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        internal EXOFileVEOData()
        {
            //YYYY[‘-‘MM[‘-‘DD[Thh’:’mm[‘:ss]Z[xx’:’yy]]]]
            DateTimeString = DateTime.Now.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
        }

        internal FileVEOClass.VERSEncapsulatedObject GeneratorVEOData(EXOFileVEOXML para, EXOFileVEOParameters paras, Folder folder)
        {
            EXOFileVEOXML = para;
            EXOFileVEOParameters = paras;
            this.EXOfolder = folder;
            FileVEOClass.VERSEncapsulatedObject vers = new FileVEOClass.VERSEncapsulatedObject();
            AddVEOFormatDescriptionElement(ref vers);
            AddVersionElement(ref vers);
            AddSignedObjectElement(ref vers);
            AddSignatureBlockElement(ref vers);
            AddLockSignatureBlockElement(ref vers);

            return vers;
        }

        private void AddVEOFormatDescriptionElement(ref FileVEOClass.VERSEncapsulatedObject vers)
        {
            EXOFileVEO_M2_VEOFormatDescription M2 = EXOFileVEOXML.M1.M2;
            if (M2 != null)
            {
                var des = new FileVEOClass.Text()
                {
                    Text1 = EXOCustomizeProperty.GetEXOFolderPropertyValue(M2.ExchangeMetadataAsSource, M2.DefaultValue, M2.ExchangeMetadata, EXOfolder)
                };
                vers.VEOFormatDescription = des;
            }
        }

        private void AddVersionElement(ref FileVEOClass.VERSEncapsulatedObject vers)
        {
            vers.Version = "2.0";
        }

        private void AddSignedObjectElement(ref FileVEOClass.VERSEncapsulatedObject vers)
        {
            vers.SignedObject = new FileVEOClass.SignedObject();
            vers.SignedObject.VEOVersion = "2.0";

            AddSignedObjectMetadataElement(ref vers);

            AddSignedObjectContentElement(ref vers);
        }


        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private void AddSignatureBlockElement(ref FileVEOClass.VERSEncapsulatedObject vers)
        {
            byte[] content1;
            #region SignatureBlocks
            var mSignatureBlock = new FileVEOClass.SignatureBlock();
            mSignatureBlock.id = "Revision-1-Signature-1";
            //此处的SignatureFormatDescription是根据客户给的VEO导出样例所写的，和VEO中文档中的有所不同，目前客户没有任何相关反馈,暂时不做修改.
            mSignatureBlock.SignatureFormatDescription = "The contents of this VEO is signed using SHA-512 hash algorithm and RSA digital signature algorithm. SHA-512 is defined in Secure Hash Standard, FIPS PUB 180-1, National Institute of Standards and Technology, US Department of Commerce, 17 April 1995, (http://csrc.nist.gov/publications/fips/fips180-1/fip180-1.pdf). The RSA algorithm (RSASSA-PKCS-v1_5) is defined in PKCS #1 v2.1: RSA Cryptography Standard, RSA Laboratories, 14 June 2002, (ftp://ftp.rsasecurity.com/pub/pkcs/pkcs-1/pkcs-1v2-1.pdf). Details of the public keys are encoded as X.509 certificates in the vers:CertificateBlock elements. X.509 certificates are define in \"Information technology - Open Systems Interconnection - The Directory: Public-key and attribute certificate frameworks\", ITU-T Recommendation X.509 (2000) The signature and certificates are encoded using Base64. Base64 is defined in Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies, Section 6.8, Base64 Content-Transfer- Encoding, IETF RFC 2045, N. Freed & N. Borenstein, November 1996, (http://www.ietf.org/rfc/rfc2045.txt?number=2045) The signature covers the contents of the vers:SignedObject element starting with the 'less than' symbol of the vers:SignedObject start tag up to and including the 'greater than' symbol of the vers:SignedObject end tag. Before verifying the signature all whitespace (Unicode characters U+0009, U+000A, U+000D, and U+0020) must be removed from the text";

            mSignatureBlock.SignatureAlgorithm = new FileVEOClass.SignatureAlgorithm();
            FileVEOClass.SignatureAlgorithm mSignatureAlgorithm = new FileVEOClass.SignatureAlgorithm() { SignatureAlgorithmIdentifier = ALGORITHMID_SHA1WITHRSA };
            mSignatureBlock.SignatureAlgorithm = mSignatureAlgorithm;

            mSignatureBlock.SignatureDate = DateTimeString;

            mSignatureBlock.Signer = VEOCommonString.SIGNER;

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("vers", "http://www.prov.vic.gov.au/gservice/standard/pros99007.htm");
            ns.Add("naa", "http://www.naa.gov.au/recordkeeping/control/rkms/contents.html");

            XmlSerializer xs = new XmlSerializer(typeof(FileVEOClass.VERSEncapsulatedObject));
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

            mSignatureBlock.CertificateBlock = new FileVEOClass.CertificateBlockCertificate[1];
            var mCertificateBlock = new FileVEOClass.CertificateBlockCertificate();
            mCertificateBlock.Value = AddValueNewLine(Convert.ToBase64String(AveCertificateOperation.ExportCertificateWithCertFormat()));
            mSignatureBlock.CertificateBlock[0] = mCertificateBlock;

            vers.SignatureBlock = mSignatureBlock;
            #endregion
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private void AddLockSignatureBlockElement(ref FileVEOClass.VERSEncapsulatedObject vers)
        {
            #region LockSignatureBlock
            //vers.LockSignatureBlock = new FileVEOClass.LockSignatureBlock();
            var mLockSignatureBlock = new FileVEOClass.LockSignatureBlock();

            mLockSignatureBlock.SignatureFormatDescription = "The contents of this VEO is signed using SHA-512 hash algorithm and RSA digital signature algorithm. SHA-512 is defined in Secure Hash Standard, FIPS PUB 180-1, National Institute of Standards and Technology, US Department of Commerce, 17 April 1995, (http://csrc.nist.gov/publications/fips/fips180-1/fip180-1.pdf). The RSA algorithm (RSASSA-PKCS-v1_5) is defined in PKCS #1 v2.1: RSA Cryptography Standard, RSA Laboratories, 14 June 2002, (ftp://ftp.rsasecurity.com/pub/pkcs/pkcs-1/pkcs-1v2-1.pdf). Details of the public keys are encoded as X.509 certificates in the vers:CertificateBlock elements. X.509 certificates are define in \"Information technology - Open Systems Interconnection - The Directory: Public-key and attribute certificate frameworks\", ITU-T Recommendation X.509 (2000) The signature and certificates are encoded using Base64. Base64 is defined in Multipurpose Internet Mail Extensions (MIME) Part One: Format of Internet Message Bodies, Section 6.8, Base64 Content-Transfer- Encoding, IETF RFC 2045, N. Freed & N. Borenstein, November 1996, (http://www.ietf.org/rfc/rfc2045.txt?number=2045) The signature covers the contents of the vers:Signature element starting with the first base 64 encoded character and ending with the last character. Before verifying the signature all whitespace (Unicode characters U+0009, U+000A, U+000D, and U+0020) must be removed from the text";

            mLockSignatureBlock.signsSignatureBlock = "Revision-1-Signature-1";

            var mLockSignatureBlockSignatureAlgorithm = new FileVEOClass.SignatureAlgorithm();
            mLockSignatureBlockSignatureAlgorithm.SignatureAlgorithmIdentifier = ALGORITHMID_SHA1WITHRSA;
            mLockSignatureBlock.SignatureAlgorithm = mLockSignatureBlockSignatureAlgorithm;

            mLockSignatureBlock.SignatureDate = DateTimeString;

            mLockSignatureBlock.Signer = "AvePoint";

            string signBlockStr = vers.SignatureBlock.Signature;
            byte[] reSignBlockByte = Encoding.UTF8.GetBytes(signBlockStr);

            byte[] result = SHA512WithRSASignature.Signature(RemoveInvalidCharacter(reSignBlockByte));
            mLockSignatureBlock.Signature = AddValueNewLine(Convert.ToBase64String(result));

            mLockSignatureBlock.CertificateBlock = new FileVEOClass.CertificateBlockCertificate[1];
            var mCertificateBlock = new FileVEOClass.CertificateBlockCertificate();
            mCertificateBlock.Value = AddValueNewLine(Convert.ToBase64String(AveCertificateOperation.ExportCertificateWithCertFormat()));
            mLockSignatureBlock.CertificateBlock[0] = mCertificateBlock;

            vers.LockSignatureBlock = mLockSignatureBlock;
            #endregion
        }

        private void AddSignedObjectMetadataElement(ref FileVEOClass.VERSEncapsulatedObject vers)
        {
            EXOFileVEO_M5_ObjectMetadata M5 = EXOFileVEOXML.M1.M4.M5;
            var mObjectMetadata = new FileVEOClass.ObjectMetadata();
            mObjectMetadata.ObjectType = "File";
            if (M5 != null)
            {
                mObjectMetadata.ObjectTypeDescription = EXOCustomizeProperty.GetEXOFolderPropertyValue(M5.M7.ExchangeMetadataAsSource, M5.M7.DefaultValue, M5.M7.ExchangeMetadata, EXOfolder);
            }
            mObjectMetadata.ObjectCreationDate = DateTimeString;
            vers.SignedObject.ObjectMetadata = mObjectMetadata;
        }

        private void AddSignedObjectContentElement(ref FileVEOClass.VERSEncapsulatedObject vers)
        {
            var mObjectContent = new FileVEOClass.ObjectContent();
            mObjectContent.File = new FileVEOClass.File();
            mObjectContent.File.FileMetadata = new FileVEOClass.FileMetadata();
            mObjectContent.File.FileMetadata.AggregationLevel = "File";

            AddSignedObjectContentAgentElement(ref mObjectContent);
            AddSignedObjectContentRightsManagementElement(ref mObjectContent);
            AddSignedObjectContentTitleElement(ref mObjectContent);
            AddSignedObjectContentSubjectElement(ref mObjectContent);
            AddSignedObjectContentDescriptionElement(ref mObjectContent);
            AddSignedObjectContentAuxiliaryDescriptionElement(ref mObjectContent);
            AddSignedObjectContentLanguageElement(ref mObjectContent);
            AddSignedObjectContentRelationElement(ref mObjectContent);
            AddSignedObjectContentCoverageElement(ref mObjectContent);
            AddSignedObjectContentFunctionElement(ref mObjectContent);
            AddSignedObjectContentDateElement(ref mObjectContent);
            AddSignedObjectContentTypeElement(ref mObjectContent);
            AddSignedObjectContentFormatElement(ref mObjectContent);
            AddSignedObjectContentRecordIdentifierElement(ref mObjectContent);
            AddSignedObjectContentManagementHistoryElement(ref mObjectContent);
            AddSignedObjectContentUseHistoryElement(ref mObjectContent);
            AddSignedObjectContentPreservationHistoryElement(ref mObjectContent);
            AddSignedObjectContentLocationElement(ref mObjectContent);
            AddSignedObjectContentDisposalElement(ref mObjectContent);
            AddSignedObjectContentMandateElement(ref mObjectContent);
            AddSignedObjectContentVEOIdentifierElement(ref mObjectContent);

            //mObjectContent.File.FileDisposal = new FileVEOClass.FileDisposal();
            AddSignedObjectContentFileDisposalElement(ref mObjectContent);

            vers.SignedObject.ObjectContent = mObjectContent;
        }

        private void AddSignedObjectContentLanguageElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            List<EXOFileVEO_M41_Language> M41 = EXOFileVEOXML.M1.M4.M9.M142.M143.M41;
            if (M41 != null)
            {
                if (M41.Count != 0)
                {
                    List<string> mLanguage = new List<string>();
                    foreach (var item in M41)
                    {
                        mLanguage.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(item.ExchangeMetadataAsSource, item.DefaultValue, item.ExchangeMetadata, EXOfolder));
                    }
                    mObjectContent.File.FileMetadata.Language = mLanguage;
                }
            }
        }

        private void AddSignedObjectContentAuxiliaryDescriptionElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            List<EXOFileVEO_M153_AuxiliaryDescription> M153 = EXOFileVEOXML.M1.M4.M9.M142.M143.M153;
            if (M153 != null)
            {
                if (M153.Count != 0)
                {
                    List<string> mAuxiliaryDescription = new List<string>();
                    foreach (var item in M153)
                    {
                        mAuxiliaryDescription.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(item.ExchangeMetadataAsSource, item.DefaultValue, item.ExchangeMetadata, EXOfolder));
                    }
                    mObjectContent.File.FileMetadata.AuxiliaryDescription = mAuxiliaryDescription;
                }
            }
        }

        private void AddSignedObjectContentDescriptionElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            List<EXOFileVEO_M40_Description> M40 = EXOFileVEOXML.M1.M4.M9.M142.M143.M40;
            if (M40 != null)
            {
                if (M40.Count != 0)
                {
                    List<string> mDescription = new List<string>();
                    foreach (var item in M40)
                    {
                        mDescription.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(item.ExchangeMetadataAsSource, item.DefaultValue, item.ExchangeMetadata, EXOfolder));
                    }
                    mObjectContent.File.FileMetadata.Description = mDescription;
                }
            }
        }

        private void AddSignedObjectContentSubjectElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            List<EXOFileVEO_M37_Subject> M37 = EXOFileVEOXML.M1.M4.M9.M142.M143.M37;
            if (M37 != null)
            {
                List<FileVEOClass.Subject> subject = new List<FileVEOClass.Subject>();
                foreach (var temp in M37)
                {
                    var mSubject = new FileVEOClass.Subject();
                    if (temp.M38 != null)
                    {
                        mSubject.KeywordLevel = EXOCustomizeProperty.GetEXOFolderPropertyValue(temp.M38.ExchangeMetadataAsSource, temp.M38.DefaultValue, temp.M38.ExchangeMetadata, EXOfolder);
                    }
                    if (temp.M39.Count != 0)
                    {
                        List<string> keyWord = new List<string>();
                        foreach (var M39 in temp.M39)
                        {
                            keyWord.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M39.ExchangeMetadataAsSource, M39.DefaultValue, M39.ExchangeMetadata, EXOfolder));
                        }
                        mSubject.Keyword = keyWord;
                    }
                    subject.Add(mSubject);
                }
                mObjectContent.File.FileMetadata.Subject = subject;
            }
        }

        private void AddSignedObjectContentAgentElement(ref FileVEOClass.ObjectContent ObjectContent)
        {
            EXOFileVEO_M143_FileMetadata M143 = EXOFileVEOXML.M1.M4.M9.M142.M143;
            if (M143 != null)
            {
                var mAgent = new List<FileVEOClass.Agent>();
                foreach (var M12 in M143.M12)
                {
                    var agent = new FileVEOClass.Agent();
                    if (M12.M13.Count != 0)
                    {
                        agent.AgentType = new List<string>();
                        foreach (var M13 in M12.M13)
                        {
                            agent.AgentType.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M13.ExchangeMetadataAsSource, M13.DefaultValue, M13.ExchangeMetadata, EXOfolder));
                        }
                    }
                    if (M12.M14.Count != 0)
                    {
                        agent.Jurisdiction = new List<string>();
                        foreach (var M14 in M12.M14)
                        {
                            agent.Jurisdiction.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M14.ExchangeMetadataAsSource, M14.DefaultValue, M14.ExchangeMetadata, EXOfolder));
                        }
                    }
                    if (M12.M15 != null)
                    {
                        agent.CorporateId = EXOCustomizeProperty.GetEXOFolderPropertyValue(M12.M15.ExchangeMetadataAsSource, M12.M15.DefaultValue, M12.M15.ExchangeMetadata, EXOfolder);
                    }
                    if (M12.M16.Count != 0)
                    {
                        agent.CorporateName = new List<string>();
                        foreach (var M16 in M12.M16)
                        {
                            agent.CorporateName.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M16.ExchangeMetadataAsSource, M16.DefaultValue, M16.ExchangeMetadata, EXOfolder));
                        }
                    }
                    if (M12.M17 != null)
                    {
                        agent.PersonId = EXOCustomizeProperty.GetEXOFolderPropertyValue(M12.M17.ExchangeMetadataAsSource, M12.M17.DefaultValue, M12.M17.ExchangeMetadata, EXOfolder);
                    }
                    if (M12.M18.Count != 0)
                    {
                        agent.PersonalName = new List<string>();
                        foreach (var M18 in M12.M18)
                        {
                            agent.PersonalName.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M18.ExchangeMetadataAsSource, M18.DefaultValue, M18.ExchangeMetadata, EXOfolder));
                        }
                    }
                    if (M12.M19.Count != 0)
                    {
                        agent.SectionName = new List<string>();
                        foreach (var M19 in M12.M19)
                        {
                            agent.SectionName.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M19.ExchangeMetadataAsSource, M19.DefaultValue, M19.ExchangeMetadata, EXOfolder));
                        }
                    }
                    if (M12.M20.Count != 0)
                    {
                        agent.PositionName = new List<string>();
                        foreach (var M20 in M12.M20)
                        {
                            agent.PositionName.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M20.ExchangeMetadataAsSource, M20.DefaultValue, M20.ExchangeMetadata, EXOfolder));
                        }
                    }
                    if (M12.M21.Count != 0)
                    {
                        agent.ContactDetails = new List<string>();
                        foreach (var M21 in M12.M21)
                        {
                            agent.ContactDetails.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M21.ExchangeMetadataAsSource, M21.DefaultValue, M21.ExchangeMetadata, EXOfolder));
                        }
                    }
                    if (M12.M22.Count != 0)
                    {
                        agent.Email = new List<string>();
                        foreach (var M22 in M12.M22)
                        {
                            agent.Email.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M22.ExchangeMetadataAsSource, M22.DefaultValue, M22.ExchangeMetadata, EXOfolder));
                        }
                    }
                    if (M12.M23.Count != 0)
                    {
                        agent.DigitalSignature = new List<string>();
                        foreach (var M23 in M12.M23)
                        {
                            agent.DigitalSignature.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M23.ExchangeMetadataAsSource, M23.DefaultValue, M23.ExchangeMetadata, EXOfolder));
                        }
                    }
                    mAgent.Add(agent);
                }
                ObjectContent.File.FileMetadata.Agent = mAgent;
            }
        }

        private void AddSignedObjectContentRightsManagementElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            EXOFileVEO_M24_RightsManagement M24 = EXOFileVEOXML.M1.M4.M9.M142.M143.M24;
            if (M24 != null)
            {
                var mRightsManagement = new FileVEOClass.RightsManagement();
                if (M24.M25 != null)
                {
                    mRightsManagement.SecurityClassification = EXOCustomizeProperty.GetEXOFolderPropertyValue(M24.M25.ExchangeMetadataAsSource, M24.M25.DefaultValue, M24.M25.ExchangeMetadata, EXOfolder);
                }
                if (M24.M26.Count != 0)
                {
                    mRightsManagement.Caveat = new List<string>();
                    foreach (var M26 in M24.M26)
                    {
                        mRightsManagement.Caveat.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M26.ExchangeMetadataAsSource, M26.DefaultValue, M26.ExchangeMetadata, EXOfolder));
                    }
                }
                if (M24.M27.Count != 0)
                {
                    mRightsManagement.Codeword = new List<string>();
                    foreach (var M27 in M24.M27)
                    {
                        mRightsManagement.Codeword.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M27.ExchangeMetadataAsSource, M27.DefaultValue, M27.ExchangeMetadata, EXOfolder));
                    }
                }
                if (M24.M28.Count != 0)
                {
                    mRightsManagement.ReleasabilityIndicator = new List<string>();
                    foreach (var M28 in M24.M28)
                    {
                        mRightsManagement.ReleasabilityIndicator.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M28.ExchangeMetadataAsSource, M28.DefaultValue, M28.ExchangeMetadata, EXOfolder));
                    }
                }
                if (M24.M29 != null)
                {
                    mRightsManagement.AccessStatus = EXOCustomizeProperty.GetEXOFolderPropertyValue(M24.M29.ExchangeMetadataAsSource, M24.M29.DefaultValue, M24.M29.ExchangeMetadata, EXOfolder);
                }
                if (M24.M30.Count != 0)
                {
                    mRightsManagement.UsageCondition = new List<string>();
                    foreach (var M30 in M24.M30)
                    {
                        mRightsManagement.UsageCondition.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M30.ExchangeMetadataAsSource, M30.DefaultValue, M30.ExchangeMetadata, EXOfolder));
                    }
                }
                if (M24.M31 != null)
                {
                    mRightsManagement.EncryptionDetails = EXOCustomizeProperty.GetEXOFolderPropertyValue(M24.M31.ExchangeMetadataAsSource, M24.M31.DefaultValue, M24.M31.ExchangeMetadata, EXOfolder);
                }
                mObjectContent.File.FileMetadata.RightsManagement = mRightsManagement;
            }
        }

        private void AddSignedObjectContentTitleElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            EXOFileVEO_M32_Title M32 = EXOFileVEOXML.M1.M4.M9.M142.M143.M32;
            if (M32 != null)
            {
                var mTitle = new FileVEOClass.Title();
                if (M32.M33.Count != 0)
                {
                    List<string> schemeTpye = new List<string>();
                    foreach (var M33 in M32.M33)
                    {
                        schemeTpye.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M33.ExchangeMetadataAsSource, M33.DefaultValue, M33.ExchangeMetadata, EXOfolder));
                    }
                    mTitle.SchemeType = schemeTpye;
                }
                if (M32.M34 != null)
                {
                    mTitle.SchemeName = EXOCustomizeProperty.GetEXOFolderPropertyValue(M32.M34.ExchangeMetadataAsSource, M32.M34.DefaultValue, M32.M34.ExchangeMetadata, EXOfolder);
                }
                if (M32.M35 != null)
                {
                    mTitle.TitleWords = EXOCustomizeProperty.GetEXOFolderPropertyValue(M32.M35.ExchangeMetadataAsSource, M32.M35.DefaultValue, M32.M35.ExchangeMetadata, EXOfolder);
                }
                if (M32.M36.Count != 0)
                {
                    List<string> alternative = new List<string>();
                    foreach (var M36 in M32.M36)
                    {
                        alternative.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M36.ExchangeMetadataAsSource, M36.DefaultValue, M36.ExchangeMetadata, EXOfolder));
                    }
                    mTitle.Alternative = alternative;
                }
                mObjectContent.File.FileMetadata.Title = mTitle;
            }
        }

        private void AddSignedObjectContentRelationElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            List<EXOFileVEO_M42_Relation> M42 = EXOFileVEOXML.M1.M4.M9.M142.M143.M42;
            if (M42 != null)
            {
                var mRelation = new List<FileVEOClass.Relation>();
                foreach (var item in M42)
                {
                    var relation = new FileVEOClass.Relation();
                    if (item.M43.Count != 0)
                    {
                        List<string> mRelatedItemId = new List<string>();
                        foreach (var M43 in item.M43)
                        {
                            mRelatedItemId.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M43.ExchangeMetadataAsSource, M43.DefaultValue, M43.ExchangeMetadata, EXOfolder));
                        }
                        relation.RelatedItemId = mRelatedItemId;
                    }
                    if (item.M44.Count != 0)
                    {
                        List<string> mRelationType = new List<string>();
                        foreach (var M44 in item.M44)
                        {
                            mRelationType.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M44.ExchangeMetadataAsSource, M44.DefaultValue, M44.ExchangeMetadata, EXOfolder));
                        }
                        relation.RelationType = mRelationType;
                    }
                    if (item.M45.Count != 0)
                    {
                        List<string> mRelationDescrition = new List<string>();
                        foreach (var M45 in item.M45)
                        {
                            mRelationDescrition.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M45.ExchangeMetadataAsSource, M45.DefaultValue, M45.ExchangeMetadata, EXOfolder));
                        }
                        relation.RelationDescription = mRelationDescrition;
                    }
                    mRelation.Add(relation);
                }
                mObjectContent.File.FileMetadata.Relation = mRelation;
            }
        }

        private void AddSignedObjectContentCoverageElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            List<EXOFileVEO_M46_Coverage> M46 = EXOFileVEOXML.M1.M4.M9.M142.M143.M46;
            if (M46 != null)
            {
                var mCoverage = new List<FileVEOClass.Coverage>();
                foreach (var item in M46)
                {
                    var coverage = new FileVEOClass.Coverage();
                    if (item.M47.Count != 0)
                    {
                        List<string> mJurisdiction = new List<string>();
                        foreach (var M47 in item.M47)
                        {
                            mJurisdiction.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M47.ExchangeMetadataAsSource, M47.DefaultValue, M47.ExchangeMetadata, EXOfolder));
                        }
                        coverage.Jurisdiction = mJurisdiction;
                    }
                    if (item.M48.Count != 0)
                    {
                        List<string> mPlaceName = new List<string>();
                        foreach (var M48 in item.M48)
                        {
                            mPlaceName.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M48.ExchangeMetadataAsSource, M48.DefaultValue, M48.ExchangeMetadata, EXOfolder));
                        }
                        coverage.PlaceName = mPlaceName;
                    }
                    if (item.M49.Count != 0)
                    {
                        List<string> mPeriodName = new List<string>();
                        foreach (var M49 in item.M49)
                        {
                            mPeriodName.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M49.ExchangeMetadataAsSource, M49.DefaultValue, M49.ExchangeMetadata, EXOfolder));
                        }
                        coverage.PeriodName = mPeriodName;
                    }
                    mCoverage.Add(coverage);
                }
                mObjectContent.File.FileMetadata.Coverage = mCoverage;
            }
        }

        private void AddSignedObjectContentFunctionElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            List<EXOFileVEO_M50_Function> M50 = EXOFileVEOXML.M1.M4.M9.M142.M143.M50;
            if (M50 != null)
            {
                var mFunction = new List<FileVEOClass.Function>();
                foreach (var item in M50)
                {
                    var function = new FileVEOClass.Function();
                    if (item.M51.Count != 0)
                    {
                        List<string> mFunctionDescriptor = new List<string>();
                        foreach (var M51 in item.M51)
                        {
                            mFunctionDescriptor.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M51.ExchangeMetadataAsSource, M51.DefaultValue, M51.ExchangeMetadata, EXOfolder));
                        }
                        function.FunctionDescriptor = mFunctionDescriptor;
                    }
                    if (item.M52.Count != 0)
                    {
                        List<string> mActivityDescriptor = new List<string>();
                        foreach (var M52 in item.M52)
                        {
                            mActivityDescriptor.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M52.ExchangeMetadataAsSource, M52.DefaultValue, M52.ExchangeMetadata, EXOfolder));
                        }
                        function.ActivityDescriptor = mActivityDescriptor;
                    }
                    if (item.M53.Count != 0)
                    {
                        List<string> mThirdLevelDescriptor = new List<string>();
                        foreach (var M53 in item.M53)
                        {
                            mThirdLevelDescriptor.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M53.ExchangeMetadataAsSource, M53.DefaultValue, M53.ExchangeMetadata, EXOfolder));
                        }
                        function.ThirdLevelDescriptor = mThirdLevelDescriptor;
                    }
                    mFunction.Add(function);
                }
                mObjectContent.File.FileMetadata.Function = mFunction;
            }
        }

        private void AddSignedObjectContentDateElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            EXOFileVEO_M54_Date M54 = EXOFileVEOXML.M1.M4.M9.M142.M143.M54;
            if (M54 != null)
            {
                var mDate = new FileVEOClass.Date();
                //DateTimeCreated
                if (M54.M55 != null)
                {
                    mDate.DateTimeCreated = EXOCustomizeProperty.GetEXOFolderPropertyValue(M54.M55.ExchangeMetadataAsSource, M54.M55.DefaultValue, M54.M55.ExchangeMetadata, EXOfolder);
                }
                if (M54.M56 != null)
                {
                    //DateTimeTransacted
                    mDate.DateTimeTransacted = EXOCustomizeProperty.GetEXOFolderPropertyValue(M54.M56.ExchangeMetadataAsSource, M54.M56.DefaultValue, M54.M56.ExchangeMetadata, EXOfolder);
                }
                if (M54.M57 != null)
                {
                    //DateTimeRegistered
                    mDate.DateTimeRegistered = EXOCustomizeProperty.GetEXOFolderPropertyValue(M54.M57.ExchangeMetadataAsSource, M54.M57.DefaultValue, M54.M57.ExchangeMetadata, EXOfolder);
                }
                if (M54.M144 != null)
                {
                    //dateTimeClosed
                    mDate.DateTimeClosed = EXOCustomizeProperty.GetEXOFolderPropertyValue(M54.M144.ExchangeMetadataAsSource, M54.M144.DefaultValue, M54.M144.ExchangeMetadata, EXOfolder);
                }
                mObjectContent.File.FileMetadata.Date = mDate;
            }
        }

        private void AddSignedObjectContentTypeElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            EXOFileVEO_M58_Type M58 = EXOFileVEOXML.M1.M4.M9.M142.M143.M58;
            if (M58 != null)
            {
                mObjectContent.File.FileMetadata.Type = EXOCustomizeProperty.GetEXOFolderPropertyValue(M58.ExchangeMetadataAsSource, M58.DefaultValue, M58.ExchangeMetadata, EXOfolder);
            }
        }

        private void AddSignedObjectContentRecordIdentifierElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            EXOFileVEO_M65_RecordIdentifier M65 = EXOFileVEOXML.M1.M4.M9.M142.M143.M65;
            if (M65 != null)
            {
                mObjectContent.File.FileMetadata.RecordIdentifier = EXOCustomizeProperty.GetEXOFolderPropertyValue(M65.ExchangeMetadataAsSource, M65.DefaultValue, M65.ExchangeMetadata, EXOfolder);
            }
        }

        private void AddSignedObjectContentUseHistoryElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            EXOFileVEO_M71_UseHistory M71 = EXOFileVEOXML.M1.M4.M9.M142.M143.M71;
            if (M71 != null)
            {
                var mUseHistory = new FileVEOClass.UseHistory();
                mUseHistory.Use = new List<FileVEOClass.Use>();
                foreach (var M72 in M71.M72)
                {
                    var use = new FileVEOClass.Use();
                    if (M72.M73 != null)
                    {
                        use.UseDateTime = EXOCustomizeProperty.GetEXOFolderPropertyValue(M72.M73.ExchangeMetadataAsSource, M72.M73.DefaultValue, M72.M73.ExchangeMetadata, EXOfolder);
                    }
                    if (M72.M74 != null)
                    {
                        use.UseType = EXOCustomizeProperty.GetEXOFolderPropertyValue(M72.M74.ExchangeMetadataAsSource, M72.M74.DefaultValue, M72.M74.ExchangeMetadata, EXOfolder);
                    }
                    if (M72.M75 != null)
                    {
                        use.UseDescription = EXOCustomizeProperty.GetEXOFolderPropertyValue(M72.M75.ExchangeMetadataAsSource, M72.M75.DefaultValue, M72.M75.ExchangeMetadata, EXOfolder);
                    }
                    mUseHistory.Use.Add(use);
                }
                mObjectContent.File.FileMetadata.UseHistory = mUseHistory;
            }
        }

        private void AddSignedObjectContentPreservationHistoryElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            EXOFileVEO_M76_PreservationHistory M76 = EXOFileVEOXML.M1.M4.M9.M142.M143.M76;
            if (M76 != null)
            {
                var mPreservationHistory = new FileVEOClass.PreservationHistory();
                if (M76.M77.Count != 0)
                {
                    mPreservationHistory.Action = new List<FileVEOClass.Action>();
                    foreach (var M77 in M76.M77)
                    {
                        var action = new FileVEOClass.Action();
                        if (M77.M78 != null)
                        {
                            action.ActionDateTime = EXOCustomizeProperty.GetEXOFolderPropertyValue(M77.M78.ExchangeMetadataAsSource, M77.M78.DefaultValue, M77.M78.ExchangeMetadata, EXOfolder);
                        }
                        if (M77.M79 != null)
                        {
                            action.ActionType = EXOCustomizeProperty.GetEXOFolderPropertyValue(M77.M79.ExchangeMetadataAsSource, M77.M79.DefaultValue, M77.M79.ExchangeMetadata, EXOfolder); ;
                        }
                        if (M77.M80 != null)
                        {
                            action.ActionDescription = EXOCustomizeProperty.GetEXOFolderPropertyValue(M77.M80.ExchangeMetadataAsSource, M77.M80.DefaultValue, M77.M80.ExchangeMetadata, EXOfolder);
                        }
                        mPreservationHistory.Action.Add(action);
                    }
                }
                if (M76.M81 != null)
                {
                    mPreservationHistory.NextAction = EXOCustomizeProperty.GetEXOFolderPropertyValue(M76.M81.ExchangeMetadataAsSource, M76.M81.DefaultValue, M76.M81.ExchangeMetadata, EXOfolder);
                }
                if (M76.M82 != null)
                {
                    mPreservationHistory.NextActionDue = EXOCustomizeProperty.GetEXOFolderPropertyValue(M76.M82.ExchangeMetadataAsSource, M76.M82.DefaultValue, M76.M82.ExchangeMetadata, EXOfolder);
                }
                mObjectContent.File.FileMetadata.PreservationHistory = mPreservationHistory;
            }
        }

        private void AddSignedObjectContentLocationElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            EXOFileVEO_M83_Location M83 = EXOFileVEOXML.M1.M4.M9.M142.M143.M83;
            if (M83 != null)
            {
                var mLocation = new FileVEOClass.Location();
                if (M83.M84 != null)
                {
                    mLocation.CurrentLocation = EXOCustomizeProperty.GetEXOFolderPropertyValue(M83.M84.ExchangeMetadataAsSource, M83.M84.DefaultValue, M83.M84.ExchangeMetadata, EXOfolder);
                }
                if (M83.M85 != null)
                {
                    mLocation.HomeLocationDetails = EXOCustomizeProperty.GetEXOFolderPropertyValue(M83.M85.ExchangeMetadataAsSource, M83.M85.DefaultValue, M83.M85.ExchangeMetadata, EXOfolder);
                }
                if (M83.M86 != null)
                {
                    mLocation.HomeStorageDetails = EXOCustomizeProperty.GetEXOFolderPropertyValue(M83.M86.ExchangeMetadataAsSource, M83.M86.DefaultValue, M83.M86.ExchangeMetadata, EXOfolder);
                }
                if (M83.M87 != null)
                {
                    mLocation.RKSID = EXOCustomizeProperty.GetEXOFolderPropertyValue(M83.M87.ExchangeMetadataAsSource, M83.M87.DefaultValue, M83.M87.ExchangeMetadata, EXOfolder);
                }
                mObjectContent.File.FileMetadata.Location = mLocation;
            }
        }

        private void AddSignedObjectContentFormatElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            EXOFileVEO_M60_Format M60 = EXOFileVEOXML.M1.M4.M9.M142.M143.M60;
            if (M60 != null)
            {
                var mFormat = new FileVEOClass.Format();
                if (M60.M61 != null)
                {
                    mFormat.MediaFormat = EXOCustomizeProperty.GetEXOFolderPropertyValue(M60.M61.ExchangeMetadataAsSource, M60.M61.DefaultValue, M60.M61.ExchangeMetadata, EXOfolder);
                }
                if (M60.M62 != null)
                {
                    mFormat.DataFormat = EXOCustomizeProperty.GetEXOFolderPropertyValue(M60.M62.ExchangeMetadataAsSource, M60.M62.DefaultValue, M60.M62.ExchangeMetadata, EXOfolder);
                }
                if (M60.M63 != null)
                {
                    mFormat.Medium = EXOCustomizeProperty.GetEXOFolderPropertyValue(M60.M63.ExchangeMetadataAsSource, M60.M63.DefaultValue, M60.M63.ExchangeMetadata, EXOfolder);
                }
                if (M60.M64.Count != 0)
                {
                    mFormat.Extent = new List<string>();
                    foreach (var M64 in M60.M64)
                    {
                        mFormat.Extent.Add( EXOCustomizeProperty.GetEXOFolderPropertyValue(M64.ExchangeMetadataAsSource, M64.DefaultValue, M64.ExchangeMetadata, EXOfolder));
                    }
                }
                mObjectContent.File.FileMetadata.Format = mFormat;
            }
        }

        private void AddSignedObjectContentDisposalElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            EXOFileVEO_M88_Disposal M88 = EXOFileVEOXML.M1.M4.M9.M142.M143.M88;
            if (M88 != null)
            {
                var mDisposal = new FileVEOClass.Disposal();
                if (M88.M89.Count != 0)
                {
                    List<string> disposalAuthorisation = new List<string>();
                    foreach (var M89 in M88.M89)
                    {
                        disposalAuthorisation.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M89.ExchangeMetadataAsSource, M89.DefaultValue, M89.ExchangeMetadata, EXOfolder));
                    }
                    mDisposal.DisposalAuthorisation = disposalAuthorisation;
                }
                if (M88.M90 != null)
                {
                    mDisposal.Sentence = EXOCustomizeProperty.GetEXOFolderPropertyValue(M88.M90.ExchangeMetadataAsSource, M88.M90.DefaultValue, M88.M90.ExchangeMetadata, EXOfolder);
                }
                if (M88.M91 != null)
                {
                    mDisposal.DisposalActionDue = EXOCustomizeProperty.GetEXOFolderPropertyValue(M88.M91.ExchangeMetadataAsSource, M88.M91.DefaultValue, M88.M91.ExchangeMetadata, EXOfolder);
                }
                if (M88.M92 != null)
                {
                    mDisposal.DisposalStatus = EXOCustomizeProperty.GetEXOFolderPropertyValue(M88.M92.ExchangeMetadataAsSource, M88.M92.DefaultValue, M88.M92.ExchangeMetadata, EXOfolder);
                }
                mObjectContent.File.FileMetadata.Disposal = mDisposal;
            }
        }

        private void AddSignedObjectContentMandateElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            List<EXOFileVEO_M93_Mandate> M93 = EXOFileVEOXML.M1.M4.M9.M142.M143.M93;
            if (M93.Count != 0)
            {
                var mMandate = new List<FileVEOClass.Mandate>();
                foreach (var item in M93)
                {
                    var mandate = new FileVEOClass.Mandate();
                    if (item.M94.Count != 0)
                    {
                        List<string> mMandateType = new List<string>();
                        foreach (var M94 in item.M94)
                        {
                            mMandateType.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M94.ExchangeMetadataAsSource, M94.DefaultValue, M94.ExchangeMetadata, EXOfolder));
                        }
                        mandate.MandateType = mMandateType;
                    }
                    if (item.M95.Count != 0)
                    {
                        List<string> mRefersto = new List<string>();
                        foreach (var M95 in item.M95)
                        {
                            mRefersto.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M95.ExchangeMetadataAsSource, M95.DefaultValue, M95.ExchangeMetadata, EXOfolder));
                        }
                        mandate.RefersTo = mRefersto;
                    }
                    if (item.M96.Count != 0)
                    {
                        List<string> mMandateName = new List<string>();
                        foreach (var M96 in item.M96)
                        {
                            mMandateName.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M96.ExchangeMetadataAsSource, M96.DefaultValue, M96.ExchangeMetadata, EXOfolder));
                        }
                        mandate.MandateName = mMandateName;
                    }
                    if (item.M97.Count != 0)
                    {
                        List<string> mMandateReference = new List<string>();
                        foreach (var M97 in item.M97)
                        {
                            mMandateReference.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M97.ExchangeMetadataAsSource, M97.DefaultValue, M97.ExchangeMetadata, EXOfolder));
                        }
                        mandate.MandateReference = mMandateReference;
                    }
                    if (item.M98.Count != 0)
                    {
                        List<string> mRequirement = new List<string>();
                        foreach (var M98 in item.M98)
                        {
                            mRequirement.Add(EXOCustomizeProperty.GetEXOFolderPropertyValue(M98.ExchangeMetadataAsSource, M98.DefaultValue, M98.ExchangeMetadata, EXOfolder));
                        }
                        mandate.Requirement = mRequirement;
                    }
                    mMandate.Add(mandate);
                }
                mObjectContent.File.FileMetadata.Mandate = mMandate;
            }
        }

        private void AddSignedObjectContentManagementHistoryElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            EXOFileVEO_M66_ManagementHistory M66 = EXOFileVEOXML.M1.M4.M9.M142.M143.M66;
            if (M66 != null)
            {
                mObjectContent.File.FileMetadata.ManagementHistory = new FileVEOClass.ManagementHistory();
                List<FileVEOClass.ManagementEvent> management = new List<FileVEOClass.ManagementEvent>();
                foreach (var M67 in M66.M67)
                {
                    var mManagementEvent1 = new FileVEOClass.ManagementEvent();
                    if (M67.M68 != null)
                    {
                        mManagementEvent1.EventDateTime = EXOCustomizeProperty.GetEXOFolderPropertyValue(M67.M68.ExchangeMetadataAsSource, M67.M68.DefaultValue, M67.M68.ExchangeMetadata, EXOfolder);
                    }
                    if (M67.M69 != null)
                    {
                        mManagementEvent1.EventType = EXOCustomizeProperty.GetEXOFolderPropertyValue(M67.M69.ExchangeMetadataAsSource, M67.M69.DefaultValue, M67.M69.ExchangeMetadata, EXOfolder);
                    }
                    if (M67.M70 != null)
                    {
                        mManagementEvent1.EventDescription = EXOCustomizeProperty.GetEXOFolderPropertyValue(M67.M70.ExchangeMetadataAsSource, M67.M70.DefaultValue, M67.M70.ExchangeMetadata, EXOfolder);
                    }
                    management.Add(mManagementEvent1);
                }
                mObjectContent.File.FileMetadata.ManagementHistory.ManagementEvent = management;
            }
        }

        private void AddSignedObjectContentVEOIdentifierElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            EXOFileVEO_M99_VEOIdentifier M99 = EXOFileVEOXML.M1.M4.M9.M142.M143.M99;
            if (M99 != null)
            {
                var mVEOIdentifier = new FileVEOClass.VEOIdentifier();
                if (M99.M102 != null)
                {
                    mVEOIdentifier.FileIdentifier = new FileVEOClass.Text();
                    mVEOIdentifier.FileIdentifier.Text1 = EXOFileVEOParameters.VFileID;
                }
                if (M99.M100 != null)
                {
                    mVEOIdentifier.AgencyIdentifier = new FileVEOClass.Text();
                    mVEOIdentifier.AgencyIdentifier.Text1 = EXOCustomizeProperty.GetEXOFolderPropertyValue(M99.M100.ExchangeMetadataAsSource, M99.M100.DefaultValue, M99.M100.ExchangeMetadata, EXOfolder);
                }
                if (M99.M101 != null)
                {
                    mVEOIdentifier.SeriesIdentifier = new FileVEOClass.Text();
                    mVEOIdentifier.SeriesIdentifier.Text1 = EXOCustomizeProperty.GetEXOFolderPropertyValue(M99.M101.ExchangeMetadataAsSource, M99.M101.DefaultValue, M99.M101.ExchangeMetadata, EXOfolder);
                }
                mObjectContent.File.FileMetadata.VEOIdentifier = mVEOIdentifier;
            }
        }

        private void AddSignedObjectContentFileDisposalElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            EXOFileVEO_M145_FileDisposal M145 = EXOFileVEOXML.M1.M4.M9.M142.M145;
            if (M145 != null)
            {
                var mFileDisposal = new FileVEOClass.FileDisposal();
                if (M145.M146 != null)
                {
                    mFileDisposal.DisposalSchedule = EXOCustomizeProperty.GetEXOFolderPropertyValue(M145.M146.ExchangeMetadataAsSource, M145.M146.DefaultValue, M145.M146.ExchangeMetadata, EXOfolder);
                }
                if (M145.M147 != null)
                {
                    mFileDisposal.DisposalDate = EXOCustomizeProperty.GetEXOFolderPropertyValue(M145.M147.ExchangeMetadataAsSource, M145.M147.DefaultValue, M145.M147.ExchangeMetadata, EXOfolder);
                }
                if (M145.M148 != null)
                {
                    mFileDisposal.AuthorizingOfficer = EXOCustomizeProperty.GetEXOFolderPropertyValue(M145.M148.ExchangeMetadataAsSource, M145.M148.DefaultValue, M145.M148.ExchangeMetadata, EXOfolder);
                }
                mObjectContent.File.FileDisposal = mFileDisposal;
            }
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

        private string AddValueNewLine(string res)
        {
            return string.Format("{0}{1}{0}", "\n", res);
        }
    }
}
