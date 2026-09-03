import React, { useEffect, useMemo, useState } from 'react';
import Chart from 'react-apexcharts';
import { ArchiveDataUnitI18NMapping, ArchiveDataUnitTypes, AXIS_LABEL_STYLE, LEGEND_ITEMS } from '../../constants';
import { renderTooltipRow } from '../helper';

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
    colors: ['#248AED', '#D01A83', '#0072D0', '#D01A83'],
    fill: {
        opacity: [1, 0.7, 1, 1],
    },
    stroke: {
        curve: 'smooth',
        width: [0, 0, 2, 2],
    },
    markers: {
        size: [0, 0, 3, 3],
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

const LegendIndicator = ({ color, indicator, opacity }) => {
    if (indicator === 'dot') {
        return (
            <div
                style={{
                    width: 10,
                    height: 10,
                    borderRadius: '50%',
                    backgroundColor: color,
                    opacity,
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

const buildCustomTooltip = (seriesIndex, dataPointIndex, rawData) => {
    const currentItem = rawData[dataPointIndex] || {};
    const title = currentItem.Period;
    const archivedStorageBalance = currentItem.ArchivedStorageBalance?.Value || 0;
    const archivedStorageBalanceUnit = currentItem.ArchivedStorageBalance?.Unit || ArchiveDataUnitTypes.GB;
    const destroyedData = currentItem.DestroyedData?.Value || 0;
    const destroyedDataUnit = currentItem.DestroyedData?.Unit || ArchiveDataUnitTypes.GB;
    const destroyedFromArchive = currentItem.DestroyedFromArchiveStorage?.Value || 0;
    const destroyedFromArchiveUnit = currentItem.DestroyedFromArchiveStorage?.Unit || ArchiveDataUnitTypes.GB;
    const destroyedFromLive = currentItem.DestroyedFromLiveStorage?.Value || 0;
    const destroyedFromLiveUnit = currentItem.DestroyedFromLiveStorage?.Unit || ArchiveDataUnitTypes.GB;
    const savingsFromArchiving = currentItem.SavingsFromArchiving ?? 0;
    const savingsFromDestruction = currentItem.SavingsFromDestruction ?? 0;
    const savingsFromArchivedDestruction = currentItem.SavingsFromArchivedDestruction ?? 0;
    const savingsFromLiveDestruction = currentItem.SavingsFromLiveDestruction ?? 0;

    let rows = '';

    if (seriesIndex === 1) {
        rows = [
            renderTooltipRow(
                RMResx.RM_JS_DSB_ChartTooltip_TotalDestroyData.format(ArchiveDataUnitI18NMapping[destroyedDataUnit]),
                destroyedData,
                '#D01A83'
            ),
            renderTooltipRow(
                RMResx.RM_JS_DSB_ChartTooltip_FromArchivedStorage.format(ArchiveDataUnitI18NMapping[destroyedFromArchiveUnit]),
                destroyedFromArchive
            ),
            renderTooltipRow(
                RMResx.RM_JS_DSB_ChartTooltip_FromLiveStorage.format(ArchiveDataUnitI18NMapping[destroyedFromLiveUnit]),
                destroyedFromLive
            ),
        ].join('');
    } else if (seriesIndex === 2) {
        rows = renderTooltipRow(RMResx.RM_JS_DSB_ChartTooltip_SavingFromArchiving, savingsFromArchiving, '#0072D0');
    } else if (seriesIndex === 3) {
        rows = [
            renderTooltipRow(RMResx.RM_JS_DSB_ChartTooltip_SavingFromDestruction, savingsFromDestruction, '#D01A83'),
            renderTooltipRow(RMResx.RM_JS_DSB_ChartTooltip_FromArchivedDestruction, savingsFromArchivedDestruction),
            renderTooltipRow(RMResx.RM_JS_DSB_ChartTooltip_FromLiveDestruction, savingsFromLiveDestruction),
        ].join('');
    } else {
        rows = renderTooltipRow(
            RMResx.RM_JS_DSB_ChartTooltip_ArchivedStorageBalance.format(ArchiveDataUnitI18NMapping[archivedStorageBalanceUnit]),
            archivedStorageBalance,
            '#248AED'
        );
    }

    return `
        <div style="position:relative;background:#323E4D;border-radius:8px;padding:8px;display:flex;flex-direction:column;gap:4px;">
            <div style="font-family:Open Sans, sans-serif;font-weight:600;font-size:14px;line-height:20px;color:#FFFFFF;">${title}</div>
            ${rows}
        </div>
    `;
};

export const StackedColumnChart = ({ data, hasPriceConfig }) => {
    const [state, setState] = useState({
        series: [],
        categories: [],
        maxDataVolume: 5,
        maxSavingPrice: 5,
    });

    useEffect(() => {
        let archivedStorageBalance = [];
        let destroyedData = [];
        let savingFromArchiving = [];
        let savingFromDestruction = [];
        let categories = [];
        let maxDataVolume = state.maxDataVolume;
        let maxSavingPrice = state.maxSavingPrice;

        data.forEach((item) => {
            archivedStorageBalance.push(item.ArchivedStorageBalance?.Value || 0);
            destroyedData.push(item.DestroyedData?.Value || 0);
            savingFromArchiving.push(item.SavingsFromArchiving ?? 0);
            savingFromDestruction.push(item.SavingsFromDestruction ?? 0);
            categories.push(item.Period);
            maxDataVolume = Math.max(maxDataVolume, ((item.ArchivedStorageBalance?.Value || 0) + (item.DestroyedData?.Value || 0)));
            maxSavingPrice = Math.max(maxSavingPrice, ((item.SavingsFromArchiving ?? 0) + (item.SavingsFromDestruction ?? 0)));
        });

        setState({
            series: [
                {
                    name: '',
                    type: 'column',
                    data: archivedStorageBalance,
                },
                {
                    name: '',
                    type: 'column',
                    data: destroyedData,
                },
                {
                    name: '',
                    type: 'line',
                    data: savingFromArchiving,
                },
                {
                    name: '',
                    type: 'line',
                    data: savingFromDestruction,
                },
            ],
            categories,
            maxDataVolume,
            maxSavingPrice,
        });

    }, [data]);

    const chartOptions = useMemo(() => {
        const _maxDataVolume = state.maxDataVolume > 5 ? (Math.ceil(state.maxDataVolume / 9) * 10) : 5;
        const _maxSavingPrice = state.maxSavingPrice > 5 ? (Math.ceil(state.maxSavingPrice / 9) * 10) : 5;

        return {
            ...CHART_OPTIONS,
            tooltip: {
                ...CHART_OPTIONS.tooltip,
                shared: false,
                intersect: false,
                arrow: false,
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
                    max: _maxDataVolume,
                    tickAmount: 5,
                    labels: {
                        style: AXIS_LABEL_STYLE,
                        formatter: (val) => `${Math.ceil(val)}`,
                    },
                },
                {
                    seriesName: '',
                    show: false,
                    min: 0,
                    max: _maxDataVolume,
                },
                {
                    seriesName: '',
                    opposite: true,
                    min: 0,
                    max: _maxSavingPrice,
                    tickAmount: 5,
                    labels: {
                        style: AXIS_LABEL_STYLE,
                        formatter: (val) => `${Math.ceil(val)}`,
                    },
                },
                {
                    seriesName: '',
                    opposite: true,
                    show: false,
                    min: 0,
                    max: _maxSavingPrice,
                },
            ]
        }
    }, [state.categories, state.maxDataVolume, state.maxSavingPrice]);

    if (!hasPriceConfig || !data || data.length === 0) {
        return (
            <NoDataAvailable hasPriceConfig={hasPriceConfig} />
        )
    }

    return (
        <div>
            <div className="flex-row justify-end align-center gap-l">
                {LEGEND_ITEMS.StorageOptimizationBySource.map((item) => (
                    <div key={item.label} className="flex-row align-center gap-s">
                        <LegendIndicator
                            color={item.color}
                            indicator={item.indicator}
                            opacity={item.opacity}
                        />
                        <span className="legend-label">
                            {item.label}
                        </span>
                    </div>
                ))}
            </div>

            <div className="flex-row align-center justify-between axis-title">
                <div>{RMResx.RM_JS_DSB_AxisTitle_DataVolume}</div>
                <div>{RMResx.RM_JS_DSB_AxisTitle_PriceSavings}</div>
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
