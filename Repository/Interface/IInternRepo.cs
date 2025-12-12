using PorjectManagement.Models;

public interface IInternRepo
{
    Task<IEnumerable<User>> GetInternsAsync();
}
