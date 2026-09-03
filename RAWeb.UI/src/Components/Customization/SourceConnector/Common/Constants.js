export const ActionMode = {
    CREATE: 1,
    EDIT: 2,
};

export const CustomizeConnectorOrigin = {
    None: 0,
    BuildIn: 1,
    ExternalCustomize: 2,
};

export const CustomizeConnectorScope = {
    None: 0,
    Global: 1,
    Template: 2,
    Source: 3,
};

export const Constant = {
    ActionMode: "MODE",
    EditItem: "ID",
};

export const ColumnAction = {
    CreateOrEdit: 1,
    Delete: 2,
};

export const ColumnType = {
    SingleText: 1,
    MultipleText: 2,
    DateTime: 3,
    SingleChoice: 4,
    PeopleOrGroup: 5,
    Number: 6,
    MultipleChoice: 7,
    Taxonomy: 10,
    Identifier: 11,
};

export const ValidateMode = {
    None: 0,
    Static: 1,
    Request: 2,
};

export const ActionStatus = {
    Succeed: 1,
    Failed: 2,
    Repeat: 3,
    Illegal: 4,
};