using System.Threading.Tasks;

namespace PolytopiaBackendBase;

public delegate Task<ServerResponseList<T>> ServerResponseListDelegate<T>() where T : IServerResponseData;
