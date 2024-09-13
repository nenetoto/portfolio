using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using EVM;

public class DecorationObjectControllPad : MonoBehaviour
	, IPointerDownHandler
	, IDragHandler
	, IPointerUpHandler
{
	[System.Flags]
	public enum State
	{
		None = 0,
		Move = 1 << 0,
		Scale = 1 << 1,
		Rotate = 1 << 2,
	}

	// - for Test
	bool _scaleTestFlag = false;
	bool _isEditor { get { return Application.isEditor; } }
	// for Test - 

	[SerializeField] CanvasGroup _canvasGroup;
	[Header("뷰 카메라")]
	[SerializeField] Camera _camera;
	[Header("패드 UI 캔버스")]
	[SerializeField] Canvas _canvas;
	[Header("회전으로 판단할 최소 각도 (도)")]
	[SerializeField] float _rotationThreshold = 0.1f;    // 회전으로 판단할 최소 각도 (도)
	[Header("회전 속도")]
	[SerializeField] float _rotationSpeed = 0.9f;        // 회전 속도
	[Header("스케일 업,다운 속도")]
	[SerializeField] float _scaleSpeed = 0.005f;           // 스케일 업,다운 속도

	[SerializeField] Button _buttonFlip;

	public BaseDecorationObject _selectedObject { get; private set; } = null;

	#region 회전 ------------ 
	Vector2 _startTouchPosition = Vector2.zero;
	Vector2 _previousTouchPosition = Vector2.zero;
	#endregion ---------- 회전 

	#region 크기 ------------ 
	const float _minScale = 0.5f;
	const float _maxScale = 2f;
	float _startDistance = 0;
	#endregion ---------- 크기 

	RectTransform _thisRectTransform = null;

	RectTransform _rectTransform
	{
		get
		{
			if (_thisRectTransform == null)
				_thisRectTransform = this.transform as RectTransform;

			return _thisRectTransform;
		}
	}

	public State _contollState { get; private set; } = State.None;

	private bool _isTouch = false;

	void Start()
	{
		_buttonFlip.onClick.AddListener(() => { FlipObject(); });

		DrawRectUI();

		SetShowRectUI(false);
	}

	void Update()
	{
		// ----------- for Test
		if (_isEditor && Input.GetKeyDown(KeyCode.LeftControl))
			_scaleTestFlag = true;

		if (_isEditor && Input.GetKeyUp(KeyCode.LeftControl))
			_scaleTestFlag = false;
		// ----------- for Test
	}

	private void OnDestroy()
	{
	}

	public void SelectObject(BaseDecorationObject selectedObject)
	{

		_selectedObject = selectedObject;

		DrawRectUI();

		SetShowRectUI(true);
	}

	public void DeselectObject()
	{
		_selectedObject = null;

		DrawRectUI();

		SetShowRectUI(false);
	}

	public void SetShowRectUI(bool _isShow)
	{
		if (_isShow)
		{
			_canvasGroup.alpha = 1;
		}
		else
		{
			_canvasGroup.alpha = 0;
		}
	}

	void DrawRectUI()
	{
		if (_selectedObject == null)
		{
			_rectTransform.anchoredPosition = Vector2.zero;
			_rectTransform.sizeDelta = Vector2.zero;
			_rectTransform.transform.rotation = Quaternion.identity;
		}
		else
		{
			/*
			* 각도가 0 이 아닌 상태에서 UI를 새로 그리면 min, max 계산할 때 가로 세로 비율이 달라지는 문제가 있어 
			* 0 으로 변경 후 min, max 를 구하고 다시 각도를 설정함.
			*/
			var originRotation = _selectedObject.transform.rotation;

			// 오브젝트 각도 0으로 고정
			_selectedObject.transform.rotation = Quaternion.identity;

			var center = _selectedObject._transformBound._center;
			var size = _selectedObject._transformBound._size;

			var canvasTransform = _canvas.transform as RectTransform;

			// 월드 좌표를 스크린 좌표로 변환
			Vector3 screenCenter = _camera.WorldToScreenPoint(center);
			Vector3 screenSize = _camera.WorldToScreenPoint(center + size) - _camera.WorldToScreenPoint(center);

			// 스크린 좌표를 UI 캔버스 좌표로 변환
			Vector2 uiPosition;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				canvasTransform,
				screenCenter,
				null,
				out uiPosition
			);

			Vector2 rectSize = new Vector2(screenSize.x / canvasTransform.lossyScale.x, screenSize.y / canvasTransform.lossyScale.y);

			// 크기와 위치 설정	
			_rectTransform.anchoredPosition = uiPosition;
			_rectTransform.sizeDelta = rectSize;

			// 오브젝트 각도 원복
			_selectedObject.transform.rotation = originRotation;
			_rectTransform.transform.rotation = originRotation;
		}
	}

	private void FlipObject()
	{
		if (_selectedObject == null)
			return;

		_selectedObject.SetFlip();

		DrawRectUI();
	}

	private void MoveObject(Vector2 deltaPos)
	{
		// 월드 좌표 스크린 좌표로 변환
		var objectScreenPosition = _camera.WorldToScreenPoint(_selectedObject.transform.position);

		// 드래그된 스크린 좌표
		var moveScreenPosition = (Vector2)objectScreenPosition + deltaPos;

		// 드래그된 스크린 좌표를 컨트롤 오브젝트를 그리고 있는 카메라의 좌표로 변환
		var destPosition = (Vector2)_camera.ScreenToWorldPoint(moveScreenPosition);

		_selectedObject.SetPosition(destPosition);
	}

	private void ScaleObject(Vector2 position1, Vector2 position2)
	{
		// 두 점 사이의 현재 거리 계산
		float currentDistance = Vector3.Distance(position1, position2);

		// 움직인 거리 계산
		var moveDistance = currentDistance - _startDistance;

		// 현재 오브젝트 스케일 
		var currentScale = _selectedObject.transform.localScale.x;

		// 움직인 거리만큼 현재 오브젝트 스케일 조절
		var newScale = currentScale + moveDistance * _scaleSpeed;

		var scale = Mathf.Clamp(newScale, _minScale, _maxScale);

		// 오브젝트의 스케일 업데이트
		_selectedObject.SetSacle(scale);

		_startDistance = currentDistance;
	}

	private void RotateObject(Vector2 position)
	{
		Vector2 previousVector = _previousTouchPosition - (Vector2)transform.position;
		Vector2 currentVector = position - (Vector2)transform.position;

		// 내적 계산
		float dotProduct = Vector2.Dot(previousVector.normalized, currentVector.normalized);

		float dotAngle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

		// 로테이션 판단
		if (dotAngle > _rotationThreshold)
		{
			// 각도 변화 계산
			float angleDelta = Vector2.SignedAngle(_previousTouchPosition - _startTouchPosition, position - _startTouchPosition);
			_selectedObject.SetRotation(angleDelta * _rotationSpeed);
		}

		_previousTouchPosition = position;
	}

	#region 이벤트 핸들러 --------------------------------------------

	public void OnPointerDown(PointerEventData eventData)
	{
		if (_selectedObject == null)
			return;

		_isTouch = true;

		if (_scaleTestFlag)
		{
			_contollState = State.Scale;
			_startTouchPosition = eventData.position;
			_previousTouchPosition = eventData.position;
		}
		else
		{
			var touchEnterObject = eventData.pointerEnter;

			if (touchEnterObject == null)
				return;

			var input = touchEnterObject.GetComponent<DecorationObjectControllInput>();

			if (input == null)
				return;

			if (input._state.HasFlag(State.Move))
			{
				_contollState = State.Move;
				_startTouchPosition = eventData.position;
				_previousTouchPosition = eventData.position;
				_startDistance = 0;
			}

			if (input._state.HasFlag(State.Scale))
			{
				if (Input.touchCount == 2)
				{
					_contollState = State.Scale;
					_startTouchPosition = eventData.position;
					_previousTouchPosition = eventData.position;

					if (_isEditor)
					{
						_startDistance = Vector3.Distance(Vector3.zero, eventData.position);
					}
					else
					{
						var input1 = Input.GetTouch(0);
						var input2 = Input.GetTouch(1);

						_startDistance = Vector3.Distance(input1.position, input2.position);
					}
				}
			}

			if (input._state.HasFlag(State.Rotate))
			{
				_contollState = State.Rotate;
				_startTouchPosition = eventData.position;
				_previousTouchPosition = eventData.position;
				_startDistance = 0;
			}
			else { }
		}

		DrawRectUI();
		SetShowRectUI(false);

		EventOrganizer.TriggerEvent(this, new Event_OnDownPoint());
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (!_isTouch)
			return;

		if (_selectedObject == null)
			return;

		if (_contollState == State.None)
			return;

		switch (_contollState)
		{
			case State.Move:
				{
					MoveObject(eventData.delta);
				}
				break;
			case State.Scale:
				{
					if (_isEditor)
					{
						ScaleObject(Vector2.zero, eventData.position);
					}
					else
					{
						if (Input.touchCount == 2)
						{
							var input1 = Input.GetTouch(0);
							var input2 = Input.GetTouch(1);

							if (input1.phase == TouchPhase.Moved && input2.phase == TouchPhase.Moved)
							{
								ScaleObject(input1.position, input2.position);
							}
						}
					}
				}
				break;

			case State.Rotate:
				{
					RotateObject(eventData.position);
				}
				break;
		}

		DrawRectUI();

		EventOrganizer.TriggerEvent(this, new Event_OnDragPoint());
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (_isTouch)
		{
			_isTouch = false;
			_contollState = State.None;
			_startTouchPosition = Vector2.zero;
			_previousTouchPosition = Vector2.zero;
			_startDistance = 0;

			DrawRectUI();
			SetShowRectUI(true);

			EventOrganizer.TriggerEvent(this, new Event_OnUpPoint());
		}
	}

	#endregion -------------------------------------------- 이벤트 핸들러


	#region 이벤트 --------------------------------------------

	public struct Event_OnDownPoint : IEventValue { }
	public struct Event_OnDragPoint : IEventValue { }
	public struct Event_OnUpPoint : IEventValue { }

	#endregion -------------------------------------------- 이벤트
}
