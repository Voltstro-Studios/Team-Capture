using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Helper
{
	/// <summary>
	///     Resolver for Newtonsoft.Json
	/// </summary>
	public class NonPublicPropertiesResolver : DefaultContractResolver
	{
		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			JsonProperty prop = base.CreateProperty(member, memberSerialization);
			if (member is PropertyInfo pi)
			{
				prop.Readable = pi.GetMethod != null;
				prop.Writable = pi.SetMethod != null;
			}

			return prop;
		}
	}
}