using Newtonsoft.Json;
using PolytopiaBackendBase;

public class BackendHttpClient : BaseClient
{
	private JsonSerializerSettings jsonSettings;

	public JsonSerializerSettings GetJsonSettings()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		if (jsonSettings == null)
		{
			jsonSettings = new JsonSerializerSettings();
		}
		return jsonSettings;
	}

	public override T DeserializePayload<T>(string json)
	{
		return JsonConvert.DeserializeObject<T>(json, GetJsonSettings());
	}

	public override string SerializePayload<T>(T payload)
	{
		return JsonConvert.SerializeObject((object)payload, GetJsonSettings());
	}
}
