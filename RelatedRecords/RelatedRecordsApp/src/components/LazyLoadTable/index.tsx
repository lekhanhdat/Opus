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
import * as React from "react";
import { DetailsList, IDetailsListProps } from "@fluentui/react";

interface LazyLoadTableProps extends IDetailsListProps {
  isSearching: boolean;
  currentPage: number;
  totalPage: number;
  onScroll: () => void;
  // eslint-disable-next-line @rushstack/no-new-null
  onRenderFooter: () => JSX.Element | null;
}

const LazyLoadTable: React.FC<LazyLoadTableProps> = (props) => {
  const {
    isSearching,
    currentPage,
    totalPage,
    onScroll,
    onRenderFooter,
    items, // this is searchItems
    ...rest
  } = props;

  const handleScroll = (event: React.UIEvent<HTMLDivElement>): void => {
    if (currentPage + 1 >= totalPage) {
      return;
    }
    const { scrollHeight, scrollTop, clientHeight } = event.currentTarget;
    if (scrollHeight - scrollTop <= clientHeight + 50) {
      if (isSearching) {
        return;
      }
      onScroll();
    }
  };

  return (
    <div>
      <div
        data-is-scrollable="true"
        className="related-records-search-result-wrapper"
        style={{ overflowY: "auto" }}
        onScroll={handleScroll}
      >
        <DetailsList {...rest} items={items} />
      </div>
      {onRenderFooter()}
    </div>
  );
};

export { LazyLoadTable };
