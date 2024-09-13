using EVM;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseDecorationObject : MonoBehaviour
{
	public struct TranformBound
	{
		public Vector2 _min;
		public Vector2 _max;
		public Vector2 _center
		{
			get
			{
				return (_min + _max) * 0.5f;
			}
		}

		public Vector2 _size
		{
			get
			{
				return _max - _min;
			}
		}
		public TranformBound(Vector2 min, Vector2 max) { _min = min; _max = max; }

	}

	protected Renderer _renderer = null;

	public DecorationData _data { get; private set; } = null;

	protected List<Transform> _listChildTransform = new List<Transform>();

	protected TranformBound __transformBound = new TranformBound();

	public virtual TranformBound _transformBound
	{
		get
		{
			if (Vector3.zero.Equals(_renderer.bounds.min) && Vector3.zero.Equals(_renderer.bounds.max))
			{
				__transformBound._min = new Vector3(-1, -1, 0);
				__transformBound._max = new Vector3(1, 1, 0);
			}
			else
			{
				__transformBound._min = _renderer.bounds.min;
				__transformBound._max = _renderer.bounds.max;
			}

			return __transformBound;
		}
	}


	private void Awake()
	{
		OnInitialize();

		var skeletonAnimation = transform.GetComponentInChildren<SkeletonAnimation>();
		if (skeletonAnimation)
			_renderer = skeletonAnimation.GetComponentInChildren<MeshRenderer>();
		else
			_renderer = transform.GetComponentInChildren<SpriteRenderer>();
	}

	protected virtual void OnInitialize()
	{
		for (int i = 0; i < transform.childCount; ++i)
			_listChildTransform.Add(transform.GetChild(i));
	}

	protected virtual void OnDestroyObject()
	{
		for (int i = 0; i < _listChildTransform.Count; ++i)
			Destroy(_listChildTransform[i]);
		_listChildTransform.Clear();
		_listChildTransform = null;

		if (_renderer)
		{
			Destroy(_renderer);
			_renderer = null;
		}
	}

	public virtual void SetData(DecorationData data)
	{
		_data = data;
	}

	public void SetSprite(Sprite sprite)
	{
		var spriteRenderer = _renderer as SpriteRenderer;
		spriteRenderer.sprite = sprite;
	}

	public void SetPosition(Vector2 position)
	{
		_data._transform._positionX = position.x;
		_data._transform._positionY = position.y;

		ApplyPositon();
	}

	public void SetRotation(float rotation)
	{
		_data._transform._rotation = transform.eulerAngles.z + rotation;

		ApplyRotation();
	}

	public void SetSacle(float scale)
	{
		_data._transform._scale = scale;

		ApplyScale();
	}

	public void SetFlip()
	{
		_data._transform._isFlip = !_data._transform._isFlip;

		ApplyFilp();
	}

	public void SetLayer(int layer)
	{
		_data._layer = layer;

		ApplyLayer();
	}

	protected virtual void ApplyPositon()
	{
		this.transform.localPosition = new Vector2(_data._transform._positionX, _data._transform._positionY);
	}

	protected virtual void ApplyRotation()
	{
		var current = this.transform.eulerAngles;
		this.transform.eulerAngles = new Vector3(current.x, current.y, _data._transform._rotation);
	}

	protected virtual void ApplyScale()
	{
		this.transform.localScale = Vector2.one * _data._transform._scale;
	}

	protected virtual void ApplyFilp()
	{
		if (_data._transform._isFlip)
		{
			foreach (var child in _listChildTransform)
				child.localRotation = Quaternion.Euler(0, 180, 0);
		}
		else
		{
			foreach (var child in _listChildTransform)
				child.localRotation = Quaternion.Euler(0, 0, 0);
		}
	}

	protected virtual void ApplyLayer()
	{
		if (_renderer)
			_renderer.sortingLayerName = $"{_data._layer}";
	}

	public void ApplyTranform()
	{
		ApplyPositon();
		ApplyRotation();
		ApplyScale();
		ApplyFilp();
		ApplyLayer();
	}

	private void OnDestroy()
	{
		OnDestroyObject();

		_data = null;
	}
}
