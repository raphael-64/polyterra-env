namespace Polytopia.Data;

public interface IPolytopiaDataRoot
{
	bool TryGetDataGeneric<T, S>(S enumValue, out T result);
}
