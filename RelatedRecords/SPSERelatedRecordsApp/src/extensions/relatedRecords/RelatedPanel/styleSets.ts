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
import { mergeStyleSets } from "office-ui-fabric-react";

const classNames = mergeStyleSets({
  iconCell: {
    display: "flex !important",
    alignItems: "center",
  },
  fileIconHeaderIcon: {
    fontSize: 16,
  },
  itemWrap: {
    height: "100%",
    display: "flex",
    alignItems: "center",
    fontSize: 14,
  },
  itemText: {
    textOverflow: "ellipsis",
    whiteSpace: "nowrap",
    overflow: "hidden",
  },
  itemLink: {
    color: "#0072d0",
    textDecoration: "underline",
    cursor: "pointer",
  },
  actionIcon: {
    width: 20,
    height: 20,
  },
  searchResultLoading: {
    height: 43,
    marginRight: 48,
  },
  searchResultEmpty: {
    display: "flex",
    justifyContent: "center",
    alignItems: "center",
    fontSize: 14,
  },
});

export default classNames;
