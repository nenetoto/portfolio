using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

namespace EVM.Editor
{
	public class ExcelTableInfo
	{
		public string TableName { get; private set; } = string.Empty;
		public List<string> CommentsString { get; private set; } = new List<string>();
		public List<string> FieldTypes { get; private set; } = new List<string>();
		public List<string> FieldNames { get; private set; } = new List<string>();
		public List<List<string>> ValuesList { get; private set; } = new List<List<string>>();
		public int RowCount { get; private set; } = 0;
		public int ColumnsCount { get; private set; } = 0;

		public ExcelTableInfo(string filePath, DataSet dataSet)
		{
			// ex. d:\level.xlsx -> levelTableData
			TableName = $"{filePath.Split(new char[] { '/', '\\' }).Last().Split(new char[] { '.' }).First()}TableData";

			if (dataSet == null)
				throw new Exception("data set is null");

			// 엑셀 테이블의 첫 번째 시트만 데이터로 사용
			DataTable table = dataSet.Tables[0];
			ColumnsCount = table.Columns.Count;
			RowCount = table.Rows.Count;

			try
			{
				for (int i = 0; i < ColumnsCount; ++i)
				{
					string commentsString	= table.Rows[0][i].ToString();
					string fieldType		= table.Rows[1][i].ToString().Trim();
					string fieldName		= table.Rows[2][i].ToString().Trim();

					// 첫 번째 행에 정의된 코멘트 저장
					CommentsString.Add(commentsString);

					// 두 번째 행에 정의된 타입 저장
					FieldTypes.Add(fieldType);

					// 세 번째 행에 정의된 필드 이름 저장
					FieldNames.Add(fieldName);
				}

				// 네 번째 행부터 정의된 테이블 값 저장
				int valueOffsetIndex = 3;
				for (int i = valueOffsetIndex; i < RowCount; ++i)
				{
					var values = new List<string>();

					for (int j = 0; j < ColumnsCount; ++j)
					{
						string type		= FieldTypes[j].Trim();
						string value	= table.Rows[i][j].ToString().Trim();

						values.Add(value);
					}

					ValuesList.Add(values);
				}
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
			}
			finally
			{
				Print();
			}
		}

		public void Print()
		{
			//for Log
			StringBuilder sb = new StringBuilder();

			sb.Append($" type : [ ");
			foreach (var type in FieldTypes)
				sb.Append($"{type}, ");
			sb.Append($" ]");
			sb.AppendLine();

			sb.Append($" name : [ ");
			foreach (var name in FieldNames)
				sb.Append($"{name}, ");
			sb.Append($" ]");
			sb.AppendLine();

			foreach (var values in ValuesList)
			{
				sb.Append($" value : [ ");
				foreach (var value in values)
					sb.Append($"{value}, ");
				sb.Append($" ]");
				sb.AppendLine();
			}

			UnityEngine.Debug.Log(sb.ToString());
		}
	}

	public class TableGenerator
	{
		private  List<ExcelTableInfo> ExcelTableInfoList { get; set; } = new List<ExcelTableInfo>();

		public  void SetExcelTableInfos(string folderPath)
		{
			var excelFiles = EditorIOUtility.GetAllFiles(folderPath, "*.xlsx");

			foreach (var filePath in excelFiles)
			{
				// ~$ 엑셀 쓰레기 파일을 거른다.
				if (Path.GetFileName(filePath).ToCharArray()[0].Equals('~'))
					continue;

				ExcelTableInfoList.Add(new ExcelTableInfo(filePath, EditorIOUtility.ReadExcel(filePath)));
			}
		}

		public  void ClearExcelTableInfos()
		{
			ExcelTableInfoList.Clear();
		}

		public  void GenerateCode(string outputPath)
		{
			if (ExcelTableInfoList.Count == 0)
			{
				UnityEngine.Debug.LogWarning($"'{outputPath}' not found excel table files.");
				return;
			}

			foreach (var excelInfo in ExcelTableInfoList)
			{
				Debug.LogColor($"____________ Generating Cs Table: {excelInfo.TableName}", Color.green);

				CodeGenerator codeGenerator = new CodeGenerator(outputPath);
				codeGenerator.SetName(excelInfo.TableName.Trim())
					.AddGlobalUsing("EVMData")
					.AddGlobalUsing("Newtonsoft.Json")
					.AddGlobalUsing("System")
					.AddClassBaseType("ITableData")
					.AddClassAttribute("System.Serializable")
					.AddNamespace("EVM")
					.AddClassComment("#주의 ITableData는 자동 생성되지 않습니다. 자동 생성된 코드를 사용 전 미리 정의가 필요합니다.");

				// 인덱스는 TableData Interface 에서 재정의 하기 때문에 접근 모호성을 방지하기 위해 private 로 생성. 
				string IndexFieldType = excelInfo.FieldTypes[0].Trim();
				string IndexFieldName = excelInfo.FieldNames[0].Trim();
				codeGenerator.AddPrivateMemberField(
					IndexFieldType,
					IndexFieldName,
					new string[] { "JsonProperty" });
					//"인덱스는 TableData Interface 에서 재정의 하기 때문에 접근 모호성을 방지하기 위해 private 로 생성. ");

				for (int i = 1; i < excelInfo.ColumnsCount; ++i)
				{
					string commentsString =  excelInfo.CommentsString[i];
					string fieldType = excelInfo.FieldTypes[i].Trim();
					string fieldName = excelInfo.FieldNames[i].Trim();

					if (string.IsNullOrEmpty(fieldType) || string.IsNullOrEmpty(fieldName))
						continue;

					// 맴버 변수 은닉화를 위해 private 변수, public Property Getter 추가 생성.
					codeGenerator.AddPrivateMemberField(fieldType, fieldName, new string[] { "JsonProperty" });
					codeGenerator.AddMemberProperty(fieldType, fieldName, comments:commentsString);
				}

				codeGenerator.Generate();
			}
		}

		public void GenerateJson(string outputPath)
		{
			foreach (var excelInfo in ExcelTableInfoList)
			{
				Debug.LogColor($"____________ Generating Json Table: {excelInfo.TableName}", Color.cyan);

				JsonGenerator jsonGenerator = new JsonGenerator(outputPath);
				jsonGenerator.SetName(excelInfo.TableName.Trim());

				for (int i = 0; i < excelInfo.ValuesList.Count; ++i)
				{
					for (int j = 0; j < excelInfo.ValuesList[i].Count; ++j)
					{
						var fieldType = excelInfo.FieldTypes[j].Trim();
						var filedName = excelInfo.FieldNames[j].Trim();
						var value = excelInfo.ValuesList[i][j].Trim();

						if (string.IsNullOrEmpty(fieldType) || string.IsNullOrEmpty(filedName))
							continue;

						// type string 에 list 문자가 포함되어 있으면 list 타입
						bool isList = fieldType.ToLower().Contains("list");
						if (isList)
						{
							// '#'으로 구분된 value string split 하여 string 배열 생성
							var valueList = value.Split(new char[] { '#' });
							jsonGenerator.AddArray(fieldType, filedName, valueList);
						}
						else
						{
							jsonGenerator.AddObject(fieldType, filedName, value);
						}
					}

					jsonGenerator.AppendRootArray();
				}
				jsonGenerator.Generate();
			}
		}
	}
}

