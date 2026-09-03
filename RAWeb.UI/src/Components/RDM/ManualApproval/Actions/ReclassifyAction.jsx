import { SourceFlags } from "../../../../Constants/Constants";
import { ElecStatusEnum } from "../../../BCM/Constants";
import { RAMessageType } from "../../../BCM/ContentRepositoryManagement/Common/CRMCommonUtil";
import PhyReclassify from "../../../PRM/RecordsExplorer/Components/PhyReclassify";
import { NodeType, ChangeTermOrigin } from "../Constants";

class ReclassifyAction extends R.Component {
    idAttr = true;

    constructor(props) {
        super(props);

        this.state = {
            showReclassifyPanel: false,
            isOverWriteSubFiles: false,
            isReclassifySubFiles: false,
        };

        this.folderReclassifyOption = {
            selectedTableItems: [],
            reclassifyParam: {},
            forceDiscoverAll: false,
            errorCallback: () => {},
        };
        this.notificationCacheData = [];
    }

    handleShowReclassifyMessageBox = (message) => {
        const args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: message,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => $$.messagedialog(false),
                },
            ],
        };
        $$.messagedialog(true, args);
    };

    onCheckOperateLimitSuccess = () => {
        if (this.props.checkedItems.length <= 5000) {
            return true;
        }
        this.handleShowReclassifyMessageBox(
            RMResx.RM_Common_Msg_CheckMoreThanActionLimitCount
        );
        return false;
    };

    onOpenReclassifyPanel = () => {
        if (!this.onCheckOperateLimitSuccess()) return;
        this.setState({ showReclassifyPanel: true });
    };

    getIsIncludeFolderForSp = (checkedTableItem) => {
        const sourcesIncludeFolderForSP = [
            SourceFlags.SP,
            SourceFlags.OneDrive,
            SourceFlags.Teams,
        ];
        return (
            sourcesIncludeFolderForSP.includes(checkedTableItem.sourceFlag) &&
            checkedTableItem.nodeType == NodeType.Folder
        );
    };

    showFolderReclassifyOption = (content) => {
        const {
            reclassifyParam,
            selectedTableItems,
            forceDiscoverAll,
            errorCallback,
        } = this.folderReclassifyOption;
        const args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_BCM_Explorer_ChangeTerm,
            content,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => $$.messagedialog(false),
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.sendRunJobReclassifyRequest.bind(
                        this,
                        reclassifyParam,
                        selectedTableItems,
                        forceDiscoverAll
                    ),
                },
            ],
        };
        $$.messagedialog(true, args);
    };

    getReclassifyOverWriteSubFilesCheckbox = () => {
        return (
            <R.Checkbox
                name="checkbox-fs-folder-opt"
                text={RMResx.RM_JS_BCM_IncludeAllFileUnderFolder_Message}
                title={RMResx.RM_JS_BCM_IncludeAllFileUnderFolder_Message}
                checked={this.state.isOverWriteSubFiles}
                onChange={(checked) =>
                    this.setState({ isOverWriteSubFiles: checked })
                }
            />
        );
    };

    getReclassifySetOptions = (checkedTableItems) => {
        let reclassifySubFilesCheckbox = <></>;
        let reclassifyOverWriteSubFiles = <></>;
        let isIncludeFolderForSp = this.getIsIncludeFolderForSp(
            checkedTableItems[0]
        );
        if (isIncludeFolderForSp) {
            reclassifySubFilesCheckbox = (
                <div>
                    <R.Checkbox
                        name="checkbox-sp-folder-opt"
                        text={RMResx.RM_HS_Msg_ReclassifyWithFolder}
                        title={RMResx.RM_HS_Msg_ReclassifyWithFolder}
                        checked={this.state.isReclassifySubFiles}
                        onChange={(checked) => {
                            this.setState(
                                {
                                    isReclassifySubFiles: checked,
                                    isOverWriteSubFiles: false,
                                },
                                () => {
                                    const content = (
                                        <div>
                                            <div>
                                                {this.getReclassifySetOptions(
                                                    checkedTableItems
                                                )}
                                            </div>
                                        </div>
                                    );
                                    this.showFolderReclassifyOption(content);
                                }
                            );
                        }}
                    />
                </div>
            );
            if (this.state.isReclassifySubFiles) {
                reclassifyOverWriteSubFiles = (
                    <div className="margin-top-s margin-left-l">
                        {this.getReclassifyOverWriteSubFilesCheckbox()}
                    </div>
                );
            }
        } else {
            reclassifyOverWriteSubFiles = this.getReclassifyOverWriteSubFilesCheckbox();
        }

        return (
            <div>
                <div className="margin-bottom-l">
                    {RMResx.RM_JS_BCM_ChangeTermForFolder_Message}
                </div>
                {reclassifySubFilesCheckbox}
                {reclassifyOverWriteSubFiles}
            </div>
        );
    };

    getGoogleReclassifySetOptions = () => {
        return (
            <div>
                <div className="margin-bottom-l">
                    {RMResx.RM_JS_BCM_ChangeTermForFolder_Message}
                </div>
                <R.Checkbox
                    name="checkbox-google-folder-opt"
                    text={
                        RMResx.RM_JS_BCM_Label_IncludeAllFileUnderFolder_Message
                    }
                    title={
                        RMResx.RM_JS_BCM_Label_IncludeAllFileUnderFolder_Message
                    }
                    checked={this.state.isOverWriteSubFiles}
                    onChange={(checked) =>
                        this.setState({ isOverWriteSubFiles: checked })
                    }
                />
            </div>
        );
    };

    handleError = (response) => {
        $$.loading(false);
        if (response.status == 403) {
            response.text().then((errorMessage) => {
                let messageDialogContent =
                    RMResx.RM_JS_Common_NoPermissionLicense;
                if (
                    errorMessage &&
                    errorMessage.includes("User have no sp access")
                ) {
                    messageDialogContent =
                        RMResx.RM_JS_Common_NoSharepointPermissionLicense;
                }
                $$.messagedialog(true, {
                    classify: "warn",
                    width: "550px",
                    hideActions: false,
                    title: RMResx.RM_JS_Common_Confirmation,
                    content: messageDialogContent,
                    buttons: [
                        {
                            text: RMResx.RM_JS_Common_OK,
                            primary: true,
                            classify: "theme",
                            onClick: () => {
                                $$.messagedialog(false);
                            },
                        },
                    ],
                });
            });
        }
    };

    // Notification related methods
    getNotificationTitleMsg = (_jobId, _data, _isDeclare, status) => {
        const actionString = RMResx.RM_JS_Notifi_Action_Reclassification;
        let statusString = "";

        // Commented the code below because in this release, we only have reclassification action. RECO-34279
        // if (jobId.startsWith("UT") || jobId.startsWith("UL")) {
        //     actionString = RMResx.RM_JS_Notifi_Action_Reclassification;
        // } else if (jobId.startsWith("PM")) {
        //     actionString = RMResx.RM_JS_Notifi_Action_Move;
        // } else {
        //     if (isDeclare) {
        //         actionString = RMResx.RM_JS_Notifi_Action_Declare;
        //     } else {
        //         actionString = RMResx.RM_JS_Notifi_Action_Undeclare;
        //     }
        // }
        switch (status) {
            case ElecStatusEnum.Failed:
                statusString = RMResx.RM_JS_Notifi_Status_Failed;
                break;
            case ElecStatusEnum.Completed:
                statusString = RMResx.RM_JS_Notifi_Status_Competed;
                break;
            case ElecStatusEnum.Exception:
                statusString = RMResx.RM_JS_Notifi_Status_Exception;
                break;
            default:
                statusString = RMResx.RM_JS_Notifi_Status_Running;
                break;
        }
        return `${actionString} ${statusString}`;
    };

    handleDeleteNotification = (index) => {
        this.notificationCacheData = this.notificationCacheData.filter(
            (_, idx) => {
                return index != idx;
            }
        );
        const notificationHtml = this.getNotificationHtml();
        this.dispatch(
            "raNotification",
            notificationHtml,
            this.notificationCacheData
        );
    };

    getNotificationHtml = () => {
        const notificationItems = RM.deepcopy(this.notificationCacheData);
        return (
            <div>
                {notificationItems.map((item, index) => {
                    return (
                        <div key={index}>
                            <div className="notification-space"></div>
                            <div className="ra-elec-notification-conetnt">
                                <div className="ra-elec-notification-msg">
                                    {item.msg}
                                </div>
                                <div className="flex">
                                    <div
                                        className="fia-searchbox-close notify-icon"
                                        onClick={() =>
                                            this.handleDeleteNotification(index)
                                        }
                                        onKeyDown={(e) => {
                                            if (e.keyCode == 13) {
                                                e.target.click();
                                            }
                                        }}
                                    ></div>
                                </div>
                            </div>
                            <div className="ra-elec-notification-items">
                                {item.selectedElecItems
                                    .slice(0, 3)
                                    .map((item, index) => {
                                        return (
                                            <div
                                                key={index}
                                                className="ra-elec-notification-item"
                                            >
                                                <div>{item.leafName}</div>
                                            </div>
                                        );
                                    })}
                                {item.selectedElecItems.length > 3 && (
                                    <div
                                        key={index}
                                        className="ra-elec-notification-item"
                                    >
                                        <div>{`...(${
                                            item.selectedElecItems.length - 3
                                        })`}</div>
                                    </div>
                                )}
                            </div>
                            <div className="ra-elec-notification-time">
                                {item.status ==
                                    RMResx.RM_JS_Notifi_Status_Running && (
                                    <div className="fia-in-progress"></div>
                                )}
                                {item.status ==
                                    RMResx.RM_JS_Notifi_Status_Competed && (
                                    <div className="fia-checkbox-device completed"></div>
                                )}
                                {item.status ==
                                    RMResx.RM_JS_Notifi_Status_Failed && (
                                    <div className="fia-status-error"></div>
                                )}
                                {item.status ==
                                    RMResx.RM_JS_Notifi_Status_Exception && (
                                    <div className="fia-status-error"></div>
                                )}
                                <span className="ra-elec-notification-showTime">
                                    {item.showTime}
                                </span>
                            </div>
                        </div>
                    );
                })}
            </div>
        );
    };

    getNotificationMenuMsgHtml = (notificationMsg, selectedElecItems) => {
        let notificationMenuItems = RM.deepcopy(selectedElecItems);
        if (notificationMenuItems.length > 3) {
            const ellipsisStr = `...(${notificationMenuItems.length - 3})`;
            notificationMenuItems = notificationMenuItems.slice(0, 3);
            notificationMenuItems.push({ leafName: ellipsisStr });
        }
        return (
            <div className="right">
                <div className="nTitle" tabIndex="0">
                    <div>{notificationMsg}</div>
                </div>
                <div className="nBody">
                    {notificationMenuItems.map((item, key) => {
                        return (
                            <div
                                className="nDescription"
                                tabIndex="0"
                                key={key}
                            >
                                {item.leafName}
                            </div>
                        );
                    })}
                </div>
            </div>
        );
    };

    handleNotificationUpdate = (
        jobId,
        isDeclare,
        selectedElecItems,
        statusEnum,
        statusText
    ) => {
        const completeNotificationCache = [];
        const failedNotificationCache = [];
        const exceptionNotificationCache = [];
        const notificationCache = [];
        const endTime = new Date();

        $$.loading(false);

        for (let item of this.notificationCacheData) {
            if (item.jobId == jobId) {
                item.msg = this.getNotificationTitleMsg(
                    jobId,
                    selectedElecItems,
                    isDeclare,
                    statusEnum
                );
                item.status = statusText;
                item.showTime = RM.TimeUtil.dateToStringSimplifyTimeZone(
                    endTime,
                    RM.TimeUtil.getGlobalTimezoneInfo()
                );
            }

            switch (item.status) {
                case RMResx.RM_JS_Notifi_Status_Competed:
                    completeNotificationCache.push(item);
                    break;
                case RMResx.RM_JS_Notifi_Status_Failed:
                    failedNotificationCache.push(item);
                    break;
                case RMResx.RM_JS_Notifi_Status_Exception:
                    exceptionNotificationCache.push(item);
                    break;
                default:
                    notificationCache.push(item);
            }
        }

        this.notificationCacheData = [
            ...notificationCache,
            ...failedNotificationCache,
            ...completeNotificationCache,
            ...exceptionNotificationCache,
        ];

        const notificationMenuMsg = this.getNotificationTitleMsg(
            jobId,
            selectedElecItems,
            isDeclare,
            statusEnum
        );
        const notificationMenuHtml = this.getNotificationMenuMsgHtml(
            notificationMenuMsg,
            selectedElecItems
        );
        const notificationHtml = this.getNotificationHtml();
        this.dispatch("raNotification", notificationHtml);
        this.dispatch("raNotificationMenu", notificationMenuHtml, statusEnum);
    };

    updateNotificationTimer = (jobId, _type, isDeclare, selectedElecItems) => {
        // _type: Used for determining action type. E.g: "reclassify", ... (Refer Hybrid search page)
        // Check if any nodes in the notification panel have been dismissed. If they have, clear the notificationCacheData.
        if ($(".rm-notification-content").children().length == 0) {
            this.notificationCacheData = [];
        }
        const startTime = new Date();
        let timerCount = 0;
        const notificationMsg = this.getNotificationTitleMsg(
            jobId,
            selectedElecItems,
            isDeclare,
            ElecStatusEnum.InProgress
        );
        const notificationMenuMsg = this.getNotificationTitleMsg(
            jobId,
            selectedElecItems,
            isDeclare,
            ElecStatusEnum.InProgress
        );
        const notificationItem = {
            msg: notificationMsg,
            selectedElecItems: selectedElecItems,
            status: RMResx.RM_JS_Notifi_Status_Running,
            jobId: jobId,
            showTime: RM.TimeUtil.dateToStringSimplifyTimeZone(
                startTime,
                RM.TimeUtil.getGlobalTimezoneInfo()
            ),
        };
        this.notificationCacheData.push(notificationItem); // Add a new item each time the listener is called.
        const notificationHtml = this.getNotificationHtml();
        const notificationMenuHtml = this.getNotificationMenuMsgHtml(
            notificationMenuMsg,
            selectedElecItems
        );
        this.dispatch("raNotification", notificationHtml);
        this.dispatch("rmSuiteBar");
        this.dispatch(
            "raNotificationMenu",
            notificationMenuHtml,
            ElecStatusEnum.InProgress
        );
        const updateChangeTerm = setInterval(() => {
            ++timerCount;
            if (jobId) {
                const option = {
                    url: `/api/RecordsExplorerApi/GetRealTimeJobStatusInfo?jobId=${jobId}`,
                    method: "GET",
                };
                fetchUtility(option).then((result) => {
                    const msg = JSON.parse(result);
                    let stopTimer = false;
                    if (timerCount === 60 * 10) {
                        // 10 min
                        stopTimer = true;
                    }

                    switch (msg.MessageType) {
                        case RAMessageType.Failed:
                            stopTimer = true;
                            this.handleNotificationUpdate(
                                jobId,
                                isDeclare,
                                selectedElecItems,
                                ElecStatusEnum.Failed,
                                RMResx.RM_JS_Notifi_Status_Failed
                            );
                            break;
                        case RAMessageType.Exception:
                            stopTimer = true;
                            this.handleNotificationUpdate(
                                jobId,
                                isDeclare,
                                selectedElecItems,
                                ElecStatusEnum.Failed,
                                RMResx.RM_JS_Notifi_Status_Exception
                            );
                            break;
                        default:
                            if (msg.Items && msg.Status === 4) {
                                stopTimer = true;
                                this.handleNotificationUpdate(
                                    jobId,
                                    isDeclare,
                                    selectedElecItems,
                                    ElecStatusEnum.Completed,
                                    RMResx.RM_JS_Notifi_Status_Competed
                                );
                            }
                            break;
                    }

                    // Stop this timer
                    if (stopTimer) {
                        clearInterval(updateChangeTerm);
                        // this.loadData();
                        this.props.onReload?.();
                    }
                });
            }
        }, 1000);
    };

    googleUpdateNotificationTimer = (jobId, selectedElecItems) => {
        // Check if any nodes in the notification panel have been dismissed. If they have, clear the notificationCacheData.
        if ($(".rm-notification-content").children().length == 0) {
            this.notificationCacheData = [];
        }
        const startTime = new Date();
        const notificationMsg = this.getNotificationTitleMsg(
            jobId,
            selectedElecItems,
            false,
            ElecStatusEnum.InProgress
        );
        const notificationMenuMsg = this.getNotificationTitleMsg(
            jobId,
            selectedElecItems,
            false,
            ElecStatusEnum.InProgress
        );
        const notificationItem = {
            msg: notificationMsg,
            selectedElecItems: selectedElecItems,
            status: RMResx.RM_JS_Notifi_Status_Running,
            jobId: jobId,
            startTime: startTime.toLocaleTimeString(),
            showTime: RM.TimeUtil.dateToStringSimplifyTimeZone(
                startTime,
                RM.TimeUtil.getGlobalTimezoneInfo()
            ),
        };
        this.notificationCacheData.push(notificationItem);
        const notificationHtml = this.getNotificationHtml();
        const notificationMenuHtml = this.getNotificationMenuMsgHtml(
            notificationMenuMsg,
            selectedElecItems
        );
        this.dispatch("raNotification", notificationHtml);
        this.dispatch("rmSuiteBar");
        this.dispatch(
            "raNotificationMenu",
            notificationMenuHtml,
            ElecStatusEnum.InProgress
        );

        const startReclassifyTime = Date.now();
        const _this = this;

        // IIFE
        (function updateChangeTerm() {
            if (jobId) {
                const option = {
                    url: `/api/RecordsExplorerApi/GetRealTimeJobStatusInfo?jobId=${jobId}`,
                    method: "GET",
                };
                fetchUtility(option).then((result) => {
                    const data = JSON.parse(result);
                    let stopTimer = false;

                    switch (data.MessageType) {
                        case RAMessageType.Failed:
                            stopTimer = true;
                            _this.handleNotificationUpdate(
                                jobId,
                                false,
                                selectedElecItems,
                                ElecStatusEnum.Failed,
                                RMResx.RM_JS_Notifi_Status_Failed
                            );
                            break;
                        case RAMessageType.Exception:
                            stopTimer = true;
                            _this.handleNotificationUpdate(
                                jobId,
                                false,
                                selectedElecItems,
                                ElecStatusEnum.Failed,
                                RMResx.RM_JS_Notifi_Status_Exception
                            );
                            break;
                        default:
                            if (data.Items && data.Status == 4) {
                                stopTimer = true;
                                _this.handleNotificationUpdate(
                                    jobId,
                                    false,
                                    selectedElecItems,
                                    ElecStatusEnum.Completed,
                                    RMResx.RM_JS_Notifi_Status_Competed
                                );
                            }
                            break;
                    }

                    if (
                        stopTimer ||
                        Date.now() - startReclassifyTime >= 10 * 60000
                    ) {
                        // 60000ms = 1 minute
                        // Stop reclassification if it fails, succeeds, or runs for over 10 minutes.
                        // _this.loadData();
                        _this.props.onReload?.();
                    } else {
                        // function will be called after 1 second
                        setTimeout(updateChangeTerm, 1000);
                    }
                });
            }
        })();
    };

    // Can use for both reclassify and google reclassify, depends on "endpoint" param
    showMessageTip = (type, msg) => {
        const option = { content: msg, classify: type };
        $$.toast(option);
    };

    sendRunJobReclassifyRequest = (
        reclassifyParam,
        selectedTableItems,
        forceDiscoverAll
    ) => {
        // $$.messagedialog(false);
        reclassifyParam.OverWriteSubFiles = this.state.isOverWriteSubFiles;
        reclassifyParam.ReclassifySubFiles = this.state.isReclassifySubFiles;
        let sourceFlag = 0;
        if (forceDiscoverAll) {
            sourceFlag = selectedTableItems[0].sourceFlag;
        } else {
            const mapping = [
                ["RecordIds", SourceFlags.SP],
                ["EXORecordIds", SourceFlags.Exo],
                ["FSRecordIds", SourceFlags.FS],
                ["PhyRecordIds", SourceFlags.Phy],
                ["OneDriveRecordIds", SourceFlags.OneDrive],
                ["GoogleDriveRecordIds", SourceFlags.Google],
                ["TeamsRecordIds", SourceFlags.Teams],
            ];

            for (let [key, flag] of mapping) {
                if (reclassifyParam[key]?.length > 0) {
                    sourceFlag = flag;
                    break;
                }
            }

            if (
                !sourceFlag &&
                reclassifyParam.CustomizeConnectorRecordIds?.length > 0
            ) {
                sourceFlag = selectedTableItems[0].sourceFlag;
            }
        }

        const filters = this.props.filterDefinitions || [];
        const searchFilterDefinition = this.props.searchFilterDefinition;
        if (searchFilterDefinition) {
            filters.push(searchFilterDefinition);
        }
        reclassifyParam.IsManualData = true;
        const params = {
            IsRealTimeAction: false,
            FilterInfo: {
                Values: filters
            },
            Action: 1, // Reclassify action
            ActionExtension: reclassifyParam,
            SourceFlag: sourceFlag,
            ForceDiscoverAll: forceDiscoverAll,
            RecordIds: selectedTableItems.map((i) => i.id),
            ChangeTermOrigin: ChangeTermOrigin.Manual
        };
        const option = {
            url: "/api/ManualApproval/DoAction",
            method: "POST",
            data: params,
        };
        $$.loading(true);
        fetchUtility(option, (response) => {
            this.handleError(response);
        }).then((resultJson) => {
            if (resultJson.MessageType == "0") {
                if (resultJson.Extension) {
                    this.showMessageTip(
                        "success",
                        <$g.I18NProvider
                            msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}
                        >
                            <a className="ra-link-a" href="/Root/JM/Index">
                                {RMResx.RM_JS_JM_Title}
                            </a>
                        </$g.I18NProvider>
                    );
                }
            } else {
                this.showMessageTip("error", resultJson.ErrorMessage);
            }
            this.setState({ showReclassifyPanel: false });
        }).finally(() => $$.loading(false));
    };

    sendReclassifyRequest = (
        checkedTableItems,
        reclassifyParam,
        errorCallback,
        endpoint = "reclassify"
    ) => {
        $$.loading(true);
        const urlMap = {
            reclassify: "/api/ManualApproval/ChangeTerm",
            googleReclassify: "/api/ManualApproval/ChangeLabel",
        };
        const url = urlMap[endpoint];
        reclassifyParam.ChangeTermOrigin = ChangeTermOrigin.Manual;
        const option = {
            url: url,
            method: "POST",
            data: reclassifyParam,
        };
        fetchUtility(option, (response) => {
            this.handleError(response);
        }).then((result) => {
            $$.loading(false);
            const resultData = JSON.parse(result);
            if (resultData.MessageType == 1) {
                errorCallback(resultData.ErrorMessage);
            } else {
                if (endpoint === "googleReclassify") {
                    this.googleUpdateNotificationTimer(
                        resultData.Extension,
                        checkedTableItems
                    );
                } else {
                    this.updateNotificationTimer(
                        resultData.Extension,
                        "reclassify",
                        false,
                        checkedTableItems
                    );
                }
                this.setState({ showReclassifyPanel: false });
            }
        });
    };

    // Separate method to handle Google reclassify action
    handleGoogleReclassify = (
        forceDiscoverAll,
        checkedTableItems,
        googleReclassifyParam,
        errorCallback
    ) => {
        const nodeId = new Set();
        checkedTableItems.forEach((item) => {
            if (item.ScopeId && !nodeId.has(item.ScopeId)) {
                nodeId.add(item.ScopeId);
            }
        });
        googleReclassifyParam.NodeId = JSON.stringify(Array.from(nodeId));

        const containsGoogleFolder = checkedTableItems.find((item) => {
            return item.NodeType == NodeType.GoogleDriveFolder;
        });

        if (containsGoogleFolder) {
            this.folderReclassifyOption = {
                selectedTableItems: checkedTableItems,
                reclassifyParam: googleReclassifyParam,
                forceDiscoverAll:
                    this.props.isCheckedAll &&
                    this.props.canDoActionForReclassify,
                errorCallback,
            };
            this.setState({
                isOverWriteSubFiles: false,
                isReclassifySubFiles: false,
            });
            // const content = (
            //     <div>
            //         <div>{this.getGoogleReclassifySetOptions()}</div>
            //     </div>
            // );
            // this.showFolderReclassifyOption(content);
            this.sendRunJobReclassifyRequest(
                googleReclassifyParam,
                checkedTableItems,
                forceDiscoverAll,
            );
        } else if (forceDiscoverAll) {
            this.sendRunJobReclassifyRequest(
                googleReclassifyParam,
                checkedTableItems,
                forceDiscoverAll
            );
        } else {
            this.sendReclassifyRequest(
                checkedTableItems,
                googleReclassifyParam,
                errorCallback,
                "googleReclassify"
            );
        }
    };

    handleSaveReclassify = () => {
        const { isCheckedAll, canDoActionForReclassify } = this.props;
        const forceDiscoverAll = isCheckedAll && canDoActionForReclassify;
        const callback = (termData, errorCallback) => {
            let validateFailed = false;
            if (
                termData.Type == "Root" ||
                termData.Type == "TermGroup" ||
                termData.Type == "TermSet"
            ) {
                validateFailed = true;
                errorCallback(
                    RMResx.RM_JS_PRM_Msg_ReclassifyNoSelecteTermLevel
                );
            }
            if (validateFailed) {
                return false;
            }
            const reclassifyParam = {
                RecordIds: [],
                EXORecordIds: [],
                FSRecordIds: [],
                PhyRecordIds: [],
                SPOnPremRecordIds: [],
                OneDriveRecordIds: [],
                AzureFileShareRecordIds: [],
                BoxRecordIds: [],
                CustomizeConnectorRecordIds: [],
                TeamsRecordIds: [],
                TermInfo: {
                    Id: termData.Id,
                    Name: termData.Name,
                    UniqueId: termData.UniqueId,
                },
                Comment: termData.Comment,
                CanReclassifyAllTerm : true,
            };

            const googleReclassifyParam = {
                GoogleDriveRecordIds: [],
                TermInfo: {
                    Id: termData.Id,
                    Name: termData.Name,
                    UniqueId: termData.UniqueId,
                },
                Comment: termData.Comment,
            };

            const checkedTableItems = _.cloneDeep(this.props.checkedItems);

            const containsFolder = checkedTableItems.find((item) => {
                return (
                    this.getIsIncludeFolderForSp(item) ||
                    item.nodeType == NodeType.FSFolder
                );
            });

            for (let item of checkedTableItems) {
                switch (item.sourceFlag) {
                    case SourceFlags.Exo:
                        reclassifyParam.EXORecordIds.push(item.id);
                        break;
                    case SourceFlags.FS:
                        reclassifyParam.FSRecordIds.push(item.id);
                        break;
                    case SourceFlags.Phy:
                        reclassifyParam.PhyRecordIds.push(item.id);
                        break;
                    case SourceFlags.SPLocal:
                        reclassifyParam.SPOnPremRecordIds.push(item.id);
                        break;
                    case SourceFlags.OneDrive:
                        reclassifyParam.OneDriveRecordIds.push(item.id);
                        break;
                    case SourceFlags.AzureFile:
                        reclassifyParam.AzureFileShareRecordIds.push(item.id);
                        break;
                    case SourceFlags.Box:
                        reclassifyParam.BoxRecordIds.push(item.id);
                        break;
                    case SourceFlags.Google:
                        googleReclassifyParam.GoogleDriveRecordIds.push(
                            item.id
                        );
                        break;
                    case SourceFlags.Teams:
                        reclassifyParam.TeamsRecordIds.push(item.id);
                        break;
                    default:
                        if (item.sourceFlag >= 1000) {
                            reclassifyParam.CustomizeConnectorRecordIds.push(
                                item.id
                            );
                        } else {
                            reclassifyParam.RecordIds.push(item.id);
                        }
                        break;
                }
            }

            // Google part
            const isGoogleReclassify =
                this.props.checkedItems.length > 0 &&
                this.props.checkedItems.every(
                    (item) =>
                        item.nodeType == NodeType.GoogleDriveFolder ||
                        item.nodeType == NodeType.GoogleDriveFile
                );

            if (isGoogleReclassify) {
                this.handleGoogleReclassify(
                    forceDiscoverAll,
                    checkedTableItems,
                    googleReclassifyParam,
                    errorCallback
                );
                return;
            }
            // End: Google part

            if (containsFolder) {
                this.folderReclassifyOption = {
                    selectedTableItems: checkedTableItems,
                    reclassifyParam: reclassifyParam,
                    forceDiscoverAll,
                    errorCallback: errorCallback,
                };
                this.setState({
                    isOverWriteSubFiles: false,
                    isReclassifySubFiles: false,
                });
                // const content = (
                //     <div>
                //         <div>
                //             {this.getReclassifySetOptions(checkedTableItems)}
                //         </div>
                //     </div>
                // );
                // this.showFolderReclassifyOption(content);
                this.sendRunJobReclassifyRequest(
                    reclassifyParam,
                    checkedTableItems,
                    forceDiscoverAll
                );
            } else if (forceDiscoverAll) {
                this.sendRunJobReclassifyRequest(
                    reclassifyParam,
                    checkedTableItems,
                    forceDiscoverAll
                );
            } else {
                this.sendReclassifyRequest(
                    checkedTableItems,
                    reclassifyParam,
                    errorCallback
                );
            }
        };
        this.dispatch("rdmReclassify", "onSave", callback);
    };

    render() {
        const { showReclassifyPanel } = this.state;
        const { checkedItems } = this.props;

        return (
            <R.Panel
                id="reclassifyPanel"
                header={RMResx.RM_JS_BCM_Explorer_ChangeTerm}
                size={664}
                status={{ show: showReclassifyPanel }}
                destroy={true}
                onHide={() => this.setState({ showReclassifyPanel: false })}
            >
                <div className="ra-panel-content reclassify-panel">
                    <div id="reclassify-content">
                        <PhyReclassify
                            id="rdmReclassify"
                            data={checkedItems}
                            isRequireComment
                            displayingPage="recordForReview"
                        ></PhyReclassify>
                    </div>
                </div>
                <>
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_JS_Common_Cancel}
                        onClick={() =>
                            this.setState({ showReclassifyPanel: false })
                        }
                    />
                    <R.Button
                        slot="buttons"
                        primary
                        classify="theme"
                        text={RMResx.RM_JS_Common_Save}
                        onClick={this.handleSaveReclassify}
                    />
                </>
            </R.Panel>
        );
    }
}

export default ReclassifyAction;
