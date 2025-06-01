using UserManagerApi.Attributes;

namespace UserManagerApi.Controllers;

[CheckPermissions("Maintenance")]
public abstract class MaintenanceController : BaseController
{
}
