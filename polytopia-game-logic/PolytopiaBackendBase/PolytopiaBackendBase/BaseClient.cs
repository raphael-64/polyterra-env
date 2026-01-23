using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PolytopiaBackendBase;

public abstract class BaseClient : HttpClient
{
	private const HttpCompletionOption DefaultCompletionOption = (HttpCompletionOption)0;

	public abstract string SerializePayload<T>(T payload);

	public abstract T DeserializePayload<T>(string json);

	public HttpContent CreateJsonHttpContent<T>(T payload)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		string text = SerializePayload(payload);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		return (HttpContent)new StringContent(text, Encoding.UTF8, "application/json");
	}

	public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
	{
		return SendAsync(request, (HttpCompletionOption)0, CancellationToken.None);
	}

	public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		return SendAsync(request, (HttpCompletionOption)0, cancellationToken);
	}

	public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return SendAsync(request, completionOption, CancellationToken.None);
	}

	public virtual Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return ((HttpClient)this).SendAsync(request, completionOption, cancellationToken);
	}

	public async Task<DataHttpResponse<T>> CreateDataResponse<T>(HttpResponseMessage response)
	{
		DataHttpResponse<T> dataResponse = new DataHttpResponse<T>(response);
		if (response.IsSuccessStatusCode)
		{
			string text = await new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEndAsync();
			try
			{
				dataResponse.Data = DeserializePayload<T>(text);
			}
			catch (Exception innerException)
			{
				throw new Exception("Failed to deserialize response data: " + text, innerException);
			}
		}
		return dataResponse;
	}

	public async Task<HttpResponseMessage> CallAsync(HttpMethod httpMethod, string path, Dictionary<string, string> parameters = null, object payload = null)
	{
		string query = null;
		if (parameters != null)
		{
			query = parameters.Select((KeyValuePair<string, string> pair) => pair.Key + "=" + pair.Value).Aggregate((string a, string b) => a + "&" + b);
		}
		UriBuilder uriBuilder = new UriBuilder(((HttpClient)this).BaseAddress)
		{
			Path = path,
			Query = query
		};
		HttpRequestMessage val = new HttpRequestMessage(httpMethod, uriBuilder.Uri);
		HttpContent val2 = ((payload != null) ? CreateJsonHttpContent(payload) : null);
		if (val2 != null)
		{
			val.Content = val2;
			val.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			((HttpClient)this).DefaultRequestHeaders.TransferEncodingChunked = true;
		}
		else
		{
			((HttpClient)this).DefaultRequestHeaders.TransferEncodingChunked = false;
		}
		return await SendAsync(val, (HttpCompletionOption)1);
	}

	public async Task<DataHttpResponse<T>> CallAsync<T>(HttpMethod httpMethod, string path, Dictionary<string, string> parameters = null, object payload = null)
	{
		return await CreateDataResponse<T>(await CallAsync(httpMethod, path, parameters, payload));
	}
}
