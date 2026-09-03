import React, { useRef, useState, useEffect } from "react";
import StringUtil from "../../../../Utilities/StringUtil";
import { showToast } from "../../../../Utilities/CommonUtil";
import { useStableCallback } from "../../../Common/Hooks";
import { RoleType } from "../Constants/RoleType";
import { ManualReviewAction , ManualReviewActionI18Ns} from "../Constants/ManualReviewActions";

const maxRetryFileSizeNum = 20; //MB

const allowRetryFileSize = maxRetryFileSizeNum * 1024 * 1024;

const UnderReviewImportPanel = ({ show, onHide }) => {

    const [files, setFiles] = useState([]);

    const [chart, setChart] = useState(false);

    const [totalCount, setTotalCount] = useState(0);

    const [approveCount, setApproveCount] = useState(0);

    const [rejectCount, setRejectCount] = useState(0);

    const [showLaoding, setShowLaoding] = useState(false);

    const [disabledButton,setDisableButton] = useState(true);

    const uploaderRef = useRef();

    useEffect(()=>{
        !show && handleDelete();
    },[show]);

    const handleUpload = (args) => {
        if (args.isSucceed) {
            args.files[0].fileId = StringUtil.newGuid();
            uploaderRef.current.files = args.files[0];
            setDisableButton(args.file.size > allowRetryFileSize);
        }
    };

    const handleDelete = () => {    
        setChart(false);
        setDisableButton(true);
    };


    const onSave = useStableCallback(async () => {
        if (!$$.verify("allValidation")) {
            return false;
        }
        $$.loading(true);
        const formData = new FormData();
        formData.append('fileUp',  uploaderRef.current.files.file,  uploaderRef.current.files.fileName);
        const url = "/api/ManualApproval/ImportManualUnderReviewDatas";
        fetch(url,
            {
                method: 'POST',
                body: formData,
            })
            .then(function(response){
                $$.loading(false);
                setFiles([]);
                setChart(false);
                return response.text().then(function (dataString) {
                    return {
                        responseStatus: response.status,
                        responseString: JSON.parse(dataString)
                    };
                });
            }).then(function(result){
                if(result.responseString.MessageType == "0"){
                    if (RM.RoleType != RoleType.StandardUser) {
                        showToast.success(<$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>);
                    } else {
                        showToast.success(RMResx.RM_JS_MA_JobSucessMessage);
                    }
                }else{
                    showToast.error(result.responseString.ErrorMessage);
                }
                onHide();
            });
    });

    const onRetrieve = useStableCallback(async () => {
        if (!$$.verify("allValidation")) {
            return false;
        }
        setShowLaoding(true);
        const formData = new FormData();
        formData.append('fileUp',  uploaderRef.current.files.file,  uploaderRef.current.files.fileName);
        const url = "/api/ManualApproval/GetImportFileInfo";
        fetch(url,
            {
                method: 'POST',
                body: formData,
            })
            .then(function(response){
                return response.text().then(function (dataString) {
                    return {
                        responseStatus: response.status,
                        responseString: JSON.parse(dataString)
                    };
                });
            }).then(function(result){
                setTotalCount(result.responseString.TotalCount);
                setApproveCount(result.responseString.ApproveCount);
                setRejectCount(result.responseString.RejectCount);
                setShowLaoding(false);
                setChart(true);
            });
            
    });

    return ( 
        <R.Panel
            header={RMResx.RM_JS_MA_ImportDataPanel_Title}
            size={670}
            status={{ show: show }}
            destroy={true}
            onHide={onHide}
        >
            <div>
                <R.Validation id="allValidation">
                    <div>
                        <div>
                            <div className="ra-import-title" tabIndex="0">
                                <$g.I18NProvider msg={StringUtil.trimEndColon(RMResx.RM_JS_TM_SelectImportFile)} />
                                <$g.Popover>
                                    {RMResx.RM_JS_MA_Import_AppovalTips}
                                </$g.Popover>
                            </div>
                            <div>
                                <R.Validation
                                    element="Uploader"
                                    require={RMResx.RM_JS_BCM_ImportSetting_selectCSVFile}>
                                    <R.Uploader
                                        ref={uploaderRef}
                                        files={files}
                                        fileTypes={["CSV"]}
                                        onUpload={handleUpload}
                                        onDelete={handleDelete}
                                        maxSize={"80MB"}
                                        showMaxSize={true}
                                    />
                                </R.Validation>
                            </div>
                            <div className="ra-import-title" tabIndex="0">
                                <$g.I18NProvider msg={RMResx.RM_JS_MA_Import_RetrieveMessage} />
                            </div>
                            <div className="ra-view-result-button" id="ra-view-result-button-id">
                                {
                                    showLaoding && <R.Button
                                        primary={false}
                                        classify="default"
                                        className={"ra-view-retrive-button"}
                                        text={RMResx.RM_JS_MA_Import_Retrieving}
                                        onClick={onRetrieve}
                                        disabled={disabledButton}
                                    />
                                }
                                {
                                    !showLaoding && <R.Button
                                        primary={false}
                                        classify="default"
                                        text={RMResx.RM_JS_MA_Import_Retrieve}
                                        onClick={onRetrieve}
                                        disabled={disabledButton}
                                    />
                                }
                                <$g.Popover>
                                    {RMResx.RM_JS_MA_Import_FileSize_ValidMsg.format(maxRetryFileSizeNum)}
                                </$g.Popover>
                            </div>
                            {
                                chart &&
                                <div className="reco-manual-import-counter-wrapper">
                                    <div className="reco-manaul-import-total-count">
                                        <div className={`reco-manual-import-counter-icon fia-show-report`}></div>
                                    </div>
                                    <div className="reco-manual-summary-counter">
                                        <div className="reco-manual-summary-counter-number">
                                            {totalCount}
                                        </div>
                                        <div className="reco-manual-import-counter-title"
                                            tabIndex="0"
                                            data-tooltip="ifneed"
                                            aria-label={RMResx.RM_JS_MA_Import_TotalImport}>
                                            {RMResx.RM_JS_MA_Import_TotalImport}
                                        </div>
                                    </div>
                                    <div className="reco-manaul-import-action-count">
                                        <div className="reco-manaul-import-approve-action-count" >
                                            <div className="reco-manaul-import-reject-action-count-number"> {approveCount}</div>
                                            <div className="reco-manaul-import-reject-action-count-action">
                                                {ManualReviewActionI18Ns.get(ManualReviewAction.Approve)}
                                            </div>
                                        </div>
                                        <div className="reco-manaul-import-action-split"></div>
                                        <div className="reco-manaul-import-reject-action-count" >
                                            <div className="reco-manaul-import-reject-action-count-number">{rejectCount}</div>
                                            <div className="reco-manaul-import-reject-action-count-action">
                                                {ManualReviewActionI18Ns.get(ManualReviewAction.Reject)}
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            }
                        </div>
                    </div>
                </R.Validation>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onHide} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={onSave} />
            </>
        </R.Panel>
    );
};

export default UnderReviewImportPanel;