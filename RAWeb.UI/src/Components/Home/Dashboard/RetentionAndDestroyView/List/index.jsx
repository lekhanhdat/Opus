import { useEffect, useState } from "react";

import { NormalCell } from "../../../../Common/TableTemplateCell";
import { RententionListRequestOption } from "../config";
import { DateUnit, Sourceflags } from "../Constants";
import { showToast } from "../../../../../Utilities/CommonUtil";

const TableColumns = [
    {
        header: RMResx.RM_DSB_Retention_Column_FileName,
        width: [150],
        resizeable: true,
    },
    {
        header: RMResx.RM_DSB_Retention_Column_Url,
        width: [180],
        resizeable: true,
    },
    {
        header: RMResx.RM_DSB_Retention_Column_ContentSource,
        width: [140],
        resizeable: true,
    },
    {
        header: RMResx.RM_DSB_Retention_Column_Size,
        width: [80],
        resizeable: true,
    },
    {
        header: RMResx.RM_DSB_Retention_Column_Setting,
        width: [150],
        resizeable: true,
    },
    {
        header: RMResx.RM_DSB_Retention_Column_Storage,
        width: [150],
        resizeable: true,
    },
];

class Template extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render(Row, Cell) {
        const {
            SiteUrl,
            SrcStorageName,
            SizeStr,
            FileName,
            SourceFlag,
            RetentionSource,
            RetentionKeepDate,
            RetentionKeepDateUnit,
        } = this.props.rowData;

        return (
            <Row>
                <NormalCell Cell={Cell} contentText={FileName} />
                <NormalCell Cell={Cell} contentText={SiteUrl} />
                <NormalCell Cell={Cell} contentText={Sourceflags[SourceFlag]} />
                <NormalCell Cell={Cell} contentText={SizeStr} />
                <NormalCell Cell={Cell}>
                    <$g.I18NProvider
                        msg={RMResx.RM_DSB_Retention_Column_SettingValue}
                    >
                        <span>{RetentionSource}</span>
                        <span>{RetentionKeepDate}</span>
                        <span>{DateUnit[RetentionKeepDateUnit]}</span>
                    </$g.I18NProvider>
                </NormalCell>
                <NormalCell Cell={Cell} contentText={SrcStorageName} />
            </Row>
        );
    }
}

function Retentionlist() {
    const [data, setData] = useState([]);
    const [pagination, setPagination] = useState({
        totalCount: 0,
        pageIndex: 0,
        pageSize: 15,
    });

    useEffect(() => {
        loadAllRetentionList();
    }, [pagination.pageIndex, pagination.pageSize]);

    const loadAllRetentionList = async () => {
        $$.loading(true);
        const res = await fetchUtility({
            ...RententionListRequestOption,
            data: {
                PageSize: pagination.pageSize,
                CurrentPage: pagination.pageIndex + 1,
            },
        });
        $$.loading(false);
        if (res) {
            const data = JSON.parse(res);
            setData(data.Details || []);
            setPagination((prev) => ({
                ...prev,
                totalCount: data.TotalNumber,
            }));
        }
    };

    const onExport = async () => {
        $$.messagedialog(false);
        $$.loading(true);
        const option = {
            url: "/api/Dashboard/RunExportArchiverRetentionSimulateInfoJob",
            method: "POST",
            data: {},
        };
        const res = await fetchUtility(option);
        $$.loading(false);
        if (res) {
            if (res.MessageType === 0) {
                const content = (
                    <$g.I18NProvider
                        msg={RMResx.RM_DSB_Retention_Export_JobStart}
                    >
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
                showToast.error(res.ErrorMessage);
            }
        }
    };

    const handleExport = () => {
        const args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_DSB_Retention_ExportConfirmMsg,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => $$.messagedialog(false),
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: onExport,
                },
            ],
        };
        $$.messagedialog(true, args);
    };

    const onPaginationChange = (pageIndex, pageSize, callback) => {
        setPagination((prev) => ({
            ...prev,
            pageIndex,
            pageSize,
        }));
        callback(true);
    };

    return (
        <div>
            <R.Button
                primary
                classify="theme"
                text={RMResx.RM_DSB_Retention_ExportBtn}
                onClick={handleExport}
            />
            <div className="margin-top-m">
                <R.Table
                    id="DSBTableTeamsGroups"
                    height={["auto", "460px"]}
                    columns={TableColumns}
                    items={data}
                    rowTemplate={Template}
                    flexible
                />
                <div
                    style={{ justifyContent: "space-between" }}
                    className="flex padding-top-m"
                >
                    <$g.Pager
                        itemsCount={pagination.totalCount}
                        pagerIndex={pagination.pageIndex}
                        pagerSize={pagination.pageSize}
                        showPagerSize
                        showPagerCounter
                        pagerSizeOptions={[5, 10, 15]}
                        onChange={onPaginationChange}
                    />
                </div>
            </div>
        </div>
    );
}

export default Retentionlist;
