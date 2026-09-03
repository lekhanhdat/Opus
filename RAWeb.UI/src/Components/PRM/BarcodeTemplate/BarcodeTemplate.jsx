import SiteMapLinks from "../../../Constants/SiteMapLinks";
import StringUtil from "../../../Utilities/StringUtil";
import RouterUrls from "../../../Constants/RouterUrls";
import BarcodePreview from "../BarcodePreview";
import { BarcodeTemplateType, BarcodeTemplateComboboxNames, BarcodeTemplateBuildInIDs, BarcodeTemplateBuildInNames } from "../Constants";
import "../../../Less/PRM/barcodeTemplete.less";
import { Fragment } from "react";
import { showToast } from "../../../Utilities/CommonUtil";

export default class BarcodeTemplate extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            tabTitles: this.getTabPanels(),
            tabIndex: 0,
            templateColumns: [],
            barcodeTempPreviewInfo: {
                selectedAreaBName: "",
                selectedAreaCName: "",
                selectedAreaDNames: [],
                selectedAreaEName: "",
                selectedAreaFName: "",
                uploadTemplateUrl: "",
                ImageName: "",
                ImageType: "",
            },
            barcodeTempAreaValues: {
                selectedAreaBValue: "",
                selectedAreaCValue: "",
                selectedAreaDValues: [],
                selectedAreaEValue: "",
                selectedAreaFValue: "",
            },
            barcodeTempAreaInfo: [],
            tempalteImg: [],
            areaDLimitValid: false
        };
    }

    componentInit() {
        this.getAllTemplateColumn(BarcodeTemplateType.Box);

    }

    showMessageTip(type, msg) {
        showToast._showMsg(type, msg);
    }

    routerTo(routerUrl) {
        this.props.history.push({
            pathname: routerUrl
        });
    }

    getTabPanels() {
        let tabTitles = [
            RMResx.RM_PRM_BarcodeTemp_BoxTab,
            RMResx.RM_PRM_BarcodeTemp_FolderTab,
        ];
        return tabTitles;
    }

    loadBarcodeTemplateByType(type) {
        $$.loading(true);
        let url = "/Api/TemplateManagementApi/LoadBarcodeTemplateByType";
        let option = {
            url: url,
            method: "POST",
            data: type
        };
        fetchUtility(option).then((res) => {
            let barcodeTempAreaB = this.getEchoAreaList(res.ColumnB);
            let barcodeTempAreaC = this.getEchoAreaList(res.ColumnC);
            let barcodeTempAreaD = this.getEchoAreaList(res.ColumnD);
            let barcodeTempAreaE = this.getEchoAreaList(res.ColumnE);
            let barcodeTempAreaF = this.getEchoAreaList(res.ColumnF);
            let barcodeTempAreaInfo = [barcodeTempAreaB, barcodeTempAreaC, barcodeTempAreaD, barcodeTempAreaE, barcodeTempAreaF];
            this.setState({
                barcodeTempAreaInfo: barcodeTempAreaInfo,
                barcodeTempPreviewInfo: {
                    selectedAreaBName: this.getSelectedAreaName(res.ColumnB),
                    selectedAreaCName: this.getSelectedAreaName(res.ColumnC),
                    selectedAreaDNames: this.getSelectedAreaName(res.ColumnD),
                    selectedAreaEName: this.getSelectedAreaName(res.ColumnE),
                    selectedAreaFName: this.getSelectedAreaName(res.ColumnF),
                    uploadTemplateUrl: res.ImgBase64Str,
                    ImageName: res.ImageName,
                    ImageType: res.ImageType,
                },
                barcodeTempAreaValues: {
                    selectedAreaBValue: res.ColumnB,
                    selectedAreaCValue: res.ColumnC,
                    selectedAreaDValues: res.ColumnD,
                    selectedAreaEValue: res.ColumnE,
                    selectedAreaFValue: res.ColumnF,
                }
            });
            //base64 转成file对象
            let base64Data = res.ImgBase64Str;
            if (base64Data) {
                let blob = this.dataURLtoBlob(base64Data);
                let file = this.blobToFile(blob, res.ImageName);
                file.fileName = file.name;
                file.fileExtension = res.ImageType;
                file.fileId = StringUtil.newGuid();
                this.setState({
                    tempalteImg: (file ? [file] : file)
                });
            } else {
                this.setState({
                    tempalteImg: null
                });
            }

            //判断是否之前已经保存过Barcode Template，保存过走upload接口，没保存过走create接口。
            this.barcodeTempIsHasSetted = !!res.Id;
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    getSelectedAreaName(valueInfo) {
        let templateColumns = RM.deepcopy(this.state.templateColumns);
        let selectedAreaNames = "";
        if (valueInfo) {
            if (typeof (valueInfo) == 'string') {
                let selectedAreaNameList = templateColumns.filter((item) => { return item.value == valueInfo; });
                if (selectedAreaNameList.length > 0) {
                    selectedAreaNames = selectedAreaNameList[0].name;
                }
            } else {
                let selectedAreaNameList = templateColumns.filter((item) => { return valueInfo.indexOf(item.value) != -1; });
                if (selectedAreaNameList) {
                    selectedAreaNames = selectedAreaNameList.map(item => {
                        return item.name;
                    });
                }
            }
        }
        return selectedAreaNames;
    }

    getEchoAreaList(valueInfo) {
        let templateColumns = RM.deepcopy(this.state.templateColumns);
        if (valueInfo) {
            if (typeof (valueInfo) == 'string') {
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

    getTooltip(value, splitChar = ",")
    {
        if(Array.isArray(value))
        {
            let newValue = value.map(o => RMResx[o] || o);
            return newValue.join(splitChar);
        }
        else 
        {
            return RMResx[value] || value;
        }
    }

    getAllTemplateColumn = (type) => {
        $$.loading(true);
        let url = "/Api/TemplateManagementApi/GetAllTemplateColumn";
        let option = {
            url: url,
            method: "POST",
            data: type
        };
        fetchUtility(option).then((res) => {
            let templateColumns = [];
            for (let key in res) {
                let column = {};
                //返回值key为Guid时代表每个template都有这个column，tooltip显示Built-in Column；
                if (BarcodeTemplateBuildInIDs.indexOf(key) != -1) {
                    column.name = BarcodeTemplateBuildInNames[key];
                    column.tooltip = RMResx.RM_PRM_BarcodeTemp_AreaF_BuildInColumn;
                } else {
                    column.name = RMResx[key] || key;
                    if (res[key]) {
                        column.tooltip = this.getTooltip(res[key]);
                    }
                }
                column.value = key;
                templateColumns.push(column);
            }
            this.setState({ templateColumns: templateColumns }, () => {
                this.loadBarcodeTemplateByType(type);
            });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    handleTabIndexChanged = (tabIndex) => {
        let barcodeType = BarcodeTemplateType.Box;
        this.setState({
            tabIndex: tabIndex,
            areaDLimitValid: false
        });
        if (tabIndex == 1) {
            barcodeType = BarcodeTemplateType.Folder;
        }
        this.getAllTemplateColumn(barcodeType);
    };

    handleTemplateColumnChange = (index, args) => {
        let barcodeTempPreviewInfo = this.state.barcodeTempPreviewInfo;
        let barcodeTempAreaValues = this.state.barcodeTempAreaValues;
        switch (index) {
            case 0:
                barcodeTempPreviewInfo.selectedAreaBName = args.newValue.name;
                barcodeTempAreaValues.selectedAreaBValue = args.newValue.value;
                break;
            case 1:
                barcodeTempPreviewInfo.selectedAreaCName = args.newValue.name;
                barcodeTempAreaValues.selectedAreaCValue = args.newValue.value;
                break;
            case 2:
                barcodeTempPreviewInfo.selectedAreaDNames = args.newValue.map((item) => { return item.name; });
                barcodeTempAreaValues.selectedAreaDValues = args.newValue.map((item) => { return item.value; });
                this.setState({ areaDLimitValid: barcodeTempPreviewInfo.selectedAreaDNames.length > 5 });
                break;
            case 3:
                barcodeTempPreviewInfo.selectedAreaEName = args.newValue.name;
                barcodeTempAreaValues.selectedAreaEValue = args.newValue.value;
                break;
            case 4:
                barcodeTempPreviewInfo.selectedAreaFName = args.newValue.name;
                barcodeTempAreaValues.selectedAreaFValue = args.newValue.value;
                break;
            default:
                break;
        }
        this.setState({
            barcodeTempPreviewInfo: RM.deepcopy(barcodeTempPreviewInfo),
            barcodeTempAreaValues: RM.deepcopy(barcodeTempAreaValues)
        });
    }

    handleUploadTemplateImg = (args) => {
        let uploadTemplateImgBase64 = "";
        let barcodeTempPreviewInfo = this.state.barcodeTempPreviewInfo;
        let fileInfo = args.files[0].file;
        let reader = new FileReader();
        reader.readAsDataURL(fileInfo);
        reader.onload = () => {
            uploadTemplateImgBase64 = reader.result;
            barcodeTempPreviewInfo.uploadTemplateUrl = uploadTemplateImgBase64;
            barcodeTempPreviewInfo.ImageName = args.files[0].fileName;
            barcodeTempPreviewInfo.ImageType = args.files[0].fileExtension;
            this.setState({
                barcodeTempPreviewInfo: RM.deepcopy(barcodeTempPreviewInfo)
            });
        };
        for (let item of args.files) {
            item.fileId = StringUtil.newGuid();
        }
        this.setState({
            tempalteImg: args.files,
        });
    }

    handleDeleteTemplateImg = () => {
        let barcodeTempPreviewInfo = this.state.barcodeTempPreviewInfo;
        barcodeTempPreviewInfo.uploadTemplateUrl = "";
        barcodeTempPreviewInfo.ImageName = null,
        barcodeTempPreviewInfo.ImageType = null,
        this.setState({
            barcodeTempPreviewInfo: RM.deepcopy(barcodeTempPreviewInfo)
        });
    }

    handleSaveBarcodeTemplate = () => {
        let barcodeTempPreviewInfo = this.state.barcodeTempPreviewInfo;
        let barcodeTempAreaValues = this.state.barcodeTempAreaValues;
        let uploadFileInfo = this.uploaderRef.getValue();
        if (barcodeTempPreviewInfo.selectedAreaDNames.length > 5) {
            this.setState({ areaDLimitValid: true });
            return false;
        }
        if (uploadFileInfo == true) {
            // //判断是否上传图片通过了验证，uploadFileInfo为true代表上传为通过验证。
            // this.showMessageTip("error",".Please follow the instructions to upload the picture");
            return false;
        }
        let data = {
            Type: this.state.tabIndex * 1 + 1,
            ImgBase64Str: barcodeTempPreviewInfo.uploadTemplateUrl,
            ImageName: barcodeTempPreviewInfo.ImageName,
            ImageType: barcodeTempPreviewInfo.ImageType,
            ColumnB: barcodeTempAreaValues.selectedAreaBValue,
            ColumnC: barcodeTempAreaValues.selectedAreaCValue,
            ColumnD: barcodeTempAreaValues.selectedAreaDValues,
            ColumnE: barcodeTempAreaValues.selectedAreaEValue,
            ColumnF: barcodeTempAreaValues.selectedAreaFValue,
        };
        $$.loading(true);
        let url = this.barcodeTempIsHasSetted ?
            "/Api/TemplateManagementApi/UpdateBarcodeTemplate" : "/Api/TemplateManagementApi/CreateBarcodeTemplate";
        let option = {
            url: url,
            method: "POST",
            data: data
        };
        fetchUtility(option).then((res) => {
            if (res == "") {
                this.showMessageTip("success", RMResx.RM_PRM_BarcodeTemp_Msg_UploadSuccess);
            } else {
                this.showMessageTip("error", res);
            }
            this.loadBarcodeTemplateByType(this.state.tabIndex + 1);
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    dataURLtoBlob(dataurl) {
        var arr = dataurl.split(','),
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
        this.routerTo(RouterUrls.PRM_TemplateManagement);
    }

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
    }

    renderPreview = () => {
        const previewInfo = this.state.barcodeTempPreviewInfo;
        const uploadTemplateUrl = previewInfo.uploadTemplateUrl;
        let areaDNames = previewInfo.selectedAreaDNames;
        if (areaDNames.length > 5) {
            areaDNames.splice(5);
            areaDNames[4] += "...";
        }

        return (
            <Fragment>
                <div className="reco-barcode-preview-title" style={{ marginBottom: "0" }} tabIndex="0">
                    {RMResx.RM_PRM_BarcodeTemp_Preview}
                </div>
                <div className="reco-barcode-preview-content">
                    <div
                        className={["reco-barcode-preview-pic", uploadTemplateUrl ? "" : "fia-placeholder"].join(" ")}
                        style={{ background: uploadTemplateUrl ? `url("${uploadTemplateUrl}")` : "#F5F5F5" }}
                    >
                    </div>
                    <div className="reco-barcode-preview-values">
                        <div className="reco-barcode-preview-value">
                            <div className="ra-ellipsis ra-flex-1" data-tooltip="ifneed">
                                {previewInfo.selectedAreaBName}
                            </div>
                            <div className="ra-ellipsis ra-flex-1 ra-text-right margin-left-s" data-tooltip="ifneed">
                                {previewInfo.selectedAreaCName}
                            </div>
                        </div>
                        <div className="reco-barcode-preview-value-content">
                            {
                                areaDNames.map((name, index) =>
                                    <div
                                        key={index}
                                        className="reco-barcode-preview-value-content-item"
                                    >
                                        {name}
                                    </div>
                                )
                            }
                        </div>
                        <div className="reco-barcode-preview-value">
                            <div className="ra-ellipsis ra-flex-1" data-tooltip="ifneed">
                                {previewInfo.selectedAreaEName}
                            </div>
                            <div className="ra-ellipsis ra-flex-1 ra-text-right margin-left-s" data-tooltip="ifneed">
                                {previewInfo.selectedAreaFName}
                            </div>
                        </div>
                    </div>
                </div>
                <div className="reco-barcode-preview-img"></div>
            </Fragment>
        );
    };

    renderFootBtns() {
        return <div className="confiuration-foot-btns">
            <R.Button
                text={RMResx.RM_JS_Common_Cancel}
                onClick={this.cancelClick} />
            <R.Button
                primary={true}
                classify="theme"
                text={RMResx.RM_JS_Common_Save}
                onClick={this.handleSaveBarcodeTemplate} />
        </div>;
    }

    renderConfiguration = () => {
        return (
            <Fragment>
                <div className="reco-barcode-config-title" tabIndex="0">
                    {RMResx.RM_PRM_BarcodeTemp_Confiuration}
                </div>
                <div className="reco-barcode-config-desc" tabIndex="0">
                    {RMResx.RM_PRM_BarcodeTemp_Confiuration_explain}
                </div>
                <div className="reco-barcode-config-item">
                    <div className="reco-barcode-config-label" tabIndex="0">
                        {RMResx.RM_PRM_BarcodeTemp_AreaA_Title}
                    </div>
                    <div className="reco-barcode-config-input">
                        <R.Uploader
                            showTypes
                            ref={r => this.uploaderRef = r}
                            files={this.state.tempalteImg}
                            fileTypes={["PNG", "JPG", "PJP", "JPEG", "JFIF", "PJPEG"]}
                            maxSize="1MB"
                            showMaxSize={true}
                            onUpload={this.handleUploadTemplateImg}
                            onDelete={this.handleDeleteTemplateImg}
                        />
                    </div>
                </div>
                {
                    BarcodeTemplateComboboxNames.map((labelName, index) => {
                        return (
                            <div className="reco-barcode-config-item" key={index}>
                                {
                                    index === 2 ?
                                        <Fragment>
                                            <div className="reco-barcode-config-label" tabIndex="0">
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
                                                    items={this.state.barcodeTempAreaInfo[index]}
                                                    onChange={this.handleTemplateColumnChange.bind(this, index)}
                                                />
                                                <$g.ValidationMsg show={this.state.areaDLimitValid}>
                                                    {RMResx.RM_PRM_BarcodeTemp_Valid_AreaDLimit}
                                                </$g.ValidationMsg>
                                            </div>
                                        </Fragment>
                                        :
                                        <Fragment>
                                            <div className="reco-barcode-config-label" tabIndex="0">
                                                {labelName}
                                            </div>
                                            <div className="reco-barcode-config-input">
                                                <R.Combobox
                                                    width="100%"
                                                    textField='name'
                                                    valueField='value'
                                                    checkedField='checked'
                                                    tooltipField="tooltip"
                                                    items={this.state.barcodeTempAreaInfo[index]}
                                                    onChange={this.handleTemplateColumnChange.bind(this, index)}
                                                />
                                            </div>
                                        </Fragment>
                                }
                            </div>
                        );
                    })
                }
            </Fragment>
        );
    }

    render() {
        return (
            <div className="reco-barcode-template-wrapper" id={this.props.id}>
                <section className="reco-barcode-template-nav">
                    <$g.SiteMap data={[SiteMapLinks.PRM_TemplateManagement, SiteMapLinks.PRM_BarcodeTemplate]} />
                </section>
                <section className="reco-barcode-template-tabs">
                    <R.Tabcontrol
                        type='underline'
                        active={this.state.tabIndex}
                        onChange={this.handleTabIndexChanged}
                        destroy={true}
                        flex={true}
                    >
                        {
                            this.state.tabTitles.map((text, index) => {
                                return <R.TabPanel tab={text} key={index} style={{ maxWidth: "unset" }} aria-label={text} data-tooltip="ifneed"></R.TabPanel>;
                            })
                        }
                    </R.Tabcontrol>
                </section>
                <section className="reco-barcode-template-content">
                    <div className="reco-barcode-template-content-left">
                        <div className="reco-barcode-template-layout">
                            {this.renderLayout()}
                        </div>
                        <div className="reco-barcode-template-preview">
                            {this.renderPreview()}
                        </div>
                    </div>
                    <div className="reco-barcode-template-content-config">
                        {this.renderConfiguration()}
                        <div className="reco-barcode-template-btns flex align-center gap-s">
                            <R.Button
                                text={RMResx.RM_JS_Common_Cancel}
                                onClick={this.cancelClick} />
                            <R.Button
                                primary={true}
                                classify="theme"
                                text={RMResx.RM_JS_Common_Save}
                                onClick={this.handleSaveBarcodeTemplate} />
                        </div>
                    </div>
                </section>
            </div>
        );
    }
}