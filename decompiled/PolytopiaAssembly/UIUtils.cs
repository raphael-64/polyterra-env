using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;
using UnityEngine.UI;

public class UIUtils
{
	public static Rect GetActualRect(RectTransform rectTransform, bool includeRootSize = true)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		Rect result = (includeRootSize ? rectTransform.rect : Rect.zero);
		Rect val2 = default(Rect);
		foreach (RectTransform item in (Transform)rectTransform)
		{
			RectTransform val = item;
			Rect rect = val.rect;
			Vector2 position = ((Rect)(ref rect)).position;
			rect = val.rect;
			((Rect)(ref val2))._002Ector(position, ((Rect)(ref rect)).size);
			((Rect)(ref result)).xMin = Mathf.Min(((Rect)(ref result)).xMin, ((Transform)val).localPosition.x + ((Rect)(ref val2)).xMin);
			((Rect)(ref result)).yMin = Mathf.Min(((Rect)(ref result)).yMin, ((Transform)val).localPosition.y + ((Rect)(ref val2)).yMin);
			((Rect)(ref result)).xMax = Mathf.Max(((Rect)(ref result)).xMax, ((Transform)val).localPosition.x + ((Rect)(ref val2)).xMax);
			((Rect)(ref result)).yMax = Mathf.Max(((Rect)(ref result)).yMax, ((Transform)val).localPosition.y + ((Rect)(ref val2)).yMax);
		}
		Vector2 position2 = rectTransform.anchoredPosition + new Vector2(((Rect)(ref result)).center.x, ((Rect)(ref result)).center.y);
		((Rect)(ref result)).position = position2;
		return result;
	}

	public static void FitImageContentInParent(RectTransform target)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		Transform parent = ((Transform)target).parent;
		RectTransform val = (RectTransform)(object)((parent is RectTransform) ? parent : null);
		float targetSize = ((!Object.op_Implicit((Object)(object)val)) ? Mathf.Max(target.sizeDelta.x, target.sizeDelta.y) : Mathf.Max(val.sizeDelta.x, val.sizeDelta.y));
		FitImageContentInside(target, targetSize);
	}

	public static void FitImageContentInside(RectTransform target, float targetSize)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		Bounds imageContentOffset = GetImageContentOffset(target);
		float num = Mathf.Max(((Bounds)(ref imageContentOffset)).size.x, ((Bounds)(ref imageContentOffset)).size.y);
		float num2 = ((num <= 0f) ? 1f : (targetSize / num));
		((Transform)target).localScale = new Vector3(num2, num2, 1f);
		target.anchoredPosition = Vector2.op_Implicit(-(((Bounds)(ref imageContentOffset)).center * num2));
	}

	public static Bounds GetImageContentOffset(RectTransform target)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		Image[] componentsInChildren = ((Component)target).GetComponentsInChildren<Image>();
		Bounds result = default(Bounds);
		float uI_PIXELS_PER_UNIT = UIConstants.UI_PIXELS_PER_UNIT;
		int num = componentsInChildren.Length;
		for (int i = 0; i < num; i++)
		{
			Image val = componentsInChildren[i];
			Sprite sprite = val.sprite;
			if (!((Object)(object)sprite == (Object)null))
			{
				Vector2[] vertices = sprite.vertices;
				Rect rect = sprite.rect;
				Vector2 val2 = -(((Rect)(ref rect)).size / 2f - sprite.pivot);
				rect = sprite.rect;
				Vector2 val3 = val2 / ((Rect)(ref rect)).size - (((Graphic)val).rectTransform.pivot - new Vector2(0.5f, 0.5f));
				Bounds bounds = sprite.bounds;
				Vector2 val4 = Vector2.op_Implicit(((Bounds)(ref bounds)).size) * val3;
				Vector2[] array = vertices;
				for (int j = 0; j < array.Length; j++)
				{
					Vector3 val5 = Vector2.op_Implicit((array[j] + val4) * uI_PIXELS_PER_UNIT);
					Vector3 val6 = ((Transform)target).InverseTransformPoint(((Component)val).transform.position) + val5;
					((Bounds)(ref result)).Encapsulate(val6);
				}
			}
		}
		return result;
	}

	public static Image GetImage(SpriteAddress spriteAddress)
	{
		Image image = GetImage();
		((Object)image).name = spriteAddress.sprite;
		GameManager.GetSpriteAtlasManager().LoadSprite(spriteAddress, delegate(string atlasName, string spriteName, Sprite sprite)
		{
			image.sprite = sprite;
			((Graphic)image).SetNativeSize();
		});
		return image;
	}

	public static Image GetImage()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		Image obj = new GameObject().AddComponent<Image>();
		obj.useSpriteMesh = true;
		((Graphic)obj).raycastTarget = false;
		return obj;
	}

	public static Image GetImage(Sprite sprite)
	{
		Image image = GetImage();
		if ((Object)(object)sprite != (Object)null)
		{
			image.sprite = sprite;
			((Graphic)image).SetNativeSize();
			((Object)image).name = ((Object)sprite).name;
		}
		return image;
	}

	private static void SelectBestScores(Selectable[] selectables, Selectable[] selection, float[,] scores)
	{
		int num = 4;
		for (int i = 0; i < num; i++)
		{
			int num2 = -1;
			int num3 = -1;
			float num4 = 0f;
			for (int j = 0; j < num; j++)
			{
				for (int k = 0; k < selectables.Length; k++)
				{
					float num5 = scores[j, k];
					if (num5 > num4)
					{
						num2 = j;
						num3 = k;
						num4 = num5;
					}
				}
			}
			if (num2 != -1)
			{
				for (int l = 0; l < selectables.Length; l++)
				{
					scores[num2, l] = 0f;
				}
				for (int m = 0; m < num; m++)
				{
					scores[m, num3] *= 0.5f;
				}
				scores[num2, num3] = 0f;
				selection[num2] = selectables[num3];
				continue;
			}
			break;
		}
	}

	public static void SetExplicitNavigation(RectTransform parent, bool useCenter = false, int distanceWeight = 1)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		Selectable[] componentsInChildren = ((Component)parent).GetComponentsInChildren<Selectable>();
		float[,] scores = new float[4, componentsInChildren.Length];
		Selectable[] array = (Selectable[])(object)new Selectable[4];
		Selectable[] array2 = componentsInChildren;
		foreach (Selectable val in array2)
		{
			Navigation navigation = val.navigation;
			if ((int)((Navigation)(ref navigation)).mode != 0 && ((Behaviour)val).enabled)
			{
				Navigation navigation2 = default(Navigation);
				((Navigation)(ref navigation2)).mode = (Mode)4;
				FindSelectable(((Component)val).transform, Vector3.left, componentsInChildren, scores, 0, useCenter, distanceWeight);
				FindSelectable(((Component)val).transform, Vector3.up, componentsInChildren, scores, 1, useCenter, distanceWeight);
				FindSelectable(((Component)val).transform, Vector3.right, componentsInChildren, scores, 2, useCenter, distanceWeight);
				FindSelectable(((Component)val).transform, Vector3.down, componentsInChildren, scores, 3, useCenter, distanceWeight);
				SelectBestScores(componentsInChildren, array, scores);
				((Navigation)(ref navigation2)).selectOnLeft = array[0];
				((Navigation)(ref navigation2)).selectOnUp = array[1];
				((Navigation)(ref navigation2)).selectOnRight = array[2];
				((Navigation)(ref navigation2)).selectOnDown = array[3];
				val.navigation = navigation2;
			}
		}
	}

	public static Selectable FindSelectable(Transform target, Vector3 dir, Selectable[] selectables, float[,] scores, int scoreDirectionIndex, bool useCenter, int distanceWeight)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		dir = ((Vector3)(ref dir)).normalized;
		Rect worldRect = ((RectTransform)(object)((target is RectTransform) ? target : null)).GetWorldRect(UIManager.Canvas.scaleFactor);
		Vector2 val = default(Vector2);
		if (!useCenter)
		{
			Vector3 val2 = Quaternion.Inverse(target.rotation) * dir;
			val = Vector2.op_Implicit(target.TransformPoint(GetPointOnRectEdge((RectTransform)(object)((target is RectTransform) ? target : null), Vector2.op_Implicit(val2))));
		}
		float num = float.NegativeInfinity;
		Selectable result = null;
		for (int i = 0; i < selectables.Length; i++)
		{
			Selectable val3 = selectables[i];
			if ((Object)(object)((Component)val3).transform == (Object)(object)target || !val3.IsInteractable())
			{
				continue;
			}
			Navigation navigation = val3.navigation;
			if ((int)((Navigation)(ref navigation)).mode == 0 || !((Behaviour)val3).enabled)
			{
				continue;
			}
			Transform transform = ((Component)val3).transform;
			RectTransform val4 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			Rect val5 = (Rect)(((Object)(object)val4 != (Object)null) ? val4.rect : default(Rect));
			Rect val6 = (Rect)(((Object)(object)val4 != (Object)null) ? val4.GetWorldRect(UIManager.Canvas.scaleFactor) : default(Rect));
			float num2 = 0f;
			float num3 = 0f;
			bool flag = ((Rect)(ref worldRect)).Overlaps(val6);
			if (useCenter || flag)
			{
				Vector2.op_Implicit(((Rect)(ref val5)).center);
				Vector3 val7 = Vector2.op_Implicit(((Rect)(ref val6)).center - ((Rect)(ref worldRect)).center);
				num2 = ((Vector3)(ref val7)).sqrMagnitude;
				num3 = Vector3.Dot(dir, val7);
			}
			else
			{
				Vector3 val8 = Vector2.op_Implicit(((Rect)(ref val6)).min - val);
				Vector3 val9 = Vector2.op_Implicit(((Rect)(ref val6)).max - val);
				if (Mathf.Sign(Vector3.Cross(val8, dir).z) != Mathf.Sign(Vector3.Cross(val9, dir).z))
				{
					num3 = Mathf.Min(Vector3.Dot(val8, dir), Vector3.Dot(val9, dir));
					num2 = num3 * num3;
				}
				else
				{
					Vector3 val10 = Vector2.op_Implicit(new Vector2(((Rect)(ref val6)).xMin, ((Rect)(ref val6)).yMax) - val);
					Vector3 val11 = Vector2.op_Implicit(new Vector2(((Rect)(ref val6)).xMax, ((Rect)(ref val6)).yMin) - val);
					num3 = Mathf.Min(new float[4]
					{
						Vector3.Dot(val8, dir),
						Vector3.Dot(val9, dir),
						Vector3.Dot(val10, dir),
						Vector3.Dot(val11, dir)
					});
					num2 = Mathf.Min(new float[4]
					{
						((Vector3)(ref val8)).sqrMagnitude,
						((Vector3)(ref val9)).sqrMagnitude,
						((Vector3)(ref val10)).sqrMagnitude,
						((Vector3)(ref val11)).sqrMagnitude
					});
				}
			}
			if (num3 <= 0.001f)
			{
				scores[scoreDirectionIndex, i] = 0f;
				continue;
			}
			float num4 = (scores[scoreDirectionIndex, i] = num3 / Mathf.Pow(Mathf.Max(1E-06f, num2), (float)distanceWeight));
			if (num4 > num)
			{
				num = num4;
				result = val3;
			}
		}
		return result;
	}

	public static void SetSelectOnLeft(Selectable selectable, Selectable target)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = selectable.navigation;
		((Navigation)(ref navigation)).mode = (Mode)4;
		((Navigation)(ref navigation)).selectOnLeft = target;
		selectable.navigation = navigation;
	}

	public static void SetSelectOnUp(Selectable selectable, Selectable target)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = selectable.navigation;
		((Navigation)(ref navigation)).mode = (Mode)4;
		((Navigation)(ref navigation)).selectOnUp = target;
		selectable.navigation = navigation;
	}

	public static void SetSelectOnRight(Selectable selectable, Selectable target)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = selectable.navigation;
		((Navigation)(ref navigation)).mode = (Mode)4;
		((Navigation)(ref navigation)).selectOnRight = target;
		selectable.navigation = navigation;
	}

	public static void SetSelectOnDown(Selectable selectable, Selectable target)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = selectable.navigation;
		((Navigation)(ref navigation)).mode = (Mode)4;
		((Navigation)(ref navigation)).selectOnDown = target;
		selectable.navigation = navigation;
	}

	public static void UpdateNavigationOnListCells<T>(List<T> cells, Selectable up, Selectable down) where T : IListCellNavigation
	{
		IListCellNavigation listCellNavigation = null;
		for (int i = 0; i < cells.Count; i++)
		{
			T val = cells[i];
			UpdateInternalNavigation(val);
			if ((Object)(object)val.GetMainSelectable() == (Object)null)
			{
				continue;
			}
			if (listCellNavigation == null && Object.op_Implicit((Object)(object)val.GetMainSelectable()))
			{
				if ((Object)(object)up != (Object)null)
				{
					SetSelectOnDown(up, val.GetMainSelectable());
				}
				SetSelectOnUp(val, up, up);
			}
			else
			{
				SetSelectOnUp(val, listCellNavigation.GetMainSelectable(), listCellNavigation.GetAccessorySelectable());
				SetSelectOnDown(listCellNavigation, val.GetMainSelectable(), val.GetAccessorySelectable());
			}
			listCellNavigation = val;
		}
		if (listCellNavigation == null)
		{
			if ((Object)(object)up != (Object)null)
			{
				SetSelectOnDown(up, down);
			}
			if ((Object)(object)down != (Object)null)
			{
				SetSelectOnUp(down, up);
			}
		}
		else
		{
			SetSelectOnDown(listCellNavigation, down, down);
			if ((Object)(object)down != (Object)null)
			{
				SetSelectOnUp(down, listCellNavigation.GetMainSelectable());
			}
		}
	}

	public static void UpdateInternalNavigation(IListCellNavigation listCellNavigation)
	{
		Selectable mainSelectable = listCellNavigation.GetMainSelectable();
		Selectable accessorySelectable = listCellNavigation.GetAccessorySelectable();
		if (!((Object)(object)mainSelectable == (Object)null))
		{
			if ((Object)(object)mainSelectable == (Object)(object)accessorySelectable)
			{
				SetSelectOnLeft(mainSelectable, null);
				SetSelectOnRight(mainSelectable, null);
			}
			else
			{
				SetSelectOnLeft(accessorySelectable, mainSelectable);
				SetSelectOnRight(mainSelectable, accessorySelectable);
			}
		}
	}

	public static void SetSelectOnUp(IListCellNavigation listCellNavigation, Selectable mainSelectable, Selectable accessorySelectable)
	{
		Selectable mainSelectable2 = listCellNavigation.GetMainSelectable();
		Selectable accessorySelectable2 = listCellNavigation.GetAccessorySelectable();
		if ((Object)(object)accessorySelectable2 != (Object)null)
		{
			SetSelectOnUp(accessorySelectable2, accessorySelectable);
		}
		if ((Object)(object)mainSelectable2 != (Object)null)
		{
			SetSelectOnUp(mainSelectable2, mainSelectable);
		}
	}

	public static void SetSelectOnDown(IListCellNavigation listCellNavigation, Selectable mainSelectable, Selectable accessorySelectable)
	{
		Selectable mainSelectable2 = listCellNavigation.GetMainSelectable();
		Selectable accessorySelectable2 = listCellNavigation.GetAccessorySelectable();
		if ((Object)(object)accessorySelectable2 != (Object)null)
		{
			SetSelectOnDown(accessorySelectable2, accessorySelectable);
		}
		if ((Object)(object)mainSelectable2 != (Object)null)
		{
			SetSelectOnDown(mainSelectable2, mainSelectable);
		}
	}

	private static Vector3 GetPointOnRectEdge(RectTransform rect, Vector2 dir)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rect == (Object)null)
		{
			return Vector3.zero;
		}
		if (dir != Vector2.zero)
		{
			dir /= Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
		}
		Rect rect2 = rect.rect;
		Vector2 center = ((Rect)(ref rect2)).center;
		rect2 = rect.rect;
		dir = center + Vector2.Scale(((Rect)(ref rect2)).size, dir * 0.5f);
		return Vector2.op_Implicit(dir);
	}

	public static RectTransform GetTile(TerrainData.Type type, int climate)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val;
		if (type == TerrainData.Type.Mountain)
		{
			val = new GameObject
			{
				name = "UIMountainContainer"
			}.AddComponent<RectTransform>();
			Image image = GetImage(SpriteData.GetTileSpriteAddress(TerrainData.Type.Field, climate));
			Image image2 = GetImage(SpriteData.GetTileSpriteAddress(type, climate));
			((Graphic)image).SetNativeSize();
			((Graphic)image2).SetNativeSize();
			((Graphic)image).raycastTarget = false;
			((Graphic)image2).raycastTarget = false;
			RectTransform rectTransform = ((Graphic)image).rectTransform;
			RectTransform rectTransform2 = ((Graphic)image2).rectTransform;
			((Transform)rectTransform).SetParent((Transform)(object)val, false);
			((Transform)rectTransform2).SetParent((Transform)(object)val, false);
			rectTransform.anchoredPosition = Vector2.zero;
			rectTransform2.anchoredPosition = new Vector2(0.19f, 15.52f);
		}
		else
		{
			val = ((Graphic)GetImage(SpriteData.GetTileSpriteAddress(type, climate))).rectTransform;
		}
		return val;
	}

	public static UIUnitRenderer GetUIUnitRenderer(UnitData unit, PlayerState playerState)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject();
		val.AddComponent<RectTransform>();
		UIUnitRenderer uIUnitRenderer = val.AddComponent<UIUnitRenderer>();
		uIUnitRenderer.UnitType = unit.type;
		uIUnitRenderer.PlayerState = playerState;
		uIUnitRenderer.RefreshGraphics();
		return uIUnitRenderer;
	}

	public static UIUnitRenderer GetUIUnitRenderer(Unit unit, PlayerState playerState)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject();
		val.AddComponent<RectTransform>();
		UIUnitRenderer uIUnitRenderer = val.AddComponent<UIUnitRenderer>();
		uIUnitRenderer.SourceUnit = unit;
		uIUnitRenderer.PlayerState = playerState;
		uIUnitRenderer.RefreshGraphics();
		return uIUnitRenderer;
	}

	public static UIUnitRenderer GetUIUnitRenderer(UnitData.Type unitType, TribeData tribeData, SkinType skinType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject();
		val.AddComponent<RectTransform>();
		UIUnitRenderer uIUnitRenderer = val.AddComponent<UIUnitRenderer>();
		uIUnitRenderer.UnitType = unitType;
		uIUnitRenderer.TribeData = tribeData;
		uIUnitRenderer.SkinType = skinType;
		uIUnitRenderer.RefreshGraphics();
		return uIUnitRenderer;
	}
}
