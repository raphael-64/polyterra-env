using Polytopia.Data;

public static class CommandExtensions
{
	public static string GetConfirmInfo(this CommandBase command)
	{
		if (command == null)
		{
			return string.Empty;
		}
		if (command.GetCommandType() == CommandType.BreakPeace)
		{
			BreakPeaceCommand breakPeaceCommand = command as BreakPeaceCommand;
			int num = GameManager.GameState.Map.CountPlayerUnitsWithinOpponentBorders(breakPeaceCommand.PlayerId, breakPeaceCommand.OpponentId);
			if (num <= 0)
			{
				return Localization.Get($"actionbox.confirm.info.breakpeace");
			}
			return string.Format("{0} {1}", Localization.Get($"actionbox.confirm.info.breakpeace"), Localization.Get("actionbox.confirm.info.breakpeace.disband", num));
		}
		return Localization.Get("actionbox.confirm.info");
	}

	public static string GetShortInfo(this CommandBase command)
	{
		if (command == null)
		{
			return string.Empty;
		}
		if (command.GetCommandType() == CommandType.Disband)
		{
			if (!(command is DisbandCommand disbandCommand))
			{
				return string.Empty;
			}
			UnitState unitState = GameManager.GameState.Map.GetTile(disbandCommand.Coordinates)?.unit;
			if (unitState == null)
			{
				return string.Empty;
			}
			GameManager.GameState.GameLogicData.TryGetData(unitState.type, out var data);
			string displayName = data.displayName;
			if (data.IsVehicle() && GameManager.GameState.GameLogicData.TryGetData(unitState.passengerUnit.type, out var data2))
			{
				displayName = data2.displayName;
			}
			return string.Format(Localization.Get($"action.info.{command.Id}"), Localization.Get(displayName));
		}
		return Localization.Get($"action.info.{command.Id}");
	}

	public static string GetInfo(this CommandBase command)
	{
		if (command == null)
		{
			return string.Empty;
		}
		switch (command.GetCommandType())
		{
		case CommandType.Build:
		{
			BuildCommand buildCommand = command as BuildCommand;
			if (!GameManager.GameState.GameLogicData.TryGetData(buildCommand.Type, out var data2))
			{
				break;
			}
			string text = ((!data2.HasAbility(ImprovementAbility.Type.Consumed)) ? Localization.Get("action.info.build", Localization.Get(data2.displayName)) : Localization.Get("action.info.do", Localization.Get(data2.displayName)));
			int num = 0;
			int num2 = 0;
			if (data2.rewards != null && data2.rewards.Count > 0)
			{
				foreach (Rewards reward in data2.rewards)
				{
					num += reward.population;
					num2 += reward.currency;
				}
			}
			if (num > 0)
			{
				text = string.Format("{0}, {1}", text, Localization.Get("action.info.reward.population"));
			}
			if (num2 > 0)
			{
				text = string.Format("{0}, {1}", text, Localization.Get("action.info.reward.resources", num2));
			}
			return text;
		}
		case CommandType.Attack:
			return Localization.Get("action.info.attack");
		case CommandType.Train:
			return Localization.Get("action.info.train");
		case CommandType.Move:
			return Localization.Get("action.info.move");
		case CommandType.Capture:
			return Localization.Get("action.info.capture2");
		case CommandType.Research:
		{
			ResearchCommand researchCommand = command as ResearchCommand;
			if (GameManager.GameState.GameLogicData.TryGetData(researchCommand.Type, out var data))
			{
				return Localization.Get("action.info.research", Localization.Get(data.displayName), TechUtils.GetInfo(data));
			}
			break;
		}
		case CommandType.EndTurn:
			return Localization.Get("action.info.endturn");
		}
		return string.Empty;
	}
}
