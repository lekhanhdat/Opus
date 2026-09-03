import React, { useContext } from "react";
import { LoadStatus, CheckStatus } from "../../../Constants/index";
import { InternalContext, ConfigureContext } from "../Store/index";

const CheckboxButton = ({ nodeKey, loadStatus, isShow }) => {

    const { updateSignal, checkAction } = useContext(InternalContext);

    const configure = useContext(ConfigureContext);

    const onCheckboxClick = (e) => {

        if(configure.isReadonly) {
            return;
        }

        e.stopPropagation();
        checkAction.setNodeCheckStatus(nodeKey);
    };

    return (
        <div className="reco-node-checkbox-wrapper" style={{ marginRight: isShow ? "8px" : "0" }}>
            {
                loadStatus === LoadStatus.Loading ?
                    <img className="reco-node-loading" src="/Images/Base/loading_18x18.gif" aira-hidden="true" /> :
                    isShow &&
                    <div
                        className={`reco-node-checkbox ${configure.isReadonly && "reco-node-checkbox-readonly"}`}
                        onClick={onCheckboxClick}
                        tabIndex="0"
                        role="radio"
                        aria-checked={updateSignal && checkAction.getNodeCheckStatus(nodeKey) === CheckStatus.Checked}
                        // onKeyUp={onRadioKeyUp}
                    >
                        <div
                            className="reco-node-checkbox-inner"
                            aria-hidden="true"
                        >
                            {
                                updateSignal && checkAction.getNodeCheckStatus(nodeKey) !== CheckStatus.Unchecked && (
                                    checkAction.getNodeCheckStatus(nodeKey) === CheckStatus.Checked ?
                                        <div className="reco-node-checkbox-checked" aria-hidden="true"></div> :
                                        <div className="reco-node-checkbox-half" aria-hidden="true"></div>
                                )
                            }
                        </div>
                    </div>
            }
        </div>
    );
};

export default CheckboxButton;