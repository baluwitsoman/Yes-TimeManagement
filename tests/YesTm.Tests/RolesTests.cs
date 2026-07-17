using YesTm.Web.Common.Security;

namespace YesTm.Tests;

public class RolesTests
{
    [Theory]
    [InlineData("TimeManagement", Roles.User)]
    [InlineData("TM-Admin", Roles.Admin)]
    [InlineData("TM-SiteAdmin", Roles.SiteAdmin)]
    public void TryMapErpRole_MapsTimeManagementRoles(string erpRole, string expected)
    {
        Assert.True(Roles.TryMapErpRole(erpRole, out var tmRole));
        Assert.Equal(expected, tmRole);
    }

    [Theory]
    [InlineData("Accountant")]
    [InlineData("timemanagement")]  // case-sensitive by design (matches ERP ROLE_NAME exactly)
    [InlineData("")]
    [InlineData(null)]
    public void TryMapErpRole_RejectsNonTimeManagementRoles(string? erpRole)
    {
        Assert.False(Roles.TryMapErpRole(erpRole, out var tmRole));
        Assert.Equal(string.Empty, tmRole);
    }
}
