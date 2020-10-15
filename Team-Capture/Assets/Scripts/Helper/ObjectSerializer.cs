using System.IO;
using Newtonsoft.Json;

namespace Helper
{
	/// <summary>
	/// Serialization and Deserialization using <see cref="JsonConvert"/>.
	/// <para>Saves to the config dir</para>
	/// </summary>
	public static class ObjectSerializer
	{
		#region Serialization

		/// <summary>
		/// This returns the Json form of the object
		/// </summary>
		public static string SerializeJson(object obj, bool compactJson = false,
			JsonSerializerSettings jsonSerializerSettings = null)
		{
			//If SerializerSettings.CompactJson is true, use no formatting, otherwise use indented
			return JsonConvert.SerializeObject(obj, compactJson ? Formatting.None : Formatting.Indented,
				jsonSerializerSettings);
		}

		/// <summary>
		/// This saves the Json of the object to a file in plain text format
		/// </summary>
		public static void SaveJson(object obj, string directory, string fileName = null,
			string extension = ".json", bool compactJson = false, JsonSerializerSettings jsonSerializerSettings = null)
		{
			//If the filename is not given
			fileName = fileName ?? obj.GetType().Name;

			string path = directory + fileName + extension;

			//Check if a directory exists
			if (Directory.Exists(directory)) Directory.CreateDirectory(directory);

			//Get the json for it
			string json = SerializeJson(obj, compactJson, jsonSerializerSettings);

			//Write our json to the file
			File.WriteAllText(path, json);
		}

		#endregion

		#region Deserialization

		//This version returns the object as a value
		public static T DeserializeJson<T>(string json, JsonSerializerSettings jsonSerializerSettings = null)
		{
			return JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings);
		}

		public static void DeserializeJsonOverwrite(string json, object @object,
			JsonSerializerSettings jsonSerializerSettings = null)
		{
			JsonConvert.PopulateObject(json, @object, jsonSerializerSettings);
		}

		public static T LoadJson<T>(string directory, string filename = null, string extension = ".json",
			JsonSerializerSettings jsonSerializerSettings = null)
		{
			//If filename not given
			filename = filename ?? typeof(T).Name;

			//Read all the Json from file
			string json = File.ReadAllText(directory + filename + extension);
			return DeserializeJson<T>(json, jsonSerializerSettings);
		}

		public static void LoadJsonOverwrite(object obj, string directory, string filename = null,
			string extension = ".json", JsonSerializerSettings jsonSerializerSettings = null)
		{
			//If filename not given
			//Make it the type name of the object
			filename = filename ?? obj.GetType().Name;

			//Read all the Json from file
			string json = File.ReadAllText(directory + filename + extension);
			DeserializeJsonOverwrite(json, obj, jsonSerializerSettings);
			//JsonConvert.PopulateObject(Json, Object, JsonSerializerSettings);
		}

		#endregion
	}
}