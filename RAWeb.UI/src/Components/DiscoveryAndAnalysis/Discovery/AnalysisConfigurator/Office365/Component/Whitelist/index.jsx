import React, { useEffect, useMemo, useRef, useState } from "react";
import _ from "lodash";

import { SimplePager } from "../../../../../../Common/Pager";
import AddWhitelist from "./AddWhitelist";
import WhitelistTable from "./WhitelistTable";
import StringUtil from "../../../../../../../Utilities/StringUtil";
import { showToast } from "../../../../../../../Utilities/CommonUtil";
import { useStableCallback } from "../../../../../../BCM/AzureFileShareConfigureConnection/Hooks";
import TopButtonsComponent from "../../../../../../Common/Util/TopButtonsComponent";

const Whitelist = (props) => {
    const { onClosePanel } = props;

    const refTopButtons = useRef();

    const uploaderRef = useRef();

    const filesRef = useRef();

    const addWhitelistTableRef = useRef();

    const whitelistTableRef = useRef();

    const [whitelistData, setWhitelistData] = useState([]);

    const [showImportDialog, setShowImportDialog] = useState(false);

    const [showWhitelistDialog, setShowWhitelistDialog] = useState(false);

    const [whitelistFiles] = useState([]);

    const [checkedItems, setCheckedItems] = useState([]);

    const [totalCount, setTotalCount] = useState(0);

    const [pageInfo, setPageInfo] = useState({
        PageIndex: 0,
        PageSize: 10,
    });

    const menuButtons = useMemo(() => {
        return [
            {
                isStatic: true,
                id: "raAddBtn",
                name: RMResx.RM_AR_RC_Whitelist_AddBtn,
                onClick: () => setShowWhitelistDialog(true),
            },
            {
                id: "raExportBtn",
                name: RMResx.RM_AR_RC_Whitelist_ExportBtn,
                icon: "fia-export-settings",
                onClick: () => {
                    handleExportWhitelist();
                },
            },
            {
                id: "raImportBtn",
                name: RMResx.RM_AR_RC_Whitelist_ImportBtn,
                icon: "fia-import",
                onClick: () => setShowImportDialog(true),
            },
        ];
    }, []);

    const deleteButton = useMemo(() => {
        return [
            {
                id: "raDeleteBtn",
                name: RMResx.RM_AR_RC_Whitelist_RemoveBtn,
                icon: "fia-delete",
                onClick: () => {
                    handleDeleteWhitelist();
                },
            },
        ];
    }, []);

    useEffect(() => {
        getWhitelist(false);
    }, [pageInfo.PageIndex]);

    const getWhitelist = async (isResetPagerIndex) => {
        $$.loading(true);
        const clonePageInfo = _.cloneDeep(pageInfo);
        if (isResetPagerIndex) {
            clonePageInfo.PageIndex = 0;
            clonePageInfo.PageSize = 10;
            setCheckedItems([]);
        }
        const requestOption = {
            url: "/api/RMDiscoveryOffice365SpecificSiteApi/GetExclusionListSitesByPagination",
            data: clonePageInfo,
        };
        const result = await fetchUtility(requestOption);
        setWhitelistData(result.SiteCollections);
        setTotalCount(result.TotalCount);
        setPageInfo(clonePageInfo);
        whitelistTableRef.current &&
            whitelistTableRef.current.setTableInfo({
                items: result.SiteCollections,
                isReset: isResetPagerIndex,
            });
        $$.loading(false);
    };

    // Add whitelist
    const onSaveAddWhitelist = async () => {
        if (
            addWhitelistTableRef.current &&
            !addWhitelistTableRef.current.isValid()
        ) {
            return false;
        }

        const addWhitelist =
            addWhitelistTableRef.current &&
            addWhitelistTableRef.current.getAddWhitelist();
        $$.loading(true);
        const requestOption = {
            url: "/api/RMDiscoveryOffice365SpecificSiteApi/AddExcludeSites",
            data: addWhitelist,
        };
        const response = await fetchUtility(requestOption);
        $$.loading(false);
        if (response.MessageType === 0) {
            setShowWhitelistDialog(false);
            showToast.success(RMResx.RM_AR_RC_AddBlacklist_Successful);
            getWhitelist(true);
        } else {
            showToast.error(response.ErrorMessage);
        }
    };

    // Export whitelist
    const onExportWhitelist = async () => {
        $$.messagedialog(false);
        $$.loading(true);
        const response = await fetchUtility({
            url: "/api/RMDiscoveryOffice365SpecificSiteApi/ExportSCExcludelist",
        });
        $$.loading(false);
        if (response.MessageType === 0) {
            const content = (
                <$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
                    <a className="ra-link-a" href="/Root/JM/Index">
                        {RMResx.RM_JS_JM_Title}
                    </a>
                    <a className="ra-link-a" href="/Root/DC/Download">
                        {RMResx.RM_JS_DC_Title}
                    </a>
                </$g.I18NProvider>
            );
            showToast.success(content);
        } else {
            showToast.error(response.ErrorMessage);
        }
        onClosePanel();
    };

    const handleExportWhitelist = () => {
        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_AR_RC_ExportBlacklist_ConfirmMsg,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => {
                        $$.messagedialog(false);
                    },
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: onExportWhitelist,
                },
            ],
        });
    };

    // Import whitelist
    const onUpload = (args) => {
        const isSucceed = args.isSucceed;
        if (isSucceed) {
            args.files[0].fileId = StringUtil.newGuid();
            filesRef.current = args.files[0];
        }
    };

    const onDelete = (args) => {
        if (args.isSucceed) {
            filesRef.current = null;
        }
    };

    const onImport = () => {
        if (!$$.verify("allValidation")) return false;

        $$.loading(true);
        const formData = new FormData();
        const url = "/api/RMDiscoveryOffice365SpecificSiteApi/ImportExcludeSClist";
        formData.append(
            "fileUp",
            filesRef.current.file,
            filesRef.current.fileName,
        );
        fetch(url, {
            method: "POST",
            body: formData,
        })
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
                if (res.responseString.MessageType == 0) {
                    let content = (
                        <$g.I18NProvider
                            msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}
                        >
                            <a className="ra-link-a" href="/Root/JM/Index">
                                {RMResx.RM_JS_JM_Title}
                            </a>
                        </$g.I18NProvider>
                    );
                    showToast.success(content);
                    setShowImportDialog(false);
                    onClosePanel();
                } else {
                    showToast.error(res.responseString.ErrorMessage);
                }
            });
    };

    // Delete whitelist
    const onDeleteWhitelist = useStableCallback(async (id) => {
        const clonedCheckedItems = _.cloneDeep(checkedItems);
        let itemIds = clonedCheckedItems.map((item) => item.Id);
        if (id) {
            itemIds = [id];
        }
        $$.messagedialog(false);
        $$.loading(true);
        const requestOption = {
            url: "/api/RMDiscoveryOffice365SpecificSiteApi/RemoveExclusionListSites",
            data: itemIds,
        };
        const response = await fetchUtility(requestOption);
        $$.loading(false);
        if (response.MessageType === 0) {
            showToast.success(RMResx.RM_AR_RC_DeleteBlacklist_Successful);
            getWhitelist(true);
        } else {
            showToast.error(response.ErrorMessage);
        }
    });

    const handleDeleteWhitelist = (id) => {
        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_AR_RC_DeleteBlacklist_ConfirmMsg,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => {
                        $$.messagedialog(false);
                    },
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => onDeleteWhitelist(id),
                },
            ],
        });
    };

    const onItemsCheckedChange = (checkedItems) => {
        if (checkedItems.length > 0) {
            refTopButtons.current.updateButtons([
                ...menuButtons,
                ...deleteButton,
            ]);
        } else {
            refTopButtons.current.updateButtons([...menuButtons]);
        }
        setCheckedItems(checkedItems);
    };

    const onPageIndexChange = (pageIndex) => {
        const clonePageInfo = _.cloneDeep(pageInfo);
        clonePageInfo.PageIndex = pageIndex;
        setPageInfo(clonePageInfo);
    };

    const renderAddDialog = () => {
        return (
            <R.Dialog
                id="raAddWhitelist"
                header={RMResx.RM_AR_RC_AddWhitelist_Title}
                width={680}
                height={346}
                status={{ show: showWhitelistDialog }}
                struct={{ foot: true }}
                destroy={true}
                closeable={true}
                onHide={() => setShowWhitelistDialog(false)}
            >
                <div>
                    <AddWhitelist ref={addWhitelistTableRef} />
                </div>
                <R.Button
                    slot="buttons"
                    classify="blank"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => setShowWhitelistDialog(false)}
                />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={onSaveAddWhitelist}
                />
            </R.Dialog>
        );
    };

    const renderImportDialog = () => {
        return (
            <R.Dialog
                id="raImportSiteMapping"
                header={RMResx.RM_AR_RC_ImportWhitelist_Title}
                width={680}
                height={346}
                status={{ show: showImportDialog }}
                struct={{ foot: true }}
                destroy={true}
                closeable={true}
                onHide={() => setShowImportDialog(false)}
            >
                <R.Validation id="allValidation">
                    <div className="flex flex-column gap-s">
                        <span className="font-semibold">
                            {StringUtil.trimEndColon(
                                RMResx.RM_JS_TM_SelectImportFile,
                            )}
                        </span>
                        <R.Validation
                            element="Uploader"
                            require={RMResx.RM_JS_BCM_ImportSetting_selectCSVFile}
                        >
                            <R.Uploader
                                ref={uploaderRef}
                                files={whitelistFiles}
                                fileTypes={["CSV"]}
                                maxSize={"10MB"}
                                showMaxSize={true}
                                showTypes={true}
                                onUpload={onUpload}
                                onDelete={onDelete}
                                multiple={false}
                            />
                        </R.Validation>
                    </div>
                </R.Validation>
                <R.Button
                    slot="buttons"
                    classify="blank"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => setShowImportDialog(false)}
                />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={onImport}
                />
            </R.Dialog>
        );
    };

    return (
        <div>
            <div className="ra-whitelist-actions">
                <TopButtonsComponent
                    ref={refTopButtons}
                    data={{ menuBtnItems: [...menuButtons] }}
                    showCount={4}
                ></TopButtonsComponent>
            </div>
            <WhitelistTable
                ref={whitelistTableRef}
                checkable={true}
                uniqueKey={"Id"}
                onChange={onItemsCheckedChange}
                onDelete={(id) => handleDeleteWhitelist(id)}
            />
            <div className="ra-main-footer" style={{ float: "right" }}>
                <SimplePager
                    pagerIndex={pageInfo.PageIndex}
                    pagerSize={pageInfo.PageSize}
                    shownCount={whitelistData.length}
                    hasNext={
                        (pageInfo.PageIndex + 1) * pageInfo.PageSize <
                        totalCount
                    }
                    onChange={onPageIndexChange}
                />
            </div>
            {renderAddDialog()}
            {renderImportDialog()}
        </div>
    );
};

export default Whitelist;
