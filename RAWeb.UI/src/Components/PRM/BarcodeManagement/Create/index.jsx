import { useEffect, useMemo, useRef, useState } from "react";
import { useLocation } from "react-router-dom";

import SiteMapLinks from "../../../../Constants/SiteMapLinks";
import StringUtil from "../../../../Utilities/StringUtil";
import {
    BarcodeTemplateBuildInIDs,
    BarcodeTemplateBuildInNames,
    BarcodeTemplateType,
} from "../../Constants";
import {
    BarcodeTemplateLabelType,
    BarcodeTemplatePosition,
    DefaultAreaList,
    DefaultLabelSizeList,
    DefaultSizeList,
    RAMessageType,
} from "../config";
import RouterUrls from "../../../../Constants/RouterUrls";
import { showToast } from "../../../../Utilities/CommonUtil";

import "./index.less";

function CreateBarcodeTemplate({ history }) {
    const [isFetchingById, setIsFetchingById] = useState(false);
    const [templateSuiteInfo, setTemplateSuiteInfo] = useState({
        Name: "",
        Description: "",
        IsDefault: false,
        LabelType: BarcodeTemplateLabelType.Rectangle289x199mm,
    });
    const [tabIndex, setTabIndex] = useState(0);
    const [labelSize, setLabelSize] = useState({
        list: RM.deepcopy(DefaultLabelSizeList),
        selected: BarcodeTemplateLabelType.Avery_200x93,
        imgUrl: RM.deepcopy(DefaultLabelSizeList)[0].imgUrl,
    });
    const [boxTemplateColumns, setBoxTemplateColumns] = useState([]);
    const [folderTemplateColumns, setFolderTemplateColumns] = useState([]);
    const [boxTemplate, setBoxTemplate] = useState({
        templateId: null,
        criterias: [],
        image: {
            name: "",
            type: "",
            base64Url: "",
        },
        areaList: RM.deepcopy(DefaultAreaList),
        selectedArea: BarcodeTemplatePosition.Above,
    });
    const [boxTemplateImages, setBoxTemplateImages] = useState([]);
    const [folderTemplate, setFolderTemplate] = useState({
        templateId: null,
        criterias: [],
        image: {
            name: "",
            type: "",
            base64Url: "",
        },
        areaList: RM.deepcopy(DefaultAreaList),
        selectedArea: DefaultAreaList[0].value,
    });
    const [folderTemplateImages, setFolderTemplateImages] = useState([]);

    const templateUploaderForBoxRef = useRef();
    const templateUploaderForFolderRef = useRef();

    const location = useLocation();
    const searchParams = new URLSearchParams(location.search);
    const suiteId = searchParams.get("suiteId");

    const defaultCriteriaListForBox = useMemo(() => {
        const clonedTemplateColumns = RM.deepcopy(boxTemplateColumns);

        if (clonedTemplateColumns.length > 0) {
            clonedTemplateColumns[0].checked = true;
        }

        return [
            {
                fieldNames: clonedTemplateColumns,
                sizes: RM.deepcopy(DefaultSizeList),
                areas: RM.deepcopy(DefaultAreaList),
            },
        ];
    }, [boxTemplateColumns]);

    const defaultCriteriaListForFolder = useMemo(() => {
        const clonedTemplateColumns = RM.deepcopy(folderTemplateColumns);

        if (clonedTemplateColumns.length > 0) {
            clonedTemplateColumns[0].checked = true;
        }

        return [
            {
                fieldNames: clonedTemplateColumns,
                sizes: RM.deepcopy(DefaultSizeList),
                areas: RM.deepcopy(DefaultAreaList),
            },
        ];
    }, [folderTemplateColumns]);

    useEffect(() => {
        const clonedBoxTemplate = RM.deepcopy(boxTemplate);
        clonedBoxTemplate.criterias = RM.deepcopy(defaultCriteriaListForBox);
        setBoxTemplate(clonedBoxTemplate);
    }, [defaultCriteriaListForBox]);

    useEffect(() => {
        const clonedFolderTemplate = RM.deepcopy(folderTemplate);
        clonedFolderTemplate.criterias = RM.deepcopy(defaultCriteriaListForFolder);
        setFolderTemplate(clonedFolderTemplate);
    }, [defaultCriteriaListForFolder]);

    useEffect(() => {
        getAllTemplateColumn();
    }, [suiteId]);

    // useEffect(() => {
    //     if (suiteId) {
    //         getBarcodeTemplateBySuiteId();
    //     }
    // }, [suiteId]);

    const getTooltip = (value, splitChar = ",") => {
        if (Array.isArray(value)) {
            const newValue = value.map((o) => RMResx[o] || o);
            return newValue.join(splitChar);
        } else {
            return RMResx[value] || value;
        }
    };

    const getNewFieldsChecked = (source, field, value) => {
        return source[field].map((item) => ({
            ...item,
            checked: item.value === value,
        }))
    }

    const echoCriteriaList = (template, prev, boxTemplateColumns, folderTemplateColumns) => {
        const properties = template.Properties || [];
        const base = prev.criterias[0];
        const templateColumns = template.Type === BarcodeTemplateType.Box ? boxTemplateColumns : folderTemplateColumns;
        const templateColumnsMap = new Map(templateColumns.map((item) => [item.value, item.name]));
        const criterias = properties.filter((item) => templateColumnsMap.has(item.Name)).map((item, index) => {
            const source = prev.criterias[index] ? prev.criterias[index] : base;
            const newFieldNames = getNewFieldsChecked(source, 'fieldNames', item.Name);
            const newSizes = getNewFieldsChecked(source, 'sizes', item.FontSize);
            const newAreas = getNewFieldsChecked(source, 'areas', item.Position);
            return {
                id: item.Id,
                templateId: item.TemplateId,
                createdTime: item.CreatedTime,
                createdTimeStr: item.CreatedTimeStr,
                modifiedTime: item.ModifiedTime,
                modifiedTimeStr: item.ModifiedTimeStr,
                fieldNames: newFieldNames,
                sizes: newSizes,
                areas: newAreas,
            };
        });
        return criterias;
    }

    const echoAreaList = (prev, template) => {
        return prev.areaList.map(
            (item) => ({
                ...item,
                checked: item.value === template.LogoProperties.Position,
            })
        );
    }

    const updateTemplate = (prev, template, boxTemplateColumns, folderTemplateColumns) => {
        if (!template) return prev;
                                    
        const criterias = echoCriteriaList(template, prev, boxTemplateColumns, folderTemplateColumns);
        const newAreaList = echoAreaList(prev, template);

        return {
            templateId: template.TemplateId,
            criterias,
            image: {
                name: template.LogoProperties.LogoImgName,
                type: template.LogoProperties.LogoImgType,
                base64Url: template.LogoProperties.LogoImgBase64Str,
            },
            areaList: newAreaList,
            selectedArea: template.LogoProperties.Position
        };
    }

    const dataURLtoBlob = (dataurl) => {
        let arr = dataurl.split(","),
            mime = arr[0].match(/:(.*?);/)[1],
            bstr = atob(arr[1]),
            n = bstr.length,
            u8arr = new Uint8Array(n);
        while (n--) {
            u8arr[n] = bstr.charCodeAt(n);
        }
        return new Blob([u8arr], { type: mime });
    }

    const blobToFile = (theBlob, fileName) => {
        theBlob.lastModifiedDate = new Date();
        theBlob.name = fileName;
        return theBlob;
    }

    const handleGetTemplateImage = (template) => {
        const base64Data = template.LogoProperties.LogoImgBase64Str;
        if (base64Data) {
            const blob = dataURLtoBlob(base64Data);
            const file = blobToFile(
                blob,
                template.LogoProperties.LogoImgName
            );
            file.fileName = file.name;
            file.fileExtension = template.LogoProperties.LogoImgType;
            file.fileId = StringUtil.newGuid();

            if (file) {
                return [file];
            } else {
                return file;
            }
        } else {
            return null;
        }
    }

    const getBarcodeTemplateBySuiteId = (boxTemplateColumns, folderTemplateColumns) => {
        $$.loading(true);
        const url = `/Api/TemplateManagementApi/GetBarcodeTemplateBySuiteId?suiteId=${suiteId}`;
        const option = {
            url: url,
            method: "GET",
        };
        fetchUtility(option)
            .then((res) => {
                if (res) {
                    if (res.IsDefault) {
                        history.replace({
                            pathname: RouterUrls.PRM_BarcodeManagement_EditDefault,
                            search: `?suiteId=${suiteId}`,
                        });
                    } else {
                        const clonedDefaultLabelSizeList = RM.deepcopy(DefaultLabelSizeList);
                        setTemplateSuiteInfo(res);
                        setLabelSize({
                            list: clonedDefaultLabelSizeList.map((item) => ({
                                ...item,
                                checked: item.value === res.LabelType,
                            })),
                            selected: res.LabelType,
                            imgUrl: clonedDefaultLabelSizeList.find((item) => item.value === res.LabelType)?.imgUrl || "",
                        });
                        const templates = res.Templates || [];
                        templates.forEach((template) => {
                            if (template.Type === BarcodeTemplateType.Box) {
                                const templateImageForBox = handleGetTemplateImage(template);
                                setBoxTemplateImages(templateImageForBox);
                                setBoxTemplate((prev) => updateTemplate(prev, template, boxTemplateColumns, folderTemplateColumns));
                            } else {
                                const templateImageForFolder = handleGetTemplateImage(template);
                                setFolderTemplateImages(templateImageForFolder);
                                setFolderTemplate((prev) => updateTemplate(prev, template, boxTemplateColumns, folderTemplateColumns));
                            }
                        });
                    }
                }
            })
            .finally(() => {
                $$.loading(false);
            });
    };

    const handleSetTemplateColumn = (templateColumnsResponse) => {
        const templateColumns = [];
        for (let key in templateColumnsResponse) {
            const column = {};
            // When the return value key is Guid, it means that every template has this column, and the tooltip displays Built-in Column;
            if (BarcodeTemplateBuildInIDs.indexOf(key) != -1) {
                column.name = BarcodeTemplateBuildInNames[key];
                column.tooltip =
                    RMResx.RM_PRM_BarcodeTemp_AreaF_BuildInColumn;
            } else {
                column.name = RMResx[key] || key;
                if (templateColumnsResponse[key]) {
                    column.tooltip = getTooltip(templateColumnsResponse[key]);
                }
            }
            column.value = key;
            templateColumns.push(column);
        }
        return templateColumns;
    }

    const getAllTemplateColumn = () => {
        $$.loading(true);
        setIsFetchingById(true);
        const url = "/Api/TemplateManagementApi/GetAllTemplateColumn";
        const option = {
            url: url,
            method: "GET",
        };
        fetchUtility(option)
            .then((res) => {
                const templateColumnsforBox = handleSetTemplateColumn(res.BoxTemplateColumns);
                const templateColumnsforFolder = handleSetTemplateColumn(res.FolderTemplateColumns);
                setBoxTemplateColumns(templateColumnsforBox);
                setFolderTemplateColumns(templateColumnsforFolder);

                if (suiteId) {
                    getBarcodeTemplateBySuiteId(templateColumnsforBox, templateColumnsforFolder);
                }
            })
            .finally((e) => {
                $$.loading(false);
                setIsFetchingById(false);
            });
    };

    const handleChangeTemplateInput = (value, field) => {
        const clonedTemplateSuiteInfo = RM.deepcopy(templateSuiteInfo);
        clonedTemplateSuiteInfo[field] = value;
        setTemplateSuiteInfo(clonedTemplateSuiteInfo);
    };

    const handleChangeLabelSize = (args) => {
        const newValue = args.newValue.value;
        const newImgUrl = args.newValue.imgUrl;
        const clonedLabelSize = RM.deepcopy(labelSize);
        clonedLabelSize.list = clonedLabelSize.list.map((item) => ({
            ...item,
            checked: item.value === newValue,
        }));
        clonedLabelSize.selected = newValue;
        clonedLabelSize.imgUrl = newImgUrl;
        setLabelSize(clonedLabelSize);
    };

    const handleChangeAreaListForBox = (args) => {
        const newValue = args.newValue.value;
        const newAreaList = boxTemplate.areaList.map((item) => ({
            ...item,
            checked: item.value === newValue,
        }));
        setBoxTemplate((prev) => ({
            ...prev,
            areaList: newAreaList,
            selectedArea: newValue,
        }));
    };

    const handleChangeAreaListForFolder = (args) => {
        const newValue = args.newValue.value;
        const newAreaList = folderTemplate.areaList.map((item) => ({
            ...item,
            checked: item.value === newValue,
        }));
        setFolderTemplate((prev) => ({
            ...prev,
            areaList: newAreaList,
            selectedArea: newValue,
        }));
    };

    const handleUploadTemplateImageForBox = (args) => {
        const boxTemplateImageInfo = RM.deepcopy(boxTemplate.image);
        let fileInfo = args.files[0].file;
        let reader = new FileReader();
        reader.readAsDataURL(fileInfo);
        reader.onload = () => {
            boxTemplateImageInfo.base64Url = reader.result;
            boxTemplateImageInfo.name = args.files[0].fileName;
            boxTemplateImageInfo.type = args.files[0].fileExtension;
            setBoxTemplate((prev) => ({
                ...prev,
                image: boxTemplateImageInfo,
            }));
        };
        for (let item of args.files) {
            item.fileId = StringUtil.newGuid();
        }
        setBoxTemplateImages(args.files);
    };

    const handleDeleteTemplateImageForBox = (args) => {
        if (args.isSucceed) {
            const boxTemplateImageInfo = RM.deepcopy(boxTemplate.image);
            boxTemplateImageInfo.name = "";
            boxTemplateImageInfo.type = "";
            boxTemplateImageInfo.base64Url = "";

            templateUploaderForBoxRef.current = null;
            setBoxTemplate((prev) => ({
                ...prev,
                image: boxTemplateImageInfo,
            }));
        }
    };

    const handleUploadTemplateImageForFolder = (args) => {
        const folderTemplateImageInfo = RM.deepcopy(folderTemplate.image);
        let fileInfo = args.files[0].file;
        let reader = new FileReader();
        reader.readAsDataURL(fileInfo);
        reader.onload = () => {
            folderTemplateImageInfo.base64Url = reader.result;
            folderTemplateImageInfo.name = args.files[0].fileName;
            folderTemplateImageInfo.type = args.files[0].fileExtension;
            setFolderTemplate((prev) => ({
                ...prev,
                image: folderTemplateImageInfo,
            }));
        };
        for (let item of args.files) {
            item.fileId = StringUtil.newGuid();
        }
        setFolderTemplateImages(args.files);
    };

    const handleDeleteTemplateImageForFolder = (args) => {
        if (args.isSucceed) {
            const folderTemplateImageInfo = RM.deepcopy(folderTemplate.image);
            folderTemplateImageInfo.name = "";
            folderTemplateImageInfo.type = "";
            folderTemplateImageInfo.base64Url = "";

            templateUploaderForFolderRef.current = null;
            setFolderTemplate((prev) => ({
                ...prev,
                image: folderTemplateImageInfo,
            }));
        }
    };

    const handleChangeCriteriasForBox = (action, index) => {
        const clonedBoxTemplate = RM.deepcopy(boxTemplate);
        if (action === "delete") {
            clonedBoxTemplate.criterias = clonedBoxTemplate.criterias.filter(
                (_, idx) => idx !== index
            );
        }

        if (action === "add") {
            clonedBoxTemplate.criterias = [
                ...clonedBoxTemplate.criterias,
                ...defaultCriteriaListForBox,
            ];
        }
        setBoxTemplate(clonedBoxTemplate);
    };

    const handleChangeCriteriasForFolder = (action, index) => {
        const clonedFolderTemplate = RM.deepcopy(folderTemplate);
        if (action === "delete") {
            clonedFolderTemplate.criterias =
                clonedFolderTemplate.criterias.filter(
                    (_, idx) => idx !== index
                );
        }

        if (action === "add") {
            clonedFolderTemplate.criterias = [
                ...clonedFolderTemplate.criterias,
                ...defaultCriteriaListForFolder,
            ];
        }
        setFolderTemplate(clonedFolderTemplate);
    };

    const handleChangeDropdownRowForBox = (index, field, args) => {
        const clonedBoxTemplate = RM.deepcopy(boxTemplate);
        const newValue = args.newValue.value;
        clonedBoxTemplate.criterias[index][field] = clonedBoxTemplate.criterias[
            index
        ][field].map((item) => ({
            ...item,
            checked: item.value === newValue,
        }));
        setBoxTemplate(clonedBoxTemplate);
    };

    const handleChangeDropdownRowForFolder = (index, field, args) => {
        const clonedFolderTemplate = RM.deepcopy(folderTemplate);
        const newValue = args.newValue.value;
        clonedFolderTemplate.criterias[index][field] =
            clonedFolderTemplate.criterias[index][field].map((item) => ({
                ...item,
                checked: item.value === newValue,
            }));
        setFolderTemplate(clonedFolderTemplate);
    };

    const handleCancel = () => {
        history.push({
            pathname: RouterUrls.PRM_BarcodeManagement,
        });
    };

    const getTemplatePayloadForSave = (barcodeTemplateType, templateType) => {
        const templateInfo = {
            Type: barcodeTemplateType,
            LogoProperties: {
                LogoImgBase64Str: templateType.image.base64Url,
                LogoImgName: templateType.image.name,
                LogoImgType: templateType.image.type,
                Position: templateType.selectedArea,
            },
            Properties: templateType.criterias.map((item) => {
                let obj = {
                    Name: item.fieldNames.find((item) => item.checked)?.value || "",
                    DisplayName: item.fieldNames.find((item) => item.checked)?.name || "",
                    FontSize: item.sizes.find((item) => item.checked)?.value || "",
                    Position: item.areas.find((item) => item.checked)?.value ?? "", // area
                }

                if (suiteId) {
                    obj = {
                        ...obj,
                        Id: item.id,
                        TemplateId: templateType.criterias[0].templateId,
                        CreatedTime: item.createdTime,
                        CreatedTimeStr: item.createdTimeStr,
                        ModifiedTime: item.modifiedTime,
                        ModifiedTimeStr: item.modifiedTimeStr,
                    }
                }

                return obj;
            }),
        };

        // Edit case
        if (suiteId) {
            templateInfo.TemplateId = templateType.templateId;
        }

        return templateInfo;
    }

    const handlePreview = () => {
        if (!$$.verify("allValidation")) return false;

        const boxTemplateInfo = getTemplatePayloadForSave(BarcodeTemplateType.Box, boxTemplate);
        const folderTemplateInfo = getTemplatePayloadForSave(BarcodeTemplateType.Folder, folderTemplate);

        const templateInfos = {
            ...templateSuiteInfo,
            LabelType: labelSize.selected,
            Templates: [boxTemplateInfo, folderTemplateInfo],
        };
        $$.loading(true);
        const divElement = document.getElementById("downloadTemplate");
        const downloadUrl = "/Api/TemplateManagementApi/ExportPreviewBarcode";
        ReactDOM.render(
            <form action={downloadUrl} method="POST">
                <input
                    type="hidden"
                    name="TemplateInfoes"
                    value={JSON.stringify(templateInfos)}
                ></input>
            </form>,
            divElement
        );
        divElement.querySelector("form").submit();
        ReactDOM.unmountComponentAtNode(divElement);
        $$.loading(false);
    };

    const handleSave = () => {
        if (!$$.verify("allValidation")) return false;

        let url = "/Api/TemplateManagementApi/CreateCustomBarcodeTemplate";
        if (suiteId) {
            url = "/Api/TemplateManagementApi/UpdateCustomBarcodeTemplate";
        }

        const boxTemplateInfo = getTemplatePayloadForSave(BarcodeTemplateType.Box, boxTemplate);
        const folderTemplateInfo = getTemplatePayloadForSave(BarcodeTemplateType.Folder, folderTemplate);

        const payload = {
            ...templateSuiteInfo,
            LabelType: labelSize.selected,
            Templates: [boxTemplateInfo, folderTemplateInfo],
        };

        if (suiteId) {
            payload.SuiteId = suiteId;
        }

        const option = {
            url,
            method: "POST",
            data: payload,
        };
        $$.loading(true);
        fetchUtility(option)
            .then((res) => {
                if (res.MessageType === RAMessageType.Successful) {
                    showToast.success(suiteId ? RMResx.RM_PRM_TM_Records_Template_EditSuccess : RMResx.RM_PRM_TM_Records_Template_CreateSuccess);
                    handleCancel();
                } else {
                    showToast.error(res.ErrorMessage);
                }
            })
            .finally(() => $$.loading(false));
    };

    // Renders
    const renderTabControls = () => {
        return (
            <R.Tabcontrol
                type="underline"
                active={tabIndex}
                onChange={setTabIndex}
                flex={true}
            >
                <R.TabPanel
                    tab={RMResx.RM_PRM_TM_Barcode_Template_BoxTab}
                    aria-label={RMResx.RM_PRM_TM_Barcode_Template_BoxTab}
                    data-tooltip="ifneed"
                ></R.TabPanel>
                <R.TabPanel
                    tab={RMResx.RM_PRM_TM_Barcode_Template_FolderTab}
                    aria-label={RMResx.RM_PRM_TM_Barcode_Template_FolderTab}
                    data-tooltip="ifneed"
                ></R.TabPanel>
            </R.Tabcontrol>
        );
    };

    const renderColumnTableForBox = () => {
        return boxTemplate.criterias.map((item, index) => (
            <div
                key={index}
                className="create-barcode-template-configuration-table-row"
            >
                <div
                    style={{ marginTop: 1 }}
                    className="create-barcode-template-configuration-table-row-first"
                >
                    <R.Combobox
                        id={`raCreateBarcodeFieldNameCbx-${index}`}
                        mini
                        height={32}
                        textField="name"
                        valueField="value"
                        checkedField="checked"
                        tooltipField="tooltip"
                        width="100%"
                        searchable={false}
                        items={item.fieldNames}
                        onChange={(args) =>
                            handleChangeDropdownRowForBox(
                                index,
                                "fieldNames",
                                args
                            )
                        }
                    />
                </div>
                <div
                    style={{ marginTop: 1 }}
                    className="create-barcode-template-configuration-table-row-second"
                >
                    <R.Combobox
                        id={`raCreateBarcodeSizeCbx-${index}`}
                        mini
                        height={32}
                        textField="name"
                        valueField="value"
                        checkedField="checked"
                        tooltipField="name"
                        width="100%"
                        searchable={false}
                        items={item.sizes}
                        onChange={(args) =>
                            handleChangeDropdownRowForBox(index, "sizes", args)
                        }
                    />
                </div>
                <div style={{ marginTop: 1, flex: 1 }}>
                    <R.Combobox
                        id={`raCreateBarcodeAreaCbx-${index}`}
                        mini
                        height={32}
                        textField="name"
                        valueField="value"
                        checkedField="checked"
                        tooltipField="name"
                        width="100%"
                        searchable={false}
                        items={item.areas}
                        onChange={(args) =>
                            handleChangeDropdownRowForBox(index, "areas", args)
                        }
                    />
                </div>
                {boxTemplate.criterias.length > 1 && (
                    <div style={{ width: "8%" }}>
                        <R.Button
                            type="bald"
                            icon="fia-delete"
                            onClick={() =>
                                handleChangeCriteriasForBox("delete", index)
                            }
                            tooltip={RMResx.RM_JS_Common_Delete}
                        />
                    </div>
                )}
            </div>
        ));
    };

    const renderColumnTableForFolder = () => {
        return folderTemplate.criterias.map((item, index) => (
            <div
                key={index}
                className="create-barcode-template-configuration-table-row"
            >
                <div
                    style={{ marginTop: 1 }}
                    className="create-barcode-template-configuration-table-row-first"
                >
                    <R.Combobox
                        id={`raCreateBarcodeFieldNameCbx-${index}`}
                        mini
                        height={32}
                        textField="name"
                        valueField="value"
                        checkedField="checked"
                        tooltipField="tooltip"
                        width="100%"
                        searchable={false}
                        items={item.fieldNames}
                        onChange={(args) =>
                            handleChangeDropdownRowForFolder(
                                index,
                                "fieldNames",
                                args
                            )
                        }
                    />
                </div>
                <div
                    style={{ marginTop: 1 }}
                    className="create-barcode-template-configuration-table-row-second"
                >
                    <R.Combobox
                        id={`raCreateBarcodeSizeCbx-${index}`}
                        mini
                        height={32}
                        textField="name"
                        valueField="value"
                        checkedField="checked"
                        tooltipField="name"
                        width="100%"
                        searchable={false}
                        items={item.sizes}
                        onChange={(args) =>
                            handleChangeDropdownRowForFolder(
                                index,
                                "sizes",
                                args
                            )
                        }
                    />
                </div>
                <div style={{ marginTop: 1, flex: 1 }}>
                    <R.Combobox
                        id={`raCreateBarcodeAreaCbx-${index}`}
                        mini
                        height={32}
                        textField="name"
                        valueField="value"
                        checkedField="checked"
                        tooltipField="name"
                        width="100%"
                        searchable={false}
                        items={item.areas}
                        onChange={(args) =>
                            handleChangeDropdownRowForFolder(
                                index,
                                "areas",
                                args
                            )
                        }
                    />
                </div>
                {folderTemplate.criterias.length > 1 && (
                    <div style={{ width: "8%" }}>
                        <R.Button
                            type="bald"
                            icon="fia-delete"
                            onClick={() =>
                                handleChangeCriteriasForFolder("delete", index)
                            }
                            tooltip={RMResx.RM_JS_Common_Delete}
                        />
                    </div>
                )}
            </div>
        ));
    };

    const renderConfigurationForBox = () => {
        return (
            <div
                className={`${
                    tabIndex === 0 ? "flex flex-column gap-l" : "none"
                }`}
            >
                <div className="flex flex-column gap-xs">
                    {renderTabControls()}
                    <div className="create-barcode-template-configuration-table-header">
                        <div tabIndex={0}>
                            {RMResx.RM_PRM_TM_Barcode_Template_FieldName}
                        </div>
                        <div tabIndex={0}>
                            {RMResx.RM_PRM_TM_Barcode_Template_Size}
                        </div>
                        <div tabIndex={0}>
                            {RMResx.RM_PRM_TM_Barcode_Template_Area}
                        </div>
                    </div>
                    {renderColumnTableForBox()}
                    <div
                        tabIndex="0"
                        className="create-barcode-template-configuration-table-add"
                        onClick={() => handleChangeCriteriasForBox("add")}
                        onKeyDown={() => {}}
                    >
                        <div className="create-barcode-template-configuration-table-add-icon">
                            <div className="fia-plus"></div>
                        </div>
                        <span>{RMResx.RM_PRM_TM_Barcode_Template_AddBtn}</span>
                    </div>
                </div>
                <div className="flex flex-column gap-xs">
                    <div
                        className="create-barcode-template-configuration-label"
                        tabIndex={0}
                    >
                        {RMResx.RM_PRM_TM_Barcode_Template_LogoOrIcon}
                    </div>
                    <R.Uploader
                        showTypes
                        ref={templateUploaderForBoxRef}
                        files={boxTemplateImages || []}
                        fileTypes={[
                            "PNG",
                            "JPG",
                            "PJP",
                            "JPEG",
                            "JFIF",
                            "PJPEG",
                        ]}
                        maxSize="1MB"
                        showMaxSize={true}
                        onUpload={handleUploadTemplateImageForBox}
                        onDelete={handleDeleteTemplateImageForBox}
                    />
                </div>
                <div className="flex flex-column gap-xs">
                    <div
                        className="create-barcode-template-configuration-label"
                        tabIndex={0}
                    >
                        {RMResx.RM_PRM_TM_Barcode_Template_LogoOrIconArea}
                    </div>
                    <R.Combobox
                        id="raCreateBarcodeLogoOrIconAreaCbx"
                        textField="name"
                        valueField="value"
                        checkedField="checked"
                        tooltipField="name"
                        width="100%"
                        searchable={false}
                        items={boxTemplate.areaList}
                        onChange={handleChangeAreaListForBox}
                    />
                </div>
            </div>
        );
    };

    const renderConfigurationForFolder = () => {
        return (
            <div
                className={`${
                    tabIndex === 0 ? "none" : "flex flex-column gap-l"
                }`}
            >
                <div className="flex flex-column gap-xs">
                    {renderTabControls()}
                    <div className="create-barcode-template-configuration-table-header">
                        <div tabIndex={0}>
                            {RMResx.RM_PRM_TM_Barcode_Template_FieldName}
                        </div>
                        <div tabIndex={0}>
                            {RMResx.RM_PRM_TM_Barcode_Template_Size}
                        </div>
                        <div tabIndex={0}>
                            {RMResx.RM_PRM_TM_Barcode_Template_Area}
                        </div>
                    </div>
                    {renderColumnTableForFolder()}
                    <div
                        tabIndex="0"
                        className="create-barcode-template-configuration-table-add"
                        onClick={() => handleChangeCriteriasForFolder("add")}
                        onKeyDown={() => {}}
                    >
                        <div className="create-barcode-template-configuration-table-add-icon">
                            <div className="fia-plus"></div>
                        </div>
                        <span>{RMResx.RM_PRM_TM_Barcode_Template_AddBtn}</span>
                    </div>
                </div>
                <div className="flex flex-column gap-xs">
                    <div
                        className="create-barcode-template-configuration-label"
                        tabIndex={0}
                    >
                        {RMResx.RM_PRM_TM_Barcode_Template_LogoOrIcon}
                    </div>
                    <R.Uploader
                        showTypes
                        ref={templateUploaderForFolderRef}
                        files={folderTemplateImages || []}
                        fileTypes={[
                            "PNG",
                            "JPG",
                            "PJP",
                            "JPEG",
                            "JFIF",
                            "PJPEG",
                        ]}
                        maxSize="1MB"
                        showMaxSize={true}
                        onUpload={handleUploadTemplateImageForFolder}
                        onDelete={handleDeleteTemplateImageForFolder}
                    />
                </div>
                <div className="flex flex-column gap-xs">
                    <div
                        className="create-barcode-template-configuration-label"
                        tabIndex={0}
                    >
                        {RMResx.RM_PRM_TM_Barcode_Template_LogoOrIconArea}
                    </div>
                    <R.Combobox
                        id="raCreateBarcodeLogoOrIconAreaCbx"
                        textField="name"
                        valueField="value"
                        checkedField="checked"
                        tooltipField="name"
                        width="100%"
                        searchable={false}
                        items={folderTemplate.areaList}
                        onChange={handleChangeAreaListForFolder}
                    />
                </div>
            </div>
        );
    };

    if (isFetchingById) {
        return null;
    }

    return (
        <div id="rmBarcodeCreateTemplate" className="rm-tm-main-container">
            <section>
                <$g.SiteMap
                    data={[
                        SiteMapLinks.PRM_BarcodeManagement,
                        suiteId
                            ? SiteMapLinks.PRM_BarcodeManagement_Edit
                            : SiteMapLinks.PRM_BarcodeManagement_Create,
                    ]}
                />
            </section>
            <section id="createBTContainer" className="rm-tm-content">
                <div className="create-barcode-template-wrapper">
                    <section className="create-barcode-template-label-format">
                        <div tabIndex={0}>
                            {RMResx.RM_PRM_TM_Barcode_Template_LabelFormat}
                        </div>
                        <div className="text-center">
                            <img src={labelSize.imgUrl} alt={RMResx.RM_JS_Common_RecourdAutomation} />
                        </div>
                    </section>
                    <section className="create-barcode-template-configuration">
                        <>
                            <div
                                className="create-barcode-template-configuration-title"
                                tabIndex={0}
                            >
                                {RMResx.RM_PRM_BarcodeTemp_Confiuration}
                            </div>
                            <div
                                className="create-barcode-template-configuration-desc"
                                tabIndex={0}
                            >
                                {RMResx.RM_PRM_BarcodeTemp_Confiuration_explain}
                            </div>
                        </>
                        <div style={{ flex: 1 }}>
                            <R.Validation>
                                <div
                                    id="allValidation"
                                    className="flex flex-column gap-l margin-top-l"
                                >
                                    <div className="flex flex-column gap-xs">
                                        <div
                                            className="create-barcode-template-configuration-label require"
                                            tabIndex={0}
                                        >
                                            {
                                                RMResx.RM_PRM_TM_Barcode_Template_Name
                                            }
                                        </div>
                                        <R.Validation element="Input" require>
                                            <R.Input
                                                placeholder={
                                                    RMResx.RM_PRM_TM_Barcode_Template_Name_Placeholder
                                                }
                                                width="100%"
                                                value={templateSuiteInfo.Name}
                                                onChange={(value) =>
                                                    handleChangeTemplateInput(
                                                        value,
                                                        "Name"
                                                    )
                                                }
                                                aria={{
                                                    ariaLabel:
                                                        RMResx.RM_PRM_TM_Barcode_Template_Name,
                                                }}
                                            />
                                        </R.Validation>
                                    </div>
                                    <div className="flex flex-column gap-xs">
                                        <div
                                            className="create-barcode-template-configuration-label"
                                            tabIndex={0}
                                        >
                                            {
                                                RMResx.RM_PRM_TM_Barcode_Template_Description
                                            }
                                        </div>
                                        <R.Input
                                            placeholder={
                                                RMResx.RM_PRM_TM_Barcode_Template_Description_Placeholder
                                            }
                                            width="100%"
                                            type="textarea"
                                            resize="vertical"
                                            height={80}
                                            value={
                                                templateSuiteInfo.Description
                                            }
                                            onChange={(value) =>
                                                handleChangeTemplateInput(
                                                    value,
                                                    "Description"
                                                )
                                            }
                                            aria={{
                                                ariaLabel:
                                                    RMResx.RM_PRM_TM_Barcode_Template_Description,
                                            }}
                                        />
                                    </div>
                                    <div className="flex flex-column gap-xs">
                                        <div
                                            className="create-barcode-template-configuration-label"
                                            tabIndex={0}
                                        >
                                            {
                                                RMResx.RM_PRM_TM_Barcode_Template_LabelSize
                                            }
                                        </div>
                                        <R.Combobox
                                            id="raCreateBarcodeLabelSizeCbx"
                                            textField="name"
                                            valueField="value"
                                            checkedField="checked"
                                            tooltipField="name"
                                            width="100%"
                                            searchable={false}
                                            items={labelSize.list}
                                            onChange={handleChangeLabelSize}
                                        />
                                    </div>
                                    {renderConfigurationForBox()}
                                    {renderConfigurationForFolder()}
                                </div>
                            </R.Validation>
                        </div>
                        <div className="flex justify-end align-center gap-s">
                            <R.Button
                                text={RMResx.RM_JS_Common_Cancel}
                                onClick={handleCancel}
                            />
                            <R.Button
                                text={RMResx.RM_PRM_BarcodeTemp_Preview}
                                onClick={handlePreview}
                            />
                            <R.Button
                                primary={true}
                                classify="theme"
                                text={RMResx.RM_JS_Common_Save}
                                onClick={handleSave}
                            />
                            <div
                                id="downloadTemplate"
                                style={{ display: "none" }}
                            />
                        </div>
                    </section>
                </div>
            </section>
        </div>
    );
}

export default CreateBarcodeTemplate;
