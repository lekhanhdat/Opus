import React, { useContext, useEffect } from "react";
import { LoadStatus } from "../../../Constants/index";
import { ConfigureContext, InternalContext } from "../Store/index";

const RadioButton = ({ nodeKey, loadStatus, isShow }) => {

    const { selectedKey, changeSelectedKey, getSelectedKeyForUnmount } = useContext(InternalContext);
    const configure = useContext(ConfigureContext);

    useEffect(() => {
        return () => {
            if (getSelectedKeyForUnmount() === nodeKey) {
                configure.onSelected("");
                changeSelectedKey("");
            }
        };
    }, []);

    const onRadioClick = async (e) => {

        e.stopPropagation();

        if (!isShow || selectedKey === nodeKey) {
            return;
        }

        if (configure.onSelected !== null && configure.onSelected !== undefined) {
            await configure.onSelected(nodeKey);
        }
        changeSelectedKey(nodeKey);
    };

    const onRadioKeyUp = (e) => {

        if (e.keyCode !== 32) {
            return;
        }

        e.stopPropagation();
        e.preventDefault();

        onRadioClick(e);
    };

    return (
        <div className="reco-node-radio-wrapper" style={{ marginRight: isShow ? "8px" : "0" }}>
            {
                loadStatus === LoadStatus.Loading ?
                    <img className="reco-node-loading" src="/Images/Base/loading_18x18.gif" aira-hidden="true" /> :
                    isShow &&
                    <div
                        className="reco-node-radio"
                        onClick={onRadioClick}
                        tabIndex="0"
                        role="radio"
                        aria-checked={selectedKey === nodeKey}
                        onKeyUp={onRadioKeyUp}
                    >
                        <div
                            className="reco-node-radio-inner"
                            aria-hidden="true"
                        >
                            {
                                selectedKey === nodeKey &&
                                <div className="reco-node-radio-selected" aria-hidden="true"></div>
                            }
                        </div>
                    </div>
            }
        </div>
    );
};

export default RadioButton;