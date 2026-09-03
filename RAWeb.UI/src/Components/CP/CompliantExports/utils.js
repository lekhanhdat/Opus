import {
    ConfiguratorContentSource,
    ConfiguratorFormatValue,
    FormatType,
} from "./Constants";
import { LicenseHelper } from "../../../Utilities/CommonUtil";

export const getConfigurationFormats = (selectedOption, hasUpgradeVEOV3) => {
    const values = [];

    // The order: VEO, NAA, NARA
    if (LicenseHelper.HasOpusGoogleLicense()) {
        values.push({
            text: RMResx.RM_ES_ExportType_NARA,
            value: ConfiguratorFormatValue.NARA,
            checked: selectedOption === ConfiguratorFormatValue.NARA,
        });
    }

    if (LicenseHelper.HasOpusILLicense() || LicenseHelper.HasOpusSOLicense()) {
        values.unshift({
            text: RMResx.RM_ES_ExportType_NAA,
            value: ConfiguratorFormatValue.NAA,
            checked: selectedOption === ConfiguratorFormatValue.NAA,
        });

        if (!values.find((item) => item.value === ConfiguratorFormatValue.NARA)) {
            values.push({
                text: RMResx.RM_ES_ExportType_NARA,
                value: ConfiguratorFormatValue.NARA,
                checked: selectedOption === ConfiguratorFormatValue.NARA,
            });
        }

        if (hasUpgradeVEOV3) {
            values.unshift({
                text: RMResx.RM_ES_ExportType_VEO,
                value: ConfiguratorFormatValue.VEO,
                checked: selectedOption === ConfiguratorFormatValue.VEO,
            });
        }
    }

    return values;
};

export const getConfigurationContentSources = (
    selectedOption,
    isNARAFormat
) => {
    const values = [];

    // The order: SPO_OD, EXO, Google
    if ((LicenseHelper.HasOpusILLicense() || LicenseHelper.HasOpusSOLicense())) {
        values.unshift(
            {
                text: RMResx.RM_ES_CompliantExport_ContentSource_SPO_OD,
                value: ConfiguratorContentSource.SPO_OD,
                checked: selectedOption === ConfiguratorContentSource.SPO_OD,
            },
        );
    }

    if (LicenseHelper.HasOpusILLicense()) {
        values.push({
            text: RMResx.RM_ES_CompliantExport_ContentSource_EXO,
            value: ConfiguratorContentSource.EXO,
            checked: selectedOption === ConfiguratorContentSource.EXO,
        });
    }

    if (isNARAFormat && LicenseHelper.HasOpusGoogleLicense()) {
        values.push({
            text: RMResx.RM_ES_CompliantExport_ContentSource_Google,
            value: ConfiguratorContentSource.Google,
            checked: selectedOption === ConfiguratorContentSource.Google,
        });
    }

    return values;
};

export const getFormatTypeList = (selectedOption) => {
    return [
        {
            name: RMResx.RM_ES_CompliantExport_Metadata_TypeDate,
            value: FormatType.Date,
            checked: selectedOption === FormatType.Date,
        },
        {
            name: RMResx.RM_ES_CompliantExport_Metadata_TypeString,
            value: FormatType.String,
            checked: selectedOption === FormatType.String,
        },
    ];
};
