using System;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using Polytopia.Data;
using UnityEngine;

public class WeaponGFX : MonoBehaviour, IPooledObject
{
	public UnitData.WeaponEnum type;

	public Transform weaponContainer;

	public SpriteRenderer spriteRenderer;

	[Header("Ranged Weapon data")]
	public float centerOffset = 0.15f;

	public float lookAheadOffset = 0.01f;

	public float handleAngleOffset = 0.3f;

	public PathType pathType = (PathType)1;

	public PathMode pathMode = (PathMode)3;

	public int pathResolution = 10;

	public Ease pathEase = (Ease)1;

	protected Sprite defaultSprite;

	protected bool m_isUsed;

	protected Action callback;

	public bool IsUsed
	{
		get
		{
			return m_isUsed;
		}
		set
		{
			m_isUsed = value;
		}
	}

	private void OnDestroy()
	{
		ShortcutExtensions.DOKill((Component)(object)weaponContainer, false);
	}

	public void SetSkin(SkinType skinType)
	{
		if ((Object)(object)defaultSprite == (Object)null)
		{
			defaultSprite = spriteRenderer.sprite;
		}
		if (skinType == SkinType.Default)
		{
			spriteRenderer.sprite = defaultSprite;
			return;
		}
		SpriteHandle spriteHandle = new SpriteHandle();
		spriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle2)
		{
			if ((Object)(object)spriteHandle2.sprite != (Object)null)
			{
				spriteRenderer.sprite = spriteHandle2.sprite;
			}
		});
		spriteHandle.Request(SpriteData.GetWeaponGFXAddress(((Object)defaultSprite).name, skinType.GetName()));
	}

	public void AnimateCloseRangeAttack(float animTime = -1f, Action onComplete = null)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		if (animTime < 0f)
		{
			animTime = 0.2f;
		}
		callback = onComplete;
		((Component)this).gameObject.SetActive(true);
		m_isUsed = true;
		TweenSettingsExtensions.OnComplete<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(ShortcutExtensions.DOLocalRotate(weaponContainer, new Vector3(0f, 0f, -360f), animTime, (RotateMode)3), (Ease)7), new TweenCallback(AttackAnimComplete));
		PlaySFX();
	}

	public void AnimateRanged(float animTime, WorldCoordinates to, Action onComplete = null)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Invalid comparison between Unknown and I4
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Expected O, but got Unknown
		if (animTime < 0f)
		{
			animTime = 0.2f;
		}
		callback = onComplete;
		((Component)this).gameObject.SetActive(true);
		m_isUsed = true;
		Tile tileInstance = MapRenderer.Current.GetTileInstance(to);
		Vector3[] array = null;
		if ((int)pathType == 2)
		{
			array = GetWaypoints(((Component)this).transform.position, tileInstance.VisualCenter);
		}
		else
		{
			Vector3.Distance(((Component)this).transform.position, tileInstance.VisualCenter);
			Vector2 val = Vector2.op_Implicit(Vector3.Lerp(((Component)this).transform.position, tileInstance.VisualCenter, 0.5f));
			float num = Mathf.Atan2(val.y - weaponContainer.position.y, val.x - weaponContainer.position.x) * 180f / (float)Math.PI;
			weaponContainer.localEulerAngles = new Vector3(0f, 0f, num);
			array = (Vector3[])(object)new Vector3[2]
			{
				Vector2.op_Implicit(val),
				tileInstance.VisualCenter
			};
		}
		TweenSettingsExtensions.SetLookAt(TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Path, PathOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Path, PathOptions>>(ShortcutExtensions.DOPath(weaponContainer, array, animTime, pathType, pathMode, pathResolution, (Color?)Color.blue), pathEase), new TweenCallback(AttackAnimComplete)), lookAheadOffset, (Vector3?)null, (Vector3?)null);
		PlaySFX();
	}

	private Vector3[] GetWaypoints(Vector3 start, Vector3 end)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = new Vector3[6];
		float num = Vector3.Distance(start, end);
		Vector3 val = Vector3.Lerp(start, end, 0.5f);
		if (type != UnitData.WeaponEnum.FireBlow)
		{
			val.y += num * centerOffset;
		}
		float num2 = AngleInDeg(start, end);
		float num3 = 1f - Mathf.Abs(num2) / 90f;
		float num4 = handleAngleOffset * (centerOffset / 0.15f) * num3;
		array[0] = val;
		array[1] = start + GetHandlePosition(start, val, num4);
		array[2] = val + GetHandlePosition(val, start, 0f - num4);
		array[3] = end;
		array[4] = val + GetHandlePosition(val, end, num4);
		array[5] = end + GetHandlePosition(end, val, 0f - num4);
		return (Vector3[])(object)array;
	}

	private Vector3 GetHandlePosition(Vector3 from, Vector3 to, float angleOffset)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		float num = AngleInRad(from, to) + angleOffset;
		return new Vector3(Mathf.Cos(num), Mathf.Sin(num), 0f) * (Vector3.Distance(from, to) * 0.33333f);
	}

	public static float AngleInRad(Vector3 vec1, Vector3 vec2)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Atan2(vec2.y - vec1.y, vec2.x - vec1.x);
	}

	public static float AngleInDeg(Vector3 vec1, Vector3 vec2)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return AngleInRad(vec1, vec2) * 180f / (float)Math.PI;
	}

	private void AttackAnimComplete()
	{
		callback?.Invoke();
		ReturnToPool();
	}

	private void PlaySFX()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		SFXTypes id = SFXTypes.None;
		switch (type)
		{
		case UnitData.WeaponEnum.Club:
			id = SFXTypes.Club;
			break;
		case UnitData.WeaponEnum.Sword:
			id = SFXTypes.Sword;
			break;
		case UnitData.WeaponEnum.Arrow:
			id = SFXTypes.Arrow;
			break;
		case UnitData.WeaponEnum.Magic:
			id = SFXTypes.Club;
			break;
		case UnitData.WeaponEnum.Gun:
			id = SFXTypes.Gun;
			break;
		case UnitData.WeaponEnum.Rock:
			id = SFXTypes.Rock;
			break;
		case UnitData.WeaponEnum.Claw:
			id = SFXTypes.Claw;
			break;
		case UnitData.WeaponEnum.FireBlow:
			id = SFXTypes.FireBlow;
			break;
		case UnitData.WeaponEnum.Trident:
			id = SFXTypes.Trident;
			break;
		case UnitData.WeaponEnum.IceArrow:
			id = SFXTypes.Ice;
			break;
		case UnitData.WeaponEnum.Poison:
			id = SFXTypes.Rock;
			break;
		case UnitData.WeaponEnum.Sting:
			id = SFXTypes.Arrow;
			break;
		case UnitData.WeaponEnum.Dagger:
			id = SFXTypes.Dagger;
			break;
		}
		Vector3 val = CameraController.Camera.WorldToScreenPoint(((Component)this).transform.position);
		AudioManager.PlaySFX(id, 1f, 1f, AudioManager.GetPanFromPosition(Vector2.op_Implicit(val)));
	}

	public void ReturnToPool()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		ShortcutExtensions.DOKill((Component)(object)weaponContainer, false);
		weaponContainer.localPosition = Vector3.zero;
		weaponContainer.localEulerAngles = Vector3.zero;
		spriteRenderer.sprite = defaultSprite;
		ObjectPool.ReturnObject(((Component)this).gameObject);
		callback = null;
	}

	[SpecialName]
	GameObject IPooledObject.get_gameObject()
	{
		return ((Component)this).gameObject;
	}
}
