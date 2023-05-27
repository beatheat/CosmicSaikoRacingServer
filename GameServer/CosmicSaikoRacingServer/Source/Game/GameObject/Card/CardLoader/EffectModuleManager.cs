

using CSR.Game.Phase;
using CSR.Game.Player;

namespace CSR.Game.GameObject;

using ParameterList =  CardEffectModule.ParameterList;
using Result = CardEffectModule.Result;
using Type = CardEffectModule.Type;

public delegate CardEffectModule.Result EffectModule(PreheatPhase preheatPhase, Card card , GamePlayer player, ParameterList parameters);

/// <summary>
/// 카드 이펙트 모듈 로딩 클래스
/// </summary>
internal static class EffectModuleManager
{
	private static readonly Dictionary<string, EffectModule> effectModules = new ();
	private static readonly Dictionary<string, Type> effectTypes = new ();
	private static bool _isLoaded = false;

	public static void Load()
	{
		if (_isLoaded) return;

		AddModule("Nothing", Nothing, Type.Nothing);
		AddModule("Add", Add, Type.Add);
		AddModule("Multiply", Multiply, Type.Multiply);
		AddModule("Draw", Draw, Type.Draw);
		AddModule("RerollCountUp", RerollCountUp, Type.RerollCountUp);
		AddModule("Death", Death, Type.Death);
		AddModule("Fail", Fail, Type.Fail);
		AddModule("Initialize", Initialize, Type.Initialize);
		AddModule("ForceReroll", ForceReroll, Type.ForceReroll);
		AddModule("CreateCardToHand", CreateCardToHand, Type.CreateCardToHand);
		AddModule("CreateCardToDeck", CreateCardToDeck, Type.CreateCardToDeck);
		AddModule("CreateCardToOther", CreateCardToOther, Type.CreateCardToOther);
		AddModule("BuffToMe", BuffToMe, Type.BuffToMe);
		AddModule("BuffToOther", BuffToOther, Type.BuffToOther);
		AddModule("DoPercent", DoPercent, Type.DoPercent);
		AddModule("SetVariable", SetVariable, Type.SetVariable);
		AddModule("Overload", Overload, Type.Overload);
		AddModule("EraseBuff", EraseBuff, Type.EraseBuff);
		AddModule("Combo", Combo, Type.Combo);
		AddModule("EnforceSelf", EnforceSelf, Type.EnforceSelf);
		AddModule("Discard", Discard, Type.Discard);
		AddModule("Choice", Choice, Type.Choice);
		AddModule("Check", Check, Type.Check);
		AddModule("Leak", Leak, Type.Leak);
		AddModule("Repeat", Repeat, Type.Repeat);
		AddModule("TurnEnd", TurnEnd, Type.TurnEnd);


		_isLoaded = true;
	}

	/// <summary>
	/// 초기화시 모듈메소드를 모듈이름과 매핑
	/// </summary>
	private static void AddModule(string moduleName, EffectModule effectModule, Type effectType)
	{
		effectModules.Add(moduleName, effectModule);
		effectTypes.Add(moduleName, effectType);
	}

	/// <summary>
	/// 모듈 이름을 통해 모듈을 가져온다
	/// </summary>
	public static bool TryGet(string moduleName, out EffectModule effectModule, out Type type)
	{
		if (effectModules.TryGetValue(moduleName, out effectModule!))
		{
			type = effectTypes[moduleName];
			return true;
		}

		type = default(Type);
		return false;
	}

	#region Effect Modules


	private static Result Nothing(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		
		return new Result {Value = null, Type = Type.Nothing};
	}

	private static Result Add(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		int amount = parameters.Get<int>(0, card, player);
		player.TurnDistance += amount;
		return new Result {Value = amount, Type = Type.Add};
		;
	}

	private static Result Multiply(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		double amount = parameters.Get<double>(0, card, player);
		player.TurnDistance = (int) Math.Round(player.TurnDistance * amount);
		return new Result {Value = amount, Type = Type.Multiply};
		;
	}

	private static Result Draw(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		int amount = parameters.Get<int>(0, card, player);

		return new Result {Value = player.DrawCard(amount), Type = Type.Draw};
	}

	private static Result RerollCountUp(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		int amount = parameters.Get<int>(0, card, player);
		player.AddRerollCount(amount);
		
		return new Result {Value = amount, Type = Type.RerollCountUp};
	}

	private static Result Death(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		card.Death = true;
		return new Result {Value = null, Type = Type.Death};
	}

	private static Result Fail(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		card.Enable = false;
		return new Result {Value = null, Type = Type.Fail};
	}

	private static Result Initialize(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		player.TurnDistance = 0;
		return new Result {Value = null, Type = Type.Initialize};
	}

	private static Result ForceReroll(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		for (int i = 0; i < player.Resource.ReelCount; i++)
		{
			player.Resource.Reel[i] = Util.GetRandomEnumValue<ResourceType>();
		}

		return new Result {Value = player.Resource.Reel, Type = Type.ForceReroll};
	}

	private static Result CreateCardToHand(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		int id = parameters.Get<int>(0, card, player);
		int amount = parameters.Get<int>(1, card, player);
		bool isDeath = parameters.Get<bool>(2, card, player);

		Card[] createdCards = new Card[amount];
		for (int i = 0; i < amount; i++)
		{
			createdCards[i] = CardManager.GetCard(id);
			if (isDeath)
				createdCards[i].Death = true;
			player.AddCardToHand(createdCards[i]);

		}

		return new Result {Value = createdCards, Type = Type.CreateCardToHand};
	}

	private static Result CreateCardToDeck(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		int id = parameters.Get<int>(0, card, player);
		int amount = parameters.Get<int>(1, card, player);
		bool isDeath = parameters.Get<bool>(3, card, player);

		Card[] createdCards = new Card[amount];
		for (int i = 0; i < amount; i++)
		{
			createdCards[i] = CardManager.GetCard(id);
			if (isDeath)
				createdCards[i].Death = true;
			player.AddCardToDeck(createdCards[i]);
		}

		return new Result {Value = createdCards, Type = Type.CreateCardToDeck};
	}

	private static Result CreateCardToOther(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		int id = parameters.Get<int>(0, card, player);
		int amount = parameters.Get<int>(1, card, player);
		int target = parameters.Get<int>(2, card, player);
		bool isDeath = parameters.Get<bool>(3, card, player);


		Card[] createdCards = new Card[amount];
		for (int i = 0; i < amount; i++)
		{
			createdCards[i] = CardManager.GetCard(id);
			if (isDeath)
				createdCards[i].Death = true;
		}

		List<int> targetPlayerIndex = new List<int>();
		//자신을 제외한 모든 플레이어
		if (target == 0)
		{
			foreach (var p in player.parent)
			{
				if (p != player)
					targetPlayerIndex.Add(p.Index);
			}
		}
		// 해당 등수
		else if (target is >= 1 and <= 4)
		{
			var p = player.parent.Find(x => x.Rank == target);
			if (p != player && p != null)
				targetPlayerIndex.Add(p!.Index);
		}
		//자신 바로 앞 플레이어
		else if (target == 5)
		{
			if (player.Rank > 1)
			{
				var p = player.parent.Find(x => x.Rank == player.Rank - 1);
				if (p != null)
					targetPlayerIndex.Add(p.Index);
			}
		}
		//자신 바로 뒤 플레이어
		else if (target == 6)
		{
			if (player.Rank < 4)
			{
				var p = player.parent.Find(x => x.Rank == player.Rank + 1);
				if (p != null)
					targetPlayerIndex.Add(p.Index);
			}
		}
		// target->distance 범위 안의 플레이어
		else if (target >= 7)
		{
			int distance = target;
			foreach (var p in player.parent)
			{
				if (p != player && Math.Abs(p.CurrentDistance - player.CurrentDistance) <= distance)
					targetPlayerIndex.Add(p.Index);
			}
		}

		var result = new Dictionary<string, object> {["srcIndex"] = player.Index, ["dstIndexList"] = targetPlayerIndex, ["createdCards"] = createdCards};

		Result CreateToOther()
		{
			foreach (int idx in targetPlayerIndex)
			{
				player.parent[idx].AddCardToDeck(createdCards);
			}

			return new Result {Value = result, Type = Type.CreateCardToOther};
		}

		if (targetPlayerIndex.Count == 0)
			return Nothing(preheatPhase, card, player, parameters);

		player.AddDepartEvent(CreateToOther);
		return new Result {Value = result, Type = Type.CreateCardToOther};
	}

	private static Result BuffToMe(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		int id = parameters.Get<int>(0, card, player);
		int amount = parameters.Get<int>(1, card, player);

		player.AddBuff((BuffType) id, amount);
		var result = new Dictionary<string, object> {["buffType"] = (BuffType) id, ["count"] = amount};

		return new Result {Value = result, Type = Type.BuffToMe};
	}

	private static Result BuffToOther(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		int id = parameters.Get<int>(0, card, player);
		int amount = parameters.Get<int>(1, card, player);
		int target = parameters.Get<int>(2, card, player);


		List<int> targetPlayerIndex = new List<int>();
		//자신을 제외한 모든 플레이어
		if (target == 0)
		{
			foreach (var p in player.parent)
			{
				if (p != player)
					targetPlayerIndex.Add(p.Index);
			}
		}
		// 해당 등수
		else if (target is >= 1 and <= 4)
		{
			var p = player.parent.Find(x => x.Rank == target);
			if (p != null && p != player)
				targetPlayerIndex.Add(p!.Index);
		}
		//자신 바로 앞 플레이어
		else if (target == 5)
		{
			if (player.Rank > 1)
			{
				var p = player.parent.Find(x => x.Rank == player.Rank - 1);
				if (p != null)
					targetPlayerIndex.Add(p.Index);
			}
		}
		//자신 바로 뒤 플레이어
		else if (target == 6)
		{
			if (player.Rank < 4)
			{
				var p = player.parent.Find(x => x.Rank == player.Rank + 1);
				if (p != null)
					targetPlayerIndex.Add(p.Index);
			}
		}
		// target->distance 범위 안의 플레이어
		else if (target >= 7)
		{
			int distance = target;
			foreach (var p in player.parent)
			{
				if (p != player && Math.Abs(p.CurrentDistance - player.CurrentDistance) <= distance)
					targetPlayerIndex.Add(p.Index);
			}
		}

		var result = new Dictionary<string, object> {["srcIndex"] = player.Index, ["dstIndexList"] = targetPlayerIndex, ["buffType"] = (BuffType) id, ["amount"] = amount};

		Result _BuffToOther()
		{
			foreach (int idx in targetPlayerIndex)
			{
				player.parent[idx].AddBuff((BuffType) id, amount);
			}

			return new Result {Value = result, Type = Type.BuffToOther};
		}

		if (targetPlayerIndex.Count == 0)
		{
			return Nothing(preheatPhase, card, player, parameters);
		}

		player.AddDepartEvent(_BuffToOther);
		return new Result {Value = result, Type = Type.BuffToOther};
	}

	private static Result EraseBuff(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		int id = parameters.Get<int>(0, card, player);
		CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();
		List<Result[]> results = new List<Result[]>();
		//모든 버프 제거
		if (id == 99)
		{
			int amount = player.ReleaseBuffAll();
			for (int i = 0; i < amount; i++)
				results.Add(effect.Use(preheatPhase, card, player));
		}
		//특정버프제거
		else
		{
			int amount = player.ReleaseBuff((BuffType) id);
			for (int i = 0; i < amount; i++)
				results.Add(effect.Use(preheatPhase, card, player));

		}

		return new Result {Value = results, Type = Type.EraseBuff};
	}

	private static Result EnforceSelf(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		// int id = parameters[0].Get<int>(player);
		// int amount = parameters[1].Get<int>(player);
		return new Result {Value = null, Type = Type.EnforceSelf};
	}

	private static Result DoPercent(PreheatPhase preheatPhase,Card card, GamePlayer player, ParameterList parameters)
	{
		string percent = parameters.Get<string>(0, card, player) ?? "";
		CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();
		Result[] result = Array.Empty<Result>();


		if (card.variables.ContainsKey(percent))
		{
			var _percent = card.variables[percent];
			Random random = new Random();
			if (random.Next(100) < _percent.value)
			{
				result = effect.Use(preheatPhase, card, player);
			}
		}

		return new Result {Value = result, Type = Type.DoPercent};
	}

	private static Result SetVariable(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		string varName = parameters.Get<string>(0, card, player) ?? "";
		int number = parameters.Get<int>(1, card, player);

		if (card.variables.ContainsKey(varName))
		{
			var variable = card.variables[varName];
			variable.value += number;
			if (variable.value < variable.lowerBound)
				variable.value = variable.lowerBound;
			if (variable.value > variable.upperBound)
				variable.value = variable.upperBound;
		}

		return new Result {Value = null, Type = Type.SetVariable};
	}

	private static Result Overload(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		string percent = parameters.Get<string>(0, card, player) ?? "";
		int amount = parameters.Get<int>(1, card, player);
		CardEffect effect = parameters.Get<CardEffect>(2, card, player) ?? CardEffect.Nothing();
		;

		Result[] result = Array.Empty<Result>();

		if (card.variables.ContainsKey(percent))
		{
			var overloadPercent = card.variables[percent];
			Random random = new Random();
			if (random.Next(100) < overloadPercent.value)
			{
				result = effect.Use(preheatPhase, card, player);
			}

			overloadPercent.value += amount;
			if (overloadPercent.value < overloadPercent.lowerBound)
				overloadPercent.value = overloadPercent.lowerBound;
			if (overloadPercent.value > overloadPercent.upperBound)
				overloadPercent.value = overloadPercent.upperBound;
		}

		return new Result {Value = result, Type = Type.DoPercent};
	}


	private static Result Combo(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		List<int> idList = parameters.Get<List<int>>(0, card, player) ?? new List<int> {0};
		CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();

		bool isComboReady = true;
		foreach (var id in idList)
		{
			Card? find = player.Card.TurnUsed.Find(card => card.Id == id);
			isComboReady = isComboReady && (find != null);
		}

		Result[] result = Array.Empty<Result>();
		if (isComboReady)
		{
			result = effect.Use(preheatPhase, card, player);
		}

		return new Result {Value = result, Type = Type.Combo};
	}

	private static Result Discard(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		int amount = parameters.Get<int>(0, card, player);
		CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();
		Random random = new Random();

		if (amount > player.Card.Hand.Count)
			amount = player.Card.Hand.Count;

		List<Result[]> leakResults = new List<Result[]>();
		List<Result[]> discardResults = new List<Result[]>();
		List<int> throwIndexList = new List<int>();
		//amount장의 카드를 버리고 leak효과+ discard특수효과를 발동한다.
		for (int i = 0; i < amount; i++)
		{
			int throwIndex = random.Next(player.Card.Hand.Count);
			throwIndexList.Add(throwIndex);
			Result[] throwResult = player.ThrowCard(throwIndex, preheatPhase);
			leakResults.Add(throwResult);
			Result[] discardResult = effect.Use(preheatPhase, card, player);
			discardResults.Add(discardResult);
		}

		var result = new Dictionary<string, object> {["throwIndexList"] = throwIndexList, ["leakResults"] = leakResults, ["discardResults"] = discardResults};

		return new Result {Value = result, Type = Type.Discard};
	}

	private static Result Choice(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		CardEffect effect = parameters.Get<CardEffect>(0, card, player) ?? CardEffect.Nothing();
		Random random = new Random();
		var result = effect.Use(random.Next(effect.Count),preheatPhase, card, player);

		return new Result {Value = result, Type = Type.Choice};
	}

	private static Result Check(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		bool condition = parameters.Get<bool>(0, card, player);
		CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();

		Result[] result = {Nothing(preheatPhase, card, player, parameters)};
		if (condition == true)
			result = effect.Use(preheatPhase, card, player);

		return new Result {Value = result, Type = Type.Check};
	}

	private static Result Leak(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		CardEffect effect = parameters.Get<CardEffect>(0, card, player) ?? CardEffect.Nothing();
		var result = effect.Use(preheatPhase, card, player);
		return new Result {Value = result, Type = Type.Leak};
	}

	private static Result Repeat(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		int amount = parameters.Get<int>(0, card, player);
		CardEffect effect = parameters.Get<CardEffect>(1, card, player) ?? CardEffect.Nothing();

		List<Result[]> results = new List<Result[]>();
		for (int i = 0; i < amount; i++)
			results.Add(effect.Use(preheatPhase, card, player));
		return new Result {Value = results, Type = Type.Repeat};
	}

	private static Result TurnEnd(PreheatPhase preheatPhase, Card card, GamePlayer player, ParameterList parameters)
	{
		card.Enable = false;
		preheatPhase.Ready(player);
		return new Result {Value = null, Type = Type.TurnEnd};
	}

	#endregion
}