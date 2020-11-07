using System.Threading.Tasks;
using Core.Entities;

namespace Core.Interfaces
{
public interface IUnitOfWork
{
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
    Task<int> Complete();
}
}