using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;

public class InternRepo : IInternRepo
{
    private readonly LabProjectManagementContext _context;

    public InternRepo(LabProjectManagementContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetInternsAsync()
    {
        return await _context.Users
            .Where(u => u.RoleId == 2)
            .ToListAsync();
    }
}
