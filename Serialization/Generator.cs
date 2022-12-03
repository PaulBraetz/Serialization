using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhoMicro.CodeAnalysis;
using RhoMicro.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace RhoMicro.Serialization
{
	[Generator]
	public sealed class Generator : ISourceGenerator
	{
		public void Execute(GeneratorExecutionContext context)
		{
			if (!(context.SyntaxContextReceiver is SyntaxNodeVisitor visitor))
			{
				return;
			}

			visitor.Execute(context);
		}


		public void Initialize(GeneratorInitializationContext context)
		{
			context.RegisterForPostInitialization(c =>
			{
				c.AddSource(AttributeAnalysis.JsonContract.GeneratedType.Source);
				c.AddSource(AttributeAnalysis.XmlContract.GeneratedType.Source);
			});
			context.RegisterForSyntaxNotifications(() => new SyntaxNodeVisitor());
		}

		private sealed class SyntaxNodeVisitor : ISyntaxContextReceiver
		{
			private readonly IBaseTypeDeclarationReceiver[] _receivers = new IBaseTypeDeclarationReceiver[]
			{
				ContractReceiver.ForJson(),
				ContractReceiver.ForXml()
			};
			private readonly List<(BaseTypeDeclarationSyntax declaration, SemanticModel semanticModel, IBaseTypeDeclarationReceiver receiver)> _matches
				= new List<(BaseTypeDeclarationSyntax declaration, SemanticModel semanticModel, IBaseTypeDeclarationReceiver receiver)>();

			public void Execute(GeneratorExecutionContext context)
			{
				foreach (var (declaration, semanticModel, receiver) in _matches)
				{
					var source = receiver.Receive(declaration, semanticModel);
					context.AddSource(source);
				}
			}
			public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
			{
				if (context.Node is BaseTypeDeclarationSyntax typeDeclaration)
				{
					foreach (var receiver in _receivers)
					{
						if (receiver.CanReceive(typeDeclaration, context.SemanticModel))
						{
							_matches.Add((typeDeclaration, context.SemanticModel, receiver));
						}
					}
				}
			}
		}
	}
}
