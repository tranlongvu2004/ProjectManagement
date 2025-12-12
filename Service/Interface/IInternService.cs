using PorjectManagement.Models;

public interface IInternService
{
    Task<IEnumerable<User>> GetInternsAsync();
}
