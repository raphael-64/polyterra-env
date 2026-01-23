using System;
using UnityEngine;

public class ResearchReaction : ReactionBase
{
	private readonly ResearchAction action;

	public ResearchReaction(ResearchAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		if (GameManager.GameState.GameLogicData.TryGetData(action.Type, out var data) && GameManager.GameState.TryGetPlayer(action.PlayerId, out var playerState))
		{
			if (action.Cost > 0)
			{
				ResourceManager.RemoveResourceOfType(action.PlayerId, ResourceManager.Type.Currency, action.Cost);
			}
			if (action.PlayerId == GameManager.LocalPlayer.Id)
			{
				TechEvents.TechCompleted(data);
				RectTransform[] unlockItems = TechItem.GetUnlockItems(data, playerState, onlyPickFirstItem: true);
				if (!GameManager.Client.IsRecap)
				{
					NotificationManager.Notify(Localization.Get("world.tech.new.message", Localization.Get(data.displayName)), Localization.Get("world.tech.new.title"), unlockItems[0]);
				}
				else if (GameManager.Client.IsSpectating)
				{
					NotificationManager.Notify(Localization.Get("replay.tech.new.message", GameManager.LocalPlayer.UserName, Localization.Get(data.displayName)), Localization.Get("world.tech.new.title"), unlockItems[0]);
				}
				AudioManager.PlaySFX(SFXTypes.Tech);
				for (int i = 0; i < GameManager.GameState.Map.Tiles.Length; i++)
				{
					TileData tileData = GameManager.GameState.Map.Tiles[i];
					if (tileData.resource != null)
					{
						tileData.GetInstance().RenderResource();
					}
					Tile tileInstance = MapRenderer.Current.GetTileInstance(GameManager.GameState.Map.Tiles[i].coordinates);
					if ((Object)(object)tileInstance.Unit != (Object)null)
					{
						tileInstance.Unit.UpdateObject();
					}
				}
			}
		}
		onComplete();
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
