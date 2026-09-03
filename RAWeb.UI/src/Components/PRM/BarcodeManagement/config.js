const RAMessageType = {
    Successful: 0,
    Failed: 1,
    Exception: 2,
};

const BarcodeTemplateLabelType = {
    None: -1, // No label type specified
    Label_200x93: 0, // 3" x 1" label
    Label_135x95: 1, // 2" x 2" label
    Label_95x65: 2, // 4" x 2" label 
    Label_99x67: 3, // 4" x 2-5/8" label
    Label_72x63: 4, // 4" x 3" label
};

const DefaultLabelSizeList = [
    {
        name: "200.7 x 93.1mm",
        value: BarcodeTemplateLabelType.Label_200x93,
        imgUrl: RM.gData.resCdnURL + "/cloud%20records/labels-200.7x93.1mm.svg",
        checked: true,
    },
    {
        name: "135 x 95mm",
        value: BarcodeTemplateLabelType.Label_135x95,
        imgUrl: RM.gData.resCdnURL + "/cloud%20records/labels-135x95mm.svg",
        checked: false,
    },
    {
        name: "95.5 x 65mm",
        value: BarcodeTemplateLabelType.Label_95x65,
        imgUrl: RM.gData.resCdnURL + "/cloud%20records/labels-95.5x65mm.svg",
        checked: false,
    },
    {
        name: "99.1 x 67.7mm",
        value: BarcodeTemplateLabelType.Label_99x67,
        imgUrl: RM.gData.resCdnURL + "/cloud%20records/labels-99.1x67.7mm.svg",
        checked: false,
    },
    {
        name: "72 x 63.5mm",
        value: BarcodeTemplateLabelType.Label_72x63,
        imgUrl: RM.gData.resCdnURL + "/cloud%20records/labels-72x63.5mm.svg",
        checked: false,
    },
];

const DefaultSizeList = [
    {
        name: RMResx.RM_PRM_TM_Barcode_Template_SizeUnit.format("10"),
        value: 10,
        checked: true,
    },
    {
        name: RMResx.RM_PRM_TM_Barcode_Template_SizeUnit.format("11"),
        value: 11,
        checked: false,
    },
    {
        name: RMResx.RM_PRM_TM_Barcode_Template_SizeUnit.format("12"),
        value: 12,
        checked: false,
    },
    {
        name: RMResx.RM_PRM_TM_Barcode_Template_SizeUnit.format("16"),
        value: 16,
        checked: false,
    },
    {
        name: RMResx.RM_PRM_TM_Barcode_Template_SizeUnit.format("18"),
        value: 18,
        checked: false,
    },
    {
        name: RMResx.RM_PRM_TM_Barcode_Template_SizeUnit.format("20"),
        value: 20,
        checked: false,
    },
    {
        name: RMResx.RM_PRM_TM_Barcode_Template_SizeUnit.format("24"),
        value: 24,
        checked: false,
    },
    {
        name: RMResx.RM_PRM_TM_Barcode_Template_SizeUnit.format("28"),
        value: 28,
        checked: false,
    },
    {
        name: RMResx.RM_PRM_TM_Barcode_Template_SizeUnit.format("36"),
        value: 36,
        checked: false,
    },
];

const BarcodeTemplatePosition = {
    Above: 0,
    Under: 1,
};

const DefaultAreaList = [
    {
        name: RMResx.RM_PRM_TM_Barcode_Template_LogoOrIconArea_Above,
        value: BarcodeTemplatePosition.Above,
        checked: true,
    },
    {
        name: RMResx.RM_PRM_TM_Barcode_Template_LogoOrIconArea_Under,
        value: BarcodeTemplatePosition.Under,
        checked: false,
    },
];

export {
    RAMessageType,
    DefaultLabelSizeList,
    DefaultSizeList,
    BarcodeTemplatePosition,
    DefaultAreaList,
    BarcodeTemplateLabelType,
};
