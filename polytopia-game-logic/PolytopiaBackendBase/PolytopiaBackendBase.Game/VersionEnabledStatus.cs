namespace PolytopiaBackendBase.Game;

public struct VersionEnabledStatus
{
	public VersionedFeature Feature { get; set; }

	public bool Enabled { get; set; }

	public string Message { get; set; }
}
