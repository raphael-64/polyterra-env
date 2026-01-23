using UnityEngine;

public class WaitForUpdate : CustomYieldInstruction
{
	public override bool keepWaiting => false;
}
