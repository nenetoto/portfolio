using EVM;
using Shared.Packet.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DecorationData
{
	public enum Type
	{
		None,
		AvatarSlot,         // 아바타 슬롯
		Background,         // 배경
		Sticker,            // 스티커
		NPC,                // 인연 (npc)
		Perfume,            // 향수 (파티클)
	}

	public Type _type { get; set; } = Type.None;

	public int _index { get; set; } = 0;

	public int _layer;

	public DecorationTransform _transform { get; set; } = new DecorationTransform();

	public bool _isTransformable
	{
		get
		{
			return _type == Type.AvatarSlot
				|| _type == Type.Sticker
				//|| _type == Type.Perfume
				|| _type == Type.NPC;

		}
	}

	public bool _isLayable
	{
		get
		{
			return _type == Type.AvatarSlot
				|| _type == Type.Sticker
				|| _type == Type.NPC
				|| _type == Type.Perfume;
		}
	}

	public DecorationData() { }

	public DecorationData(DecorateObjectInfo info)
	{
		_index = (int)info.ObjectIndex;

		var decorationTableData = TableManager.GetTable<ItemDecorationTable>().Collection.GetByIndex(info.ObjectIndex);

		switch (decorationTableData.DecorationCategory)
		{
			case EVMData.DecorationCategory.None:
				break;
			case EVMData.DecorationCategory.Sticker:
				_type = Type.Sticker;
				break;
			case EVMData.DecorationCategory.DecoNpc:
				_type = Type.NPC;
				break;
			case EVMData.DecorationCategory.Perfume:
				_type = Type.Perfume;
				break;
			case EVMData.DecorationCategory.AvatarSlot:
				_type = Type.AvatarSlot;
				break;
		}

		_layer = info.Layer;
		_transform = new DecorationTransform(info);
	}
}

[System.Serializable]
public class DecorationTransform
{
	/// <summary>
	/// 레이아웃 오브젝트 위치 ( Unity3d.Vector 사용 시 json 파싱이 안되는 문제가 있음 )
	/// </summary>
	public float _positionX;
	public float _positionY;
	/// <summary>
	/// 레이아웃 오브젝트 로테이션
	/// </summary>
	public float _rotation;
	/// <summary>
	/// 레이아웃 오브젝트 크기
	/// </summary>
	public float _scale;
	/// <summary>
	/// 레이아웃 오브젝트 반전 여부
	/// </summary>
	public bool _isFlip;


	public DecorationTransform()
	{
		_positionX = 0;
		_positionY = 0;
		_rotation = 0f;
		_scale = 1f;
		_isFlip = false;
	}

	public DecorationTransform(DecorateBase decorateBase)
	{
		_positionX = decorateBase.Position_X;
		_positionY = decorateBase.Position_Y;
		_rotation = decorateBase.Rotation;
		_scale = decorateBase.Scale;
		_isFlip = decorateBase.Isflip;
	}
}

[System.Serializable]
public class DecorationAvatarSlotData : DecorationData
{
	/*
	 *  _index 는 꾸미기 씬에서 사용되는 아바타 슬롯(Item_DecorationTableData) 의 인덱스로
	 *  슬롯에 장착된 아바타를 표현하기 위해 사용되며 서버에서 받은 패킷으로 설정되는 _dataIndex는  
	 *  단순 보여주기 용도이기 때문에 DecorateInfo.DecorateAvatarInfos 리스트의 인덱트로 설정함.
	 */
	public bool _isMainSlot { get { return _index == 0; } }
	public bool _isValid { get { return _listSimplePartsDatas.Count > 0; } }

	public List<PartsBase> _listSimplePartsDatas = new List<PartsBase>();
}

public class DecorationPostingAvatarSlotData : DecorationAvatarSlotData
{
	public long _userSeq = -1;
	public List<PartsTierInfo> _listPartsTierInfos = new List<PartsTierInfo>();

	public DecorationPostingAvatarSlotData(DecorationAvatarSlotData decorationAvatarSlotData)
	{
		_type = decorationAvatarSlotData._type;
		_index = decorationAvatarSlotData._index;
		_layer = decorationAvatarSlotData._layer;
		_transform = decorationAvatarSlotData._transform;
		_listSimplePartsDatas = decorationAvatarSlotData._listSimplePartsDatas;
	}
}

[System.Serializable]
public class DecorateRoomData
{
	public List<DecorationAvatarSlotData> _listDecorationAvatarSlotData { get; set; } = new List<DecorationAvatarSlotData>();

	public List<DecorationData> _listDecorationObjectDatas { get; set; } = new List<DecorationData>();

	public List<DecorationData> _totlaDecorationData
	{
		get
		{
			List<DecorationData> ret = new List<DecorationData>();
			ret.AddRange(_listDecorationAvatarSlotData);
			ret.AddRange(_listDecorationObjectDatas);
			return ret;
		}
	}

	public bool _isValid
	{
		get
		{
			return _listDecorationAvatarSlotData.Count > 0 && _listDecorationObjectDatas.Count > 0;
		}
	}

	public DecorationAvatarSlotData GetMainAvatar()
	{
		var found = _listDecorationAvatarSlotData.Find(x => x._type == DecorationData.Type.AvatarSlot && x._index == 0);

		return found;
	}


	public void CreateMainAvatar()
	{
		_listDecorationAvatarSlotData.Add(new DecorationAvatarSlotData()
		{
			_index = 0,
			_type = DecorationData.Type.AvatarSlot,
			_listSimplePartsDatas = new List<PartsBase>()
		});
	}

	public DecorateRoomData() { }

	public DecorateRoomData(DecorateInfo decorateInfo)
	{
		//메인 아바타 설정
		var mainAvatar = decorateInfo.DecorateAvatarInfos.Find(x => x.IsMainAvatar);

		_listDecorationAvatarSlotData.Add(new DecorationAvatarSlotData()
		{
			_index = 0,
			_type = DecorationData.Type.AvatarSlot,
			_layer = mainAvatar.Layer,
			_transform = new DecorationTransform(mainAvatar),
			_listSimplePartsDatas = mainAvatar.Parts,
		});

		//메인 외 아바타 설정
		int otherAvatarIndex = 1;
		foreach (var avatarInfo in decorateInfo.DecorateAvatarInfos.FindAll(x => x.IsMainAvatar == false))
		{
			_listDecorationAvatarSlotData.Add(new DecorationAvatarSlotData()
			{
				_index = otherAvatarIndex,
				_type = DecorationData.Type.AvatarSlot,
				_layer = avatarInfo.Layer,
				_transform = new DecorationTransform(avatarInfo),
				_listSimplePartsDatas = avatarInfo.Parts,

			});

			++otherAvatarIndex;
		}

		// 아바타 외 꾸미기 오브젝트
		foreach (var objectInfo in decorateInfo.DecorateObjectInfos)
		{
			_listDecorationObjectDatas.Add(new DecorationData(objectInfo));
		}

		//  배경 
		_listDecorationObjectDatas.Add(new DecorationData()
		{
			_index = (int)decorateInfo.BackgroundIndex,
			_type = DecorationData.Type.Background,
			_layer = 0,
		});
	}

	// 서버 패킷 데이터
	public DecorateInfo ToDecorateInfo()
	{
		DecorateInfo decorateInfo = new DecorateInfo();

		foreach(var decorationAvatarData in _listDecorationAvatarSlotData)
		{
			decorateInfo.DecorateAvatarInfos.Add(new DecorateAvatarInfo()
			{
				IsMainAvatar = decorationAvatarData._isMainSlot,
				Parts = decorationAvatarData._listSimplePartsDatas,
				Layer = decorationAvatarData._layer,
				Position_X = decorationAvatarData._transform._positionX,
				Position_Y = decorationAvatarData._transform._positionY,
				Rotation = decorationAvatarData._transform._rotation,
				Scale = decorationAvatarData._transform._scale,
				Isflip = decorationAvatarData._transform._isFlip,
			});
		}

		foreach (var decorationData in _listDecorationObjectDatas)
		{
			if (decorationData._type == DecorationData.Type.Background)
			{
				decorateInfo.BackgroundIndex = decorationData._index;
			}
			else
			{
				decorateInfo.DecorateObjectInfos.Add(new DecorateObjectInfo()
				{
					ObjectIndex = decorationData._index,
					Layer = decorationData._layer,
					Position_X = decorationData._transform._positionX,
					Position_Y = decorationData._transform._positionY,
					Rotation = decorationData._transform._rotation,
					Scale = decorationData._transform._scale,
					Isflip = decorationData._transform._isFlip,
				});
			}
		}

		return decorateInfo;
	}


	public override string ToString()
	{
		System.Text.StringBuilder sb = new System.Text.StringBuilder();

		sb.AppendLine($"DecorateRoomData ------------------- / ");

		foreach (var avatarData in _listDecorationAvatarSlotData)
		{
			sb.AppendLine($"Decoration Avatar -- > index: {avatarData._index} , isMain: {avatarData._isMainSlot}");
		}

		foreach (var objectData in _listDecorationObjectDatas)
		{
			if (objectData._type == DecorationData.Type.Background)
				continue;

			var tableData = TableManager.GetTable<ItemDecorationTable>().Collection.GetByIndex(objectData._index);
			sb.AppendLine($"Decoration Object -- > index: {objectData._index}, type: {objectData._type}, name: {tableData.FileName}");
		}

		var bgObjectData = _listDecorationObjectDatas.Find(x => x._type == DecorationData.Type.Background);
		sb.AppendLine($"Decoration Background -- > index: {bgObjectData._index}");

		return sb.ToString();
	}
}

