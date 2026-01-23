using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class GraphicsUtils
{
	public static void ReduceGraphicsSettings()
	{
		Application.targetFrameRate = 30;
		QualitySettings.vSyncCount = 0;
		SetMSAAEnabled(enabled: false);
		GetPipelineAsset().supportsDynamicBatching = false;
		Log.Info("Setting low quality graphics", Array.Empty<object>());
		GameManager.GetAnalyticsManager().SendEvent("ReduceGraphicsSettings", new Dictionary<string, object>());
	}

	public static UniversalRenderPipelineAsset GetPipelineAsset()
	{
		RenderPipelineAsset renderPipelineAsset = GraphicsSettings.renderPipelineAsset;
		return (UniversalRenderPipelineAsset)(object)((renderPipelineAsset is UniversalRenderPipelineAsset) ? renderPipelineAsset : null);
	}

	public static void SetMSAAEnabled(bool enabled)
	{
		GetPipelineAsset().msaaSampleCount = ((!enabled) ? 1 : 2);
	}
}
