namespace PolytopiaBackendBase.Game;

public class UploadHighscoresBindingModel
{
	public byte[] InitialGameStateData { get; set; }

	public byte[] CurrentGameStateData { get; set; }

	public bool LZ4Compressed { get; set; }
}
