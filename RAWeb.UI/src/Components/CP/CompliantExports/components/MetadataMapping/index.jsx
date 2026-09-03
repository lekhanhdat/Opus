import { forwardRef, useEffect, useImperativeHandle, useMemo, useRef, useState } from "react";
import { Prompt } from 'react-router';
import _ from "lodash";

import MetadataMappingPanel from "./Panel";
import ExportSettingsTree from "../../../../Common/Tree/Instances/ExportSettings";
import { OrderTable } from "../../../../Common/OrderTable";
import {
    ConfiguratorContentSource,
    ConfiguratorFormatValue,
} from "../../Constants";
import StringUtil from "../../../../../Utilities/StringUtil";
import { getConfigurationContentSources } from "../../utils";

import "./MetadataMapping.less";

function MetadataMappingComponent(props, ref) {
    const {
        filteredExportSettingsByFormat,
        selectedFormat,
        renderPromptForLocal,
        onChangeChildTableItems,
    } = props;

    const [contentSource, setContentSource] = useState({
        list: getConfigurationContentSources(
            ConfiguratorContentSource.SPO_OD,
            false
        ),
        selected: ConfiguratorContentSource.SPO_OD,
    });
    const [isNARAFormat, setIsNARAFormat] = useState(false);
    const [filteredExportSettingsByCts, setFilteredExportSettingsByCts] = useState([]);
    const [selectedNode, setSelectedNode] = useState(null);
    const [treeNodeMetadataName, setTreeNodeMetadataName] = useState("");
    const [isEditingNodeMetadataName, setIsEditingNodeMetadataName] = useState(false);
    const [childTableItems, setChildTableItems] = useState([]);
    const [mappingRowData, setMappingRowData] = useState(null);
    const [showMappingPanel, setShowMapingPanel] = useState(false);

    const mappingPanelRef = useRef();
    const exportSettingsTreeRef = useRef();

    useImperativeHandle(ref, () => ({
        validateMetadataName: () => {
            if (isEditingNodeMetadataName && !treeNodeMetadataName.trim()) {
                return false;
            }
            return true;
        },
        isEditingNodeMetadataName: () => isEditingNodeMetadataName,
        saveMetadataName: handleSaveMetadataName,
        cancelMetadataName: handleCancelMetadataName,
    }))

    useEffect(() => {
        const isNARAFormat = selectedFormat === ConfiguratorFormatValue.NARA;
        const contentSourceList = getConfigurationContentSources(null, !!isNARAFormat);
        const defaultSelectedContentSource = contentSourceList.find((item) => [ConfiguratorContentSource.SPO_OD, ConfiguratorContentSource.EXO].includes(item.value))?.value ?? contentSourceList[0]?.value;
        setContentSource({
            list: getConfigurationContentSources(
                defaultSelectedContentSource,
                !!isNARAFormat
            ),
            selected: defaultSelectedContentSource,
        });
        setIsNARAFormat(!!isNARAFormat);

        // For VEO tree
        setSelectedNode(null);
        setChildTableItems([]);
    }, [selectedFormat]);

    useEffect(() => {
        const filteredExportSettingsByCts = filteredExportSettingsByFormat.find(
            (item) => item.SourceFlag === contentSource.selected
        );
        if (filteredExportSettingsByCts) {
            if (selectedFormat === ConfiguratorFormatValue.VEO) {
                setFilteredExportSettingsByCts([filteredExportSettingsByCts]); // Use for VEO tree
            } else {
                if (filteredExportSettingsByCts.ExportColumnInfoes?.length > 0) {
                    setChildTableItems(
                        filteredExportSettingsByCts.ExportColumnInfoes.map((item) => ({
                            ...item,
                            Id: item.Id ?? StringUtil.newGuid(), // Need to fake Id when get data from API
                        }))
                    );
                }
            }
        }
    }, [filteredExportSettingsByFormat, selectedFormat]);

    const columnsForVEO = useMemo(() => {
        const isEXO = contentSource.selected === ConfiguratorContentSource.EXO;
        return [
            {
                key: "MetadataName",
                name: RMResx.RM_ES_CompliantExport_ChildTable_DisplayNameColumn,
                width: 250,
                minWidth: 160,
                onRender: (item) => {
                    return <div data-tooltip="ifneed" className="ra-ellipsis flex-auto">{item.MetadataName}</div>
                }
            },
            {
                key: isEXO ? "ExchangeMetadata" : "SharePointMetadata",
                name: RMResx.RM_ES_CompliantExport_ChildTable_MappedKeyColumn,
                width: 250,
                minWidth: 160,
                onRender: (item) => {
                    const value = isEXO ? item.ExchangeMetadata : item.SharePointMetadata;
                    return <div data-tooltip="ifneed" className="ra-ellipsis flex-auto">{value}</div>
                }
            },
            {
                key: "actions",
                name: "",
                width: 80,
                minWidth: 100,
                onRender: (item) => (
                    <div className="action-buttons">
                        <R.Button
                            type="bald"
                            icon="fia-edit icon-option-item"
                            onClick={() => handleEditMapping(item)}
                            tooltip={RMResx.RM_JS_Common_Edit}
                        />
                        <R.Button
                            type="bald"
                            icon="fia-delete"
                            onClick={() => handleDeleteMapping(item)}
                            tooltip={RMResx.RM_JS_Common_Delete}
                        />
                    </div>
                ),
            },
        ];
    }, [mappingRowData, childTableItems, contentSource.selected]);

    const columns = useMemo(() => {
        return [
            {
                key: "DisplayName",
                name: RMResx.RM_ES_CompliantExport_ChildTable_DisplayNameColumn,
                width: 250,
                minWidth: 160,
                onRender: (item) => {
                    return <div data-tooltip="ifneed" className="ra-ellipsis flex-auto">{item.DisplayName}</div>
                }
            },
            {
                key: "MappedKey",
                name: RMResx.RM_ES_CompliantExport_ChildTable_MappedKeyColumn,
                width: 250,
                minWidth: 160,
                onRender: (item) => {
                    return <div data-tooltip="ifneed" className="ra-ellipsis flex-auto">{item.MappedKey}</div>
                }
            },
            {
                key: "actions",
                name: "",
                width: 80,
                minWidth: 100,
                onRender: (item) => (
                    <div className="action-buttons">
                        <R.Button
                            type="bald"
                            icon="fia-edit icon-option-item"
                            onClick={() => handleEditMapping(item)}
                            tooltip={RMResx.RM_JS_Common_Edit}
                        />
                        <R.Button
                            type="bald"
                            icon="fia-delete"
                            onClick={() => handleDeleteMapping(item)}
                            tooltip={RMResx.RM_JS_Common_Delete}
                        />
                    </div>
                ),
            },
        ];
    }, [mappingRowData, childTableItems, contentSource.selected]);

    const handleChangeContentSource = (args) => {
        const newValue = args.newValue.value;
        const filteredExportSettingsByCts = filteredExportSettingsByFormat.find(
            (item) => item.SourceFlag === newValue
        );
        if (selectedFormat === ConfiguratorFormatValue.VEO) {
            setSelectedNode(null);
            setFilteredExportSettingsByCts([filteredExportSettingsByCts]);
        } else {
            if (filteredExportSettingsByCts.ExportColumnInfoes?.length > 0) {
                setChildTableItems(filteredExportSettingsByCts.ExportColumnInfoes.map((item) => ({
                    ...item,
                    Id: item.Id ?? StringUtil.newGuid(), // Need to fake Id when get data from API
                })));
            }
        }
        setContentSource({
            list: getConfigurationContentSources(newValue, isNARAFormat),
            selected: newValue,
        });
    }

    const handleSelectedNode = (treeNode) => {
        setSelectedNode(treeNode);
        setTreeNodeMetadataName(treeNode.MetadataName || "");
        if (treeNode.ChildTable && treeNode.ChildTable.length > 0) {
            setChildTableItems(treeNode.ChildTable.map((item) => ({
                ...item,
                Id: item.Id ?? StringUtil.newGuid()
            })));
        } else {
            setChildTableItems([]);
        }
    }

    const onSelectedNode = (treeNode, funcAllow) => {
        // treeNode.ChildTable is load the table of right;
        if (isEditingNodeMetadataName) {
            renderPromptForLocal(() => {
                handleCancelMetadataName();
                handleSelectedNode(treeNode);
                if (funcAllow) {
                    funcAllow(true);
                }
            });
        } else {
            handleSelectedNode(treeNode);
        }

        if (funcAllow) {
            funcAllow(!isEditingNodeMetadataName);
        }
    };

    const handleCancelMetadataName = () => {
        setTreeNodeMetadataName(selectedNode.MetadataName || "");
        setIsEditingNodeMetadataName(false);
    }

    const handleSaveMetadataName = () => {
        if (!$$.verify('raCPCompliantExportsMetadataMapping')) return false;
        setSelectedNode((prev) => ({ ...prev, MetadataName: treeNodeMetadataName }));
        exportSettingsTreeRef.current.refreshSelectedNode({
            ...selectedNode,
            MetadataName: treeNodeMetadataName,
        });
        onChangeChildTableItems(contentSource.selected, exportSettingsTreeRef.current.getTreeData()[0]);
        setIsEditingNodeMetadataName(false);
        return true;
    }

    const handleShowMappingPanel = () => {
        setShowMapingPanel(true);
    };

    const handleHideMappingPanel = () => {
        setShowMapingPanel(false);
        setMappingRowData(null);
    };

    const handleSaveMappingPanel = () => {
        if (mappingPanelRef.current) {
            if (!mappingPanelRef.current.onValidate()) return false;
            const data = mappingPanelRef.current.getMappingInfo();
            let currentChildTableItems = [...childTableItems];

            if (mappingRowData) {
                // Case edit
                currentChildTableItems = currentChildTableItems.map((item) => {
                    if (item.Id === data.Id) {
                        return { ...data };
                    }
                    return item;
                });
            } else {
                // Case create
                data.Id = StringUtil.newGuid();
                data.Order = currentChildTableItems.length + 1;
                // data.TreeNodeName = selectedNode.TreeNodeName;
                currentChildTableItems.push(data);
            }
            
            if (exportSettingsTreeRef.current && selectedFormat === ConfiguratorFormatValue.VEO) {
                // For VEO
                exportSettingsTreeRef.current.refreshSelectedNode({
                    ...selectedNode,
                    ChildTable: currentChildTableItems,
                });
                setSelectedNode((prev) => ({ ...prev, ChildTable: currentChildTableItems }));
                onChangeChildTableItems(contentSource.selected, exportSettingsTreeRef.current.getTreeData()[0]);
            } else {
                // For NAA and NARA
                onChangeChildTableItems(contentSource.selected, currentChildTableItems);
            }
            
            setChildTableItems(currentChildTableItems);
            handleHideMappingPanel();
        }
        return false;
    };

    const handleOrderChange = (newItems) => {
        const updatedNewItems = newItems.map((item) => ({
            ...item,
            Order: item.orderNumber,
        }));
        if (selectedFormat === ConfiguratorFormatValue.VEO && exportSettingsTreeRef.current) {
            exportSettingsTreeRef.current.refreshSelectedNode({
                ...selectedNode,
                ChildTable: updatedNewItems,
            });
            setChildTableItems(updatedNewItems);
            setSelectedNode((prev) => ({ ...prev, ChildTable: updatedNewItems }));
            onChangeChildTableItems(contentSource.selected, exportSettingsTreeRef.current.getTreeData()[0]);
            return;
        }
        onChangeChildTableItems(contentSource.selected, updatedNewItems);
    };

    const handleEditMapping = (rowData) => {
        handleShowMappingPanel();
        setMappingRowData(rowData);
    };

    const handleDeleteMapping = (rowData) => {
        const updatedChildtableItems = childTableItems.filter(
            (item) => item.Id !== rowData.Id
        );
        if (selectedFormat === ConfiguratorFormatValue.VEO && exportSettingsTreeRef.current) {
            exportSettingsTreeRef.current.refreshSelectedNode({
                ...selectedNode,
                ChildTable: updatedChildtableItems,
            });
            setChildTableItems(updatedChildtableItems);
            setSelectedNode((prev) => ({ ...prev, ChildTable: updatedChildtableItems }));
            onChangeChildTableItems(contentSource.selected, exportSettingsTreeRef.current.getTreeData()[0]);
            return;
        }
        onChangeChildTableItems(contentSource.selected, updatedChildtableItems);
    };

    const renderMappingPanel = () => {
        return (
            <R.Panel
                id="raMetadataMappingPanel"
                header={
                    mappingRowData
                        ? RMResx.RM_ES_CompliantExport_EditMappingTitle
                        : RMResx.RM_ES_CompliantExport_AddMappingTitle
                }
                size={668}
                status={{ show: showMappingPanel }}
                destroy={true}
                onClose={handleHideMappingPanel}
            >
                <MetadataMappingPanel
                    ref={mappingPanelRef}
                    mappingRowData={mappingRowData}
                    selectedFormat={selectedFormat}
                    selectedContentSource={contentSource.selected}
                />
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={handleHideMappingPanel}
                />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={handleSaveMappingPanel}
                />
            </R.Panel>
        );
    };

    return (
        <R.Validation>
            <div id="raCPCompliantExportsMetadataMapping">
                <Prompt message={RMResx.RM_ES_CompliantExport_Wizard_PromptLeave} when={isEditingNodeMetadataName} />
                <section className="ce-component-title-main">
                    <span tabIndex="0">
                        {RMResx.RM_ES_CompliantExport_Wizard_Step02}
                    </span>
                </section>
                <section style={{ flex: 1 }}>
                    <div tabIndex="0" className="ce-component-title-secondary">
                        {RMResx.RM_ES_CompliantExport_ContentSource}
                        <span className="ce-required-input">*</span>
                    </div>
                    <div style={{ width: 200 }}>
                        <R.Combobox
                            id="compliant-export-content-source"
                            tooltipField="text"
                            width="100%"
                            textField="text"
                            valueField="value"
                            checkedField="checked"
                            linkMode={false}
                            searchable={false}
                            items={contentSource.list}
                            willChange={(args) => {
                                if (isEditingNodeMetadataName) {
                                    renderPromptForLocal(() => {
                                        handleCancelMetadataName();
                                        handleChangeContentSource(args);
                                    });
                                    return false;
                                }
                                return true;
                            }}
                            onChange={handleChangeContentSource}
                            aria={{
                                ariaLabel:
                                    RMResx.RM_ES_CompliantExport_ContentSource,
                            }}
                        />
                    </div>
    
                    {selectedFormat === ConfiguratorFormatValue.VEO ? (
                        (
                            <div className="content-wrapper margin-top-m">
                                <div className="tree-wrapper">
                                    <ExportSettingsTree
                                        ref={exportSettingsTreeRef}
                                        items={filteredExportSettingsByCts}
                                        exportType={selectedFormat}
                                        sourceFlag={contentSource.selected}
                                        onSelectedNode={onSelectedNode}
                                        onActionNode={(nodes, actionType) => {
                                            if (actionType === "delete") {
                                                setSelectedNode(null);
                                            }
                                            setFilteredExportSettingsByCts(nodes);
                                            onChangeChildTableItems(contentSource.selected, nodes[0]);
                                        }}
                                    />
                                </div>
                                <div className="flex column-table-wrapper">
                                    <div style={{ width: "100%" }} className="flex flex-column gap-l">
                                        {selectedNode && (
                                            <>
                                                <div className="flex flex-column gap-xs padding-left-m padding-right-m">
                                                    <div tabIndex={0} className="require">
                                                        {RMResx.RM_ES_CompliantExport_Wizard_Step02_MetadataName}
                                                    </div>
                                                    {isEditingNodeMetadataName ? (
                                                        <div className="flex gap-s">
                                                            <div style={{ flex: 1 }}>
                                                                <R.Validation element="Input" require>
                                                                    <R.Input
                                                                        id="raVEONodeMetadataName"
                                                                        type="text"
                                                                        value={treeNodeMetadataName}
                                                                        width="100%"
                                                                        onChange={setTreeNodeMetadataName}
                                                                    />
                                                                </R.Validation>
                                                            </div>
                                                            <R.Button
                                                                type="icon"
                                                                icon="fia-check"
                                                                text={RMResx.RM_ES_CompliantExport_Wizard_Step02_SaveMetadataNameBtn}
                                                                classify="blank"
                                                                round={false}
                                                                onClick={handleSaveMetadataName}
                                                            />
                                                            <R.Button
                                                                type="icon"
                                                                icon="fia-close"
                                                                text={RMResx.RM_JS_Common_Cancel}
                                                                classify="blank"
                                                                round={false}
                                                                onClick={handleCancelMetadataName}
                                                            />
                                                        </div>
                                                    ) : (
                                                        <div className="flex align-center gap-s">
                                                            <div tabIndex={selectedNode.MetadataName ? 0 : -1} className="ra-ellipsis" data-tooltip='ifneed'>{selectedNode.MetadataName}</div>
                                                            <R.Button
                                                                type="icon"
                                                                icon="fia-edit"
                                                                text={RMResx.RM_ES_CompliantExport_Wizard_Step02_EditMetadataNameBtn}
                                                                classify="blank"
                                                                round={false}
                                                                onClick={() => setIsEditingNodeMetadataName(true)}
                                                            />
                                                        </div>
                                                    )}
                                                </div>
                                                <div style={{ overflow: "auto" }} className="flex padding-left-m padding-right-m padding-bottom-m">
                                                    <OrderTable
                                                        columns={columnsForVEO}
                                                        items={childTableItems}
                                                        onOrderChange={handleOrderChange}
                                                        onAddRow={handleShowMappingPanel}
                                                    />
                                                </div>
                                            </>
                                        )}
                                    </div>
                                </div>
                            </div>
                        )
                    ) : (
                        <div className="margin-top-m">
                            <OrderTable
                                columns={columns}
                                items={childTableItems}
                                onOrderChange={handleOrderChange}
                                onAddRow={handleShowMappingPanel}
                            />
                        </div>
                    )}
                </section>
                {renderMappingPanel()}
            </div>
        </R.Validation>
    );
}

export default forwardRef(MetadataMappingComponent);
