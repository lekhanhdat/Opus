import _ from "lodash";
import { forwardRef, useEffect, useMemo, useRef, useState } from "react";
import TopButtonsComponent from "../../Common/Util/TopButtonsComponent";
import StringUtil from '../../../Utilities/StringUtil';
import { showToast } from "../../../Utilities/CommonUtil";
import SiteMappingTable from "./SiteMappingTable";
import AddMapping from "./AddMapping";
import { SimplePager } from "../../Common/Pager";
import { useStableCallback } from "../../BCM/AzureFileShareConfigureConnection/Hooks";

const SiteMapping = ({ onClosePanel }, ref) => {

    const refTopButtons = useRef();

    const uploaderRef = useRef();

    const filesRef = useRef();

    const addMappingTableRef = useRef();

    const siteMappingTableRef = useRef();

    const [siteMappingList, setSiteMappingList] = useState([]);

    const [showImportDialog, setShowImportDialog] = useState(false);

    const [showAddMappingDialog, setShowAddMappingDialog] = useState(false);

    const [siteMappingFiles] = useState([]);

    const [checkedItems, setCheckedItems] = useState([]);

    const [totalCount, setTotalCount] = useState(0);

    const [isOverrideChecked, setIsOverrideChecked] = useState(false);

    const [pageInfo, setPageInfo] = useState({
        PageIndex: 0,
        PageSize: 10,
    });

    const menuButtons = useMemo(() => {
        return [
            {
                isStatic: true,
                id: "raAddBtn",
                name: RMResx.RM_JS_BCM_Explorer_ExoMoveToSP_AddBtn,
                onClick: () => setShowAddMappingDialog(true),
            },
            {
                id: "raExportBtn",
                name: RMResx.RM_JS_BCM_Explorer_ExoMoveToSP_ExportBtn,
                icon: "fia-export-settings",
                onClick: () => { onExportMapping(); },
            },
            {
                id: "raImportBtn",
                name: RMResx.RM_JS_BCM_Explorer_ExoMoveToSP_ImportBtn,
                icon: "fia-import",
                onClick: () => setShowImportDialog(true),
            },
        ];
    }, []);

    const deleteButton = useMemo(() => {
        return [
            {
                id: "raDeleteBtn",
                name: RMResx.RM_JS_Common_Delete,
                icon: "fia-delete",
                onClick: () => { onDeleteMapping(); },
            },
        ];
    }, [])

    useEffect(() => {
        getSiteMapping(false);
    }, [pageInfo.PageIndex]);

    const getSiteMapping = async (isResetPagerIndex) => {
        $$.loading(true);
        const clonePageInfo = _.cloneDeep(pageInfo);
        if (isResetPagerIndex) {
            clonePageInfo.PageIndex = 0;
            clonePageInfo.PageSize = 10;
            setCheckedItems([]);
        }
        const requestOption = {
            url: "/api/ArchiverRestore/GetSCMappings",
            data: clonePageInfo,
        };
        const result = await fetchUtility(requestOption);
        setSiteMappingList(result.SiteMappings);
        setTotalCount(result.TotalCount);
        setPageInfo(clonePageInfo);
        siteMappingTableRef.current && siteMappingTableRef.current.setTableInfo({ items: result.SiteMappings, isReset: isResetPagerIndex });
        $$.loading(false);
    };

    const onExportMapping = () => {
        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_AR_RC_ExportMapping_ConfirmMsg,
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
                    onClick: onExportMappingOK,
                },
            ],
        });
    };

    const onExportMappingOK = async () => {
        $$.messagedialog(false);
        $$.loading(true);
        const requestOption = {
            url: "/api/ArchiverRestore/ExportSCMappings",
        };
        const response = await fetchUtility(requestOption);
        $$.loading(false);
        if (response.MessageType === 0) {
            const content = (
                <$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
                    <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    <a className="ra-link-a" href="/Root/DC/Download">{RMResx.RM_JS_DC_Title}</a>
                </$g.I18NProvider>
            );
            showToast.success(content);
        } else {
            showToast.error(response.ErrorMessage);
        }
        onClosePanel();
    };

    const onDeleteMapping = () => {
        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_AR_RC_DeleteMapping_ConfirmMsg,
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
                    onClick: onDeleteMappingOK,
                },
            ],
        });
    };

    const onDeleteMappingOK = useStableCallback(async () => {
        const clonedCheckedItems = _.cloneDeep(checkedItems);
        const itemIds = clonedCheckedItems.map(item => item.Id);
        $$.messagedialog(false);
        $$.loading(true);
        const requestOption = {
            url: "/api/ArchiverRestore/DeleteSCMappings",
            data: itemIds
        };
        const response = await fetchUtility(requestOption);
        $$.loading(false);
        if (response.MessageType === 0) {
            showToast.success(RMResx.RM_AR_RC_DeleteMapping_Successful);
            getSiteMapping(true);
        } else {
            showToast.error(response.ErrorMessage);
        }
    });

    const handleUpload = (args) => {
        if (args.isSucceed) {
            args.files[0].fileId = StringUtil.newGuid();
            filesRef.current = args.files[0]; 
        }
    };

    const handleDelete = (args) => {
        if (args.isSucceed) {
            filesRef.current = null;
        }
    };

    const onOverrideChanged = (args) => {
        setIsOverrideChecked(args);
    }

    const handleCancel = () => {
        setShowImportDialog(false);
        setIsOverrideChecked(false);
    }

    const handleImport = () => {
        if (!$$.verify("allValidation")) return false;

        $$.loading(true);
        const formData = new FormData();
        const url = "/api/ArchiverRestore/ImportSCMappings";
        formData.append('fileUp', filesRef.current.file, filesRef.current.fileName);
        formData.append('isOverride', isOverrideChecked);
        fetch(url, {
            method: 'POST',
            body: formData,
        })
            .then(function (response) {
                return response.text().then(function (dataString) {
                    return {
                        responseStatus: response.status,
                        responseString: JSON.parse(dataString)
                    };
                });
            })
            .then(function (res) {
                $$.loading(false);
                if (res.responseString.MessageType == 0) {
                    let content = <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>;
                    showToast.success(content);
                    setShowImportDialog(false);
                    onClosePanel();
                } else {
                    showToast.error(res.responseString.ErrorMessage);
                }
            });
    };

    const onPageIndexChange = (pageIndex) => {
        const clonePageInfo = _.cloneDeep(pageInfo);
        clonePageInfo.PageIndex = pageIndex;
        setPageInfo(clonePageInfo);
    };

    const onItemsCheckedChange = (checkedItems) => {
        if (checkedItems.length > 0) {
            refTopButtons.current.updateButtons([...menuButtons, ...deleteButton]);
        } else {
            refTopButtons.current.updateButtons([...menuButtons]);
        }
        setCheckedItems(checkedItems);
    };

    const onSaveAddMapping = async () => {
        if (addMappingTableRef.current && !addMappingTableRef.current.isValid()) {
            return false;
        }

        const addMappingList = addMappingTableRef.current && addMappingTableRef.current.getAddMapping();
        $$.loading(true);
        const requestOption = {
            url: "/api/ArchiverRestore/AddSCMappings",
            data: addMappingList
        };
        const response = await fetchUtility(requestOption);
        $$.loading(false);
        if (response.MessageType === 0) {
            setShowAddMappingDialog(false);
            showToast.success(RMResx.RM_AR_RC_AddMapping_Successful);
            getSiteMapping(true);
        } else {
            showToast.error(response.ErrorMessage);
        }
    };

    const renderAddMappingDialog = () => {
        return (
            <R.Dialog
                id="raAddMapping"
                header={RMResx.RM_JS_BCM_Explorer_ExoMoveToSP_AddBtn}
                width={680}
                height={346}
                status={{ show: showAddMappingDialog }}
                struct={{ foot: true }}
                destroy={true}
                closeable={true}
                onHide={() => setShowAddMappingDialog(false)}
            >
                <div>
                    <AddMapping
                        ref={addMappingTableRef}
                    />
                </div>
                <R.Button slot="buttons" classify="blank" text={RMResx.RM_JS_Common_Cancel} onClick={() => setShowAddMappingDialog(false)} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={onSaveAddMapping} />
            </R.Dialog>
        )
    };

    const renderImportDialog = () => {
        return (
            <R.Dialog
                id="raImportSiteMapping"
                header={RMResx.RM_AR_RC_ImportMapping}
                width={680}
                height={360}
                status={{ show: showImportDialog }}
                struct={{ foot: true }}
                destroy={true}
                closeable={true}
                onHide={handleCancel}
            >
                <R.Validation id="allValidation">
                    <div className="flex flex-column gap-s">
                        <span className="font-semibold">{StringUtil.trimEndColon(RMResx.RM_JS_TM_SelectImportFile)}</span>
                        <R.Validation
                            element="Uploader"
                            require={RMResx.RM_AR_RC_ImportMapping_ErrorMsg}
                        >
                            <R.Uploader
                                ref={uploaderRef}
                                files={siteMappingFiles}
                                fileTypes={["XLSX"]}
                                maxSize={"10MB"}
                                showMaxSize={true}
                                showTypes={true}
                                onUpload={handleUpload}
                                onDelete={handleDelete}
                                multiple={false}
                            />
                        </R.Validation>
                        <div>
                            <R.Checkbox
                                id="raImportSiteMappingOverride"
                                text={RMResx.RM_RS_SiteMappings_CheckOverrideInfo}
                                checked={isOverrideChecked}
                                onChange={onOverrideChanged}
                            />
                        </div>
                    </div>
                </R.Validation>
                <R.Button slot="buttons" classify="blank" text={RMResx.RM_JS_Common_Cancel} onClick={handleCancel} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={handleImport} />
            </R.Dialog>
        )
    };

    return (
        <div>
            <div className="flex flex-column gap-s">
                <TopButtonsComponent
                    ref={refTopButtons}
                    data={{ menuBtnItems: [...menuButtons] }}
                    showCount={4}
                ></TopButtonsComponent>
            </div>
            <SiteMappingTable
                ref={siteMappingTableRef}
                checkable={true}
                uniqueKey={"Id"}
                onChange={onItemsCheckedChange}
            />
            <div className="ra-main-footer" style={{ float: "right" }}>
                <SimplePager
                    pagerIndex={pageInfo.PageIndex}
                    pagerSize={pageInfo.PageSize}
                    shownCount={siteMappingList.length}
                    hasNext={(pageInfo.PageIndex + 1) * pageInfo.PageSize < totalCount}
                    onChange={onPageIndexChange}
                />
            </div>
            {renderAddMappingDialog()}
            {renderImportDialog()}
        </div>
    );
}

export default forwardRef(SiteMapping);