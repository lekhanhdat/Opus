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




namespace AvePoint.GCommon.Media.StorageService
{
    #region using directives

    #endregion

    internal class PathGenerator : IPathGenerator
    {
        INameGeneratorFactory nameGeneratorFactory;

        public PathGenerator()
        {
            this.nameGeneratorFactory = new NameGeneratorFactory();
        }

        public virtual HoldFileInfo Generate(PathParameter path)
        {
            var nameGenerator = nameGeneratorFactory.CreateGenerator(path.JobId);
            var tempFolderName = nameGenerator.GenerateFolderName();
            var tempFileName = nameGenerator.GenerateFileName();
            var dataFileName = tempFileName + ".data";
            var metadataFileName = tempFileName + ".metadata";
            var fileContainer = @"\" + path.JobId + @"\" + tempFolderName;
            var fileInfo = new HoldFileInfo(fileContainer, dataFileName, metadataFileName);
            return fileInfo;
        }

        public void Reset(PathParameter path)
        {
            this.nameGeneratorFactory.DestoryGenerator(path.JobId);
        }
    }
}