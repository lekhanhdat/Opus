import _ from "lodash";
import { LicenseHelper } from "../../../../Utilities/CommonUtil";
import { SourceFlag } from "../../../Common/Constants";
import { AutoFitDriverName } from "../../Discovery/AnalysisConfigurator/GoogleDrive/Components/AnalysisConfigurationScopeComponent";
import { BasicDataRequester, ConfigurationRequester } from "../requests";
import { FileSystemBasicDataRequester } from "../requests/FileSystem";
import { GoogleDriveBasicDataRequester } from "../requests/GoogleDrive";
import NumberUtil from "./NumberUtil";
import UnitConvertsionUtil from "./UnitConvertsionUtil";

class CalculateUtil {
    static CalculateProgressOptimizedSiteInfoes = async (items) => {
        const clonedItems = _.cloneDeep(items);
        const savingInfo =
            await ConfigurationRequester.getCostSavingConfigurationInfo();
        for (const item of clonedItems) {
            const archivedSaving =
                item.contentSource === SourceFlag.SharePoint
                    ? savingInfo.spStoragePrice -
                      savingInfo.archivedDataStoragePrice
                    : savingInfo.odStoragePrice -
                      savingInfo.archivedDataStoragePrice;
            const storagePrice =
                item.contentSource === SourceFlag.SharePoint
                    ? savingInfo.spStoragePrice
                    : savingInfo.odStoragePrice;
            item.fileTotalSize = NumberUtil.internaltionalCounting(
                item.fileTotalSize
            );
            item.fileSumCount = NumberUtil.internaltionalCounting(
                item.fileSumCount
            );
            item.nextOptimizableFileTotalSize =
                NumberUtil.internaltionalCounting(
                    item.nextOptimizableFileTotalSize
                );
            item.nextOptimizableVersionTotalSize =
                NumberUtil.internaltionalCounting(
                    item.nextOptimizableVersionTotalSize
                );
            item.saving = (
                item.deleted * storagePrice +
                item.archived * archivedSaving
            ).toFixed(2);
            item.archived = NumberUtil.internaltionalCounting(item.archived);
            item.deleted = NumberUtil.internaltionalCounting(item.deleted);
        }
        return clonedItems;
    };

    static CalculateInactivesNodeTotalAggregateInfoV3 = async (item) => {
        const clonedItem = await this.CalculateInactivesNodeTotalAggregateInfo(item);
        clonedItem["phlTotalSize"] = NumberUtil.internaltionalCounting(
            CalculateUtil.GetConvertValue(clonedItem["phlTotalSize"], 2)
        );
        return clonedItem;
    };

    static CalculateInactivesNodeTotalAggregateInfo = async (item) => {
        const clonedItem = _.cloneDeep(item);
        const inativeRuleColumns =
            await BasicDataRequester.getInactiveTableColumns();
        const savingInfo =
            await ConfigurationRequester.getCostSavingConfigurationInfo();

        const spInactiveFileSize = _.isNil(
            clonedItem[`inactiveFileTotalSize_${SourceFlag.SharePoint}`]
        )
            ? 0
            : clonedItem[`inactiveFileTotalSize_${SourceFlag.SharePoint}`];
        const odInactiveFileSize = _.isNil(
            clonedItem[`inactiveFileTotalSize_${SourceFlag.OneDrive}`]
        )
            ? 0
            : clonedItem[`inactiveFileTotalSize_${SourceFlag.OneDrive}`];

        const spInactiveFileSumCount = _.isNil(
            clonedItem[`inactiveFileSumCount_${SourceFlag.SharePoint}`]
        )
            ? 0
            : clonedItem[`inactiveFileSumCount_${SourceFlag.SharePoint}`];
        const odInactiveFileSumCount = _.isNil(
            clonedItem[`inactiveFileSumCount_${SourceFlag.OneDrive}`]
        )
            ? 0
            : clonedItem[`inactiveFileSumCount_${SourceFlag.OneDrive}`];

        const spFileTotalSize = _.isNil(
            clonedItem[`fileTotalSize_${SourceFlag.SharePoint}`]
        )
            ? 0
            : clonedItem[`fileTotalSize_${SourceFlag.SharePoint}`];
        const odFileTotalSize = _.isNil(
            clonedItem[`fileTotalSize_${SourceFlag.OneDrive}`]
        )
            ? 0
            : clonedItem[`fileTotalSize_${SourceFlag.OneDrive}`];

        const spFileSumCount = _.isNil(
            clonedItem[`fileSumCount_${SourceFlag.SharePoint}`]
        )
            ? 0
            : clonedItem[`fileSumCount_${SourceFlag.SharePoint}`];
        const odFileSumCount = _.isNil(
            clonedItem[`fileSumCount_${SourceFlag.OneDrive}`]
        )
            ? 0
            : clonedItem[`fileSumCount_${SourceFlag.OneDrive}`];

        const spPriceSize = Math.min(
            CalculateUtil.GetConvertValue(spFileTotalSize) -
                CalculateUtil.GetConvertValue(
                    savingInfo.spFreeStorage * 1024 * 1024 * 1024
                ),
            CalculateUtil.GetConvertValue(spInactiveFileSize)
        );
        const odPriceSize = Math.min(
            CalculateUtil.GetConvertValue(odFileTotalSize) -
                CalculateUtil.GetConvertValue(
                    savingInfo.odFreeStorage * 1024 * 1024 * 1024
                ),
            CalculateUtil.GetConvertValue(odInactiveFileSize)
        );

        const spSaving =
            Math.max((savingInfo.spStoragePrice - savingInfo.archivedDataStoragePrice), 0) *
            (spPriceSize > 0 ? spPriceSize : 0);
        const odSaving =
            Math.max((savingInfo.odStoragePrice - savingInfo.archivedDataStoragePrice), 0) *
            (odPriceSize > 0 ? odPriceSize : 0);

        clonedItem["inScope"] = NumberUtil.internaltionalCounting(
            clonedItem["inScope"]
        );
        clonedItem["siteCount"] = NumberUtil.internaltionalCounting(
            clonedItem["siteCount"]
        );
        clonedItem["fileTotalSize"] = NumberUtil.internaltionalCounting(
            CalculateUtil.GetConvertValue(spFileTotalSize + odFileTotalSize, 2)
        );
        clonedItem["fileSumCount"] = NumberUtil.internaltionalCounting(
            spFileSumCount + odFileSumCount
        );
        clonedItem["inactiveFileSumCount"] = NumberUtil.internaltionalCounting(
            spInactiveFileSumCount + odInactiveFileSumCount
        );
        clonedItem["inactiveFileTotalSize"] = NumberUtil.internaltionalCounting(
            CalculateUtil.GetConvertValue(spInactiveFileSize + odInactiveFileSize, 2)
        );
        for (const inaciveRuleColumn of inativeRuleColumns) {
            clonedItem[inaciveRuleColumn.internalName] = _.isNil(
                item[inaciveRuleColumn.internalName]
            )
                ? 0
                : NumberUtil.internaltionalCounting(
                      CalculateUtil.GetConvertValue(
                          clonedItem[inaciveRuleColumn.internalName], 2
                      )
                  );
        }
        clonedItem["saving"] = NumberUtil.internaltionalCounting(
            (spSaving + odSaving).toFixed(0)
        );
        if (spFileTotalSize + odFileTotalSize === 0) {
            clonedItem["rate"] = "0%";
        } else {
            const temp =
                Number.parseInt(
                    (
                        (spInactiveFileSize + odInactiveFileSize) /
                        (spFileTotalSize + odFileTotalSize)
                    )
                        .toFixed(2)
                        .replace(".", "")
                ) + "%";
            clonedItem["rate"] =
                temp === "0%" && spInactiveFileSize + odInactiveFileSize !== 0
                    ? "1%"
                    : temp;
        }
        return clonedItem;
    };

    static CalculateInactivesNodesData = async (items) => {
        const clonedItems = _.cloneDeep(items);
        const inativeRuleColumns =
            await BasicDataRequester.getInactiveTableColumns();

        const savingInfo =
            await ConfigurationRequester.getCostSavingConfigurationInfo();
        for (let item of clonedItems) {
            const price =
                Math.max(
                item.contentSource === SourceFlag.SharePoint
                    ? savingInfo.spStoragePrice -
                      savingInfo.archivedDataStoragePrice
                    : savingInfo.odStoragePrice -
                      savingInfo.archivedDataStoragePrice, 0);

            const inactiveSize = CalculateUtil.GetConvertValue(
                item["inactiveFileTotalSize"], 2
            );
            const fileSize = CalculateUtil.GetConvertValue(item["fileTotalSize"], 2);

            item["siteCount"] = NumberUtil.internaltionalCounting(
                item["siteCount"]
            );
            item["fileTotalSize"] = NumberUtil.internaltionalCounting(fileSize);
            item["fileSumCount"] = NumberUtil.internaltionalCounting(
                item["fileSumCount"]
            );
            item["inactiveFileSumCount"] = NumberUtil.internaltionalCounting(
                item["inactiveFileSumCount"]
            );
            item["inactiveFileTotalSize"] =
                NumberUtil.internaltionalCounting(inactiveSize);
            for (const inaciveRuleColumn of inativeRuleColumns) {
                item[inaciveRuleColumn.internalName] =
                    NumberUtil.internaltionalCounting(
                        CalculateUtil.GetConvertValue(
                            item[inaciveRuleColumn.internalName], 2
                        )
                    );
            }
            item["saving"] = NumberUtil.internaltionalCounting(
                (price * inactiveSize).toFixed(0)
            );
            if (fileSize === 0) {
                item["rate"] = "0%";
            } else {
                const temp =
                    Number.parseInt(
                        (inactiveSize / fileSize).toFixed(2).replace(".", "")
                    ) + "%";
                item["rate"] =
                    temp === "0%" && inactiveSize !== 0 ? "1%" : temp;
            }
        }

        return clonedItems;
    };

    static CalculateInactivesNodesDataV3 = async (items) => {
        let clonedItems = await this.CalculateInactivesNodesData(items);
        for (let item of clonedItems) {
            const phlTotalSize = CalculateUtil.GetConvertValue(item["phlTotalSize"], 2);
            item["phlTotalSize"] = NumberUtil.internaltionalCounting(phlTotalSize);
        }
        return clonedItems;
    };

    static CalculateRotSummaryNodeTotalAggregateInfo = async (item) => {
        const clonedItem = _.cloneDeep(item);
        clonedItem["rotFileTotalSize"] = _.isNil(clonedItem["rotFileTotalSize"])
            ? 0
            : NumberUtil.internaltionalCounting(
                  CalculateUtil.GetConvertValue(clonedItem["rotFileTotalSize"], 2)
              );
        return clonedItem;
    };

    static CalculateRotOptimizationNodeTotalAggregateInfo = async (item, customColumns = []) => {
        const clonedItem = _.cloneDeep(item);

        const savingInfo =
            await ConfigurationRequester.getCostSavingConfigurationInfo();
        const spPrice =
            Math.max(savingInfo.spStoragePrice - savingInfo.archivedDataStoragePrice, 0);
        const odPrice =
            Math.max(savingInfo.odStoragePrice - savingInfo.archivedDataStoragePrice, 0);

        const spRedundant = _.isNil(
            clonedItem[`redundant_${SourceFlag.SharePoint}`]
        )
            ? 0
            : CalculateUtil.GetConvertValue(
                  clonedItem[`redundant_${SourceFlag.SharePoint}`]
              );
        const spObsolete = _.isNil(
            clonedItem[`obsolete_${SourceFlag.SharePoint}`]
        )
            ? 0
            : CalculateUtil.GetConvertValue(
                  clonedItem[`obsolete_${SourceFlag.SharePoint}`]
              );
        const spTrivial = _.isNil(
            clonedItem[`trivial_${SourceFlag.SharePoint}`]
        )
            ? 0
            : CalculateUtil.GetConvertValue(
                  clonedItem[`trivial_${SourceFlag.SharePoint}`]
              );
        const odRedundant = _.isNil(
            clonedItem[`redundant_${SourceFlag.OneDrive}`]
        )
            ? 0
            : CalculateUtil.GetConvertValue(
                  clonedItem[`redundant_${SourceFlag.OneDrive}`]
              );
        const odObsolete = _.isNil(
            clonedItem[`obsolete_${SourceFlag.OneDrive}`]
        )
            ? 0
            : CalculateUtil.GetConvertValue(
                  clonedItem[`obsolete_${SourceFlag.OneDrive}`]
              );
        const odTrivial = _.isNil(clonedItem[`trivial_${SourceFlag.OneDrive}`])
            ? 0
            : CalculateUtil.GetConvertValue(
                  clonedItem[`trivial_${SourceFlag.OneDrive}`]
              );
        const spRotFileTotalSize = spRedundant + spObsolete + spTrivial;
        const odRotFileTotalSize = odRedundant + odObsolete + odTrivial;

        const spFileTotalSize = _.isNil(
            clonedItem[`fileTotalSize_${SourceFlag.SharePoint}`]
        )
            ? 0
            : clonedItem[`fileTotalSize_${SourceFlag.SharePoint}`];
        const odFileTotalSize = _.isNil(
            clonedItem[`fileTotalSize_${SourceFlag.OneDrive}`]
        )
            ? 0
            : clonedItem[`fileTotalSize_${SourceFlag.OneDrive}`];
        const fileTotalSize = CalculateUtil.GetConvertValue(spFileTotalSize + odFileTotalSize);

        const spPriceSize = Math.min(
            CalculateUtil.GetConvertValue(spFileTotalSize) -
                CalculateUtil.GetConvertValue(
                    savingInfo.spFreeStorage * 1024 * 1024 * 1024
                ),
            spRotFileTotalSize
        );
        const odPriceSize = Math.min(
            CalculateUtil.GetConvertValue(odFileTotalSize) -
                CalculateUtil.GetConvertValue(
                    savingInfo.odFreeStorage * 1024 * 1024 * 1024
                ),
            odRotFileTotalSize
        );

        const spSaving =
            (Math.max(savingInfo.spStoragePrice - savingInfo.archivedDataStoragePrice, 0)) *
            (spPriceSize > 0 ? spPriceSize : 0);
        const odSaving =
            (Math.max(savingInfo.odStoragePrice - savingInfo.archivedDataStoragePrice, 0)) *
            (odPriceSize > 0 ? odPriceSize : 0);

        clonedItem["inScope"] = NumberUtil.internaltionalCounting(
            clonedItem["inScope"]
        );
        clonedItem["fileTotalSize"] =
            NumberUtil.internaltionalCounting(fileTotalSize);
        clonedItem["redundant"] = NumberUtil.internaltionalCounting(
            spRedundant + odRedundant
        );
        clonedItem["obsolete"] = NumberUtil.internaltionalCounting(
            spObsolete + odObsolete
        );
        clonedItem["trivial"] = NumberUtil.internaltionalCounting(
            spTrivial + odTrivial
        );
        clonedItem["rotFileTotalSize"] = NumberUtil.internaltionalCounting(
            spRotFileTotalSize + odRotFileTotalSize
        );
        clonedItem["rSaving"] = NumberUtil.internaltionalCounting(
            (spPrice * spRedundant + odPrice * odRedundant).toFixed(0)
        );
        clonedItem["oSaving"] = NumberUtil.internaltionalCounting(
            (spPrice * spObsolete + odPrice * odObsolete).toFixed(0)
        );
        clonedItem["tSaving"] = NumberUtil.internaltionalCounting(
            (spPrice * spTrivial + odPrice * odTrivial).toFixed(0)
        );
        clonedItem["rotSaving"] = NumberUtil.internaltionalCounting(
            (spSaving + odSaving).toFixed(0)
        );
        if (fileTotalSize === 0) {
            clonedItem["rate"] = "0%";
        } else {
            const temp =
                Number.parseInt(
                    ((spRotFileTotalSize + odRotFileTotalSize) / fileTotalSize)
                        .toFixed(2)
                        .replace(".", "")
                ) + "%";
            clonedItem["rate"] =
                temp === "0%" && spRotFileTotalSize + odRotFileTotalSize !== 0
                    ? "1%"
                    : temp;
        }

        for(let customColumn of customColumns){
            clonedItem[customColumn.internalName] =  NumberUtil.internaltionalCounting(CalculateUtil.GetConvertValue(clonedItem[customColumn.internalName]));
        }

        return clonedItem;
    };

    static CalculateRotOptimizationNodeTotalAggregateInfoV3 = async (item, customColumns = []) => {
        const clonedItem = _.cloneDeep(item);

        const savingInfo =
            await ConfigurationRequester.getCostSavingConfigurationInfo();
        const spPrice =
            Math.max(savingInfo.spStoragePrice - savingInfo.archivedDataStoragePrice, 0);
        const odPrice =
            Math.max(savingInfo.odStoragePrice - savingInfo.archivedDataStoragePrice, 0);

        const spRedundant = _.isNil(
            clonedItem[`redundant_${SourceFlag.SharePoint}`]
        )
            ? 0
            : CalculateUtil.GetConvertValue(
                  clonedItem[`redundant_${SourceFlag.SharePoint}`],
                  2
              );
        const spObsolete = _.isNil(
            clonedItem[`obsolete_${SourceFlag.SharePoint}`]
        )
            ? 0
            : CalculateUtil.GetConvertValue(
                  clonedItem[`obsolete_${SourceFlag.SharePoint}`],
                  2
              );
        const spTrivial = _.isNil(
            clonedItem[`trivial_${SourceFlag.SharePoint}`]
        )
            ? 0
            : CalculateUtil.GetConvertValue(
                  clonedItem[`trivial_${SourceFlag.SharePoint}`],
                  2
              );
        const odRedundant = _.isNil(
            clonedItem[`redundant_${SourceFlag.OneDrive}`]
        )
            ? 0
            : CalculateUtil.GetConvertValue(
                  clonedItem[`redundant_${SourceFlag.OneDrive}`],
                  2
              );
        const odObsolete = _.isNil(
            clonedItem[`obsolete_${SourceFlag.OneDrive}`]
        )
            ? 0
            : CalculateUtil.GetConvertValue(
                  clonedItem[`obsolete_${SourceFlag.OneDrive}`],
                  2
              );
        const odTrivial = _.isNil(clonedItem[`trivial_${SourceFlag.OneDrive}`])
            ? 0
            : CalculateUtil.GetConvertValue(
                  clonedItem[`trivial_${SourceFlag.OneDrive}`],
                  2
              );
        const spRotFileTotalSize = CalculateUtil.GetConvertValue(
            clonedItem[`rotFileTotalSize_${SourceFlag.SharePoint}`],
            2
        );
        const odRotFileTotalSize = CalculateUtil.GetConvertValue(
            clonedItem[`rotFileTotalSize_${SourceFlag.OneDrive}`],
            2
        );

        const spFileTotalSize = _.isNil(
            clonedItem[`fileTotalSize_${SourceFlag.SharePoint}`]
        )
            ? 0
            : clonedItem[`fileTotalSize_${SourceFlag.SharePoint}`];
        const odFileTotalSize = _.isNil(
            clonedItem[`fileTotalSize_${SourceFlag.OneDrive}`]
        )
            ? 0
            : clonedItem[`fileTotalSize_${SourceFlag.OneDrive}`];
        const fileTotalSize = CalculateUtil.GetConvertValue(spFileTotalSize + odFileTotalSize, 2);

        const spPriceSize = Math.min(
            CalculateUtil.GetConvertValue(spFileTotalSize) -
                CalculateUtil.GetConvertValue(
                    savingInfo.spFreeStorage * 1024 * 1024 * 1024
                ),
            spRotFileTotalSize
        );
        const odPriceSize = Math.min(
            CalculateUtil.GetConvertValue(odFileTotalSize) -
                CalculateUtil.GetConvertValue(
                    savingInfo.odFreeStorage * 1024 * 1024 * 1024
                ),
            odRotFileTotalSize
        );

        const spSaving =
            (Math.max(savingInfo.spStoragePrice - savingInfo.archivedDataStoragePrice, 0)) *
            (spPriceSize > 0 ? spPriceSize : 0);
        const odSaving =
            (Math.max(savingInfo.odStoragePrice - savingInfo.archivedDataStoragePrice, 0)) *
            (odPriceSize > 0 ? odPriceSize : 0);

        clonedItem["inScope"] = NumberUtil.internaltionalCounting(
            clonedItem["inScope"]
        );
        clonedItem["fileTotalSize"] =
            NumberUtil.internaltionalCounting(fileTotalSize);
        clonedItem["rCategoryFileTotalSize"] = NumberUtil.internaltionalCounting(
            spRedundant + odRedundant
        );
        clonedItem["oCategoryFileTotalSize"] = NumberUtil.internaltionalCounting(
            spObsolete + odObsolete
        );
        clonedItem["tCategoryFileTotalSize"] = NumberUtil.internaltionalCounting(
            spTrivial + odTrivial
        );
        clonedItem["rotFileTotalSize"] = NumberUtil.internaltionalCounting(
            spRotFileTotalSize + odRotFileTotalSize
        );
        clonedItem["rSaving"] = NumberUtil.internaltionalCounting(
            (spPrice * spRedundant + odPrice * odRedundant).toFixed(0)
        );
        clonedItem["oSaving"] = NumberUtil.internaltionalCounting(
            (spPrice * spObsolete + odPrice * odObsolete).toFixed(0)
        );
        clonedItem["tSaving"] = NumberUtil.internaltionalCounting(
            (spPrice * spTrivial + odPrice * odTrivial).toFixed(0)
        );
        clonedItem["rotSaving"] = NumberUtil.internaltionalCounting(
            (spSaving + odSaving).toFixed(0)
        );
        if (fileTotalSize === 0) {
            clonedItem["rate"] = "0%";
        } else {
            const temp =
                Number.parseInt(
                    ((spRotFileTotalSize + odRotFileTotalSize) / fileTotalSize)
                        .toFixed(2)
                        .replace(".", "")
                ) + "%";
            clonedItem["rate"] =
                temp === "0%" && spRotFileTotalSize + odRotFileTotalSize !== 0
                    ? "1%"
                    : temp;
        }

        for(let customColumn of customColumns){
            clonedItem[customColumn.internalName] =  NumberUtil.internaltionalCounting(CalculateUtil.GetConvertValue(clonedItem[customColumn.internalName]));
        }

        return clonedItem;
    };

    static CalculateRotOptimizationNodesData = async (items, customColumns = []) => {
        const clonedItems = _.cloneDeep(items);

        const savingInfo =
            await ConfigurationRequester.getCostSavingConfigurationInfo();

        for (let item of clonedItems) {
            const price =
            Math.max(
                item.contentSource === SourceFlag.SharePoint
                    ? savingInfo.spStoragePrice -
                      savingInfo.archivedDataStoragePrice
                    : savingInfo.odStoragePrice -
                      savingInfo.archivedDataStoragePrice, 0);

            const fileTotalSize = CalculateUtil.GetConvertValue(
                item["fileTotalSize"]
            );
            const redundant = CalculateUtil.GetConvertValue(item["redundant"]);
            const obsolete = CalculateUtil.GetConvertValue(item["obsolete"]);
            const trivial = CalculateUtil.GetConvertValue(item["trivial"]);
            const rotFileTotalSize = redundant + obsolete + trivial;

            item["fileTotalSize"] =
                NumberUtil.internaltionalCounting(fileTotalSize);
            item["redundant"] = NumberUtil.internaltionalCounting(redundant);
            item["obsolete"] = NumberUtil.internaltionalCounting(obsolete);
            item["trivial"] = NumberUtil.internaltionalCounting(trivial);
            item["rotFileTotalSize"] =
                NumberUtil.internaltionalCounting(rotFileTotalSize);
            item["rSaving"] = NumberUtil.internaltionalCounting(
                (price * redundant).toFixed(0)
            );
            item["oSaving"] = NumberUtil.internaltionalCounting(
                (price * obsolete).toFixed(0)
            );
            item["tSaving"] = NumberUtil.internaltionalCounting(
                (price * trivial).toFixed(0)
            );
            item["rotSaving"] = NumberUtil.internaltionalCounting(
                (price * rotFileTotalSize).toFixed(0)
            );
            if (fileTotalSize === 0) {
                item["rate"] = "0%";
            } else {
                const temp =
                    Number.parseInt(
                        (rotFileTotalSize / fileTotalSize)
                            .toFixed(2)
                            .replace(".", "")
                    ) + "%";
                item["rate"] =
                    temp === "0%" && rotFileTotalSize !== 0 ? "1%" : temp;
            }

            for(let customColumn of customColumns){
                item[customColumn.internalName] =  NumberUtil.internaltionalCounting(CalculateUtil.GetConvertValue(item[customColumn.internalName]));
            }
        }

        return clonedItems;
    };

    static CalculateRotOptimizationNodesDataV3 = async (items, customColumns = []) => {
        const clonedItems = _.cloneDeep(items);

        const savingInfo =
            await ConfigurationRequester.getCostSavingConfigurationInfo();

        for (let item of clonedItems) {
            const price =
            Math.max(
                item.contentSource === SourceFlag.SharePoint
                    ? savingInfo.spStoragePrice -
                      savingInfo.archivedDataStoragePrice
                    : savingInfo.odStoragePrice -
                      savingInfo.archivedDataStoragePrice, 0);

            const fileTotalSize = CalculateUtil.GetConvertValue(
                item["fileTotalSize"], 2
            );
            const redundant = CalculateUtil.GetConvertValue(item["rCategoryFileTotalSize"], 2);
            const obsolete = CalculateUtil.GetConvertValue(item["oCategoryFileTotalSize"], 2);
            const trivial = CalculateUtil.GetConvertValue(item["tCategoryFileTotalSize"], 2);
            const rotFileTotalSize = CalculateUtil.GetConvertValue(item["rotFileTotalSize"], 2);

            item["fileTotalSize"] =
                NumberUtil.internaltionalCounting(fileTotalSize);
            item["rCategoryFileTotalSize"] = NumberUtil.internaltionalCounting(redundant);
            item["oCategoryFileTotalSize"] = NumberUtil.internaltionalCounting(obsolete);
            item["tCategoryFileTotalSize"] = NumberUtil.internaltionalCounting(trivial);
            item["rotFileTotalSize"] =
                NumberUtil.internaltionalCounting(rotFileTotalSize);
            item["rSaving"] = NumberUtil.internaltionalCounting(
                (price * redundant).toFixed(0)
            );
            item["oSaving"] = NumberUtil.internaltionalCounting(
                (price * obsolete).toFixed(0)
            );
            item["tSaving"] = NumberUtil.internaltionalCounting(
                (price * trivial).toFixed(0)
            );
            item["rotSaving"] = NumberUtil.internaltionalCounting(
                (price * rotFileTotalSize).toFixed(0)
            );
            if (fileTotalSize === 0) {
                item["rate"] = "0%";
            } else {
                const temp =
                    Number.parseInt(
                        (rotFileTotalSize / fileTotalSize)
                            .toFixed(2)
                            .replace(".", "")
                    ) + "%";
                item["rate"] =
                    temp === "0%" && rotFileTotalSize !== 0 ? "1%" : temp;
            }

            for(let customColumn of customColumns){
                item[customColumn.internalName] =  NumberUtil.internaltionalCounting(CalculateUtil.GetConvertValue(item[customColumn.internalName], 2));
            }
        }

        return clonedItems;
    };

    static CalculateRotSummaryNodesData = async (items) => {
        const clonedItems = _.cloneDeep(items);

        for (let item of clonedItems) {
            item["name"] = AutoFitDriverName(item["name"]);
            item["rotFileTotalSize"] = NumberUtil.internaltionalCounting(
                CalculateUtil.GetConvertValue(item["rotFileTotalSize"], 2)
            );
        }

        return clonedItems;
    };

    static CalculateRotTreeRuleData = async (treeRuleInfo) => {
        treeRuleInfo.label +=
            ": " +
            NumberUtil.internaltionalCounting(CalculateUtil.GetConvertValue(treeRuleInfo.fileTotalSize, 2)) +
            " GB";
        if (!_.isNil(treeRuleInfo.children)) {
            treeRuleInfo.children.forEach((category) => {
                category.label +=
                    ": " +
                    NumberUtil.internaltionalCounting(CalculateUtil.GetConvertValue(category.fileTotalSize, 2)) +
                    " GB";
                if (!_.isNil(category.children)) {
                    category.children.forEach((rule) => {
                        rule.label +=
                            ": " +
                            NumberUtil.internaltionalCounting(CalculateUtil.GetConvertValue(rule.fileTotalSize, 2)) +
                            " GB";
                    });
                }
            });
        }
        return treeRuleInfo;
    };

    static CalculateGoogleInactivesNodeTotalAggregateInfo = async (item) => {
        const clonedItem = _.cloneDeep(item);
        const inativeRuleColumns =
            await GoogleDriveBasicDataRequester.getInactiveTableColumns();

        const fileTotalSize = _.isNil(clonedItem["fileTotalSize"]) ? 0 : clonedItem["fileTotalSize"];
        const inactiveFileTotalSize = _.isNil(clonedItem["inactiveFileTotalSize"]) ? 0 : clonedItem["inactiveFileTotalSize"];

        clonedItem["driveCount"] = NumberUtil.internaltionalCounting(
            clonedItem["driveCount"]
        );
        clonedItem["fileTotalSize"] = NumberUtil.internaltionalCounting(
            CalculateUtil.GetConvertValue(fileTotalSize, 2)
        );
        clonedItem["fileSumCount"] = NumberUtil.internaltionalCounting(
            clonedItem["fileSumCount"]
        );
        clonedItem["inactiveFileSumCount"] = NumberUtil.internaltionalCounting(
            clonedItem["inactiveFileSumCount"]
        );
        clonedItem["inactiveFileTotalSize"] = NumberUtil.internaltionalCounting(
            CalculateUtil.GetConvertValue(inactiveFileTotalSize, 2)
        );
        for (const inaciveRuleColumn of inativeRuleColumns) {
            clonedItem[inaciveRuleColumn.internalName] = _.isNil(
                item[inaciveRuleColumn.internalName]
            )
                ? 0
                : NumberUtil.internaltionalCounting(
                      CalculateUtil.GetConvertValue(
                          clonedItem[inaciveRuleColumn.internalName]
                      )
                  );
        }

        if (fileTotalSize === 0) {
            clonedItem["rate"] = "0%";
        } else {
            const temp =
                Number.parseInt(
                    (
                        CalculateUtil.GetConvertValue(inactiveFileTotalSize) /
                        CalculateUtil.GetConvertValue(fileTotalSize)
                    )
                        .toFixed(2)
                        .replace(".", "")
                ) + "%";
            clonedItem["rate"] =
                temp === "0%" && inactiveFileTotalSize !== 0
                    ? "1%"
                    : temp;
        }
        return clonedItem;
    };

    static CalculateGoogleInactivesNodesData = async (items) => {
        const clonedItems = _.cloneDeep(items);
        const inativeRuleColumns =
            await GoogleDriveBasicDataRequester.getInactiveTableColumns();

        for (let item of clonedItems) {

            const inactiveSize = CalculateUtil.GetConvertValue(
                item["inactiveFileTotalSize"], 2
            );
            const fileSize = CalculateUtil.GetConvertValue(item["fileTotalSize"], 2);

            item["name"] = AutoFitDriverName(item["name"]);
            item["driveCount"] = NumberUtil.internaltionalCounting(
                item["driveCount"]
            );
            item["fileTotalSize"] = NumberUtil.internaltionalCounting(fileSize);
            item["fileSumCount"] = NumberUtil.internaltionalCounting(
                item["fileSumCount"]
            );
            item["inactiveFileSumCount"] = NumberUtil.internaltionalCounting(
                item["inactiveFileSumCount"]
            );
            item["inactiveFileTotalSize"] =
                NumberUtil.internaltionalCounting(inactiveSize);
            for (const inaciveRuleColumn of inativeRuleColumns) {
                item[inaciveRuleColumn.internalName] =
                    NumberUtil.internaltionalCounting(
                        CalculateUtil.GetConvertValue(
                            item[inaciveRuleColumn.internalName]
                        )
                    );
            }
            if (fileSize === 0) {
                item["rate"] = "0%";
            } else {
                const temp =
                    Number.parseInt(
                        (inactiveSize / fileSize).toFixed(2).replace(".", "")
                    ) + "%";
                item["rate"] =
                    temp === "0%" && inactiveSize !== 0 ? "1%" : temp;
            }
        }

        return clonedItems;
    };

    static CalculateGoogleRotOptimizationNodesDataV3 = async (items, customColumns = []) => {
        const clonedItems = _.cloneDeep(items);

        for (let item of clonedItems) {
            const fileTotalSize = CalculateUtil.GetConvertValue(item["fileTotalSize"], 2);
            const redundant = CalculateUtil.GetConvertValue(item["rCategoryFileTotalSize"], 2);
            const obsolete = CalculateUtil.GetConvertValue(item["oCategoryFileTotalSize"], 2);
            const trivial = CalculateUtil.GetConvertValue(item["tCategoryFileTotalSize"], 2);
            const rotFileTotalSize = CalculateUtil.GetConvertValue(item["rotFileTotalSize"], 2);

            item["fileTotalSize"] = NumberUtil.internaltionalCounting(fileTotalSize);
            item["rCategoryFileTotalSize"] = NumberUtil.internaltionalCounting(redundant);
            item["oCategoryFileTotalSize"] = NumberUtil.internaltionalCounting(obsolete);
            item["tCategoryFileTotalSize"] = NumberUtil.internaltionalCounting(trivial);
            item["rotFileTotalSize"] = NumberUtil.internaltionalCounting(rotFileTotalSize);

            if (fileTotalSize === 0) {
                item["rate"] = "0%";
            } else {
                const temp = Number.parseInt((rotFileTotalSize / fileTotalSize).toFixed(2).replace(".", "")) + "%";
                item["rate"] = temp === "0%" && rotFileTotalSize !== 0 ? "1%" : temp;
            }

            for(let customColumn of customColumns){
                item[customColumn.internalName] =  NumberUtil.internaltionalCounting(CalculateUtil.GetConvertValue(item[customColumn.internalName], 2));
            }
        }

        return clonedItems;
    };

    static CalculateGoogleRotOptimizationNodeTotalAggregateInfoV3 = async (item, customColumns = []) => {
        const clonedItem = _.cloneDeep(item);

        const googleRedundant = CalculateUtil.GetConvertValue(clonedItem['redundant'], 2);
        const googleObsolete = CalculateUtil.GetConvertValue(clonedItem['obsolete'], 2);
        const googleTrivial = CalculateUtil.GetConvertValue(clonedItem['trivial'], 2);
        const googleRotFileTotalSize = CalculateUtil.GetConvertValue(clonedItem['rotFileTotalSize'], 2);
        const fileTotalSize = CalculateUtil.GetConvertValue(clonedItem['fileTotalSize'], 2);

        clonedItem["inScope"] = NumberUtil.internaltionalCounting(clonedItem["inScope"]);
        clonedItem["fileTotalSize"] = NumberUtil.internaltionalCounting(fileTotalSize);
        clonedItem["rCategoryFileTotalSize"] = NumberUtil.internaltionalCounting(googleRedundant);
        clonedItem["oCategoryFileTotalSize"] = NumberUtil.internaltionalCounting(googleObsolete);
        clonedItem["tCategoryFileTotalSize"] = NumberUtil.internaltionalCounting(googleTrivial);
        clonedItem["rotFileTotalSize"] = NumberUtil.internaltionalCounting(googleRotFileTotalSize);

        if (fileTotalSize === 0) {
            clonedItem["rate"] = "0%";
        } else {
            const temp = Number.parseInt((googleRotFileTotalSize / fileTotalSize).toFixed(2).replace(".", "")) + "%";
            clonedItem["rate"] = temp === "0%" && googleRotFileTotalSize !== 0 ? "1%" : temp;
        }

        for(let customColumn of customColumns){
            clonedItem[customColumn.internalName] =  NumberUtil.internaltionalCounting(CalculateUtil.GetConvertValue(clonedItem[customColumn.internalName], 2));
        }

        return clonedItem;
    };

    // File system
    static CalculateFileSystemInactivesNodeTotalAggregateInfo = async (item) => {
        const clonedItem = _.cloneDeep(item);
        const inativeRuleColumns =
            await FileSystemBasicDataRequester.getInactiveTableColumns();

        const fileTotalSize = _.isNil(clonedItem["fileTotalSize"]) ? 0 : clonedItem["fileTotalSize"];
        const inactiveFileTotalSize = _.isNil(clonedItem["inactiveFileTotalSize"]) ? 0 : clonedItem["inactiveFileTotalSize"];

        clonedItem["connectionCount"] = NumberUtil.internaltionalCounting(
            clonedItem["connectionCount"]
        );
        clonedItem["fileTotalSize"] = NumberUtil.internaltionalCounting(
            CalculateUtil.GetConvertValue(fileTotalSize, 2)
        );
        clonedItem["fileSumCount"] = NumberUtil.internaltionalCounting(
            clonedItem["fileSumCount"]
        );
        clonedItem["inactiveFileSumCount"] = NumberUtil.internaltionalCounting(
            clonedItem["inactiveFileSumCount"]
        );
        clonedItem["inactiveFileTotalSize"] = NumberUtil.internaltionalCounting(
            CalculateUtil.GetConvertValue(inactiveFileTotalSize, 2)
        );
        for (const inaciveRuleColumn of inativeRuleColumns) {
            clonedItem[inaciveRuleColumn.internalName] = _.isNil(
                item[inaciveRuleColumn.internalName]
            )
                ? 0
                : NumberUtil.internaltionalCounting(
                      CalculateUtil.GetConvertValue(
                          clonedItem[inaciveRuleColumn.internalName]
                      )
                  );
        }

        if (fileTotalSize === 0) {
            clonedItem["rate"] = "0%";
        } else {
            const temp =
                Number.parseInt(
                    (
                        CalculateUtil.GetConvertValue(inactiveFileTotalSize) /
                        CalculateUtil.GetConvertValue(fileTotalSize)
                    )
                        .toFixed(2)
                        .replace(".", "")
                ) + "%";
            clonedItem["rate"] =
                temp === "0%" && inactiveFileTotalSize !== 0
                    ? "1%"
                    : temp;
        }
        return clonedItem;
    };

    static CalculateFileSystemInactivesNodesData = async (items) => {
        const clonedItems = _.cloneDeep(items);
        const inativeRuleColumns =
            await FileSystemBasicDataRequester.getInactiveTableColumns();

        for (let item of clonedItems) {

            const inactiveSize = CalculateUtil.GetConvertValue(
                item["inactiveFileTotalSize"], 2
            );
            const fileSize = CalculateUtil.GetConvertValue(item["fileTotalSize"], 2);

            item["connectionCount"] = NumberUtil.internaltionalCounting(
                item["connectionCount"]
            );
            item["fileTotalSize"] = NumberUtil.internaltionalCounting(fileSize);
            item["fileSumCount"] = NumberUtil.internaltionalCounting(
                item["fileSumCount"]
            );
            item["inactiveFileSumCount"] = NumberUtil.internaltionalCounting(
                item["inactiveFileSumCount"]
            );
            item["inactiveFileTotalSize"] =
                NumberUtil.internaltionalCounting(inactiveSize);
            for (const inaciveRuleColumn of inativeRuleColumns) {
                item[inaciveRuleColumn.internalName] =
                    NumberUtil.internaltionalCounting(
                        CalculateUtil.GetConvertValue(
                            item[inaciveRuleColumn.internalName]
                        )
                    );
            }
            if (fileSize === 0) {
                item["rate"] = "0%";
            } else {
                const temp =
                    Number.parseInt(
                        (inactiveSize / fileSize).toFixed(2).replace(".", "")
                    ) + "%";
                item["rate"] =
                    temp === "0%" && inactiveSize !== 0 ? "1%" : temp;
            }
        }

        return clonedItems;
    };

    static CalculateFileSystemRotOptimizationNodesDataV3 = async (items, customColumns = []) => {
        const clonedItems = _.cloneDeep(items);

        for (let item of clonedItems) {
            const fileTotalSize = CalculateUtil.GetConvertValue(item["fileTotalSize"]);
            const redundant = CalculateUtil.GetConvertValue(item["rCategoryFileTotalSize"]);
            const obsolete = CalculateUtil.GetConvertValue(item["oCategoryFileTotalSize"]);
            const trivial = CalculateUtil.GetConvertValue(item["tCategoryFileTotalSize"]);
            const rotFileTotalSize = CalculateUtil.GetConvertValue(item["rotFileTotalSize"]);

            item["fileTotalSize"] = NumberUtil.internaltionalCounting(fileTotalSize);
            item["rCategoryFileTotalSize"] = NumberUtil.internaltionalCounting(redundant);
            item["oCategoryFileTotalSize"] = NumberUtil.internaltionalCounting(obsolete);
            item["tCategoryFileTotalSize"] = NumberUtil.internaltionalCounting(trivial);
            item["rotFileTotalSize"] = NumberUtil.internaltionalCounting(rotFileTotalSize);

            if (fileTotalSize === 0) {
                item["rate"] = "0%";
            } else {
                const temp = Number.parseInt((rotFileTotalSize / fileTotalSize).toFixed(2).replace(".", "")) + "%";
                item["rate"] = temp === "0%" && rotFileTotalSize !== 0 ? "1%" : temp;
            }

            for(let customColumn of customColumns){
                item[customColumn.internalName] =  NumberUtil.internaltionalCounting(CalculateUtil.GetConvertValue(item[customColumn.internalName]));
            }
        }

        return clonedItems;
    };

    static CalculateFileSystemRotOptimizationNodeTotalAggregateInfoV3 = async (item, customColumns = []) => {
        const clonedItem = _.cloneDeep(item);

        const fileSystemRedundant = CalculateUtil.GetConvertValue(clonedItem['redundant']);
        const fileSystemObsolete = CalculateUtil.GetConvertValue(clonedItem['obsolete']);
        const fileSystemTrivial = CalculateUtil.GetConvertValue(clonedItem['trivial']);
        const fileSystemRotFileTotalSize = CalculateUtil.GetConvertValue(clonedItem['rotFileTotalSize']);
        const fileTotalSize = CalculateUtil.GetConvertValue(clonedItem['fileTotalSize']);

        clonedItem["inScope"] = NumberUtil.internaltionalCounting(clonedItem["inScope"]);
        clonedItem["fileTotalSize"] = NumberUtil.internaltionalCounting(fileTotalSize);
        clonedItem["rCategoryFileTotalSize"] = NumberUtil.internaltionalCounting(fileSystemRedundant);
        clonedItem["oCategoryFileTotalSize"] = NumberUtil.internaltionalCounting(fileSystemObsolete);
        clonedItem["tCategoryFileTotalSize"] = NumberUtil.internaltionalCounting(fileSystemTrivial);
        clonedItem["rotFileTotalSize"] = NumberUtil.internaltionalCounting(fileSystemRotFileTotalSize);

        if (fileTotalSize === 0) {
            clonedItem["rate"] = "0%";
        } else {
            const temp = Number.parseInt((fileSystemRotFileTotalSize / fileTotalSize).toFixed(2).replace(".", "")) + "%";
            clonedItem["rate"] = temp === "0%" && fileSystemRotFileTotalSize !== 0 ? "1%" : temp;
        }

        for(let customColumn of customColumns){
            clonedItem[customColumn.internalName] =  NumberUtil.internaltionalCounting(CalculateUtil.GetConvertValue(clonedItem[customColumn.internalName]));
        }

        return clonedItem;
    };

    static GetConvertValue = (value, decimalPlaces = 2) => {
        //RECO-41257: Support 2 decimal places for new logic account and account has discovery license only
        const isSupportTwoDecimalPlaces = LicenseHelper.HasDiscoveryLicenseOnly() || LicenseHelper.EnableRecordsArchiver();
        
        if (isSupportTwoDecimalPlaces) {
            const result = UnitConvertsionUtil.DecimalConvert(value, decimalPlaces);
            //RECO-40517: add 1 byte to the result to avoid the case where the result is exactly equal to a whole number and loses its decimal places
            const oneBytePerGB = 1 / Math.pow(1024, 3);
            return result > 0 ? result + oneBytePerGB : result;
        }
        return UnitConvertsionUtil.Convert(value);
    };
}

export default CalculateUtil;
