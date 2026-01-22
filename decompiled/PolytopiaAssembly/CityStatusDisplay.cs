using System.Runtime.CompilerServices;
using DG.Tweening;
using Polytopia.Data;
using UnityEngine;

public class CityStatusDisplay : MonoBehaviour, IPooledObject
{
	[SerializeField]
	protected CityStatusNameContainer nameContainer;

	[SerializeField]
	protected CityStatusProgressBar progressBar;

	protected City city;

	protected TribeData m_tribe;

	protected bool isSelected;

	protected Vector3 worldOffset = new Vector3(0f, -0.25f, 0f);

	public bool Selected
	{
		get
		{
			return isSelected;
		}
		set
		{
			isSelected = value;
			ShortcutExtensions.DOLocalMoveY(((Component)this).transform, isSelected ? (-0.2f) : 0f, 0.2f, false);
		}
	}

	public bool IsUsed { get; set; }

	public void SetCity(City city)
	{
		this.city = city;
		if ((Object)(object)this.city != (Object)null)
		{
			nameContainer.SetCity(this.city);
			progressBar.TotalFields = this.city.State.level + 1;
			progressBar.FilledFields = this.city.State.xp;
			progressBar.Dots = GameManager.GameState.Map.GetCityUnitCount(this.city.Tile.Coordinates);
			((Component)progressBar).gameObject.SetActive(GameManager.IsPlayerViewing(this.city.Owner.Id));
		}
	}

	public void ReturnToPool()
	{
		((Component)this).transform.parent = ((Component)ObjectPool.instance).transform;
		ObjectPool.ReturnObject(((Component)this).gameObject);
	}

	[SpecialName]
	GameObject IPooledObject.get_gameObject()
	{
		return ((Component)this).gameObject;
	}
}
