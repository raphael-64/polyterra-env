using PolytopiaBackendBase.Game;
using UnityEngine;

public class TribeButtonStarContainer : UIBasicComponent
{
	[SerializeField]
	protected TribeButtonStar[] stars;

	[HideInInspector]
	public GameMode gameMode;

	protected int score = -1;

	protected int starCount = -1;

	public virtual int Score
	{
		get
		{
			return score;
		}
		set
		{
			score = value;
			Stars = GetStarCount(score);
		}
	}

	public virtual float Rating
	{
		get
		{
			return Score;
		}
		set
		{
			Score = Mathf.RoundToInt(value);
		}
	}

	public virtual int Stars
	{
		get
		{
			return starCount;
		}
		protected set
		{
			starCount = value;
			int num = stars.Length;
			for (int i = 0; i < num; i++)
			{
				TribeButtonStar obj = stars[i];
				((Component)obj).gameObject.SetActive(starCount >= 0);
				obj.Active = i < starCount;
			}
		}
	}

	public void Awake()
	{
		Score = score;
	}

	public override void Init()
	{
		base.Init();
		TribeButtonStar[] array = stars;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Init();
		}
	}

	protected virtual int GetStarCount(int value)
	{
		if (value < 0)
		{
			return -1;
		}
		int result = 0;
		int num = ((gameMode == GameMode.Perfection) ? ScoreSheet.scoreLimits.Length : ScoreSheet.ratingLimits.Length);
		for (int i = 0; i < num; i++)
		{
			uint num2 = ((gameMode == GameMode.Perfection) ? ScoreSheet.scoreLimits[i] : ScoreSheet.ratingLimits[i]);
			if (value >= num2)
			{
				result = i + 1;
			}
		}
		return result;
	}
}
