import { forwardRef, useImperativeHandle, useState } from "react";
import _ from "lodash";

const buildInColumns = [
    {
        header: RMResx.RM_AR_RC_Whitelist_TableCol_SiteCollectionUrl,
        width: 500,
    },
    {
        header: "",
        width: 100,
    },
];

const whitelistDefaultObj = {
    SiteCollectionUrl: "",
    IsShowDeleteBtn: false,
};

const AddWhitelist = ({ isSCBlackListForEdiscovery }, ref) => {
    const [addWhitelist, setAddWhitelist] = useState([
        _.cloneDeep(whitelistDefaultObj),
    ]);

    const [isTooLong, setIsTooLong] = useState(false);

    const [isEmpty, setIsEmpty] = useState(false);

    useImperativeHandle(ref, () => ({
        getAddWhitelist: () => {
            return addWhitelist;
        },
        isValid: () => {
            if (addWhitelist.some((item) => item.SiteCollectionUrl === "")) {
                setIsEmpty(true);
                return false;
            }
            return true;
        },
    }));

    const onInitValid = () => {
        setIsTooLong(false);
        setIsEmpty(false);
    };

    const onRowEvent = (args) => {
        onInitValid();
        const rowData = args.rowData;
        const rowIndex = args.rowIndex;
        const cloneAddWhiteList = _.cloneDeep(addWhitelist);
        switch (args.type) {
            case "setRowData":
                cloneAddWhiteList[rowIndex] = rowData;
                if (cloneAddWhiteList.some((item) => item.SiteCollectionUrl === "")) {
                    setIsEmpty(true);
                }
                break;
            case "deleteData":
                cloneAddWhiteList.splice(rowIndex, 1);
                if (cloneAddWhiteList.length > 1) {
                    cloneAddWhiteList.forEach(
                        (item) => (item.IsShowDeleteBtn = true)
                    );
                } else {
                    cloneAddWhiteList.forEach(
                        (item) => (item.IsShowDeleteBtn = false)
                    );
                }
                break;
            case "addRowData":
                onInitValid();
                const add = _.cloneDeep(whitelistDefaultObj);
                if (cloneAddWhiteList.length >= 10) {
                    setIsTooLong(true);
                } else {
                    cloneAddWhiteList.push(add);
                    if (cloneAddWhiteList.length > 1) {
                        cloneAddWhiteList.forEach(
                            (item) => (item.IsShowDeleteBtn = true)
                        );
                    } else {
                        cloneAddWhiteList.forEach(
                            (item) => (item.IsShowDeleteBtn = false)
                        );
                    }
                }
                break;
            default:
                break;
        }
        setAddWhitelist(cloneAddWhiteList);
    };

    return (
        <div>
            <div className="margin-bottom-m">
                <R.Table
                    id="raAddWhitelistTable"
                    columns={buildInColumns}
                    rowTemplate={AddWhitelistTableTemplate}
                    items={addWhitelist}
                    onRowEvent={onRowEvent}
                />
            </div>
            <div>
                <$g.ValidationMsg show={isTooLong}>
                    {isSCBlackListForEdiscovery ? RMResx.RM_AR_RC_AddBlacklist_ErrorMsg : RMResx.RM_AR_RC_AddWhitelist_ErrorMsg}
                </$g.ValidationMsg>
                <$g.ValidationMsg show={!isTooLong && isEmpty}>
                    {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue}
                </$g.ValidationMsg>
            </div>
        </div>
    );
};

export default forwardRef(AddWhitelist);

class AddWhitelistTableTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    onChanged(args0, args1) {
        if (args0 === "SiteColumn") {
            this.props.rowData.SiteCollectionUrl = args1;
        }
        this.dispatch("setRowData");
    }

    removeData(args) {
        this.dispatch("deleteData");
    }

    addData(args) {
        this.dispatch("addRowData");
    }

    render(Row, Cell) {
        const rowData = this.props.rowData;

        return (
            <Row>
                <Cell>
                    <div className="flex ra-flex-align-top">
                        <R.Input
                            id="raSiteCollectionUrlIpt"
                            type="text"
                            maxlength={255}
                            value={rowData.SiteCollectionUrl}
                            onChange={this.onChanged.bind(
                                this,
                                "SiteColumn"
                            )}
                        />
                    </div>
                </Cell>
                <Cell>
                    {rowData.IsShowDeleteBtn && (
                        <R.Button
                            type="bald"
                            icon="crm-criteria fia-close"
                            tooltip={RMResx.RM_JS_Common_Delete}
                            onClick={this.removeData.bind(this)}
                        />
                    )}
                    <R.Button
                        type="bald"
                        icon="crm-criteria fia-plus"
                        tooltip={RMResx.RM_JS_BCM_Explorer_MRR_Add_Button_Add}
                        onClick={this.addData.bind(this)}
                    />
                </Cell>
            </Row>
        );
    }
}
