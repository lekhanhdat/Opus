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
using AvePoint.RA.Contract.Explorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class MoveDestinationManager : IDisposable
    {
        private RALogger logger = RALogger.GetInstance(typeof(MoveDestinationManager));
        private DestinationBase destination;
        private AppendItemMapping appendItemMapping = null;
        private MoveSettingInfo moveSetting;

        //dest connection path
        public string DestRootPath { get; set; }
        //connection id created by docave
        public Guid AveSiteId { get; set; }
        public string ContainerId { get; set; }
        public bool KeepSourceClassification { get; set; }

        public MoveDestinationManager(RMExplorerMoveJobMessage msg, MoveSettingInfo mMoveSetting, AppendItemMapping mapping)
        {
            appendItemMapping = mapping;
            moveSetting = mMoveSetting;
            DestRootPath = msg.MoveDestination.RootSiteUrl;
            AveSiteId = msg.MoveDestination.AveSiteId;
            ContainerId = msg.MoveDestination.ContainerId;
            KeepSourceClassification = msg.MoveDestination.KeepSourceClassification;
            Init(msg);
        }

        public DestinationBase Destination
        {
            get
            {
                return destination;
            }
            private set
            {
                destination = value;
            }
        }

        private void Init(RMExplorerMoveJobMessage msg)
        {
            logger.Info("Init destination info");
            try
            {
                switch (msg.DestFlag)
                {
                    case RecordFlag.Teams:
                    case RecordFlag.Groups:
                    case RecordFlag.SP:
                        {
                            var restore = new SPMoveRestore(msg.MoveDestination, moveSetting, appendItemMapping);
                            destination = new SPDestination(restore, restore.spImport.destinationContainerUrl, restore.spImport.SiteId);
                            break;
                        }
                    //case RecordFlag.FS:
                    //    {
                    //        var restore = new FileSystemMoveRestore(msg.MoveDestination, moveSetting, appendItemMapping);
                    //        destination = new FileSystemDestination(restore, restore.fsRestore.destinationContainerUrl);
                    //        break;
                    //    }
                    case RecordFlag.None:
                    default:
                        {
                            logger.Warn("unknow destination type");
                            throw new Exception("Unknow destination type");
                        }
                }
                destination.AppendItemMapping = appendItemMapping;
            }
            catch (Exception ex)
            {
                JobManagement.GetInstance(msg).HasErrorNode = true;
                logger.Error(string.Format("Error in generate destination info, reason : {0}.", ex.ToString()));
                throw;
            }
        }

        public void Dispose()
        {
            using(destination as IDisposable) { }
        }
    }

    public class DestinationListTermSetting
    {
        public bool HasDefaultTermValue { get; set; }
        public Guid DefautTermId { get; set; }
        public string DefaultTermName { get; set; }
        public Guid FieldId { get; set; }
        public Guid TextFieldId { get; set; }
    }
}
