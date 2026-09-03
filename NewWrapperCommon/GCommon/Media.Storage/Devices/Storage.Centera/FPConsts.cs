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



using System.Diagnostics.CodeAnalysis;
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Centera.FPStreamPerm.#.cctor()", MessageId = "rb")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Centera.FPStreamPerm.#.cctor()", MessageId = "wb")]
namespace AvePoint.Media.Storage.Centera
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.Media.Storage.Util;
    using System.Globalization;
    #endregion

    class FPStreamPerm
    {
        string perm;

        private FPStreamPerm(string perm)
        {
            this.perm = perm;
        }

        public override string ToString()
        {
            return perm;
        }

        public static readonly FPStreamPerm Overwrite = new FPStreamPerm("wb");
        public static readonly FPStreamPerm Append = new FPStreamPerm("ab");
        public static readonly FPStreamPerm NonNewWrite = new FPStreamPerm("rb+");
        public static readonly FPStreamPerm ReadWrite = new FPStreamPerm("wb+");
        public static readonly FPStreamPerm AppendOrCreate = new FPStreamPerm("ab+");
    }

    class FPOption
    {
        string name;

        private FPOption(string name)
        {
            this.name = name.ToLower(CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            return this.name;
        }
        public static readonly FPOption FP_WRITE = new FPOption("read");
        public static readonly FPOption FP_READ = new FPOption("write");
        public static readonly FPOption FP_DELETE = new FPOption("delete");
        public static readonly FPOption FP_ALLOWED = new FPOption("allowed");
        public static readonly FPOption BUFFERSIZE = new FPOption("bufferSize");
        public static readonly FPOption TIMEOUT = new FPOption("timeout");
        public static readonly FPOption RETRYCOUNT = new FPOption("retryCount");
        public static readonly FPOption RETRYSLEEP = new FPOption("retrySleep");
        public static readonly FPOption MAXCONNECTIONS = new FPOption("MaxConnections");
        public static readonly FPOption MULTICLUSTERFAILOVER = new FPOption("multiClusterFailover");
        public static readonly FPOption COLLISIONAVOIDANCE = new FPOption("collisionAvoidance");
        public static readonly FPOption PREFETCHSIZE = new FPOption("prefetchSize");
        public static readonly FPOption CLUSTERNONAVAILTIME = new FPOption("CLUSTERNONAVAILTIME");
        public static readonly FPOption PROBETIMELIMIT = new FPOption("probeTimeLimit");
        public static readonly FPOption EMBEDDING_THRESHOLD = new FPOption("embedding_threshold");
        public static readonly FPOption OPENSTRATEGY = new FPOption("openStrategy");
        public static readonly FPOption MULTICLUSTER_READ_STRATEGY = new FPOption("multiCluster_read_strategy");
        public static readonly FPOption MULTICLUSTER_WRITE_STRATEGY = new FPOption("multiCluster_write_strategy");
        public static readonly FPOption MULTICLUSTER_DELETE_STRATEGY = new FPOption("multiCluster_delete_strategy");
        public static readonly FPOption MULTICLUSTER_EXISTS_STRATEGY = new FPOption("multiCluster_exists_strategy");
        public static readonly FPOption MULTICLUSTER_QUERY_STRATEGY = new FPOption("multiCluster_query_strategy");
        public static readonly FPOption MULTICLUSTER_READ_CLUSTERS = new FPOption("multiCluster_read_clusters");
        public static readonly FPOption MULTICLUSTER_WRITE_CLUSTERS = new FPOption("multiCluster_write_clusters");
        public static readonly FPOption MULTICLUSTER_DELETE_CLUSTERS = new FPOption("multiCluster_delete_clusters");
        public static readonly FPOption MULTICLUSTER_EXISTS_CLUSTERS = new FPOption("multiCluster_exists_clusters");
        public static readonly FPOption MULTICLUSTER_QUERY_CLUSTERS = new FPOption("multiCluster_query_clusters");
    }

    class FPBlobOption
    {
        long option;
        public long Value
        {
            get { return option; }
        }
        private FPBlobOption(long option)
        {
            this.option = option;
        }

        public static readonly FPBlobOption FP_OPTION_DEFAULT_OPTIONS = new FPBlobOption(0);                       /**< select default options for that parameter                */
        public static readonly FPBlobOption FP_OPTION_CALCID_MASK = new FPBlobOption(0x0000000F);
        public static readonly FPBlobOption FP_OPTION_CLIENT_CALCID = new FPBlobOption(0x00000001);              /**< calculate the digest upfront                             */
        public static readonly FPBlobOption FP_OPTION_CLIENT_CALCID_STREAMING = new FPBlobOption(0x00000002);              /**< calculate the digest by the client while streaming       */
        public static readonly FPBlobOption FP_OPTION_SERVER_CALCID_STREAMING = new FPBlobOption(0x00000003);              /**< calculate the digest by the server                       */

        public static readonly FPBlobOption FP_OPTION_CALCID_NOCHECK = new FPBlobOption(0x00000010);              /**< don't check md5 along the way (deprecated)               */

        public static readonly FPBlobOption FP_OPTION_ENABLE_DUPLICATE_DETECTION = new FPBlobOption(0x00000020);              /**< enable detection of duplicate content addresses          */
        public static readonly FPBlobOption FP_OPTION_ENABLE_COLLISION_AVOIDANCE = new FPBlobOption(0x00000040);              /**< enable collision avoidance (override FPPool - Setting)   */
        public static readonly FPBlobOption FP_OPTION_DISABLE_COLLISION_AVOIDANCE = new FPBlobOption(0x00000080);

        public static readonly FPBlobOption FP_OPTION_EMBED_DATA = new FPBlobOption(0x00000100);
        public static readonly FPBlobOption FP_OPTION_LINK_DATA = new FPBlobOption(0x00000200);
    }

    class FPTagCopyOption
    {
        int option;
        public long Value
        {
            get { return option; }
        }

        private FPTagCopyOption(int option)
        {
            this.option = option;
        }

        public static readonly FPTagCopyOption FP_OPTION_NO_COPY_OPTIONS = new FPTagCopyOption(0x00);                    /**< just copy the tag with its attributes                     */
        public static readonly FPTagCopyOption FP_OPTION_COPY_BLOBDATA = new FPTagCopyOption(0x01);                    /**< copy the blobdata (if any) with the tag                   */
        public static readonly FPTagCopyOption FP_OPTION_COPY_CHILDREN = new FPTagCopyOption(0x02);

    }

    enum FPClipOpenMode : int
    {
        FP_OPEN_ASTREE = 1,
        FP_OPEN_FLAT = 2
    }

    class CenteraConst
    {
        public const string CENTERA_RESOURCES_ROOT = @"storage\centera";
        public const string CENTERA_API_FILE_FULLNAME = CENTERA_RESOURCES_ROOT + @"\api\FPLibrary.dll";
        public const string CENTERA_RESOURCES_CONFIG = @"storage\centera\configs";
        public const string PREVIOUS_CLIP_ID = "PREVIOUS_CLIP_ID";
    }
    static class EMCErrorInformation
    {
        public static Dictionary<int, string> errorDictionary = null;
        public static Dictionary<int, string> systemErrorDictionary = null;
        public static string GetInformation(int errorNumber, int systemErrNumber)
        {
            if (errorDictionary == null)
            {
                Init();
            }
            if (systemErrorDictionary.ContainsKey(systemErrNumber))
            {
                string errorInformation = string.Empty;
                try
                {
                    errorInformation = systemErrorDictionary[systemErrNumber] + "   " + errorDictionary[errorNumber];
                }
                catch (Exception e)
                {
                    errorInformation = "error code :"+errorNumber+"    unKnow error" + e.Message;
                }
                return errorInformation;
            }
            return errorDictionary[errorNumber];
        }
        public static void Init()
        {
            errorDictionary = new Dictionary<int, string>();
            systemErrorDictionary = new Dictionary<int, string>();
            errorDictionary.Add(-10001, "The name is not XML compliant");
            errorDictionary.Add(-10002, "Unknown option name with FP_SetIntOption/GetIntOption");
            errorDictionary.Add(-10003, "An error occurred when you sent a request to the server.This internal error was generated because the server could not accept the request packet. Verify all LAN connections and try again");
            errorDictionary.Add(-10004, "No reply was received from the server.This internal error was generated because the server did not send a reply to the request packet. Verify all LAN connections and try again");
            errorDictionary.Add(-10005, "The server reported an error from the operation");
            errorDictionary.Add(-10006, "Incorrect or unknown parameter detected");
            errorDictionary.Add(-10007, "Path does not correspond to a file/directory on the local system");
            errorDictionary.Add(-10008, "");//SDK 2.0: no longer used
            errorDictionary.Add(-10009, "");//SDK 2.0: no longer used
            errorDictionary.Add(-10010, "Duplicate blob found on server");
            errorDictionary.Add(-10011, "");//SDK 2.0: no longer used
            errorDictionary.Add(-10012, "Operation not (yet) supported please check your setting");
            errorDictionary.Add(-10013, "A write acknowledgement not received.Verify your LAN connections and try again.");
            errorDictionary.Add(-10014, "Blob could not be stored on write or could not be found on server.Verify that the original data was correctly stored, verify your LAN connections and try again");
            errorDictionary.Add(-10015, "");
            errorDictionary.Add(-10016, "");
            errorDictionary.Add(-10017, "Tag in C-Clip description file not found");
            errorDictionary.Add(-10018, "Attribute with that name not found");
            errorDictionary.Add(-10019, "You have used an invalid reference");
            errorDictionary.Add(-10020, "No connection with any pool.Verify your LAN connections and server settings, and try again.");
            errorDictionary.Add(-10021, "ClipFile (CDF) is not found in the pool");
            errorDictionary.Add(-10022, "An error in the tag tree was discovered");
            errorDictionary.Add(-10023, "We expect a path to a directory, not to a file.Verify the path to the data and try again");
            errorDictionary.Add(-10024, "We expected either a 'file' or 'folder' tag");
            errorDictionary.Add(-10025, "The tag cannot be changed or deleted.(it is probably a top tag");
            errorDictionary.Add(-10026, "The options parameter is out of bounds");
            errorDictionary.Add(-10027, "A file system error occurred .Maybe an incorrect path was given, or you are trying to open an unknown file or a file in the wrong mode. Verify the path and try again.");
            //errorDictionary.Add(-10028,"Attribute with that name not found");
            errorDictionary.Add(-10029, "Maximum depth of enclosing tags is reached");
            errorDictionary.Add(-10030, "The tag does not contain blob data.");
            errorDictionary.Add(-10031, "Mismatch between C-Clip version and current software version");
            errorDictionary.Add(-10032, "The tag has already data associated with it");
            errorDictionary.Add(-10033, "You have used an unknown protocol option (Only HPP is supported).");
            errorDictionary.Add(-10034, "No more socket is supported for the transaction please change your setting to increase the number of sockets");
            errorDictionary.Add(-10035, "");
            errorDictionary.Add(-10036, "BlobID mismatch between client & server, blob is corrupt");
            errorDictionary.Add(-10037, "");
            //errorDictionary.Add(-10038,"Attribute with that name not found");
            // errorDictionary.Add(-10039,"Maximum depth of enclosing tags is reached");
            errorDictionary.Add(-10040, "The blob on the cluster is busy and cannot be read from or written to.");
            errorDictionary.Add(-10041, "The server is not yet ready to process your request");
            errorDictionary.Add(-10042, "The server has no capacity to store data. Enlarge the server's capacity and try again");
            errorDictionary.Add(-10043, "App passed in a sequence ID that was used before.");
            errorDictionary.Add(-10044, "Detected a generic stream validation error");
            errorDictionary.Add(-10045, "Detected a generic stream byte count mismatch");
            errorDictionary.Add(-10101, "An error on the network socket occurred. Verify the network.");
            errorDictionary.Add(-10102, "The data packet contains wrong data. Verify the network,the version of the server or try again later");
            errorDictionary.Add(-10103, "No node with the access role can be found. Verify the IP addresses provided.");
            errorDictionary.Add(-10151, "BlobID mismatch between client & server, blob is corrupt");
            errorDictionary.Add(-10152, "The packet field is missing.");

            errorDictionary.Add(-10153, "Authentication to get access to the server failed. Check the profile name and secret or pea file.");
            errorDictionary.Add(-10154, "An unknown authentication scheme has been used.");
            errorDictionary.Add(-10155, "An unknown authentication protocol has been used.");
            errorDictionary.Add(-10156, "Transaction on the server failed.FPClip_Delete() or FPClip_AuditedDelete() could not delete the complete C-Clip because of server problems. Try again later.");
            errorDictionary.Add(-10157, "No profile clip was found.");
            errorDictionary.Add(-10201, "The application requires marker support but the stream does not provide that.");
            // errorDictionary.Add(-10202,"Unknown option name with FP_SetIntOption/GetIntOption");
            errorDictionary.Add(-10203, "The function expects an input stream and gets an output stream or vice-versa.");
            errorDictionary.Add(-10204, "The use of this operation is restricted or this operation is not allowed because the server capability is false.");
            errorDictionary.Add(-10205, "An SDK internal programming error has been detected.");
            errorDictionary.Add(-10206, "The system ran out of memory. Check the system's capacity.");
            errorDictionary.Add(-10207, "Cannot close the object because it is in use.");
            errorDictionary.Add(-10208, "The object is not yet opened.");
            errorDictionary.Add(-10209, "An error occurred in the generic stream.");
            // errorDictionary.Add(-10210,"Duplicate blob found on server");
            errorDictionary.Add(-10211, "An error occurred while creating a background thread.");
            errorDictionary.Add(-10212, "The probe limit time was reached.");
            errorDictionary.Add(-10213, "There was an error while storing the profile clip ID.");
            errorDictionary.Add(-10214, "The specified string is not valid XML.");
            errorDictionary.Add(-10215, "The call to FPPool_GetLastError() or FPPool_GetLastErrorInfo() failed. The error status of the previous function call is unknown; the previous call may have succeeded.");
            systemErrorDictionary.Add(0, "");
            systemErrorDictionary.Add(1, "PAI_INTERNAL");
            systemErrorDictionary.Add(2, "POOL_EXISTS");
            systemErrorDictionary.Add(3, "PAI_NO_POOL");
            systemErrorDictionary.Add(4, "PAI_NO_ACCESS,please check your input location");
            systemErrorDictionary.Add(5, "PAI_BAD_REFERENCE ");
            systemErrorDictionary.Add(6, "PAI_STRING_OVERFLOW");
            systemErrorDictionary.Add(7, "PAI_NO_ADDRESS");
            systemErrorDictionary.Add(8, "PAI_UNKNOWN_SECTION,Target format is not correct");
            systemErrorDictionary.Add(9, "PAI_USE_ANONYMOUS");
        }
    }
}
