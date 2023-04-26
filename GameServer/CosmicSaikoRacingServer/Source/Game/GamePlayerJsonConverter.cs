using System.Text.Json;
using System.Text.Json.Serialization;
using CSRServer.Game;

namespace CSRServer.Game;

public class GamePlayerJsonConverter : JsonConverter<GamePlayer>
{
	public override GamePlayer? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, GamePlayer player, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		
		writer.WriteNumber(nameof(player.index), player.index);
		writer.WriteString(nameof(player.nickname), player.nickname);
		writer.WriteNumber(nameof(player.remainDistance), player.remainDistance);
		writer.WriteNumber(nameof(player.currentDistance), player.currentDistance);
		writer.WriteNumber(nameof(player.turnDistance), player.turnDistance);
		writer.WriteNumber(nameof(player.rank), player.rank);
		
		writer.WritePropertyName(nameof(player.cardSystem.hand));
		JsonSerializer.Serialize(writer, player.cardSystem.hand, options);
		writer.WritePropertyName(nameof(player.cardSystem.deck));
		JsonSerializer.Serialize(writer, player.cardSystem.deck, options);
		writer.WritePropertyName(nameof(player.cardSystem.usedCard));
		JsonSerializer.Serialize(writer, player.cardSystem.usedCard, options);
		writer.WritePropertyName(nameof(player.cardSystem.unusedCard));
		JsonSerializer.Serialize(writer, player.cardSystem.unusedCard, options);
		writer.WritePropertyName(nameof(player.cardSystem.turnUsedCard));
		JsonSerializer.Serialize(writer, player.cardSystem.turnUsedCard, options);
		
		writer.WritePropertyName(nameof(player.buffSystem.buffList));
		JsonSerializer.Serialize(writer, player.buffSystem.buffList, options);
		
		writer.WritePropertyName("resourceReel");
		JsonSerializer.Serialize(writer, player.resourceSystem.reel, options);
		writer.WritePropertyName("resourceRerollCount");
		JsonSerializer.Serialize(writer, player.resourceSystem.rerollCount, options);		
		writer.WritePropertyName("resourceReelCount");
		JsonSerializer.Serialize(writer, player.resourceSystem.reelCount, options);

		writer.WritePropertyName(nameof(player.maintainSystem.level));
		JsonSerializer.Serialize(writer, player.maintainSystem.level, options);
		writer.WritePropertyName(nameof(player.maintainSystem.coin));
		JsonSerializer.Serialize(writer, player.maintainSystem.coin, options);
		writer.WritePropertyName(nameof(player.maintainSystem.exp));
		JsonSerializer.Serialize(writer, player.maintainSystem.exp, options);
		writer.WritePropertyName(nameof(player.maintainSystem.turnCoinCount));
		JsonSerializer.Serialize(writer, player.maintainSystem.turnCoinCount, options);
		
		writer.WriteEndObject();
	}
	
	
}