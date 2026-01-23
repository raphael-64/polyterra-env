using System.Collections.Generic;
using Newtonsoft.Json;

namespace Polytopia.Data;

public class AvatarCategory
{
	public enum Type
	{
		None,
		Layer0,
		Layer1,
		Layer2,
		Layer3,
		Layer4
	}

	public int idx;

	[JsonConverter(typeof(StringIDsToObjectsConverter<AvatarPart, AvatarPart.Type>))]
	public List<AvatarPart> parts = new List<AvatarPart>();

	public Type type => (Type)idx;
}
