using System.Threading.Tasks;

namespace PolytopiaBackendBase;

public delegate Task<ServerResponse<T>> ServerResponseDelegate<T>() where T : IServerResponseData;
