using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhoMicro.CodeAnalysis;
using RhoMicro.CodeAnalysis.Attributes;
using RhoMicro.Serialization.Attributes;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml.Linq;

namespace RhoMicro.Serialization
{
	internal static class ContractReceiver
	{
		private sealed class Receiver<TAttribute> : IBaseTypeDeclarationReceiver
			where TAttribute : Attribute, IDataContractAttribute
		{
			public Receiver(AttributeAnalysisUnit<TAttribute> analysisUnit, ITypeIdentifier serializerType, String serializationType)
			{
				_analysisUnit = analysisUnit ?? throw new ArgumentNullException(nameof(analysisUnit));
				_serializerType = serializerType ?? throw new ArgumentNullException(nameof(serializerType));
				_serializationType = serializationType ?? throw new ArgumentNullException(nameof(serializationType));
			}

			private readonly AttributeAnalysisUnit<TAttribute> _analysisUnit;
			private readonly ITypeIdentifier _serializerType;
			private readonly String _serializationType;

			public Boolean CanReceive(BaseTypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel)
			{
				var result = typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword) &&
						!typeDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword) &&
						typeDeclaration.HasAttributes(semanticModel, _analysisUnit.GeneratedType.Identifier) &&
						typeDeclaration.HasAttributes(semanticModel, TypeIdentifier.Create<DataContractAttribute>());

				return result;
			}

			public GeneratedSource Receive(BaseTypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel)
			{
				TAttribute attribute = typeDeclaration.AttributeLists
					.SelectMany(l => l.Attributes)
					.Select(a => (success: _analysisUnit.Factory.TryBuild(a, semanticModel, out TAttribute builtAttribute), builtAttribute))
					.Single(t => t.success).builtAttribute;

				String name = typeDeclaration.Identifier.Text;
				String serializerInitialization = GetSerializerInitialization(typeDeclaration, attribute);
				String instanceMethod = GetInstanceMethod(attribute);
				String source =
	$@"{instanceMethod}/// <summary>
/// Serializes an instance of <see cref=""{name}""/> into a stream.
/// </summary>
/// <param name=""target"">The stream to serialize into.</param>
/// <param name=""instance"">The instance to serialize.</param>
/// <exception cref=""{TypeIdentifier.Create<ArgumentNullException>()}"">Thrown when <paramref name=""instance""/> or <paramref name=""target""/> is <see langword=""null""/>.</exception>
public static void Write{_serializationType}({TypeIdentifier.Create<Stream>()} target, {name} instance)
{{
	if (target == null)
	{{
		throw new {TypeIdentifier.Create<ArgumentNullException>()}(nameof(target));
	}}
	if (instance == null)
	{{
		throw new {TypeIdentifier.Create<ArgumentNullException>()}(nameof(instance));
	}}

	{serializerInitialization}
	serializer.WriteObject(target, instance);
}}
/// <summary>
/// Deserializes an instance of <see cref=""{name}""/> from a stream.
/// </summary>
/// <param name=""source"">The stream to deserialize from.</param>
/// <returns>A deserialized instance of <see cref=""{name}""/> if deserialized successfully; otherwise, <see langword=""null""/>.</returns>
/// <exception cref=""{TypeIdentifier.Create<ArgumentNullException>()}"">Thrown when <paramref name=""source""/> is <see langword=""null""/>.</exception>
public static {name} Read{_serializationType}({TypeIdentifier.Create<Stream>()} source)
{{
	if (source == null)
	{{
		throw new {TypeIdentifier.Create<ArgumentNullException>()}(nameof(source));
	}}

	{serializerInitialization}
	var deserialized = serializer.ReadObject(source) as {name};

	return deserialized;
}}";

				SyntaxNode node = typeDeclaration;
				while (!(node is BaseNamespaceDeclarationSyntax) && node != null)
				{
					if (node is BaseTypeDeclarationSyntax typeNode)
					{
						source = EncloseInType(source, typeNode);
					}

					node = node.Parent;
				}

				if (node is BaseNamespaceDeclarationSyntax namespaceNode)
				{
					source = EncloseInNamespace(source, namespaceNode);
				}

				var identifier = TypeIdentifier.Create(semanticModel.GetDeclaredSymbol(typeDeclaration));
				var hintName = $"{identifier.ToNonGenericString().Replace('.', '_')}_{_serializationType}Serialization";
				var generatedSource = new GeneratedSource(source, hintName);

				return generatedSource;
			}

			private static String EncloseInType(String source, BaseTypeDeclarationSyntax typeDeclaration)
			{
				if (typeDeclaration == null)
				{
					return source;
				}

				String name = typeDeclaration.Identifier.Text;
				String modifiers = typeDeclaration.Modifiers.ToString();
				String declarationType = typeDeclaration.Kind() == SyntaxKind.ClassDeclaration ? "class" :
					typeDeclaration.IsKind(SyntaxKind.RecordDeclaration) ? "record" :
					typeDeclaration.IsKind(SyntaxKind.RecordStructDeclaration) ? "record struct" :
					"struct";

				String result =
	$@"{modifiers} {declarationType} {name}
{{
{source}
}}";

				return result;
			}
			private static String EncloseInNamespace(String source, BaseNamespaceDeclarationSyntax namespaceDeclaration)
			{
				if (namespaceDeclaration == null)
				{
					return source;
				}

				String name = namespaceDeclaration.Name.ToString();
				String result = name != null ?
	$@"namespace {name}
{{
{source}
}}" : source;

				return result;
			}
			private String GetInstanceMethod(TAttribute attribute)
			{
				var result = attribute.ImplementInstanceWriteMethod ?
$@"/// <summary>
/// Serializes this instance into a stream.
/// </summary>
/// <param name=""target"">The stream to serialize into.</param>
/// <exception cref=""{TypeIdentifier.Create<ArgumentNullException>()}"">Thrown when <paramref name=""target""/> is <see langword=""null""/>.</exception>
public void Write{_serializationType}({TypeIdentifier.Create<Stream>()} target)
{{
	if (target == null)
	{{
		throw new {TypeIdentifier.Create<ArgumentNullException>()}(nameof(target));
	}}

	Write{_serializationType}(target, this);
}}
" :
					String.Empty;

				return result;
			}
			private String GetSerializerInitialization(BaseTypeDeclarationSyntax typeDeclaration, TAttribute attribute)
			{
				String access = String.IsNullOrEmpty(attribute.SettingsMember) ?
					null :
					typeDeclaration.ChildNodes()
						.Select(c =>
						{
							(Boolean success, String access) r = (success: false, access: String.Empty);

							if (c is PropertyDeclarationSyntax property &&
								property.Modifiers.Any(SyntaxKind.StaticKeyword) &&
								property.Identifier.Text == attribute.SettingsMember)
							{
								r = (success: true, access: property.Identifier.Text);
							}
							else if (c is FieldDeclarationSyntax field &&
									field.Modifiers.Any(SyntaxKind.StaticKeyword) &&
									field.Declaration.Variables.First().Identifier.Text == attribute.SettingsMember)
							{
								r = (success: true, access: field.Declaration.Variables.First().Identifier.Text);
							}
							else if (c is MethodDeclarationSyntax method &&
									method.Modifiers.Any(SyntaxKind.StaticKeyword) &&
									method.Identifier.Text == attribute.SettingsMember &&
									method.ReturnType.ToString() == "void" &&
									!method.ParameterList.Parameters.Any())
							{
								r = (success: true, access: $"{method.Identifier.Text}()");
							}

							return r;
						})
						.FirstOrDefault(t => t.success).access;

				String defaultInitialization = $"new {_serializerType}(typeof({typeDeclaration.Identifier.Text}))";
				String initialization = access != null
					?
	$@"var settings = {access};
var serializer = settings != null ? new {_serializerType}(typeof({typeDeclaration.Identifier.Text}), settings) : {defaultInitialization};"

	: $"var serializer = {defaultInitialization};";

				return initialization;
			}
		}

		public static IBaseTypeDeclarationReceiver ForXml()
		{
			var result = new Receiver<XmlContractAttribute>(AttributeAnalysis.XmlContract, TypeIdentifier.Create<DataContractSerializer>(), "Xml");

			return result;
		}
		public static IBaseTypeDeclarationReceiver ForJson()
		{
			var result = new Receiver<JsonContractAttribute>(AttributeAnalysis.JsonContract, TypeIdentifier.Create<DataContractJsonSerializer>(), "Json");

			return result;
		}
	}
}
