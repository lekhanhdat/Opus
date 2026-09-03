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
import {
  DefaultButton,
  DetailsList,
  IDetailsListProps,
  PrimaryButton,
} from "office-ui-fabric-react";

import { EResultState } from "../../constants/searchResult";

import * as strings from "RelatedRecordsCommandSetStrings";

import "./LazyLoadTable.css";

export interface IProps extends IDetailsListProps {
  isSearching: boolean;
  currentPage: number;
  totalPage: number;
  onScroll: () => void;
  onDismissCallout: () => void;
  onAddRelated: () => void;
}

function LazyLoadSearchResults(props: IProps) {
  const {
    isSearching,
    currentPage,
    totalPage,
    onScroll,
    onDismissCallout,
    onAddRelated,
    items, // this is searchItems
    ...rest
  } = props;

  const handleScroll = (e: React.UIEvent<HTMLDivElement>): void => {
    if (currentPage + 1 >= totalPage) {
      return;
    }
    const { scrollHeight, scrollTop, clientHeight } = e.currentTarget;
    if (scrollHeight - scrollTop <= clientHeight + 50) {
      if (isSearching) {
        return;
      }
      onScroll();
    }
  };

  const onRenderSearchResultsFooter = (): JSX.Element => {
    if (
      items.length &&
      items[0].key !== EResultState.IS_SEARCHING &&
      items[0].key !== EResultState.EMPTY
    ) {
      return (
        <div className="related-records-search-results-footer">
          <DefaultButton onClick={onDismissCallout}>
            {strings.Related_App_Common_Cancel}
          </DefaultButton>
          <PrimaryButton onClick={onAddRelated}>
            {strings.Related_App_AddRelatedBtn}
          </PrimaryButton>
        </div>
      );
    }

    return null;
  };

  return (
    <div>
      <div className="lazy-load-table" onScroll={handleScroll}>
        <DetailsList {...rest} items={items} onShouldVirtualize={() => false} />
      </div>
      {onRenderSearchResultsFooter()}
    </div>
  );
}

export default LazyLoadSearchResults;
