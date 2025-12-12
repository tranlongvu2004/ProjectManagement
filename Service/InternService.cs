using PorjectManagement.Models;

public class InternService : IInternService
{
    private readonly IInternRepo _internRepo;

    public InternService(IInternRepo internRepo)
    {
        _internRepo = internRepo;
    }

    public async Task<IEnumerable<User>> GetInternsAsync()
    {
        return await _internRepo.GetInternsAsync();
    }
}
