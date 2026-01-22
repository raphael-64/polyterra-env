using System;
using UnityEngine;

public class RevealReaction : ReactionBase
{
	private readonly RevealAction action;

	public RevealReaction(RevealAction action)
	{
		this.action = action;
	}

	public override bool ShouldFocusCamera()
	{
		return IsRecapOrOpponentAction(action);
	}

	public override WorldCoordinates GetCameraFocusCoordinates()
	{
		return action.Coordinates;
	}

	public override void Execute(Action onComplete)
	{
		InputEvents.SelectionCleared();
		Tile tileInstance = MapRenderer.Current.GetTileInstance(action.Coordinates);
		if (Object.op_Implicit((Object)(object)tileInstance) && !tileInstance.IsHidden && Object.op_Implicit((Object)(object)tileInstance.Unit))
		{
			string arg = Localization.Get($"unit.names.{tileInstance.Unit.Data.type.GetName()}");
			NotificationManager.Notify(Localization.Get("world.reveal.text", arg), Localization.Get("world.reveal.header"));
			tileInstance.RenderUnit();
			tileInstance.SpawnPuff();
			tileInstance.SpawnSparkles();
			GameManager.DelayCall(200, onComplete);
			AudioManager.PlaySFXAtTile(SFXTypes.Explore, tileInstance.Coordinates);
		}
		else
		{
			onComplete();
		}
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
