namespace Domain.Repositories.Factory;

public interface IRepositoryFactory<out TEfRepository, out TDapperRepository>
{
    TDapperRepository DapperRepository();
    TEfRepository EfRepository();
}