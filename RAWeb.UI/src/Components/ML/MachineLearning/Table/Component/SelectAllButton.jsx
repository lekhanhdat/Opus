import { useState, useEffect } from "react";

const SelectAllButton = ({showPopover, onSelectAll, isShowSelectAll, popoverContent}) => {
    
    const [isShowSelectAllBtn, setIsShowSelectAllBtn] = useState(false);

    useEffect(()=>{
        setIsShowSelectAllBtn(isShowSelectAll);
    },[isShowSelectAll]);

    const onClick = () => {
        setIsShowSelectAllBtn(!isShowSelectAllBtn);
        onSelectAll(isShowSelectAllBtn);
    };

    const getSelectAllBtnName = () => {
        let buttonName = isShowSelectAllBtn
            ? RMResx.RM_MT_PickList_SelectAll
            : RMResx.RM_PRM_PRE_GlobalSearch_AllResultClear;
        return buttonName;
    };

    return (
        <div className="ra-selectall-button">
            {
                !isShowSelectAll && <span className="ra-main-selected-counter">
                    {RMResx.RM_PRM_PRE_GlobalSearch_ResultSelected}
                </span>
            }
            <a
                tabIndex="0"
                role="button"
                aria-label={getSelectAllBtnName()}
                className="ra-main-italics-link margin-left-xs"
                onClick={onClick}
            >
                {getSelectAllBtnName()}
            </a>
            { showPopover && isShowSelectAllBtn && <$g.Popover>{popoverContent}</$g.Popover> }
        </div>
    );
};

export default SelectAllButton;
