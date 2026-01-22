using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

public class SwitchHttpClient : BackendHttpClient
{
	public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
	{
		UnityWebRequest www = new UnityWebRequest();
		www.method = request.Method.Method;
		www.url = request.RequestUri.AbsoluteUri;
		if (request.Content != null)
		{
			UnityWebRequest val = www;
			val.uploadHandler = (UploadHandler)new UploadHandlerRaw(await request.Content.ReadAsByteArrayAsync());
			www.uploadHandler.contentType = request.Content.Headers.ContentType.MediaType;
		}
		foreach (KeyValuePair<string, IEnumerable<string>> item in (HttpHeaders)request.Headers)
		{
			using IEnumerator<string> enumerator2 = item.Value.GetEnumerator();
			if (enumerator2.MoveNext())
			{
				string current2 = enumerator2.Current;
				www.SetRequestHeader(item.Key, current2);
			}
		}
		www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
		await www.SendWebRequest();
		return new HttpResponseMessage((HttpStatusCode)www.responseCode)
		{
			Content = (HttpContent)new StringContent(www.downloadHandler.text)
		};
	}
}
