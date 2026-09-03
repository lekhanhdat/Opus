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
using AvePoint.GCommon;
using AvePoint.ObjectModel.ClientOM;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ClientFile = Microsoft.SharePoint.Client.File;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveFileVersionHandler
    {
        protected static AveLogger Log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private bool _SourceCheckOut;
        private bool _DestinationCheckOut;
        private SPDocVersion _SourceVersion;
        private SPDocVersion _DestinationVersion;
        private AveListMemento _ListMemento;
        private string _CheckInComment;
        public List<int> NeedDeleteVersion = new List<int>();
        private int _QueryCount = 0;
        public AveFileVersionHandler(bool sCheckOut, bool dCheckout, SPDocVersion sVersion, SPDocVersion dVersion, string comment, AveListMemento memento)
        {
            _SourceCheckOut = sCheckOut;
            _DestinationCheckOut = dCheckout;
            _SourceVersion = sVersion;
            _DestinationVersion = dVersion;
            _ListMemento = memento;
            _CheckInComment = comment;
        }

        int ConditionToInt()
        {
            if (_SourceCheckOut)
            {
                if (_DestinationCheckOut)
                {
                    if (_SourceVersion.Major > _DestinationVersion.Major)
                    {
                        if (_DestinationVersion.Minor != 0)
                        {
                            if (_SourceVersion.Minor == 0 && _SourceVersion.Major - _DestinationVersion.Major == 1)
                            {
                                return 0;
                            }
                            else
                            {
                                return 1;
                            }
                        }
                        else
                        {
                            return 2;
                        }
                    }
                    else
                    {
                        return 3;
                    }
                }
                else
                {
                    if (_SourceVersion.Major > _DestinationVersion.Major)
                    {
                        return 4;
                    }
                    else
                    {
                        return 5;
                    }
                }
            }
            else
            {
                if (_DestinationCheckOut)
                {
                    if (_SourceVersion.Major > _DestinationVersion.Major)
                    {
                        return 6;
                    }
                    else
                    {
                        return 7;
                    }
                }
                else
                {
                    if (_SourceVersion.Major > _DestinationVersion.Major)
                    {
                        return 8;
                    }
                    else
                    {
                        return 9;
                    }
                }
            }
        }

        protected void IncreaseMajor(ClientFile file)
        {
            while (_SourceVersion.Major - _DestinationVersion.Major > 0)
            {
                _QueryCount++;
                file.CheckOut();
                _DestinationVersion.AddVersion(1, 0);
                if (_SourceCheckOut && _SourceVersion.Major == _DestinationVersion.Major && _SourceVersion.Minor == 0)
                {
                    break;
                }
                file.CheckIn(_CheckInComment, CheckinType.MajorCheckIn);
                ResetQueryCount(file);
                if (!_DestinationVersion.Equal(_SourceVersion))
                {
                    if (_DestinationVersion.Major.Equals(_SourceVersion.Major))
                    {
                        Log.Debug("Current version: {0}, delete version: {1}. Skip delete this version as it is the current publish version.", _SourceVersion.ToInt(), _DestinationVersion.ToInt());
                        continue;
                    }
                    NeedDeleteVersion.Add(_DestinationVersion.ToInt());
                }
            }
        }

        private void ResetQueryCount(ClientFile file)
        {
            if (_QueryCount >= 20)
            {
                file.Context.ExecuteQuery();
                _QueryCount = 0;
            }
        }

        protected void IncreaseMinor(ClientFile file)
        {
            while (_SourceVersion.Minor > _DestinationVersion.Minor)
            {
                _QueryCount++;
                file.CheckOut();
                _DestinationVersion.AddVersion(0, 1);
                if (_SourceCheckOut && _SourceVersion.Minor == _DestinationVersion.Minor)
                {
                    break;
                }
                file.CheckIn(_CheckInComment, CheckinType.MinorCheckIn);
                ResetQueryCount(file);
                if (!_DestinationVersion.Equal(_SourceVersion))
                {
                    NeedDeleteVersion.Add(_DestinationVersion.ToInt());
                }
            }
        }

        protected void DeleteVersions(ClientFile file)
        {
            int count = NeedDeleteVersion.Count;
            if (_SourceCheckOut && count > 0)
            {
                count = count - 1;
            }
            for (int i = 0; i < count; i++)
            {
                file.Versions.DeleteByID(NeedDeleteVersion[i]);
            }
        }

        public void IncreaseVersion(ClientFile file)
        {
            int condition = ConditionToInt();
            switch (condition)
            {
                case 0:
                    _ListMemento.SetListSetting(true, false, false, false);
                    file.ListItemAllFields.Update();
                    _DestinationVersion.AddVersion(1, 0);
                    break;
                case 1:
                case 2:
                    _ListMemento.SetListSetting(true, false, false, false);
                    file.CheckIn(_CheckInComment, CheckinType.MajorCheckIn);
                    if (condition == 1)
                    {
                        _DestinationVersion.AddVersion(1, 0);
                    }
                    IncreaseMajor(file);
                    if (_SourceVersion.Minor > _DestinationVersion.Minor)
                    {
                        _ListMemento.SetListSetting(true, true, false, false);
                        IncreaseMinor(file);
                    }
                    break;
                case 6:
                    _ListMemento.SetListSetting(true, false, false, false);
                    file.CheckIn(_CheckInComment, CheckinType.MajorCheckIn);
                    if (_DestinationVersion.Minor != 0)
                    {
                        _DestinationVersion.AddVersion(1, 0);
                    }
                    IncreaseMajor(file);
                    if (_SourceVersion.Minor > _DestinationVersion.Minor)
                    {
                        _ListMemento.SetListSetting(true, true, false, false);
                        IncreaseMinor(file);
                    }
                    break;
                case 3:
                case 7:
                    _ListMemento.SetListSetting(true, true, false, false);
                    file.CheckIn(_CheckInComment, CheckinType.MinorCheckIn);
                    if (_DestinationVersion.Minor == 0)
                    {
                        _DestinationVersion.AddVersion(0, 1);
                    }
                    IncreaseMinor(file);
                    break;
                case 4:
                case 8:
                    _ListMemento.SetListSetting(true, false, false, false);
                    IncreaseMajor(file);
                    if (_SourceVersion.Minor > _DestinationVersion.Minor)
                    {
                        _ListMemento.SetListSetting(true, true, false, false);
                        IncreaseMinor(file);
                    }
                    break;
                case 5:
                case 9:
                    _ListMemento.SetListSetting(true, true, false, false);
                    IncreaseMinor(file);
                    break;
                default:
                    break;
            }
            DeleteVersions(file);
        }

        public void AddNewFileNeedDeleteVersion(RestoreResult restoreResult)
        {
            if (restoreResult == RestoreResult.AddNew)
            {
                NeedDeleteVersion.Add(_DestinationVersion.ToInt());
            }
        }
    }
}
