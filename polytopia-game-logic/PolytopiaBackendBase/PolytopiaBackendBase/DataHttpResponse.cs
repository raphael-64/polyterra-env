using System.Net.Http;

namespace PolytopiaBackendBase;

public class DataHttpResponse<T>
{
	public T Data { get; set; }

	public HttpResponseMessage Message { get; set; }

	public DataHttpResponse(HttpResponseMessage response)
	{
		Message = response;
	}
}
