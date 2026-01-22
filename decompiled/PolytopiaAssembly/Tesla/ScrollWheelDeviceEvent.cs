namespace Tesla;

public struct ScrollWheelDeviceEvent
{
	public int scrollMovement;

	public uint buttons;

	public bool leftButtonDown => false;

	public bool middleButtonDown => false;

	public bool rightButtonDown => false;
}
