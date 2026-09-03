import React, { useEffect, useMemo, useState } from 'react';
import Chart from 'react-apexcharts';
import { ArchiveDataUnitI18NMapping, ArchiveDataUnitTypes, AXIS_LABEL_STYLE, LEGEND_ITEMS } from '../../constants';

import '../../index.less';
import { renderTooltipContainer, renderTooltipRow } from '../helper';
import { NoDataAvailable } from '../NoDataAvailable';

/** @type {import('apexcharts').ApexOptions} */
const CHART_OPTIONS = {
    chart: {
        type: 'line',
        height: 300,
        stacked: false,
        toolbar: { show: false },
        zoom: { enabled: false },
        fontFamily: 'Open Sans, sans-serif',
    },
    colors: ['#149EB0', '#D01A83', '#0072D0'],
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
    yaxis: {
        min: 0,
        max: 500,
        tickAmount: 5,
        labels: {
            style: AXIS_LABEL_STYLE,
        },
    },
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

const buildCustomTooltip = (_seriesIndex, dataPointIndex, rawData) => {
    const currentItem = rawData[dataPointIndex] || {};
    const title = currentItem.Period ?? ''

    const newlyArchivedData = Number(currentItem.NewlyArchivedData?.Value ?? 0);
    const newlyArchivedDataUnit = currentItem.NewlyArchivedData?.Unit ?? ArchiveDataUnitTypes.GB;
    const destroyedData = Number(currentItem.DestroyedDataFromArchive?.Value ?? 0);
    const destroyedDataUnit = currentItem.DestroyedDataFromArchive?.Unit ?? ArchiveDataUnitTypes.GB;
    const archivedStorageBalance = Number(currentItem.ArchivedStorageBalance?.Value ?? 0);
    const archivedStorageBalanceUnit = currentItem.ArchivedStorageBalance?.Unit ?? ArchiveDataUnitTypes.GB;

    return renderTooltipContainer(title, [
        renderTooltipRow(
            RMResx.RM_JS_DSB_ChartTooltip_NewlyArchived.format(ArchiveDataUnitI18NMapping[newlyArchivedDataUnit]),
            newlyArchivedData,
            '#149EB0'
        ),
        renderTooltipRow(
            RMResx.RM_JS_DSB_ChartTooltip_DestroyedFromArchived.format(ArchiveDataUnitI18NMapping[destroyedDataUnit]),
            destroyedData,
            '#D01A83'
        ),
        renderTooltipRow(
            RMResx.RM_JS_DSB_ChartTooltip_ArchivedBalance.format(ArchiveDataUnitI18NMapping[archivedStorageBalanceUnit]),
            archivedStorageBalance,
            '#0072D0'
        ),
    ]);
};

export const MultipleColumnChart = ({ data, hasPriceConfig }) => {
    const [state, setState] = useState({
        series: [],
        categories: [],
        maxY: 5,
    });

    useEffect(() => {
        if (data && data.length > 0) {
            let newlyArchivedData = [];
            let destroyedData = [];
            let archivedStorageBalance = [];
            let categories = [];
            let maxY = state.maxY;

            data.forEach(item => {
                newlyArchivedData.push(item.NewlyArchivedData?.Value ?? 0);
                destroyedData.push(item.DestroyedDataFromArchive?.Value ?? 0);
                archivedStorageBalance.push(item.ArchivedStorageBalance?.Value ?? 0);
                categories.push(item.Period ?? '');
                maxY = Math.max(maxY, item.NewlyArchivedData?.Value ?? 0, item.DestroyedDataFromArchive?.Value ?? 0, item.ArchivedStorageBalance?.Value ?? 0);
            });

            setState({
                series: [
                    {
                        name: '',
                        type: 'column',
                        data: newlyArchivedData,
                    },
                    {
                        name: '',
                        type: 'column',
                        data: destroyedData,
                    },
                    {
                        name: '',
                        type: 'line',
                        data: archivedStorageBalance,
                    },
                ],
                categories: categories,
                maxY: maxY,
            })
        }
    }, [data]);

    const chartOptions = useMemo(() => ({
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
        yaxis: {
            ...CHART_OPTIONS.yaxis,
            max: state.maxY > 5 ? (Math.ceil(state.maxY / 9) * 10) : 5,
            labels: {
                ...CHART_OPTIONS.yaxis.labels,
                formatter: (val) => `${Math.ceil(val)}`,
            },
        },
    }), [state.categories, state.maxY]);

    if (!hasPriceConfig || !data || data.length === 0) { 
        return (
            <NoDataAvailable hasPriceConfig={hasPriceConfig} />
        )
    }

    return (
        <div>
            <div className="flex-row justify-end align-center gap-l">
                {LEGEND_ITEMS.ArchiveStorageOverview.map((item) => (
                    <div key={item.label} className="flex-row align-center gap-s">
                        <LegendIndicator color={item.color} indicator={item.indicator} />
                        <span className="legend-label">
                            {item.label}
                        </span>
                    </div>
                ))}
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
