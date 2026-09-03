import React, { useCallback, useEffect, useRef } from "react";
import PropTypes from "prop-types";
import "./index.less";

const Menu = ({ onPopupClose, onRefresh }) => {

    const popupRef = useRef();

    const onEscFunction = useCallback((e) => {

        if(e.keyCode === 27) {
            onPopupClose();
        }

    }, []);

    const onTabFunction = useCallback((e) => {

        if(e.keyCode === 9 && !popupRef.current.contains(e.target)) {
            onPopupClose(false);
        }
        
    }, []);

    useEffect(() => {
        popupRef.current.focus();
        document.addEventListener("mousedown", onDocumentClick);
        document.addEventListener("keydown", onEscFunction);
        document.addEventListener("keyup", onTabFunction);

        return () => {
            document.removeEventListener("mousedown", onDocumentClick);
            document.removeEventListener("keydown", onEscFunction);
            document.removeEventListener("keyup", onTabFunction);
        };
    }, []);

    const onDocumentClick = (e) => {
        if (popupRef.current.contains(e.target)) {
            return;
        }

        onPopupClose();
    };

    const onMenuItemClick = (e, needInvokeFunc) => {

        e.stopPropagation();

        needInvokeFunc();
        onPopupClose();
    };

    const onMenuKeyUp = (e) => {
        e.stopPropagation();
        return;
    };

    const onMenuItemKeyUp = (e, needInvokeFunc) => {
        e.stopPropagation();
        if (e.keyCode !== 13) {
            return;
        }

        onMenuItemClick(e, needInvokeFunc);
    };

    return (
        <div className="reco-tree-menu-popup-wrapper">
            <div className="reco-tree-menu-popup"
                ref={popupRef}
                onClick={e => e.stopPropagation()}
                role="menu"
                tabIndex="0"
                onKeyUp={onMenuKeyUp}
            >
                <div className="reco-tree-menu-item"
                    onClick={(e) => onMenuItemClick(e, onRefresh)}
                    role="menuitem"
                    tabIndex="0"
                    onKeyUp={(e) => onMenuItemKeyUp(e, onRefresh)}
                >
                    <div className="reco-tree-menu-item-icon fia-refresh" aria-hidden="true"></div>
                    <div className="reco-tree-menu-item-title">{RMResx.RM_DAM_Refesh}</div>
                </div>
            </div>
            <div className="reco-tree-menu-arrow-wrapper">
                <div className="reco-tree-menu-arrow-border"></div>
                <div className="reco-tree-menu-arrow"></div>
            </div>
        </div>
    );
};

Menu.propTypes = {
    onPopupClose: PropTypes.func,
    onRefresh: PropTypes.func,
};

export default Menu;