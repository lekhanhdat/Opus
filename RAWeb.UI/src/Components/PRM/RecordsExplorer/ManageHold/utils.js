export const convertPeoplePickerData = (users) => {
    return users.map((user) => ({
        UserId: user.UserId,
        UserName: user.UserName,
        UserPrincipalName: user.UserPrincipalName,
        Email: user.Email,
        DisplayName: user.DisplayName,
        InviteType: user.InviteType,
        RMUserId: user.RMUserId,
        Id: user.Id,
        SurName: user.SurName,
        GivenName: user.GivenName,
        TenantId: user.TenantId,
    }));
};
