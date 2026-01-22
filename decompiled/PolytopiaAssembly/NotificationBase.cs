using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationBase : MonoBehaviour
{
	public enum State
	{
		None,
		Showing,
		Hiding,
		Hidden
	}

	[SerializeField]
	protected CanvasGroup canvasGroup;

	[SerializeField]
	protected TextMeshProUGUI header;

	[SerializeField]
	protected TextMeshProUGUI description;

	[SerializeField]
	protected RectTransform iconContainer;

	[SerializeField]
	protected float showTime = 2f;

	[HideInInspector]
	public NotificationManager notificationManager;

	[HideInInspector]
	public string notificationId;

	[HideInInspector]
	public float startOffsetY;

	[HideInInspector]
	public float showPosY;

	protected RectTransform m_rectTransform;

	protected Dictionary<int, float> contentHeights = new Dictionary<int, float>();

	protected float defaultShowTime = 2f;

	protected float totalHeight = -1f;

	protected Tween moveTween;

	protected Tween scaleTween;

	protected State m_state;

	public virtual string Title
	{
		get
		{
			return ((TMP_Text)header).text;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				value = Localization.Get("notifications.defaultheader");
			}
			((TMP_Text)header).text = value;
		}
	}

	public virtual string Message
	{
		get
		{
			return ((TMP_Text)description).text;
		}
		set
		{
			((TMP_Text)description).text = value;
			UpdateDescriptionSize();
		}
	}

	public virtual Sprite IconSprite
	{
		set
		{
			bool flag = (Object)(object)value != (Object)null;
			if (flag)
			{
				Image image = UIUtils.GetImage(value);
				((Transform)((Graphic)image).rectTransform).SetParent((Transform)(object)iconContainer, false);
				UIUtils.FitImageContentInParent(((Graphic)image).rectTransform);
			}
			SetLabelLeft(header, flag ? 64 : 18);
			SetLabelLeft(description, flag ? 64 : 18);
			UpdateDescriptionSize();
		}
	}

	public virtual RectTransform IconContent
	{
		set
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			bool flag = (Object)(object)value != (Object)null;
			if (flag)
			{
				((Transform)value).SetParent((Transform)(object)iconContainer, false);
				Vector2 val = default(Vector2);
				((Vector2)(ref val))._002Ector(0.5f, 0.5f);
				value.anchorMin = val;
				value.anchorMax = val;
				value.anchoredPosition = Vector2.zero;
				UIUtils.FitImageContentInParent(value);
			}
			SetLabelLeft(header, flag ? 64 : 18);
			SetLabelLeft(description, flag ? 64 : 18);
		}
	}

	public State state
	{
		get
		{
			return m_state;
		}
		protected set
		{
			m_state = value;
		}
	}

	public virtual RectTransform rectTransform
	{
		get
		{
			if ((Object)(object)m_rectTransform == (Object)null)
			{
				m_rectTransform = ((Component)this).GetComponent<RectTransform>();
			}
			return m_rectTransform;
		}
	}

	public virtual void Init()
	{
		((Component)this).gameObject.SetActive(false);
		defaultShowTime = showTime;
		state = State.Hidden;
	}

	public virtual void Show(float showTime = -1f)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if (showTime > -1f)
		{
			this.showTime = showTime;
		}
		if (!notificationManager.IsNotificationQueued(this))
		{
			UpdateSize();
			if ((Object)(object)notificationManager != (Object)null)
			{
				notificationManager.AddNotificationToQueue(this);
			}
			return;
		}
		((Transform)rectTransform).localScale = Vector3.zero;
		canvasGroup.alpha = 0f;
		float num = GetTotalHeightOfContent() * 0.5f;
		rectTransform.anchoredPosition = new Vector2(0f, showPosY + startOffsetY - num);
		((Component)this).gameObject.SetActive(true);
		canvasGroup.DOFade(1f, 0.3f);
		scaleTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)rectTransform, 1f, 0.3f), (Ease)27, 2f);
		TweenUtils.KillTween(moveTween);
		moveTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(rectTransform.DOAnchorPosY(showPosY - num, 0.2f), (Ease)6);
		((MonoBehaviour)this).Invoke("Hide", this.showTime);
		state = State.Showing;
		AudioManager.PlaySFX(SFXTypes.Popup);
	}

	private void OnDisable()
	{
		TweenUtils.KillTween(moveTween);
		TweenUtils.KillTween(scaleTween);
		DOTween.Kill((object)rectTransform, false);
		DOTween.Kill((object)canvasGroup, false);
	}

	public virtual void Hide()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		if (!((Object)(object)this == (Object)null))
		{
			state = State.Hiding;
			((MonoBehaviour)this).CancelInvoke("Show");
			((MonoBehaviour)this).CancelInvoke("Hide");
			if ((Object)(object)notificationManager != (Object)null)
			{
				notificationManager.NotificationStartHiding(this);
			}
			TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(canvasGroup.DOFade(0f, 0.5f), new TweenCallback(OnHideComplete));
			TweenUtils.KillTween(moveTween);
			TweenUtils.KillTween(scaleTween);
		}
	}

	private void OnHideComplete()
	{
		((Component)this).gameObject.SetActive(false);
		if ((Object)(object)notificationManager != (Object)null)
		{
			notificationManager.NotificationHidden(this);
		}
		state = State.Hidden;
	}

	protected virtual void UpdateSize()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Vector2 sizeDelta = rectTransform.sizeDelta;
		sizeDelta.y = GetTotalHeightOfContent();
		rectTransform.sizeDelta = sizeDelta;
	}

	protected virtual void UpdateSizeForObject(GameObject content, float height)
	{
		totalHeight = -1f;
		int instanceID = ((Object)content).GetInstanceID();
		if (!contentHeights.ContainsKey(instanceID))
		{
			contentHeights.Add(instanceID, height);
		}
		else
		{
			contentHeights[instanceID] = height;
		}
	}

	public virtual float GetTotalHeightOfContent()
	{
		if (totalHeight > -1f)
		{
			return totalHeight;
		}
		totalHeight = 0f;
		foreach (KeyValuePair<int, float> contentHeight in contentHeights)
		{
			totalHeight += contentHeight.Value;
		}
		return totalHeight;
	}

	protected void SetLabelLeft(TextMeshProUGUI label, float left)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 offsetMin = ((TMP_Text)label).rectTransform.offsetMin;
		offsetMin.x = left;
		((TMP_Text)label).rectTransform.offsetMin = offsetMin;
	}

	protected void UpdateDescriptionSize()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		TextMeshProUGUI obj = description;
		string text = ((TMP_Text)description).text;
		Rect rect = ((TMP_Text)description).rectTransform.rect;
		float width = ((Rect)(ref rect)).width;
		rect = ((TMP_Text)description).rectTransform.rect;
		float y = ((TMP_Text)obj).GetPreferredValues(text, width, ((Rect)(ref rect)).height).y;
		UpdateSizeForObject(((Component)description).gameObject, Mathf.Abs(((TMP_Text)description).rectTransform.offsetMax.y) + y + 12f);
	}

	public virtual void ResetNotification()
	{
		int[] array = contentHeights.Keys.ToArray();
		foreach (int key in array)
		{
			contentHeights[key] = 0f;
		}
		showTime = defaultShowTime;
		Title = null;
		IconSprite = null;
		state = State.Hidden;
		if (((Transform)iconContainer).childCount > 0)
		{
			for (int num = ((Transform)iconContainer).childCount - 1; num >= 0; num--)
			{
				Object.Destroy((Object)(object)((Component)((Transform)iconContainer).GetChild(num)).gameObject);
			}
		}
	}
}
