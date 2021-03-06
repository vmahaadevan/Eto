﻿#if PCL
using System;
using Newtonsoft.Json;
using System.Reflection;
using System.Collections.Generic;

namespace Eto.Serialization.Json
{
	public class TypeConverterConverter : JsonConverter
	{
		readonly Dictionary<Type, TypeConverter> converters = new Dictionary<Type, TypeConverter>();

		public override bool CanRead { get { return true; } }
		public override bool CanWrite { get { return false; } }

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
		}

		public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			TypeConverter converter;
			if (converters.TryGetValue(objectType, out converter))
			{
				return converter.ConvertFrom(reader.Value);
			}
			return existingValue;
		}

		public override bool CanConvert(Type objectType)
		{
			if (converters.ContainsKey(objectType))
				return true;
			var attr = objectType.GetTypeInfo().GetCustomAttribute<TypeConverterAttribute>();
			if (attr != null)
			{
				var converter = Activator.CreateInstance(Type.GetType(attr.TypeName)) as TypeConverter;
				if (converter != null && converter.CanConvertFrom(typeof(string)))
				{
					converters.Add(objectType, converter);
					return true;
				}
			}
			return false;
		}
	}
}
#endif