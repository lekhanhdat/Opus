import React, { useEffect, useMemo, useRef, useState } from "react";
import _ from 'lodash'

import TopButtonsComponent from "../../Common/Util/TopButtonsComponent";
import { SimplePager } from "../../Common/Pager";
import AddWhitelist from "./AddWhitelist";
import WhitelistTable from "./WhitelistTable";
import StringUtil from "../../../Utilities/StringUtil";
import { showToast } from "../../../Utilities/CommonUtil";
import { useStableCallback } from "../../BCM/AzureFileShareConfigureConnection/Hooks";
import { ContentSearchType, getContentSearchOptions } from "../Constants";
import { MessageType } from "../../CP/CPConstants";

const Whitelist = (props, ref) => {
    const {
        isSCBlackListForEdiscovery,
        checkIsSCBlackListForEdiscovery,
        getUrlListByContentSearchListlist,
        onClosePanel,
        onReset
    } = props;

    const [contentSearchOption, setContentSearchOption] = useState({
        selected: ContentSearchType.Whitelist,
        list: getContentSearchOptions(ContentSearchType.Whitelist),
    });

    const refTopButtons = useRef();

    const uploaderRef = useRef();

    const filesRef = useRef();

    const addContentSearchlistTableRef = useRef();

    const whitelistTableRef = useRef();

    const [siteCollectionData, setSiteCollectionData] = useState([]);

    const [showImportDialog, setShowImportDialog] = useState(false);

    const [showWhitelistDialog, setShowWhitelistDialog] = useState(false);

    const [whitelistFiles] = useState([]);

    const [checkedItems, setCheckedItems] = useState([]);

    const [totalCount, setTotalCount] = useState(0);

    const [pageInfo, setPageInfo] = useState({
        PageIndex: 0,
        PageSize: 10,
    });

    useEffect(() => {
        const selectedContentSearchOption = isSCBlackListForEdiscovery ? ContentSearchType.Blacklist : ContentSearchType.Whitelist;
        setContentSearchOption({
            selected: selectedContentSearchOption,
            list: getContentSearchOptions(selectedContentSearchOption)
        })
    }, [isSCBlackListForEdiscovery])

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
    }, [isSCBlackListForEdiscovery]);

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
        const selectedContentSearchOption = isSCBlackListForEdiscovery ? ContentSearchType.Blacklist : ContentSearchType.Whitelist;
        getContentSearchList(false, selectedContentSearchOption);
    }, [pageInfo.PageIndex, isSCBlackListForEdiscovery]);

    const getContentSearchList = (isResetPagerIndex, selectedContentSearchOption) => {
        $$.loading(true);
        const clonePageInfo = _.cloneDeep(pageInfo);
        if (isResetPagerIndex) {
            clonePageInfo.PageIndex = 0;
            clonePageInfo.PageSize = 10;
            setCheckedItems([]);
        }
        const requestOption = {
            url: selectedContentSearchOption === ContentSearchType.Whitelist ? "/api/ArchiverRestore/GetSCWhiteList" : "/api/ArchiverRestore/GetSCBlackList", // update later
            data: clonePageInfo,
        };
        fetchUtility(requestOption)
            .then((result) => {
                if (result) {
                    setSiteCollectionData(result.SiteCollections);
                    setTotalCount(result.TotalCount);
                    setPageInfo(clonePageInfo);
                    whitelistTableRef.current &&
                        whitelistTableRef.current.setTableInfo({
                            items: result.SiteCollections,
                            isReset: isResetPagerIndex,
                        });
                }
            })
            .finally(() => $$.loading(false));
    };

    const switchFullTextIndexType = (deleteCurentTypeOldData, newValue) => {
        const requestOption = {
            url: "/api/ArchiverRestore/SwitchFullTextIndexType",
            method: "POST",
            data: {
                Type: newValue,
                CleanSCList: deleteCurentTypeOldData,
            }
        };
        $$.loading(true);
        fetchUtility(requestOption)
            .then(async (res) => {
                if (res.MessageType === MessageType.Successful) {
                    await checkIsSCBlackListForEdiscovery();
                    getContentSearchList(true, newValue); // refetch
                    onReset();
                }
            })
            .finally(() => $$.loading(false));
    }

    const onChangeContentSearchOption = (newValue, deleteCurentTypeOldData, isSwitch = false) => {
        $$.messagedialog(false);
        setContentSearchOption({
            selected: newValue,
            list: getContentSearchOptions(newValue),
        });
        if (isSwitch) {
            switchFullTextIndexType(deleteCurentTypeOldData, newValue);
        }
    }
    
    const handleChangeContentSearchOption = (newValue) => {
        onChangeContentSearchOption(contentSearchOption.selected);
        const args = {
            classify: "warn",
            width: "550px",
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div tabIndex={0}>{RMResx.RM_AR_RC_White_Blacklist_SwitchConfirmMsg}</div>,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => $$.messagedialog(false),
                },
                {
                    text: RMResx.RM_AR_RC_White_Blacklist_No,
                    onClick: () => onChangeContentSearchOption(newValue, false, true),
                },
                {
                    id: "rcClearContentSearchList",
                    text: RMResx.RM_AR_RC_White_Blacklist_Yes,
                    primary: true,
                    classify: "theme",
                    onClick: () => onChangeContentSearchOption(newValue, true, true),
                },
            ],
        }
        $$.messagedialog(true, args);
    }

    // Add whitelist or blacklist
    const onSaveAddContentSearchList = async (selectedContentSearchOption) => {
        if (
            addContentSearchlistTableRef.current &&
            !addContentSearchlistTableRef.current.isValid()
        ) {
            return false;
        }

        const addContentSearchList =
            addContentSearchlistTableRef.current &&
            addContentSearchlistTableRef.current.getAddWhitelist();
        $$.loading(true);
        const requestOption = {
            url: selectedContentSearchOption === ContentSearchType.Whitelist ? "/api/ArchiverRestore/AddSCWhitelist" : "/api/ArchiverRestore/AddSCBlacklist", // update later
            data: addContentSearchList ? addContentSearchList : siteCollectionData.map((item) => ({ SiteCollectionUrl: item.SiteCollectionUrl, Id: item.Id })),
        };
        fetchUtility(requestOption)
            .then((response) => {
                if (response) {
                    if (response.MessageType === 0) {
                        setShowWhitelistDialog(false);
                        showToast.success(isSCBlackListForEdiscovery ? RMResx.RM_AR_RC_AddBlacklist_Successful : RMResx.RM_AR_RC_AddWhitelist_Successful);
                        getContentSearchList(true, selectedContentSearchOption);
                        getUrlListByContentSearchListlist();
                        onReset();
                    } else {
                        showToast.error(response.ErrorMessage);
                    }
                }
            })
            .finally(() => $$.loading(false));        
    };

    // Export whitelist or blacklist
    const onExportContentSearchList = useStableCallback(async () => {
        $$.messagedialog(false);
        $$.loading(true);
        const response = await fetchUtility({
            url: contentSearchOption.selected === ContentSearchType.Whitelist ? "/api/ArchiverRestore/ExportSCWhitelist" : "/api/ArchiverRestore/ExportSCBlacklist", // update later
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
    });

    const handleExportWhitelist = () => {
        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div tabIndex={0}>{isSCBlackListForEdiscovery ? RMResx.RM_AR_RC_ExportBlacklist_ConfirmMsg : RMResx.RM_AR_RC_ExportWhitelist_ConfirmMsg}</div>,
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
                    onClick: onExportContentSearchList,
                },
            ],
        });
    };

    // Import whitelist or blacklist
    const onUpload = (args) => {
        if (args.isSucceed) {
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
        const url = contentSearchOption.selected === ContentSearchType.Whitelist ? "/api/ArchiverRestore/ImportSCWhitelist" : "/api/ArchiverRestore/ImportSCBlacklist"; // update later
        formData.append(
            "fileUp",
            filesRef.current.file,
            filesRef.current.fileName
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
                    onReset();
                } else {
                    showToast.error(res.responseString.ErrorMessage);
                }
            });
    };

    // Delete whitelist or blacklist
    const onDeleteContentSearchlist = useStableCallback(async (id) => {
        const clonedCheckedItems = _.cloneDeep(checkedItems);
        let itemIds = clonedCheckedItems.map((item) => item.Id);
        if (id) {
            itemIds = [id];
        }
        $$.messagedialog(false);
        $$.loading(true);
        const requestOption = {
            url: contentSearchOption.selected === ContentSearchType.Whitelist ? "/api/ArchiverRestore/DeleteSCWhitelist" : "/api/ArchiverRestore/DeleteSCBlacklist", // update later
            data: itemIds,
        };
        const response = await fetchUtility(requestOption);
        $$.loading(false);
        if (response.MessageType === 0) {
            showToast.success(isSCBlackListForEdiscovery ? RMResx.RM_AR_RC_DeleteBlacklist_Successful : RMResx.RM_AR_RC_DeleteWhitelist_Successful);
            getContentSearchList(true, contentSearchOption.selected);
            getUrlListByContentSearchListlist();
            onReset();
        } else {
            showToast.error(response.ErrorMessage);
        }
    });

    const handleDeleteWhitelist = (id) => {
        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div tabIndex={0}>{isSCBlackListForEdiscovery ? RMResx.RM_AR_RC_DeleteBlacklist_ConfirmMsg : RMResx.RM_AR_RC_DeleteWhitelist_ConfirmMsg}</div>,
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
                    onClick: () => onDeleteContentSearchlist(id),
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
                    <AddWhitelist ref={addContentSearchlistTableRef} isSCBlackListForEdiscovery={isSCBlackListForEdiscovery} />
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
                    onClick={() => onSaveAddContentSearchList(contentSearchOption.selected)}
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
                                RMResx.RM_JS_TM_SelectImportFile
                            )}
                        </span>
                        <R.Validation
                            element="Uploader"
                            require={RMResx.RM_AR_RC_ImportWhitelist_ErrorMsg}
                        >
                            <R.Uploader
                                ref={uploaderRef}
                                files={whitelistFiles}
                                fileTypes={["XLSX"]}
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
            <div id="export-history-dialog">
                <h4 tabIndex={0} className="font-semibold">
                    {RMResx.RM_AR_RC_White_Blacklist_OptionLabel}
                </h4>
                <div className="margin-top-s">
                    <R.Radio.Group
                        id="export-setting-radio"
                        block
                        name="export-setting"
                        items={contentSearchOption.list}
                        onChange={handleChangeContentSearchOption}
                    />
                </div>
            </div>
            <div className="margin-top-l">
                <div tabIndex={0} className="font-semibold">
                    {isSCBlackListForEdiscovery ? RMResx.RM_AR_RC_Blacklist_TableTitle : RMResx.RM_AR_RC_Whitelist_TableTitle}
                </div>
                <div className="flex flex-column gap-s">
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
                        shownCount={siteCollectionData.length}
                        hasNext={
                            (pageInfo.PageIndex + 1) * pageInfo.PageSize <
                            totalCount
                        }
                        onChange={onPageIndexChange}
                    />
                </div>
            </div>
            {renderAddDialog()}
            {renderImportDialog()}
        </div>
    );
};

export default Whitelist;
