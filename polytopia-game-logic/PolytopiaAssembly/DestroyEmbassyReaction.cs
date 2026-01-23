using System;

public class DestroyEmbassyReaction : ReactionBase
{
	private readonly DestroyEmbassyAction action;

	public DestroyEmbassyReaction(DestroyEmbassyAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		GameManager.GameState.TryGetPlayer(action.PlayerId, out var player);
		GameManager.GameState.TryGetPlayer(action.OpponentId, out var otherPlayerState);
		Tile tile = MapRenderer.Current.GetTileInstance(otherPlayerState.startTile);
		ResourceManager.IncomeChanged(action.PlayerId);
		ResourceManager.IncomeChanged(action.OpponentId);
		bool playerIsViewing = GameManager.IsPlayerViewing(action.PlayerId);
		bool flag = GameManager.IsPlayerViewing(action.OpponentId);
		if (!playerIsViewing && !flag)
		{
			onComplete();
			return;
		}
		MapRenderer.Current.GetTileInstance(otherPlayerState.startTile).RenderImprovement();
		Tile capitalTile = MapRenderer.Current.GetTileInstance(player.startTile);
		capitalTile.RenderImprovement();
		ReactionUtils.CameraFocusIfExplored(action.PlayerId, otherPlayerState.startTile, shouldNudgeToCenter: false, 0.8f, delegate
		{
			tile.SpawnDarkPuff();
			tile.SpawnEmbers();
			string text = "";
			string text2 = "diplomacy.destroyedembassy.description";
			if (playerIsViewing)
			{
				text = otherPlayerState.GetLinkedTribeNameWithSpace(GameManager.GameState);
				text2 = "diplomacy.destroyedembassy.description";
			}
			else
			{
				text = player.GetLinkedTribeNameWithSpace(GameManager.GameState);
				text2 = "diplomacy.destroyedembassy.description2";
			}
			DiplomacyPopup diplomacyPopup = PopupManager.GetDiplomacyPopup();
			diplomacyPopup.Header = Localization.Get("diplomacy.destroyedembassy.title");
			diplomacyPopup.Description = Localization.Get(text2, text);
			diplomacyPopup.SetTribeInfoButtons(TextType.Header);
			diplomacyPopup.SetTribeInfoButtons(TextType.Description);
			diplomacyPopup.SetData(player, otherPlayerState, DiplomacyGraphics.Type.EmbassyDestroyed);
			diplomacyPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
				{
					if (!GameManager.Client.IsSpectating)
					{
						onComplete();
					}
				}, -1, !GameManager.Client.IsSpectating)
			};
			if (GameManager.Client.IsSpectating)
			{
				diplomacyPopup.ReplayClickButton(0, delegate
				{
					onComplete();
				}, 2f);
			}
			AudioManager.PlaySFXAtTile(SFXTypes.FireImpact, capitalTile.Coordinates);
			GameManager.DelayCall(500, diplomacyPopup.Show);
		});
	}
}
