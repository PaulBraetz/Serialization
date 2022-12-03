using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhoMicro.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace RhoMicro.Serialization
{
	internal interface IBaseTypeDeclarationReceiver
	{
		Boolean CanReceive(BaseTypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel);
		GeneratedSource Receive(BaseTypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel);
	}
}
