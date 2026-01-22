using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IchiGamepad;
using UnityEngine;

[CreateAssetMenu(menuName = "151A/Button Prompts/Button Sprite Hub")]
public class ButtonSpriteHub : ScriptableObject
{
	[Serializable]
	public struct ButtonSprite
	{
		public ButtonSpriteMode Mode;

		public GamepadType Gamepad;

		public GamepadButton Button;

		public Sprite Sprite;
	}

	[Serializable]
	private struct KeyboardSprite
	{
		public Sprite Sprite;

		[LogicalButtonDropdown]
		public ulong Button;
	}

	private struct SpriteKey
	{
		public ButtonSpriteMode Mode;

		public GamepadType Gamepad;

		public GamepadButton Button;

		public bool Equals(SpriteKey other)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			if (other.Mode == Mode && other.Gamepad == Gamepad)
			{
				return other.Button == Button;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is SpriteKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((465355115 * -1521134295 + Mode.GetHashCode()) * -1521134295 + ((object)Unsafe.As<GamepadType, GamepadType>(ref Gamepad)/*cast due to .constrained prefix*/).GetHashCode()) * -1521134295 + ((object)Unsafe.As<GamepadButton, GamepadButton>(ref Button)/*cast due to .constrained prefix*/).GetHashCode();
		}

		public override string ToString()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			return $"{Mode} {Gamepad} {Button}";
		}
	}

	private class SpriteKeyEqualityComparer : IEqualityComparer<SpriteKey>
	{
		public bool Equals(SpriteKey x, SpriteKey y)
		{
			return x.Equals(y);
		}

		public int GetHashCode(SpriteKey obj)
		{
			return obj.GetHashCode();
		}
	}

	[HideInInspector]
	[SerializeField]
	private Texture _texture;

	[SerializeField]
	private ButtonSprite[] _buttonSprites;

	[SerializeField]
	private KeyboardSprite[] _keyboardSprites;

	private Dictionary<SpriteKey, Sprite> _spriteLookup;

	public ButtonSprite[] ButtonSprites
	{
		get
		{
			return _buttonSprites;
		}
		set
		{
			_buttonSprites = value;
		}
	}

	public Texture Texture
	{
		get
		{
			return _texture;
		}
		set
		{
			_texture = value;
		}
	}

	public Sprite GetSprite(ButtonSpriteMode mode, LogicalButton button)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (!Backend.IsInitialized)
		{
			return null;
		}
		if (_spriteLookup == null)
		{
			RebuildSpriteCache();
		}
		IBackend instance = Backend.GetInstance();
		GamepadButton val = instance.MapButtonToPhysical(button);
		GamepadType gamepadType = instance.GetGamepadType();
		SpriteKey key = new SpriteKey
		{
			Mode = mode,
			Gamepad = gamepadType,
			Button = val
		};
		if (!_spriteLookup.TryGetValue(key, out var value))
		{
			throw new KeyNotFoundException($"Could not find sprite key {mode} {gamepadType} {val} in the sprite lookup");
		}
		return value;
	}

	private void RebuildSpriteCache()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<SpriteKey, Sprite> dictionary = new Dictionary<SpriteKey, Sprite>();
		_ = (GamepadType[])Enum.GetValues(typeof(GamepadType));
		ButtonSprite[] buttonSprites = _buttonSprites;
		for (int i = 0; i < buttonSprites.Length; i++)
		{
			ButtonSprite buttonSprite = buttonSprites[i];
			if (Object.op_Implicit((Object)(object)buttonSprite.Sprite))
			{
				SpriteKey key = new SpriteKey
				{
					Mode = buttonSprite.Mode,
					Gamepad = buttonSprite.Gamepad,
					Button = buttonSprite.Button
				};
				dictionary[key] = buttonSprite.Sprite;
			}
		}
		_spriteLookup = dictionary;
	}
}
