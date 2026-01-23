using System;
using UnityEngine;

namespace UnityAsyncAwaitUtil;

public class TestButtonHandler
{
	[Serializable]
	public class Settings
	{
		public int NumPerColumn = 6;

		public float VerticalMargin = 50f;

		public float VerticalSpacing = 50f;

		public float HorizontalSpacing = 50f;

		public float HorizontalMargin = 50f;

		public float ButtonWidth = 50f;

		public float ButtonHeight = 50f;
	}

	private readonly Settings _settings;

	private int _buttonVCount;

	private int _buttonHCount;

	public TestButtonHandler(Settings settings)
	{
		_settings = settings;
	}

	public void Restart()
	{
		_buttonVCount = 0;
		_buttonHCount = 0;
	}

	public bool Display(string text)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if (_buttonVCount > _settings.NumPerColumn)
		{
			_buttonHCount++;
			_buttonVCount = 0;
		}
		bool result = GUI.Button(new Rect(_settings.HorizontalMargin + (float)_buttonHCount * (_settings.ButtonWidth + _settings.HorizontalSpacing), _settings.VerticalMargin + (float)_buttonVCount * (_settings.ButtonHeight + _settings.VerticalSpacing), _settings.ButtonWidth, _settings.ButtonHeight), text);
		_buttonVCount++;
		return result;
	}
}
