namespace Polytopia.IO;

public struct PolytopiaFileInfo
{
	private string _path;

	public bool Exists => PolytopiaFile.Exists(_path);

	public PolytopiaFileInfo(string path)
	{
		_path = path;
	}

	public void Delete()
	{
		PolytopiaFile.Delete(_path);
	}

	public void Refresh()
	{
	}
}
