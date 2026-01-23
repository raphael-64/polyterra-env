namespace Tesla;

public interface VirtualKeyboardTextEntryHandler
{
	void OnTextEntryStarted(object sender, string text, int cursorPosition);

	void OnTextEntryUpdated(object sender, string text, int cursorPosition);

	void OnTextEntrySubmitted(object sender, string text);

	void OnTextEntryCanceled(object sender);
}
