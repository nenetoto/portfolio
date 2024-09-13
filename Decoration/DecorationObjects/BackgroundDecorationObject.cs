using EVM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundDecorationObject : BaseDecorationObject
{
	public override void SetData(DecorationData data)
	{
		base.SetData(data);

		var backgroundPartsTableData = TableManager.GetTable<PartsTable>().Collection.GetByIndex(data._index);
		SetSprite(ResourceLoadUtil.GetCostumeBackground(backgroundPartsTableData.PartsId));
		SetSacle(Constants.DecorationBacogkroundDefaultScale);
	}

	protected override void ApplyLayer()
	{
		if (_renderer)
			_renderer.sortingLayerName = $"{0}";
	}
}
