import React, { useState, useEffect } from "react";
import PropTypes from "prop-types";
import "./index.less";

const Pager = ({ distance, pageIndex, pageCount, onPageIndexChange }) => {

    const [internalPageIndex, setInternalPageIndex] = useState(pageIndex);

    const [inputPageIndex, setInputPageIndex] = useState(pageIndex);

    useEffect(() => {
        if (pageIndex !== internalPageIndex) {
            setInternalPageIndex(pageIndex);
            setInputPageIndex(pageIndex);
            onPageIndexChange(pageIndex);
        }
    }, [pageIndex]);

    const onPagerButtonClick = (index) => {
        if(index === internalPageIndex) {
            return;
        }
        setInputPageIndex(index);
        setInternalPageIndex(index);
        onPageIndexChange(index);
    };

    const onPageInputChange = (e) => {
        setInputPageIndex(e.target.value);
    };

    const onPageBlurChange = (e) => {

        e.stopPropagation();

        let value = e.target.value;
        if(value < 1) {
            value = 1;
        }
        if(value > pageCount) {
            value = pageCount;
        }
        const intValue = parseInt(value);

        setInputPageIndex(intValue);

        if(intValue === internalPageIndex) {
            return;
        }

        setInternalPageIndex(intValue);
        onPageIndexChange(intValue);
    };

    const onPageInputKeyUp = (e) => {
        e.stopPropagation();

        if(e.keyCode !== 13) {
            return;
        }

        onPageBlurChange(e);
    };

    const onPageButtonKeyUp = (e, value) => {
        e.stopPropagation();
        e.preventDefault();

        if(e.keyCode !== 13) {
            return;
        }

        onPagerButtonClick(value);
    };

    return (
        <div className="reco-pager-wrapper" style={{paddingLeft: 14 * distance + 8}}>
            <div className={`reco-pager-button fia-pager-first ${internalPageIndex <= 1 ? "reco-pager-button-disable" : ""}`}
                tabIndex={internalPageIndex <= 1 ? -1 : 0}
                onClick={() => onPagerButtonClick(1)}
                data-tooltip aria-label={RMResx.RM_JS_Common_GoToTheFirstPage}
                onKeyUp={(e) => onPageButtonKeyUp(e, 1)}
                aria-disabled={internalPageIndex <= 1}
            />
            <div className={`reco-pager-button fia-pager-previous ${internalPageIndex <= 1 ? "reco-pager-button-disable" : ""}`}
                tabIndex={internalPageIndex <= 1 ? -1 : 0}
                onClick={() => onPagerButtonClick(internalPageIndex - 1)}
                data-tooltip aria-label={RMResx.RM_JS_Common_GoToThePreviousPage}
                onKeyUp={(e) => onPageButtonKeyUp(e, internalPageIndex - 1)}
                aria-disabled={internalPageIndex <= 1}
            />
            <div className="reco-pager-content">
                <div className="reco-pager-text">{RMResx.RM_JS_Common_AUI_Pager_Page}</div>
                <input
                    type="text"
                    className="reco-pager-input"
                    aria-label={ RMResx.RM_JS_Common_AUI_Pager_Page + " " + internalPageIndex + RMResx.RM_JS_Common_AUI_Pager_Of + pageCount }
                    value={inputPageIndex}
                    onChange={onPageInputChange}
                    onBlur={onPageBlurChange}
                    onKeyUp={onPageInputKeyUp}
                />
                <div className="reco-pager-text">{RMResx.RM_JS_Common_AUI_Pager_Of + pageCount}</div>
            </div>
            <div className={`reco-pager-button fia-pager-next ${internalPageIndex >= pageCount ? "reco-pager-button-disable" : ""}`}
                tabIndex={internalPageIndex >= pageCount ? -1 : 0}
                onClick={() => onPagerButtonClick(internalPageIndex + 1)}
                data-tooltip aria-label={RMResx.RM_JS_Common_GoToTheNextPage}
                onKeyUp={(e) => onPageButtonKeyUp(e, internalPageIndex + 1)}
                aria-disabled={internalPageIndex >= pageCount}
            />
            <div className={`reco-pager-button fia-pager-last ${internalPageIndex >= pageCount ? "reco-pager-button-disable" : ""}`}
                tabIndex={internalPageIndex >= pageCount ? -1 : 0}
                onClick={() => onPagerButtonClick(pageCount)}
                data-tooltip aria-label={RMResx.RM_JS_Common_GoToTheLastPage}
                onKeyUp={(e) => onPageButtonKeyUp(e, pageCount)}
                aria-disabled={internalPageIndex >= pageCount}
            />
        </div>
    );
};

Pager.propTypes = {
    distance: PropTypes.number,
    pageIndex: PropTypes.number,
    pageCount: PropTypes.number,
    onPageIndexChange: PropTypes.func,
};

export default Pager;