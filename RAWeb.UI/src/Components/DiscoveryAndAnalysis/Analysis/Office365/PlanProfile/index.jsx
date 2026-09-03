import { useMemo, useState, useEffect, useRef } from "react";
import { useDispatch } from "react-redux";
import { useHistory } from "react-router-dom";
import { setAvaExternalActionRequest } from "../../../../../Redux/slices/avaDialogSlice";
import SiteMapLinks from "../../../../../Constants/SiteMapLinks";
import RouterUrls from "../../../../../Constants/RouterUrls";
import SiteMap from "../../Components/SiteMap";
import "./index.less";
import PlanProfilePanel from "./PlanProfilePanel";
import { showToast } from "../../../../../Utilities/CommonUtil";
// import { AvaWidget } from "@gui/chat-dialog";
import { OpusExternalRequestType, ExternalRequestProductType, RoleType } from '../../../../../Constants/Constants'
import LogicBuilder from "../../../RuleManagement/util/LogicBuilder";
import { AnalyseMethodConstants } from "../../../RuleManagement/Constants";

const API_BASE_URL = "/api/RMDiscoveryPlanProfileApi";

const createGuidId = () => {
    return globalThis.crypto ? globalThis.crypto.randomUUID() : Math.random().toString(36).substring(2);
};

class PlanProfileRowTemplate extends R.TableRow {
    render(Row, Cell) {
        const { rowData } = this.props;
        return (
            <Row>
                <Cell>
                    <div className="text-overflow">{rowData.Name}</div>
                </Cell>
                <Cell>
                    <div className="text-overflow">{rowData.Scope}</div>
                </Cell>
                <Cell>
                    <div style={{ whiteSpace: "normal" }}>{rowData.Rule}</div>
                </Cell>
                <Cell>
                    <div className="text-overflow">{rowData.Action}</div>
                </Cell>
            </Row>
        );
    }
}

const normalizeTenantId = (payload) => {
    if (typeof payload === "string") return payload;
    if (!payload || typeof payload !== "object") return "";
    return payload.tenantId || payload.id || payload.value || "";
};

const ActionEnums = {
    None: 0,
    ArchiveAndDestroy: 1,
    DestroyFile: 2
};

const OptScopeTypeEnums = {
    ContentSource: 1,
    SpecifyContainers: 2
};

const OptContentSourceEnums = {
    SharePoint: 1,
    OneDrive: 6
};

const mapActionLabel = (action) => {
    if (action == ActionEnums.ArchiveAndDestroy) return RMResx.RM_FA_PlanProfile_Action_Radio_ArchiveAndDestroy;
    if (action == ActionEnums.DestroyFile) return RMResx.RM_FA_PlanProfile_Action_Radio_Destroy;
    return "";
};

const RuleDisplayCell = ({ criteriaInfoes }) => {
    if (!criteriaInfoes || criteriaInfoes.length === 0) return null;

    let displayInfoes = [];
    let logicText = "";

    try {
        displayInfoes = LogicBuilder.getCriteriaDisplayInfoes(
            AnalyseMethodConstants.type.AVADocument, 
            criteriaInfoes
        ) || [];
        logicText = LogicBuilder.translate(LogicBuilder.build(criteriaInfoes));
    } catch (e) {
        console.warn(e);
    }

    const textContent = displayInfoes.map((item) => {
        const extra = item.extraComponent ? `( ${item.extraValue} )` : "";
        return `${item.order}. ${item.criteriaName} ${extra} ${item.conidtionName} ( ${item.value} )`;
    }).join("\n") + `\n\n${logicText}`;

    return (
        <div 
            title={textContent}
            style={{ 
                display: "-webkit-box", 
                WebkitLineClamp: 3,
                WebkitBoxOrient: "vertical", 
                overflow: "hidden", 
                whiteSpace: "pre-wrap",
                wordBreak: "break-all",
                cursor: "pointer"
            }}
        >
            {textContent}
        </div>
    );
};

const mapTableItems = (list) => {
    return list.map((item) => {
        const siteCount = item.totalMappingSites;
        
        const scopeSuffix = siteCount === 1 || siteCount === 0 ? RMResx.RM_FA_PlanProfile_Column_Scope_Desc : RMResx.RM_FA_PlanProfile_Column_Scope_Descs;
        const scopeLabel = item.totalMappingSites ? `${siteCount} ${scopeSuffix}` : "";

        return {
            Id: item.id,
            Name: item.name,
            Scope: scopeLabel,
            Rule: <RuleDisplayCell criteriaInfoes={item.criteriaInfoes} />,
            Action: mapActionLabel(item.action)
        };
    });
};

const showMsgToast = (content, type) => {
    $$.toast({
        content: content,
        classify: type
    });
};

const PlanProfile = () => {
    const dispatch = useDispatch();
    const history = useHistory();
    const [selectedO365TenantId, setSelectedO365TenantId] = useState("");
    const [selectedItems, setSelectedItems] = useState([]);
    const [searchValue, setSearchValue] = useState("");
    const [sortMetadata, setSortMetadata] = useState({ sortBy: "", isDesc: null });
    const [items, setItems] = useState([]);
    const [pager, setPager] = useState({ pageIndex: 1, pageSize: 10, total: 0 });

    const [editorPanelStatus, setEditorPanelStatus] = useState({ show: false });
    const [editorMode, setEditorMode] = useState("create");
    const [editorPayload, setEditorPayload] = useState(null);
    const [isWidgetExpanded, setIsWidgetExpanded] = useState(true);
    const panelApiRef = useRef(null);

    const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);

    const [isOptDialogOpen, setIsOptDialogOpen] = useState(false);
    const [optScopeType, setOptScopeType] = useState(1);
    const [optSourceSp, setOptSourceSp] = useState(false);
    const [optSourceOd, setOptSourceOd] = useState(false);
    
    const [optAvailableContainers, setOptAvailableContainers] = useState([]);
    
    const [optValidateInfo, setOptValidateInfo] = useState({ isValidated: true, errorMessages: [] });
    const [showAvaWidget, setShowAvaWidget] = useState(false);
    const [shouldHidePlanChatInputBox, setShouldHidePlanChatInputBox] = useState(false);

    const optDialogRef = useRef(null);

    const columns = useMemo(
        () => [
            { header: RMResx.RM_FA_PlanProfile_Column_Name, valuePath: "Name", width: [220, 480], resizeable: true, sortable: true },
            { header: RMResx.RM_FA_PlanProfile_Column_Scope, valuePath: "Scope", width: [180, 420], resizeable: true, sortable: true },
            { header: RMResx.RM_FA_PlanProfile_Column_Rule, valuePath: "Rule", width: [240, 560], resizeable: true, sortable: true },
            { header: RMResx.RM_FA_PlanProfile_Column_Action, valuePath: "Action", width: [140, 300], resizeable: true, sortable: true }
        ],
        []
    );

    const loadTableData = async () => {
        const payload = {
            pageIndex: pager.pageIndex,
            pageSize: pager.pageSize,
            searchValue: searchValue
        };

        if (sortMetadata.sortBy) {
            payload.sortBy = sortMetadata.sortBy;
            payload.isDesc = sortMetadata.isDesc;
        }

        try {
            $$.loading(true);

            const response = await fetchUtility({
                url: `${API_BASE_URL}/GetPlanProfilesPaged`,
                method: "POST", 
                data: payload
            });

            const mappedItems = mapTableItems(response?.items || []);

            setItems(mappedItems);
            setPager((prev) => ({
                ...prev,
                total: response.totalCount,
                pageIndex: response.pageIndex,
                pageSize: response.pageSize
            }));
        } catch (error) {
            console.error(error);
            setItems([]);
            setPager((prev) => ({ ...prev, total: 0 }));
        } finally {
            $$.loading(false);
        }
    };

    useEffect(() => {
        loadTableData();
    }, [pager.pageIndex, pager.pageSize, selectedO365TenantId, searchValue, sortMetadata.sortBy, sortMetadata.isDesc]);

    useEffect(() => {
        fetchUtility({
            url: `${API_BASE_URL}/EnableAIMessage`,
            method: "POST"
        })
        .then(res => {
            setShowAvaWidget(res === true);
        })
        .catch(err => console.error(err));
    }, [selectedO365TenantId]);

    useEffect(() => {
        fetchUtility({
            url: `${API_BASE_URL}/GetPlanChatDisplayConfiguration`,
            method: "GET"
        })
        .then(shouldDisplayChat => {
            setShouldHidePlanChatInputBox(shouldDisplayChat !== true);
        })
        .catch(err => {
            console.error(err)
        });
    }, []);

    const handleSiteMapChange = (payload) => {
        const tenantId = normalizeTenantId(payload);
        setSelectedO365TenantId(tenantId);
        setSelectedItems([]);
        setPager((prev) => ({ ...prev, pageIndex: 1 }));
    };

    const handleSearch = (value) => {
        setSearchValue(value || "");
        setPager((prev) => ({ ...prev, pageIndex: 1 }));
    };

    const handleSort = ({ status, column }) => {
        const sortBy = column?.valuePath || "";
        const isDesc = String(status || "").toUpperCase() === "DESC";
        setSortMetadata({ sortBy, isDesc });
        setPager((prev) => ({ ...prev, pageIndex: 1 }));
    };

    const handleTableCheck = (list) => {
        setSelectedItems(Array.isArray(list) ? list : []);
    };

    const handlePaging = ({ newValue }) => {
        const nextPageIndex = Number(newValue?.selectedPage || 1);
        const nextPageSize = Number(newValue?.pageSize || pager.pageSize);

        setSelectedItems([]);
        setPager((prev) => ({
            ...prev,
            pageIndex: nextPageIndex,
            pageSize: nextPageSize
        }));
    };

    const openCreatePanel = () => {
        setEditorMode("create");
        setEditorPayload(null);
        setEditorPanelStatus({ show: true });
    };

    const fetchPlanProfileById = async (id) => {
        try {
            $$.loading(true);

            const response = await fetchUtility({
                url: `${API_BASE_URL}/GetPlanProfileById`,
                method: "POST",
                data: id
            });
            return response;
        } catch (error) {
            console.error("[PlanProfile] Failed to fetch profile details", error);
            return null;
        } finally {
            $$.loading(false);
        }
    };

    const openEditPanel = async () => {
        const selectedId = selectedItems[0]?.Id;
        if (!selectedId) return;

        const detailedProfileData = await fetchPlanProfileById(selectedId);
        
        if (detailedProfileData) {
            setEditorMode("edit");
            setEditorPayload(detailedProfileData);
            setEditorPanelStatus({ show: true });
        }
    };

    const closePanel = () => setEditorPanelStatus({ show: false });

    const openDeleteDialog = () => setIsDeleteDialogOpen(true);
    const closeDeleteDialog = () => setIsDeleteDialogOpen(false);

    const confirmDelete = async () => {
        const idsToDelete = selectedItems.map(item => item.Id);
        if (idsToDelete.length === 0) return;

        try {
            $$.loading(true);
            
            const response = await fetchUtility({
                url: `${API_BASE_URL}/DeletePlanProfiles`,
                method: "POST",
                data: idsToDelete
            });

            if (response && response.MessageType !== undefined && response.MessageType !== 0) {
                showToast.error(response.ErrorMessage);
                closeDeleteDialog();
                return;
            }
            
            setSelectedItems([]);
            closeDeleteDialog();
            showToast.success(idsToDelete.length === 1
                ? RMResx.RM_FA_PlanProfile_Delete_Success_Single
                : RMResx.RM_FA_PlanProfile_Delete_Success
            );

            if (pager.pageIndex === 1) {
                loadTableData();
            } else {
                setPager(prev => ({ ...prev, pageIndex: 1 })); 
            }
        } catch (error) {
            console.error("Delete operation failed.", error);
            closeDeleteDialog();
        } finally {
            $$.loading(false);
        }
    };

    const submitProfileJob = async (url) => {
        const profileIds = selectedItems.map(item => String(item.Id));
        if (profileIds.length === 0) return;

        try {
            $$.loading(true);

            const response = await fetchUtility({
                url,
                method: "POST",
                data: { profiles: profileIds }
            });

            const isSuccess = response && response.FaildType === 0;

            if (isSuccess) {
                $$.toast({ content: RMResx.RM_FA_PlanProfile_StartIntelligentOptimization_Success, classify: 'success' });
            } else {
                $$.toast({ content: RMResx.RM_FA_PlanProfile_StartIntelligentOptimization_Failed, classify: 'error' });
            }
        } catch (error) {
            console.error("Failed to submit profile job.", error);
            $$.toast({ content: RMResx.RM_FA_PlanProfile_StartIntelligentOptimization_Failed, classify: 'error' });
        } finally {
            $$.loading(false);
        }
    };

    const handleRun = () => {
        submitProfileJob("/api/RMDiscoveryOffice365OptimizationApi/SaveDiscoveryPlanProOptimizationSetting");
    };

    const handleSimulate = () => {
        submitProfileJob("/api/RMDiscoveryOffice365OptimizationApi/SaveDiscoveryPlanProScanSetting");
    };

    const triggerExternalAction = (type) => {
        const nextRequest = {
            id: createGuidId(),
            productType: ExternalRequestProductType.Opus,
            data: { type, shouldHideChatInputBox: shouldHidePlanChatInputBox }
        };

        dispatch(setAvaExternalActionRequest(nextRequest));
    };

    const onSaveClick = async () => {
        const panelApi = panelApiRef.current;
        if (!panelApi || !panelApi.validate()) return;

        const payload = panelApi.getPayload();

        try {
            $$.loading(true);
            const response = await fetchUtility({
                url: `${API_BASE_URL}/SaveOrUpdatePlanProfile`,
                method: "POST",
                data: payload
            });

            if (response && response.MessageType !== undefined && response.MessageType !== 0) {
                showMsgToast(response.ErrorMessage, "error");
                return;
            }

            closePanel();
            if (editorMode === "create") {
                showToast.success(RMResx.RM_FA_PlanProfile_Create_Success);
            }
            loadTableData(); 
        } catch (error) {
            console.error("Save operation failed.", error);
        } finally {
            $$.loading(false);
        }
    };

    const handleStartIntelligentOptimization = () => {
        setIsOptDialogOpen(true);
    };

    const closeOptDialog = () => {
        setIsOptDialogOpen(false);
        setOptScopeType(OptScopeTypeEnums.ContentSource);
        setOptSourceSp(false);
        setOptSourceOd(false);
        setOptValidateInfo({ isValidated: true, errorMessages: [] });
        
        setOptAvailableContainers(prev => prev.map(item => ({ ...item, checked: false })));
    };

    useEffect(() => {
        if (optScopeType === OptScopeTypeEnums.SpecifyContainers && optAvailableContainers.length === 0) {
            optDialogRef.current?.loading(true);

            fetchUtility({
                url: `/api/RMDiscoveryOffice365ConfigurationApi/GetNewlyAvaliableOpusContainers`,
                method: "GET"
            })
            .then(res => {
                const data = res?.items || res || [];
                const formattedItems = data.map(container => ({
                    name: container.Name || container.Url || container.Id,
                    value: container.Id,
                    checked: false
                }));
                setOptAvailableContainers(formattedItems);
            })
            .catch(err => console.error("Failed to fetch opus containers", err))
            .finally(() => {
                optDialogRef.current?.loading(false);
            });
        }
    }, [optScopeType, optAvailableContainers.length]);

    const submitOptimization = async () => {
        if (optScopeType === OptScopeTypeEnums.ContentSource && !optSourceSp && !optSourceOd) {
            setOptValidateInfo({ 
                isValidated: false, 
                errorMessages: [RMResx.RM_FA_PlanProfile_StartIntelligentOptimization_Verify]
            });
            return;
        }

        const selectedIds = optAvailableContainers.filter(x => x.checked).map(x => x.value);

        if (optScopeType === OptScopeTypeEnums.SpecifyContainers && selectedIds.length === 0) {
            setOptValidateInfo({ 
                isValidated: false, 
                errorMessages: [RMResx.RM_FA_PlanProfile_StartIntelligentOptimization_Verify] 
            });
            return;
        }

        setOptValidateInfo({ isValidated: true, errorMessages: [] });

        const sources = [];
        if (optScopeType === OptScopeTypeEnums.ContentSource) {
            if (optSourceSp) sources.push(OptContentSourceEnums.SharePoint); 
            if (optSourceOd) sources.push(OptContentSourceEnums.OneDrive); 
        }

        const payload = {
            scopeType: optScopeType,
            contentSources: sources,
            specifyContainerIds: optScopeType === OptScopeTypeEnums.SpecifyContainers ? selectedIds : []
        };

        try {
            $$.loading(true);
            const response = await fetchUtility({
                url: `/api/RMDiscoveryPlanProfileApi/TriggerDalJob`,
                method: "POST",
                data: payload
            });
            closeOptDialog();

            const isSuccess = response && response.FaildType === 0;
            
            if (isSuccess) {
                $$.toast({ content: RMResx.RM_FA_PlanProfile_StartIntelligentOptimization_Success, classify: 'success' });
            } else {
                $$.toast({ content: RMResx.RM_FA_PlanProfile_StartIntelligentOptimization_Failed, classify: 'error' });
            }
        } catch (err) {
            console.error("Failed to trigger intelligent optimization.", err);
        } finally {
            $$.loading(false);
        }
    };

    const selectItemsCountMsg = RMResx.RM_Common_SelectTableItemsCounter
        ? RMResx.RM_Common_SelectTableItemsCounter.format(selectedItems.length, pager.total)
        : `${selectedItems.length} of ${pager.total} selected`;

    const renderHeader = () => (
        <div className="ra-main-header">
            <R.Searchbox placeholder={RMResx.RM_FA_PlanProfile_SearchByName} disabled={false} onSearch={handleSearch} width={380} />
        </div>
    );

    const renderNavbar = () => (
        <div className="ra-main-navbar">
            <div className="flex">
                <R.Button primary classify="theme" text={RMResx.RM_JS_Common_Create} onClick={openCreatePanel} />
                {selectedItems.length === 1 && (
                    <R.Button icon="fia-edit" text={RMResx.RM_JS_Common_Edit} onClick={openEditPanel} />
                )}
                {selectedItems.length > 0 && (
                    <>
                        <R.Button icon="fia-run" text={RMResx.RM_FA_PlanProfile_Actions_Run} onClick={handleRun} />
                        <R.Button icon="fia-calculator" text={RMResx.RM_FA_PlanProfile_Actions_Simulate} onClick={handleSimulate} />
                        <R.Button icon="fia-delete" text={RMResx.RM_JS_Common_Delete} onClick={openDeleteDialog} />
                    </>
                )}
            </div>
            <div className="ra-main-selected-counter">{selectItemsCountMsg}</div>
        </div>
    );

    const renderTable = () => (
        <div className="ra-main-table">
            <R.Table
                id="PlanProfileTable"
                columns={columns}
                rowTemplate={PlanProfileRowTemplate}
                items={items}
                checkable
                onCheck={handleTableCheck}
                doSort={handleSort}
            />
        </div>
    );

    const renderFooter = () => {
        const calculatedPageCount = Math.ceil(pager.total / pager.pageSize) || 1;

        return (
            <div className="plan-profile-footer-container">
                <R.Pager
                    pageSize={pager.pageSize}
                    pageCount={calculatedPageCount}
                    type="simple"
                    selectedPage={pager.pageIndex}
                    onChange={handlePaging}
                />
            </div>
        );
    };

    const renderAvaWidget = () => {
        if (!(!RM.gData.diableChatBot && RM.gData.chatBotApiURL) || RM.RoleType != RoleType.SupAdmin) return null;
        
        return (
            <div className="margin-bottom-m">
                {/* <AvaWidget
                    layout="vertical"
                    showMore={true}
                    onToggle={() => setIsWidgetExpanded(!isWidgetExpanded)}
                >
                    <AvaWidget.GroupAction
                        title={RMResx.RM_AVA_Title}
                        description={RMResx.RM_AVA_Description}
                    >
                        <AvaWidget.Button onClick={() => triggerExternalAction(OpusExternalRequestType.BuildPlanOpus)}>
                            {RMResx.RM_AVA_BuildNewPlan_Button}
                        </AvaWidget.Button>
                        <AvaWidget.Button onClick={() => history.push(RouterUrls.FA_Plan_PlanView)}>
                            {RMResx.RM_AVA_ViewTheLatestPlan_Button}
                        </AvaWidget.Button>
                        <AvaWidget.Button onClick={() => triggerExternalAction(OpusExternalRequestType.OpusViewHistoryPlan)}>
                            {RMResx.RM_AVA_ViewHistoryPlan_Button}
                        </AvaWidget.Button>
                    </AvaWidget.GroupAction>
                </AvaWidget> */}
            </div>
        );
    };

    const onSimulateClick = () => console.log("Panel Request: Simulate");

    const renderPanel = () => (
        <R.Panel
            id="raPlanProfileEditorPanel"
            header={editorMode === "create" ? RMResx.RM_FA_PlanProfile_Create_Panel_Title : RMResx.RM_FA_PlanProfile_Edit_Panel_Title}
            size={670}
            status={editorPanelStatus}
            destroy={true}
            onClose={closePanel}
        >
            <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={closePanel} />
            <R.Button slot="buttons" text={RMResx.RM_FA_PlanProfile_Actions_Simulate} onClick={onSimulateClick} />
            <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={onSaveClick} />

            <div className="plan-profile-panel-content">
                <PlanProfilePanel
                    mode={editorMode}
                    tenantId={selectedO365TenantId}
                    data={editorPayload}
                    onReady={(api) => {
                        panelApiRef.current = api;
                    }}
                />
            </div>
        </R.Panel>
    );

    const renderDeleteDialog = () => (
        <R.Dialog
            id="raDeleteDialog"
            header={RMResx.RM_JS_Common_Confirmation || "Confirmation"}
            width={400}
            status={{ show: isDeleteDialogOpen }}
            struct={{ foot: true }}
            destroy
            onClose={closeDeleteDialog}
        >
            <div style={{ padding: "16px 0", fontSize: "14px" }}>
                {RMResx.RM_FA_PlanProfile_Delete_Panel_Desc}
            </div>
            <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} classify="blank" onClick={closeDeleteDialog} />
            <R.Button slot="buttons" text={RMResx.RM_JS_Common_Delete} primary classify="theme" onClick={confirmDelete} />
        </R.Dialog>
    );

    const renderOptimizationDialog = () => {
        return (
            <R.Dialog
                id="raOptimizationDialog"
                ref={optDialogRef}  
                header={RMResx.RM_FA_PlanProfile_StartIntelligentOptimization}
                width={500}
                height={550}
                status={{ show: isOptDialogOpen }}
                struct={{ foot: true }}
                destroy
                onClose={closeOptDialog}
            >
                <div style={{ padding: "16px 0", display: "flex", flexDirection: "column", gap: "16px" }}>
                    <div style={{ fontWeight: "bold" }}>
                        {RMResx.RM_FA_PlanProfile_SIO_SelectScope}
                        <span style={{ color: "red", marginLeft: "4px" }}>*</span>
                    </div>
                    
                    <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                        <R.Radio 
                            checked={optScopeType === OptScopeTypeEnums.ContentSource} 
                            onChange={() => {
                                setOptScopeType(OptScopeTypeEnums.ContentSource);
                                setOptValidateInfo({ isValidated: true, errorMessages: [] });
                            }} 
                            text={RMResx.RM_FA_PlanProfile_SIO_ContentSource}
                        />
                    </div>
                    {optScopeType === OptScopeTypeEnums.ContentSource && (
                        <div style={{ marginLeft: "24px", display: "flex", flexDirection: "column", gap: "8px" }}>
                            <R.Checkbox 
                                checked={optSourceSp} 
                                onChange={(val) => setOptSourceSp(val)} 
                                text={RMResx.RM_FA_PlanProfile_SIO_SP}
                            />
                            <R.Checkbox 
                                checked={optSourceOd} 
                                onChange={(val) => setOptSourceOd(val)} 
                                text={RMResx.RM_FA_PlanProfile_SIO_OneDrive}
                            />
                        </div>
                    )}

                    <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                        <R.Radio 
                            checked={optScopeType === OptScopeTypeEnums.SpecifyContainers} 
                            onChange={() => {
                                setOptScopeType(OptScopeTypeEnums.SpecifyContainers);
                                setOptValidateInfo({ isValidated: true, errorMessages: [] });
                            }} 
                            text={RMResx.RM_FA_PlanProfile_SIO_AnalyzeSelectedContainers}
                        />
                    </div>
                    {optScopeType === OptScopeTypeEnums.SpecifyContainers && (
                        <div style={{ marginLeft: "24px" }} className="margin-top-s margin-bottom-s">
                            <R.Multicombobox
                                id="raOptContainers"
                                width={400}
                                popupMaxHeight={350}
                                items={optAvailableContainers}
                                textField="name"
                                valueField="value"
                                checkedField="checked"
                                onChange={(arg) => {
                                    const selectedValues = (arg?.newValue || []).map(x => x.value);
                                    
                                    setOptAvailableContainers(prev => prev.map(item => ({
                                        ...item,
                                        checked: selectedValues.includes(item.value)
                                    })));
                                    
                                    setOptValidateInfo({ isValidated: true, errorMessages: [] });
                                }}
                            />
                        </div>
                    )}
                    {!optValidateInfo.isValidated && (
                        <div className="reco-error-messages margin-top-s margin-bottom-s">
                            {optValidateInfo.errorMessages.map((item, index) => (
                                <div className="reco-error-message" key={index} tabIndex="0" style={{ color: "#c80b0b", fontSize: "14px" }}>
                                    {item}
                                </div>
                            ))}
                        </div>
                    )}
                </div>

                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} classify="blank" onClick={closeOptDialog} />
                <R.Button slot="buttons" text={RMResx.RM_FA_PlanProfile_StartIntelligentOptimization_Start} primary classify="theme" onClick={submitOptimization} />
            </R.Dialog>
        );
    };

    return (
        <div id="raPlanProfile">
            <div style={{ display: "flex", alignItems: "flex-start", justifyContent: "space-between" }}>
                {/* <SiteMap URL={[SiteMapLinks.FA_Plan_Profile]} onChange={handleSiteMapChange} /> */}
                <$g.SiteMap data={[SiteMapLinks.FA_Plan_Profile]} />
                <div style={{ paddingTop: "3px" }}>
                    <R.Button primary classify="theme" text={RMResx.RM_FA_PlanProfile_StartIntelligentOptimization} onClick={handleStartIntelligentOptimization} />
                </div>
            </div>
            <div>
                <div className="reco-plan-profile">
                     <div className="reco-ava-box">
                        {showAvaWidget && (
                        <div className="reco-ava-box">
                            {renderAvaWidget()}
                        </div>
                    )}
                    </div>
                    <div className="ra-page-container reco-plan-profile-list">
                        {renderHeader()}
                        {renderNavbar()}
                        {renderTable()}
                        {renderFooter()}
                    </div>
                </div>
            </div>
            {renderPanel()}
            {renderDeleteDialog()}
            {renderOptimizationDialog()}
        </div>
    );
};

export default PlanProfile;
