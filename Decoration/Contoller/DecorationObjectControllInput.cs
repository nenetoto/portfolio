using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationObjectControllInput : MonoBehaviour
{
	[SerializeField] DecorationObjectControllPad.State _inputState = DecorationObjectControllPad.State.None;

	public DecorationObjectControllPad.State _state { get { return _inputState; } }
}
