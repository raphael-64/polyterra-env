using System;

public class RevealCapitalReaction : ReactionBase
{
	private readonly RevealCapitalAction action;

	public RevealCapitalReaction(RevealCapitalAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		if (!GameManager.IsPlayerViewing(action.PlayerId))
		{
			onComplete();
			return;
		}
		TechView techView = UIManager.Instance.GetScreen(UIConstants.Screens.TechTree) as TechView;
		if (techView.Showing)
		{
			techView.OnBack();
		}
		Tile targetTile = MapRenderer.Current.GetTileInstance(action.Coordinates);
		ReactionUtils.CameraFocusIfExplored(GameManager.LocalPlayer.Id, action.Coordinates, shouldNudgeToCenter: false, 0.8f, delegate
		{
			targetTile.Render();
			targetTile.SpawnSparkles();
			GameManager.DelayCall(400, onComplete);
		});
	}
}
