import { useState, useRef, forwardRef, useImperativeHandle } from "react";
import _ from "lodash";
import "./index.less";
import PanelContent from "./PanelComponents/PanelContent";
import { ArchiveDataType, ArchiveOrRemoveFileType, ArchiveOrRemoveVersionType, MS365DataType, ScheduleType, TierTypes } from "../../Constants/DataOptimizeType";
import { DiscoveryNodeViewMode, DiscoveryQueryDataType } from "../../Constants";
import { useStableCallback } from "../../../../Common/Hooks";
import { getRequestVerificationToken, LicenseHelper, ServiceHelper, showToast } from "../../../../../Utilities/CommonUtil";
import StringUtil from "../../../../../Utilities/StringUtil";

const defaultDataOptimizeParameter = {
    ms365DataType: MS365DataType.None,
    dataType: DiscoveryQueryDataType.None,
    withoutDateQueryParameter: {
        from: -1,
        to: 999,
    },
    sizeRangeQueryParameter: {},
    nodeQueryParameter: {
        viewMode: DiscoveryNodeViewMode.Container,
        joinedContainerId: 0,
        containerIds: [],
        siteIds: [],
        pageSize: 5
    },
    fileExtensionQueryParameter: {},
    archiveDataType: ArchiveDataType.None,
    inactiveRuleQueryParameter: {
        enable: false,
        ruleIds: [],
    },
    rotRuleQueryParameter: {
        enable: true,
        ruleCategories : [
            {
                ruleCategory : 2,
                ruleIds : [],
                checked: false,
            },
            {
                ruleCategory : 3,
                ruleIds : [],
                checked: false,
            },
            {
                ruleCategory : 4,
                ruleIds : [],
                checked: false,
            },
        ]
    },
    processActionParameter: {
        archiveOrRemoveFile: ArchiveOrRemoveFileType.ArchiveAndRemove,
        archiveOrRemoveVersion: ArchiveOrRemoveVersionType.ArchiveAndRemove,
        isEnableLeaveStub: false,
        deleteRecords: false,
        isArchiveVersionOption: false,
        archiveVersionValue: "0",
        selectedLevelStub: {},
    },
    selectedStorageParameter: {},
    moveToAnotherTierType: TierTypes.DefaultTier,
    scheduleParameter: {
        scheduleType: ScheduleType.Now
    },
};

const RunJobButtonType = {
    ScanJobBtn: 1,
    ArchiverJobBtn: 2,
    RunForImportBtn: 3,
};

const DataOptimizePanel = ({ viewMode }, ref) => {

    const validationRef = useRef(null);

    const [dataOptimizeParameter, setDataOptimizeParameter] = useState(defaultDataOptimizeParameter);

    const [o365TenantId, setO365TenantId] = useState();

    const [showPanel, setShowPanel] = useState(false);

    const [showPanelJobInfos, setShowPanelJobInfos] = useState({});

    const [showRunForImportPanel, setShowRunForImportPanel] = useState(false);

    const [runForImportUploadFiles, setRunForImportUploadFiles] = useState([]);

    const [showBackupDataDialog, setShowBackupDataDialog] = useState(false);

    const runForImportValidationRef = useRef(null);

    const runForImportUploaderRef = useRef(null);

    const isNewOpusTenantAccount = LicenseHelper.EnableRecordsArchiver(); 

    useImperativeHandle(ref, () => ({
        onShow: (query, o365TenantId, jobStatusInfo) => {
            const clonedDataOptimizeParameter = _.cloneDeep(defaultDataOptimizeParameter);

            let obj = assiginObj(clonedDataOptimizeParameter, query);
            if (obj.dataType === DiscoveryQueryDataType.Inactive) {
                obj.archiveDataType = ArchiveDataType.All;
            } else {
                obj.archiveDataType = ArchiveDataType.Special;
            }
            obj.ms365DataType = MS365DataType.Default;

            setDataOptimizeParameter(obj);
            setO365TenantId(o365TenantId);
            setShowPanelJobInfos(jobStatusInfo);
            setShowPanel(true);
        }
    }));

    const assiginObj = (target, sources) => {
        let obj = target;
        if (typeof target != 'object' || typeof sources != 'object') {
            return sources;
        }
        for (let key in sources) {
            if (Object.hasOwnProperty.call(target, key)) {
                obj[key] = assiginObj(target[key], sources[key]);
            } else {
                obj[key] = sources[key];
            }
        }
        if (_.isEmpty(sources)) {
            obj = sources;
        }
        return obj;
    };

    const onSave = useStableCallback(async (jobType, apiUrl) => {
        if (!$$.verify(validationRef.current)) {
            return false;
        }

        const currentJobInfos = await fetchUtility({
            url: "/api/RMDiscoveryOffice365JobManagementApi/GetLatest",
            method: "Get",
        });
        if (currentJobInfos.endTimeLong === showPanelJobInfos.endTimeLong) {
            const clonedDataInfo = _.cloneDeep(dataOptimizeParameter);
            clonedDataInfo.processActionParameter.archiveVersionValue = Number(clonedDataInfo.processActionParameter.archiveVersionValue);
            const formData = new FormData();
            if (runForImportUploaderRef.current) {
                formData.append('fileUp', runForImportUploaderRef.current.file, runForImportUploaderRef.current.file.fileName);
            }
            formData.append('setting', JSON.stringify(clonedDataInfo));
            $$.loading(true);
            fetch(apiUrl, {
                method: "POST",
                body: formData,
            }).then(function (response) {
                return response.text().then(function (dataString) {
                    return {
                        responseStatus: response.status,
                        responseString: JSON.parse(dataString),
                    };
                });
            }).then((res) => {
                const result = res.responseString;
                if (result.MessageType === 0) {
                    setShowPanel(false);
                    let content = clonedDataInfo.scheduleParameter.scheduleType === ScheduleType.Now ?
                        <$g.I18NProvider msg={RMResx.RM_FA_DataOptimize_SaveSuccessful}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider> : RMResx.RM_FA_DataOptimize_SaveSettingSuccessful;

                    if (jobType == RunJobButtonType.ScanJobBtn) {
                        content = (
                            <$g.I18NProvider msg={RMResx.RM_FA_DataOptimize_SaveSuccessful}>
                                <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                            </$g.I18NProvider>
                        );
                    }
                    
                    showToast.success(content);
                    setShowPanel(false);
                    setShowRunForImportPanel(false);
                } else {
                    showToast.error(result.ErrorMessage);
                }
            }).finally(() => $$.loading(false))
        } else {
            setShowPanel(false);
            setShowRunForImportPanel(false);
            showToast.error(RMResx.RM_FA_DataOptimize_SaveConflict);
        }
    });
    
    const runJobMessageBox = (jobType, apiUrl) => {
        let args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: (
                <div>
                    <div className="margin-bottom-m">{jobType === RunJobButtonType.ScanJobBtn ? RMResx.RM_FA_DataOptimize_PreOrScan_ConfirmPopup : RMResx.RM_FA_DataOptimize_Optimize_ConfirmPopup}</div>
                </div>
            ),
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => $$.messagedialog(false)
                },
                {
                    id: "raAnalysisDataOptimizeBtn",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => onSave(jobType, apiUrl),
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    const handleDownloadTemplateRunForImport = () => {
        const downloadTemplate = StringUtil.newGuid();
        const $downloadStatusKey = $("#importDownloadFlag");
        const url = "/api/BCMAdminSettingApi/DownloadArchiverImportTemplate";
        $downloadStatusKey.val(downloadTemplate);

        $("#reco-optimize-form-download")
            .attr("action", url)
            .submit();
    }

    const onKeyDown = (e) => {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    const handleUploadRunForImport = (args) => {
        if (args.isSucceed) {
            args.files[0].fileId = StringUtil.newGuid();
            runForImportUploaderRef.current = args.files[0];
        }
    }

    const handleDeleteRunForImport = (args) => {
        if (args.isSucceed) {
            runForImportUploaderRef.current = null;
        }
    }

    const handleImportRunForImport = () => {
        if (!$$.verify(runForImportValidationRef.current)) {
            return false;
        }
        runJobMessageBox(RunJobButtonType.RunForImportBtn, "/api/RMDiscoveryOffice365OptimizationApi/SaveOptimizationSetting")
    }

    const renderRunForImportPanel = () => {
        const requestVerificationToken = getRequestVerificationToken();
        return (
            <R.Panel
                id="reco-optimize-panel"
                header={RMResx.RM_FA_DataOptimize_RunForImportTitlePanel}
                size={660}
                status={{ show: showRunForImportPanel }}
                destroy={true}
                onHide={() => setShowRunForImportPanel(false)}
            >
                <R.Validation>
                    <div ref={runForImportValidationRef}>
                        <div className="reco-optimize-import-download">
                            <form id="reco-optimize-form-download" method="POST" action="">
                                <input type="hidden" id="importDownloadFlag" name="importDownloadFlag" value="" />
                                <input name='RequestVerificationToken' type='hidden' value={requestVerificationToken} readOnly />
                            </form>
                            <span className="reco-optimize-import-download-span" onClick={handleDownloadTemplateRunForImport} tabIndex="0" onKeyDown={onKeyDown}>
                                {RMResx.RM_FA_DataOptimize_RunForImport_DownloadTemplateBtn}
                            </span>
                        </div>
                        <div>
                            <div className="reco-optimize-import-title" tabIndex="0">
                                <$g.I18NProvider msg={StringUtil.trimEndColon(RMResx.RM_JS_TM_SelectImportFile)} />
                            </div>
                            <div>
                                <R.Validation
                                    element="Uploader"
                                    require={RMResx.RM_JS_BCM_ImportSetting_selectCSVFile}>
                                    <R.Uploader
                                        ref={runForImportUploaderRef}
                                        files={runForImportUploadFiles}
                                        fileTypes={["CSV"]}
                                        onUpload={handleUploadRunForImport}
                                        onDelete={handleDeleteRunForImport}
                                        multiple={false}
                                    />
                                </R.Validation>
                            </div>
                        </div>
                    </div>
                </R.Validation>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => setShowRunForImportPanel(false)} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_FA_DataOptimize_RunForImport_RunNowBtn} onClick={handleImportRunForImport} />
            </R.Panel>
        );
    }

    const handleBackupDataDialog = () => {
        setShowBackupDataDialog(false);
        runJobMessageBox(RunJobButtonType.ArchiverJobBtn, "/api/RMDiscoveryOffice365OptimizationApi/SaveOptimizationSetting");
    }

    const handleOptimizeData = () => {
        let isShowConfirmPopup = dataOptimizeParameter.processActionParameter.archiveOrRemoveVersion === ArchiveOrRemoveVersionType.Remove ||
            dataOptimizeParameter.processActionParameter.archiveOrRemoveFile === ArchiveOrRemoveFileType.Remove ||
            (dataOptimizeParameter.processActionParameter.archiveOrRemoveFile === ArchiveOrRemoveFileType.ArchiveAndRemove && dataOptimizeParameter.processActionParameter.isArchiveVersionOption);
        if (isShowConfirmPopup && isNewOpusTenantAccount) {
            setShowBackupDataDialog(true);
        } else {
            handleBackupDataDialog();
        }
    }

    const renderBackupDataDialog = () => {
        return (
            <R.Dialog
                id="raDeleteSC"
                header={RMResx.RM_JS_Common_Confirmation}
                width={550}
                status={{ show: showBackupDataDialog }}
                destroy
                closeable={false}
            >
                <div>{RMResx.RM_JS_RDM_DestroyDataWithoutBackup}</div>
                <R.Button
                    slot="buttons"
                    classify="blank"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => setShowBackupDataDialog(false)}
                />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={RMResx.RM_JS_Common_OK}
                    onClick={handleBackupDataDialog}
                />
            </R.Dialog>
        );
    }

    return (
        <R.Panel
            id="reco-optimize-panel"
            header={RMResx.RM_FA_DataOptimize_OptimizePanelBtn}
            size={660}
            status={{ show: showPanel }}
            onHide={() => setShowPanel(false)}
            destroy={true}
        >
            <div className="reco-optimize-content">
                <R.Validation>
                    <div ref={validationRef}>
                        <PanelContent
                            dataOptimizeParameter={dataOptimizeParameter}
                            o365TenantId={o365TenantId}
                            onChange={setDataOptimizeParameter}
                        />
                    </div>
                </R.Validation>
            </div>
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => setShowPanel(false)}
                />
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_FA_DataOptimize_PreOrScanBtn}
                    onClick={() => runJobMessageBox(RunJobButtonType.ScanJobBtn, "/api/RMDiscoveryOffice365OptimizationApi/SaveOptimizationPreScanSetting")}
                />
                {viewMode === DiscoveryNodeViewMode.Container && ServiceHelper.CanArchiverImportSC() && (
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_FA_DataOptimize_RunForImportBtn}
                        onClick={() => setShowRunForImportPanel(true)}
                    />
                )}
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={RMResx.RM_FA_DataOptimize_OptimizeBtn}
                    onClick={handleOptimizeData}
                />
                {renderRunForImportPanel()}
                {renderBackupDataDialog()}
            </>
        </R.Panel>
    );
};

export default forwardRef(DataOptimizePanel);