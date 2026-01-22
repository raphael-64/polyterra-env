public static class UnitExtensions
{
	public static Unit GetInstance(this UnitState unit)
	{
		return MapRenderer.Current.GetUnitInstance(unit.id);
	}
}
