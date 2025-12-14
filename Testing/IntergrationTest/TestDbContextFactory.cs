using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;

namespace PorjectManagement.Testing.IntergrationTest
{
    public static class TestDbContextFactory
    {
        public static LabProjectManagementContext Create()
        {
            var options = new DbContextOptionsBuilder<LabProjectManagementContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new LabProjectManagementContext(options);
        }
    }
}
