using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Polytopia.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitStatusDisplay : MonoBehaviour, IPooledObject
{
	public Transform healthContainer;

	public Transform typeContainer;

	[Header("Health")]
	public SpriteRenderer healthBg;

	public TextMeshPro healthLabel;

	public Sprite defenceBonusBg0;

	public Sprite defenceBonusBg1;

	public Sprite defenceBonusBg2;

	[Header("Unit Type")]
	public SpriteRenderer typeBg;

	public SpriteRenderer typeOutline;

	public SpriteRenderer typeIcon;

	public SpriteRenderer typeIconShadow;

	public SpriteRenderer peaceIcon;

	[Space(10f)]
	public GameObject detectObject;

	public SpriteRenderer detectBg;

	private Vector3 defaultDotScale;

	protected Unit unit;

	private SpriteHandle typeSpriteHandle = new SpriteHandle();

	public bool IsUsed { get; set; }

	private void Awake()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		typeSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
		{
			typeIcon.sprite = spriteHandle.sprite;
			typeIconShadow.sprite = spriteHandle.sprite;
		});
		defaultDotScale = ((Component)detectBg).transform.localScale;
	}

	private void OnDestroy()
	{
		typeSpriteHandle.SetCompletion(null);
	}

	public void Show()
	{
		IsUsed = true;
		((Component)this).gameObject.SetActive(true);
	}

	public void ReturnToPool()
	{
		IsUsed = false;
		if (!((Object)(object)this == (Object)null) && !((Object)(object)((Component)this).gameObject == (Object)null) && !((Object)(object)((Component)this).gameObject.transform == (Object)null) && !((Object)(object)ObjectPool.instance == (Object)null))
		{
			((Component)this).gameObject.SetActive(false);
			detectObject.SetActive(false);
			StopDotAnimation();
			((Component)this).gameObject.transform.parent = ((Component)ObjectPool.instance).transform;
			ObjectPool.ReturnObject(((Component)this).gameObject);
		}
	}

	public void SetState(UnitState state)
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		UnitData.Type type = ((state.passengerUnit != null) ? state.passengerUnit.type : state.type);
		typeSpriteHandle.Request(SpriteData.GetUnitIconAddress(type));
		float num = (float)state.GetDefenceBonus(GameManager.GameState) * 0.1f;
		((TMP_Text)healthLabel).text = Mathf.Ceil((float)(int)state.health * 0.1f).ToString();
		((Component)typeOutline).gameObject.SetActive(state.CanMove() && state.CanAttack() && unit.Owner.Id == GameManager.GameState.CurrentPlayer);
		PlayerState currentLocalPlayer = GameManager.Client.GetCurrentLocalPlayer();
		((Component)peaceIcon).gameObject.SetActive(currentLocalPlayer.HasPeaceWith(state.owner));
		((Graphic)healthLabel).color = ((state.health < 50) ? Color.red : Color.white);
		((Component)healthBg).gameObject.SetActive(num != 1f);
		if (num < 1f)
		{
			healthBg.sprite = defenceBonusBg0;
		}
		if (num > 1f)
		{
			healthBg.sprite = defenceBonusBg1;
		}
		if (num >= 2f)
		{
			healthBg.sprite = defenceBonusBg2;
		}
	}

	public void SetDot(bool show)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (detectObject.activeSelf != show)
		{
			detectObject.SetActive(show);
			if (show)
			{
				Vector3 localPosition = ((Component)detectBg).transform.localPosition;
				localPosition -= new Vector3(0f, 0.01f, 0f);
				TweenSettingsExtensions.SetLoops<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveY(((Component)detectBg).transform, localPosition.y, 1f, false), (Ease)24), -1);
			}
			else
			{
				StopDotAnimation();
			}
		}
	}

	public void SetUnit(Unit unit)
	{
		this.unit = unit;
	}

	public void RefreshColor()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Color color = ColorUtil.SetAlphaOnColor(unit.Owner.GetPlayerColor(GameManager.GameState), 0.4f);
		typeBg.color = color;
		color.a = 1f;
	}

	private void StopDotAnimation()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		ShortcutExtensions.DOKill((Component)(object)((Component)detectBg).transform, false);
		((Component)detectBg).transform.localScale = defaultDotScale;
	}

	[SpecialName]
	GameObject IPooledObject.get_gameObject()
	{
		return ((Component)this).gameObject;
	}
}
