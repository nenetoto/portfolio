using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EVM.Editor
{
	public class JsonGenerator
	{
		// #주의 BigInteger를 현재 json 라이브러리에서 Object 에 Add 할 수 없어서 double 형으로 대응. 
		private static Dictionary<string, Type> TypeCodes = new Dictionary<string, Type>()
		{
			{ "BOOL", typeof(bool)},
			{ "BOOLEAN",typeof(bool)},
			{ "BYTE", typeof(byte)},
			{ "SHORT", typeof(short)},
			{ "FLOAT", typeof(float)},
			{ "INT", typeof(int)},
			{ "LONG", typeof(long)},
			{ "DOUBLE", typeof(double)},
			{ "STRING", typeof(string)},
			{ "USHORT", typeof(ushort)},
			{ "UINT", typeof(uint)},
			{ "ULONG", typeof(ulong)},
			{ "BIGINTEGER", typeof(double)},
			{ "LIST<BOOL>", typeof(List<bool>)},
			{ "LIST<BYTE>", typeof(List<byte>)},
			{ "LIST<SHORT>", typeof(List<short>)},
			{ "LIST<FLOAT>", typeof(List<float>)},
			{ "LIST<INT>", typeof(List<int>)},
			{ "LIST<LONG>", typeof(List<long>)},
			{ "LIST<DOUBLE>", typeof(List<double>)},
			{ "LIST<STRING>", typeof(List<string>)},
			{ "LIST<USHORT>", typeof(List<ushort>)},
			{ "LIST<UINT>", typeof(List<uint>)},
			{ "LIST<ULONG>", typeof(List<ulong>)},
			{ "LIST<BIGINTEGER>", typeof(List<double>)},
			{ "DATE",typeof(string)},
			{ "DATETIME",typeof(DateTime)},
		};

		private string OutputPath { get; set; } = string.Empty;
		private string FileName { get; set; } = string.Empty;
		private JArray RootArray { get; set; } = new JArray();
		private JObject Object { get; set; } = new JObject();

		public JsonGenerator(string outputPath)
		{
			OutputPath = outputPath;
		}

		public JsonGenerator SetName(string name)
		{
			FileName = name;
			return this;
		}

		public JsonGenerator AppendRootArray()
		{
			RootArray.Add(new JObject(Object));
			Object.RemoveAll();

			return this;
		}

		public JsonGenerator AddObject(string type, string name, string value)
		{
			if (TypeCodes.ContainsKey(type.ToUpper()))
				Object.Add(name, JToken.FromObject(ChangeType(value, TypeCodes[type.ToUpper()])));
			else
				Object.Add(name, value);

			return this;
		}

		public JsonGenerator AddArray(string type, string name, string[] values)
		{
			JArray jArray = new JArray();

			if (TypeCodes.ContainsKey(type.ToUpper()))
			{
				var changedTypeValues = ChangeTypeList(values, TypeCodes[type.ToUpper()]);
				foreach (var value in changedTypeValues)
					jArray.Add(value);
			}
			else
			{
				foreach (var value in values)
					jArray.Add(value);
			}
			Object.Add(name, jArray);

			return this;
		}

		public void Generate()
		{
			if (!RootArray.HasValues && !Object.HasValues)
				throw new System.Exception("json generate failed. invalid object");

			string jsonString = string.Empty;

			// RootArray에 값이 있으면 RootArray로 생성
			if (RootArray.HasValues)
				jsonString = RootArray.ToString();
			// 아니면 Object 생성
			else
				jsonString = Object.ToString();

			UnityEngine.Debug.Log($"FileName: {FileName}\n" + jsonString);

			string fullPath = $"{Path.Combine(OutputPath, FileName)}.json";
			EditorIOUtility.WriteText(fullPath, jsonString);
		}

		private object ChangeType(string value, Type type)
		{
			object ret = null;
			try
			{
				ret = Convert.ChangeType(value, type);
			}
			catch (Exception e)
			{
				Debug.LogError($"value: {value}, type: {type}, exception: " + e.Message);
			}

			return ret;
		}

		private List<object> ChangeTypeList(string[] values, Type type)
		{
			List<object> objs = values.Cast<object>().ToList();
			Type containedType = type.GenericTypeArguments.First();
			return objs.Select(_item => Convert.ChangeType(_item, containedType)).ToList();
		}
	}
}
