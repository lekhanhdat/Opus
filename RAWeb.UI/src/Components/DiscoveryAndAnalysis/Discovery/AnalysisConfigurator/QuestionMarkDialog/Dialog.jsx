import { useEffect, useState } from "react";
import "./dialog.less";

const DialogInfo = ({ isShow, onCloseDialog, messages }) => {

    const onClose = () => {
        onCloseDialog();
    };

    const configDialogContent = () => {
        return <div className="dialog-container">
            <div className="dialog-container-msg" tabIndex="0">
                <div className="margin-bottom-l">{messages.Content1}</div>
                <div>{messages.Content2}</div>
            </div>
            <div className="dialog-container-table">
                <div className="dialog-container-tableheader">
                    <div className="dialog-container-col">
                        <span className="dialog-tableheader-text" tabIndex="0">{messages.Header1}</span>
                    </div>
                    <div className="dialog-container-col">
                        <span className="dialog-tableheader-text" tabIndex="0">{messages.Header2}</span>
                    </div>
                    <div>
                        <span className="dialog-tableheader-text" tabIndex="0">{messages.Header3}</span>
                    </div>
                </div>
                <div className="dialog-container-tablectn">
                    <div className="dialog-container-col">
                        <span className="dialog-tablectn-text" tabIndex="0">{messages.Cell1}</span>
                    </div>
                    <div className="dialog-container-col">
                        <span className="dialog-tablectn-text" tabIndex="0">{messages.Cell2}</span>
                    </div>
                    <div>
                        <span className="dialog-tablectn-text" tabIndex="0">{messages.Cell3}</span>
                    </div>
                </div>
            </div>
        </div>;
    };

    return <div>
        <R.Dialog
            id="questionDialog"
            header={messages.Title}
            status={{ show: isShow }}
            width={700}
            closeable={false}
            destroy={true}
        >
            {configDialogContent()}
            <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_FA_Discovery_DialogBtn} onClick={onClose} />
        </R.Dialog>
    </div>;
};

export default DialogInfo;