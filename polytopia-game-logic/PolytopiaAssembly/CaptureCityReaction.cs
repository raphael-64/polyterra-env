using System;
using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;

public class CaptureCityReaction : ReactionBase
{
	private readonly CaptureCityAction action;

	public CaptureCityReaction(CaptureCityAction action)
	{
		this.action = action;
	}

	private void ValidateUnitForCaptureCity(TileData tile)
	{
		if (tile.unit == null || tile.unit.previousTurnEndCoordinates == WorldCoordinates.NULL_COORDINATES || tile.unit.coordinates == tile.unit.previousTurnEndCoordinates)
		{
			return;
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("tileCoords", tile.coordinates.ToString());
		dictionary.Add("unitCoords", tile.unit.coordinates.ToString());
		dictionary.Add("unitPreviousTurnCoords", tile.unit.previousTurnEndCoordinates.ToString());
		dictionary.Add("unitType", tile.unit.type.ToString());
		dictionary.Add("unitMoved", tile.unit.moved);
		dictionary.Add("unitAttacked", tile.unit.attacked);
		dictionary.Add("unitId", tile.unit.id);
		UnitAbility.Type[] obj = Enum.GetValues(typeof(UnitAbility.Type)) as UnitAbility.Type[];
		List<string> list = new List<string>();
		UnitAbility.Type[] array = obj;
		for (int i = 0; i < array.Length; i++)
		{
			UnitAbility.Type ability = array[i];
			if (tile.unit.HasAbility(ability, GameManager.GameState, tile.unit.coordinates))
			{
				list.Add(ability.ToString());
			}
		}
		dictionary.Add("unitAbilities", string.Join(", ", list));
		GameManager.GetAnalyticsManager().SendEvent("sameTurnCapture", dictionary);
	}

	public override void Execute(Action onComplete)
	{
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		ReactionUtils.NullEmbassyIncome(action.PlayerId, action.OldOwnerId);
		TileData tile = GameManager.GameState.Map.GetTile(action.Coordinates);
		GameManager.GameState.TryGetPlayer(action.PlayerId, out var playerState);
		PlayerState prevOwnerState;
		bool hasPreviousOwner = GameManager.GameState.TryGetPlayer(action.OldOwnerId, out prevOwnerState);
		bool isPreviousOwnerCapital = hasPreviousOwner && tile.capitalOf == action.OldOwnerId;
		bool num = isPreviousOwnerCapital && GameManager.IsPlayerViewing(action.OldOwnerId) && !GameManager.Client.IsSpectating;
		ValidateUnitForCaptureCity(tile);
		Tile instance = tile.GetInstance();
		int milliseconds = 1000;
		if (hasPreviousOwner && (action.PlayerId == GameManager.LocalPlayer.Id || action.OldOwnerId == GameManager.LocalPlayer.Id))
		{
			string value = ((action.PlayerId == GameManager.LocalPlayer.Id) ? "win" : "lose");
			string eventName = ((tile.capitalOf != 0) ? "capital_capture" : "city_capture");
			if (!GameManager.Client.IsRecap)
			{
				GameManager.GetAnalyticsManager().SendEvent(eventName, new Dictionary<string, object>
				{
					{ "capture_type", value },
					{
						"game_id",
						GameManager.Client.CurrentGameId
					}
				});
			}
		}
		if ((Object)(object)instance != (Object)null)
		{
			if (!instance.IsHidden)
			{
				CameraController.Instance.CenterOnPosition(tile.coordinates.ToPosition(), 0.8f, delegate
				{
					if (GameManager.IsPlayerViewing(action.PlayerId))
					{
						AudioManager.PlaySFXAtTile(SFXTypes.Capture, tile.coordinates);
						instance.Render();
						instance.SpawnShine(2f);
						instance.SpawnSparkles(2f);
						if (GameManager.GameState.Version >= 12)
						{
							ReactionUtils.UpdateSurroundingBordersAndTransportPaths(action.PlayerId, tile);
						}
						ResourceManager.AddResourceOfTypeToResourceBar(action.PlayerId, ResourceManager.Type.Score, action.Score, action.Coordinates);
						string title = Localization.Get("wcontroller.convertvillage.title");
						string message = Localization.Get("wcontroller.convertvillage.description");
						if (hasPreviousOwner)
						{
							bool num2 = hasPreviousOwner && tile.capitalOf == action.PlayerId;
							Log.Learn("extra: city_captured", Array.Empty<object>());
							if (num2)
							{
								title = Localization.Get("wcontroller.capital.regained.title");
								message = Localization.Get("wcontroller.capital.regained.description");
							}
							else if (isPreviousOwnerCapital)
							{
								title = Localization.Get("wcontroller.capital.captured.title");
								message = Localization.Get("wcontroller.capital.captured.description", prevOwnerState.GetLocalizedTribeName(GameManager.GameState));
							}
							else
							{
								title = Localization.Get("wcontroller.capital.captured2.title");
								message = Localization.Get("wcontroller.capital.captured2.description", instance.Improvement.State.name, playerState.GetLocalizedTribeName(GameManager.GameState));
							}
						}
						else
						{
							Log.Learn("extra: city_founded", Array.Empty<object>());
							if (GameManager.IsPlayerViewing(action.PlayerId) && !GameManager.Client.IsRecap)
							{
								GameManager.GetAnalyticsManager().SendEvent("village_capture", new Dictionary<string, object> { 
								{
									"game_id",
									GameManager.Client.CurrentGameId
								} });
							}
						}
						if (!GameManager.Client.IsSpectating)
						{
							NotificationManager.Notify(message, title, null, playerState);
						}
					}
					else
					{
						AudioManager.PlaySFXAtTile(SFXTypes.EnemyAction, tile.coordinates);
						instance.Render();
						instance.SpawnDarkPuff();
						instance.SpawnEmbers(2f);
						if (GameManager.GameState.Version >= 12)
						{
							ReactionUtils.UpdateSurroundingBordersAndTransportPaths(action.PlayerId, tile);
						}
						if (GameManager.IsPlayerViewing(action.OldOwnerId) && !GameManager.Client.IsSpectating)
						{
							Log.Learn("extra: city_lost", Array.Empty<object>());
							if (isPreviousOwnerCapital)
							{
								string linkedTribeNameWithSpace = playerState.GetLinkedTribeNameWithSpace(GameManager.GameState);
								IconPopup iconPopup = PopupManager.GetIconPopup();
								iconPopup.sprite = UIManager.IconData.GetSprite("CapitalCapture");
								iconPopup.Header = Localization.Get("wcontroller.capital.lost.title");
								iconPopup.Description = Localization.Get("wcontroller.capital.lost.description", linkedTribeNameWithSpace);
								iconPopup.SetTribeInfoButtons(TextType.Description);
								iconPopup.buttonData = new PopupBase.PopupButtonData[1]
								{
									new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
									{
										onComplete();
									})
								};
								iconPopup.Show();
							}
							else
							{
								NotificationManager.Notify(Localization.Get("wcontroller.capital.captured2.description", instance.Improvement.State.name, playerState.GetLocalizedTribeName(GameManager.GameState)), Localization.Get("wcontroller.capital.captured2.title"), null, prevOwnerState);
							}
						}
					}
				});
			}
			else
			{
				if (GameManager.GameState.Version >= 12)
				{
					ReactionUtils.UpdateSurroundingBordersAndTransportPaths(action.PlayerId, tile);
				}
				milliseconds = 0;
			}
			instance.StopFire();
		}
		if (tile.unit != null)
		{
			Tile tileInstance = MapRenderer.Current.GetTileInstance(action.PreviousHomeTown);
			if ((Object)(object)tileInstance != (Object)null && !tileInstance.IsHidden)
			{
				tileInstance.Render();
			}
		}
		InputEvents.SelectionCleared();
		ResourceManager.IncomeChanged(action.PlayerId);
		if (!num)
		{
			GameManager.DelayCall(milliseconds, onComplete);
		}
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
