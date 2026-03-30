using API.DTOs;

namespace API.Services.Interfaces;

public interface IOrigenService
{
    Task<IEnumerable<OrigenResponse>> GetAll();
    Task<OrigenResponse?> GetById(int id);
    Task<OrigenResponse> Create(CreateOrigenRequest request);
    Task<OrigenResponse?> Update(int id, UpdateOrigenRequest request);
    Task<bool> Delete(int id);
}
