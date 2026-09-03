import { useEffect, useState } from "react";
import { ConfigureStoragePrice } from "./ConfigureStoragePrice";
import { ArchiveDataUnitI18NMapping, DateRangeSelectorItems, ResourceSelectorItems, ResourceTypes, STORAGE_VALUE_SUMMARY_CARD_ITEMS, TimeRangeTypes } from "./constants";

import "./index.less";
import { StatisticCard } from "./Components/StatisticCard";
import { MultipleColumnChart } from "./Components/MultipleColumnChart";
import { HundredPercentStakedColumnChart } from "./Components/HundredPercentStakedColumnChart";
import { StackedColumnChart } from "./Components/StackedColumnChart";
import { showToast } from "../../../../Utilities/CommonUtil";

export const ValueAndSavingView = () => {
    const [timeRange, setTimeRange] = useState(TimeRangeTypes.All);

    const [sourceFilter, setSourceFilter] = useState(ResourceTypes.All);

    const [storageValueSummary, setStorageValueSummary] = useState(null);

    const [archivedOverview, setArchivedOverview] = useState([]);

    const [optimizationOverview, setOptimizationOverview] = useState([]);

    const [optimizationContribution, setOptimizationContribution] = useState([]);

    const [showConfigureStoragePricePanel, setShowConfigureStoragePricePanel] = useState({
        show: false,
    });

    const [hasPriceConfig, setHasPriceConfig] = useState({
        archiveStorageOverview: false,
        storageOptimizationBySource: false,
        storageOptimizationContributionBySource: false,
    });

    const [storagePriceSettings, setStoragePriceSettings] = useState({
        sharePointLivePrice: 0,
        oneDriveLivePrice: 0,
        sharePointArchivePrice: 0,
        oneDriveArchivePrice: 0,
    });

    const [draftStoragePriceSettings, setDraftStoragePriceSettings] = useState({
        sharePointLivePrice: 0,
        oneDriveLivePrice: 0,
        sharePointArchivePrice: 0,
        oneDriveArchivePrice: 0,
    });

    useEffect(() => {
        (async () => {
            $$.loading(true);
            await Promise.all([
                onGetArchivedOverview(),
                onGetStorageValueSummary(),
                onGetOptimizationOverviewBySource(),
                onGetOptimizationContributionBySource(),
                onGetValueAndSavingsPriceConfiguration(),
            ]);
            $$.loading(false);
        })();
    }, []);

    const handleShowConfigureStoragePricePanel = () => {
        setDraftStoragePriceSettings({ ...storagePriceSettings });
        setShowConfigureStoragePricePanel({ show: true });
    }

    const onHideConfigureStoragePricePanel = () => {
        setShowConfigureStoragePricePanel({ show: false })
    }

    const onStoragePriceSettingChange = (fieldName, value) => {
        setDraftStoragePriceSettings((prevValues) => ({
            ...prevValues,
            [fieldName]: value,
        }));
    }

    const onTimeRangeChange = (args) => {
        const newValue = args.newValue.value;
        setTimeRange(newValue);
        onRefreshByTimeRange(newValue);
    }

    const onSourceFilterChange = (args) => {
        const newValue = args.newValue.value;
        setSourceFilter(newValue);
        onRefreshBySourceFilter(newValue);
    }

    const onRefreshByTimeRange = async (newTimeRange) => {
        $$.loading(true);
        await Promise.all([
            onGetStorageValueSummary(newTimeRange, sourceFilter),
            onGetArchivedOverview(newTimeRange),
            onGetOptimizationOverviewBySource(newTimeRange, sourceFilter),
            onGetOptimizationContributionBySource(newTimeRange),
        ]);
        $$.loading(false);
    }

    const onRefreshBySourceFilter = async (newSourceFilter) => {
        $$.loading(true);
        await onGetOptimizationOverviewBySource(timeRange, newSourceFilter);
        $$.loading(false);
    }

    const onGetStorageValueSummary = async (newTimeRange = timeRange, newSourceFilter = sourceFilter) => {
        const options = {
            url: '/api/Dashboard/GetStorageValueSummary',
            method: 'POST',
            data: {
                TimeRange: newTimeRange,
                SourceFilter: newSourceFilter,
            }
        };
        const res = await fetchUtility(options);
        if (res) {
            setStorageValueSummary(res);
        }
    };

    const onGetArchivedOverview = async (newTimeRange = timeRange) => {
        const options = {
            url: '/api/Dashboard/GetArchivedOverview',
            method: 'POST',
            data: {
                TimeRange: newTimeRange,
            }
        };
        const res = await fetchUtility(options);
        if (res) {
            setArchivedOverview(res?.ArchivedOverview ?? []);
            setHasPriceConfig((prev) => ({
                ...prev,
                archiveStorageOverview: res?.HasPriceConfig ?? false,
            }));
        }
    };

    const onGetOptimizationOverviewBySource = async (newTimeRange = timeRange, newSourceFilter = sourceFilter) => {
        const options = {
            url: '/api/Dashboard/GetOptimizationOverviewBySource',
            method: 'POST',
            data: {
                TimeRange: newTimeRange,
                SourceFilter: newSourceFilter,
            }
        };
        const res = await fetchUtility(options);
        if (res) {
            setOptimizationOverview(res?.OptimizationOverviewBySource ?? []);
            setHasPriceConfig((prev) => ({
                ...prev,
                storageOptimizationBySource: res?.HasPriceConfig ?? false,
            }));
        }
    };

    const onGetOptimizationContributionBySource = async (newTimeRange = timeRange) => {
        const options = {
            url: '/api/Dashboard/GetOptimizationContributionBySource',
            method: 'POST',
            data: {
                TimeRange: newTimeRange,
            }
        };
        const res = await fetchUtility(options);
        if (res) {
            setOptimizationContribution(res?.OptimizationContributionBySource ?? []);
            setHasPriceConfig((prev) => ({
                ...prev,
                storageOptimizationContributionBySource: res?.HasPriceConfig ?? false,
            }));
        }
    };

    const onGetValueAndSavingsPriceConfiguration = async () => {
        const options = {
            url: '/api/Dashboard/GetValueAndSavingsPriceConfiguration',
            method: 'POST',
        };
        const res = await fetchUtility(options);
        if (res) {
            setStoragePriceSettings({
                sharePointLivePrice: res.SpoLivePrice,
                oneDriveLivePrice: res.OdLivePrice,
                sharePointArchivePrice: res.SpoArchivePrice,
                oneDriveArchivePrice: res.OdArchivePrice,
            });
        }
    };

    const onSaveSetting = () => {
        if (!$$.verify("allValidation")) return;

        const options = {
            url: '/api/Dashboard/SaveValueAndSavingsPriceConfiguration',
            method: 'POST',
            data: {
                SpoLivePrice: draftStoragePriceSettings.sharePointLivePrice,
                OdLivePrice: draftStoragePriceSettings.oneDriveLivePrice,
                SpoArchivePrice: draftStoragePriceSettings.sharePointArchivePrice,
                OdArchivePrice: draftStoragePriceSettings.oneDriveArchivePrice,
            }
        };

        fetchUtility(options).then((res) => {
            if (res) {
                showToast.success(RMResx.RM_JS_Common_SaveSucess);
                setShowConfigureStoragePricePanel({ show: false });
                onGetValueAndSavingsPriceConfiguration();
                onRefreshByTimeRange(timeRange);
            }
        });
    };

    return (
        <>
            <div className="flex-column gap-m">
                <R.Button
                    primary={true}
                    classify="theme"
                    text={RMResx.RM_JS_DSB_ConfigureStoragePrice}
                    onClick={handleShowConfigureStoragePricePanel}
                />

                <section className="reco-dashboard-cards">
                    <div className="flex-row justify-between margin-bottom-m">
                        <div className="reco-dashboard-cards-title" tabIndex={0}>
                            {RMResx.RM_JS_DSB_StorageSummary}
                        </div>
                        <div className="reco-dashboard-selector-wrapper">
                            <div className="reco-dashboard-time-selector">
                                <R.Combobox
                                    width="100%"
                                    height="100%"
                                    items={DateRangeSelectorItems}
                                    disabled={false}
                                    textField="name"
                                    valueField="value"
                                    searchable={false}
                                    linkMode={false}
                                    excludeChecked={true}
                                    mini={true}
                                    onChange={onTimeRangeChange}
                                />
                            </div>
                        </div>
                    </div>

                    <div className="statistic-cards-wrapper">
                        {STORAGE_VALUE_SUMMARY_CARD_ITEMS.map((item) => {
                            let value = 0;
                            let unit = item.hasUnit ? item.unit : undefined;
                            if (storageValueSummary?.[item.key]) {
                                if (typeof storageValueSummary[item.key] === 'number' || typeof storageValueSummary[item.key] === 'string') {
                                    value = storageValueSummary[item.key];
                                } else if (typeof storageValueSummary[item.key] === 'object') {
                                    value = storageValueSummary[item.key].Value;
                                    unit = ArchiveDataUnitI18NMapping?.[storageValueSummary[item.key].Unit];
                                }
                            }
                            return (
                                <StatisticCard
                                    key={item.key}
                                    title={item.label}
                                    description={item.description}
                                    value={value}
                                    unit={unit}
                                />
                            )
                        })}
                    </div>
                </section>

                <section className="reco-dashboard-cards">
                    <div className="flex-row justify-between margin-bottom-m">
                        <div className="reco-dashboard-cards-title" tabIndex={0}>
                            {RMResx.RM_JS_DSB_ArchiveStorageOverview}
                        </div>
                    </div>
                    <div>
                        <MultipleColumnChart
                            data={archivedOverview}
                            hasPriceConfig={hasPriceConfig.archiveStorageOverview}
                        />
                    </div>
                </section>

                <section className="reco-dashboard-cards">
                    <div className="flex-row justify-between margin-bottom-m">
                        <div className="reco-dashboard-cards-title" tabIndex={0}>
                            {RMResx.RM_JS_DSB_StorageOptimizationBySource}
                        </div>
                        <div className="reco-dashboard-selector-wrapper">
                            <div className="reco-dashboard-resource-selector">
                                <R.Combobox
                                    width="100%"
                                    height="100%"
                                    items={ResourceSelectorItems}
                                    disabled={false}
                                    textField="name"
                                    valueField="value"
                                    searchable={false}
                                    linkMode={false}
                                    excludeChecked={true}
                                    mini={true}
                                    onChange={onSourceFilterChange}
                                />
                            </div>
                        </div>
                    </div>
                    <div>
                        <StackedColumnChart
                            data={optimizationOverview}
                            hasPriceConfig={hasPriceConfig.storageOptimizationBySource}
                        />
                    </div>
                </section>

                <section className="reco-dashboard-cards">
                    <div className="flex-row justify-between margin-bottom-m">
                        <div className="reco-dashboard-cards-title" tabIndex={0}>
                            {RMResx.RM_JS_DSB_StorageOptimizationContributionBySource}
                        </div>
                    </div>
                    <div>
                        <HundredPercentStakedColumnChart
                            data={optimizationContribution}
                            hasPriceConfig={hasPriceConfig.storageOptimizationContributionBySource}
                        />
                    </div>
                </section>
            </div>

            <R.Panel
                id="raConfigureStoragePricePanel"
                header={RMResx.RM_JS_DSB_ConfigureStoragePrice}
                size={680}
                status={showConfigureStoragePricePanel}
                destroy={true}
                onClose={onHideConfigureStoragePricePanel}
            >
                <R.Validation id="allValidation">
                    <ConfigureStoragePrice
                        value={draftStoragePriceSettings}
                        onChange={onStoragePriceSettingChange}
                    />
                </R.Validation>
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onHideConfigureStoragePricePanel} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={onSaveSetting} />
                </>
            </R.Panel>
        </>
    )
}