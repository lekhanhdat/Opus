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




namespace AvePoint.GCommon.Contract.AveModuleContract
{
    internal class AgentTypes
    {
        internal const string AGENT_TYPE_SP_VERSION_UNKNOW = "0";
        internal const string AGENT_TYPE_SP2003 = "1";
        internal const string AGENT_TYPE_SP2007 = "2";
        internal const string AGENT_TYPE_SP2010 = "4";
        internal const string AGENT_TYPE_BPOS = "8";

        internal const string AGENT_TYPE_SPDR                               = "1";          //1L
        internal const string AGENT_TYPE_SQL                                = "10";          //2L
        internal const string AGENT_TYPE_TOPOLOGY                           = "100";          //4L
        internal const string AGENT_TYPE_ITEM_LEVEL                         = "1000";          //8L
        internal const string AGENT_TYPE_SITE_LEVEL                         = "10000";          //16L
        internal const string AGENT_TYPE_SUBSITE_LEVEL                      = "100000";          //32L
        internal const string AGENT_TYPE_ARCHIVER                           = "1000000";          //64L
        internal const string AGENT_TYPE_AUDITOR                            = "10000000";          //128L
        internal const string AGENT_TYPE_TRASH_BIN                          = "100000000";          //256L
        internal const string AGENT_TYPE_SP2007_ITEM_LEVEL                  = "1000000000";          //512L
        internal const string AGENT_TYPE_SP2007_SITE_LEVEL                  = "10000000000";          //1024L
        internal const string AGENT_TYPE_SP2007_SUBSITE_LEVEL               = "100000000000";          //2048L
        internal const string AGENT_TYPE_SP2007_ARCHIVER                    = "1000000000000";          //4096L
        internal const string AGENT_TYPE_SP2007_TOPOLOGY                    = "10000000000000";          //8192L
        internal const string AGENT_TYPE_MIGRATION_PFTO2003                 = "100000000000000";          //16384L
        internal const string AGENT_TYPE_MIGRATION_PFTO2007                 = "1000000000000000";          //32768L
        internal const string AGENT_TYPE_MIGRATION_SPTOMOSS                 = "10000000000000000";          //65536L
        internal const string AGENT_TYPE_PR_CONTROL                         = "100000000000000000";          //131072L
        internal const string AGENT_TYPE_PR_MEMBER                          = "1000000000000000000";          //262144L
        internal const string AGENT_TYPE_FRONTEND_DEPLOMENT                 = "10000000000000000000";          //524288L
        internal const string AGENT_TYPE_SOLUTION_CENTER                    = "100000000000000000000";          //1048576L
        internal const string AGENT_TYPE_COMPLIANCE_REPORTS                 = "1000000000000000000000";          //2097152L
        internal const string AGENT_TYPE_MIGRATION_DIRECT_SP2003            = "10000000000000000000000";          //4194304L
        internal const string AGENT_TYPE_MIGRATION_DIRECT_MOSS2007          = "100000000000000000000000";          //8388608L
        internal const string AGENT_TYPE_HIGH_AVAILABILITY_SYNC2007         = "1000000000000000000000000";          //16777216L
        internal const string AGENT_TYPE_HIGH_AVAILABILITY_SYNC2010         = "1000000000000000000000000";          //16777216L
        internal const string AGENT_TYPE_HIGH_AVAILABILITY_SQL2007          = "10000000000000000000000000";          //33554432L    
        internal const string AGENT_TYPE_MIGRATION_FILE                     = "100000000000000000000000000";          //67108864L
        internal const string AGENT_TYPE_SMS                                = "1000000000000000000000000000";          //134217728L
        internal const string AGENT_TYPE_REPLICATOR                         = "10000000000000000000000000000";          //268435456L
        internal const string AGENT_TYPE_SP2007_COMPLIANCE_ARCHIVE          = "100000000000000000000000000000";          //536870912L
        internal const string AGENT_TYPE_AUDITOR2007                        = "1000000000000000000000000000000";          //1073741824L
        internal const string AGENT_TYPE_MIGRATION_EROOM_SRC                = "10000000000000000000000000000000";          //2147483648L
        internal const string AGENT_TYPE_MIGRATION_EROOM_DEST               = "100000000000000000000000000000000";          //4294967296L
        internal const string AGENT_TYPE_SMS_DISCOVERY                      = "1000000000000000000000000000000000";          //8589934592L
        internal const string AGENT_TYPE_CONTENT_MANAGER2010                = "10000000000000000000000000000000000";          //17179869184L
        internal const string AGENT_TYPE_MIGRATION_NOTES_SRC                = "100000000000000000000000000000000000";          //34359738368L
        internal const string AGENT_TYPE_MIGRATION_NOTES_DEST               = "1000000000000000000000000000000000000";          //68719476736L
        internal const string AGENT_TYPE_DEPLOYMENT_SITE_LEVEL              = "10000000000000000000000000000000000000";          //137438953472L
        internal const string AGENT_TYPE_BIMODE_AUDIT                       = "100000000000000000000000000000000000000";          //274877906944L
        internal const string AGENT_TYPE_USER_CLUSTERING                    = "1000000000000000000000000000000000000000";          //549755813888L
        internal const string AGENT_TYPE_REPORT_CENTER                      = "10000000000000000000000000000000000000000";          //1099511627776L
        internal const string AGENT_TYPE_EDISCOVERY                         = "100000000000000000000000000000000000000000";          //2199023255552L
        internal const string AGENT_TYPE_MIGRATION_LIVELINK_SRC             = "1000000000000000000000000000000000000000000";          //4398046511104L
        internal const string AGENT_TYPE_MIGRATION_LIVELINK_DEST            = "10000000000000000000000000000000000000000000";          //8796093022208L
        internal const string AGENT_TYPE_SITE_BIN                           = "100000000000000000000000000000000000000000000";          //17592186044416L    
        internal const string AGENT_TYPE_CONNECTOR                          = "1000000000000000000000000000000000000000000000";          //35184372088832L
        internal const string AGENT_TYPE_REAL_TIME_ARCHIVE                  = "10000000000000000000000000000000000000000000000";          //70368744177664L
        internal const string AGENT_TYPE_CONNECTOR_VIDEO                    = "100000000000000000000000000000000000000000000000";          //140737488355328L
        internal const string AGENT_TYPE_INTEGRATE_WITH_SP_SEARCH           = "1000000000000000000000000000000000000000000000000";          //281474976710656L
        internal const string AGENT_TYPE_MIGRATION_EMC_SRC                  = "10000000000000000000000000000000000000000000000000";          //562949953421312L
        internal const string AGENT_TYPE_MIGRATION_EMC_DEST                 = "100000000000000000000000000000000000000000000000000";          //112589990684262L
        internal const string AGENT_TYPE_MIGRATION_07_10                    = "1000000000000000000000000000000000000000000000000000";          //225179981368525L
        internal const string AGENT_TYPE_CONTENT_MANAGER_OFFICE365          = "10000000000000000000000000000000000000000000000000000";          //4503599627370496L;
        internal const string AGENT_TYPE_GOVERNANCE_AUTOMATION              = "100000000000000000000000000000000000000000000000000000";          //9007199254740992L;
        internal const string AGENT_TYPE_COMPLIANCE_VAULT                   = "1000000000000000000000000000000000000000000000000000000";          //18014398509481984L;
        internal const string AGENT_TYPE_MIGRATION_EPF_SRC                  = "10000000000000000000000000000000000000000000000000000000";          //36028797018963968L;
    }
}
