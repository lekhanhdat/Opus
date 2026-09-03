import React, { useEffect, useMemo, useState } from 'react';
import Chart from 'react-apexcharts';
import { AXIS_LABEL_STYLE, LEGEND_ITEMS } from '../../constants';
import { formatNumber, renderTooltipContainer, renderTooltipRow } from '../helper';

import '../../index.less';
import { NoDataAvailable } from '../NoDataAvailable';

/** @type {import('apexcharts').ApexOptions} */
const CHART_OPTIONS = {
    chart: {
        type: 'line',
        height: 300,
        stacked: true,
        toolbar: { show: false },
        zoom: { enabled: false },
        fontFamily: 'Open Sans, sans-serif',
    },
    colors: ['#0072D0', '#149EB0', '#D95630'],
    fill: {
        opacity: [1, 1, 1],
    },
    stroke: {
        curve: 'smooth',
        width: [0, 0, 2],
    },
    markers: {
        size: [0, 0, 3],
    },
    plotOptions: {
        bar: {
            columnWidth: '20%',
        },
    },
    xaxis: {
        categories: [],
        labels: {
            style: AXIS_LABEL_STYLE,
        },
        axisBorder: { show: false },
        axisTicks: { show: false },
    },
    yaxis: [],
    grid: {
        borderColor: '#C4C6C8',
        xaxis: { lines: { show: false } },
        yaxis: { lines: { show: true } },
    },
    legend: {
        show: false,
    },
    tooltip: {},
    dataLabels: {
        enabled: false,
    },
}

const buildCustomTooltip = (seriesIndex, dataPointIndex, rawData) => {
    const currentItem = rawData[dataPointIndex] || {};
    const title =  currentItem.Period ?? ''

    const spoContribution = Number(currentItem.SpoContribution ?? 0);
    const odContribution = Number(currentItem.OdContribution ?? 0);
    const totalSavings = Number(currentItem.TotalSavings ?? 0);
    const spoTotalSavings = Number(currentItem.SpoTotalSavings ?? 0);
    const odTotalSavings = Number(currentItem.OdTotalSavings ?? 0);

    if (seriesIndex === 2) {
        return renderTooltipContainer(title, [
            renderTooltipRow(RMResx.RM_JS_DSB_ChartTooltip_TotalSavings, totalSavings, '#D95630'),
            renderTooltipRow(RMResx.RM_JS_DSB_ChartTooltip_SPOSavings, spoTotalSavings),
            renderTooltipRow(RMResx.RM_JS_DSB_ChartTooltip_ODSavings, odTotalSavings),
        ]);
    }

    return renderTooltipContainer(title, [
        `<div style="display:flex;align-items:center;gap:8px;font-family:Open Sans, sans-serif;font-size:14px;line-height:20px;color:#FFFFFF;">
            <span style="display:inline-block;width:12px;height:12px;background:#0072D0;border:1px solid #FFFFFF;box-sizing:border-box;flex-shrink:0;"></span>
            <span>
                ${RMResx.RM_JS_DSB_ChartTooltip_SPOSource.format(
                    `${formatNumber(spoTotalSavings)} - ${formatNumber(spoContribution)}%`
                )}
            </span>
        </div>`,
        `<div style="display:flex;align-items:center;gap:8px;font-family:Open Sans, sans-serif;font-size:14px;line-height:20px;color:#FFFFFF;">
            <span style="display:inline-block;width:12px;height:12px;background:#149EB0;border:1px solid #FFFFFF;box-sizing:border-box;flex-shrink:0;"></span>
            <span>
                ${RMResx.RM_JS_DSB_ChartTooltip_ODSource.format(
                    `${formatNumber(odTotalSavings)} - ${formatNumber(odContribution)}%`
                )}
            </span>
        </div>`,
    ]);
};

const LegendIndicator = ({ color, indicator }) => {
    if (indicator === 'dot') {
        return (
            <div
                style={{
                    width: 10,
                    height: 10,
                    borderRadius: '50%',
                    backgroundColor: color,
                    flexShrink: 0,
                }}
            />
        )
    }
    return (
        <div
            style={{
                width: 10,
                height: 2,
                borderRadius: 8,
                backgroundColor: color,
                flexShrink: 0,
            }}
        />
    )
}

export const HundredPercentStakedColumnChart = ({ data, hasPriceConfig }) => {
    const [state, setState] = useState({
        series: [],
        categories: [],
        maxTotalSaving: 5,
    });
    useEffect(() => {
        let spoContribution = [];
        let odContribution = [];
        let totalSaving = [];
        let categories = [];
        let maxTotalSaving = state.maxTotalSaving;

        data.forEach(item => {
            spoContribution.push(item.SpoContribution ?? 0);
            odContribution.push(item.OdContribution ?? 0);
            totalSaving.push(item.TotalSavings ?? 0);
            categories.push(item.Period ?? '');
            maxTotalSaving = Math.max(maxTotalSaving, item.TotalSavings ?? 0);
        });

        setState({
            series: [
                {
                    name: '',
                    type: 'column',
                    data: spoContribution,
                },
                {
                    name: '',
                    type: 'column',
                    data: odContribution,
                },
                {
                    name: '',
                    type: 'line',
                    data: totalSaving,
                },
            ],
            categories: categories,
            maxTotalSaving: maxTotalSaving,
        });
    }, [data]);

    const chartOptions = useMemo(() => {
        return {
            ...CHART_OPTIONS,
            tooltip: {
                ...CHART_OPTIONS.tooltip,
                shared: false,
                intersect: false,
                theme: 'dark',
                custom: ({ seriesIndex, dataPointIndex }) => buildCustomTooltip(seriesIndex, dataPointIndex, data),
            },
            xaxis: {
                ...CHART_OPTIONS.xaxis,
                categories: state.categories,
            },
            yaxis: [
                {
                    seriesName: '',
                    min: 0,
                    max: 100,
                    tickAmount: 5,
                    labels: {
                        style: AXIS_LABEL_STYLE,
                        formatter: (val) => `${val}%`,
                    },
                },
                {
                    seriesName: '',
                    show: false,
                    min: 0,
                    max: 100,
                },
                {
                    seriesName: '',
                    opposite: true,
                    min: 0,
                    max: state.maxTotalSaving > 5 ? (Math.ceil(state.maxTotalSaving / 9) * 10) : 5,
                    tickAmount: 5,
                    labels: {
                        style: AXIS_LABEL_STYLE,
                        formatter: (val) => `${Math.ceil(val)}`,
                    },
                },

            ]
        }
    }, [state.categories, state.maxTotalSaving])

    if (!hasPriceConfig || !data || data.length === 0) { 
        return (
            <NoDataAvailable hasPriceConfig={hasPriceConfig} />
        )
    }

    return (
        <div>
            <div className="flex-row justify-end align-center gap-l">
                {LEGEND_ITEMS.StorageOptimizationContributionBySource.map((item) => (
                    <div key={item.label} className="flex-row align-center gap-s">
                        <LegendIndicator color={item.color} indicator={item.indicator} />
                        <span className="legend-label">
                            {item.label}
                        </span>
                    </div>
                ))}
            </div>

            <div className="flex-row align-center justify-between axis-title">
                <div>{RMResx.RM_JS_DSB_AxisTitle_ContributionToLocalSaving}</div>
                <div>{RMResx.RM_JS_DSB_AxisTitle_TotalSavings}</div>
            </div>

            <Chart
                options={chartOptions}
                series={state.series}
                type="line"
                height={300}
            />
        </div>
    )
}
