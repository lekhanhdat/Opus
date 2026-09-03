import ReactToPrint from 'react-to-print';
import {NodeType} from "../../../../Constants/DAEnums";
import {PhysicalObjectColumnType} from "../../../../Constants/Constants";
import BarcodePreview from "../../BarcodePreview";
import StringUtil from '../../../../Utilities/StringUtil';



class ComponentToPrint extends React.Component {
    render() {
        return <React.Fragment>
            {this.props.children}
        </React.Fragment>;
    }
}

export default class PhyObjectInfo extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            showDialog: false,
            selectedIndex: 0,
            permissionInfo: null,
            barcodePreviewData:{
                selectedAreaBName: "A",
                selectedAreaCName: "B",
                selectedAreaDNames: ["A","C"],
                selectedAreaEName: "E",
                selectedAreaFName: "F",
                uploadTemplateUrl: ""
            },
        };
        this.bind(['handleChangedTab']);
    }

    componentReceive(type, item) {
        if (type == 'reset') {
            this.setState({selectedIndex: 0});
            this.setPermissionInfo(item);
        }
    }

    componentInit() {
        this.setPermissionInfo(this.props.data);
    }

    renderDetailCellValue(column) {
        if (column.isDeleted) {
            if (column.typeId == PhysicalObjectColumnType.SingleChoice) {
                return <span className='ra-wavyLine'>{column.columnValue}</span>;
            }
            if (column.typeId == PhysicalObjectColumnType.MultipleChoice) {
                return <div>
                    {
                        column.columnValue.map((item,index) => {
                            return <span key={index} className={item.showWavyLine ? 'ra-wavyLine ' : ''}>{item.Name};</span>;
                        })
                    }
                </div>;
            }
        } else {
            return column.columnValue;
        }
    }

    categoryContentRender(item) {
        let total = item.length;
        if (total == 0) {
            return [];
        }
        let rows = [];
        let maxRowIdx = Math.ceil(total / 2.0);
        let rowIdx = 0;
        do {
            let start = rowIdx * 2,
                end = Math.min(start + 2, total),
                row = [];
            for (let i = start; i < end; i++) {
                row.push(item[i]);
            }
            rows.push(row);
        } while (rowIdx++ < maxRowIdx);
        return <React.Fragment>
            {
                rows.map((row, rIdx) => {
                    return <$g.DetailRow key={rIdx}>
                        {row.map((cell, cIdx) => {
                            let columnName = RMResx[cell.columnName] ? RMResx[cell.columnName] : cell.columnName;
                            return <$g.DetailCell
                                key={cIdx}
                                label={columnName}
                                value={this.renderDetailCellValue(cell)}/>;
                        })}
                    </$g.DetailRow>;
                })
            }
        </React.Fragment>;
    }

    showBarcodeDlg = () => {
        this.setState({
            showDialog: true
        });
    };

    hideBarcodeDlg = () => {
        this.setState({
            showDialog: false
        });
    };

    handleChangedTab(index) {
        this.setState({
            selectedIndex: index
        });
    }

    setPermissionInfo(item) {
        if(item.NodeType >= NodeType.PhysicalRootLocation){
            let nodeId = item.NodeType == (NodeType.PhysicalNormalLocation || NodeType.PhysicalBottomLocation) ? (item.LocationId || item.Id) : item.Id;
            $$.loading(true);
            let option = {
                url: `/api/PhysicalRecordApi/GetBreakOrInheritPermission?scopeId=${nodeId}&includeSelf=${true}`,
                method: "GET",
            };
            fetchUtility(option).then((result) => {
                $$.loading(false);
                let res = JSON.parse(result);
                this.setState({
                    permissionInfo: RM.deepcopy(res)
                });
            }).catch((e) => {
                $$.loading(false);
            });
        }

    }

    getBarcodeImg(imgBase64Str) {
        let imgSrc = `data:image/png;base64,${imgBase64Str}`;
        //let imgSrc = `data:image/svg+xml;base64,${imgBase64Str}`;
        return (<img className='barcodeImg' src={imgSrc} alt=''/>);
    }

    getBorcodeColumnDInfo(columnDObj){
        let columnDInfo = null;
        if(columnDObj){
            columnDInfo = [];
            for(let key in columnDObj){
                columnDInfo.push(columnDObj[key]);
            }
        }
        return columnDInfo;
    }

    renderBarcode() {
        let imgBase64Str = this.props.data.barcode.barcodeBase64Str;
        if (imgBase64Str) {
            let tabIndex = this.props.data.metaData.length + 2;
            return <R.TabPanel key={tabIndex} tab={RMResx.RM_PRM_PRE_BarCode_CategoryName}>
                <section>
                    <div className="barcode-content">
                        <div className="barcode-img-div">{this.getBarcodeImg(imgBase64Str)}</div>
                        <div className="barcode-print">
                            <R.Button primary={true} classify="theme" text={RMResx.RM_PRM_PRE_BarCode_Print} onClick={this.showBarcodeDlg} />
                        </div>
                    </div>
                </section>
            </R.TabPanel>;
        }
    }

    renderHasPermissionUserList(users){
        return (
            users.map((item,index) => {
                return <div key={index}>{item.DisplayName}</div>;
            })
        );
    }

    renderPermission() {
        let tabIndex = this.props.data.metaData.length + 1;
        let permissionInfo = this.state.permissionInfo;
        if(permissionInfo && (this.props.data.NodeType > NodeType.PhysicalRootLocation)){
            let isInherit = !permissionInfo.BreakInheritStatus ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No;
            return <R.TabPanel key={tabIndex} tab={RMResx.RM_PRM_PRE_PermissionTitle}>
                <section>
                    <$g.DetailList className="category-content" labelWidth={180}>
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={StringUtil.trimEndColon(RMResx.RM_PRM_PRE_InheritPermissionOrNot)}
                                value={isInherit}
                            />
                            <$g.DetailCell
                                label={StringUtil.trimEndColon(RMResx.RM_PRM_PRE_UsersWithPermissions)}
                                value={this.renderHasPermissionUserList(permissionInfo.Accounts)}
                            />
                        </$g.DetailRow>
                    </$g.DetailList>
                </section>
            </R.TabPanel>;
        }
    }

    render() {
        let phyObjInfo = this.props.data;
        let barcodePreviewData = {
            uploadTemplateUrl: phyObjInfo.ImageBase64Str,
            selectedAreaBName: phyObjInfo.ColumnB,
            selectedAreaCName: phyObjInfo.ColumnC,
            selectedAreaDNames: this.getBorcodeColumnDInfo(phyObjInfo.ColumnD),
            selectedAreaEName: phyObjInfo.ColumnE,
            selectedAreaFName: phyObjInfo.ColumnF,
        };
        return <div className='recordsExplorer_info' id={this.props.id}>
            <R.Tabcontrol
                active={this.state.selectedIndex}
                onChange={this.handleChangedTab}
                type='underline'
            >
                {
                    this.props.data.metaData.map((item, index) => {
                        let categoryName = RMResx[item.name] ? RMResx[item.name] : item.name;
                        return <R.TabPanel key={index} tab={categoryName} aria-label={categoryName} data-tooltip="ifneed">
                            <section>
                                <$g.DetailList className="category-content" labelWidth={180}>
                                    {this.categoryContentRender(item.columns)}
                                </$g.DetailList>
                            </section>
                        </R.TabPanel>;
                    })
                }
                {this.renderPermission()}
                {this.renderBarcode()}
            </R.Tabcontrol>
            {
                this.props.data.barcode.uniqueId &&
                <R.Dialog
                    id="raPhyBarcodePrintDlg"
                    header={RMResx.RM_PRM_PRE_BarCode_Print}
                    width={720}
                    height={500}
                    status={{show: this.state.showDialog}}
                    struct={{foot: false}}
                    onClose={this.hideBarcodeDlg}
                >
                   
                    <span className="barcode-dialog">
                        <div className="dialog-content">
                            <ComponentToPrint ref={r => this.refPrintContent = r}>
                                <div className="barcode-print-content">
                                    <BarcodePreview
                                        data={barcodePreviewData}
                                        barcodeImg={this.getBarcodeImg(this.props.data.barcode.barcodeBase64Str)}
                                        type="1"
                                    />
                                    {/* <div className="barcode-print-row">
                                        <div className="barcode-print-label">{RMResx.RM_PRM_PRE_BarCode_Title}</div>
                                        <div className="barcode-print-value">{this.props.data.barcode.title}</div>
                                    </div>
                                    <div className="barcode-print-row">
                                        <div className="barcode-print-label">{RMResx.RM_PRM_PRE_BarCode_UniqueId}</div>
                                        <div className="barcode-print-value">{this.props.data.barcode.uniqueId}</div>
                                    </div>
                                    <div className="barcode-print-row">
                                        <div className="barcode-print-label">{RMResx.RM_PRM_PRE_BarCode}</div>
                                        <div
                                            className="barcode-print-value">{this.getBarcodeImg(this.props.data.barcode.barcodeBase64Str)}</div>
                                    </div> */}
                                </div>
                            </ComponentToPrint>
                        </div>
                        <div className="dialog-btns flex align-center gap-s">
                            <R.Button text={RMResx.RM_JS_Common_Cancel} onClick={this.hideBarcodeDlg}/>
                            <ReactToPrint
                                trigger={() => <R.Button primary={true} classify="theme" text={RMResx.RM_PRM_PRE_BarCode_Print}/>}
                                content={() => this.refPrintContent}
                            />
                        </div>
                    </span>
                </R.Dialog>
            }
        </div>;
    }
}

