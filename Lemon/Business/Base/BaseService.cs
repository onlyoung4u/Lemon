namespace Lemon.Business.Base;

public abstract class BaseService(IFreeSql freeSql)
{
    protected readonly IFreeSql _freeSql = freeSql;
}
