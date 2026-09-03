import React, { useState } from "react";
import { useDispatch } from "react-redux";
// import { AvaWidget } from "@gui/chat-dialog";
import "./index.less";
import CardWrap from "./CardWrap";
import StatusChip from "./StatusChip";
import ManageCategoryPanel from "./ManageCategoryPanel";
import SiteMap from "../../Components/SiteMap";
import SiteMapLinks from "../../../../../Constants/SiteMapLinks";
import { setAvaExternalActionRequest } from "../../../../../Redux/slices/avaDialogSlice";
import { ExternalRequestProductType, OpusExternalRequestType } from "../../../../../Constants/Constants";

const createGuidId = () => {
    return globalThis.crypto ? globalThis.crypto.randomUUID() : Math.random().toString(36).substring(2);
};

const summaryCards = [
    {
        key: "total-storage",
        value: "5,485.21",
        unit: "GB",
        label: "^^Total storage",
        icon: "fia-set-as-index-storage",
    },
    {
        key: "primary-files-count",
        value: "476.08",
        unit: "millions",
        label: "^^Primary files",
        icon: "fia-file",
    },
    {
        key: "primary-files-size",
        value: "5,009.13",
        unit: "GB",
        label: "^^Primary files",
        icon: "fia-file-lines",
    },
    {
        key: "version-history",
        value: "1,645.56",
        unit: "GB",
        label: "^^Version history",
        icon: "fia-select-all",
    },
];

const categoryRows = [
    { name: "Sales", value: 26 },
    { name: "Marketing", value: 19 },
    { name: "Finance", value: 18 },
    { name: "HR", value: 17 },
    { name: "IT", value: 16 },
];

const categoryMaxValue = Math.max(...categoryRows.map((item) => item.value));

const insights = [
    "80% of the data are concentrated in 15% of the sites. It is recommended to focus on the larger sites first.",
    "The data from the Sales, Marketing and IT departments account for 70% of total data. It is recommended that these areas be given priority attention.",
    "You are using multi-geo. Currently, the SCs in the US and CA have a larger storage capacity. Therefore, priority should be given to optimizing these areas.",
];

const insightIcons = ["fia-status-warning", "fia-chart-trend", "fia-multiple"];

const plans = [
    {
        key: "plan-1",
        title: "^^Plan 1",
        target: "^^90% of the target",
        rows: [
            {
                scope: "^^IT, Sales (412 sites)",
                rule: "^^Keep 5 versions, delete >365 days",
                saved: "3,668.88",
                percent: "66.89%",
                status: "Aggressive",
            },
            {
                scope: "^^HR, Finance (52 sites)",
                rule: "^^Keep 10 versions, delete >730 days",
                saved: "2,400.92",
                percent: "43.76%",
                status: "Recommended",
            },
        ],
    },
    {
        key: "plan-2",
        title: "^^Plan 2",
        target: "^^88% of the target",
        rows: [
            {
                scope: "^^IT, Sales (412 sites)",
                rule: "^^Keep 5 versions, delete >365 days",
                saved: "3,668.88",
                percent: "66.89%",
                status: "Aggressive",
            },
            {
                scope: "^^HR, Finance (52 sites)",
                rule: "^^Keep 10 versions, delete >730 days",
                saved: "2,400.92",
                percent: "43.76%",
                status: "Recommended",
            },
        ],
    },
];

const figmaBarChartData = [
    { group: "^^Sales", name: "^^Primary files (GB)", value: 70 },
    { group: "^^Sales", name: "^^Version history (GB)", value: 88 },
    { group: "^^Marketing", name: "^^Primary files (GB)", value: 45 },
    { group: "^^Marketing", name: "^^Version history (GB)", value: 68 },
    { group: "^^Finance", name: "^^Primary files (GB)", value: 64 },
    { group: "^^Finance", name: "^^Version history (GB)", value: 87 },
    { group: "^^HR", name: "^^Primary files (GB)", value: 81 },
    { group: "^^HR", name: "^^Version history (GB)", value: 50 },
    { group: "^^IT", name: "^^Primary files (GB)", value: 73 },
    { group: "^^IT", name: "^^Version history (GB)", value: 49 },
];

const planColumns = [
    {
        header: "^^Scope",
        width: [, 500],
        resizeable: true,
    },
    {
        header: "^^Rule",
        width: [, 500],
        resizeable: true,
    },
    {
        header: "^^Storage saved (GB)",
        width: [, 500],
        resizeable: true,
    },
    {
        header: "^^% saved",
        width: [, 500],
        resizeable: true,
    },
    {
        header: "^^Status",
        width: [, 500],
        resizeable: true,
    },
    {
        header: "^^Actions",
        width: [, 160],
        resizeable: true,
    },
];

class PlanTableRow extends R.TableRow {
    onScopeClick(e, rowData) {
        e.preventDefault();
        if (rowData && rowData.onCreate) {
            rowData.onCreate();
        }
    }

    render(Row, Cell) {
        const rowData = this.props.rowData;

        return (
            <Row>
                <Cell>
                    <div
                        className="ra-view-plan-table-text"
                        data-tooltip="ifneed"
                        aria-label={rowData.scope}
                    >
                        <a
                            href="#"
                            className="ra-main-cell-link"
                            onClick={(e) => this.onScopeClick(e, rowData)}
                        >
                            {rowData.scope}
                        </a>
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="ra-view-plan-table-text"
                        data-tooltip="ifneed"
                        aria-label={rowData.rule}
                    >
                        {rowData.rule}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="ra-view-plan-table-text"
                        data-tooltip="ifneed"
                        aria-label={rowData.saved}
                    >
                        {rowData.saved}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="ra-view-plan-table-text"
                        data-tooltip="ifneed"
                        aria-label={rowData.percent}
                    >
                        {rowData.percent}
                    </div>
                </Cell>
                <Cell>
                    {rowData.status === "Recommended" ? (
                        <StatusChip
                            className="ra-view-plan-status-recommended"
                            color="#078240"
                            backgroundColor="#F5FFF9"
                        >
                            ^^Recommended
                        </StatusChip>
                    ) : (
                        <StatusChip
                            className="ra-view-plan-status-aggressive"
                            backgroundColor="#f0f2f4"
                            color="#687687"
                        >
                            ^^Aggressive
                        </StatusChip>
                    )}
                </Cell>
                <Cell>
                    <R.Button text="^^Create" classify="default" />
                </Cell>
            </Row>
        );
    }
}

const AnalysisPlanViewPage = () => {
    const dispatch = useDispatch();
    const [showManageCategoryPanel, setShowManageCategoryPanel] =
        useState(false);
    const [isWidgetExpanded, setIsWidgetExpanded] = useState(true);

    const openManageCategoryPanel = () => {
        setShowManageCategoryPanel(true);
    };

    const hideManageCategoryPanel = () => {
        setShowManageCategoryPanel(false);
    };

    const triggerExternalAction = (type, profileGroupId) => {
        dispatch(setAvaExternalActionRequest({
            id: createGuidId(),
            productType: ExternalRequestProductType.Opus,
            data: { type, profileGroupId },
        }));
    };

    const renderAvaWidget = () => {
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
                        <AvaWidget.Button onClick={() => triggerExternalAction(OpusExternalRequestType.OptimizeCurrentPlan)}>
                            {"Optimize the current plan"}
                        </AvaWidget.Button>
                        <AvaWidget.Button onClick={() => triggerExternalAction(OpusExternalRequestType.OpusViewHistoryPlan)}>
                            {RMResx.RM_AVA_ViewHistoryPlan_Button}
                        </AvaWidget.Button>
                    </AvaWidget.GroupAction>
                </AvaWidget> */}
            </div>
        );
    };

    const renderSummaryRow = () => {
        return (
            <section className="ra-view-plan-summary-row">
                {summaryCards.map((item) => (
                    <article
                        key={item.key}
                        className="ra-view-plan-summary-card"
                    >
                        <div className="ra-view-plan-summary-value-wrap">
                            <div>
                                <span className="ra-view-plan-summary-value">
                                    {item.value}
                                </span>
                                <span className="ra-view-plan-summary-unit">
                                    {item.unit}
                                </span>
                            </div>
                            <i className={item.icon} aria-hidden="true" />
                        </div>
                        <div className="ra-view-plan-summary-label">
                            {item.label}
                        </div>
                    </article>
                ))}
            </section>
        );
    };

    const renderInsightsRow = () => {
        return (
            <section className="ra-view-plan-insights-row">
                <CardWrap title={"^^AI active insights"}>
                    <div className="ra-view-plan-chart-aui">
                        <R.Charts
                            onDataClick={console.info}
                            onSeriesClick={console.info}
                            height={220}
                        >
                            <R.Charts.Grid
                                items={figmaBarChartData}
                                groupMode="group"
                                axisHeader="Storage (GB)"
                                axisLabelFormat="{0}%"
                                minValue={0}
                                maxValue={100}
                                color={["#5d65e6", "#ffb34a"]}
                            />
                            <R.Charts.Legend
                                slot="block-start"
                                style={{ justifyContent: "start" }}
                            />
                        </R.Charts>
                    </div>
                </CardWrap>
                <CardWrap title="^^Workspace category">
                    <div className="ra-view-plan-category-list">
                        {categoryRows.map((item) => (
                            <div
                                key={item.name}
                                className="ra-view-plan-category-item"
                            >
                                <span className="ra-view-plan-category-name">
                                    {`^^${item.name}`}
                                </span>
                                <div className="ra-view-plan-category-progress-row">
                                    <div className="ra-view-plan-category-progress">
                                        <R.Progressbar
                                            id={`raViewPlanCategory-${item.name}`}
                                            value={item.value}
                                            max={categoryMaxValue}
                                            classify="success"
                                            template={false}
                                            style={{ "--weight": 12 }}
                                        />
                                    </div>
                                    <strong className="ra-view-plan-category-value">
                                        {item.value}
                                    </strong>
                                </div>
                            </div>
                        ))}
                    </div>
                </CardWrap>
                <CardWrap
                    className="ra-view-plan-advice-card"
                    title={
                        <span className="ra-view-plan-ai-title">
                            <i className="fia-brain" aria-hidden="true" />
                            <span>^^AI active insights</span>
                            <i className="fia-robot" aria-hidden="true" />
                        </span>
                    }
                >
                    <ul>
                        {insights.map((item, index) => (
                            <li
                                key={index}
                                className="ra-view-plan-insight-item"
                            >
                                <i
                                    className={`${insightIcons[index]} ra-view-plan-insight-icon`}
                                    aria-hidden="true"
                                />
                                <span>{item}</span>
                            </li>
                        ))}
                    </ul>
                </CardWrap>
            </section>
        );
    };

    const renderPlanSections = () => {
        return plans.map((plan) => {
            const items = plan.rows.map((item) => ({
                ...item,
                onCreate: openManageCategoryPanel,
            }));

            return (
                <section key={plan.key} className="ra-view-plan-plan-section">
                    <div className="ra-view-plan-plan-title">
                        <strong>{plan.title}</strong>
                        <StatusChip
                            className="ra-view-plan-plan-target"
                            color="#078240"
                            backgroundColor="#E5FEEF"
                        >
                            {plan.target}
                        </StatusChip>
                    </div>
                    <div className="ra-view-plan-table-wrap">
                        <R.Table
                            id={`ra-view-plan-table-${plan.key}`}
                            rowTemplate={PlanTableRow}
                            items={items}
                            columns={planColumns}
                            checkable={false}
                            frozenCount={0}
                        />
                    </div>
                </section>
            );
        });
    };

    return (
        <>
            <$g.SiteMap
                data={[
                    SiteMapLinks.FA_Plan_Profile,
                    SiteMapLinks.FA_Plan_ViewPlan,
                ]}
            />
            <div className="ra-view-plan-page">
                {renderAvaWidget()}
                {renderSummaryRow()}
                {renderInsightsRow()}
                {renderPlanSections()}
                {showManageCategoryPanel && (
                    <ManageCategoryPanel onHide={hideManageCategoryPanel} />
                )}
            </div>
        </>
    );
};

export default AnalysisPlanViewPage;
