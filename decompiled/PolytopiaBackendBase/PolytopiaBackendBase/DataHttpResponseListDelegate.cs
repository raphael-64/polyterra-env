using System.Threading.Tasks;

namespace PolytopiaBackendBase;

public delegate Task<DataHttpResponse<ServerResponseList<T>>> DataHttpResponseListDelegate<T>() where T : IServerResponseData;
