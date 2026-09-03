import React, { useState } from "react";
import { SimplePager } from "../../Common/Pager";

const DefaultPageSize = 10;

const BuildPageSizeSelectorItems = () => (
    [
        {
            key: 5,
            value: 5,
            checked: false,
        },
        {
            key: 10,
            value: 10,
            checked: true,
        },
        {
            key: 15,
            value: 15,
            checked: true
        },
        {
            key: 50,
            value: 50,
            checked: true
        }
    ]
);

const Paginate = ({ onPageIndexChange, onPageSizeChange, currentPageCount, hasNextPage, pageIndex }) => {

    const [pageSize, setPageSize] = useState(DefaultPageSize);

    const [pageSizeSelectorItems] = useState(BuildPageSizeSelectorItems());

    const onChange = (index, size) => {
        onPageIndexChange(index + 1);
    };

    const onInnerPageSizeChange = (args) => {
        const value = parseInt(args.newValue.value);
        setPageSize(value);
        onPageSizeChange(value);
    };

    return (
        <div className="reco-az-paginate">
            <div className="reco-az-paginate-rows">
                <span>
                    {RMResx.RM_Common_ShowRows}
                </span>
                <R.Combobox
                    width="60px"
                    height={24}
                    searchable={false}
                    textField='value'
                    valueField='key'
                    checkedField='checked'
                    items={pageSizeSelectorItems}
                    onChange={onInnerPageSizeChange}
                    aria={{ ariaLabel: RMResx.RM_Common_ShowRows }}
                />
            </div>
            <div className="reco-az-paginate-paging">
                <SimplePager
                    pagerIndex={pageIndex - 1}
                    pagerSize={pageSize}
                    shownCount={currentPageCount}
                    hasNext={hasNextPage}
                    onChange={onChange}
                />
            </div>
        </div>
    );

};

export default Paginate;