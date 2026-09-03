import { forwardRef, useImperativeHandle, useRef, useState } from "react";
import StringUtil from "../../../../../../Utilities/StringUtil";
import {
    LicenseHelper,
    setCheckedStatus,
    showToast,
} from "../../../../../../Utilities/CommonUtil";
import { DuplicatedRequester } from "../../../requests";
import { RAMessageType } from "../../../../../BCM/ContentRepositoryManagement/Common/CRMCommonUtil";

function DuplicatedFileCleanupPanel({ o365TenantId }, ref) {
    const [showPanel, setShowPanel] = useState(false);

    const [showMessageBarInfo, setShowMessageBarInfo] = useState(true);

    const [siteMappingFiles] = useState([]);

    const [storage, setStorage] = useState(null);

    const [exportLocationList, setExportLocationList] = useState([]);

    const uploaderRef = useRef();

    const filesRef = useRef([]);

    const hasOpusSOLicense = LicenseHelper.HasOpusSOLicense();
    const isTrialLicense = LicenseHelper.IsTrialLicense();

    useImperativeHandle(ref, () => ({
        show: () => {
            setShowPanel(true);
            if (hasOpusSOLicense) {
                getAllActiveExportLocation();
            }
        },
        hide: handleCancel,
    }));

    const getAllActiveExportLocation = () => {
        $$.loading(true);
        const data = {
            PageIndex: -1,
            PageSize: 10,
            SearchValue: "",
            TotalNumber: 0,
        };
        DuplicatedRequester.getAllActiveExportLocation(data)
            .then((res) => {
                const exportList = [];
                const indexDeviceId = res.IndexDeviceId;
                res.StorageDeviceUIDtosList.forEach((item) => {
                    item.Checked = item.Id == indexDeviceId;
                    if (item.Checked) {
                        setStorage(item);
                    }

                    exportList.push(item);
                });
                setExportLocationList(exportList);
            })
            .catch((e) => {
                showToast.error(
                    RMResx.RM_DA_Summary_DuplicateCleanupPanel_AllFailed,
                );
            })
            .finally(() => $$.loading(false));
    };

    const handleKeyDown = (e) => {
        if (e.keyCode == 13) {
            e.target.click();
        }
    };

    const handleExport = () => {
        $$.loading(true);
        DuplicatedRequester.exportDuplicationReport(o365TenantId)
            .then((res) => {
                if (res) {
                    if (res.MessageType === RAMessageType.Successful) {
                        const content = (
                            <$g.I18NProvider
                                msg={RMResx.RM_DSB_Retention_Export_JobStart}
                            >
                                <a className="ra-link-a" href="/Root/JM/Index">
                                    {RMResx.RM_JS_JM_Title}
                                </a>
                                <a
                                    className="ra-link-a"
                                    href="/Root/DC/Download"
                                >
                                    {RMResx.RM_JS_DC_Title}
                                </a>
                            </$g.I18NProvider>
                        );
                        showToast.success(content);
                    } else {
                        showToast.error(res.ErrorMessage);
                    }
                }
            })
            .catch((e) => {
                showToast.error(
                    RMResx.RM_DA_Summary_DuplicateCleanupPanel_AllFailed,
                );
            })
            .finally(() => $$.loading(false));
    };

    const handleCancel = () => {
        setShowPanel(false);
    };

    const handleUpload = (args) => {
        const isSucceed = args.isSucceed;
        if (isSucceed) {
            args.files.forEach((file) => {
                if (!file.fileId) {
                    file.fileId = StringUtil.newGuid();
                }
            });
            filesRef.current = [...args.files];
        }
    };

    const handleDelete = (args) => {
        const isSucceed = args.isSucceed;
        if (isSucceed) {
            filesRef.current = args.files;
        }
    };

    const handleChangeStorageLocation = (args) => {
        const newValue = args.newValue;
        setStorage(newValue);
    };

    const handleSave = () => {
        if (!$$.verify("allValidationForDuplicated")) return false;

        const formData = new FormData();

        filesRef.current.forEach((file, index) => {
            formData.append(
                `duplicatedFileUp${index}`,
                file.file,
                file.fileName,
            );
        });
        formData.append(
            "CleanupInfo",
            JSON.stringify({
                StoragePolicyId: storage.Id,
                StoragePolicyName: storage.Name,
                StoragePolicyType: storage.Type,
            }),
        );
        formData.append("O365TenantId", o365TenantId);
        $$.loading(true);
        fetch(
            "/api/RMDiscoveryOffice365DuplicationDataApi/CleanupDiscoveryDuplication",
            {
                method: "POST",
                body: formData,
            },
        )
            .then(function (response) {
                return response.text().then(function (dataString) {
                    return {
                        responseStatus: response.status,
                        responseString: JSON.parse(dataString),
                    };
                });
            })
            .then(function (res) {
                $$.loading(false);
                const data = res.responseString;
                if (data) {
                    if (data.MessageType == RAMessageType.Successful) {
                        const content = (
                            <$g.I18NProvider
                                msg={
                                    RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage
                                }
                            >
                                <a className="ra-link-a" href="/Root/JM/Index">
                                    {RMResx.RM_JS_JM_Title}
                                </a>
                            </$g.I18NProvider>
                        );
                        showToast.success(content);
                        handleCancel();
                    } else {
                        showToast.error(data.ErrorMessage);
                    }
                }
            });
    };

    const renderDuplicateCleanupConfirmPopup = () => {
        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_JS_JM_DuplicatedDataCleanup_Confirmation,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => {
                        $$.messagedialog(false);
                        handleSave();
                    },
                },
            ],
        });
    } 

    return (
        <R.Panel
            id="reco-rot-duplicated-file-cleanup-panel"
            header={RMResx.RM_DA_Summary_DuplicateCleanupPanel_Title}
            size={660}
            status={{ show: showPanel }}
            onHide={handleCancel}
            destroy
        >
            <R.Validation>
                <div id="allValidationForDuplicated">
                    <div
                        style={{ marginBottom: 30 }}
                        hidden={!showMessageBarInfo}
                    >
                        <R.Messagebar
                            classify="info"
                            message={
                                RMResx.RM_DA_Summary_DuplicateCleanupPanel_InfoBar
                            }
                            status={{ show: showMessageBarInfo }}
                            hasClose
                            onClose={() => setShowMessageBarInfo(false)}
                        />
                    </div>
                    <div className="flex flex-column gap-l">
                        <div tabIndex={0}>
                            {
                                RMResx.RM_DA_Summary_DuplicateCleanupPanel_StepGuide
                            }
                        </div>
                        <ol
                            style={{ margin: 0 }}
                            className="flex flex-column gap-s padding-left-m"
                        >
                            <li>
                                <div tabIndex={0}>
                                    <$g.I18NProvider
                                        msg={
                                            RMResx.RM_DA_Summary_DuplicateCleanupPanel_Step01
                                        }
                                    >
                                        <span
                                            tabIndex={0}
                                            className="reco-duplicated-export-download-span"
                                            onKeyDown={handleKeyDown}
                                            onClick={handleExport}
                                        >
                                            {
                                                RMResx.RM_DA_Summary_DuplicateCleanupPanel_Export
                                            }
                                        </span>
                                    </$g.I18NProvider>
                                </div>
                            </li>
                            <li>
                                <div tabIndex={0}>
                                    <$g.I18NProvider
                                        msg={
                                            RMResx.RM_DA_Summary_DuplicateCleanupPanel_Step02
                                        }
                                    >
                                        <span className="font-semibold">
                                            {
                                                RMResx.RM_DA_Summary_DuplicateCleanupPanel_DupFileList
                                            }
                                        </span>
                                        <span className="font-semibold">
                                            {
                                                RMResx.RM_DA_Summary_DuplicateCleanupPanel_DownLoadCenter
                                            }
                                        </span>
                                    </$g.I18NProvider>
                                </div>
                            </li>
                            <li>
                                <div tabIndex={0}>
                                    {
                                        RMResx.RM_DA_Summary_DuplicateCleanupPanel_Step03
                                    }
                                </div>
                                <ul
                                    style={{ listStyleType: "disc" }}
                                    className="margin-top-s padding-left-m"
                                >
                                    <li tabIndex={0}>
                                        {
                                            RMResx.RM_DA_Summary_DuplicateCleanupPanel_Step03_Archive
                                        }
                                    </li>
                                    <li tabIndex={0}>
                                        {
                                            RMResx.RM_DA_Summary_DuplicateCleanupPanel_Step03_Destroy
                                        }
                                    </li>
                                    <li tabIndex={0}>
                                        {
                                            RMResx.RM_DA_Summary_DuplicateCleanupPanel_Step03_Blank
                                        }
                                    </li>
                                </ul>
                            </li>
                            <li>
                                <div tabIndex={0}>
                                    {
                                        RMResx.RM_DA_Summary_DuplicateCleanupPanel_Step04
                                    }
                                </div>
                            </li>
                        </ol>
                        <div className="flex flex-column gap-xs">
                            <div tabIndex={0} className="font-semibold">
                                {
                                    RMResx.RM_DA_Summary_DuplicateCleanupPanel_UploadReport
                                }
                            </div>
                            <R.Validation
                                element="Uploader"
                                require={
                                    RMResx.RM_PRM_PRE_BulkUpdate_NoImportFileError
                                }
                            >
                                <R.Uploader
                                    ref={uploaderRef}
                                    files={siteMappingFiles}
                                    fileTypes={["CSV"]}
                                    maxSize="100MB"
                                    showMaxSize={true}
                                    showTypes={true}
                                    onUpload={handleUpload}
                                    onDelete={handleDelete}
                                    multiple
                                />
                            </R.Validation>
                        </div>
                        {hasOpusSOLicense && !isTrialLicense && (
                            <div>
                                <div
                                    tabIndex={0}
                                    className="flex align-center font-semibold"
                                >
                                    {RMResx.RM_JS_RDM_CreateRule_ArchiveStorage}
                                    <$g.Popover>
                                        <$g.I18NProvider
                                            msg={
                                                RMResx.RM_JS_RDM_CreateRule_ArchiveStorageTip
                                            }
                                        >
                                            <a
                                                className="ra-link-a"
                                                href="/Root/CP/StorageSettings"
                                            >
                                                {RMResx.RM_JS_CP_StorageSetting}
                                            </a>
                                        </$g.I18NProvider>
                                    </$g.Popover>
                                </div>
                                <R.Validation element="Combobox" require>
                                    <R.Combobox
                                        id="raExportLocationCom"
                                        tooltipField="Name"
                                        width="100%"
                                        textField="Name"
                                        valueField="Id"
                                        checkedField="Checked"
                                        linkMode={false}
                                        searchable={false}
                                        items={setCheckedStatus(
                                            "Id",
                                            "Checked",
                                            exportLocationList,
                                            storage,
                                        )}
                                        onChange={handleChangeStorageLocation}
                                    />
                                </R.Validation>
                            </div>
                        )}
                    </div>
                </div>
            </R.Validation>
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={handleCancel}
                />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    disabled={!hasOpusSOLicense || isTrialLicense}
                    text={RMResx.RM_DA_Summary_DuplicateCleanupPanel_CleanupBtn}
                    onClick={renderDuplicateCleanupConfirmPopup}
                />
            </>
        </R.Panel>
    );
}

export default forwardRef(DuplicatedFileCleanupPanel);
