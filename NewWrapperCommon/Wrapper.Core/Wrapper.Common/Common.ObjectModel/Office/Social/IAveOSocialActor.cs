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
using System.Text;

namespace AvePoint.Wrapper.Common.Office
{
    public interface IAveOSocialActor
    {
        AveOSocialActorType ActorType { get;  }
        Uri Uri { get;  }
        string Name { get;  }
        string AccountName { get;  }
        string StatusText { get;  }
        string Id { get;  }
        Guid TagGuid { get;  }
        AveOSocialStatusCode Status { get;  }
        bool CanFollow { get;  }
        Uri ContentUri { get;  }
        string EmailAddress { get;  }
        Uri FollowedContentUri { get;   }
        Uri ImageUri { get;   }
        bool IsFollowed { get;   }
        Uri LibraryUri { get;   }
        Uri PersonalSiteUri { get;   }
        string Title { get;   }
    }
}
