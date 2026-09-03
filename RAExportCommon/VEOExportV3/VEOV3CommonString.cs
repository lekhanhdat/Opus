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
namespace RAExportCommon
{
    public static class VEOV3CommonString
    {
        public const string SIGNER = "AvePoint";
        public const string ALGORITHMID_SHA512WITHRSA = "SHA512withRSA";
        public const string ALGORITHM_SHA512 = "SHA-512";
        public const string VEO_VERSION = "3.0";
        public const string FORMAT_DATETIME_V3 = "yyyy-MM-dd'T'HH:mm:sszzz"; //IOS-8601 standard
        public const string OBJECTTYPE_RECORD = "Record";
        public const string OBJECTTYPE_FILE = "File";
        public const string OBJECTTYPE_ITEM = "Item";
        public const string NO_DEPTH_STRUCTURE = "0";
        public const string RESOURCE_PARSETYPE = "Resource";
        public const string VEO_V3_FOLDERPOSTFIX = ".veo";
        public const string VEO_V3_BASECONTENTFOLDER = "Documents";
        public const string INBOX = "Inbox";
        public const string NO_DISPOSAL_COVERAGE = "No Disposal Coverage";

        #region Mandatory files
        public const string VEOV3TemplateZipFile = "VEO V3 Configuration Files.zip";
        public const string VEOContent = "VEOContent.xml";
        public const string VEOHistory = "VEOHistory.xml";
        public const string VEOContentSignature = "VEOContentSignature.xml";
        public const string VEOHistorySignature = "VEOHistorySignature.xml";
        public const string EXOVEOContent = "EXOVEOContent.xml";
        public const string EXOVEOContentSignature = "EXOVEOContentSignature.xml";
        public const string EXOVEOHistory = "EXOVEOHistory.xml";
        public const string EXOVEOHistorySignature = "EXOVEOHistorySignature.xml";
        public const string VEOReadme = "VEOReadme.txt";
        #endregion

        public const string VEO_README_CONTENT =
@"This zip file is a VERS Encapsulated Object (VEO). VEO files are
specified in Public Record Office Victoria (PROV) Standard
PROS 15/03 “Long term management of Electronic Rcords”. As at 2015 this
Standard was available from http://prov.vic.gov.au/government/vers

This VEOReadme.txt file contains a summary of the information in
PROS 15/03.

This zip file contains a collection of information that it is desired to
keep accessible for a long period. The content of this information is
contained in subdirectories of this directory. The metadata that
organises and describes the information is contained in the
VEOContent.xml file.

The VEOContent.xml file contains one or more Information Objects – each
of which is a logical collection of information. Information Objects
may contain one or more Information Pieces. An Information Piece
represents a piece of information contained in this VEO. An Information
Piece includes references to at least one physical file (a Content File)
that actually represent the content. If more than one Content File is
present, this represents the same information expressed using different
software formats.

The XML elements in the VEOContent file are:
*	VEOVersion. The version of this Standard. This should be ‘3.0’.
*	HashFunctionAlgorithm. The hash function used to calculate the
	hash values stored in HashValue (see below)
*	InformationObject. A logical collection of information in this
	VEO.
*	InformationObjectType. A text label describing the purpose of
	this Information Object.
*	InformationObjectDepth. If the VEO contains more than one
	Information Object, the Information Objects may be organised in
	a tree. The sequence of Information Objects in the
	VEOContent.xml file will form a depth first traversal of this
	tree, and InformationObjectDepth is the depth of this
	particular Information Object in the tree.
*	MetadataPackage. This is a collection of metadata describing
	the Information Object. An Information Object may have multiple
	Metadata Packages.
*	MetadataSchemaIdentifier. This is used to identify the type of
	this Metadata Package.
*	MetadataSyntaxIdentifier. This is used to identify the way of
	representing the metadata package in XML. We encourage the use
	of RDF.
*	InformationPiece. This is a piece of information content within
	the Information Object.
*	Label. This is a text string that labels the Information Piece.
*	ContentFile. This represents a specific file in the VEO that
	represents the content of the InformationPiece.
*	ContentFilePathName. This is the file name of the file that
	contains the content. It is relative to this directory.
*	HashValue. The hash value resulting from applying the specified
	hash function (see HashFunctionAlgorithm above) to the sequence
	of octets forming the file.

The VEOContentSignature?.xml (where ‘?’ is a number) files each contain
a digital signature that allows detection of corruption of the
VEOContent.xml file (and because the VEOContent.xml file includes hash
values of the content files, corruption of the content files as well).
The signature is generated by reading the VEOContent.xml file as a
sequence of octets and applying specified signature algorithm. The
contents of a VEOContentSignaturen.xml file are:
*	SignatureAlgorithm. This identifies the hash algorithm and the
	digital signature algorithm used to generate the signature.
*	SignatureDateTime. The date and time the signature was applied.
	Expressed in ISO8601.
*	Signer. A text string naming the person or organisation that
	created the digital signature
*	Signature. The resulting signature, encoded as Base64.
*	CertificateValue. An X.509 DER encoded certificate encoded as
Base64. The first certificate contains the public key used to validate
the signature. The second certificate contains the public key used to
validate the first certificate (and so on). The last certificate must
be self signed.

The VEOHistory.xml file contains a summary of events in the life of
this VEO. The elements in this file are:
*	VEOVersion. The version of this Standard. This should be ‘3.0’.
*	Event. A collection of information about an event in the life
	of this VEO.
*	EventDateTime. The date and time the event occurred. Expressed
	in ISO8601.
*	EventType. The a text string labelling the type of the event.
*	Initiator. The person or organisation that authorised or
	initiated this event.
*	Description. A text description of the event.
*	Errors. Any error resulting from this event.

The VEOHistorySignaturen.xml (where ‘n’ is a number) files each contain
a digital signature that allows detection of corruption of the
VEOHistory.xml file. The contents and method of generation of a
VEOHistorySignature file is identical to a VEOContentSignature file.";
    }
}
