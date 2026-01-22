using System;

namespace PolytopiaBackendBase.Game;

public class SkipTurnBindingModel
{
	public Guid GameId { get; set; }

	public Guid UserId { get; set; }

	public uint? TurnNumber { get; set; }

	public bool IsAutoSkip { get; set; }

	public override bool Equals(object obj)
	{
		if (obj != null && obj is SkipTurnBindingModel skipTurnBindingModel)
		{
			if (GameId == skipTurnBindingModel.GameId && UserId == skipTurnBindingModel.UserId)
			{
				return TurnNumber == skipTurnBindingModel.TurnNumber;
			}
			return false;
		}
		return false;
	}
}
