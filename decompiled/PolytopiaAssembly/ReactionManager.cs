public static class ReactionManager
{
	public static ReactionBase GetReaction(this ActionBase action)
	{
		if (action is MoveAction action2)
		{
			return new MoveReaction(action2);
		}
		if (action is ExploreAction action3)
		{
			return new ExploreReaction(action3);
		}
		if (action is EndTurnAction action4)
		{
			return new EndTurnReaction(action4);
		}
		if (action is StartTurnAction action5)
		{
			return new StartTurnReaction(action5);
		}
		if (action is IncreaseCurrencyAction action6)
		{
			return new IncreaseCurrencyReaction(action6);
		}
		if (action is IncreasePopulationAction action7)
		{
			return new IncreasePopulationReaction(action7);
		}
		if (action is DecreasePopulationAction action8)
		{
			return new DecreasePopulationReaction(action8);
		}
		if (action is IncreaseScoreAction action9)
		{
			return new IncreaseScoreReaction(action9);
		}
		if (action is BuildAction action10)
		{
			return new BuildReaction(action10);
		}
		if (action is BuildRoadAction buildRoadAction)
		{
			return new UpdateTilesWithNeighborsReaction(buildRoadAction.newTransportPathTiles);
		}
		if (action is UpdateRoutesAction updateRoutesAction)
		{
			return new UpdateTilesWithNeighborsReaction(updateRoutesAction.newTransportPathTiles);
		}
		if (action is CityLevelUpAction action11)
		{
			return new CityLevelUpReaction(action11);
		}
		if (action is TrainAction action12)
		{
			return new TrainReaction(action12);
		}
		if (action is KillUnitAction killUnitAction)
		{
			return new KillUnitReaction(killUnitAction.Coordinates);
		}
		if (action is RuleAreaAction action13)
		{
			return new RuleAreaReaction(action13);
		}
		if (action is CaptureCityAction action14)
		{
			return new CaptureCityReaction(action14);
		}
		if (action is ScoutMoveAction action15)
		{
			return new ScoutMoveReaction(action15);
		}
		if (action is AttackAction action16)
		{
			return new AttackReaction(action16);
		}
		if (action is CityRewardAction action17)
		{
			return new CityRewardReaction(action17);
		}
		if (action is ResearchAction action18)
		{
			return new ResearchReaction(action18);
		}
		if (action is MeetAction action19)
		{
			return new MeetReaction(action19);
		}
		if (action is CityRewardPopupAction action20)
		{
			return new CityRewardPopupReaction(action20);
		}
		if (action is EmbarkAction embarkAction)
		{
			return new UpdateTileReaction(embarkAction.Coordinates);
		}
		if (action is DisembarkAction disembarkAction)
		{
			return new UpdateTileReaction(disembarkAction.Coordinates);
		}
		if (action is DestroyImprovementAction action21)
		{
			return new DestroyImprovementReaction(action21);
		}
		if (action is DisbandUnitAction action22)
		{
			return new DisbandUnitReaction(action22);
		}
		if (action is ExamineRuinsAction action23)
		{
			return new ExamineRuinsReaction(action23);
		}
		if (action is RecoverAction action24)
		{
			return new RecoverReaction(action24);
		}
		if (action is HealAction action25)
		{
			return new HealReaction(action25);
		}
		if (action is HealOthersAction action26)
		{
			return new HealOthersReaction(action26);
		}
		if (action is PromoteAction action27)
		{
			return new PromoteReaction(action27);
		}
		if (action is StartMatchAction action28)
		{
			return new StartMatchReaction(action28);
		}
		if (action is GameOverAction action29)
		{
			return new GameOverReaction(action29);
		}
		if (action is ModifyProductionAction action30)
		{
			return new ModifyProductionReaction(action30);
		}
		if (action is ImprovementLevelUpAction action31)
		{
			return new ImprovementLevelUpReaction(action31);
		}
		if (action is ImprovementLevelDownAction action32)
		{
			return new ImprovementLevelDownReaction(action32);
		}
		if (action is UpgradeAction action33)
		{
			return new UpgradeReaction(action33);
		}
		if (action is ConvertAction action34)
		{
			return new ConvertReaction(action34);
		}
		if (action is EnableTaskAction action35)
		{
			return new EnableTaskReaction(action35);
		}
		if (action is TaskCompletedAction action36)
		{
			return new TaskCompletedReaction(action36);
		}
		if (action is FreezeUnitAction action37)
		{
			return new FreezeUnitReaction(action37);
		}
		if (action is FreezeAreaAction action38)
		{
			return new FreezeAreaReaction(action38);
		}
		if (action is FreezeTileAction action39)
		{
			return new FreezeTileReaction(action39);
		}
		if (action is ClimateChangeAction action40)
		{
			return new ClimateChangeReaction(action40);
		}
		if (action is BreakIceAreaAction action41)
		{
			return new BreakIceAreaReaction(action41);
		}
		if (action is BreakIceAction action42)
		{
			return new BreakIceReaction(action42);
		}
		if (action is ReselectAction action43)
		{
			return new ReselectReaction(action43);
		}
		if (action is CreateResourceAction action44)
		{
			return new CreateResourceReaction(action44);
		}
		if (action is DestroyResourceAction action45)
		{
			return new DestroyResourceReaction(action45);
		}
		if (action is PassPlayerAction action46)
		{
			return new PassPlayerReaction(action46);
		}
		if (action is ConnectCityAction action47)
		{
			return new ConnectCityReaction(action47);
		}
		if (action is DisconnectCityAction action48)
		{
			return new DisconnectCityReaction(action48);
		}
		if (action is ChangeCityConnectionAction action49)
		{
			return new ChangeCityConnectionReaction(action49);
		}
		if (action is ExpandCityAction action50)
		{
			return new ExpandCityReaction(action50);
		}
		if (action is WipePlayerAction action51)
		{
			return new WipePlayerReaction(action51);
		}
		if (action is HarvestImprovementAction action52)
		{
			return new HarvestImprovementReaction(action52);
		}
		if (action is ExplodeUnitAction action53)
		{
			return new ExplodeUnitReaction(action53);
		}
		if (action is PoisonUnitAction action54)
		{
			return new PoisonUnitReaction(action54);
		}
		if (action is DecomposeAction action55)
		{
			return new DecomposeReaction(action55);
		}
		if (action is EatAction action56)
		{
			return new EatReaction(action56);
		}
		if (action is BoostAction action57)
		{
			return new BoostReaction(action57);
		}
		if (action is BoostOthersAction action58)
		{
			return new BoostOthersReaction(action58);
		}
		if (action is DecreaseScoreAction action59)
		{
			return new DecreaseScoreReaction(action59);
		}
		if (action is EndMatchAction action60)
		{
			return new EndMatchReaction(action60);
		}
		if (action is WipePlayerEndAction action61)
		{
			return new WipePlayerEndReaction(action61);
		}
		if (action is EndCommandAction action62)
		{
			return new EndCommandReaction(action62);
		}
		if (action is ReceiveDiplomacyMessageAction action63)
		{
			return new ReceiveDiplomacyMessageReaction(action63);
		}
		if (action is PeaceRequestResponseAction action64)
		{
			return new PeaceRequestResponseReaction(action64);
		}
		if (action is PeaceTreatyAction action65)
		{
			return new PeaceTreatyReaction(action65);
		}
		if (action is BreakPeaceAction action66)
		{
			return new BreakPeaceReaction(action66);
		}
		if (action is EstablishEmbassyAction action67)
		{
			return new EstablishEmbassyReaction(action67);
		}
		if (action is RevealCapitalAction action68)
		{
			return new RevealCapitalReaction(action68);
		}
		if (action is UpgradeEmbassyAction action69)
		{
			return new UpgradeEmbassyReaction(action69);
		}
		if (action is DestroyEmbassyAction action70)
		{
			return new DestroyEmbassyReaction(action70);
		}
		if (action is HideAction action71)
		{
			return new HideReaction(action71);
		}
		if (action is RevealAction action72)
		{
			return new RevealReaction(action72);
		}
		if (action is InfiltrateAction action73)
		{
			return new InfiltrateReaction(action73);
		}
		if (action is InfiltrationRewardAction action74)
		{
			return new InfiltrationRewardReaction(action74);
		}
		if (action is ResignAction action75)
		{
			return new ResignReaction(action75);
		}
		return null;
	}
}
