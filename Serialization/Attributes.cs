using RhoMicro.CodeAnalysis.Attributes;
using RhoMicro.Serialization.Attributes;
using System;

namespace RhoMicro.Serialization.Attributes
{
	internal interface IDataContractAttribute
	{
		String SettingsMember { get; set; }
		Boolean ImplementInstanceWriteMethod { get; set; }
	}
	/// <summary>
	/// Denotes the target type for code generation of json serialization members.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	internal sealed class JsonContractAttribute : Attribute, IDataContractAttribute
	{
		/// <summary>
		/// The name of the static member providing settings for serialization.
		/// </summary>
		public String SettingsMember { get; set; } = String.Empty;
		/// <summary>
		/// Defines wether or an instance method for writing to a serializer shall be generated.
		/// </summary>
		public Boolean ImplementInstanceWriteMethod { get; set; }
	}
	/// <summary>
	/// Denotes the target type for code generation of xml serialization members.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	internal sealed class XmlContractAttribute : Attribute, IDataContractAttribute
	{
		/// <summary>
		/// The name of the static member providing settings for serialization.
		/// </summary>
		public String SettingsMember { get; set; } = String.Empty;
		/// <summary>
		/// Defines wether or an instance method for writing to a serializer shall be generated.
		/// </summary>
		public Boolean ImplementInstanceWriteMethod { get; set; }
	}
}

namespace RhoMicro.Serialization
{
	internal class AttributeAnalysis
	{
		#region JsonContractAttribute
		private const String JsonContractAttributeSource =
@"using System;

namespace RhoMicro.Serialization.Attributes
{
	/// <summary>
	/// Denotes the target type for code generation of json serialization members.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	internal sealed class JsonContractAttribute : Attribute
	{
		/// <summary>
		/// The name of the static member providing settings for serialization.
		/// </summary>
		public String SettingsMember { get; set; } = String.Empty;
		/// <summary>
		/// Defines wether or an instance method for writing to a serializer shall be generated.
		/// </summary>
		public Boolean ImplementInstanceWriteMethod { get; set; }
	}
}";
		public static readonly AttributeAnalysisUnit<JsonContractAttribute> JsonContract
			= new AttributeAnalysisUnit<JsonContractAttribute>(JsonContractAttributeSource);
		#endregion
		#region XmlContractAttribute
		private const String XmlContractAttributeSource =
@"using System;

namespace RhoMicro.Serialization.Attributes
{
	/// <summary>
	/// Denotes the target type for code generation of xml serialization members.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	internal sealed class XmlContractAttribute : Attribute
	{
		/// <summary>
		/// The name of the static member providing settings for serialization.
		/// </summary>
		public String SettingsMember { get; set; } = String.Empty;
		/// <summary>
		/// Defines wether or an instance method for writing to a serializer shall be generated.
		/// </summary>
		public Boolean ImplementInstanceWriteMethod { get; set; }
	}
}";
		public static readonly AttributeAnalysisUnit<XmlContractAttribute> XmlContract
			= new AttributeAnalysisUnit<XmlContractAttribute>(XmlContractAttributeSource);
		#endregion
	}
}
