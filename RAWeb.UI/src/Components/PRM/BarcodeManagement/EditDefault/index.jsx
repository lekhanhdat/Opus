import { Fragment } from "react";

import SiteMapLinks from "../../../../Constants/SiteMapLinks";
import StringUtil from "../../../../Utilities/StringUtil";
import RouterUrls from "../../../../Constants/RouterUrls";
import {
    BarcodeTemplateType,
    BarcodeTemplateComboboxNames,
    BarcodeTemplateBuildInIDs,
    BarcodeTemplateBuildInNames,
} from "../../Constants";
import { showToast } from "../../../../Utilities/CommonUtil";

import "../../../../Less/PRM/barcodeTemplete.less";
import { RAMessageType } from "../config";

export default class EditBarcodeTemplate extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            tabTitles: this.getTabPanels(),
            tabIndex: 0,
            templateColumns: [],
            templateSuiteInfo: null,

            // Box template
            boxTemplateColumns: [],
            barcodeTempAreaInfoForBox: {},
            barcodeTempPreviewInfoForBox: {
                selectedAreaBName: "",
                selectedAreaCName: "",
                selectedAreaDNames: [],
                selectedAreaEName: "",
                selectedAreaFName: "",
                uploadTemplateUrl: "",
                ImageName: "",
                ImageType: "",
                id: "",
            },
            barcodeTempAreaValueForBox: {
                selectedAreaBValue: "",
                selectedAreaCValue: "",
                selectedAreaDValues: [],
                selectedAreaEValue: "",
                selectedAreaFValue: "",
            },
            templateImageForBox: [],
            areaDLimitValidForBox: false,

            // Folder template
            folderTemplateColumns: [],
            barcodeTempAreaInfoForFolder: {},
            barcodeTempPreviewInfoForFolder: {
                selectedAreaBName: "",
                selectedAreaCName: "",
                selectedAreaDNames: [],
                selectedAreaEName: "",
                selectedAreaFName: "",
                uploadTemplateUrl: "",
                ImageName: "",
                ImageType: "",
                id: "",
            },
            barcodeTempAreaValueForFolder: {
                selectedAreaBValue: "",
                selectedAreaCValue: "",
                selectedAreaDValues: [],
                selectedAreaEValue: "",
                selectedAreaFValue: "",
            },
            templateImageForFolder: [],
            areaDLimitValidForFolder: false,
            isValidForBox: true,
            isValidForFolder: true,
        };
    }

    componentInit() {
        this.getAllTemplateColumn();
    }

    showMessageTip(type, msg) {
        showToast._showMsg(type, msg);
    }

    routerTo(routerUrl) {
        this.props.history.push({
            pathname: routerUrl,
        });
    }

    getTabPanels() {
        const tabTitles = [
            RMResx.RM_PRM_TM_Barcode_Template_BoxTab,
            RMResx.RM_PRM_TM_Barcode_Template_FolderTab,
        ];
        return tabTitles;
    }

    getSelectedAreaName(valueInfo, stateField) {
        let templateColumns = RM.deepcopy(this.state[stateField]);
        let selectedAreaNames = "";
        if (valueInfo) {
            if (typeof valueInfo == "string") {
                let selectedAreaNameList = templateColumns.filter((item) => {
                    return item.value == valueInfo;
                });
                if (selectedAreaNameList.length > 0) {
                    selectedAreaNames = selectedAreaNameList[0].name;
                }
            } else {
                let selectedAreaNameList = templateColumns.filter((item) => {
                    return valueInfo.indexOf(item.value) != -1;
                });
                if (selectedAreaNameList) {
                    selectedAreaNames = selectedAreaNameList.map((item) => {
                        return item.name;
                    });
                }
            }
        }
        return selectedAreaNames;
    }

    getEchoAreaList(valueInfo, stateField) {
        let templateColumns = RM.deepcopy(this.state[stateField]);
        if (valueInfo) {
            if (typeof valueInfo == "string") {
                for (let item of templateColumns) {
                    if (valueInfo == item.value) {
                        item.checked = true;
                        break;
                    }
                }
            } else {
                templateColumns.filter((item, index) => {
                    if (valueInfo.indexOf(item.value) != -1) {
                        item.checked = true;
                    }
                });
            }
        }
        return templateColumns;
    }

    getTooltip(value, splitChar = ",") {
        if (Array.isArray(value)) {
            let newValue = value.map((o) => RMResx[o] || o);
            return newValue.join(splitChar);
        } else {
            return RMResx[value] || value;
        }
    }

    getBarcodeTemplateBySuiteId = () => {
        const searchParams = new URLSearchParams(this.props.location.search);
        const suiteId = searchParams.get("suiteId");
        $$.loading(true);
        const url = `/Api/TemplateManagementApi/GetBarcodeTemplateBySuiteId?suiteId=${suiteId}`;
        const option = {
            url: url,
            method: "GET",
        };
        fetchUtility(option)
            .then((res) => {
                if (res) {
                    this.setState({
                        templateSuiteInfo: res,
                    });
                    const templates = res.Templates || [];
                    const barcodeTempAreaInfos = {};
                    const barcodeTempPreviewInfos = {};
                    const barcodeTempAreaValues = {};
                    const templateImages = {};
                    for (let i = 0; i < templates.length; i++) {
                        const barcodeTempAreaB = this.getEchoAreaList(
                            templates[i].ColumnB,
                            templates[i].Type === BarcodeTemplateType.Box ? "boxTemplateColumns" : "folderTemplateColumns"
                        );
                        const barcodeTempAreaC = this.getEchoAreaList(
                            templates[i].ColumnC,
                            templates[i].Type === BarcodeTemplateType.Box ? "boxTemplateColumns" : "folderTemplateColumns"
                        );
                        const barcodeTempAreaD = this.getEchoAreaList(
                            templates[i].ColumnD,
                            templates[i].Type === BarcodeTemplateType.Box ? "boxTemplateColumns" : "folderTemplateColumns"
                        );
                        const barcodeTempAreaE = this.getEchoAreaList(
                            templates[i].ColumnE,
                            templates[i].Type === BarcodeTemplateType.Box ? "boxTemplateColumns" : "folderTemplateColumns"
                        );
                        const barcodeTempAreaF = this.getEchoAreaList(
                            templates[i].ColumnF,
                            templates[i].Type === BarcodeTemplateType.Box ? "boxTemplateColumns" : "folderTemplateColumns"
                        );
                        const barcodeTempAreaInfo = [
                            barcodeTempAreaB,
                            barcodeTempAreaC,
                            barcodeTempAreaD,
                            barcodeTempAreaE,
                            barcodeTempAreaF,
                        ];
                        const barcodeTempPreviewInfo = {
                            selectedAreaBName: this.getSelectedAreaName(
                                templates[i].ColumnB,
                                templates[i].Type === BarcodeTemplateType.Box ? "boxTemplateColumns" : "folderTemplateColumns"
                            ),
                            selectedAreaCName: this.getSelectedAreaName(
                                templates[i].ColumnC,
                                templates[i].Type === BarcodeTemplateType.Box ? "boxTemplateColumns" : "folderTemplateColumns"
                            ),
                            selectedAreaDNames: this.getSelectedAreaName(
                                templates[i].ColumnD,
                                templates[i].Type === BarcodeTemplateType.Box ? "boxTemplateColumns" : "folderTemplateColumns"
                            ),
                            selectedAreaEName: this.getSelectedAreaName(
                                templates[i].ColumnE,
                                templates[i].Type === BarcodeTemplateType.Box ? "boxTemplateColumns" : "folderTemplateColumns"
                            ),
                            selectedAreaFName: this.getSelectedAreaName(
                                templates[i].ColumnF,
                                templates[i].Type === BarcodeTemplateType.Box ? "boxTemplateColumns" : "folderTemplateColumns"
                            ),
                            uploadTemplateUrl: templates[i].ImgBase64Str,
                            ImageName: templates[i].ImageName,
                            ImageType: templates[i].ImageType,
                            id: templates[i].Id,
                        };
                        const barcodeTempAreaValue = {
                            selectedAreaBValue: templates[i].ColumnB,
                            selectedAreaCValue: templates[i].ColumnC,
                            selectedAreaDValues: templates[i].ColumnD,
                            selectedAreaEValue: templates[i].ColumnE,
                            selectedAreaFValue: templates[i].ColumnF,
                        };
                        barcodeTempAreaInfos[i] = barcodeTempAreaInfo;
                        barcodeTempPreviewInfos[i] = barcodeTempPreviewInfo;
                        barcodeTempAreaValues[i] = barcodeTempAreaValue;

                        // this.setState({
                        //     barcodeTempAreaInfo,
                        //     barcodeTempPreviewInfo: {},
                        //     barcodeTempAreaValues: {},
                        // });

                        // Convert base64 to file object
                        const base64Data = templates[i].ImgBase64Str;
                        if (base64Data) {
                            const blob = this.dataURLtoBlob(base64Data);
                            const file = this.blobToFile(
                                blob,
                                templates[i].ImageName
                            );
                            file.fileName = file.name;
                            file.fileExtension = templates[i].ImageType;
                            file.fileId = StringUtil.newGuid();

                            if (file) {
                                templateImages[i] = [file];
                            } else {
                                templateImages[i] = file;
                            }

                            // this.setState({
                            //     tempalteImg: file ? [file] : file,
                            // });
                        } else {
                            templateImages[i] = null;
                            // this.setState({
                            //     tempalteImg: null,
                            // });
                        }

                        //判断是否之前已经保存过Barcode Template，保存过走upload接口，没保存过走create接口。
                        // this.barcodeTempIsHasSetted = !!templates[i].Id;
                    }

                    this.setState({
                        barcodeTempAreaInfoForBox:
                            barcodeTempAreaInfos[BarcodeTemplateType.Box - 1],
                        barcodeTempPreviewInfoForBox:
                            barcodeTempPreviewInfos[
                                BarcodeTemplateType.Box - 1
                            ],
                        barcodeTempAreaValueForBox:
                            barcodeTempAreaValues[BarcodeTemplateType.Box - 1],
                        templateImageForBox:
                            templateImages[BarcodeTemplateType.Box - 1],
                        barcodeTempAreaInfoForFolder:
                            barcodeTempAreaInfos[
                                BarcodeTemplateType.Folder - 1
                            ],
                        barcodeTempPreviewInfoForFolder:
                            barcodeTempPreviewInfos[
                                BarcodeTemplateType.Folder - 1
                            ],
                        barcodeTempAreaValueForFolder:
                            barcodeTempAreaValues[
                                BarcodeTemplateType.Folder - 1
                            ],
                        templateImageForFolder:
                            templateImages[BarcodeTemplateType.Folder - 1],
                    });
                }
            })
            .finally(() => {
                $$.loading(false);
            });
    };

    handleSetTemplateColumn = (templateColumnsResponse) => {
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
                    column.tooltip = this.getTooltip(templateColumnsResponse[key]);
                }
            }
            column.value = key;
            templateColumns.push(column);
        }
        return templateColumns;
    }

    getAllTemplateColumn = () => {
        $$.loading(true);
        let url = "/Api/TemplateManagementApi/GetAllTemplateColumn";
        let option = {
            url: url,
            method: "GET",
        };
        fetchUtility(option)
            .then((res) => {
                const templateColumnsforBox = this.handleSetTemplateColumn(res.BoxTemplateColumns);
                const templateColumnsforFolder = this.handleSetTemplateColumn(res.FolderTemplateColumns);
                this.setState({
                    boxTemplateColumns: templateColumnsforBox,
                    folderTemplateColumns: templateColumnsforFolder,
                }, () => {
                    this.getBarcodeTemplateBySuiteId();
                });
            })
            .finally((e) => {
                $$.loading(false);
            });
    };

    handleTabIndexChanged = (tabIndex) => {
        let barcodeType = BarcodeTemplateType.Box;
        this.setState({
            tabIndex: tabIndex,
            areaDLimitValidForBox: false,
            areaDLimitValidForFolder: false,
        });
        if (tabIndex == 1) {
            barcodeType = BarcodeTemplateType.Folder;
        }
    };

    handleTemplateColumnChangeForBox = (index, args) => {
        const barcodeTempAreaInfoForBox = this.state.barcodeTempAreaInfoForBox;
        let updated = [];

        const newName = args.newValue.name;
        const newValue = args.newValue.value;

        const barcodeTempPreviewInfo = this.state.barcodeTempPreviewInfoForBox;
        const barcodeTempAreaValue = this.state.barcodeTempAreaValueForBox;
        switch (index) {
            case 0:
                updated = barcodeTempAreaInfoForBox[index].map((item) => ({
                    ...item,
                    checked: item.value === newValue,
                }));
                barcodeTempPreviewInfo.selectedAreaBName = newName;
                barcodeTempAreaValue.selectedAreaBValue = newValue;
                break;
            case 1:
                updated = barcodeTempAreaInfoForBox[index].map((item) => ({
                    ...item,
                    checked: item.value === newValue,
                }));
                barcodeTempPreviewInfo.selectedAreaCName = newName;
                barcodeTempAreaValue.selectedAreaCValue = newValue;
                break;
            case 2:
                const newValues = args.newValue.map((item) => {
                    return item.value;
                });
                updated = barcodeTempAreaInfoForBox[index].map((item) => ({
                    ...item,
                    checked: newValues.includes(item.value),
                }));
                barcodeTempPreviewInfo.selectedAreaDNames = args.newValue.map(
                    (item) => {
                        return item.name;
                    }
                );
                barcodeTempAreaValue.selectedAreaDValues = newValues;
                this.setState({
                    areaDLimitValidForBox:
                        barcodeTempPreviewInfo.selectedAreaDNames.length > 5,
                });
                break;
            case 3:
                updated = barcodeTempAreaInfoForBox[index].map((item) => ({
                    ...item,
                    checked: item.value === newValue,
                }));
                barcodeTempPreviewInfo.selectedAreaEName = newName;
                barcodeTempAreaValue.selectedAreaEValue = newValue;
                break;
            case 4:
                updated = barcodeTempAreaInfoForBox[index].map((item) => ({
                    ...item,
                    checked: item.value === newValue,
                }));
                barcodeTempPreviewInfo.selectedAreaFName = newName;
                barcodeTempAreaValue.selectedAreaFValue = newValue;
                break;
            default:
                break;
        }

        barcodeTempAreaInfoForBox[index] = updated;

        this.setState({
            barcodeTempAreaInfoForBox: RM.deepcopy(barcodeTempAreaInfoForBox),
            barcodeTempPreviewInfoForBox: RM.deepcopy(barcodeTempPreviewInfo),
            barcodeTempAreaValueForBox: RM.deepcopy(barcodeTempAreaValue),
        });
    };

    handleTemplateColumnChangeForFolder = (index, args) => {
        const barcodeTempAreaInfoForFolder =
            this.state.barcodeTempAreaInfoForFolder;
        let updated = [];

        const barcodeTempPreviewInfo =
            this.state.barcodeTempPreviewInfoForFolder;
        const barcodeTempAreaValue = this.state.barcodeTempAreaValueForFolder;

        const newName = args.newValue.name;
        const newValue = args.newValue.value;

        switch (index) {
            case 0:
                updated = barcodeTempAreaInfoForFolder[index].map((item) => ({
                    ...item,
                    checked: item.value === newValue,
                }));
                barcodeTempPreviewInfo.selectedAreaBName = newName;
                barcodeTempAreaValue.selectedAreaBValue = newValue;
                break;
            case 1:
                updated = barcodeTempAreaInfoForFolder[index].map((item) => ({
                    ...item,
                    checked: item.value === newValue,
                }));
                barcodeTempPreviewInfo.selectedAreaCName = newName;
                barcodeTempAreaValue.selectedAreaCValue = newValue;
                break;
            case 2:
                const newValues = args.newValue.map((item) => {
                    return item.value;
                });
                updated = barcodeTempAreaInfoForFolder[index].map((item) => ({
                    ...item,
                    checked: newValues.includes(item.value),
                }));
                barcodeTempPreviewInfo.selectedAreaDNames = args.newValue.map(
                    (item) => {
                        return item.name;
                    }
                );
                barcodeTempAreaValue.selectedAreaDValues = newValues;
                this.setState({
                    areaDLimitValidForFolder:
                        barcodeTempPreviewInfo.selectedAreaDNames.length > 5,
                });
                break;
            case 3:
                updated = barcodeTempAreaInfoForFolder[index].map((item) => ({
                    ...item,
                    checked: item.value === newValue,
                }));
                barcodeTempPreviewInfo.selectedAreaEName = newName;
                barcodeTempAreaValue.selectedAreaEValue = newValue;
                break;
            case 4:
                updated = barcodeTempAreaInfoForFolder[index].map((item) => ({
                    ...item,
                    checked: item.value === newValue,
                }));
                barcodeTempPreviewInfo.selectedAreaFName = newName;
                barcodeTempAreaValue.selectedAreaFValue = newValue;
                break;
            default:
                break;
        }

        barcodeTempAreaInfoForFolder[index] = updated;

        this.setState({
            barcodeTempAreaInfoForFolder: RM.deepcopy(
                barcodeTempAreaInfoForFolder
            ),
            barcodeTempPreviewInfoForFolder: RM.deepcopy(
                barcodeTempPreviewInfo
            ),
            barcodeTempAreaValueForFolder: RM.deepcopy(barcodeTempAreaValue),
        });
    };

    handleUploadTemplateImgForBox = (args) => {
        let uploadTemplateImgBase64 = "";
        let barcodeTempPreviewInfo = this.state.barcodeTempPreviewInfoForBox;
        let fileInfo = args.files[0].file;
        let reader = new FileReader();
        reader.readAsDataURL(fileInfo);
        reader.onload = () => {
            uploadTemplateImgBase64 = reader.result;
            barcodeTempPreviewInfo.uploadTemplateUrl = uploadTemplateImgBase64;
            barcodeTempPreviewInfo.ImageName = args.files[0].fileName;
            barcodeTempPreviewInfo.ImageType = args.files[0].fileExtension;
            this.setState({
                barcodeTempPreviewInfoForBox: RM.deepcopy(
                    barcodeTempPreviewInfo
                ),
            });
        };
        for (let item of args.files) {
            item.fileId = StringUtil.newGuid();
        }
        this.setState({
            templateImageForBox: args.files,
        });
    };

    handleUploadTemplateImgForFolder = (args) => {
        let uploadTemplateImgBase64 = "";
        let barcodeTempPreviewInfo = this.state.barcodeTempPreviewInfoForFolder;
        let fileInfo = args.files[0].file;
        let reader = new FileReader();
        reader.readAsDataURL(fileInfo);
        reader.onload = () => {
            uploadTemplateImgBase64 = reader.result;
            barcodeTempPreviewInfo.uploadTemplateUrl = uploadTemplateImgBase64;
            barcodeTempPreviewInfo.ImageName = args.files[0].fileName;
            barcodeTempPreviewInfo.ImageType = args.files[0].fileExtension;
            this.setState({
                barcodeTempPreviewInfoForFolder: RM.deepcopy(
                    barcodeTempPreviewInfo
                ),
            });
        };
        for (let item of args.files) {
            item.fileId = StringUtil.newGuid();
        }
        this.setState({
            templateImageForFolder: args.files,
        });
    };

    handleDeleteTemplateImgForBox = () => {
        let barcodeTempPreviewInfo = this.state.barcodeTempPreviewInfoForBox;
        barcodeTempPreviewInfo.uploadTemplateUrl = "";
        barcodeTempPreviewInfo.ImageName = null;
        barcodeTempPreviewInfo.ImageType = null;
        this.setState({
            barcodeTempPreviewInfoForBox: RM.deepcopy(
                barcodeTempPreviewInfo
            )
        });
    };

    handleDeleteTemplateImgForFolder = () => {
        let barcodeTempPreviewInfo = this.state.barcodeTempPreviewInfoForFolder;
        barcodeTempPreviewInfo.uploadTemplateUrl = "";
        barcodeTempPreviewInfo.ImageName = null;
        barcodeTempPreviewInfo.ImageType = null;
        this.setState({
            barcodeTempPreviewInfoForFolder: RM.deepcopy(
                barcodeTempPreviewInfo
            )
        });
    };

    handleSaveBarcodeTemplate = () => {
        const barcodeTempPreviewInfoForBox =
            this.state.barcodeTempPreviewInfoForBox;
        const barcodeTempPreviewInfoForFolder =
            this.state.barcodeTempPreviewInfoForFolder;

        if (barcodeTempPreviewInfoForBox.selectedAreaDNames.length > 5) {
            this.setState({
                areaDLimitValidForBox: true,
            });
            return false;
        }

        if (barcodeTempPreviewInfoForFolder.selectedAreaDNames.length > 5) {
            this.setState({
                areaDLimitValidForFolder: true,
            });
            return false;
        }

        // Existing code
        if (!this.state.isValidForBox || !this.state.isValidForFolder) {
            return false;
        }

        const barcodeTempAreaValueForBox =
            this.state.barcodeTempAreaValueForBox;
        const barcodeTempAreaValueForFolder =
            this.state.barcodeTempAreaValueForFolder;

        const boxTemplate = {
            Id: barcodeTempPreviewInfoForBox.id,
            Type: BarcodeTemplateType.Box,
            ImgBase64Str: barcodeTempPreviewInfoForBox.uploadTemplateUrl,
            ColumnB: barcodeTempAreaValueForBox.selectedAreaBValue,
            ColumnC: barcodeTempAreaValueForBox.selectedAreaCValue,
            ColumnD: barcodeTempAreaValueForBox.selectedAreaDValues,
            ColumnE: barcodeTempAreaValueForBox.selectedAreaEValue,
            ColumnF: barcodeTempAreaValueForBox.selectedAreaFValue,
            ImageName: barcodeTempPreviewInfoForBox.ImageName,
            ImageType: barcodeTempPreviewInfoForBox.ImageType,
        };
        const folderTemplate = {
            Id: barcodeTempPreviewInfoForFolder.id,
            Type: BarcodeTemplateType.Folder,
            ImgBase64Str: barcodeTempPreviewInfoForFolder.uploadTemplateUrl,
            ColumnB: barcodeTempAreaValueForFolder.selectedAreaBValue,
            ColumnC: barcodeTempAreaValueForFolder.selectedAreaCValue,
            ColumnD: barcodeTempAreaValueForFolder.selectedAreaDValues,
            ColumnE: barcodeTempAreaValueForFolder.selectedAreaEValue,
            ColumnF: barcodeTempAreaValueForFolder.selectedAreaFValue,
            ImageName: barcodeTempPreviewInfoForFolder.ImageName,
            ImageType: barcodeTempPreviewInfoForFolder.ImageType,
        };
        const searchParams = new URLSearchParams(this.props.location.search);
        const suiteId = searchParams.get("suiteId");
        const payload = {
            ...this.state.templateSuiteInfo,
            IsDefault: true,
            SuiteId: suiteId,
            Templates: [boxTemplate, folderTemplate],
        };
        $$.loading(true);
        const option = {
            url: "/Api/TemplateManagementApi/UpdateDefaultBarcodeTemplate",
            method: "POST",
            data: payload,
        };
        fetchUtility(option)
            .then((res) => {
                if (res.MessageType === RAMessageType.Successful) {
                    this.showMessageTip(
                        "success",
                        RMResx.RM_PRM_BarcodeTemp_Msg_UploadSuccess
                    );
                    this.cancelClick();
                } else {
                    this.showMessageTip("error", res.ErrorMessage);
                }
            })
            .finally((e) => {
                $$.loading(false);
            });
    };

    dataURLtoBlob(dataurl) {
        var arr = dataurl.split(","),
            mime = arr[0].match(/:(.*?);/)[1],
            bstr = atob(arr[1]),
            n = bstr.length,
            u8arr = new Uint8Array(n);
        while (n--) {
            u8arr[n] = bstr.charCodeAt(n);
        }
        return new Blob([u8arr], { type: mime });
    }

    blobToFile(theBlob, fileName) {
        theBlob.lastModifiedDate = new Date();
        theBlob.name = fileName;
        return theBlob;
    }

    cancelClick = () => {
        this.routerTo(RouterUrls.PRM_BarcodeManagement);
    };

    handleChangeTemplateImgForBox = (files) => {
        if (files && files.length > 0) {
           this.setState({ isValidForBox: files[0]?.isSucceed });
           return;
        }

        this.setState({ isValidForBox: true });
    };

    handleChangeTemplateImgForFolder = (files) => {
        if (files && files.length > 0) {
            this.setState({ isValidForFolder: files[0]?.isSucceed });
            return;
        }
 
        this.setState({ isValidForFolder: true });
    };

    renderLayout = () => {
        return (
            <Fragment>
                <div className="reco-barcode-layout-title" tabIndex="0">
                    {RMResx.RM_PRM_BarcodeTemp_Layout}
                </div>
                <div className="reco-barcode-layout-desc" tabIndex="0">
                    {RMResx.RM_PRM_BarcodeTemp_Preview_explain}
                </div>
                <div className="reco-barcode-layout-content">
                    <div className="reco-barcode-layout-pic fia-placeholder">
                        <div className="reco-barcode-layout-pic-text">
                            {RMResx.RM_PRM_BarcodeTemp_Layout_A}
                        </div>
                    </div>
                    <div className="reco-barcode-layout-inputs">
                        <div className="reco-barcode-layout-input">
                            {RMResx.RM_PRM_BarcodeTemp_Layout_B}
                        </div>
                        <div className="reco-barcode-layout-input">
                            {RMResx.RM_PRM_BarcodeTemp_Layout_C}
                        </div>
                        <div className="reco-barcode-layout-input reco-barcode-layout-input-content">
                            {RMResx.RM_PRM_BarcodeTemp_Layout_D}
                        </div>
                        <div className="reco-barcode-layout-input">
                            {RMResx.RM_PRM_BarcodeTemp_Layout_E}
                        </div>
                        <div className="reco-barcode-layout-input">
                            {RMResx.RM_PRM_BarcodeTemp_Layout_F}
                        </div>
                    </div>
                </div>
                <div className="reco-barcode-layout-img"></div>
            </Fragment>
        );
    };

    renderPreviewForBox = () => {
        const previewInfo = this.state.barcodeTempPreviewInfoForBox;
        const tabIndex = this.state.tabIndex;
        const uploadTemplateUrl = previewInfo.uploadTemplateUrl;
        let areaDNames = previewInfo.selectedAreaDNames;
        if (areaDNames.length > 5) {
            areaDNames.splice(5);
            areaDNames[4] += "...";
        }

        return (
            <div style={{ display: tabIndex === 0 ? "block" : "none" }}>
                <div
                    className="reco-barcode-preview-title"
                    style={{ marginBottom: "0" }}
                    tabIndex="0"
                >
                    {RMResx.RM_PRM_BarcodeTemp_Preview}
                </div>
                <div className="reco-barcode-preview-content">
                    <div
                        className={[
                            "reco-barcode-preview-pic",
                            uploadTemplateUrl ? "" : "fia-placeholder",
                        ].join(" ")}
                        style={{
                            background: uploadTemplateUrl
                                ? `url("${uploadTemplateUrl}")`
                                : "#F5F5F5",
                        }}
                    ></div>
                    <div className="reco-barcode-preview-values">
                        <div className="reco-barcode-preview-value">
                            <div
                                className="ra-ellipsis ra-flex-1"
                                data-tooltip="ifneed"
                            >
                                {previewInfo.selectedAreaBName}
                            </div>
                            <div
                                className="ra-ellipsis ra-flex-1 ra-text-right margin-left-s"
                                data-tooltip="ifneed"
                            >
                                {previewInfo.selectedAreaCName}
                            </div>
                        </div>
                        <div className="reco-barcode-preview-value-content">
                            {areaDNames.map((name, index) => (
                                <div
                                    key={index}
                                    className="reco-barcode-preview-value-content-item"
                                >
                                    {name}
                                </div>
                            ))}
                        </div>
                        <div className="reco-barcode-preview-value">
                            <div
                                className="ra-ellipsis ra-flex-1"
                                data-tooltip="ifneed"
                            >
                                {previewInfo.selectedAreaEName}
                            </div>
                            <div
                                className="ra-ellipsis ra-flex-1 ra-text-right margin-left-s"
                                data-tooltip="ifneed"
                            >
                                {previewInfo.selectedAreaFName}
                            </div>
                        </div>
                    </div>
                </div>
                <div className="reco-barcode-preview-img"></div>
            </div>
        );
    };

    renderPreviewForFolder = () => {
        const previewInfo = this.state.barcodeTempPreviewInfoForFolder;
        const tabIndex = this.state.tabIndex;
        const uploadTemplateUrl = previewInfo.uploadTemplateUrl;
        let areaDNames = previewInfo.selectedAreaDNames;
        if (areaDNames.length > 5) {
            areaDNames.splice(5);
            areaDNames[4] += "...";
        }

        return (
            <div style={{ display: tabIndex === 0 ? "none" : "block" }}>
                <div
                    className="reco-barcode-preview-title"
                    style={{ marginBottom: "0" }}
                    tabIndex="0"
                >
                    {RMResx.RM_PRM_BarcodeTemp_Preview}
                </div>
                <div className="reco-barcode-preview-content">
                    <div
                        className={[
                            "reco-barcode-preview-pic",
                            uploadTemplateUrl ? "" : "fia-placeholder",
                        ].join(" ")}
                        style={{
                            background: uploadTemplateUrl
                                ? `url("${uploadTemplateUrl}")`
                                : "#F5F5F5",
                        }}
                    ></div>
                    <div className="reco-barcode-preview-values">
                        <div className="reco-barcode-preview-value">
                            <div
                                className="ra-ellipsis ra-flex-1"
                                data-tooltip="ifneed"
                            >
                                {previewInfo.selectedAreaBName}
                            </div>
                            <div
                                className="ra-ellipsis ra-flex-1 ra-text-right margin-left-s"
                                data-tooltip="ifneed"
                            >
                                {previewInfo.selectedAreaCName}
                            </div>
                        </div>
                        <div className="reco-barcode-preview-value-content">
                            {areaDNames.map((name, index) => (
                                <div
                                    key={index}
                                    className="reco-barcode-preview-value-content-item"
                                >
                                    {name}
                                </div>
                            ))}
                        </div>
                        <div className="reco-barcode-preview-value">
                            <div
                                className="ra-ellipsis ra-flex-1"
                                data-tooltip="ifneed"
                            >
                                {previewInfo.selectedAreaEName}
                            </div>
                            <div
                                className="ra-ellipsis ra-flex-1 ra-text-right margin-left-s"
                                data-tooltip="ifneed"
                            >
                                {previewInfo.selectedAreaFName}
                            </div>
                        </div>
                    </div>
                </div>
                <div className="reco-barcode-preview-img"></div>
            </div>
        );
    };

    renderFootBtns() {
        return (
            <div className="confiuration-foot-btns">
                <R.Button
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={this.cancelClick}
                />
                <R.Button
                    primary={true}
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={this.handleSaveBarcodeTemplate}
                />
            </div>
        );
    }

    renderTabControls = () => {
        const tabIndex = this.state.tabIndex;
        const tabTitles = this.state.tabTitles;

        return (
            <R.Tabcontrol
                type="underline"
                active={tabIndex}
                onChange={this.handleTabIndexChanged}
                flex={true}
            >
                {tabTitles.map((text, index) => {
                    return (
                        <R.TabPanel
                            tab={text}
                            key={index}
                            style={{ maxWidth: "unset" }}
                            aria-label={text}
                            data-tooltip="ifneed"
                        ></R.TabPanel>
                    );
                })}
            </R.Tabcontrol>
        );
    };

    renderConfigurationForBox = () => {
        const {
            tabIndex,
            templateImageForBox,
            barcodeTempAreaInfoForBox,
            areaDLimitValidForBox,
        } = this.state;

        return (
            <div style={{ display: tabIndex === 0 ? "block" : "none" }}>
                <div className="reco-barcode-config-title" tabIndex="0">
                    {RMResx.RM_PRM_BarcodeTemp_Confiuration}
                </div>
                <div className="reco-barcode-config-desc" tabIndex="0">
                    {RMResx.RM_PRM_BarcodeTemp_Confiuration_explain}
                </div>
                <div className="reco-barcode-template-tabs">
                    {this.renderTabControls()}
                </div>
                <div>
                    <div className="margin-bottom-xs" tabIndex="0">
                        {RMResx.RM_PRM_BarcodeTemp_AreaA_Title}
                    </div>
                    <div className="reco-barcode-config-input">
                        <R.Uploader
                            showTypes
                            ref={(r) => (this.uploaderForBoxRef = r)}
                            files={templateImageForBox || []}
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
                            onUpload={this.handleUploadTemplateImgForBox}
                            onDelete={this.handleDeleteTemplateImgForBox}
                            onChange={this.handleChangeTemplateImgForBox}
                        />
                    </div>
                </div>
                {BarcodeTemplateComboboxNames.map((labelName, index) => {
                    return (
                        <div className="reco-barcode-config-item" key={index}>
                            {index === 2 ? (
                                <Fragment>
                                    <div
                                        className="reco-barcode-config-label"
                                        tabIndex="0"
                                    >
                                        {labelName}
                                    </div>
                                    <div className="reco-barcode-config-input">
                                        <R.Multicombobox
                                            width="100%"
                                            textField="name"
                                            valueField="value"
                                            checkedField="checked"
                                            tooltipField="tooltip"
                                            clearable={true}
                                            hasSelectAll={true}
                                            items={
                                                barcodeTempAreaInfoForBox[
                                                    index
                                                ] || []
                                            }
                                            onChange={this.handleTemplateColumnChangeForBox.bind(
                                                this,
                                                index
                                            )}
                                        />
                                        <$g.ValidationMsg
                                            show={areaDLimitValidForBox}
                                        >
                                            {
                                                RMResx.RM_PRM_BarcodeTemp_Valid_AreaDLimit
                                            }
                                        </$g.ValidationMsg>
                                    </div>
                                </Fragment>
                            ) : (
                                <Fragment>
                                    <div
                                        className="reco-barcode-config-label"
                                        tabIndex="0"
                                    >
                                        {labelName}
                                    </div>
                                    <div className="reco-barcode-config-input">
                                        <R.Combobox
                                            width="100%"
                                            textField="name"
                                            valueField="value"
                                            checkedField="checked"
                                            tooltipField="tooltip"
                                            items={
                                                barcodeTempAreaInfoForBox[
                                                    index
                                                ] || []
                                            }
                                            onChange={this.handleTemplateColumnChangeForBox.bind(
                                                this,
                                                index
                                            )}
                                        />
                                    </div>
                                </Fragment>
                            )}
                        </div>
                    );
                })}
            </div>
        );
    };

    renderConfigurationForFolder = () => {
        const {
            tabIndex,
            templateImageForFolder,
            barcodeTempAreaInfoForFolder,
            areaDLimitValidForFolder,
        } = this.state;

        return (
            <div style={{ display: tabIndex === 0 ? "none" : "block" }}>
                <div className="reco-barcode-config-title" tabIndex="0">
                    {RMResx.RM_PRM_BarcodeTemp_Confiuration}
                </div>
                <div className="reco-barcode-config-desc" tabIndex="0">
                    {RMResx.RM_PRM_BarcodeTemp_Confiuration_explain}
                </div>
                <div className="reco-barcode-template-tabs">
                    {this.renderTabControls()}
                </div>
                <div>
                    <div className="margin-bottom-xs" tabIndex="0">
                        {RMResx.RM_PRM_BarcodeTemp_AreaA_Title}
                    </div>
                    <div className="reco-barcode-config-input">
                        <R.Uploader
                            showTypes
                            ref={(r) => (this.uploaderForFolderRef = r)}
                            files={templateImageForFolder || []}
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
                            onUpload={this.handleUploadTemplateImgForFolder}
                            onDelete={this.handleDeleteTemplateImgForFolder}
                            onChange={this.handleChangeTemplateImgForFolder}
                        />
                    </div>
                </div>
                {BarcodeTemplateComboboxNames.map((labelName, index) => {
                    return (
                        <div className="reco-barcode-config-item" key={index}>
                            {index === 2 ? (
                                <Fragment>
                                    <div
                                        className="reco-barcode-config-label"
                                        tabIndex="0"
                                    >
                                        {labelName}
                                    </div>
                                    <div className="reco-barcode-config-input">
                                        <R.Multicombobox
                                            width="100%"
                                            textField="name"
                                            valueField="value"
                                            checkedField="checked"
                                            tooltipField="tooltip"
                                            clearable={true}
                                            hasSelectAll={true}
                                            items={
                                                barcodeTempAreaInfoForFolder[
                                                    index
                                                ] || []
                                            }
                                            onChange={this.handleTemplateColumnChangeForFolder.bind(
                                                this,
                                                index
                                            )}
                                        />
                                        <$g.ValidationMsg
                                            show={areaDLimitValidForFolder}
                                        >
                                            {
                                                RMResx.RM_PRM_BarcodeTemp_Valid_AreaDLimit
                                            }
                                        </$g.ValidationMsg>
                                    </div>
                                </Fragment>
                            ) : (
                                <Fragment>
                                    <div
                                        className="reco-barcode-config-label"
                                        tabIndex="0"
                                    >
                                        {labelName}
                                    </div>
                                    <div className="reco-barcode-config-input">
                                        <R.Combobox
                                            width="100%"
                                            textField="name"
                                            valueField="value"
                                            checkedField="checked"
                                            tooltipField="tooltip"
                                            items={
                                                barcodeTempAreaInfoForFolder[
                                                    index
                                                ] || []
                                            }
                                            onChange={this.handleTemplateColumnChangeForFolder.bind(
                                                this,
                                                index
                                            )}
                                        />
                                    </div>
                                </Fragment>
                            )}
                        </div>
                    );
                })}
            </div>
        );
    };

    render() {
        return (
            <div className="reco-barcode-template-wrapper" id={this.props.id}>
                <section className="reco-barcode-template-nav">
                    <$g.SiteMap
                        data={[
                            SiteMapLinks.PRM_BarcodeManagement,
                            SiteMapLinks.PRM_BarcodeManagement_EditDefault,
                        ]}
                    />
                </section>
                <section className="reco-barcode-template-content">
                    <div className="reco-barcode-template-content-left">
                        <div className="reco-barcode-template-layout">
                            {this.renderLayout()}
                        </div>
                        <div className="reco-barcode-template-preview">
                            {this.renderPreviewForBox()}
                            {this.renderPreviewForFolder()}
                        </div>
                    </div>
                    <div className="reco-barcode-template-content-config">
                        <div>
                            {this.renderConfigurationForBox()}
                            {this.renderConfigurationForFolder()}
                            <div className="flex justify-end align-center gap-s margin-top-l">
                                <R.Button
                                    text={RMResx.RM_JS_Common_Cancel}
                                    onClick={this.cancelClick}
                                />
                                <R.Button
                                    primary={true}
                                    classify="theme"
                                    text={RMResx.RM_JS_Common_Save}
                                    onClick={this.handleSaveBarcodeTemplate}
                                />
                            </div>
                            <div></div>
                        </div>
                    </div>
                </section>
            </div>
        );
    }
}
