const ResponseErrorType = {
    //Common
    None: 0,
    Unknown: 1,
    NameExists: 2,
    Timeout: 3,
    ValidationError: 4,

    //BoxConnection 10~20
    ClientIdExists: 10,
    JsonFileInvalid: 11,
    EnterpriseIdExists: 12,
    AuthorizationCodeTimeout: 13,
};

const ActionMode = {
    Create: 0,
    Edit: 1
};

const AuthenticationType = {
    User: 0,
    Server: 1,
};

const TypeItems = [
    {
        name: RMResx.RM_Box_Register_Connection_Type_User,
        value: AuthenticationType.User,
        checked: true,
    },
    {
        name: RMResx.RM_Box_Register_Connection_Type_Server,
        value: AuthenticationType.Server,
        checked: true,
    },
];

export {
    ActionMode,
    AuthenticationType,
    TypeItems,
    ResponseErrorType,
};