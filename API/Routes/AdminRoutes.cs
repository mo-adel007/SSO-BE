using SsoBackend.API.Controllers;

namespace SsoBackend.API.Routes;

public static class AdminRoutes
{
    public static void MapAdminRoutes(this WebApplication app)
    {
        var adminController = new AdminController();

        // Get all users (admin only)
        app.MapGet("/admin/users", adminController.GetAllUsers);
        
        // Update user role (admin only)
        app.MapPut("/admin/users/{id}/role", adminController.UpdateUserRole);
    }
}
