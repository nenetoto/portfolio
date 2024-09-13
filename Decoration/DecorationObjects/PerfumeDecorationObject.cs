using EVM;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PerfumeDecorationObject : BaseDecorationObject
{
	List<ParticleSystemRenderer> _particleSystemRenderers = new List<ParticleSystemRenderer>();

	//public override TranformBound _transformBound
	//{
	//	get
	//	{
	//		var cam = Camera.main;

	//		// 카메라의 절반 높이와 폭 계산
	//		float height = 2f * cam.orthographicSize;
	//		float width = height * cam.aspect;
	//		float scale = 0.25f * Mathf.Max(0.5f, _data._transform._scale);

	//		// 카메라의 월드 좌표계에서의 경계 계산
	//		float minX = transform.position.x - width * scale;
	//		float maxX = transform.position.x + width * scale;
	//		float minY = transform.position.y - height * scale;
	//		float maxY = transform.position.y + height * scale;

	//		__transformBound._min = new Vector2(minX, minY);
	//		__transformBound._max = new Vector2(maxX, maxY);

	//		return __transformBound;
	//	}
	//}

	public override void SetData(DecorationData data)
	{
		base.SetData(data);

		var particles = transform.GetComponentsInChildren<ParticleSystem>();

		foreach (var particle in particles)
		{
			ParticleSystemRenderer renderer = particle.GetComponent<ParticleSystemRenderer>();
			_particleSystemRenderers.Add(renderer);
		}
	}

	protected override void ApplyLayer()
	{
		base.ApplyLayer();

		foreach (var spriteRenderer in _particleSystemRenderers)
		{
			spriteRenderer.sortingLayerName = $"{_data._layer}";
		}
	}
}
