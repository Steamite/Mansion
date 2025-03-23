using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public interface ITab
{
	/// <summary>Clearing bindings.</summary>
	public void Clear(out int i, out AddressableAssetGroup g);

	/// <summary>Reloads Data entries.</summary>
	public void ReloadData();

	/// <summary>When changing elements in the list.</summary>
	public string LinkEntry();

	/// <summary>Switching tabs.</summary>
	public void SelectTab();

	/// <summary>Defines how to handle renaming elements.</summary>
	public void Rename();
}
