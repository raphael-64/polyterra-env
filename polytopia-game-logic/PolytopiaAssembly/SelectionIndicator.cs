using UnityEngine;

public class SelectionIndicator : MonoBehaviour
{
	[SerializeField]
	protected SpriteRenderer tileSelectionSprite;

	[SerializeField]
	protected SpriteRenderer unitSelectionSprite;

	[SerializeField]
	protected CityAreaSelection cityAreaSelection;

	private void Start()
	{
		Hide();
		InputEvents.OnTileSelected += OnTileSelected;
		InputEvents.OnBuildingSelected += OnBuildingSelected;
		InputEvents.OnUnitSelected += OnUnitSelected;
		InputEvents.OnSelectionCleared += OnSelectionCleared;
	}

	private void OnDestroy()
	{
		InputEvents.OnTileSelected -= OnTileSelected;
		InputEvents.OnBuildingSelected -= OnBuildingSelected;
		InputEvents.OnUnitSelected -= OnUnitSelected;
		InputEvents.OnSelectionCleared -= OnSelectionCleared;
	}

	private void OnSelectionCleared()
	{
		Hide();
	}

	private void OnBuildingSelected(Building building)
	{
		OnTileSelected(building.Tile);
	}

	private void OnUnitSelected(Unit unit)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)unit == (Object)null)
		{
			Hide();
			return;
		}
		((Renderer)tileSelectionSprite).enabled = false;
		((Renderer)unitSelectionSprite).enabled = true;
		cityAreaSelection.Hide();
		unitSelectionSprite.color = (unit.IsInteractableByPlayer(GameManager.LocalPlayer.Id) ? ColorConstants.blue : ColorConstants.darkGray);
		MoveToTile(unit.Tile);
		Show();
	}

	private void OnTileSelected(Tile tile)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)tile == (Object)null)
		{
			Hide();
			return;
		}
		((Renderer)tileSelectionSprite).enabled = true;
		((Renderer)unitSelectionSprite).enabled = false;
		tileSelectionSprite.color = (tile.IsInteractableByPlayer(GameManager.LocalPlayer.Id) ? ColorConstants.blue : ColorConstants.darkGray);
		MoveToTile(tile);
		if ((Object)(object)tile.Improvement != (Object)null && tile.Improvement is City)
		{
			cityAreaSelection.Show(tile.Improvement as City, tile.Improvement.State.borderSize);
		}
		else
		{
			cityAreaSelection.Hide();
		}
		Show();
	}

	public void Show()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
		cityAreaSelection.Hide();
	}

	public void MoveToTile(Tile tile)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		SetPosition(tile.Position + (Vector3)(tile.Data.IsWater ? new Vector3(0f, -0.067f, 0f) : Vector3.zero));
	}

	private void OnEnable()
	{
		UpdateSize();
	}

	private void UpdateSize()
	{
	}

	public void SetPosition(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = position;
	}
}
