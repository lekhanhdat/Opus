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
using RAExportCommon.VEOExportV2;

namespace RAExportCommon
{
    public class FileVEODate
    {
        private const string ALGORITHMID_SHA512WITHRSA = "1.2.840.113549.1.1.13";
        private string DateTimeString = string.Empty;
        private FileVEOXML fileVEOXML = null;
        private FileVEOParameters fileVEOParameters = null;
        private bool isFolder = false;
        private AveSPFolder folder = null;
        private AveSPList list = null;

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        internal FileVEODate()
        {
            //YYYY[‘-‘MM[‘-‘DD[Thh’:’mm[‘:ss]Z[xx’:’yy]]]]
            DateTimeString = DateTime.Now.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
        }

        internal FileVEOClass.VERSEncapsulatedObject GeneratorVEOData(FileVEOXML para, FileVEOParameters paras, bool isFolder, AveSPFolder aveFolder = null, AveSPList aveList = null)
        {
            fileVEOXML = para;
            fileVEOParameters = paras;
            this.isFolder = isFolder;
            folder = aveFolder;
            list = aveList;
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
            FileVEO_M2_VEOFormatDescription M2 = fileVEOXML.M1.M2;
            if (M2 != null)
            {
                var des = new FileVEOClass.Text()
                {
                    Text1 = isFolder ? CustomizeProperty.GetPropertyValue(M2.SharePointMetadataAsSource, M2.DefaultValue, M2.SharePointMetadata, folder) : CustomizeProperty.GetPropertyValue(M2.SharePointMetadataAsSource, M2.DefaultValue, M2.SharePointMetadata, list)
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
            FileVEOClass.SignatureAlgorithm mSignatureAlgorithm = new FileVEOClass.SignatureAlgorithm() { SignatureAlgorithmIdentifier = ALGORITHMID_SHA512WITHRSA };
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
            mLockSignatureBlockSignatureAlgorithm.SignatureAlgorithmIdentifier = ALGORITHMID_SHA512WITHRSA;
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
            FileVEO_M5_ObjectMetadata M5 = fileVEOXML.M1.M4.M5;
            var mObjectMetadata = new FileVEOClass.ObjectMetadata();
            mObjectMetadata.ObjectType = "File";
            if (M5 != null)
            {
                mObjectMetadata.ObjectTypeDescription = isFolder ? CustomizeProperty.GetPropertyValue(M5.M7.SharePointMetadataAsSource, M5.M7.DefaultValue, M5.M7.SharePointMetadata, folder)
               : CustomizeProperty.GetPropertyValue(M5.M7.SharePointMetadataAsSource, M5.M7.DefaultValue, M5.M7.SharePointMetadata, list);
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
            AddSignedObjectContentExtensionContextPathElement(ref mObjectContent);
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
            List<FileVEO_M41_Language> M41 = fileVEOXML.M1.M4.M9.M142.M143.M41;
            if (M41 != null)
            {
                if (M41.Count != 0)
                {
                    List<string> mLanguage = new List<string>();
                    foreach (var item in M41)
                    {
                        mLanguage.Add(isFolder ? CustomizeProperty.GetPropertyValue(item.SharePointMetadataAsSource, item.DefaultValue, item.SharePointMetadata, folder)
                            : CustomizeProperty.GetPropertyValue(item.SharePointMetadataAsSource, item.DefaultValue, item.SharePointMetadata, list));
                    }
                    mObjectContent.File.FileMetadata.Language = mLanguage;
                }
            }
        }

        private void AddSignedObjectContentAuxiliaryDescriptionElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            List<FileVEO_M153_AuxiliaryDescription> M153 = fileVEOXML.M1.M4.M9.M142.M143.M153;
            if (M153 != null)
            {
                if (M153.Count != 0)
                {
                    List<string> mAuxiliaryDescription = new List<string>();
                    foreach (var item in M153)
                    {
                        mAuxiliaryDescription.Add(isFolder ? CustomizeProperty.GetPropertyValue(item.SharePointMetadataAsSource, item.DefaultValue, item.SharePointMetadata, folder)
                            : CustomizeProperty.GetPropertyValue(item.SharePointMetadataAsSource, item.DefaultValue, item.SharePointMetadata, list));
                    }
                    mObjectContent.File.FileMetadata.AuxiliaryDescription = mAuxiliaryDescription;
                }
            }
        }

        private void AddSignedObjectContentDescriptionElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            List<FileVEO_M40_Description> M40 = fileVEOXML.M1.M4.M9.M142.M143.M40;
            if (M40 != null)
            {
                if (M40.Count != 0)
                {
                    List<string> mDescription = new List<string>();
                    foreach (var item in M40)
                    {
                        mDescription.Add(isFolder ? CustomizeProperty.GetPropertyValue(item.SharePointMetadataAsSource, item.DefaultValue, item.SharePointMetadata, folder)
                            : CustomizeProperty.GetPropertyValue(item.SharePointMetadataAsSource, item.DefaultValue, item.SharePointMetadata, list));
                    }
                    mObjectContent.File.FileMetadata.Description = mDescription;
                }
            }
        }

        private void AddSignedObjectContentSubjectElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            List<FileVEO_M37_Subject> M37 = fileVEOXML.M1.M4.M9.M142.M143.M37;
            if (M37 != null)
            {
                List<FileVEOClass.Subject> subject = new List<FileVEOClass.Subject>();
                foreach (var temp in M37)
                {
                    var mSubject = new FileVEOClass.Subject();
                    if (temp.M38 != null)
                    {
                        mSubject.KeywordLevel = isFolder ? CustomizeProperty.GetPropertyValue(temp.M38.SharePointMetadataAsSource, temp.M38.DefaultValue, temp.M38.SharePointMetadata, folder)
                    : CustomizeProperty.GetPropertyValue(temp.M38.SharePointMetadataAsSource, temp.M38.DefaultValue, temp.M38.SharePointMetadata, list);
                    }
                    if (temp.M39.Count != 0)
                    {
                        List<string> keyWord = new List<string>();
                        foreach (var M39 in temp.M39)
                        {
                            keyWord.Add(isFolder ? CustomizeProperty.GetPropertyValue(M39.SharePointMetadataAsSource, M39.DefaultValue, M39.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M39.SharePointMetadataAsSource, M39.DefaultValue, M39.SharePointMetadata, list));
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
            FileVEO_M143_FileMetadata M143 = fileVEOXML.M1.M4.M9.M142.M143;
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
                            agent.AgentType.Add(isFolder ? CustomizeProperty.GetPropertyValue(M13.SharePointMetadataAsSource, M13.DefaultValue, M13.SharePointMetadata, folder) : CustomizeProperty.GetPropertyValue(M13.SharePointMetadataAsSource, M13.DefaultValue, M13.SharePointMetadata, list));
                        }
                    }
                    if (M12.M14.Count != 0)
                    {
                        agent.Jurisdiction = new List<string>();
                        foreach (var M14 in M12.M14)
                        {
                            agent.Jurisdiction.Add(isFolder ? CustomizeProperty.GetPropertyValue(M14.SharePointMetadataAsSource, M14.DefaultValue, M14.SharePointMetadata, folder) : CustomizeProperty.GetPropertyValue(M14.SharePointMetadataAsSource, M14.DefaultValue, M14.SharePointMetadata, list));
                        }
                    }
                    if (M12.M15 != null)
                    {
                        agent.CorporateId = isFolder ? CustomizeProperty.GetPropertyValue(M12.M15.SharePointMetadataAsSource, M12.M15.DefaultValue, M12.M15.SharePointMetadata, folder) : CustomizeProperty.GetPropertyValue(M12.M15.SharePointMetadataAsSource, M12.M15.DefaultValue, M12.M15.SharePointMetadata, list);
                    }
                    if (M12.M16.Count != 0)
                    {
                        agent.CorporateName = new List<string>();
                        foreach (var M16 in M12.M16)
                        {
                            agent.CorporateName.Add(isFolder ? CustomizeProperty.GetPropertyValue(M16.SharePointMetadataAsSource, M16.DefaultValue, M16.SharePointMetadata, folder) : CustomizeProperty.GetPropertyValue(M16.SharePointMetadataAsSource, M16.DefaultValue, M16.SharePointMetadata, list));
                        }
                    }
                    if (M12.M17 != null)
                    {
                        agent.PersonId = isFolder ? CustomizeProperty.GetPropertyValue(M12.M17.SharePointMetadataAsSource, M12.M17.DefaultValue, M12.M17.SharePointMetadata, folder) : CustomizeProperty.GetPropertyValue(M12.M17.SharePointMetadataAsSource, M12.M17.DefaultValue, M12.M17.SharePointMetadata, list);
                    }
                    if (M12.M18.Count != 0)
                    {
                        agent.PersonalName = new List<string>();
                        foreach (var M18 in M12.M18)
                        {
                            agent.PersonalName.Add(isFolder ? CustomizeProperty.GetPropertyValue(M18.SharePointMetadataAsSource, M18.DefaultValue, M18.SharePointMetadata, folder) : CustomizeProperty.GetPropertyValue(M18.SharePointMetadataAsSource, M18.DefaultValue, M18.SharePointMetadata, list));
                        }
                    }
                    if (M12.M19.Count != 0)
                    {
                        agent.SectionName = new List<string>();
                        foreach (var M19 in M12.M19)
                        {
                            agent.SectionName.Add(isFolder ? CustomizeProperty.GetPropertyValue(M19.SharePointMetadataAsSource, M19.DefaultValue, M19.SharePointMetadata, folder) : CustomizeProperty.GetPropertyValue(M19.SharePointMetadataAsSource, M19.DefaultValue, M19.SharePointMetadata, list));
                        }
                    }
                    if (M12.M20.Count != 0)
                    {
                        agent.PositionName = new List<string>();
                        foreach (var M20 in M12.M20)
                        {
                            agent.PositionName.Add(isFolder ? CustomizeProperty.GetPropertyValue(M20.SharePointMetadataAsSource, M20.DefaultValue, M20.SharePointMetadata, folder) : CustomizeProperty.GetPropertyValue(M20.SharePointMetadataAsSource, M20.DefaultValue, M20.SharePointMetadata, list));
                        }
                    }
                    if (M12.M21.Count != 0)
                    {
                        agent.ContactDetails = new List<string>();
                        foreach (var M21 in M12.M21)
                        {
                            agent.ContactDetails.Add(isFolder ? CustomizeProperty.GetPropertyValue(M21.SharePointMetadataAsSource, M21.DefaultValue, M21.SharePointMetadata, folder) : CustomizeProperty.GetPropertyValue(M21.SharePointMetadataAsSource, M21.DefaultValue, M21.SharePointMetadata, list));
                        }
                    }
                    if (M12.M22.Count != 0)
                    {
                        agent.Email = new List<string>();
                        foreach (var M22 in M12.M22)
                        {
                            agent.Email.Add(isFolder ? CustomizeProperty.GetPropertyValue(M22.SharePointMetadataAsSource, M22.DefaultValue, M22.SharePointMetadata, folder) : CustomizeProperty.GetPropertyValue(M22.SharePointMetadataAsSource, M22.DefaultValue, M22.SharePointMetadata, list));
                        }
                    }
                    if (M12.M23.Count != 0)
                    {
                        agent.DigitalSignature = new List<string>();
                        foreach (var M23 in M12.M23)
                        {
                            agent.DigitalSignature.Add(isFolder ? CustomizeProperty.GetPropertyValue(M23.SharePointMetadataAsSource, M23.DefaultValue, M23.SharePointMetadata, folder) : CustomizeProperty.GetPropertyValue(M23.SharePointMetadataAsSource, M23.DefaultValue, M23.SharePointMetadata, list));
                        }
                    }
                    mAgent.Add(agent);
                }
                ObjectContent.File.FileMetadata.Agent = mAgent;
            }
        }

        private void AddSignedObjectContentRightsManagementElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            FileVEO_M24_RightsManagement M24 = fileVEOXML.M1.M4.M9.M142.M143.M24;
            if (M24 != null)
            {
                var mRightsManagement = new FileVEOClass.RightsManagement();
                if (M24.M25 != null)
                {
                    mRightsManagement.SecurityClassification = isFolder ? CustomizeProperty.GetPropertyValue(M24.M25.SharePointMetadataAsSource, M24.M25.DefaultValue, M24.M25.SharePointMetadata, folder)
                    : CustomizeProperty.GetPropertyValue(M24.M25.SharePointMetadataAsSource, M24.M25.DefaultValue, M24.M25.SharePointMetadata, list);
                }
                if (M24.M26.Count != 0)
                {
                    mRightsManagement.Caveat = new List<string>();
                    foreach (var M26 in M24.M26)
                    {
                        mRightsManagement.Caveat.Add(isFolder ? CustomizeProperty.GetPropertyValue(M26.SharePointMetadataAsSource, M26.DefaultValue, M26.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M26.SharePointMetadataAsSource, M26.DefaultValue, M26.SharePointMetadata, list));
                    }
                }
                if (M24.M27.Count != 0)
                {
                    mRightsManagement.Codeword = new List<string>();
                    foreach (var M27 in M24.M27)
                    {
                        mRightsManagement.Codeword.Add(isFolder ? CustomizeProperty.GetPropertyValue(M27.SharePointMetadataAsSource, M27.DefaultValue, M27.SharePointMetadata, folder)
                           : CustomizeProperty.GetPropertyValue(M27.SharePointMetadataAsSource, M27.DefaultValue, M27.SharePointMetadata, list));
                    }
                }
                if (M24.M28.Count != 0)
                {
                    mRightsManagement.ReleasabilityIndicator = new List<string>();
                    foreach (var M28 in M24.M28)
                    {
                        mRightsManagement.ReleasabilityIndicator.Add(isFolder ? CustomizeProperty.GetPropertyValue(M28.SharePointMetadataAsSource, M28.DefaultValue, M28.SharePointMetadata, folder)
                              : CustomizeProperty.GetPropertyValue(M28.SharePointMetadataAsSource, M28.DefaultValue, M28.SharePointMetadata, list));
                    }
                }
                if (M24.M29 != null)
                {
                    mRightsManagement.AccessStatus = isFolder ? CustomizeProperty.GetPropertyValue(M24.M29.SharePointMetadataAsSource, M24.M29.DefaultValue, M24.M29.SharePointMetadata, folder)
                    : CustomizeProperty.GetPropertyValue(M24.M29.SharePointMetadataAsSource, M24.M29.DefaultValue, M24.M29.SharePointMetadata, list);
                }
                if (M24.M30.Count != 0)
                {
                    mRightsManagement.UsageCondition = new List<string>();
                    foreach (var M30 in M24.M30)
                    {
                        mRightsManagement.UsageCondition.Add(isFolder ? CustomizeProperty.GetPropertyValue(M30.SharePointMetadataAsSource, M30.DefaultValue, M30.SharePointMetadata, folder)
                              : CustomizeProperty.GetPropertyValue(M30.SharePointMetadataAsSource, M30.DefaultValue, M30.SharePointMetadata, list));
                    }
                }
                if (M24.M31 != null)
                {
                    mRightsManagement.EncryptionDetails = isFolder ? CustomizeProperty.GetPropertyValue(M24.M31.SharePointMetadataAsSource, M24.M31.DefaultValue, M24.M31.SharePointMetadata, folder)
                    : CustomizeProperty.GetPropertyValue(M24.M31.SharePointMetadataAsSource, M24.M31.DefaultValue, M24.M31.SharePointMetadata, list);
                }
                mObjectContent.File.FileMetadata.RightsManagement = mRightsManagement;
            }
        }

        private void AddSignedObjectContentExtensionContextPathElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            var mContextPath = new FileVEOClass.ContextPath();
            mContextPath.ContextPathDomain = "URL";
            if (isFolder)
            {
                mContextPath.ContextPathValue = CustomizeProperty.GetPropertyValue(true, "@EncodingContextPath@", "@EncodingContextPath@", folder);
            }
            else
            {
                mContextPath.ContextPathValue = CustomizeProperty.GetPropertyValue(true, "@EncodingContextPath@", "@EncodingContextPath@", list);
            }
            mObjectContent.File.FileMetadata.ContextPath = mContextPath;
        }

        private void AddSignedObjectContentTitleElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            FileVEO_M32_Title M32 = fileVEOXML.M1.M4.M9.M142.M143.M32;
            if (M32 != null)
            {
                var mTitle = new FileVEOClass.Title();
                if (M32.M33.Count != 0)
                {
                    List<string> schemeTpye = new List<string>();
                    foreach (var M33 in M32.M33)
                    {
                        schemeTpye.Add(isFolder ? CustomizeProperty.GetPropertyValue(M33.SharePointMetadataAsSource, M33.DefaultValue, M33.SharePointMetadata, folder) : CustomizeProperty.GetPropertyValue(M33.SharePointMetadataAsSource, M33.DefaultValue, M33.SharePointMetadata, list));
                    }
                    mTitle.SchemeType = schemeTpye;
                }
                if (M32.M34 != null)
                {
                    mTitle.SchemeName = isFolder ? CustomizeProperty.GetPropertyValue(M32.M34.SharePointMetadataAsSource, M32.M34.DefaultValue, M32.M34.SharePointMetadata, folder)
                    : CustomizeProperty.GetPropertyValue(M32.M34.SharePointMetadataAsSource, M32.M34.DefaultValue, M32.M34.SharePointMetadata, list);
                }
                if (M32.M35 != null)
                {
                    mTitle.TitleWords = isFolder ? CustomizeProperty.GetPropertyValue(M32.M35.SharePointMetadataAsSource, M32.M35.DefaultValue, M32.M35.SharePointMetadata, folder)
                    : CustomizeProperty.GetPropertyValue(M32.M35.SharePointMetadataAsSource, M32.M35.DefaultValue, M32.M35.SharePointMetadata, list);
                }
                if (M32.M36.Count != 0)
                {
                    List<string> alternative = new List<string>();
                    foreach (var M36 in M32.M36)
                    {
                        alternative.Add(isFolder ? CustomizeProperty.GetPropertyValue(M36.SharePointMetadataAsSource, M36.DefaultValue, M36.SharePointMetadata, folder) : CustomizeProperty.GetPropertyValue(M36.SharePointMetadataAsSource, M36.DefaultValue, M36.SharePointMetadata, list));
                    }
                    mTitle.Alternative = alternative;
                }
                mObjectContent.File.FileMetadata.Title = mTitle;
            }
        }

        private void AddSignedObjectContentRelationElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            List<FileVEO_M42_Relation> M42 = fileVEOXML.M1.M4.M9.M142.M143.M42;
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
                            mRelatedItemId.Add(isFolder ? CustomizeProperty.GetPropertyValue(M43.SharePointMetadataAsSource, M43.DefaultValue, M43.SharePointMetadata, folder)
                         : CustomizeProperty.GetPropertyValue(M43.SharePointMetadataAsSource, M43.DefaultValue, M43.SharePointMetadata, list));
                        }
                        relation.RelatedItemId = mRelatedItemId;
                    }
                    if (item.M44.Count != 0)
                    {
                        List<string> mRelationType = new List<string>();
                        foreach (var M44 in item.M44)
                        {
                            mRelationType.Add(isFolder ? CustomizeProperty.GetPropertyValue(M44.SharePointMetadataAsSource, M44.DefaultValue, M44.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M44.SharePointMetadataAsSource, M44.DefaultValue, M44.SharePointMetadata, list));
                        }
                        relation.RelationType = mRelationType;
                    }
                    if (item.M45.Count != 0)
                    {
                        List<string> mRelationDescrition = new List<string>();
                        foreach (var M45 in item.M45)
                        {
                            mRelationDescrition.Add(isFolder ? CustomizeProperty.GetPropertyValue(M45.SharePointMetadataAsSource, M45.DefaultValue, M45.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M45.SharePointMetadataAsSource, M45.DefaultValue, M45.SharePointMetadata, list));
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
            List<FileVEO_M46_Coverage> M46 = fileVEOXML.M1.M4.M9.M142.M143.M46;
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
                            mJurisdiction.Add(isFolder ? CustomizeProperty.GetPropertyValue(M47.SharePointMetadataAsSource, M47.DefaultValue, M47.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M47.SharePointMetadataAsSource, M47.DefaultValue, M47.SharePointMetadata, list));
                        }
                        coverage.Jurisdiction = mJurisdiction;
                    }
                    if (item.M48.Count != 0)
                    {
                        List<string> mPlaceName = new List<string>();
                        foreach (var M48 in item.M48)
                        {
                            mPlaceName.Add(isFolder ? CustomizeProperty.GetPropertyValue(M48.SharePointMetadataAsSource, M48.DefaultValue, M48.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M48.SharePointMetadataAsSource, M48.DefaultValue, M48.SharePointMetadata, list));
                        }
                        coverage.PlaceName = mPlaceName;
                    }
                    if (item.M49.Count != 0)
                    {
                        List<string> mPeriodName = new List<string>();
                        foreach (var M49 in item.M49)
                        {
                            mPeriodName.Add(isFolder ? CustomizeProperty.GetPropertyValue(M49.SharePointMetadataAsSource, M49.DefaultValue, M49.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M49.SharePointMetadataAsSource, M49.DefaultValue, M49.SharePointMetadata, list));
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
            List<FileVEO_M50_Function> M50 = fileVEOXML.M1.M4.M9.M142.M143.M50;
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
                            mFunctionDescriptor.Add(isFolder ? CustomizeProperty.GetPropertyValue(M51.SharePointMetadataAsSource, M51.DefaultValue, M51.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M51.SharePointMetadataAsSource, M51.DefaultValue, M51.SharePointMetadata, list));
                        }
                        function.FunctionDescriptor = mFunctionDescriptor;
                    }
                    if (item.M52.Count != 0)
                    {
                        List<string> mActivityDescriptor = new List<string>();
                        foreach (var M52 in item.M52)
                        {
                            mActivityDescriptor.Add(isFolder ? CustomizeProperty.GetPropertyValue(M52.SharePointMetadataAsSource, M52.DefaultValue, M52.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M52.SharePointMetadataAsSource, M52.DefaultValue, M52.SharePointMetadata, list));
                        }
                        function.ActivityDescriptor = mActivityDescriptor;
                    }
                    if (item.M53.Count != 0)
                    {
                        List<string> mThirdLevelDescriptor = new List<string>();
                        foreach (var M53 in item.M53)
                        {
                            mThirdLevelDescriptor.Add(isFolder ? CustomizeProperty.GetPropertyValue(M53.SharePointMetadataAsSource, M53.DefaultValue, M53.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M53.SharePointMetadataAsSource, M53.DefaultValue, M53.SharePointMetadata, list));
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
            FileVEO_M54_Date M54 = fileVEOXML.M1.M4.M9.M142.M143.M54;
            if (M54 != null)
            {
                var mDate = new FileVEOClass.Date();
                //DateTimeCreated
                if (M54.M55 != null)
                {
                    mDate.DateTimeCreated = isFolder ? CustomizeProperty.GetPropertyValue(M54.M55.SharePointMetadataAsSource, M54.M55.DefaultValue, M54.M55.SharePointMetadata, folder)
                   : CustomizeProperty.GetPropertyValue(M54.M55.SharePointMetadataAsSource, M54.M55.DefaultValue, M54.M55.SharePointMetadata, list);
                }
                if (M54.M56 != null)
                {
                    //DateTimeTransacted
                    mDate.DateTimeTransacted = isFolder ? CustomizeProperty.GetPropertyValue(M54.M56.SharePointMetadataAsSource, M54.M56.DefaultValue, M54.M56.SharePointMetadata, folder)
                            : CustomizeProperty.GetPropertyValue(M54.M56.SharePointMetadataAsSource, M54.M56.DefaultValue, M54.M56.SharePointMetadata, list);
                }
                if (M54.M57 != null)
                {
                    //DateTimeRegistered
                    mDate.DateTimeRegistered = isFolder ? CustomizeProperty.GetPropertyValue(M54.M57.SharePointMetadataAsSource, M54.M57.DefaultValue, M54.M57.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M54.M57.SharePointMetadataAsSource, M54.M57.DefaultValue, M54.M57.SharePointMetadata, list);
                }
                if (M54.M144 != null)
                {
                    //dateTimeClosed
                    mDate.DateTimeClosed = isFolder ? CustomizeProperty.GetPropertyValue(M54.M144.SharePointMetadataAsSource, M54.M144.DefaultValue, M54.M144.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M54.M144.SharePointMetadataAsSource, M54.M144.DefaultValue, M54.M144.SharePointMetadata, list);
                }
                mObjectContent.File.FileMetadata.Date = mDate;
            }
        }

        private void AddSignedObjectContentTypeElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            FileVEO_M58_Type M58 = fileVEOXML.M1.M4.M9.M142.M143.M58;
            if (M58 != null)
            {
                mObjectContent.File.FileMetadata.Type = isFolder ? CustomizeProperty.GetPropertyValue(M58.SharePointMetadataAsSource, M58.DefaultValue, M58.SharePointMetadata, folder)
                    : CustomizeProperty.GetPropertyValue(M58.SharePointMetadataAsSource, M58.DefaultValue, M58.SharePointMetadata, list);
            }
        }

        private void AddSignedObjectContentRecordIdentifierElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            FileVEO_M65_RecordIdentifier M65 = fileVEOXML.M1.M4.M9.M142.M143.M65;
            if (M65 != null)
            {
                mObjectContent.File.FileMetadata.RecordIdentifier = isFolder ? CustomizeProperty.GetPropertyValue(M65.SharePointMetadataAsSource, M65.DefaultValue, M65.SharePointMetadata, folder)
               : CustomizeProperty.GetPropertyValue(M65.SharePointMetadataAsSource, M65.DefaultValue, M65.SharePointMetadata, list);
            }
        }

        private void AddSignedObjectContentUseHistoryElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            FileVEO_M71_UseHistory M71 = fileVEOXML.M1.M4.M9.M142.M143.M71;
            if (M71 != null)
            {
                var mUseHistory = new FileVEOClass.UseHistory();
                mUseHistory.Use = new List<FileVEOClass.Use>();
                foreach (var M72 in M71.M72)
                {
                    var use = new FileVEOClass.Use();
                    if (M72.M73 != null)
                    {
                        use.UseDateTime = isFolder ? CustomizeProperty.GetPropertyValue(M72.M73.SharePointMetadataAsSource, M72.M73.DefaultValue, M72.M73.SharePointMetadata, folder)
                   : CustomizeProperty.GetPropertyValue(M72.M73.SharePointMetadataAsSource, M72.M73.DefaultValue, M72.M73.SharePointMetadata, list);
                    }
                    if (M72.M74 != null)
                    {
                        use.UseType = isFolder ? CustomizeProperty.GetPropertyValue(M72.M74.SharePointMetadataAsSource, M72.M74.DefaultValue, M72.M74.SharePointMetadata, folder)
                   : CustomizeProperty.GetPropertyValue(M72.M74.SharePointMetadataAsSource, M72.M74.DefaultValue, M72.M74.SharePointMetadata, list);
                    }
                    if (M72.M75 != null)
                    {
                        use.UseDescription = isFolder ? CustomizeProperty.GetPropertyValue(M72.M75.SharePointMetadataAsSource, M72.M75.DefaultValue, M72.M75.SharePointMetadata, folder)
                  : CustomizeProperty.GetPropertyValue(M72.M75.SharePointMetadataAsSource, M72.M75.DefaultValue, M72.M75.SharePointMetadata, list);
                    }
                    mUseHistory.Use.Add(use);
                }
                mObjectContent.File.FileMetadata.UseHistory = mUseHistory;
            }
        }

        private void AddSignedObjectContentPreservationHistoryElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            FileVEO_M76_PreservationHistory M76 = fileVEOXML.M1.M4.M9.M142.M143.M76;
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
                            action.ActionDateTime = isFolder ? CustomizeProperty.GetPropertyValue(M77.M78.SharePointMetadataAsSource, M77.M78.DefaultValue, M77.M78.SharePointMetadata, folder)
                      : CustomizeProperty.GetPropertyValue(M77.M78.SharePointMetadataAsSource, M77.M78.DefaultValue, M77.M78.SharePointMetadata, list);
                        }
                        if (M77.M79 != null)
                        {
                            action.ActionType = isFolder ? CustomizeProperty.GetPropertyValue(M77.M79.SharePointMetadataAsSource, M77.M79.DefaultValue, M77.M79.SharePointMetadata, folder)
                       : CustomizeProperty.GetPropertyValue(M77.M79.SharePointMetadataAsSource, M77.M79.DefaultValue, M77.M79.SharePointMetadata, list);
                        }
                        if (M77.M80 != null)
                        {
                            action.ActionDescription = isFolder ? CustomizeProperty.GetPropertyValue(M77.M80.SharePointMetadataAsSource, M77.M80.DefaultValue, M77.M80.SharePointMetadata, folder)
                       : CustomizeProperty.GetPropertyValue(M77.M80.SharePointMetadataAsSource, M77.M80.DefaultValue, M77.M80.SharePointMetadata, list);
                        }
                        mPreservationHistory.Action.Add(action);
                    }
                }
                if (M76.M81 != null)
                {
                    mPreservationHistory.NextAction = isFolder ? CustomizeProperty.GetPropertyValue(M76.M81.SharePointMetadataAsSource, M76.M81.DefaultValue, M76.M81.SharePointMetadata, folder)
                   : CustomizeProperty.GetPropertyValue(M76.M81.SharePointMetadataAsSource, M76.M81.DefaultValue, M76.M81.SharePointMetadata, list);
                }
                if (M76.M82 != null)
                {
                    mPreservationHistory.NextActionDue = isFolder ? CustomizeProperty.GetPropertyValue(M76.M82.SharePointMetadataAsSource, M76.M82.DefaultValue, M76.M82.SharePointMetadata, folder)
                   : CustomizeProperty.GetPropertyValue(M76.M82.SharePointMetadataAsSource, M76.M82.DefaultValue, M76.M82.SharePointMetadata, list);
                }
                mObjectContent.File.FileMetadata.PreservationHistory = mPreservationHistory;
            }
        }

        private void AddSignedObjectContentLocationElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            FileVEO_M83_Location M83 = fileVEOXML.M1.M4.M9.M142.M143.M83;
            if (M83 != null)
            {
                var mLocation = new FileVEOClass.Location();
                if (M83.M84 != null)
                {
                    mLocation.CurrentLocation = isFolder ? CustomizeProperty.GetPropertyValue(M83.M84.SharePointMetadataAsSource, M83.M84.DefaultValue, M83.M84.SharePointMetadata, folder)
                  : CustomizeProperty.GetPropertyValue(M83.M84.SharePointMetadataAsSource, M83.M84.DefaultValue, M83.M84.SharePointMetadata, list);
                }
                if (M83.M85 != null)
                {
                    mLocation.HomeLocationDetails = isFolder ? CustomizeProperty.GetPropertyValue(M83.M85.SharePointMetadataAsSource, M83.M85.DefaultValue, M83.M85.SharePointMetadata, folder)
                   : CustomizeProperty.GetPropertyValue(M83.M85.SharePointMetadataAsSource, M83.M85.DefaultValue, M83.M85.SharePointMetadata, list);
                }
                if (M83.M86 != null)
                {
                    mLocation.HomeStorageDetails = isFolder ? CustomizeProperty.GetPropertyValue(M83.M86.SharePointMetadataAsSource, M83.M86.DefaultValue, M83.M86.SharePointMetadata, folder)
                   : CustomizeProperty.GetPropertyValue(M83.M86.SharePointMetadataAsSource, M83.M86.DefaultValue, M83.M86.SharePointMetadata, list);
                }
                if (M83.M87 != null)
                {
                    mLocation.RKSID = isFolder ? CustomizeProperty.GetPropertyValue(M83.M87.SharePointMetadataAsSource, M83.M87.DefaultValue, M83.M87.SharePointMetadata, folder)
                    : CustomizeProperty.GetPropertyValue(M83.M87.SharePointMetadataAsSource, M83.M87.DefaultValue, M83.M87.SharePointMetadata, list);
                }
                mObjectContent.File.FileMetadata.Location = mLocation;
            }
        }

        private void AddSignedObjectContentFormatElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            FileVEO_M60_Format M60 = fileVEOXML.M1.M4.M9.M142.M143.M60;
            if (M60 != null)
            {
                var mFormat = new FileVEOClass.Format();
                if (M60.M61 != null)
                {
                    mFormat.MediaFormat = isFolder ? CustomizeProperty.GetPropertyValue(M60.M61.SharePointMetadataAsSource, M60.M61.DefaultValue, M60.M61.SharePointMetadata, folder)
                    : CustomizeProperty.GetPropertyValue(M60.M61.SharePointMetadataAsSource, M60.M61.DefaultValue, M60.M61.SharePointMetadata, list);
                }
                if (M60.M62 != null)
                {
                    mFormat.DataFormat = isFolder ? CustomizeProperty.GetPropertyValue(M60.M62.SharePointMetadataAsSource, M60.M62.DefaultValue, M60.M62.SharePointMetadata, folder)
                    : CustomizeProperty.GetPropertyValue(M60.M62.SharePointMetadataAsSource, M60.M62.DefaultValue, M60.M62.SharePointMetadata, list);
                }
                if (M60.M63 != null)
                {
                    mFormat.Medium = isFolder ? CustomizeProperty.GetPropertyValue(M60.M63.SharePointMetadataAsSource, M60.M63.DefaultValue, M60.M63.SharePointMetadata, folder)
                   : CustomizeProperty.GetPropertyValue(M60.M63.SharePointMetadataAsSource, M60.M63.DefaultValue, M60.M63.SharePointMetadata, list);
                }
                if (M60.M64.Count != 0)
                {
                    mFormat.Extent = new List<string>();
                    foreach (var M64 in M60.M64)
                    {
                        mFormat.Extent.Add(isFolder ? CustomizeProperty.GetPropertyValue(M64.SharePointMetadataAsSource, M64.DefaultValue, M64.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M64.SharePointMetadataAsSource, M64.DefaultValue, M64.SharePointMetadata, list));
                    }
                }
                mObjectContent.File.FileMetadata.Format = mFormat;
            }
        }

        private void AddSignedObjectContentDisposalElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            FileVEO_M88_Disposal M88 = fileVEOXML.M1.M4.M9.M142.M143.M88;
            if (M88 != null)
            {
                var mDisposal = new FileVEOClass.Disposal();
                if (M88.M89.Count != 0)
                {
                    List<string> disposalAuthorisation = new List<string>();
                    foreach (var M89 in M88.M89)
                    {
                        disposalAuthorisation.Add(isFolder ? CustomizeProperty.GetPropertyValue(M89.SharePointMetadataAsSource, M89.DefaultValue, M89.SharePointMetadata, folder) : CustomizeProperty.GetPropertyValue(M89.SharePointMetadataAsSource, M89.DefaultValue, M89.SharePointMetadata, list));
                    }
                    mDisposal.DisposalAuthorisation = disposalAuthorisation;
                }
                if (M88.M90 != null)
                {
                    mDisposal.Sentence = isFolder ? CustomizeProperty.GetPropertyValue(M88.M90.SharePointMetadataAsSource, M88.M90.DefaultValue, M88.M90.SharePointMetadata, folder)
                   : CustomizeProperty.GetPropertyValue(M88.M90.SharePointMetadataAsSource, M88.M90.DefaultValue, M88.M90.SharePointMetadata, list);
                }
                if (M88.M91 != null)
                {
                    mDisposal.DisposalActionDue = isFolder ? CustomizeProperty.GetPropertyValue(M88.M91.SharePointMetadataAsSource, M88.M91.DefaultValue, M88.M91.SharePointMetadata, folder)
                    : CustomizeProperty.GetPropertyValue(M88.M91.SharePointMetadataAsSource, M88.M91.DefaultValue, M88.M91.SharePointMetadata, list);
                }
                if (M88.M92 != null)
                {
                    mDisposal.DisposalStatus = isFolder ? CustomizeProperty.GetPropertyValue(M88.M92.SharePointMetadataAsSource, M88.M92.DefaultValue, M88.M92.SharePointMetadata, folder)
                    : CustomizeProperty.GetPropertyValue(M88.M92.SharePointMetadataAsSource, M88.M92.DefaultValue, M88.M92.SharePointMetadata, list);
                }
                mObjectContent.File.FileMetadata.Disposal = mDisposal;
            }
        }

        private void AddSignedObjectContentMandateElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            List<FileVEO_M93_Mandate> M93 = fileVEOXML.M1.M4.M9.M142.M143.M93;
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
                            mMandateType.Add(isFolder ? CustomizeProperty.GetPropertyValue(M94.SharePointMetadataAsSource, M94.DefaultValue, M94.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M94.SharePointMetadataAsSource, M94.DefaultValue, M94.SharePointMetadata, list));
                        }
                        mandate.MandateType = mMandateType;
                    }
                    if (item.M95.Count != 0)
                    {
                        List<string> mRefersto = new List<string>();
                        foreach (var M95 in item.M95)
                        {
                            mRefersto.Add(isFolder ? CustomizeProperty.GetPropertyValue(M95.SharePointMetadataAsSource, M95.DefaultValue, M95.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M95.SharePointMetadataAsSource, M95.DefaultValue, M95.SharePointMetadata, list));
                        }
                        mandate.RefersTo = mRefersto;
                    }
                    if (item.M96.Count != 0)
                    {
                        List<string> mMandateName = new List<string>();
                        foreach (var M96 in item.M96)
                        {
                            mMandateName.Add(isFolder ? CustomizeProperty.GetPropertyValue(M96.SharePointMetadataAsSource, M96.DefaultValue, M96.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M96.SharePointMetadataAsSource, M96.DefaultValue, M96.SharePointMetadata, list));
                        }
                        mandate.MandateName = mMandateName;
                    }
                    if (item.M97.Count != 0)
                    {
                        List<string> mMandateReference = new List<string>();
                        foreach (var M97 in item.M97)
                        {
                            mMandateReference.Add(isFolder ? CustomizeProperty.GetPropertyValue(M97.SharePointMetadataAsSource, M97.DefaultValue, M97.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M97.SharePointMetadataAsSource, M97.DefaultValue, M97.SharePointMetadata, list));
                        }
                        mandate.MandateReference = mMandateReference;
                    }
                    if (item.M98.Count != 0)
                    {
                        List<string> mRequirement = new List<string>();
                        foreach (var M98 in item.M98)
                        {
                            mRequirement.Add(isFolder ? CustomizeProperty.GetPropertyValue(M98.SharePointMetadataAsSource, M98.DefaultValue, M98.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M98.SharePointMetadataAsSource, M98.DefaultValue, M98.SharePointMetadata, list));
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
            FileVEO_M66_ManagementHistory M66 = fileVEOXML.M1.M4.M9.M142.M143.M66;
            if (M66 != null)
            {
                mObjectContent.File.FileMetadata.ManagementHistory = new FileVEOClass.ManagementHistory();
                List<FileVEOClass.ManagementEvent> management = new List<FileVEOClass.ManagementEvent>();
                foreach (var M67 in M66.M67)
                {
                    var mManagementEvent1 = new FileVEOClass.ManagementEvent();
                    if (M67.M68 != null)
                    {
                        mManagementEvent1.EventDateTime = isFolder ? CustomizeProperty.GetPropertyValue(M67.M68.SharePointMetadataAsSource, M67.M68.DefaultValue, M67.M68.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M67.M68.SharePointMetadataAsSource, M67.M68.DefaultValue, M67.M68.SharePointMetadata, list);
                    }
                    if (M67.M69 != null)
                    {
                        mManagementEvent1.EventType = isFolder ? CustomizeProperty.GetPropertyValue(M67.M69.SharePointMetadataAsSource, M67.M69.DefaultValue, M67.M69.SharePointMetadata, folder)
                       : CustomizeProperty.GetPropertyValue(M67.M69.SharePointMetadataAsSource, M67.M69.DefaultValue, M67.M69.SharePointMetadata, list);
                    }
                    if (M67.M70 != null)
                    {
                        mManagementEvent1.EventDescription = isFolder ? CustomizeProperty.GetPropertyValue(M67.M70.SharePointMetadataAsSource, M67.M70.DefaultValue, M67.M70.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M67.M70.SharePointMetadataAsSource, M67.M70.DefaultValue, M67.M70.SharePointMetadata, list);
                    }
                    management.Add(mManagementEvent1);
                }
                mObjectContent.File.FileMetadata.ManagementHistory.ManagementEvent = management;
            }
        }

        private void AddSignedObjectContentVEOIdentifierElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            FileVEO_M99_VEOIdentifier M99 = fileVEOXML.M1.M4.M9.M142.M143.M99;
            if (M99 != null)
            {
                var mVEOIdentifier = new FileVEOClass.VEOIdentifier();
                if (M99.M102 != null)
                {
                    mVEOIdentifier.FileIdentifier = new FileVEOClass.Text();
                    mVEOIdentifier.FileIdentifier.Text1 = M99.M102.SharePointMetadataAsSource? isFolder ? folder.Id.ToString() : list.Id.ToString(): M99.M102.DefaultValue;
                    //mVEOIdentifier.FileIdentifier.Text1 = fileVEOParameters.VFileID;
                }
                if (M99.M100 != null)
                {
                    mVEOIdentifier.AgencyIdentifier = new FileVEOClass.Text();
                    mVEOIdentifier.AgencyIdentifier.Text1 = isFolder ? CustomizeProperty.GetPropertyValue(M99.M100.SharePointMetadataAsSource, M99.M100.DefaultValue, M99.M100.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M99.M100.SharePointMetadataAsSource, M99.M100.DefaultValue, M99.M100.SharePointMetadata, list);
                }
                if (M99.M101 != null)
                {
                    mVEOIdentifier.SeriesIdentifier = new FileVEOClass.Text();
                    mVEOIdentifier.SeriesIdentifier.Text1 = isFolder ? CustomizeProperty.GetPropertyValue(M99.M101.SharePointMetadataAsSource, M99.M101.DefaultValue, M99.M101.SharePointMetadata, folder)
                        : CustomizeProperty.GetPropertyValue(M99.M101.SharePointMetadataAsSource, M99.M101.DefaultValue, M99.M101.SharePointMetadata, list);
                }
                mObjectContent.File.FileMetadata.VEOIdentifier = mVEOIdentifier;
            }
        }

        private void AddSignedObjectContentFileDisposalElement(ref FileVEOClass.ObjectContent mObjectContent)
        {
            FileVEO_M145_FileDisposal M145 = fileVEOXML.M1.M4.M9.M142.M145;
            if (M145 != null)
            {
                var mFileDisposal = new FileVEOClass.FileDisposal();
                if (M145.M146 != null)
                {
                    mFileDisposal.DisposalSchedule = isFolder ? CustomizeProperty.GetPropertyValue(M145.M146.SharePointMetadataAsSource, M145.M146.DefaultValue, M145.M146.SharePointMetadata, folder)
                    : CustomizeProperty.GetPropertyValue(M145.M146.SharePointMetadataAsSource, M145.M146.DefaultValue, M145.M146.SharePointMetadata, list);
                }
                if (M145.M147 != null)
                {
                    mFileDisposal.DisposalDate = isFolder ? CustomizeProperty.GetPropertyValue(M145.M147.SharePointMetadataAsSource, M145.M147.DefaultValue, M145.M147.SharePointMetadata, folder)
                    : CustomizeProperty.GetPropertyValue(M145.M147.SharePointMetadataAsSource, M145.M147.DefaultValue, M145.M147.SharePointMetadata, list);
                }
                if (M145.M148 != null)
                {
                    mFileDisposal.AuthorizingOfficer = isFolder ? CustomizeProperty.GetPropertyValue(M145.M148.SharePointMetadataAsSource, M145.M148.DefaultValue, M145.M148.SharePointMetadata, folder)
                    : CustomizeProperty.GetPropertyValue(M145.M148.SharePointMetadataAsSource, M145.M148.DefaultValue, M145.M148.SharePointMetadata, list);
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
