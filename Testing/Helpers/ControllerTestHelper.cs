
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectManagement.Testing.IntegrationTest;


namespace PorjectManagement.Testing.Helpers
{
    public static class ControllerTestHelper
    {
        public static T CreateControllerWithSession<T>(
            T controller,
            int? userId = null,
            int? roleId = null)
            where T : Controller
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new FakeSession();

            if (userId.HasValue)
                httpContext.Session.SetInt32("UserId", userId.Value);

            if (roleId.HasValue)
                httpContext.Session.SetInt32("RoleId", roleId.Value);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }
    }

}
