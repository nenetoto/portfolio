
using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace EVM.Editor
{
    public class CodeGenerator
    {
        private static Dictionary<string, Type> TypeCodes = new Dictionary<string, Type>()
        {
            { "BOOL",				typeof(bool)},
			{ "BOOLEAN",            typeof(bool)},
			{ "BYTE",				typeof(byte)},
            { "SHORT",				typeof(short)},
            { "FLOAT",				typeof(float)},
            { "INT",				typeof(int)},
            { "LONG",				typeof(long)},
            { "DOUBLE",				typeof(double)},
            { "STRING",				typeof(string)},
            { "USHORT",				typeof(ushort)},
            { "UINT",				typeof(uint)},
            { "ULONG",				typeof(ulong)},
            { "BIGINTEGER",			typeof(BigInteger)},
            { "LIST<BOOL>",			typeof(List<bool>)},
            { "LIST<BYTE>",			typeof(List<byte>)},
            { "LIST<SHORT>",		typeof(List<short>)},
            { "LIST<FLOAT>",		typeof(List<float>)},
            { "LIST<INT>",			typeof(List<int>)},
            { "LIST<LONG>",			typeof(List<long>)},
            { "LIST<DOUBLE>",		typeof(List<double>)},
            { "LIST<STRING>",		typeof(List<string>)},
            { "LIST<USHORT>",		typeof(List<ushort>)},
            { "LIST<UINT>",			typeof(List<uint>)},
            { "LIST<ULONG>",		typeof(List<ulong>)},
            { "LIST<BIGINTEGER>",	typeof(List<BigInteger>)},
			{ "DATE",               typeof(string)},
			{ "DATETIME",           typeof(DateTime)},
		};

        private string OutputPath { get; set; } = string.Empty;
        private string FileName { get; set; } = string.Empty;
        private CodeCompileUnit CompileUnit { get; set; } = new CodeCompileUnit();
        private CodeTypeDeclaration Class { get; set; } = new CodeTypeDeclaration();
        private CodeNamespace GlobalNamespace { get; set; } = new CodeNamespace();
        private CodeNamespace BlankNamespace { get; set; } = new CodeNamespace();

        public CodeGenerator(string outputPath)
        {
            OutputPath = outputPath;
        }

        public CodeGenerator SetName(string name)
        {
            FileName = name;
            Class.Name = name;
            Class.IsClass = true;
            Class.Attributes = MemberAttributes.Public;
            return this;
        }

        public CodeGenerator AddGlobalComment(string comment)
        {
            GlobalNamespace.Comments.Add(new CodeCommentStatement(comment));
            return this;
        }

        public CodeGenerator AddClassComment(string comment)
        {
            Class.Comments.Add(new CodeCommentStatement(comment));
            return this;
        }

        public CodeGenerator AddGlobalUsing(string usingString)
        {
            BlankNamespace.Imports.Add(new CodeNamespaceImport(usingString));
            return this;
        }

        public CodeGenerator AddNamespace(string namespaceString)
        {
            GlobalNamespace = new CodeNamespace(namespaceString);
            return this;
        }

        public CodeGenerator AddClassBaseType(string baseTypeName)
        {
            Class.BaseTypes.Add(baseTypeName);
            return this;
        }
    
        public CodeGenerator AddClassAttribute(string attributeString)
        {
            Class.CustomAttributes.Add(new CodeAttributeDeclaration(attributeString));
            return this;
        }

        public CodeGenerator AddMemberField(string typeString, string name, string[] customAttribute = null, string comments = "")
        {
            CodeMemberField field = new CodeMemberField();
            field.Attributes = MemberAttributes.Public;
            field.Name = name;

            // TypeCode 에 포함 되어 있지 않으면 class 또는 Enum Type으로 생성
            if (TypeCodes.ContainsKey(typeString.ToUpper()))
				field.Type = new CodeTypeReference(TypeCodes[typeString.ToUpper()]);
            else
                field.Type = new CodeTypeReference(typeString);

            if (customAttribute != null &&  customAttribute.Length > 0)
                foreach (var attribute in customAttribute)
                    field.CustomAttributes.Add(new CodeAttributeDeclaration(attribute));

            if (!string.IsNullOrEmpty(comments))
                field.Comments.Add(new CodeCommentStatement(comments));

            Class.Members.Add(field);
            return this;
        }

        public CodeGenerator AddPrivateMemberField(string typeString, string name, string[] customAttribute = null, string comments = "")
        {
            CodeMemberField field = new CodeMemberField();
            field.Attributes = MemberAttributes.Private;
            field.Name = name;

            // TypeCode 에 포함 되어 있지 않으면 class 또는 Enum Type으로 생성
            if (TypeCodes.ContainsKey(typeString.ToUpper()))
                field.Type = new CodeTypeReference(TypeCodes[typeString.ToUpper()]);
            else
                field.Type = new CodeTypeReference(typeString);

            if (customAttribute != null && customAttribute.Length > 0)
                foreach (var _attribute in customAttribute)
                    field.CustomAttributes.Add(new CodeAttributeDeclaration(_attribute));

            if (!string.IsNullOrEmpty(comments))
                field.Comments.Add(new CodeCommentStatement(comments));

            Class.Members.Add(field);

            return this;
        }

        public CodeGenerator AddMemberProperty(string typeString, string name, string[] customAttribute = null, string comments = "")
        {
            CodeMemberProperty IndexProperty = new CodeMemberProperty();
            IndexProperty.Name = EditorIOUtility.ToTitleCase(name);
            IndexProperty.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            IndexProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name)));

            if (TypeCodes.ContainsKey(typeString.ToUpper()))
                IndexProperty.Type = new CodeTypeReference(TypeCodes[typeString.ToUpper()]);
            else
                IndexProperty.Type = new CodeTypeReference(typeString);

            if (customAttribute != null && customAttribute.Length > 0)
                foreach (var attribute in customAttribute)
                    IndexProperty.CustomAttributes.Add(new CodeAttributeDeclaration(attribute));

            if (!string.IsNullOrEmpty(comments))
                IndexProperty.Comments.Add(new CodeCommentStatement(comments));

            Class.Members.Add(IndexProperty);

            return this;
        }

        public void Generate()
        {
            if (string.IsNullOrEmpty(OutputPath))
            {
                throw new Exception("output path is Empty");
            }

            if (string.IsNullOrEmpty(FileName))
                throw new Exception("file name is Empty");

            string fullPath = $"{Path.Combine(OutputPath, FileName)}.cs";

            GlobalNamespace.Types.Add(Class);
            CompileUnit.Namespaces.Add(BlankNamespace);
            CompileUnit.Namespaces.Add(GlobalNamespace);

            CodeGeneratorUtil.AddInterfaceMemberIndexPropertyCode(Class);
            CodeGeneratorUtil.AddToStringMemberMethodCode(Class);

            CSharpCodeProvider provider = new CSharpCodeProvider();
            using (StreamWriter sw = new StreamWriter(fullPath))
            {
                IndentedTextWriter tw = new IndentedTextWriter(sw, "    ");
                provider.GenerateCodeFromCompileUnit(
                    CompileUnit, tw, new CodeGeneratorOptions());
            }
        }
    }

    internal class CodeGeneratorUtil
    {
        public static void AddInterfaceMemberIndexPropertyCode(CodeTypeDeclaration @class) 
        { 
            CodeMemberProperty indexProperty = new CodeMemberProperty();
            indexProperty.Name = @"Index";
            indexProperty.Type = new CodeTypeReference(typeof(int));
            indexProperty.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            indexProperty.Comments.Add(new CodeCommentStatement("테이블의 index를 ITableData Interface의 Property를 재정의함."));
            indexProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "index")));
            @class.Members.Add(indexProperty);
        }

        public static void AddInterfaceMemberNamePropertyCode(CodeTypeDeclaration @class)
        {
            CodeMemberProperty nameProperty = new CodeMemberProperty();
            nameProperty.Name = @"Name";
            nameProperty.Type = new CodeTypeReference(typeof(string));
            nameProperty.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            nameProperty.Comments.Add(new CodeCommentStatement("ITableData Interface의 Property를 재정의함."));
            nameProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), @class.GetType().Name)));
            @class.Members.Add(nameProperty);
        }

        public static void AddToStringMemberMethodCode(CodeTypeDeclaration @class)
        {
            CodeMemberMethod toStringMethod = new CodeMemberMethod();
            toStringMethod.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            toStringMethod.Name = "ToString";
            toStringMethod.ReturnType = new CodeTypeReference(typeof(System.String));

            // StringBuilder 변수 선언
            CodeSnippetExpression codeSnippetDeclareStringBuilder = new CodeSnippetExpression(@"System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder()");
            toStringMethod.Statements.Add(codeSnippetDeclareStringBuilder);

            // object 이름 로그 추가
            CodeSnippetExpression codeSnippetAppendObjectName = new CodeSnippetExpression(@$"stringBuilder.AppendLine($""object Name= {@class.Name}"")");
            toStringMethod.Statements.Add(codeSnippetAppendObjectName);

            foreach (var member in @class.Members)
            {
                if (member is CodeMemberField)
                {
                    var _field = member as CodeMemberField;
                    // value 로그 추가
                    CodeSnippetExpression codeSnippetAppendValues = new CodeSnippetExpression(@$"stringBuilder.AppendLine($""{_field.Name}= {{{_field.Name}}}"")");
                    toStringMethod.Statements.Add(codeSnippetAppendValues);
                }
            }

            // return ToString
            CodeSnippetExpression codeSnippetReturn = new CodeSnippetExpression(@"return stringBuilder.ToString()");
            toStringMethod.Statements.Add(codeSnippetReturn);

            @class.Members.Add(toStringMethod);
        }
    }
}

