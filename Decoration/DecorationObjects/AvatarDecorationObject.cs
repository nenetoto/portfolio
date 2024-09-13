using EVM;
using Shared.Packet.Models;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AvatarDecorationObject : BaseDecorationObject
{
	public CostumeAvatar _avatar { get; private set; } = null;

	Renderer[] _renderers = null;

	public override TranformBound _transformBound
	{
		get
		{
			if (_renderers.Length == 0)
				return __transformBound;

			var min = _renderers[0].bounds.min;
			var max = _renderers[0].bounds.max;

			foreach (var sprite in _renderers)
			{
				min = Vector3.Min(min, sprite.bounds.min);
				max = Vector3.Max(max, sprite.bounds.max);
			}

			__transformBound._min = min;
			__transformBound._max = max;

			return __transformBound;
		}
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		_avatar = transform.GetComponentInChildren<CostumeAvatar>();
	}

	public override void SetData(DecorationData data)
	{
		base.SetData(data);

		var avatarSlotData = data as DecorationAvatarSlotData;

		foreach (var partsData in avatarSlotData._listSimplePartsDatas)
			_avatar.AttachParts(new PartsData(partsData));

		_avatar.Apply();

		_renderers = _avatar.GetRenderers();
	}

	protected override void ApplyLayer()
	{
		base.ApplyLayer();

		foreach(var spriteRenderer in _renderers)
		{
			spriteRenderer.sortingLayerName = $"{_data._layer}";
		}
	}

	protected override void OnDestroyObject()
	{
		base.OnDestroyObject();

		for (int i = 0; i < _renderers.Length; ++i)
			Destroy(_renderers[i]);

		_renderer = null;

		if(_avatar != null)
		{
			Destroy(_avatar);
			_avatar = null;
		}
	}
}
