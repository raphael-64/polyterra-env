using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RendererToTexturePass : ScriptableRenderPass
{
	private List<RendererToTexture> pendingDraws = new List<RendererToTexture>();

	private RenderTextureDescriptor rtDescriptor;

	private CommandBuffer cmd;

	public RendererToTexturePass()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		cmd = new CommandBuffer();
		cmd.name = "render to texture pass";
	}

	public void PassPendingDraws(IEnumerable<RendererToTexture> draws)
	{
		pendingDraws.Clear();
		pendingDraws.AddRange(draws);
	}

	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		rtDescriptor = renderingData.cameraData.cameraTargetDescriptor;
		((RenderTextureDescriptor)(ref rtDescriptor)).width = 32;
		((RenderTextureDescriptor)(ref rtDescriptor)).height = 32;
		((RenderTextureDescriptor)(ref rtDescriptor)).depthBufferBits = 0;
		((RenderTextureDescriptor)(ref rtDescriptor)).memoryless = (RenderTextureMemoryless)6;
		foreach (RendererToTexture pendingDraw in pendingDraws)
		{
			pendingDraw.renderTexture = new RenderTexture(rtDescriptor);
			pendingDraw.renderTexture.Create();
		}
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		cmd.Clear();
		foreach (RendererToTexture pendingDraw in pendingDraws)
		{
			ExecutePendingDraw(cmd, pendingDraw, ref renderingData);
		}
		((ScriptableRenderContext)(ref context)).ExecuteCommandBuffer(cmd);
		cmd.Clear();
	}

	private void ExecutePendingDraw(CommandBuffer cmd, RendererToTexture pendingDraw, ref RenderingData renderingData)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		cmd.SetRenderTarget(RenderTargetIdentifier.op_Implicit((Texture)(object)pendingDraw.renderTexture), 0);
		cmd.ClearRenderTarget(true, true, new Color(0f, 0f, 0f, 0f));
		Bounds bounds = ((Renderer)pendingDraw.SpriteRenderer).bounds;
		Vector3 center = ((Bounds)(ref bounds)).center;
		center.z = ((Bounds)(ref bounds)).min.z - 1f;
		Matrix4x4 val = Matrix4x4.Scale(new Vector3(1f, 1f, -1f));
		Matrix4x4 val2 = Matrix4x4.TRS(center, Quaternion.identity, Vector3.one);
		Matrix4x4 val3 = val * ((Matrix4x4)(ref val2)).inverse;
		Matrix4x4 val4 = Matrix4x4.Ortho(0f - ((Bounds)(ref bounds)).extents.x, ((Bounds)(ref bounds)).extents.x, 0f - ((Bounds)(ref bounds)).extents.y, ((Bounds)(ref bounds)).extents.y, 0.1f, 10000f);
		cmd.SetViewProjectionMatrices(val3, GL.GetGPUProjectionMatrix(val4, false));
		cmd.DrawRenderer((Renderer)(object)pendingDraw.SpriteRenderer, ((Renderer)pendingDraw.SpriteRenderer).sharedMaterial, 0, 0);
	}
}
