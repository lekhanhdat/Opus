
const EXPANDER_ITEMS = [
    {
        key: 'sharePointStorage',
        title: RMResx.RM_JS_DSB_ConfigureStoragePrice_SPOStorage_Title,
        children: [
            {
                key: 'sharePointLivePrice',
                label: RMResx.RM_JS_DSB_ConfigureStoragePrice_SPOStorage_Label,
                description: RMResx.RM_JS_DSB_ConfigureStoragePrice_SPOStorage_Desc,
            }
        ]
    },
    {
        key: 'oneDriveStorage',
        title: RMResx.RM_JS_DSB_ConfigureStoragePrice_ODStorage_Title,
        children: [
            {
                key: 'oneDriveLivePrice',
                label: RMResx.RM_JS_DSB_ConfigureStoragePrice_ODStorage_Label,
                description: RMResx.RM_JS_DSB_ConfigureStoragePrice_ODStorage_Desc,
            }
        ]
    },
    {
        key: 'archivalStorage',
        title: RMResx.RM_JS_DSB_ConfigureStoragePrice_ArchivalStorage_Title,
        children: [
            {
                key: 'sharePointArchivePrice',
                label: RMResx.RM_JS_DSB_ConfigureStoragePrice_ArchivalStorage_SPO,
            },
            {
                key: 'oneDriveArchivePrice',
                label: RMResx.RM_JS_DSB_ConfigureStoragePrice_ArchivalStorage_OD,
            }
        ]
    }
]

export const ConfigureStoragePrice = (props) => {
    const { value: formValues, onChange } = props;

    return (
        <div className="flex-column gap-s">
            {EXPANDER_ITEMS.map((expander) => (
                <R.Expander key={expander.key} title={expander.title} level={2} status={{ show: true }}>
                    <div className="flex-column gap-l">
                        {expander.children.map((child) => (
                            <div key={child.key} className="flex-column">
                                <div className="flex-row align-center">
                                    <div tabIndex={0} className="strong require margin-bottom-xs">
                                        {child.label}
                                    </div>
                                    {child.description && <$g.Popover>{child.description}</$g.Popover>}
                                </div>
                                <R.Validation element="Input" require={RMResx.RM_JS_DSB_ConfigureStoragePrice_ValidationMsg} value={formValues[child.key]}>
                                    <div className="flex-row align-center gap-s">
                                        <R.Input
                                            id={`ra${child.key}`}
                                            type="number"
                                            float={4}
                                            min={0}
                                            value={formValues[child.key]}
                                            onChange={(value) => onChange(child.key, value)}
                                        />
                                        <div className='text-nowrap' tabIndex={0}>
                                            {RMResx.RM_JS_DSB_Unit_GBPerMonth}
                                        </div>
                                    </div>
                                </R.Validation>
                            </div>
                        ))}
                    </div>
                </R.Expander>
            ))}
        </div>
    )
}