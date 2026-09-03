import React, { useEffect, useState } from "react";
import { Messagebox } from "../../../Common/Messagebox";
import { showToast } from "../../../../Utilities/CommonUtil";
import ExportMLReport from "./Export/ExportMLReport";

const Actions = () =>{

    const [showDataDialog, setShowDataDialog] = useState(false);
    const onExport = () => {
        //Messagebox({ content: RMResx.RM_JS_Common_ExportMsg, actionFun: processExport.bind(this) });
        setShowDataDialog(true);
    };

    const onHide = ()=>{
        setShowDataDialog(false);
    };
    return <div>
        <R.Button primary={true} classify="theme" text={RMResx.RM_MA_Export} onClick={onExport} />
        <ExportMLReport show={showDataDialog} onHide={onHide} />
    </div>;
};

export default Actions;