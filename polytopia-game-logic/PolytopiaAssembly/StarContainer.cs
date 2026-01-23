using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using PolytopiaBackendBase.Game;
using TMPro;
using UnityEngine;

public class StarContainer : TribeButtonStarContainer
{
	[SerializeField]
	protected TextMeshProUGUI messageLabel;

	public string aboveAllStarsMessageKey = "tribepicker.topscore";

	public string belowAllStarsMessageKey = "tribepicker.topscore.next";

	protected bool isBig;

	protected float ogFontSize;

	protected int previousScore;

	protected bool playSfx;

	public int PreviousScore
	{
		set
		{
			previousScore = value;
		}
	}

	public float PreviouseRating
	{
		set
		{
			previousScore = Mathf.RoundToInt(value);
		}
	}

	public override int Score
	{
		get
		{
			return base.Score;
		}
		set
		{
			base.Score = value;
			if (Score < 0)
			{
				((Component)this).gameObject.SetActive(false);
				return;
			}
			((Component)this).gameObject.SetActive(true);
			string text = string.Empty;
			if (!string.IsNullOrEmpty(aboveAllStarsMessageKey))
			{
				text = Localization.Get(aboveAllStarsMessageKey, LocalizationUtils.FormatNumber(Score));
			}
			if (Stars > -1 && Stars < 3)
			{
				string arg = ((gameMode == GameMode.Perfection) ? LocalizationUtils.FormatNumber(ScoreSheet.scoreLimits[Stars]) : ScoreSheet.ratingLimits[Stars].ToString());
				text = $"{text}\n{Localization.Get(belowAllStarsMessageKey, arg)}";
			}
			((TMP_Text)messageLabel).text = text;
		}
	}

	public bool IsBig
	{
		get
		{
			return isBig;
		}
		set
		{
			isBig = value;
			TribeButtonStar[] array = stars;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].IsBig = isBig;
			}
			((TMP_Text)messageLabel).fontSize = (isBig ? 18f : ogFontSize);
		}
	}

	public override int Stars
	{
		get
		{
			return base.Stars;
		}
		protected set
		{
			bool flag = base.Stars != value;
			base.Stars = value;
			if (playSfx && flag)
			{
				AudioManager.PlaySFX(SFXTypes.Hit1);
			}
		}
	}

	public override void Init()
	{
		base.Init();
		ogFontSize = ((TMP_Text)messageLabel).fontSize;
	}

	public void Animate(float time, float delay = 0f)
	{
		playSfx = true;
		int num = GetStarCount(previousScore);
		int num2 = GetStarCount(Score);
		if (num < num2)
		{
			Stars = num;
			TribeButtonStar[] array = stars;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].AnimationTime = 0.5f;
			}
			TweenSettingsExtensions.SetDelay<TweenerCore<int, int, NoOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<int, int, NoOptions>>(DOTween.To((DOGetter<int>)(() => Stars), (DOSetter<int>)delegate(int x)
			{
				Stars = x;
			}, num2, time), (Ease)1), delay);
		}
	}
}
