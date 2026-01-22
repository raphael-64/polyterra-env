using System.Threading.Tasks;

namespace PolytopiaBackendBase;

public delegate Task<DataHttpResponse<ServerResponse<T>>> DataHttpResponseDelegate<T>() where T : IServerResponseData;
