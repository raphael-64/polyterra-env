using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class RendererToTextureFeature : ScriptableRendererFeature
{
	public static RendererToTextureFeature Instance;

	private RendererToTexturePass pass;

	private List<RendererToTexture> pendingDraws = new List<RendererToTexture>();

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		pass.PassPendingDraws(pendingDraws);
		pendingDraws.Clear();
		renderer.EnqueuePass((ScriptableRenderPass)(object)pass);
	}

	public override void Create()
	{
		Instance = this;
		pass = new RendererToTexturePass();
	}

	public void Add(RendererToTexture pendingDraw)
	{
		pendingDraws.Add(pendingDraw);
	}
}
