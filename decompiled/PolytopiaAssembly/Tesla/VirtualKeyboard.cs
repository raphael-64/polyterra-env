using System.Threading.Tasks;

namespace Tesla;

public abstract class VirtualKeyboard
{
	public delegate void VisibilityHandler(bool visible);

	public abstract bool isVisible { get; }

	public event VisibilityHandler visibilityChanged;

	public abstract void BeginSimpleTextEntrySession(VirtualKeyboardTextEntryHandler handler, object sender, string text, int caretPos);

	public abstract void DismissSession(VirtualKeyboardTextEntryHandler handler);

	internal void Internal_VirtualKeyboardVisibilityChange(bool visible)
	{
		Task.Run(delegate
		{
			this.visibilityChanged?.Invoke(visible);
		});
	}
}
