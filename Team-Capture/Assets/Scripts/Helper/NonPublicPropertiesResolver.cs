// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Team_Capture.Helper
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