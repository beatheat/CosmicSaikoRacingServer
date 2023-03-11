using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSRServer.Game;

public class CardJsonConverter : JsonConverter<Card>
{
	public override Card? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, Card card, JsonSerializerOptions options)
	{
		//{
		writer.WriteStartObject();
		
		//"id" : card.id,
		writer.WriteNumber(nameof(card.id), card.id);
		if (card.isExposure)
		{
			//"condition" : 
			writer.WritePropertyName(nameof(card.condition));
			// {CardCondition}
			JsonSerializer.Serialize(writer, card.condition, options);
		}
		if (card.variable.Count > 0)
		{
			//"variable": 
			writer.WritePropertyName(nameof(card.variable));
			//{
			writer.WriteStartObject();
			foreach (var v in card.variable)
			{
				//"var-name" : value
				writer.WriteNumber(v.Key, v.Value.value);
			}
			//}
			writer.WriteEndObject();
		}

		//}
		writer.WriteEndObject();
	}
}