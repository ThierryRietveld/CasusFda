using Application.Models;

namespace Application;

public interface IRealEstateService
{
    Task<IEnumerable<FundaObject>> GetFundaObjectsAsync(string searchQuery, CancellationToken cancellationToken = default);
}