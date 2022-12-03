using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using RhoMicro.Serialization.Attributes;

namespace TestApp
{
	internal partial class Program
	{
		[DataContract]
		[JsonContract(ImplementInstanceWriteMethod = true)]
		[XmlContract(ImplementInstanceWriteMethod = true)]
		private partial record MyContract
		{
			[DataMember]
			readonly String _field = Guid.NewGuid().ToString();

			private static readonly DataContractJsonSerializerSettings JsonContractSettings = new DataContractJsonSerializerSettings()
			{
				EmitTypeInformation = EmitTypeInformation.Always
			};
			private static readonly DataContractSerializerSettings XmlContractSettings = new DataContractSerializerSettings()
			{

			};

			public override String ToString()
			{
				return $"{nameof(_field)} = {_field}";
			}
		}

		static void Main(String[] args)
		{
			var instance = new MyContract();
			Console.WriteLine(instance);

			using var stream = new MemoryStream();
			instance.WriteJson(stream);
			stream.Seek(0, SeekOrigin.Begin);
			using var reader = new StreamReader(stream);
			Console.WriteLine(reader.ReadToEnd());

			stream.Seek(0, SeekOrigin.Begin);
			instance = MyContract.ReadJson(stream);
			Console.WriteLine(instance);

			stream.Seek(0, SeekOrigin.Begin);
			instance.WriteXml(stream);
			stream.Seek(0, SeekOrigin.Begin);
			Console.WriteLine(reader.ReadToEnd());

			stream.Seek(0, SeekOrigin.Begin);
			instance = MyContract.ReadXml(stream);
			Console.WriteLine(instance);
		}
	}
}