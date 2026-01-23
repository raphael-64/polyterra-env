namespace PolytopiaBackendBase.Game;

public class PolytopiaCommandViewModel
{
	public byte[] SerializedData { get; set; }

	public PolytopiaCommandViewModel(byte[] serializedData)
	{
		SerializedData = serializedData;
	}
}
